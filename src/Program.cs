using System;

namespace skaktego {
    class Program {
        static void Main(string[] args) {
            const string stateStr = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
            var gameState = GameState.FromString(stateStr);
            System.Console.WriteLine(stateStr);
            System.Console.WriteLine(gameState.ToString());
        }
    }
}
