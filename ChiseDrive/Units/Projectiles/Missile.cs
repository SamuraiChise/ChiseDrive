using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ChiseDrive.Physics;
using ChiseDrive.Graphics;

namespace ChiseDrive.Units
{
    class Missile
    {
        #region Fields
        // Disposable
        IPhysics physics = new BasicPhysics();
        List<Vector3> trailpoints = new List<Vector3>();
        PointSpriteD missilesprite;
        Arc missilearc;

        // References
        ID parentid;

        // Data
        Vector3 position;
        Vector3 waypoint;
        Vector3 target;

        float acceleration = 0f;        // Acceleration to apply
        Time decaytrail = 120f;         // How long before the trail goes away

        // Flags
        bool detonated = false;
        bool pastwaypoint = false;

        Faction faction;
        #endregion
        /*
        public Missile(Vector3 origin, Vector3 target, Faction faction, ID parentid)
        {
            this.target = target;
            this.position = origin;
            this.parentid = parentid;
            physics.TerminalVelocity = 100f;// 500f;
            physics.Mass = 1f;
            physics.Dampen = 0.1f;
            acceleration = 5f;//7f;


            //missilesprite = new PointSpriteD(AssetPreloader.FetchAsset<AnimatedTexture>(Asset.TextureMissile));
            //missilesprite.Texture.Loop();
            missilearc = new Arc(new Color(200, 200, 255, 150));

            this.faction = faction;
            CalculateArcBulgePoint();
        }

        void CalculateArcBulgePoint()
        {
            lastdistance = Vector3.DistanceSquared(this.position, target.Position);
            Vector3 totarget = target.Position - Position;
            Vector3 midpoint = Position + (totarget / 12f);
            totarget.Normalize();

            Vector3 up = Vector3.Up;

            Matrix rotation = Matrix.CreateFromAxisAngle(Vector3.Forward, Helper.Randomf() * 6.28f);
            up = Vector3.Transform(up, rotation);

            Vector3 cross = Vector3.Cross(totarget, up);
            cross *= 500f;

            waypoint = midpoint + cross;
        }

        bool drop = false;

        void Accelerate(Time elapsed)
        {
            Vector3 destination;    // For this frame

            if (!pastwaypoint)
            {
                destination = waypoint;
            }
            else
            {
                destination = target.Position;
            }

            Vector3 totarget = destination - position;
            float newdistance = Math.Abs(totarget.Length());


            // And move the missile
            if (totarget != Vector3.Zero)
            {
                // Build the acceleration vector
                totarget.Normalize();

                if (newdistance < closerange && pastwaypoint)
                {
                    totarget *= closeacceleration;
                }
                else
                {
                    totarget *= acceleration;
                }
                physics.Push(totarget);
                Position = physics.RunPhysics(elapsed, Position);
            }
            trailpoints.Add(Position);
            drop = !drop;
            if (drop) trailpoints.RemoveRange(0, 1);

            if (pastwaypoint)
            {
                if (lastdistance < newdistance)
                {
                    // Destruct on a miss
                    TakeDamage(1f, 0, Damage.Missile);
                }

                if (detonated)
                {
                    // On hits and misses, get rid of the last few points.
                    const int maxtrim = 2;
                    int remove = trailpoints.Count > maxtrim ? maxtrim : trailpoints.Count;
                    trailpoints.RemoveRange(trailpoints.Count - remove, remove);
                }
            }
            else //!pastwaypoint
            {
                if (lastdistance < newdistance || newdistance < hitdistance)
                {
                    pastwaypoint = true;
                    newdistance = Vector3.DistanceSquared(target.Position, Position);
                }
            }

            if (trailpoints.Count > 200) trailpoints.RemoveRange(0, 4);

            lastdistance = newdistance;
        }

        public void Update(Time elapsed)
        {
            launchtime -= elapsed;
            decaytrail -= elapsed;
            if (detonated)
            {
                if (deathtime.IsZero && trailpoints.Count == 0) Remove();
                else Detonate(elapsed);
            }
            else// Active
            {
                Accelerate(elapsed);
            }
        }

        public void Draw()
        {
            if (!detonated)
            {
                missilesprite.Draw(Position, 5000f * SolInvasion.Settings.GlobalScaling, 1f);
            }
            missilearc.Draw(trailpoints, 5f, 1f);
        }*/
    }
}