using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using ChiseDrive.Particles;

namespace ChiseDrive.Units
{
    public class DebrisStreamer : IFollow, IDisposable
    {
        public Vector3 Position 
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
            }
        }
        public Quaternion Rotation
        {
            get
            {
                return rotation;
            }
            set
            {
                rotation = value;
            }
        }
        public Matrix RotationPosition
        {
            get
            {
                return Matrix.CreateFromQuaternion(rotation) * Matrix.CreateTranslation(position);
            }
        }
        public float Scale { get { return 1f; } }
        public Vector3 Velocity
        {
            get
            {
                return velocity;
            }
        }

        ParticleEmitter emitter;
        Quaternion rotation;
        Vector3 position;
        Vector3 velocity;

        public bool IsDone
        {
            get
            {
                return emitter.IsDone;
            }
        }

        public void Reset()
        {
            emitter.Reset();
        }

        public DebrisStreamer(ParticleEmitterSettings particlesettings)
        {
            this.emitter = new ParticleEmitter(this, particlesettings);
        }

        public void Emit(Vector3 position, Vector3 velocity)
        {
            this.position = position;
            this.velocity = velocity;
            this.rotation = Helper.RotateToFaceQuaternion(position, position + velocity, Vector3.Up);
            this.emitter.EmitOnce();
        }

        public void Dispose()
        {
            emitter.Dispose();
        }

        public void Update(Time elapsed)
        {
            position += velocity * elapsed;
            emitter.Update(elapsed);
        }
    }
}