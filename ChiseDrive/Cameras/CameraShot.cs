using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ChiseDrive.Cameras
{
    /// <summary>
    /// CameraShots are created as structs so they may pass by 
    /// value and avoid generating garbage.
    /// </summary>
    public struct CameraShot
    {
        public static readonly CameraShot Invalid = 
            new CameraShot(Vector3.Zero, Vector3.Zero, -1f);

        // *************** Convenient Default Zooms
        public const float CloseZoom = 25f;
        public const float StandardZoom = 45f;
        public const float WideZoom = 65f;

        // Lens settings
        public float Lensangle;

        // Location
        public Vector3 Position;
        public Vector3 Lookat;

        /// <summary>
        /// Creates a new CameraShot.
        /// </summary>
        /// <param name="position">A game world position.</param>
        /// <param name="lookat">A game world position to look at.</param>
        /// <param name="zoom">The zoom setting in degrees.</param>
        public CameraShot(Vector3 position, Vector3 lookat, float zoom)
        {
            Helper.TestValidity(lookat);
            Helper.TestValidity(position);

            Lensangle = zoom;
            Position = position;
            Lookat = lookat;
        }

        /// <summary>
        /// Copies a camera shot.
        /// </summary>
        /// <param name="copy">The shot to copy.</param>
        public CameraShot(CameraShot copy)
        {
            Lensangle = copy.Lensangle;
            Position = copy.Position;
            Lookat = copy.Lookat;
        }

        public static bool operator ==(CameraShot lhs, CameraShot rhs)
        {
            return lhs.Lensangle == rhs.Lensangle
                && lhs.Position == rhs.Position
                && lhs.Lookat == rhs.Lookat;
        }

        public static bool operator !=(CameraShot lhs, CameraShot rhs)
        {
            return lhs.Lensangle != rhs.Lensangle
                && lhs.Position != rhs.Position
                && lhs.Lookat != rhs.Lookat;
        }

        /// <summary>
        /// Applies a cubic smooth step between two CameraShots.
        /// </summary>
        /// <param name="initial">Initial Shot.</param>
        /// <param name="final">Final Shot.</param>
        /// <param name="percent">Percentage of Transition.</param>
        /// <returns>The generated camera shot.</returns>
        public static CameraShot Lerp
            (CameraShot initial, CameraShot final, float percent)
        {
            // Test for CameraShot.Invalid
            if (initial == CameraShot.Invalid 
                && final == CameraShot.Invalid) return new CameraShot();
            else if (initial == CameraShot.Invalid) return final;
            else if (final == CameraShot.Invalid) return initial;

            CameraShot temp = new CameraShot();

            // Find the lens delta
            float lensdelta = final.Lensangle - initial.Lensangle;

            // Scale the delta
            lensdelta *= percent;

            // Apply it
            temp.Lensangle = initial.Lensangle + lensdelta;

            // Vector3 has nice handy built in Lerps.
            temp.Lookat = Vector3.Lerp(initial.Lookat, 
                final.Lookat, percent);
            temp.Position = Vector3.Lerp(initial.Position, 
                final.Position, percent);

            return temp;
        }

        /// <summary>
        /// Applies a cubic smooth step between two CameraShots.
        /// </summary>
        /// <param name="initial">Initial Shot.</param>
        /// <param name="final">Final Shot.</param>
        /// <param name="percent">Percentage of Transition.</param>
        /// <returns>The generated camera shot.</returns>
        public static CameraShot SmoothStep
            (CameraShot initial, CameraShot final, float percent)
        {
            // Test for CameraShot.Invalid
            if (initial == CameraShot.Invalid 
                && final == CameraShot.Invalid) return new CameraShot();
            else if (initial == CameraShot.Invalid) return final;
            else if (final == CameraShot.Invalid) return initial;

            CameraShot temp = new CameraShot();

            // Use the nice Helper Cubic smooth step function
            temp.Lensangle = Helper.SmoothStep(initial.Lensangle, 
                final.Lensangle, percent);

            // Vector3 conveniently has built in smooth steps.
            temp.Lookat = Vector3.SmoothStep(initial.Lookat, 
                final.Lookat, percent);
            temp.Position = Vector3.SmoothStep(initial.Position, 
                final.Position, percent);

            return temp;
        }
    }
}