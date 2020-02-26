using System;

namespace skaktego {

    public enum GameResults {
        WhiteWin, BlackWin, Tie, Quit
    }

    public class Game {
        const string startStateStr = "rnbqkbnr/℗℗℗℗℗℗℗℗/8/8/8/8/ℙℙℙℙℙℙℙℙ/RNBQKBNR w KQkq:a1:h1:a8:h8 - - 0 1 st";

        public IPlayer whitePlayer;
        public IPlayer blackPlayer;
        public bool quit = false;

        public Game(IPlayer whitePlayer, IPlayer blackPlayer) {
            this.whitePlayer = whitePlayer;
            this.blackPlayer = blackPlayer;
        }

        public Tuple<GameState, GameResults> PlayGame() {
            GameState startState = GameState.FromString(startStateStr);
            return PlayGame(startState);
        }

        public Tuple<GameState, GameResults> PlayGame(GameState startState) {
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
            bool kingTaken = startState.kingTaken.HasValue;

            GameState gameState = GameState.FromString(startState.ToString());
            while (!checkMate && !kingTaken && !tie && !quit) {
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

                gameState = Engine.ApplyMove(gameState, move, false);
                checkMate = Engine.IsCheckmate(gameState);
                tie = Engine.IsTie(gameState);
                kingTaken = gameState.kingTaken.HasValue;
            }

            GameResults results = GameResults.Quit;

            if (quit) {
                Console.WriteLine("En gamer har stoppet spillet 😳");
                results = GameResults.Quit;
            } else if (checkMate) {
                switch (gameState.player) {
                    case ChessColors.Black:
                        Console.WriteLine("Hvid vinder");
                        results = GameResults.WhiteWin;
                        break;
                    default:
                        Console.WriteLine("Sort vinder");
                        results = GameResults.BlackWin;
                        break;
                }
            } else if (kingTaken) {
                switch (gameState.kingTaken) {
                    case ChessColors.Black:
                        Console.WriteLine("Hvid vinder");
                        results = GameResults.WhiteWin;
                        break;
                    default:
                        Console.WriteLine("Sort vinder");
                        results = GameResults.BlackWin;
                        break;
                }
            } else if (tie) {
                Console.WriteLine("Det står lige, gamere..");
                results = GameResults.Tie;
            }

            return new Tuple<GameState, GameResults> (gameState, results);
        }
    }

}
