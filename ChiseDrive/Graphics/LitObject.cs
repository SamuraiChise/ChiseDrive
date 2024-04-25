using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChiseDrive.Cameras;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ChiseDrive.Graphics.SkinnedModel;

namespace ChiseDrive.Graphics
{
    public struct Visibility
    {
        public static readonly Visibility Invisible     = new Visibility(0x0000000);
        public static readonly Visibility Standard      = new Visibility(0x0100011);
        public static readonly Visibility Opaque        = new Visibility(0x0000001);
        public static readonly Visibility Transparent   = new Visibility(0x0000010);
        public static readonly Visibility Game          = new Visibility(0x0000011);
        public static readonly Visibility Refract       = new Visibility(0x0000100);
        public static readonly Visibility Reflect       = new Visibility(0x0001000);
        public static readonly Visibility Shadow        = new Visibility(0x0010000);
        public static readonly Visibility Normal        = new Visibility(0x0100000);
        public static readonly Visibility Full          = new Visibility(0x1111111);

        Visibility(int values) { value = values; }
        public Visibility(Visibility visibility) { value = visibility.value; }

        int value;

        public override string ToString()
        {
            if (this == Standard) return "Standard";
            if (this == Opaque) return "Opaque";
            if (this == Transparent) return "Transparent";
            if (this == Game) return "Game";
            if (this == Refract) return "Refract";
            if (this == Reflect) return "Reflect";
            if (this == Shadow) return "Shadow";
            if (this == Normal) return "Normals";
            if (this == Full) return "Full";
            return "Unknown";
        }

        public static bool operator &(Visibility lhs, Visibility rhs)
        {
            if ((lhs.value & rhs.value) != 0) return true;
            return false;
        }

        public static bool operator ==(Visibility lhs, Visibility rhs)
        {
            if (lhs & rhs)//They share bits
            {
                if ((lhs.value ^ rhs.value) == 0)//And don't have exclusive bits
                    return true;
            }//They are not exactly equal
            return false;
        }
        public static bool operator !=(Visibility lhs, Visibility rhs)
        {
            return !(lhs == rhs);
        }
    }

    class SortFrontBack : IComparer<LitObject>
    {
        public static SortFrontBack Sorter = new SortFrontBack();

        public int Compare(LitObject lhs, LitObject rhs)
        {
            return lhs.DistanceToCamera < rhs.DistanceToCamera ? -1 : lhs.DistanceToCamera > rhs.DistanceToCamera ? 1 : 0;
        }
    }

    class SortBackFront : IComparer<LitObject>
    {
        public static SortBackFront Sorter = new SortBackFront();

        public int Compare(LitObject lhs, LitObject rhs)
        {
            return lhs.DistanceToCamera > rhs.DistanceToCamera ? -1 : lhs.DistanceToCamera < rhs.DistanceToCamera ? 1 : 0;
        }
    }

    /// <summary>
    /// An object that exists inside the world.  Includes Model, Material,
    /// List of LightSources, Effects, etc.
    /// </summary>
    public class LitObject : IBounding, IDisposable
    {
        #region GlobalLighting
        public static Color AmbientLight { set { ambientLight = value; ambientchanged = true; } }
        static bool ambientchanged = false;
        static Color ambientLight = new Color(125, 125, 125);
        #endregion

        #region Draw Lists
        const int MaxObjects = 400;
        static List<LitObject> drawlist = new List<LitObject>(MaxObjects);

