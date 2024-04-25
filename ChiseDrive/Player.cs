using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using ChiseDrive.Units;

namespace ChiseDrive
{
    /// <summary>
    /// Inherit AI or User players on top of this abstract class.
    /// </summary>
    abstract public class Player : IDisposable
    {
        protected ChiseDriveGame Game;
        public Player(ChiseDriveGame game)
        {
            Game = game;
        }
        public abstract void Dispose();
        public abstract void Update(Time elapsed);
        public abstract void Draw();
        public abstract void RecieveEvent(UnitEvent e);
    }
}