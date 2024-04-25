using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ChiseDrive.Physics;
using ChiseDrive.Units;

namespace ChiseDrive.Cameras
{
    /// <summary>
    /// A holder for all things camera related.  May or may not
    /// contain sub-camera components.
    /// </summary>
    public class AdvancedCamera : Camera
    {
        #region Fields

        Vector3 position = Vector3.Zero;
        Quaternion rotation = Quaternion.Identity;

        // This is where the camera WANTS to be, but it can't always be there
        Vector3 destination = Vector3.Zero;

        // This is what the camera WANTS to be looking at
        Vector3 focus = Vector3.Zero;

        // This is what the camera ACTUALLY is looking at
        Vector3 lookat = Vector3.Zero;

        // When "bumped" the up will shift as the camera recovers
        Vector3 up = Vector3.Up;
        Vector3 masterup = Vector3.Up;
        Vector3 masterleft = Vector3.Left;

        protected BoundingFrustum frustum;

        Vector3 offset = Vector3.Zero;
        //float destinationzoom = 0f;
        OperatorError zoomoperator = new OperatorError(0f, 0.005f, 0.03f, 2, 0.05f, Time.FromSeconds(6f), Time.FromSeconds(1f));
        OperatorError lookatxoperator = new OperatorError(0f, 4f, 0f, 3, 0.1f, Time.FromSeconds(6.5f), Time.FromSeconds(0f));
        OperatorError lookatyoperator = new OperatorError(0f, 4f, 0f, 3, 0.1f, Time.FromSeconds(5.0f), Time.FromSeconds(0f));
        OperatorError lookatzoperator = new OperatorError(0f, 4f, 0f, 3, 0.1f, Time.FromSeconds(7.0f), Time.FromSeconds(0f));
        OperatorError positionxoperator = new OperatorError(0f, 1f, 1f, 0, 0.5f, Time.FromSeconds(6.5f), Time.FromSeconds(0f));
        OperatorError positionyoperator = new OperatorError(0f, 1f, 1f, 0, 0.5f, Time.FromSeconds(5.0f), Time.FromSeconds(0f));
        OperatorError positionzoperator = new OperatorError(0f, 1f, 1f, 0, 0.5f, Time.FromSeconds(7.0f), Time.FromSeconds(0f));

        // The camera can be on follow for an object
        protected IFollow target = null;

        // This is the object the camera is going to follow around
        protected IFollow follow = null;

        protected IPhysics physics = new BasicPhysics();

        #endregion