        /// <summary>
        /// Draws all of the LitObjects that are in the draw list.
        /// </summary>
        /// <param name="device">A valid GraphicsDevice is required.</param>
        /// <param name="camera">A valid Camera is required.</param>
        /// <param name="pass">The Visibility to render.</param>
        public static void DrawAll(GraphicsDevice device, Camera camera, Visibility pass)
        {
            int count = 0;
            int polys = 0;

            if (drawlist.Count > 0)
            {
                drawlist[0].Effect.Parameters["ViewProjection"].SetValue(camera.View * camera.Projection);
                drawlist[0].Effect.Parameters["View"].SetValue(camera.View);
                drawlist[0].Effect.Parameters["CameraPosition"].SetValue(camera.Position);
            }

            foreach (LitObject obj in drawlist)
            {
                obj.UpdateEffect(); // Prepares effect data (lights, bones, etc)
            }

            device.RenderState.DepthBufferEnable = true;
            device.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
            device.RenderState.SourceBlend = Blend.SourceAlpha;
            device.RenderState.BlendFunction = BlendFunction.Add;
            device.RenderState.AlphaTestEnable = true;
            device.RenderState.DepthBufferWriteEnable = true;
            
            if (pass & Visibility.Normal)
            {
                // Render the opaque pass from the front to the back so 
                // we can use the depth buffer to keep our pixel shader
                // calls to ScreenWidth * ScreenHeight or so.
                ChiseDrive.Debug.Metrics.OpenMetric("LitSort");

                drawlist.Sort(SortFrontBack.Sorter);
                //drawlist.Sort(delegate(LitObject lhs, LitObject rhs) { return LitObject.FrontToBack(lhs, rhs); });

                ChiseDrive.Debug.Metrics.CloseMetric("LitSort");

                foreach (LitObject obj in drawlist)
                {
                    if (obj.Visibility & pass)
                    {
                        obj.Draw(device, camera, Visibility.Normal);
                        count++;
#if Debug
                        polys += obj.Mesh.PolygonCount;
#endif
                    }
                }
            }

            if (pass & Visibility.Opaque)
            {
                // Render the opaque pass from the front to the back so 
                // we can use the depth buffer to keep our pixel shader
                // calls to ScreenWidth * ScreenHeight or so.
                ChiseDrive.Debug.Metrics.OpenMetric("LitSort");

                drawlist.Sort(SortFrontBack.Sorter);
                //drawlist.Sort(delegate(LitObject lhs, LitObject rhs) { return LitObject.FrontToBack(lhs, rhs); });

                ChiseDrive.Debug.Metrics.CloseMetric("LitSort");

                device.RenderState.AlphaBlendEnable = false;    // Force Opaque (faster)
                device.RenderState.ReferenceAlpha = 254;        // Only render things with an alpha greater than this in the opaque pass

                foreach (LitObject obj in drawlist)
                {
                    if (obj.Visibility & pass)
                    {
                        obj.Draw(device, camera, Visibility.Opaque);
                        count++;
#if Debug
                        polys += obj.Mesh.PolygonCount;
#endif
                    }
                }

                device.RenderState.ReferenceAlpha = 0;          // Restore refrence alpha
            }
            
            if (pass & Visibility.Transparent)
            {
                // Render the transparent pass from back to front,
                // so objects in the front still can see partial transparency
                // from objects in the back.
                ChiseDrive.Debug.Metrics.OpenMetric("LitSort");

                drawlist.Sort(SortBackFront.Sorter);
                //drawlist.Sort(delegate(LitObject lhs, LitObject rhs) { return LitObject.BackToFront(lhs, rhs); });

                ChiseDrive.Debug.Metrics.CloseMetric("LitSort");

                device.RenderState.AlphaBlendEnable = true;     // Allow Blending (slower)

                foreach (LitObject obj in drawlist)
                {
                    if (obj.Visibility & pass)
                    {
                        obj.Draw(device, camera, Visibility.Transparent);
                        count++;
#if Debug
                        polys += obj.Mesh.PolygonCount;
#endif
                    }
                }
            }

#if Debug
            //ChiseDrive.Debug.DebugText.Write("Current Draw Pass [" + pass + "] Number Meshes [" + count + "] Number Polygons [" + polys + "]");
#endif
        }

        public static void UpdateAll(Time elapsed, Camera camera)
        {
            foreach (LitObject obj in drawlist)
            {
                if (obj.Mesh != null)
                {
                    obj.distanceToCamera = Vector3.Distance(obj.Mesh.RotationPosition.Translation, camera.Position);
                    obj.Mesh.Update(elapsed);
                }
            }
        }

        /// <summary>
        /// Setting this object to visible will add it to the draw list.
        /// The object will then automatically render depending on it's
        /// established Visibility setting.
        /// </summary>
        public bool Visible
        {
            get
            {
                return drawlist.Contains(this);
            }
            set
            {
                if (value && !drawlist.Contains(this))
                {
                    drawlist.Add(this);
                }
                else if (!value && drawlist.Contains(this))
                {
                    drawlist.Remove(this);
                }
            }
        }
        #endregion

