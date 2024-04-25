using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ChiseDrive.Cameras
{
    public class SmartCamera : Camera
    {
        public SmartCamera(GraphicsDevice graphics) : base(graphics) { }
        /*
        struct PointofInterest
        {
            public float interestingness;
            public Time decay;
            public IFollow point;
            public float zoom;
        };

        /// <summary>
        /// Target is close to center screen
        /// </summary>
        public const float LowInterest = 2f;

        /// <summary>
        /// Target is onscreen
        /// </summary>
        public const float MediumInterest = 4f;

        /// <summary>
        /// Target can be close to screen (good for unzooms)
        /// </summary>
        public const float HighInterest = 6f;

        /// <summary>
        /// Target can be anywhere
        /// </summary>
        public const float VeryHighInterest = 8f;

        public const float StandardZoom = 45f;
        public const float MinorZoom = StandardZoom - 10f;
        public const float MajorZoom = StandardZoom - 20f;
        public const float LockZoom = StandardZoom - 7f;
        public const float UnZoom = StandardZoom + 10f;

        bool active = false;
        public bool Active
        {
            set
            {
                active = value;
            }
        }

        const int maxinterest = 30;
        PointofInterest[] pointsofinterest = new PointofInterest[maxinterest];
        PointofInterest defaultinterest = new PointofInterest();
        Vector3 overrideOffset;

        Cooldown relookcooldown = new Cooldown(Time.Seconds(0.8f));
        Cooldown minorlookcooldown = new Cooldown(Time.Seconds(4f));
        Cooldown standardlookcooldown = new Cooldown(Time.Seconds(8f));
        Cooldown majorlookcooldown = new Cooldown(Time.Seconds(16f));

        public float InterestThreshold
        {
            set
            {
                defaultinterest.interestingness = value;
            }
        }
        public override Vector3 Offset
        {
            get
            {
                return overrideOffset;
            }
            set
            {
                overrideOffset = value;
            }
        }

        public SmartCamera()
        {
            for (int i = 0; i < maxinterest; i++)
            {
                pointsofinterest[i] = new PointofInterest();
                pointsofinterest[i].interestingness = 0f;
                pointsofinterest[i].decay = Time.Zero;
                pointsofinterest[i].point = null;
            }

            SetDefaultInterest(this);
        }

        public void SetDefaultInterest(IFollow point, float zoom)
        {
            defaultinterest.point = point;
            defaultinterest.interestingness = 1f;
            defaultinterest.zoom = zoom;
            SetFocus(MostInterestingPoint());
        }

        public void SetDefaultInterest(IFollow point)
        {
            SetDefaultInterest(point, StandardZoom);
        }

        /// <summary>
        /// Assigns interesting things for the camera to look at.
        /// </summary>
        /// <param name="point">IPosition object to lookat.</param>
        /// <param name="interestingness">2f - 5f is less interesting than lock on 5f - oo is more interesting than lock on.</param>
        /// <param name="decay">Time that the point will stay queued.</param>
        public void AddPointofInterest(IFollow point, float interestingness, Time decay)
        {
            AddPointofInterest(point, interestingness, decay, StandardZoom);
        }

        /// <summary>
        /// Assigns interesting things for the camera to look at.
        /// </summary>
        /// <param name="point">IPosition object to lookat.</param>
        /// <param name="interestingness">2f - 5f is less interesting than lock on 5f - oo is more interesting than lock on.</param>
        /// <param name="decay">Time that the point will stay queued.</param>
        /// <param name="zoom">Special zoom modifier, zoom>1 to zoom in more, 1>zoom to zoom in less.  null for default.</param>
        public void AddPointofInterest(IFollow point, float interestingness, Time decay, float zoom)
        {
            for (int i = 0; i < maxinterest; i++)
            {
                if (pointsofinterest[i].interestingness == 0f)
                {
                    pointsofinterest[i].point = point;
                    pointsofinterest[i].interestingness = interestingness;
                    pointsofinterest[i].decay = decay;
                    pointsofinterest[i].zoom = zoom;
                    return;
                }
            }
            //throw new SystemException("Points of Interest overloaded.");
        }

        IFollow MostInterestingPoint()
        {
            return MostInterestingPointofInterest().point;
        }

        PointofInterest MostInterestingPointofInterest()
        {
            if (!active) return defaultinterest;
            PointofInterest currentleader = defaultinterest;
            float currentinterest = currentleader.interestingness;

            Matrix minorProject = Matrix.CreatePerspectiveFieldOfView(0.2f, aspectRatio, near, far);
            Matrix standardProject = Matrix.CreatePerspectiveFieldOfView(0.4f, aspectRatio, near, far);
            Matrix majorProject = Matrix.CreatePerspectiveFieldOfView(1f, aspectRatio, near, far);

            BoundingFrustum minorLook = new BoundingFrustum(View * minorProject);
            BoundingFrustum standardLook = new BoundingFrustum(View * standardProject);
            BoundingFrustum majorLook = new BoundingFrustum(View * majorProject);

            foreach (PointofInterest poi in pointsofinterest)
            {
                // 0f - 1f
                float randombump = Helper.Randomf;
                float interest = poi.interestingness + randombump;

                if (poi.point == null) continue;
                if (interest > VeryHighInterest
                    && interest > currentinterest)
                {
                    // Very high interest doesn't care where it is
                    currentleader = poi;
                    currentinterest = interest;
                    continue;
                }

                if (poi.interestingness >= HighInterest
                    && interest > currentinterest
                    && majorlookcooldown.IsReady
                    && majorLook.Contains(poi.point.Position) == ContainmentType.Contains)
                {
                    currentleader = poi;
                    currentinterest = interest;
                    continue;
                }

                if (poi.interestingness >= MediumInterest
                    && interest > currentinterest
                    && standardlookcooldown.IsReady
                    && standardLook.Contains(poi.point.Position) == ContainmentType.Contains)
                {
                    currentleader = poi;
                    currentinterest = interest;
                    continue;
                }   

                if (poi.interestingness >= LowInterest
                    && interest > currentinterest
                    && minorlookcooldown.IsReady
                    && minorLook.Contains(poi.point.Position) == ContainmentType.Contains)
                {
                    currentleader = poi;
                    currentinterest = interest;
                    continue;
                }
            }

            if (currentleader.interestingness >= HighInterest) majorlookcooldown.Trigger();
            else if (currentleader.interestingness >= MediumInterest) standardlookcooldown.Trigger();
            else if (currentleader.interestingness >= LowInterest) minorlookcooldown.Trigger();

            return currentleader;
        }

        void DecayInterest(Time elapsed)
        {
            for (int i = 0; i < maxinterest; i++)
            {
                pointsofinterest[i].decay -= elapsed;
                if (pointsofinterest[i].decay.IsZero())
                {
                    pointsofinterest[i].interestingness = 0f;
                    pointsofinterest[i].point = null;
                }
            }
        }

        void DoLookZoom(Time elapsed)
        {
            PointofInterest mostinteresting = MostInterestingPointofInterest();

            // Will refocus on default interest if that was lost
            SetFocus(mostinteresting.point);
            Zoom = mostinteresting.zoom;
        }

        public override void Update(Time elapsed)
        {
            DecayInterest(elapsed);
            this.minorlookcooldown.Update(elapsed);
            this.standardlookcooldown.Update(elapsed);
            this.majorlookcooldown.Update(elapsed);
            if (relookcooldown.AutoTrigger(elapsed)) DoLookZoom(elapsed);
            base.Update(elapsed);
        }
    }*/
    }
}
