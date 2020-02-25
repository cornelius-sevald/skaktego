using System;

namespace skaktego {

    public class Game {
        const string startStateStr = "rnbqkbnr/℗℗℗℗℗℗℗℗/8/8/8/8/ℙℙℙℙℙℙℙℙ/RNBQKBNR w KQkq:a1:h1:a8:h8 - 0 1";

        public IPlayer whitePlayer;
        public IPlayer blackPlayer;
        public bool quit = false;

        public Game(IPlayer whitePlayer, IPlayer blackPlayer) {
            this.whitePlayer = whitePlayer;
            this.blackPlayer = blackPlayer;
        }

        public GameState PlayGame() {
            GameState startState = GameState.FromString(startStateStr);
            return PlayGame(startState);
        }

        public GameState PlayGame(GameState startState) {
            whitePlayer.GameStart(startState);
            blackPlayer.GameStart(startState);

            bool checkMate = Engine.IsCheckmate(startState);
            bool tie = Engine.IsTie(startState);

            GameState gameState = GameState.FromString(startState.ToString());
            while (!checkMate && !tie && !quit) {
                ChessMove move;
                switch (gameState.player) {
                    case ChessColors.Black:
                        move = blackPlayer.GetMove(gameState);
                        break;
                    default:
                        move = whitePlayer.GetMove(gameState);
                        break;
                }

                gameState = Engine.ApplyMove(gameState, move, true);
                checkMate = Engine.IsCheckmate(gameState);
                tie = Engine.IsTie(gameState);
            }

            if (quit) {
                Console.WriteLine("En gamer har stoppet spillet 😳");
            } else if (checkMate) {
                switch (gameState.player) {
                    case ChessColors.Black:
                        Console.WriteLine("Hvid vinder");
                        break;
                    default:
                        Console.WriteLine("Sort vinder");
                        break;
                }
            } else if (tie) {
                Console.WriteLine("Det står lige, gamere..");
            }

            return gameState;
        }
    }

}