        #region References
        const int MaxLocalLights = 5; // if you change this, change it in ShadedEffect.fx as well!
        const int MaxGlobalLights = 3;

        // References
        Game game;
        Effect effectInstance;
        EffectParameter lightParameter;

        Material material;

        PointLight[] localLights = new PointLight[MaxLocalLights];
        PointLight[] totalLights = new PointLight[MaxGlobalLights + MaxLocalLights];
        DrawableMesh mesh;
        #endregion
        
        #region Values
        // Values
        int localLightCount = 0;
        int totalLightCount = 0;
        bool lightsChanged = false;

        bool diffuseEnabled;
        bool specularEnabled;
        bool normalEnabled;
        bool emittanceEnabled;

        Visibility visibility;
        #endregion

        #region Parameters
        public DrawableMesh Mesh
        {
            get
            {
                return mesh;
            }
            set
            {
                mesh = value;
            }
        }
        public Material Material
        {
            get
            {
                return material;
            }
            set
            {
                material = value;
                material.SetEffectValues(effectInstance);
            }
        }
        public bool DiffuseEnabled
        {
            get
            {
                return diffuseEnabled;
            }
            set
            {
                diffuseEnabled = value;
                effectInstance.Parameters["DiffuseTextureEnabled"].SetValue(diffuseEnabled);
            }
        }
        public bool NormalEnabled
        {
            get
            {
                return normalEnabled;
            }
            set
            {
                normalEnabled = value;
                effectInstance.Parameters["NormalTextureEnabled"].SetValue(normalEnabled);
            }
        }
        public bool SpecularEnabled
        {
            get
            {
                return specularEnabled;
            }
            set
            {
                specularEnabled = value;
                effectInstance.Parameters["SpecularTextureEnabled"].SetValue(specularEnabled);
            }
        }
        public bool EmittanceEnabled
        {
            get
            {
                return emittanceEnabled;
            }
            set
            {
                emittanceEnabled = value;
                effectInstance.Parameters["EmittanceTextureEnabled"].SetValue(emittanceEnabled);
            }
        }
        public AnimationPlayer AnimationPlayer
        {
            get
            {
                return mesh.AnimationPlayer;
            }
        }
        public Matrix RotationPosition
        {
            get
            {
                return mesh.RotationPosition;
            }
            set
            {
                mesh.RotationPosition = value;
            }
        }
        public Effect Effect
        {
            get
            {
                return effectInstance;
            }
        }
        public EffectParameter LightsParameter
        {
            get
            {
                return lightParameter;
            }
        }
        public Visibility Visibility
        {
            get
            {
                return visibility;
            }
            set
            {
                visibility = value;
            }
        }
        static int FrontToBack(LitObject lhs, LitObject rhs) { return lhs.distanceToCamera < rhs.distanceToCamera ? -1 : lhs.distanceToCamera > rhs.distanceToCamera ? 1 : 0; }
        static int BackToFront(LitObject lhs, LitObject rhs) { return lhs.distanceToCamera > rhs.distanceToCamera ? -1 : lhs.distanceToCamera < rhs.distanceToCamera ? 1 : 0; }

        float distanceToCamera;
        public float DistanceToCamera { get { return distanceToCamera; } }
        #endregion 

        #region IBounding
        public Nullable<Vector4> Intersects(Ray test, float length)
        {
            return mesh.Intersects(test, length);
        }
        public Nullable<Vector4> Intersects(BoundingBox test)
        {
            return mesh.Intersects(test);
        }
        public Nullable<Vector4> Intersects(BoundingSphere test)
        {
            return mesh.Intersects(test);
        }
        public Nullable<Vector4> Intersects(IBounding test)
        {
            return mesh.Intersects(test);
        }
        public BoundingBox BoundingBox
        {
            get { return mesh.BoundingBox; }
        }
        public BoundingSphere BoundingSphere
        {
            get { return mesh.BoundingSphere; }
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            Visible = false;
            material = null;
            mesh = null;
            
            //effectInstance.Dispose();
        }
        #endregion

