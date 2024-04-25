using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChiseDrive.Cameras
{
    public class BlendCamera : Camera
    {
        const int MaxBlends = 10;
     
        struct Blend
        {
            const float FadeTime = 10f;

            public CameraShot CameraShot;
            public Timer Timer;
            public float Destination;
            public float FadeStep;
            public float Weight;

            public void Reset()
            {
                Timer.Set(Time.Zero);
            }
            public void Create(CameraShot shot, float weight, Time time)
            {
                Timer.Set(time);
                Destination = weight;
                Weight = 0f;
                FadeStep = Destination / FadeTime;
                CameraShot = shot;
            }
            public void Update(Time elapsed)
            {
                Timer.SubTime(elapsed);

                if (Timer.Frames < FadeTime)
                {
                    Weight -= FadeStep;
                }
                else if (Weight != Destination)
                {
                    Weight += FadeStep;
                }

                Helper.Clamp(ref Weight, 0f, 1f);
            }
        };

        Blend[] Blends = new Blend[MaxBlends];

        public BlendCamera(ChiseDriveGame game)
            : base(game.GraphicsDevice)
        {
            for (int i = 0; i < MaxBlends; i++)
            {
                Blends[i].Reset();
            }
        }

        public void NewBlend(CameraShot blend, float weight, Time time)
        {
            for (int i = 0; i < MaxBlends; i++)
            {
                if (Blends[i].Timer.IsZero)
                {
                    Blends[i].Create(blend, weight, time);
                    break;
                }
            }
        }

        public override void Update(Time elapsed)
        {
            base.Update(elapsed);

            for (int i = 0; i < MaxBlends; i++)
            {
                Blends[i].Update(elapsed);

                if (!Blends[i].Timer.IsZero)
                {
                    currentshot = CameraShot.Lerp(currentshot, Blends[i].CameraShot, Blends[i].Weight);
                }
            }
        }
    }
}