using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ChiseDrive.Graphics;
using ChiseDrive.Motion;
using Microsoft.Xna.Framework;

namespace ChiseDrive.Units
{
    /// <summary>
    /// This is a passthrough Addon.  It has a physical body, but passes
    /// another set of systems through to the Unit
    /// </summary>
    public class PhysicalAddon : Addon
    {
        LitObject body = null;

        List<AttachmentPoint> attachments = new List<AttachmentPoint>();
        public List<AttachmentPoint> AttachmentPoints
        {
            get
            {
                return attachments;
            }
        }
        public virtual void BuildAttachments()
        {
            if (body != null)
            {
                List<String> names = body.GetBoneNames();

                if (names != null)
                {
                    foreach (String name in names)
                    {
                        attachments.Add(new AttachmentPoint(name, body, this));
                    }
                }
            }
        }
        public LitObject Body
        {
            get
            {
                return body;
            }
        }

        float spin;
        bool dospin = false;
        public const float FastSpin = 0.1f;
        public const float FastAcceleration = 0.01f;
        public const float SlowAcceleration = 0.001f;

        float spindest = 0f;
        float spinrate = 0f;
        float spinaccel = 0f;

        public float SpinRate
        {
            set
            {
                spindest = value;
            }
        }
        public float SpinAcceleration
        {
            set
            {
                spinaccel = value;
            }
        }

        public override bool Visible
        {
            get
            {
                return body.Visible;
            }
            set
            {
                body.Visible = value;
            }
        }

        public PhysicalAddon(LitObject body, AttachmentPoint root)
            : base(root)
        {
            this.body = body;
            BuildAttachments();
        }

        public PhysicalAddon(ChiseDriveGame game, AddonSettings settings)
            : base(settings)
        {
            this.body = settings.AddonBody.BuildLitObject(game);
        }

        public override void Dispose()
        {
            this.body.Dispose();
            base.Dispose();
        }

        public override Addon Clone(AttachmentPoint root)
        {
            return new PhysicalAddon(body.Clone(), root);
        }

        public override void Update(Time elapsed)
        {
            base.Update(elapsed);

            Helper.EaseTo(ref spinrate, spindest, spinaccel, elapsed);

            spin += spinrate;
            Helper.Radianize(ref spin);

            //body.PositionRotation = PositionRotation;

            Matrix roll = Matrix.CreateFromYawPitchRoll(spin, 0f, 0f);
            body.RotationPosition = roll * RotationPosition;
        }
    }
}