using System;
using System.IO;
using System.Text;

namespace skaktego
{
    class Program
    {

        static int Main(string[] args)
        {
            // Initialize
            const string stateStr = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
            var gameState = GameState.FromString(stateStr);
            UI ui = UI.Instance;

<<<<<<< HEAD
            // Run the UI.
            while (!ui.quit)
=======
            

            while (!ui.Quit)
>>>>>>> engine
            {
                ui.Update(gameState);
            }

            // Clean up
            ui.Quit();

            return 0;
        }
    }
}
