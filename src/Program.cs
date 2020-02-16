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
            const string stateStr = "rnbqkbnr/pppppppp/3P4/8/8/8/PPP1PPPP/RNBQKBNR w KQkq - 0 1";
            var gameState = GameState.FromString(stateStr);
            UI ui = UI.Instance;

            //Engine.GetLegalPawnMoves(gameState, new BoardPosition(4,5));

            // Run the UI.
            while (!ui.quit)
            {
                ui.Update(gameState);
            }

            // Clean up
            ui.Quit();

            return 0;
        }
    }
}
