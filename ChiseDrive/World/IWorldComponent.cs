using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#if !Xbox
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
#endif
using ChiseDrive.Cameras;
using ChiseDrive.Graphics;

namespace ChiseDrive.World
{
    public interface IWorldComponent : IDisposable
    {
        bool Visible { get; set; }

        void Initialize(ChiseDriveGame game);

        /// <summary>
        /// If a World Component does not need to modify height, return the initial
        /// </summary>
        /// <param name="initial">The vector to modify</param>
        /// <returns>A modified height or the initial</returns>
        Vector3 CorrectForHeight(Vector3 initial);
        Vector3 CorrectForBounds(Vector3 initial);
        void SizeBounds(ref Rectangle rectangle);

        void Update(Time elapsed);

#if !Xbox
        void Write(ContentWriter output);
#endif
        void Read(ContentReader input);
    }
}
