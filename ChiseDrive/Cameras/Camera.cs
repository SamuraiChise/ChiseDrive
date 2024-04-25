using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace ChiseDrive.Cameras
{
    /// <summary>
    /// The base camera used in 3D games.
    /// </summary>
    public class Camera
    {
        #region Values
        protected CameraShot currentshot = CameraShot.Invalid;
        CameraShot initial = CameraShot.Invalid;
        CameraShot final = CameraShot.Invalid;
        Timer totaltransition = new Timer(Time.Zero);
        Timer elapsedtransition = new Timer(Time.Zero);

        float aspectRatio = 1f;
        float near = 1f;
        float far = 40000f;
        Vector2 dimensions = Vector2.Zero;

        Matrix view = Matrix.Identity;
        Matrix projection = Matrix.Identity;
        Matrix rotation = Matrix.Identity;
        Vector3 up = Vector3.Up;

        Vector3 previous;
        #endregion

        #region References
        GraphicsDevice graphics;
        #endregion

        static AudioListener audioListener = new AudioListener();
        public static AudioListener AudioListener { get { return audioListener;} }

        public enum ProjectionStyle
        {
            Perspective,
            Orthographic,
        };
        public ProjectionStyle CameraProjection { get; set; }

        /// <summary>
        /// The direction the camera is facing.
        /// </summary>
        public Matrix View
        {
            get
            {
                return view;
            }
        }

        /// <summary>
        /// Applies lens angle and near/far to create the 
        /// camera projection.
        /// </summary>
        public Matrix Projection
        {
            get
            {
                return projection;
            }
        }

        /// <summary>
        /// Game coordinates for the camera's position.
        /// </summary>
        public Vector3 Position
        {
            get
            {
                return currentshot.Position;
            }
            set
            {
                currentshot.Position = value;
            }
        }

        /// <summary>
        /// Game coordinates for the camera's current target.
        /// </summary>
        public Vector3 LookAt
        {
            get
            {
                return currentshot.Lookat;
            }
            set
            {
                currentshot.Lookat = value;
            }
        }

        /// <summary>
        /// The camera's lense angle in degrees.
        /// </summary>
        public float Zoom
        {
            get
            {
                return currentshot.Lensangle;
            }
            set
            {
                currentshot.Lensangle = value;
            }
        }

        /// <summary>
        /// The direction of Up.
        /// </summary>
        public Vector3 Up
        {
            get
            {
                return up;
            }
            set
            {
                up = value;
            }
        }

        public Vector3 Forward
        {
            get
            {
                if (Position == LookAt) return Vector3.Forward;
                Vector3 forward = Position - LookAt;
                Helper.Normalize(ref forward);
                return forward;
            }
        }

        public Vector3 Velocity
        {
            get
            {
                return Position - previous;
            }
        }

        /// <summary>
        /// Creates a new camera using a GraphicsDevice
        /// </summary>
        /// <param name="graphicsDevice">The Game's GraphicsDevice</param>
        public Camera(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice == null) throw new ArgumentException("Invalid GraphicsDevice.");

            CameraProjection = ProjectionStyle.Perspective;

            this.graphics = graphicsDevice;
            currentshot = CameraShot.Invalid;

            this.SetAspect(1280, 720);
            this.SetNearFar(1f, 40000f);
        }

        /// <summary>
        /// Creates a new camera using an existing Camera
        /// </summary>
        /// <param name="copy">The camera to copy.</param>
        public Camera(Camera copy)
        {
            if (copy == null) throw new ArgumentException("Invalid camera passed.");

            CameraProjection = copy.CameraProjection;

            this.view = copy.View;
            this.projection = copy.Projection;
            this.Up = copy.Up;
            this.currentshot = new CameraShot(copy.currentshot);
            this.graphics = copy.graphics;
        }

        /// <summary>
        /// Sets the aspect ratio from width and height.
        /// </summary>
        /// <param name="width">On Xbox 360 native is 1280.</param>
        /// <param name="height">On Xbox 360 native is 720.</param>
        public void SetAspect(float width, float height)
        {
            if (width <= 0f) throw new ArgumentException("Width needs to be positive.");
            if (height <= 0f) throw new ArgumentException("Height needs to be positive.");

            dimensions.X = width;
            dimensions.Y = height;

            aspectRatio = width / height;
        }

        /// <summary>
        /// The near and far values for the projection.
        /// </summary>
        /// <param name="near">A positive value.</param>
        /// <param name="far">A positive value.</param>
        public void SetNearFar(float near, float far)
        {
            if (near <= 0f) throw new ArgumentException("Near value needs to be positive.");
            if (far <= near) throw new ArgumentException("Far value needs to be greater than near.");

            this.near = near;
            this.far = far;
        }

        /// <summary>
        /// Builds the Camera matrices used by the GraphicsDevice for 
        /// rendering frames.
        /// </summary>
        /// <param name="elapsed"></param>
        public void GenerateCameraMatrixes()
        {
            // Make sure we're not looking at ourself!
            if (currentshot.Position == currentshot.Lookat) currentshot.Lookat = Vector3.Forward;

            // Rotate the camera to find a corrected up vector.
            rotation = Helper.RotateToFace(currentshot.Position, currentshot.Lookat, up);

            view = Matrix.CreateLookAt(currentshot.Position, currentshot.Lookat, rotation.Up);
            
            float lensangle = 1f;

            if (currentshot != CameraShot.Invalid) lensangle = MathHelper.ToRadians(currentshot.Lensangle);

            switch (CameraProjection)
            {
                case ProjectionStyle.Perspective: 
                    projection = Matrix.CreatePerspectiveFieldOfView(lensangle, aspectRatio, near, far);
                    break;
                case ProjectionStyle.Orthographic:
                    projection = Matrix.CreateOrthographic(dimensions.X, dimensions.Y, near, far);
                    break;
            }
        }

        /// <summary>
        /// Reflects all the camera matrices off a plane to allow 
        /// rendering of reflections.
        /// </summary>
        /// <param name="plane">A plane to reflect the camera matrices by.</param>
        /// <returns>A new reflected camera.</returns>
        public Camera CreateReflectionCamera(Plane plane)
        {
            Camera reflection = new Camera(this);

            Vector3 planescaled = plane.Normal * plane.D;

            Vector3 deltaposition = Position * plane.Normal;
            Vector3 deltalookat = LookAt * plane.Normal;

            deltaposition += deltaposition;
            deltalookat += deltalookat;

            deltaposition += planescaled;
            deltalookat += planescaled;

            reflection.Position = Position + deltaposition;
            reflection.LookAt = LookAt + deltalookat;

            Vector3 right = Vector3.Transform(Vector3.Right, rotation);
            Vector3 forward = reflection.LookAt - reflection.Position;
            if (forward == Vector3.Zero) forward = Vector3.Forward;
            reflection.Up = Vector3.Cross(right, forward);

            reflection.GenerateCameraMatrixes();
            return reflection;
        }

        public Camera CreateZoomedCamera(float zoom)
        {
            Camera zoomcam = new Camera(this);
            zoomcam.currentshot = new CameraShot(zoomcam.Position, zoomcam.LookAt, zoom);
            return zoomcam;
        }

        /// <summary>
        /// Translates a game position (3D) to a screen position (2D).
        /// </summary>
        /// <param name="gameposition">A 3D position in game coordinates.</param>
        /// <returns>A 2D position in screen coordinates.  May be off screen.</returns>
        public Vector2 GameToScreen(Vector3 gameposition)
        {
            Vector2 screencoords = Vector2.Zero;
            Vector3 project = graphics.Viewport.Project(gameposition, Projection, View, Matrix.Identity);
            screencoords.X = project.X;
            screencoords.Y = project.Y;
            return screencoords;
        }

        /// <summary>
        /// Begins a new camera move.
        /// </summary>
        /// <param name="destination">The final shot destination.</param>
        /// <param name="transition">The time the transition takes.</param>
        public void ShotTransition(CameraShot destination, Time transition)
        {
            this.elapsedtransition.Set(Time.Zero);
            this.totaltransition.Set(transition);

            if (transition.IsZero)
            {
                this.final = destination;
                this.currentshot = destination;
                this.initial = destination;
            }
            else
            {
                this.final = destination;
                this.initial = currentshot;
            }
        }

        /// <summary>
        /// Updates the camera.
        /// </summary>
        /// <param name="elapsed">The elapsed time to apply.</param>
        public virtual void Update(Time elapsed)
        {
            previous = Position;
            if (!totaltransition.IsZero)
            {
                elapsedtransition.AddTime(elapsed);

                // Divide the elapsed time by the total time to find what percentage of frames have elapsed
                float percent = (elapsedtransition / totaltransition).Remaining;

                if (percent > 1f)
                {
                    percent = 1f;
                    totaltransition.Set(Time.Zero);
                }

                currentshot = CameraShot.SmoothStep(initial, final, percent);

                UpdateListener();
            }
        }

        /// <summary>
        /// The listener is automatically updated every time a camera is updated.
        /// If you wish to override this automatic update, then after all cameras are updated,
        /// call this method explicitly from the desired listening camera.
        /// </summary>
        public void UpdateListener()
        {
            audioListener.Position = currentshot.Position;
            audioListener.Up = Up;
            audioListener.Forward = Forward;
        }

        public override string ToString()
        {
            return "Position: " + Position + " Target: " + LookAt;
        }
    }
}
