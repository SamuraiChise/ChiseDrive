using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using ChiseDrive.Input;

namespace ChiseDrive.Menu
{
#if Xbox
    class MenuInput : ControlPad
    {
        public MenuInput(PlayerIndex index)
        {
            player = index;
        }

        protected override InstructionStack ParseInput()
        {
            stack.Clear();

            DirectionPressed(DirectionControl.DirectionPad);
            DirectionPressed(DirectionControl.LeftStick);
            DirectionPressed(DirectionControl.RightStick);

            if (IsPressed(Buttons.A)) stack.Add(new Instruction("A"));
            if (IsPressed(Buttons.B)) stack.Add(new Instruction("B"));
            if (IsPressed(Buttons.X)) stack.Add(new Instruction("X"));
            if (IsPressed(Buttons.Y)) stack.Add(new Instruction("Y"));

            if (IsPressed(Buttons.Start)) stack.Add(new Instruction("Start"));

            return stack;
        }
    }
#else
    class MenuKeyboard : KeyboardReader
    {
        protected override InstructionStack ParseInput()
        {
            InstructionStack stack = new InstructionStack();

            if (IsPressed(Keys.X)) stack.Add(new Instruction("X"));
            if (IsPressed(Keys.Y)) stack.Add(new Instruction("Y"));
            if (IsPressed(Keys.B)) stack.Add(new Instruction("B"));
            if (IsPressed(Keys.A)) stack.Add(new Instruction("A"));

            if (IsPressed(Keys.Enter)) stack.Add(new Instruction("Start"));
            if (IsPressed(Keys.Back)) stack.Add(new Instruction("Back"));

            if (IsPressed(Keys.Left)) stack.Add(new Instruction("Left"));
            if (IsPressed(Keys.Right)) stack.Add(new Instruction("Right"));
            if (IsPressed(Keys.Up)) stack.Add(new Instruction("Up"));
            if (IsPressed(Keys.Down)) stack.Add(new Instruction("Down"));

            return stack;
        }
    }

    class MenuMouse : MouseReader
    {
        protected override InstructionStack ParseInput()
        {
            return new InstructionStack();
        }
    }

    class MenuInput : IInput
    {
        public InstructionStack GetInput() { return new InstructionStack(); }
    
        public PlayerIndex PlayerIndex
        {
            get
            {
                return PlayerIndex.One;
            }
        }

        MenuKeyboard keyboard;
        MenuMouse mouse;

        public MenuInput(PlayerIndex index)
        {
            // PlayerIndex is just to keep down the number of #if Xbox
            keyboard = new MenuKeyboard();
            mouse = new MenuMouse();
        }

        public InstructionStack ParseInput()
        {
            InstructionStack stack = new InstructionStack();

            stack.Add(keyboard.GetInput());
            stack.Add(mouse.GetInput());

            return stack;
        }
    }
#endif
}