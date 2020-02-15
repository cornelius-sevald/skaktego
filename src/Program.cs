using System;
using System.IO;
using System.Text;

namespace skaktego
{
    class Program
    {

        static int Main(string[] args)
        {

            const string stateStr = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
            var gameState = GameState.FromString(stateStr);
            UI ui = UI.Instance;

            //Engine.GetLegalKnightMoves(gameState, new BoardPosition(2,2));

            while (!ui.Quit)
            {
                ui.Update(gameState);
            }

            return 0;
        }
    }
}
