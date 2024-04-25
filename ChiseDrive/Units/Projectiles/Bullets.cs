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
    public class Bullet
    {
        public enum Caliber
        {
            c50mm,
            c100mm,
        };

        const int MaxBullets = 4000;

        #region Buffers and Shared Resources
        static ChiseDriveGame Game;
        static int particlecount = 0;
        static Bullet[] buffer = new Bullet[MaxBullets];
        static VertexPositionTexture[] vertices = new VertexPositionTexture[MaxBullets * 6];
        static Texture2D bulletTexture = null;
        static Effect effect;
        static VertexDeclaration declaration;

        /// <summary>
        /// Initializes the shared resources of the Bullet class
        /// </summary>
        /// <param name="game">The game to run.</param>
        /// <param name="effectfile">The name of the bullet effect.</param>
        /// <param name="texturefile">The name of the bullet texture.</param>
        static public void Initialize(ChiseDriveGame game, string effectfile, string texturefile)
        {
            Game = game;
            effect = game.Content.Load<Effect>(effectfile);
            declaration = new VertexDeclaration(game.GraphicsDevice, VertexPositionTexture.VertexElements);
            for (int i = 0; i < MaxBullets; i++)
            {
                buffer[i] = new Bullet();
                buffer[i].Life = 0f;

                vertices[i * 6 + 0].TextureCoordinate = new Vector2(1f, 1f);
                vertices[i * 6 + 1].TextureCoordinate = new Vector2(1f, 0f);
                vertices[i * 6 + 2].TextureCoordinate = new Vector2(0f, 0f);
                vertices[i * 6 + 3].TextureCoordinate = new Vector2(0f, 0f);
                vertices[i * 6 + 4].TextureCoordinate = new Vector2(0f, 1f);
                vertices[i * 6 + 5].TextureCoordinate = new Vector2(1f, 1f);
            }
            bulletTexture = game.Content.Load<Texture2D>(texturefile);
        }
        #endregion

        // Update until Expired
        public Vector3 Position;
        public float Life;

        // Fire and Forget
        Vector3 direction;
        float scale;
        float speed;
        float damage;
        ID parentID;

        public Vector3 Velocity
        {
            get
            {
                return direction * speed;
            }
        }
        static public float Speed(Caliber caliber)
        {
            switch (caliber)
            {
                case Caliber.c50mm: return 8f;
                case Caliber.c100mm: return 5f;
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
        const float Length = 10f;

        /// <summary>
        /// Fires a single bullet.  (does not use a constructor to avoid allocating "new" memory)
        /// </summary>
        /// <param name="position">Point of origin.</param>
        /// <param name="direction">Direction fired.</param>
        /// <param name="caliber">Type of bullet.</param>
        /// <param name="parentID">Who fired it. (important for record keeping and collision cheats)</param>
        static public void FireBullet(Vector3 position, Vector3 direction, Caliber caliber, ID parentID)
        {
            if (particlecount >= MaxBullets) throw new Exception("Increase Bullet.MaxBullets");

            buffer[particlecount].Position = position;
            buffer[particlecount].direction = Helper.Normalize(direction);
            buffer[particlecount].parentID = parentID;
            buffer[particlecount].speed = Speed(caliber);

            switch (caliber)
            {
                case Caliber.c50mm:
                    buffer[particlecount].scale = 2f;
                    buffer[particlecount].damage = 10f;
                    buffer[particlecount].Life = 100f;
                    break;
                case Caliber.c100mm:
                    buffer[particlecount].scale = 2f;
                    buffer[particlecount].damage = 10f;
                    buffer[particlecount].Life = 150f;
                    break;
            }

            particlecount++;
        }

        public override string ToString()
        {
            return "[POS: " + Position + ", ID: " + parentID.Value + "]";
        }

        /// <summary>
        /// Moves expired bullets to the end.  Should be called once per update.
        /// </summary>
        public static void CompactParticles()
        {
            int compactplace = 0;

            if (particlecount == 0) return;

            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i].Life <= 0f)
                {
                    // We have an expired bullet!
                    // Assume it's our last one!
                    particlecount = i;

                    // Keep track of the last place we found good data
                    if (compactplace < i) compactplace = i;
                    for (int k = compactplace; k <= buffer.Length; k++)
                    {
                        if (k == buffer.Length) return;
                        if (buffer[k].Life > 0f)
                        {
                            Bullet temp = buffer[i];
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
        /// Clears all the bullets.  Good to call between stages.
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
        /// Updates all the bullet trajectories.  Should be called once per update.
        /// </summary>
        /// <param name="elapsed">The time elapsed.</param>
        public static void UpdateAll(Time elapsed)
        {
            for (int i = 0; i < particlecount; i++)
            {
                buffer[i].Position += buffer[i].Velocity * elapsed;
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
        /// Call this once per update to resolve as many bullet collisions
        /// as the system has time for.  Not all bullets may be processed on
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

            // Not every bullet is processed on every collision update
            int collisionstorun = 400 < particlecount ? 400 : particlecount;
            updatespan = particlecount / 400;
            if (updatespan < 1) updatespan = 1;
            Ray particleray = new Ray();
            float length = 0f;

            while (collisionstorun > 0)
            {
                if (buffer[step].Life > 0f)
                {
                    particleray.Position = buffer[step].Position;
                    particleray.Direction = buffer[step].Velocity;
                    particleray.Direction.Normalize();

                    if (ChiseDriveGame.Force2D)
                    {
                        particleray.Direction.Z = 0f;
                        particleray.Position.Z = 0f;
                    }

                    // Figure out how long this ray is, correcting for elapsed time and the updatespan.
                    length = Math.Abs(buffer[step].Velocity.Length()*elapsed*(float)(updatespan+10));

                    for (int i = 0; i < collisionlist.Count; i++)
                    {
                        // Skip any self shooting
                        Unit testunit = collisionlist[i] as Unit;
                        if (testunit == null
                            || !testunit.Active
                            || testunit.ID == buffer[step].ID) 
                            continue;
                        
                        Vector4? intersect = collisionlist[i].Intersects(particleray, length);
                        if (intersect != null)
                            if (((Vector4)intersect).W < length)
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

                            IDestructible takesdamage = collisionlist[i] as IDestructible;
                            if (takesdamage != null) takesdamage.TakeDamage(buffer[step].damage, buffer[step].ID);

                            IFollow follows = collisionlist[i] as IFollow;
                            if (follows != null)
                            {
                                Vector3 impact = new Vector3(((Vector4)intersect).X, ((Vector4)intersect).Y, ((Vector4)intersect).Z);
                                Vector3 delta = impact - follows.Position;
                                delta *= 0.8f;
                                Vector3 dir = delta;
                                dir.Normalize();
                                Explosion.Emit(follows, delta, dir, 1f);
                                UnitEvent.Announce(EventType.WeaponHit, buffer[step].ID, ID.None, 1f);
                            }

                            buffer[step].Life = 0f;
                            break;
                        }

                        // Each inside loop counts as a collision...
                        collisionstorun--;
                    }
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
        /// Fills in the vertices for a single bullet.
        /// </summary>
        /// <param name="camera">The camera being used.</param>
        /// <param name="index">The index of this bullet.</param>
        void FillVertices(Cameras.Camera camera, int index)
        {
            Vector3 centerpoint = Position;
            centerpoint += direction * scale * Length;

            Vector3 tocamera = camera.Position - centerpoint;
            Helper.Normalize(ref tocamera);
            Vector3 width = Vector3.Cross(direction, tocamera);
            Helper.Normalize(ref width);
            width *= scale;
            Vector3 length = direction * Length * 2f;
            
            vertices[index + 0].Position = Position - width;
            vertices[index + 1].Position = Position - width + length;
            vertices[index + 2].Position = Position + width + length;
            vertices[index + 3].Position = Position + width + length;
            vertices[index + 4].Position = Position + width;
            vertices[index + 5].Position = Position - width;
        }

        /// <summary>
        /// Draws all the bullets.  Should be called once per Draw cycle.
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
                effect.Parameters["TextureMap"].SetValue(bulletTexture);

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
                    device.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleList,
                        vertices, 0, particlecount * 2);
                    pass.End();
                }

                effect.End();
            }
        }
    }
}
