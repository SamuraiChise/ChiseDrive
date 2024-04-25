using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace ChiseDrive.Units
{
    public class Addon : IDisposable, IFollow
    {
        protected AttachmentPoint root;
        string name;

        public Matrix Attachment
        {
            get
            {
                return root.Offset;
            }
        }

        public string AttachmentName
        {
            get
            {
                return name;
            }
        }

        #region IFollow
        public Vector3 Position 
        {
            get
            {
                return Vector3.Transform(Vector3.Zero, Attachment);
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public Quaternion Rotation 
        {
            get
            {
                return Quaternion.CreateFromRotationMatrix(Attachment);
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public Matrix RotationPosition
        {
            get
            {
                return root.Offset;
            }
        }
        public float Scale
        {
            get
            {
                return Attachment.Determinant();
            }
        }
        public Vector3 Velocity
        {
            get
            {
                return root.Velocity;
            }
        }
        #endregion

        public virtual bool Visible
        {
            get
            {
                return false;
            }
            set
            {

            }
        }

        public Addon(AttachmentPoint root)
        {
            this.root = root;
            this.name = root.Name;
        }

        public Addon(AddonSettings settings)
        {
            this.root = null;
            this.name = settings.ParentBoneName;
        }

        public virtual Addon Clone(AttachmentPoint root)
        {
            return new Addon(root);
        }

        public virtual void Dispose() { root = null; }
        public virtual void Update(Time elapsed) 
        {
            if (root == null) throw new NotImplementedException("This addon has not been attached to an object.");
            root.Refresh();
        }
    }
}