        void Initialize(Game game, DrawableMesh mesh, Material material, Effect effect)
        {
            if (game == null) throw new ArgumentNullException("Game");
            if (game.GraphicsDevice == null) throw new ArgumentNullException("GraphicsDevice");
            if (game.Content == null) throw new ArgumentNullException("ContentManager");
            if (effect == null) throw new ArgumentNullException("Effect");
            if (mesh == null) throw new ArgumentNullException("Mesh");
            if (material == null) throw new ArgumentNullException("Material");

            this.game = game;
            this.mesh = mesh;

            this.effectInstance = effect.Clone(game.GraphicsDevice);
            this.mesh.Effect = effectInstance;

            this.effectInstance.CurrentTechnique = effectInstance.Techniques[0];
            this.lightParameter = effectInstance.Parameters["Lights"];

            this.material = material;
            material.SetEffectValues(effectInstance);

            // Set Default Values
            effectInstance.Parameters["AmbientLightColor"].SetValue(ambientLight.ToVector4());

            this.Visibility = Visibility.Standard;

#if Debug
            System.Diagnostics.Trace.WriteLine("New LitObject initialized [" + mesh.PolygonCount + "]");
#endif

            CompactAndCountLights();
        }
        public LitObject(Game game, DrawableMesh mesh, Material material, Effect effect)
        {
            Initialize(game, mesh, material, effect);
        }
        public LitObject(Game game, Effect effect)
        {
            this.game = game;
            this.effectInstance = effect.Clone(game.GraphicsDevice);
        }
        public LitObject(LitObject copy)
        {
            game = copy.game;
            mesh = copy.mesh.Clone();

            effectInstance = copy.effectInstance.Clone(game.GraphicsDevice);
            this.mesh.Effect = effectInstance;

            this.effectInstance.CurrentTechnique = effectInstance.Techniques[0];
            lightParameter = effectInstance.Parameters["Lights"];

            material = new Material(copy.material);
            material.SetEffectValues(effectInstance);

            effectInstance.Parameters["AmbientLightColor"].SetValue(copy.effectInstance.Parameters["AmbientLightColor"].GetValueVector4());

            this.Visibility = copy.Visibility;

            CompactAndCountLights();
        }
        public LitObject(Game game, Units.UnitSettings.GameModelSettings settings)
        {
            GameModel mesh = new GameModel(
                game.Content.Load<Model>(settings.MeshName), settings.Scale);
            Material material = new Material(game.Content,
                settings.Material);
            Effect effect = game.Content.Load<Effect>(settings.Effect);
            Initialize(game, mesh, material, effect);
        }
        public LitObject Clone()
        {
            LitObject retvalue = new LitObject(this);
            retvalue.DiffuseEnabled = this.DiffuseEnabled;
            retvalue.NormalEnabled = this.NormalEnabled;
            retvalue.SpecularEnabled = this.SpecularEnabled;
            retvalue.EmittanceEnabled = this.EmittanceEnabled;
            return retvalue;
        }

        /// <summary>
        /// Adds a light to the list of material lights, and caches data on the GFX card.
        /// </summary>
        /// <param name="light"></param>
        public void AddLight(PointLight light)
        {
            if (localLights.Contains<PointLight>(light)) return;

            for (int i = 0; i < localLights.Length; i++)
            {
                if (localLights[i] == null)
                {
                    localLights[i] = light;
                    break; // for loop
                }
            }

            CompactAndCountLights();
        }

        /// <summary>
        /// Removes a light from the list of material lights, and removes cached data from the GFX card.
        /// </summary>
        /// <param name="light"></param>
        public void RemoveLight(PointLight light)
        {
            if (!localLights.Contains<PointLight>(light)) return;

            for (int i = 0; i < localLights.Length; i++)
            {
                if (localLights[i] == light)
                {
                    localLights[i] = null;
                    break; // for loop
                }
            }

            CompactAndCountLights();
        }

        public void ClearLights()
        {
            for (int i = 0; i < localLights.Length; i++)
            {
                localLights[i] = null;
            }
        }

