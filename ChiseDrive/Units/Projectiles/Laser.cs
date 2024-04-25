using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ChiseDrive.Graphics;
using ChiseDrive.Cameras;
using ChiseDrive.Physics;

namespace ChiseDrive.Units
{
    public class Laser
    {
        public enum Caliber
        {
            n200mm,
        };

        const int MaxLasers = 100;

        #region Buffers and Shared Resources
        static int particlecount = 0;
        static Laser[] buffer = new Laser[MaxLasers];
        static VertexPositionColorTexture[] vertices = new VertexPositionColorTexture[MaxLasers * 6];
        static Texture2D LaserTexture = null;
        static Effect effect;
        static VertexDeclaration declaration;
        static ChiseDriveGame Game;

        /// <summary>
        /// Initializes the shared resources of the Laser class
        /// </summary>
        /// <param name="game">The game to run.</param>
        /// <param name="effectfile">The name of the Laser effect.</param>
        /// <param name="texturefile">The name of the Laser texture.</param>
        static public void Initialize(ChiseDriveGame game, string effectfile, string texturefile)
        {
            Game = game;
            effect = game.Content.Load<Effect>(effectfile);
            declaration = new VertexDeclaration(game.GraphicsDevice, VertexPositionColorTexture.VertexElements);
            for (int i = 0; i < MaxLasers; i++)
            {
                buffer[i] = new Laser();
                buffer[i].Life = 0f;

                vertices[i * 6 + 0].TextureCoordinate = new Vector2(1f, 1f);
                vertices[i * 6 + 1].TextureCoordinate = new Vector2(1f, 0f);
                vertices[i * 6 + 2].TextureCoordinate = new Vector2(0f, 0f);
                vertices[i * 6 + 3].TextureCoordinate = new Vector2(0f, 0f);
                vertices[i * 6 + 4].TextureCoordinate = new Vector2(0f, 1f);
                vertices[i * 6 + 5].TextureCoordinate = new Vector2(1f, 1f);
            }
            for (int i = 0; i < MaxLasers * 6; i++)
            {
                vertices[i].Color = Color.White;
            }
            LaserTexture = game.Content.Load<Texture2D>(texturefile);
        }
        #endregion

        // Update until Expired
        public IFollow Emitter;
        public float Life;

        float scale;
        float length;
        float damage;
        ID parentID;

        public Vector3 Velocity
        {
            get
            {
                return Emitter.RotationPosition.Up * length;
            }
        }
        static public float Speed(Caliber caliber)
        {
            switch (caliber)
            {
                case Caliber.n200mm: return 0f;
            }
            return 0f;
        }
        public ID ID
        {
            get
            {
                return parentID;
            }
        }
        const float DefaultLength = 1000f;
        const float MaxLife = 120f;
        const float FadeTime = 30f;

        /// <summary>
        /// Fires a single Laser.  (does not use a constructor to avoid allocating "new" memory)
        /// </summary>
        /// <param name="Emitter.Position">Point of origin.</param>
        /// <param name="direction">Direction fired.</param>
        /// <param name="caliber">Type of Laser.</param>
        /// <param name="parentID">Who fired it. (important for record keeping and collision cheats)</param>
        static public void FireLaser(IFollow emitter, Vector3 direction, Caliber caliber, ID parentID)
        {
            if (particlecount >= MaxLasers) throw new Exception("Increase Laser.MaxLasers");

            buffer[particlecount].Emitter = emitter;
            buffer[particlecount].parentID = parentID;
            buffer[particlecount].length = DefaultLength;

            switch (caliber)
            {
                case Caliber.n200mm:
                    buffer[particlecount].scale = 20f;
                    buffer[particlecount].damage = 1f;
                    buffer[particlecount].Life = MaxLife;
                    break;
            }

            particlecount++;
        }

        public override string ToString()
        {
            return "[POS: " + Emitter.Position + ", ID: " + parentID.Value + "]";
        }

