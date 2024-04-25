using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using ChiseDrive.Graphics;
using ChiseDrive.Motion;

namespace ChiseDrive.Units
{
    public class AttachmentPoint
    {
        public const int MaxAttachments = 30;
        static public List<AttachmentPoint> BuildList(LitObject model, IFollow parent)
        {
            List<AttachmentPoint> attachments = new List<AttachmentPoint>();
            List<String> names = model.GetBoneNames();

            if (names != null)
            {
                foreach (String name in names)
                {
                    attachments.Add(new AttachmentPoint(name, model, parent));
                }
            }
            return attachments;
        }
        static public List<AttachmentPoint> BuildList(LitObject model, IFollow parent, String search)
        {
            List<AttachmentPoint> list = BuildList(model, parent);
            return TrimList(search, list);
        }
        static public List<AttachmentPoint> TrimList(
            String boneName,
            List<AttachmentPoint> attachments)
        {
            return TrimList(boneName, attachments, MaxAttachments);
        }
        static public List<AttachmentPoint> TrimList(
            String boneName,
            List<AttachmentPoint> attachments,
            int maxAttachments)
        {
            List<AttachmentPoint> retval = new List<AttachmentPoint>();

            foreach (AttachmentPoint attachment in attachments)
            {
                if (String.Compare(boneName, 0, attachment.Name, 0, boneName.Length) == 0)
                {
                    retval.Add(attachment);
                }
            }
            /*
            for (int i = 0; i < maxAttachments; i++)
            {
                for (int j = 0; j < attachments.Count; j++)
                {
                    if (attachments[j].Name == boneName + i)
                    {
                        retval.Add(attachments[j]);
                    }
                }
            }*/
            if (retval.Count == 0) throw new Exception("There were no bones found matching the name " + boneName);
            return retval;
        }

        string name;
        Matrix offset;
        LitObject model;
        IFollow parent;

        public String Name
        {
            get
            {
                return name;
            }
        }

        public Matrix Offset
        {
            get
            {
                return offset * model.RotationPosition;
            }
        }

        public Vector3 Velocity
        {
            get 
            {
                return parent.Velocity;
            }
        }

        public AttachmentPoint(String name, LitObject model, IFollow parent)
        {
            this.name = name;
            this.model = model;
            this.parent = parent;
            Refresh();
        }

        public void Refresh()
        {
            offset = model.GetBoneMatrix(name);
        }

        public void Dispose()
        {
            name = null;
            model = null;
        }
    }
}
