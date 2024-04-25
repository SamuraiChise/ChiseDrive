using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ChiseDrive.World
{
    public class Wind
    {
        Vector4 direction;
        public Vector3 Direction
        {
            get
            {
                return new Vector3(direction.X, direction.Y, direction.Z);
            }
            set
            {
                direction.X = value.X;
                direction.Y = value.Y;
                direction.Z = value.Z;
            }
        }
        public float Intensity
        {
            get
            {
                return direction.W;
            }
            set
            {
                direction.W = value;
            }
        }

        public float Percent
        {
            get
            {
                return loop.Percent(elapsed);
            }
        }

        Time loop;
        Time elapsed;

        public Wind(Vector4 direction)
        {
            this.direction = direction;
            this.loop = Time.FromSeconds(100f);
            this.elapsed = Time.Zero;
        }

        public void Update(Time elapsed)
        {
            this.elapsed += elapsed * direction.W;
            if (this.elapsed > this.loop)
            {
                this.elapsed -= this.loop;
            }
        }
    }
}