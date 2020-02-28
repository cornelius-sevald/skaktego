using System;
using System.Linq;

namespace skaktego {

    public enum GameResults {
        WhiteWin, BlackWin, Tie, Quit
    }

    public class Game {
        // Move signaling that the player is done preparing in skaktego
        public static readonly ChessMove DONE_PREPARING_MOVE = new ChessMove( new BoardPosition(-1, -1)
                                                                            , new BoardPosition(-1, -1));
        const string normalStartStateStr = "rnbqkbnr/℗℗℗℗℗℗℗℗/8/8/8/8/ℙℙℙℙℙℙℙℙ/RNBQKBNR - w KQkq:a1:h1:a8:h8 - 0 1 s";
        const string skaktegoStartStateStr = "rnbqkbnr/℗℗℗℗℗℗℗℗/8/8/8/8/ℙℙℙℙℙℙℙℙ/RNBQKBNR - w KQkq:a1:h1:a8:h8 - 0 1 stp";

        public IPlayer whitePlayer;
        public IPlayer blackPlayer;
        public GameTypes gameType;
        public bool quit = false;

        public Game(IPlayer whitePlayer, IPlayer blackPlayer, GameTypes gameType) {
            this.whitePlayer = whitePlayer;
            this.blackPlayer = blackPlayer;
            this.gameType = gameType;
        }

        public Game(IPlayer whitePlayer, IPlayer blackPlayer)
            : this(whitePlayer, blackPlayer, GameTypes.Normal) {}

        public Tuple<GameState, GameResults> PlayGame() {
            GameState startState;
            if (gameType == GameTypes.Normal) {
                startState = GameState.FromString(normalStartStateStr);
            } else {
                startState = GameState.FromString(skaktegoStartStateStr);
            }
            return PlayGame(startState);
        }

        public Tuple<GameState, GameResults> PlayGame(GameState startState) {
            {
                GameState whiteObfGameState = GameState.FromString(startState.ToString());
                GameState blackObfGameState = GameState.FromString(startState.ToString());
                whiteObfGameState.Obfuscate(ChessColors.White);
                blackObfGameState.Obfuscate(ChessColors.Black);
            }

            bool checkMate = Engine.IsCheckmate(startState);
            bool tie = Engine.IsTie(startState);
            bool kingTaken = startState.taken.Any(p => p.Type == PieceTypes.King);

            GameState gameState = GameState.FromString(startState.ToString());
            while (!checkMate && !kingTaken && !tie && !quit) {
                // Obfuscated version of the board, if playing skaktego.
                // Otherwise just the normal game state.
                GameState whiteObfGameState = GameState.FromString(gameState.ToString());
                GameState blackObfGameState = GameState.FromString(gameState.ToString());;

                if (gameType == GameTypes.Skaktego || gameType == GameTypes.SkaktegoPrep) {
                    whiteObfGameState.Obfuscate(ChessColors.White);
                    blackObfGameState.Obfuscate(ChessColors.Black);
                }

                whitePlayer.SetGameState(whiteObfGameState);
                blackPlayer.SetGameState(blackObfGameState);

                ChessMove move;
                switch (gameState.player) {
                    case ChessColors.Black:
                        move = blackPlayer.GetMove(ChessColors.Black);
                        break;
                    default:
                        move = whitePlayer.GetMove(ChessColors.White);
                        break;
                }

                // Check if the player is done preparing
                if (move.to == DONE_PREPARING_MOVE.to && move.from == DONE_PREPARING_MOVE.from) {
                    // When the black player is done preparing, end the preparation phase
                    if (gameState.player == ChessColors.Black) {
                        gameState.gameType = GameTypes.Skaktego;
                    }
                    gameState.player = gameState.player.Other();
                    continue;
                }

                gameState = Engine.ApplyMove(gameState, move, false);
                checkMate = Engine.IsCheckmate(gameState);
                tie = Engine.IsTie(gameState);
                kingTaken = gameState.taken.Any(p => p.Type == PieceTypes.King);
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
                switch (gameState.taken.Find(p => p.Type == PieceTypes.King).Color) {
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