        #region Properties
        public virtual Vector3 Offset
        {
            get
            {
                return offset;
            }
            set
            {
                offset = value;
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
        public float Scale
        {
            get
            {
                return 1f;
            }
            set
            {
            }
        }/*
        public float InstantZoom
        {
            get
            {
                return MathHelper.ToDegrees(lensangle);
            }
            set
            {
                float degrees = value;
                if (degrees < 1.0f) degrees = 1.0f;
                if (degrees > 89.0f) degrees = 89.0f;
                lensangle = MathHelper.ToRadians(degrees);
                GenerateCameraMatrixes();
            }
        }
        public float Zoom
        {
            get
            {
                return MathHelper.ToDegrees(destinationzoom);
            }
            set
            {
                destinationzoom = MathHelper.ToRadians(value);
            }
        }*/

        #endregion

        /// <summary>
        /// After building a new Camera, call SetAspect, SetFocus and SetFollow!
        /// </summary>
        public AdvancedCamera(GraphicsDevice graphics) : base(graphics)
        {
            #region IPhysics tuning
            physics.TerminalVelocity = 20f;
            physics.Mass = 10.0f;
            physics.DecelerationRate = 50f;
            #endregion

            Offset = new Vector3(15.0f, 15.0f, 30.0f);
        }

        public ContainmentType OnScreen(Vector3 gamepoint)
        {
            return frustum.Contains(gamepoint);
        }



        /// <summary>
        /// The active focus of the camera's composition (or what the camera is trying to look at).
        /// </summary>
        /// <param name="target">Will track a moving object.</param>
        public void SetFocus(IFollow target)
        {
            this.target = target;
            this.focus = target.Position;
        }

        /// <summary>
        /// The active focus of the camera's composition (or what the camera is trying to look at).
        /// </summary>
        /// <param name="target">For turning to look at a stationary point.</param>
        public void SetFocus(Vector3 target)
        {
            this.target = null;
            this.focus = target;
        }

        /// <summary>
        /// This is what the Camera is anchored to in the scene.
        /// </summary>
        /// <param name="target">Will follow the object around on the X,Y,Z plane.</param>
        public void SetFollow(IFollow target)
        {
            this.follow = target;
        }

        public void ForceLookAt(Vector3 position)
        {
            lookat = position;
            lookatxoperator.Reset(position.X);
            lookatyoperator.Reset(position.Y);
            lookatzoperator.Reset(position.Z);
        }

        public void ApplyCameraShake(Vector3 force)
        {
            physics.Push(force);
        }

        public void ApplyRandomCameraShake(float scale)
        {
            Vector3 newshake = Vector3.One;
            newshake.X = 1f - (2f * Helper.Randomf);
            newshake.Y = 1f - (2f * Helper.Randomf);
            newshake.Z = 1f - (2f * Helper.Randomf);
            newshake *= scale;
            ApplyCameraShake(newshake);
        }

        /// <summary>
        /// The camera does it's best to get from it's current position/focus
        /// to it's desired destination/lookat.
        /// </summary>
        private void DoMove(Time elapsed)
        {
            up = masterup;

            positionxoperator.SetSpeed(position.X, destination.X, 10f);
            positionyoperator.SetSpeed(position.Y, destination.Y, 10f);
            positionzoperator.SetSpeed(position.Z, destination.Z, 10f);

            position.X = positionxoperator.MoveTo(destination.X, elapsed);
            position.Y = positionyoperator.MoveTo(destination.Y, elapsed);
            position.Z = positionzoperator.MoveTo(destination.Z, elapsed);

            position = physics.RunPhysics(elapsed, position);
        }

        /// <summary>
        /// The camera changes it's destination as the follow/focus objects move around. 
        /// </summary>
        private void DoFollow()
        {
            if (follow == null) return;
            float scale = follow.Scale;

            // Build a set of vectors to shift the camera by...
            Vector3 rightshift = Vector3.Right * Offset.X * scale;
            Vector3 upshift = Vector3.Up * Offset.Y * scale;
            Vector3 backshift = Vector3.Backward * Offset.Z * scale;

            // ...and composite them together
            Vector3 shift = rightshift + upshift + backshift;

            // Find our rotations to look from one object to the other
            Vector3 followposition = Vector3.One;
            Vector3 targetposition = Vector3.One;

            if (follow != null) followposition = follow.Position;
            if (target != null) targetposition = target.Position;

            Matrix followtotarget = Helper.RotateToFace(followposition, targetposition, Vector3.Up);

            // Store the rotation information of our root object
            Vector3 rotatedshift = Vector3.Transform(shift, followtotarget);

            // Take our follow position and move the camera by it's rotated and shifted position.
            destination = followposition - rotatedshift;

            //SolGame.DebugText("Camera", Position);
        }

        /// <summary>
        /// TODO: this should use focus to determine where the point of reference is.
        /// </summary>
        private void DoLookAt(Time elapsed)
        {
            if (target != null)
            {
                focus = target.Position;
            }
            else if (follow != null)
            {
                focus = follow.Position;
            }
            else
            {
                focus = Vector3.Zero;
            }

            lookatxoperator.SetSpeed(lookat.X, focus.X, 2f);
            lookatyoperator.SetSpeed(lookat.Y, focus.Y, 2f);
            lookatzoperator.SetSpeed(lookat.Z, focus.Z, 2f);

            lookat.X = lookatxoperator.MoveTo(focus.X, elapsed);
            lookat.Y = lookatyoperator.MoveTo(focus.Y, elapsed);
            lookat.Z = lookatzoperator.MoveTo(focus.Z, elapsed);
        }

        private void DoZoom(Time elapsed)
        {
            //lensangle = zoomoperator.MoveTo(destinationzoom, elapsed);
            //Helper.Clamp(ref lensangle, 0.0001f, 179f);
        }

        public override void Update(Time elapsed)
        {
            DoZoom(elapsed);
            DoLookAt(elapsed);
            DoFollow();
            DoMove(elapsed);
            //SolGame.DebugText("Camera Position: ", Position);
            //SolGame.DebugText("Camera Lookat: ", lookat);
        }


    }
}
