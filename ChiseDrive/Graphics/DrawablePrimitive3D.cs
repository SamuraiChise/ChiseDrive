using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ChiseDrive.Cameras;

namespace ChiseDrive.Graphics
{
    public class DrawablePrimitive3D : IDisposable
    {
        static List<DrawablePrimitive3D> DrawList = new List<DrawablePrimitive3D>();
        static public void DrawAll(GraphicsDevice device, Camera camera, Visibility visibility)
        {
            if (device == null) throw new NullReferenceException();
            if (camera == null) throw new NullReferenceException();

            if (visibility == Visibility.Opaque)
            {
                device.RenderState.DepthBufferEnable = true;
                device.RenderState.DepthBufferWriteEnable = true;
                device.RenderState.AlphaBlendEnable = true;
                device.RenderState.SourceBlend = Blend.SourceAlpha;
                device.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
            }
            else if (visibility == Visibility.Normal)
            {
                device.RenderState.DepthBufferEnable = true;
                device.RenderState.DepthBufferWriteEnable = true;
                device.RenderState.AlphaBlendEnable = true;
                device.RenderState.SourceBlend = Blend.SourceAlpha;
                device.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
            }
            else
            {
                return;
            }

            foreach (DrawablePrimitive3D drawable in DrawList)
            {
                drawable.Draw(device, camera);
            }
        }

        public bool Visible
        {
            get
            {
                return DrawList.Contains(this);
            }
            set
            {
                if (value && !Visible) DrawList.Add(this);
                if (!value && Visible) DrawList.Remove(this);
            }
        }

        public virtual void Dispose()
        {
            Visible = false;
        }

        public virtual void Draw(GraphicsDevice device, Camera camera)
        {
        }
    }
}