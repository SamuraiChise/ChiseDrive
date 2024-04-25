using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ChiseDrive;
using ChiseDrive.Cameras;
using ChiseDrive.Graphics;

namespace ChiseDrive.Units
{
    public struct BladePoints
    {
        public Vector3 Point1;
        public Vector3 Point2;
        public static BladePoints Empty = new BladePoints();
    };

    /// <summary>
    /// The edge of a blade used for slashing through enemies.
    /// </summary>
    public struct BladeEdge
    {
        /// <summary>
        /// An internal set of points used for creating
        /// bounding boxes, pre-allocated(shared) to avoid new 
        /// allocations inside a struct constructor.
        /// </summary>
        static List<Vector3> points = new List<Vector3>(4);

        /// <summary>
        /// The direction the blade is traveling in.
        /// </summary>
        public Vector3 Direction
        {
            get
            {
                if (First.Point1 == Second.Point1) return Vector3.Up;
                return Second.Point1 - First.Point1;
            }
        }

        /// <summary>
        /// Creates a new BladeEdge.  Because it's a value type,
        /// this comes from the stack, not the heap, so does not
        /// generate garbage.
        /// </summary>
        /// <param name="start">The beginning of the sweep.</param>
        /// <param name="finish">The end of the sweep.</param>
        public BladeEdge(BladePoints start, BladePoints finish)
        {
            First = start;
            Second = finish;

            points.Clear();
            points.Add(First.Point1);
            points.Add(First.Point2);
            points.Add(Second.Point1);
            points.Add(Second.Point2);
            area = BoundingBox.CreateFromPoints(points);

            Center = First.Point1
                    + First.Point2
                    + Second.Point1
                    + Second.Point2;
            Center *= 0.25f; // Faster than /4

            Base = First.Point2
                    + Second.Point2;
            Base *= 0.5f;
        }

        public BladePoints First;
        public BladePoints Second;
        public Vector3 Center;
        public Vector3 Base;

        BoundingBox area;

        /// <summary>
        /// A general area that the blade sweeps through
        /// for rough approximations of hits.
        /// </summary>
        public BoundingBox Area
        {
            get
            {
                return area;
            }
        }

        /// <summary>
        /// A blade's plane that it's slicing through.
        /// </summary>
        public Plane Plane
        {
            get
            {
                return new Plane(First.Point1, First.Point2, Second.Point1);
            }
        }
    };

    public class BladeWeapon : PhysicalAddon, IDrawableEffect3D
    {
        static Effect effect;
        static VertexDeclaration declaration;
        static Texture2D texture;
        static ChiseDriveGame Game;
        static public void Initialize(ChiseDriveGame game, Effect effect, Texture2D texture)
        {
            BladeWeapon.Game = game;
            BladeWeapon.effect = effect.Clone(game.GraphicsDevice);
            BladeWeapon.texture = texture;
            BladeWeapon.declaration = new VertexDeclaration(game.GraphicsDevice, VertexPositionColorTexture.VertexElements);

            effect.Parameters["TextureMap"].SetValue(texture);
            effect.CommitChanges();
        }

        public bool Active { get; set; }

        const int MaxTrail = 8;
        const int VerticesPerSide = MaxTrail * 2;
        int activeTail = 0;
        int Step = 0;

        BladePoints[] Trail = new BladePoints[MaxTrail];
        VertexPositionColorTexture[] Vertices = new VertexPositionColorTexture[VerticesPerSide * 2];

        public BladeWeapon(LitObject body, AttachmentPoint root) 
            : base(body, root)
        {
            Initialize();
        }

