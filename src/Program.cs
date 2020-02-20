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
            const string stateStr = "rnbqkbnr/℗℗℗℗℗℗℗℗/8/8/8/8/ℙℙℙℙℙℙℙℙ/RNBQKBNR w KQkq - 0 1";
            var gameState = GameState.FromString(stateStr);
            UI ui = UI.Instance;

            //Engine.GetLegalPawnMoves(gameState, new BoardPosition(4,5));

            // Run the UI.
            while (!ui.quit)
            {
                ui.Update(ref gameState);
            }

            // Clean up
            ui.Quit();

            return 0;
        }
    }
}
