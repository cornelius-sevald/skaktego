using System;
using System.Linq;
using System.Collections.Generic;

namespace skaktego {

    /// <summary>
    /// Class that can play chess and skaktego using the miniax algorithm
    /// </summary>
    public class ChessAI : IPlayer {

        // The value of different piece types
        private static readonly Dictionary<PieceTypes, double> PIECE_VALUES
            = new Dictionary<PieceTypes, double> {
                { PieceTypes.Pawn  , 1.0    },
                { PieceTypes.Knight, 3.0    },
                { PieceTypes.Bishop, 3.0    },
                { PieceTypes.Rook  , 5.0    },
                { PieceTypes.Queen , 9.0    },
                { PieceTypes.King  , 1000.0 }
            };

        /// <summary>
        /// The search depth of the minimax function.
        /// 
        /// Higher values will make the AI better,
        /// but use more time to compute.
        /// </summary>
        public int SearchDepth { get; private set; }

        private GameState gameState = null;
        private Nullable<ChessMove> bestMove = null;
        private Random rand;

        /// <summary>
        /// Construct a new <c>ChessAI</c> with a given search depth
        /// </summary>
        /// <param name="searchDepth">The depth of the algorithm search</param>
        public ChessAI(int searchDepth)
            : this(searchDepth, new Random()) {}

        /// <summary>
        /// Construct a new <c>ChessAI</c> with a given search depth and RNG generator
        /// </summary>
        /// <param name="searchDepth">The depth of the algorithm search</param>
        /// <param name="rand">The random number generator</param>
        public ChessAI(int searchDepth, Random rand) {
            SearchDepth = searchDepth;
            this.rand = rand;
        }


        public void SetGameState(GameState gameState) {
            this.gameState = gameState;
        }

        public ChessMove GetMove(ChessColors color) {
            if (color == ChessColors.White) {
                MiniMax(gameState, SearchDepth, true);
            } else {
                MiniMax(gameState, SearchDepth, false);
            }

            if (!bestMove.HasValue) {
                throw new InvalidOperationException("No best move found");
            }

            return bestMove.Value;
        }

        private double MiniMax(GameState gameState, int depth, bool maximizingPlayer) {
            // Return early if reached max depth, or the game is over
            if (depth == 0 || Engine.IsGameOver(gameState)) {
                return EvaluateBoard(gameState.board);
            }


            List<ChessMove> legalMoves = Engine.GetAllLegalMoves(gameState);
            // Shuffle the list
            legalMoves = legalMoves.OrderBy(x => rand.Next()).ToList();
            if (maximizingPlayer) {

                double maxEval = double.NegativeInfinity;
                foreach (ChessMove move in legalMoves) {
                    // Advance the game state one move
                    GameState newGameStage = Engine.ApplyMove(gameState, move, false);
                    // Recursively call the minimax algorithm with the new state,
                    // decremented depth and opposite maximizing player.
                    double eval = MiniMax(newGameStage, depth - 1, false);
                    // Check if eval is greater than maxEval
                    if (eval > maxEval) {
                        maxEval = eval;
                        if (depth == SearchDepth) {
                            bestMove = move;
                        }
                    }
                }
                return maxEval;

            } else {

                double minEval = double.PositiveInfinity;
                foreach (ChessMove move in legalMoves) {
                    // Advance the game state one move
                    GameState newGameStage = Engine.ApplyMove(gameState, move, false);
                    // Recursively call the minimax algorithm with the new state,
                    // decremented depth and opposite maximizing player.
                    double eval = MiniMax(newGameStage, depth - 1, true);
                    // Check if eval is lesser than maxEval
                    if (eval < minEval) {
                        minEval = eval;
                        if (depth == SearchDepth) {
                            bestMove = move;
                        }
                    }
                }
                return minEval;
            }
        }

        /// <summary>
        /// Evaluate the value of the board.
        /// 
        /// White's pieces have a positive value,
        /// while black's have negative value
        /// </summary>
        /// <param name="board">The game board</param>
        /// <returns></returns>
        private double EvaluateBoard(Board board) {
            // Start with neutral value
            double boardValue = 0;

            for (int i = 0; i < board.Size; i++) {
                for (int j = 0; j < board.Size; j++) {
                    BoardPosition pos = new BoardPosition(i, j);
                    Piece piece = board.GetPiece(pos);

                    if (piece == null) {
                        continue;
                    }
                    // Record the piece value
                    double pieceValue = PIECE_VALUES[piece.Type];
                    // Adjust the board value accordingly
                    if (piece.Color == ChessColors.White) {
                        boardValue += pieceValue;
                    } else {
                        boardValue -= pieceValue;
                    }
                }
            }

            return boardValue;
        }

    }

}