        public BladeWeapon(ChiseDriveGame game, AddonSettings settings)
            : base(game, settings)
        {
            Initialize();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public void SetColor(Color color)
        {
            for (int i = 0; i < Vertices.Length; i+=2)
            {
                //float percent = 1f - (float)i / (float)(Vertices.Length / 2);
                //if (percent < 0f) percent += 1f;
                float percent = (float)i / (float)(Vertices.Length / 2);
                if (percent > 1f) percent -= 1f;

                Vertices[i].Color = new Color(color.R, color.G, color.B, (byte)(percent * 255f));
                Vertices[i+1].Color = new Color(color.R, color.G, color.B, (byte)(percent * 255f));
            }
        }

        public override bool Visible
        {
            get
            {
                return base.Visible;
            }
            set
            {
                base.Visible = value;
                if (value && !Game.DrawableEffects.Contains(this))
                {
                    Game.DrawableEffects.Add(this);
                }
                else if (!value && Game.DrawableEffects.Contains(this))
                {
                    Game.DrawableEffects.Remove(this);
                }
            }
        }

        void Initialize()
        {   
            Game.DrawableEffects.Add(this);

            SetColor(Color.White);

            for (int i = 0; i < Vertices.Length; i++)
            {
                if (i < VerticesPerSide)
                switch (i % 4)
                {
                    case 3: Vertices[i].TextureCoordinate = new Vector2(0f, 1f);
                        break;
                    case 2: Vertices[i].TextureCoordinate = new Vector2(0f, 0f);
                        break;
                    case 1: Vertices[i].TextureCoordinate = new Vector2(1f, 1f);
                        break;
                    case 0: Vertices[i].TextureCoordinate = new Vector2(1f, 0f);
                        break;
                }
                else 
                switch (i % 4)
                {
                    case 3: Vertices[i].TextureCoordinate = new Vector2(0f, 0f);
                        break;
                    case 2: Vertices[i].TextureCoordinate = new Vector2(0f, 1f);
                        break;
                    case 1: Vertices[i].TextureCoordinate = new Vector2(1f, 0f);
                        break;
                    case 0: Vertices[i].TextureCoordinate = new Vector2(1f, 1f);
                        break;
                }  
            }
        }

        public override Addon Clone(AttachmentPoint root)
        {
            return new BladeWeapon(Body.Clone(), root);
        }

        void AddPoints(Vector3 point1, Vector3 point2)
        {
            Step++;
            if (Step >= Trail.Length) 
                Step = 0;

            Trail[Step].Point1 = point1;
            Trail[Step].Point2 = point2;
        }

        public BladeEdge GetBladeEdge()
        {
            BladePoints finish = Trail[Step];
            BladePoints start = BladePoints.Empty;

            if (activeTail < 2)
            {
                start = Trail[Step];
            }
            else
            {
                int previous = Step - 1;
                if (previous < 0) previous = Trail.Length - 1;
                start = Trail[previous];
            }

            return new BladeEdge(start, finish);
        }

        void FillVertices()
        {
            // The first half of vertices are filled with alpha
            // from 0 to 1.  
            //
            // The first draw location is Step - activeTail
            // The first vertice location is 0
            // The last draw location is Step
            // The last vertice location is VerticesToDraw

            int verticesToDraw = activeTail * 2;

            int drawStep = Step - activeTail + 1;
            if (drawStep < 0) drawStep += Trail.Length;

            for (int i = 0; i < verticesToDraw; i += 2)
            {
                Vertices[i].Position = Trail[drawStep].Point1;
                Vertices[i + 1].Position = Trail[drawStep].Point2;

                drawStep++;
                if (drawStep >= Trail.Length - 1) drawStep = 0;
            }
            
            drawStep = Step - activeTail;
            if (drawStep < 0) drawStep += Trail.Length;

            for (int i = VerticesPerSide + verticesToDraw - 2; i >= VerticesPerSide; i -= 2)
            {
                Vertices[i].Position = Trail[drawStep].Point2;
                Vertices[i + 1].Position = Trail[drawStep].Point1;

                drawStep--;
                if (drawStep < 0) drawStep = Trail.Length - 1;
            }
           
            /*
            for (int i = 0; i < VerticesPerSide; i += 2)
            {
                Vertices[i].Position = Trail[Step].Point1;
                Vertices[i + 1].Position = Trail[Step].Point2;

                Step--;
                if (Step < 0) Step = Trail.Length - 1;
            }
            
            for (int i = VerticesPerSide * 2 - 2; i >= VerticesPerSide; i -= 2)
            {
                // Move ahead first, so our first coord is at the tail, and the
                // last coord at the head.
                Step++;
                if (Step >= Trail.Length) Step = 0;

                Vertices[i].Position = Trail[Step].Point2;
                Vertices[i + 1].Position = Trail[Step].Point1;
            }
            */
        }

        public void Draw()
        {
            if (effect == null) throw new Exception("Call BladeWeapon.Initialize before using.");

            FillVertices();

            effect.CurrentTechnique = effect.Techniques[0];

            effect.Parameters["World"].SetValue(Matrix.Identity);
            effect.Parameters["View"].SetValue(Game.Camera.View);
            effect.Parameters["Projection"].SetValue(Game.Camera.Projection);

            effect.Parameters["TextureMap"].SetValue(BladeWeapon.texture);

            Game.GraphicsDevice.VertexDeclaration = BladeWeapon.declaration;

            Game.GraphicsDevice.RenderState.DepthBufferEnable = true;
            Game.GraphicsDevice.RenderState.DepthBufferWriteEnable = false;
            Game.GraphicsDevice.RenderState.AlphaBlendEnable = true;
            Game.GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
            Game.GraphicsDevice.RenderState.DestinationBlend = Blend.One;
            Game.GraphicsDevice.RenderState.BlendFunction = BlendFunction.Add;
            
            effect.Begin();
            int verticesToDraw = (activeTail * 2) - 4;

            if (verticesToDraw > 0)
            {
                for (int i = 0; i < effect.CurrentTechnique.Passes.Count; i++)
                {
                    effect.CurrentTechnique.Passes[i].Begin();

                    Game.GraphicsDevice.DrawUserPrimitives<VertexPositionColorTexture>(
                        PrimitiveType.TriangleStrip,
                        Vertices,
                        0,
                        verticesToDraw);

                    Game.GraphicsDevice.DrawUserPrimitives<VertexPositionColorTexture>(
                        PrimitiveType.TriangleStrip,
                        Vertices,
                        VerticesPerSide + 2,
                        verticesToDraw);

                    effect.CurrentTechnique.Passes[i].End();
                }
            }

            effect.End();
        }

        public override void Update(Time elapsed)
        {
            base.Update(elapsed);

            Matrix bladeBase = Body.Mesh.BoneTransform("Base");
            Matrix bladeTip = Body.Mesh.BoneTransform("Tip");

            bladeBase *= RotationPosition;
            bladeTip *= RotationPosition;

            //ChiseDrive.Debug.DebugText.Write("Weapon Step [" + Step + "]"
            //    + " Base [" + bladeBase.Translation + "]"
            //    + " Tip [" + bladeTip.Translation + "]");

            AddPoints(bladeTip.Translation, bladeBase.Translation);
            
            if (Active) activeTail++;
            else activeTail-=2;

            Helper.Clamp(ref activeTail, 0, MaxTrail);
        }
    }
}