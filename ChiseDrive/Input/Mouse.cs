using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ChiseDrive.Input
{
    abstract class MouseReader : IInput
    {
        public InstructionStack GetInput()
        {
            currentMouse = Mouse.GetState();
            InstructionStack retstack = ParseInput();
            previousMouse = currentMouse;
            return retstack;
        }
        protected MouseState currentMouse;
        protected MouseState previousMouse;

        protected float DX { get { return previousMouse.X - currentMouse.X; } }
        protected float DY { get { return previousMouse.Y - currentMouse.Y; } }

        public MouseReader()
        {
            currentMouse = previousMouse = Mouse.GetState();
        }

        protected virtual InstructionStack ParseInput()
        {
            return new InstructionStack();
        }

        /// <summary>
        /// Calls to CheckCommand will clear the queued mouse commands.
        /// TODO: Make the queued commands clear after they return so multiple commands
        /// can be stacked together.
        /// </summary>
        /// <returns>Pops a command off the waiting stack.</returns>
        //public abstract InstructionStack ParseInput();

        protected bool IsPressed(ButtonState current, ButtonState previous)
        {
            if (current == ButtonState.Released && previous == ButtonState.Pressed)
            {
                return true;
            }
            return false;
        }

        protected bool IsDown(ButtonState current)
        {
            if (current == ButtonState.Pressed)
            {
                return true;
            }
            return false;
        }
    }
}