        public void CompactAndCountLights()
        {
            lightsChanged = true;
            localLightCount = 0;

            // First, compact all the lights
            for (int i = 0; i < localLights.Length; i++)
            {
                if (localLights[i] == null)
                {
                    for (int j = i; j < localLights.Length; j++)
                    {
                        if (localLights[j] != null)
                        {
                            localLights[i] = localLights[j];
                            localLights[j] = null;
                            break; // j loop
                        }
                    }
                }

                // Count the lights (will count a recently compacted one)
                if (localLights[i] != null)
                {
                    localLightCount++;
                }
            }

            int l = 0;
            for (l = 0; l < PointLight.GlobalLights.Count && l < MaxGlobalLights; l++)
            {
                totalLights[l] = PointLight.GlobalLights[l];
            }
            for (int i = 0; i < localLightCount && i < MaxLocalLights; i++, l++)
            {
                totalLights[l] = localLights[i];
            }
            totalLightCount = l;
        }

        public bool ForceLightUpdate { get; set; }

        float FadeDestination = 1f;
        float FadeCurrent = 1f;
        float FadeSpeed = 1f;
        public static readonly Time FadeTime = Time.FromFrames(90f);
        public void DoFade(float destination, Time fadeTime)
        {
            if (FadeDestination > 1f || FadeDestination < 0f) throw new Exception("Fade Destination between 0 and 1 expected.");
            FadeDestination = destination;
            FadeSpeed = 1f / FadeTime;
        }
        public bool IsFading
        {
            get
            {
                return FadeCurrent != FadeDestination;
            }
        }
        public float Fade
        {
            get
            {
                return FadeCurrent;
            }
        }

        public void UpdateEffect()
        {
            if (mesh.SkinningBones != null)
            {
                ChiseDrive.Debug.Metrics.OpenMetric("EffectInstance");
                effectInstance.Parameters["Bones"].SetValue(mesh.SkinningBones);
                ChiseDrive.Debug.Metrics.CloseMetric("EffectInstance");
            }

            if (ForceLightUpdate || PointLight.GlobalLightsChanged || (totalLightCount != PointLight.GlobalLights.Count + localLightCount))
            {
                ForceLightUpdate = false;
                CompactAndCountLights();
            }
            if (lightsChanged)
            {
                for (int i = 0; i < totalLightCount; i++)
                {
                    totalLights[i].UpdateAllParameters(lightParameter.Elements[i]);
                }
                lightsChanged = false;
            }
            else
            {
                for (int light = 0; light < totalLightCount; light++)
                {
                    totalLights[light].UpdateChangedParameters(lightParameter.Elements[light]);
                }
            }

            if (ambientchanged)
            {
                effectInstance.Parameters["AmbientLightColor"].SetValue(ambientLight.ToVector4());
            }

            effectInstance.Parameters["NumLightsPerPass"].SetValue(totalLightCount);

            effectInstance.CommitChanges();

            if (IsFading)
            {
                FadeCurrent = Helper.EaseTo(FadeCurrent, FadeDestination, FadeSpeed, 1f);
                effectInstance.Parameters["AlphaFade"].SetValue(FadeCurrent);
            }

            #region Debug
#if Debug
            numberlightscounted += localLightCount;
            numberpassescounted++;
#endif
            #endregion
        }

        public virtual void Draw(GraphicsDevice device, Camera camera, Visibility pass)
        {
            string name = pass == Visibility.Normal ? "NormalDepth" : "Lit";

            EffectTechnique technique = null;
            for (int i = 0; i < effectInstance.Techniques.Count; i++)
                if (effectInstance.Techniques[i].Name == name) technique = effectInstance.Techniques[i];

            if (technique != null)
            {
                effectInstance.CurrentTechnique = technique;

                effectInstance.Begin();

                effectInstance.CurrentTechnique.Passes["Full"].Begin();

                mesh.Draw(device, camera);

                effectInstance.CurrentTechnique.Passes["Full"].End();
                effectInstance.End();
            }
        }

        public List<String> GetBoneNames()
        {
            return mesh.GetBoneNames();
        }
        public Matrix GetBoneMatrix(String name)
        {
            return mesh.BoneTransform(name);
        }

        #region Debug
#if Debug
        static int numberlightscounted = 0;
        static int numberpassescounted = 0;
        public static int AverageLightsPerRender
        {
            get
            {
                return numberpassescounted > 0 ? numberlightscounted / numberpassescounted : 0;
            }
        }
#endif
        #endregion
    }
}