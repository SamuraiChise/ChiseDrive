using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
#if !Xbox
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
#endif
using ChiseDrive.Graphics;
using ChiseDrive.Units;

namespace ChiseDrive.World
{
    public class SceneObject : IWorldComponent
    {
        LitObject Body;

        public bool Visible
        {
            get
            {
                return Body.Visible;
            }
            set
            {
                Body.Visible = value;
            }
        }

        Quaternion rotation;
        Vector3 position;
        BodySettings settings;

        public Quaternion Rotation
        {
            get
            {
                return rotation;
            }
            set
            {
                rotation = value;
                Body.RotationPosition = Matrix.CreateFromQuaternion(Rotation) * Matrix.CreateTranslation(Position);
            }
        }

        public Vector3 Position 
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
                Body.RotationPosition = Matrix.CreateFromQuaternion(Rotation) * Matrix.CreateTranslation(Position);
            }
        }

        public Vector3 CorrectForHeight(Vector3 initial) { return initial; }
        public Vector3 CorrectForBounds(Vector3 initial) { return GameWorld.InvalidPosition; }
        public void SizeBounds(ref Rectangle rectangle) { }

        public void Initialize(ChiseDriveGame game)
        {
            this.Body = settings.BuildLitObject(game);
            this.Body.RotationPosition = Matrix.CreateFromQuaternion(rotation) * Matrix.CreateTranslation(position);
            this.Body.CompactAndCountLights();
            this.Visible = true;
        }

        public SceneObject(ChiseDriveGame game, BodySettings settings, Vector3 position)
        {
            this.position = position;
            this.rotation = Quaternion.CreateFromAxisAngle(Vector3.Backward, 0f);
            this.settings = settings;

            Initialize(game);
        }

        public SceneObject(BodySettings settings, Vector3 position, Vector3 rotation)
        {
            this.settings = settings;
            this.position = position;
            this.rotation = Quaternion.CreateFromYawPitchRoll(rotation.X, rotation.Y, rotation.Z);
        }

        public SceneObject()
        {
            this.settings = new BodySettings();
        }

        public void Dispose()
        {
            Body.Dispose();
        }

        public void Update(Time elapsed)
        {
        }

#if !Xbox
        public void Write(ContentWriter output)
        {
            settings.Write(output);
            output.Write(position);
            output.Write(rotation);
        }
#endif
        public void Read(ContentReader input)
        {
            settings.Read(input);
            position = input.ReadVector3();
            rotation = input.ReadQuaternion();
        }
    }
}