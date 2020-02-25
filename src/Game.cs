using System;

namespace skaktego {

    public class Game {
        const string startStateStr = "rnbqkbnr/℗℗℗℗℗℗℗℗/8/8/8/8/ℙℙℙℙℙℙℙℙ/RNBQKBNR w KQkq - 0 1";

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
            {
                GameState whiteObfGameState = GameState.FromString(startState.ToString());
                GameState blackObfGameState = GameState.FromString(startState.ToString());
                whiteObfGameState.Obfuscate(ChessColors.White);
                blackObfGameState.Obfuscate(ChessColors.Black);
                whitePlayer.GameStart(whiteObfGameState);
                blackPlayer.GameStart(blackObfGameState);
            }

            bool checkMate = Engine.IsCheckmate(startState);
            bool tie = Engine.IsTie(startState);

            GameState gameState = GameState.FromString(startState.ToString());
            while (!checkMate && !tie && !quit) {
                GameState obfGameState;
                ChessMove move;
                switch (gameState.player) {
                    case ChessColors.Black:
                        obfGameState = GameState.FromString(gameState.ToString());
                        obfGameState.Obfuscate(gameState.player);
                        move = blackPlayer.GetMove(obfGameState);
                        break;
                    default:
                        obfGameState = GameState.FromString(gameState.ToString());
                        obfGameState.Obfuscate(gameState.player);
                        move = whitePlayer.GetMove(obfGameState);
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
