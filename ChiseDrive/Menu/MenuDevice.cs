using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using ChiseDrive;
using ChiseDrive.Input;
using ChiseDrive.Graphics;

namespace ChiseDrive.Menu
{
    public class MenuDevice : DrawableGameComponent
    {
        public MenuScreen ActiveScreen
        {
            get
            {
                return activeScreen;
            }
        }
        public PlayerIndex ActiveController { get; set; }
        MenuInput[] inputs;
        MenuScreen activeScreen;
        MenuScreen waitingScreen;

        InstructionStack waitingInstructions = new InstructionStack();
        InstructionStack userInput = new InstructionStack();

        List<MenuCommand> menuCommands = new List<MenuCommand>(10);

        SpriteBatch sprites;

        const string One = "PlayerIndexOne";
        const string Two = "PlayerIndexTwo";
        const string Three = "PlayerIndexThree";
        const string Four = "PlayerIndexFour";

        /// <summary>
        /// Builds a new MenuDevice.  While it is possible to run multiple
        /// simultaneous MenuDevices, the results are undefined.
        /// </summary>
        /// <param name="game">An XNA game to reference.</param>
        public MenuDevice(Game game) :
            base(game)
        {
            inputs = new MenuInput[4];
            for (int i = 0; i < inputs.Length; i++)
            {
                inputs[i] = new MenuInput((Microsoft.Xna.Framework.PlayerIndex)i);
            }
            activeScreen = null;

            ActiveController = PlayerIndex.One;

            sprites = new SpriteBatch(Game.GraphicsDevice);

            base.UpdateOrder = ChiseDrive.UpdateOrder.Menu;
            base.DrawOrder = ChiseDrive.DrawOrder.Menu;
            Game.Components.Add(this);
        }

        /// <summary>
        /// Runs a new MenuScreen.  Will close a current screen before running the new one.
        /// </summary>
        /// <param name="filename">The .xml filename.</param>
        public void OpenScreen(String filename)
        {
            this.Enabled = true;

            if (activeScreen == null || activeScreen.IsClosed())
            {
                activeScreen = Game.Content.Load<MenuScreen>(filename);
                activeScreen.Open(Game.Content);
                waitingScreen = null;
            }
            else
            {
                waitingScreen = Game.Content.Load<MenuScreen>(filename);
                activeScreen.Close();
            }
        }

        /// <summary>
        /// Closes any active screens.
        /// </summary>
        public void CloseScreen()
        {
            if (activeScreen != null) activeScreen.Close();
        }

        /// <summary>
        /// Checks the menu device for waiting game instructions.
        /// The instruction stack returned may be an empty stack.
        /// </summary>
        /// <returns>A stack of waiting instructions.  May be empty.</returns>
        public InstructionStack GetWaitingInstructions()
        {
            return waitingInstructions;
        }

        public override void Update(GameTime gameTime)
        {
            if (activeScreen == null)
            {
                // Looking to start the waiting screen
                if (waitingScreen != null)
                {
                    activeScreen = waitingScreen;
                    waitingScreen = null;
                    activeScreen.Open(Game.Content);
                }
            }
            else if (activeScreen.IsClosed())
            {
                // Looking to dump the active screen
                activeScreen = null;

                if (waitingScreen == null)
                {
                    waitingInstructions.Add(new Instruction("NoScreen"));
                }
            }

            if (activeScreen == null)
            {
                // Skip anything that relies on an active screen
                base.Update(gameTime);
                this.Enabled = false;
                return;
            }

            // 
            // From this point on, it's safe to assume that activeScreen
            // is valid, and that all it's data is valid.
            // 
            
            // Get all the instructions from all the devices
            foreach (MenuInput device in inputs)
            {
                if (device != null)
                {
                    InstructionStack temp = device.GetInput();
                    userInput.Add(temp);
                    if (temp.Pop() != Instruction.Empty)
                    {
                        ActiveController = device.PlayerIndex;
                    }
                }
            }
           
            activeScreen.Update(gameTime, userInput, ref menuCommands);

            userInput.Clear();

            for (int i = 0; i < menuCommands.Count; i++)
            {
                switch (menuCommands[i].Type)
                {
                    case MenuCommand.CommandType.External: waitingInstructions.Add(menuCommands[i].AsInstruction()); break;
                    case MenuCommand.CommandType.OpenScreen: OpenScreen(menuCommands[i].Command); break;
                }
            }

            menuCommands.Clear();
            
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (activeScreen != null)
            {
                sprites.Begin();
                activeScreen.Draw(sprites);
                sprites.End();
            }

            base.Draw(gameTime);
        }
    }
}