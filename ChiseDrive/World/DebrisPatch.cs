using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using ChiseDrive.Graphics.InstancedModel;

namespace ChiseDrive.World
{
    class DebrisPatch
    {
        public const float PatchScale = 3000f;
        public const int PatchDensity = 5000;
        const float DebrisScale = 0.4f;

        public struct Debris
        {
            public float size;
            public Vector3 offset;
            public Vector3 rotation;

            public void BuildTransform(Vector3 neworigin, out Matrix transform)
            {
                Matrix scale, rotation, translation;

                scale = Matrix.CreateScale(this.size);
                rotation = Matrix.CreateFromYawPitchRoll(this.rotation.X,
                    this.rotation.Y, this.rotation.Z);
                translation = Matrix.CreateTranslation(neworigin + this.offset);

                transform = scale * rotation * translation;
            }
        }

        Debris[] data;
        Vector3 origin;
        public Vector3 Origin
        {
            get
            {
                return origin;
            }
            set
            {
                origin = value;
            }
        }
        public Debris[] Data
        {
            get
            {
                return data;
            }
        }

        public DebrisPatch()
        {
            data = new Debris[PatchDensity];

            for (int i = 0; i < PatchDensity; i++)
            {
                data[i] = new Debris();
                data[i].size = (2.5f + Helper.Randomf) * DebrisScale;
                data[i].offset = Helper.RandomDisplacement(Vector3.Zero, PatchScale);
                data[i].rotation.X = Helper.Randomf * Helper.PI2;
                data[i].rotation.Y = Helper.Randomf * Helper.PI2;
                data[i].rotation.Z = Helper.Randomf * Helper.PI2;
            }
            origin = new Vector3(0f, 0f, 0f);
        }
        public DebrisPatch(DebrisPatch copy)
        {
            data = new Debris[PatchDensity];

            for (int i = 0; i < PatchDensity; i++)
            {
                data[i] = new Debris();
                data[i].size = copy.data[i].size;
                data[i].offset = copy.data[i].offset;
                data[i].rotation = copy.data[i].rotation;
            }
            origin = new Vector3(1000000f, 1000000f, 1000000f);
        }
    }
}