        /// <summary>
        /// Moves expired Lasers to the end.  Should be called once per update.
        /// </summary>
        public static void CompactParticles()
        {
            int compactplace = 0;

            if (particlecount == 0) return;

            for (int i = 0; i < buffer.Length; i++)
            {
                foreach (Unit u in Game.Units)
                {
                    // In the case of bosses, the sub hulls might be dead
                    // after the hull that fired this laser...GunsHack
                    if (u.ID == buffer[i].ID && u.IsAlive) break;
                    if (u.ID == buffer[i].ID && !u.IsAlive)
                    {
                        buffer[i].Life = 0f;
                    }
                }

                if (buffer[i].Life <= 0f)
                {
                    // We have an expired Laser!
                    // Assume it's our last one!
                    particlecount = i;

                    // Keep track of the last place we found good data
                    if (compactplace < i) compactplace = i;
                    for (int k = compactplace; k <= buffer.Length; k++)
                    {
                        if (k == buffer.Length) return;
                        if (buffer[k].Life > 0f)
                        {
                            Laser temp = buffer[i];
                            buffer[i] = buffer[k];
                            buffer[k] = temp;
                            compactplace = k + 1;
                            k = buffer.Length;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Clears all the Lasers.  Good to call between stages.
        /// </summary>
        public static void Reset()
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i].Life = 0f;
            }
            CompactParticles();
        }

        /// <summary>
        /// Updates all the Laser trajectories.  Should be called once per update.
        /// </summary>
        /// <param name="elapsed">The time elapsed.</param>
        public static void UpdateAll(Time elapsed)
        {
            for (int i = 0; i < particlecount; i++)
            {
                buffer[i].Life -= elapsed;
            }
            CompactParticles();
        }

        #region Collisions
        static int step = 0;
        static int updatecount = 0;
        static int updatespan = 0;
        const int MaxObjects = 200;
        // One-time allocate the array to keep the memory nice and contiguous.
        static List<IBounding> collisionlist = new List<IBounding>(MaxObjects);


        /// <summary>
        /// Call this once per update to resolve as many Laser collisions
        /// as the system has time for.  Not all Lasers may be processed on
        /// any one call.
        /// </summary>
        /// <param name="game">The game that we're using.</param>
        /// <param name="elapsed">The time that's elapsed.</param>
        public static void ResolveCollisions(ChiseDriveGame game, Time elapsed)
        {
            // Build a collision list on the first step, or after completing
            // a full collision cycle.
            if (step == 0 || step > particlecount)
            {
                // Build a list of the current objects that can be collided.
                collisionlist.Clear();
                foreach (Unit u in game.Units)
                {
                    collisionlist.Add(u as IBounding);
                }
            }

            // Not every Laser is processed on every collision update
            int collisionstorun = 400 < particlecount ? 400 : particlecount;
            updatespan = particlecount / 400;
            if (updatespan < 1) updatespan = 1;
            Ray particleray1 = new Ray();
            Ray particleray2 = new Ray();
            Ray particleray3 = new Ray();

            while (collisionstorun > 0)
            {
                // Reset the length to be shortened on impact
                buffer[step].length = DefaultLength;

                particleray1.Position = buffer[step].Emitter.Position;
                particleray1.Direction = buffer[step].Emitter.RotationPosition.Up;
                particleray1.Direction.Normalize();

                particleray2 = particleray1;
                particleray3 = particleray1;

                Vector3 centerpoint = buffer[step].Emitter.Position;
                Vector3 tocamera = game.Camera.Position - centerpoint;
                Helper.Normalize(ref tocamera);
                Vector3 width = Vector3.Cross(particleray1.Direction, tocamera);
                Helper.Normalize(ref width);
                width *= buffer[step].scale * 0.2f;

                particleray2.Position += width;
                particleray3.Position -= width;

                if (ChiseDriveGame.Force2D)
                {
                    particleray1.Position.Z = 0f;
                    particleray1.Direction.Z = 0f;
                    particleray2.Position.Z = 0f;
                    particleray2.Direction.Z = 0f;
                    particleray3.Position.Z = 0f;
                    particleray3.Direction.Z = 0f;
                }

                for (int i = 0; i < collisionlist.Count; i++)
                {
                    // Skip any self shooting
                    Unit testunit = collisionlist[i] as Unit;
                    if (testunit == null
                        || !testunit.Active
                        || testunit.ID == buffer[step].ID) 
                        continue;

                    Vector4? intersect = collisionlist[i].Intersects(particleray1, buffer[step].length);
                    if (intersect == null)
                    {
                        intersect = collisionlist[i].Intersects(particleray2, buffer[step].length);
                    }
                    if (intersect == null)
                    {
                        intersect = collisionlist[i].Intersects(particleray3, buffer[step].length);
                    }

                    if (intersect != null && ((Vector4)intersect).W < DefaultLength)
                    {
                        bool FriendlyFire = false;
                        foreach (Unit u in Game.Units)
                        {
                            if (u.ID == buffer[step].ID)
                            {
                                if (u.Faction.GetAlignment(testunit.Faction) == Alignment.Friendly)
                                {
                                    FriendlyFire = true;
                                    break;
                                }
                            }
                        }
                        if (FriendlyFire) continue;

                        buffer[step].length = ((Vector4)intersect).W;

                        float damage = buffer[step].damage * elapsed;

                        if (buffer[step].Life < FadeTime)
                        {
                            damage *= buffer[step].Life / FadeTime;
                        }
                        if (buffer[step].Life > (MaxLife - FadeTime))
                        {
                            damage *= (MaxLife - buffer[step].Life) / FadeTime;
                        }

                        IDestructible takesdamage = collisionlist[i] as IDestructible;
                        if (takesdamage != null) takesdamage.TakeDamage(buffer[step].damage * elapsed, buffer[step].ID);

                        IFollow follows = collisionlist[i] as IFollow;
                        if (follows != null)
                        {
                            Vector3 impact = new Vector3(((Vector4)intersect).X, ((Vector4)intersect).Y, ((Vector4)intersect).Z);
                            Vector3 delta = impact - follows.Position;
                            delta *= 0.8f;
                            Vector3 dir = delta;
                            dir.Normalize();
                            Explosion.Emit(follows, delta, dir, 1f);
                        }

                        break;
                    }

                    // Each inside loop counts as a collision...
                    collisionstorun--;
                }

                // ... and each outside loop counts as a collision.
                collisionstorun--;

                // And we're on to the next element in the list.
                step++;

                if (step >= particlecount)
                {
                    // Always reset when finished resolving collisions,
                    // since there could be either new objects added,
                    // or old objects that have been destroyed.
                    collisionlist.Clear();
                    step = 0;
                    collisionstorun = 0;
                }
            }

            updatecount++;
        }
        #endregion

        /// <summary>
        /// Fills in the vertices for a single Laser.
        /// </summary>
        /// <param name="camera">The camera being used.</param>
        /// <param name="index">The index of this Laser.</param>
        void FillVertices(Cameras.Camera camera, int index)
        {
            Vector3 centerpoint = Emitter.Position;
            centerpoint += Emitter.RotationPosition.Up * scale * this.length * 0.5f;

            Vector3 tocamera = camera.Position - centerpoint;
            Helper.Normalize(ref tocamera);
            Vector3 width = Vector3.Cross(Emitter.RotationPosition.Up, tocamera);
            Helper.Normalize(ref width);
            width *= scale;
            Vector3 direction = Emitter.RotationPosition.Up;
            direction.Normalize();
            Vector3 length = direction * this.length;
            
            vertices[index + 0].Position = Emitter.Position - width;
            vertices[index + 1].Position = Emitter.Position - width + length;
            vertices[index + 2].Position = Emitter.Position + width + length;
            vertices[index + 3].Position = Emitter.Position + width + length;
            vertices[index + 4].Position = Emitter.Position + width;
            vertices[index + 5].Position = Emitter.Position - width;

            float delta = FadeTime;
            if (Life - FadeTime < 0f) delta = Life;
            if (Life + FadeTime > MaxLife) delta = MaxLife - Life;
            delta /= FadeTime;

            byte alpha = (byte)(delta * 255f);

            vertices[index + 0].Color.A = alpha;
            vertices[index + 1].Color.A = alpha;
            vertices[index + 2].Color.A = alpha;
            vertices[index + 3].Color.A = alpha;
            vertices[index + 4].Color.A = alpha;
            vertices[index + 5].Color.A = alpha;

        }

        /// <summary>
        /// Draws all the Lasers.  Should be called once per Draw cycle.
        /// </summary>
        /// <param name="device">The graphics device to use.</param>
        /// <param name="camera">The camera to use.</param>
        public static void DrawAll(GraphicsDevice device, Camera camera)
        {
            if (particlecount > 0)
            {
                for (int i = 0; i < particlecount; i++)
                {
                    buffer[i].FillVertices(camera, i * 6);
                }

                effect.CurrentTechnique = effect.Techniques[0];

                effect.Parameters["World"].SetValue(Matrix.Identity);
                effect.Parameters["View"].SetValue(camera.View);
                effect.Parameters["Projection"].SetValue(camera.Projection);
                effect.Parameters["TextureMap"].SetValue(LaserTexture);

                device.VertexDeclaration = declaration;

                device.RenderState.DepthBufferEnable = true;
                device.RenderState.DepthBufferWriteEnable = false;
                device.RenderState.AlphaBlendEnable = true;
                device.RenderState.AlphaTestEnable = false;
                device.RenderState.ReferenceAlpha = 0;
                device.RenderState.DestinationBlend = Blend.One;
                device.RenderState.SourceBlend = Blend.SourceAlpha;
                device.RenderState.BlendFunction = BlendFunction.Add;

                effect.Begin();

                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Begin();
                    device.DrawUserPrimitives<VertexPositionColorTexture>(PrimitiveType.TriangleList,
                        vertices, 0, particlecount * 2);
                    pass.End();
                }

                effect.End();
            }
        }
    }
}
