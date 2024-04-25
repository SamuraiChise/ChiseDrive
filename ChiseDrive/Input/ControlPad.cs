using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.GamerServices;

namespace ChiseDrive.Input
{
    public abstract class ControlPad : IInput
    {
        protected enum DirectionControl
        {
            LeftStick,
            RightStick,
            DirectionPad,
        };

        const int MaxButtonHistory = 16;

        Vector2[] History = new Vector2[3];
        float[] ButtonHistory = new float[4];

        protected GamePadState currentState;
        protected GamePadState previousState;
        protected PlayerIndex player;
        protected InstructionStack stack = new InstructionStack();

        public PlayerIndex PlayerIndex
        {
            get
            {
                return player;
            }
        }

        public bool IsConnected
        {
            get
            {
                return GamePad.GetState(PlayerIndex).IsConnected;
            }
        }

        protected ControlPad()
        {
            player = PlayerIndex.One;
            currentState = previousState = GamePad.GetState(player);
        }
        public ControlPad(PlayerIndex assignindex)
        {
            player = assignindex;
            currentState = previousState = GamePad.GetState(player);
        }

        public InstructionStack GetInput()
        {
            currentState = GamePad.GetState(player);

            History[0] *= 0.92f;
            History[1] *= 0.92f;
            History[2] *= 0.92f;

            ButtonHistory[0] += currentState.Buttons.A == ButtonState.Pressed ? 0.2f : 0f;
            ButtonHistory[1] += currentState.Buttons.B == ButtonState.Pressed ? 0.2f : 0f;
            ButtonHistory[2] += currentState.Buttons.X == ButtonState.Pressed ? 0.2f : 0f;
            ButtonHistory[3] += currentState.Buttons.Y == ButtonState.Pressed ? 0.2f : 0f;

            for (int i = 0; i < 4; i++)
            {
                if (ButtonHistory[i] > 1f) ButtonHistory[i] = 1f;
                else ButtonHistory[i] -= 0.1f;
                if (ButtonHistory[i] < 0f) ButtonHistory[i] = 0f;
            }

            Vector2 LStickVector = new Vector2();
            LStickVector.X = currentState.ThumbSticks.Left.X < -0.1f ? -1f :
                currentState.ThumbSticks.Left.X > 0.1f ? 1f : 0f;
            LStickVector.Y = currentState.ThumbSticks.Left.Y < -0.1f ? -1f :
                currentState.ThumbSticks.Left.Y > 0.1f ? 1f : 0f;
            History[0] += (LStickVector * 0.08f);

            Vector2 RStickVector = new Vector2();
            RStickVector.X = currentState.ThumbSticks.Right.X < -0.1f ? -1f :
                currentState.ThumbSticks.Right.X > 0.1f ? 1f : 0f;
            RStickVector.Y = currentState.ThumbSticks.Right.Y < -0.1f ? -1f :
                currentState.ThumbSticks.Right.Y > 0.1f ? 1f : 0f;
            History[1] += (RStickVector * 0.08f);

            Vector2 DPadVector = new Vector2();
            DPadVector.X = currentState.DPad.Left == ButtonState.Pressed ? -1f :
                currentState.DPad.Right == ButtonState.Pressed ? 1f : 0f;
            DPadVector.Y = currentState.DPad.Up == ButtonState.Pressed ? 1f :
                currentState.DPad.Down == ButtonState.Pressed ? -1f : 0f;
            History[2] += (DPadVector * 0.08f);

            InstructionStack retval = ParseInput();
            previousState = currentState;
            return retval;
        }

        /// <summary>
        /// Overwrite this with game specific code.
        /// </summary>
        /// <returns>Processes the controller state into Instructions.</returns>
        protected virtual InstructionStack ParseInput()
        {
            stack.Clear();
            if (IsPressed(Buttons.A)) stack.Add(new Instruction("A"));
            if (IsPressed(Buttons.B)) stack.Add(new Instruction("B"));
            if (IsPressed(Buttons.X)) stack.Add(new Instruction("X"));
            if (IsPressed(Buttons.Y)) stack.Add(new Instruction("Y"));
            if (IsPressed(Buttons.RightShoulder)) stack.Add(new Instruction("RightShoulder"));
            if (IsPressed(Buttons.LeftShoulder)) stack.Add(new Instruction("LeftShoulder"));
            if (IsPressed(Buttons.Start)) stack.Add(new Instruction("Start"));
            if (IsPressed(Buttons.Back)) stack.Add(new Instruction("Back"));
            if (IsPressed(Buttons.DPadUp)) stack.Add(new Instruction("DUp"));
            if (IsPressed(Buttons.DPadLeft)) stack.Add(new Instruction("DLeft"));
            if (IsPressed(Buttons.DPadRight)) stack.Add(new Instruction("DRight"));
            if (IsPressed(Buttons.DPadDown)) stack.Add(new Instruction("DDown"));
            if (IsPressed(Buttons.LeftStick)) stack.Add(new Instruction("LeftStick"));
            if (IsPressed(Buttons.RightStick)) stack.Add(new Instruction("RightStick"));
            
            float dt = currentState.Triggers.Left;
            if (dt != 0) stack.Add(new Instruction("LeftTrigger", new Vector2(dt, 0f)));
            dt = currentState.Triggers.Right;
            if (dt != 0) stack.Add(new Instruction("RightTrigger", new Vector2(dt, 0f)));

            float dx = 0f;
            float dy = 0f;

            dx = currentState.ThumbSticks.Left.X;
            dy = currentState.ThumbSticks.Left.Y;

            if (dx != 0 || dy != 0) stack.Add(new Instruction("LeftStick", new Vector2(dx, dy)));

            dx = currentState.ThumbSticks.Right.X;
            dy = currentState.ThumbSticks.Right.Y;

            if (dx != 0 || dy != 0) stack.Add(new Instruction("RightStick", new Vector2(dx, dy)));

            return stack;
        }

        protected void DirectionPressed(DirectionControl control)
        {
            const float Threshold = 0.4f;

            if (History[(int)control].X > Threshold)
            {
                stack.Add(new Instruction("Right"));
                History[(int)control].X = -Threshold;
            }

            else if (History[(int)control].X < -Threshold)
            {
                stack.Add(new Instruction("Left"));
                History[(int)control].X = Threshold;
            }

            else if (History[(int)control].Y > Threshold)
            {
                stack.Add(new Instruction("Up"));
                History[(int)control].Y = -Threshold;
            }

            else if (History[(int)control].Y < -Threshold)
            {
                stack.Add(new Instruction("Down"));
                History[(int)control].Y = Threshold;
            }
        }

        public float Pressure(Buttons button)
        {
            switch (button)
            {
                case Buttons.A: return ButtonHistory[0];
                case Buttons.B: return ButtonHistory[1];
                case Buttons.X: return ButtonHistory[2];
                case Buttons.Y: return ButtonHistory[3];
                default: return 1f;
            }
        }

        public bool IsDown(Buttons button)
        {
            if (currentState.IsButtonDown(button))
                return true;
            return false;
        }

        public bool IsPressed(Buttons button)
        {
            if (currentState.IsButtonDown(button) && previousState.IsButtonUp(button))
                return true;
            return false;
        }
    }
}
