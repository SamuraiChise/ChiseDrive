using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ChiseDrive.Input
{
    abstract class KeyboardReader : IInput
    {
        KeyboardState currentKeyboard;
        KeyboardState previousKeyboard;

        public KeyboardReader()
        {
            currentKeyboard = previousKeyboard = Keyboard.GetState();
        }

        public InstructionStack GetInput()
        {
            currentKeyboard = Keyboard.GetState();
            InstructionStack retstack = ParseInput();
            previousKeyboard = currentKeyboard;
            return retstack;
        }

        protected virtual InstructionStack ParseInput()
        {
            return new InstructionStack();
        }

        protected bool IsPressed(Keys key)
        {
            if (currentKeyboard.IsKeyUp(key) && previousKeyboard.IsKeyDown(key))
            {
                return true;
            }
            return false;
        }

        protected bool IsDown(Keys key)
        {
            if (currentKeyboard.IsKeyDown(key))
            {
                return true;
            }
            return false;
        }
    }
}
