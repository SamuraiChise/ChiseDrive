using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChiseDrive
{
    public enum GameState
    {
        Initialize, // Preloads things, displays the Chiseland splash
        RunMenu,    // Displays a menu 
        LoadGame,   // Resets variables, loads a game
        RunGame,    // The game is running
        PauseGame,  // The pause menu is running
        EndGame,    // Shutdown the game
    };
}