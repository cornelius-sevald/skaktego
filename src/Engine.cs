using System;
using System.Linq;
using System.Collections.Generic;

namespace skaktego {

    public static class Engine{

        //Takes the legal moves for all pieces
        public static List<BoardPosition> GetAllLegalMoves(GameState gameState) {
            List<BoardPosition> allMoves = new List<BoardPosition>();
            for (int i = 0; i < gameState.board.Size; i++) {
                for (int j = 0; j < gameState.board.Size; j++) {
                    List<BoardPosition> legalMoves = GetLegalMoves(gameState, new BoardPosition(i, j));
                    allMoves = allMoves.Concat(legalMoves).ToList();
                }
            }
            return allMoves;
        }

        //Takes in the pseudo legal moves for the piece and checks if they are actually legal
        public static List<BoardPosition> GetLegalMoves(GameState gameState, BoardPosition pos) {
            Piece piece = gameState.board.GetPiece(pos);
            return GetLegalMoves(gameState, pos, piece);
        }

        public static List<BoardPosition> GetLegalMoves(GameState gameState, BoardPosition pos, Piece piece) {
            List<BoardPosition> moves = GetPseudoLegalMoves(gameState, pos, piece);
            List<BoardPosition> legalMoves = new List<BoardPosition>();
            foreach (BoardPosition move in moves) {
                GameState tempState = ApplyMove(gameState, pos, move, false);
                tempState.player = tempState.player.Other();
                if (!IsCheck(tempState)) {
                    legalMoves.Add(move);
                }
            }
            return legalMoves;
        }
        
        //Finds the piece of the current tile, and then redirects to specific move checker
        public static List<BoardPosition> GetPseudoLegalMoves(GameState gameState, BoardPosition pos) {
            Piece piece = gameState.board.GetPiece(pos);
            return GetPseudoLegalMoves(gameState, pos, piece);
        }
        public static List<BoardPosition> GetPseudoLegalMoves(GameState gameState, BoardPosition pos, Piece piece) {
            // There are no PseudoLegal moves, if there is no piece
            // or it is not the piece's colors turn
            if (piece == null || piece.Color != gameState.player) {
                return new List<BoardPosition>();
            }
            switch (piece.Type) {
                case PieceTypes.Pawn:
                    return GetPseudoLegalPawnMoves(gameState, pos, piece);

                case PieceTypes.Rook:
                    return GetPseudoLegalRookMoves(gameState, pos);

                case PieceTypes.Knight:
                    return GetPseudoLegalKnightMoves(gameState, pos);

                case PieceTypes.Bishop:
                    return GetPseudoLegalBishopMoves(gameState, pos);

                case PieceTypes.Queen:
                    return GetPseudoLegalQueenMoves(gameState, pos);

                case PieceTypes.King:
                    return GetPseudoLegalKingMoves(gameState, pos);

                default:
                    throw new ArgumentException("Unknown piece type " + piece.Type);

            }
        }

        /// <summary>
        /// Advance an entire turn.
        /// 
        /// Currently does not implement castling or queening, and only checks
        /// for pseudo-legal moves.
        /// </summary>
        /// <param name="gameState">The current game state</param>
        /// <param name="from">The position to move the piece from</param>
        /// <param name="to">The position to move the piece to</param>
        /// <param name="strict">Should the moved be checked if it is legal
        /// before being applied?</param>
        /// <returns></returns>
        public static GameState ApplyMove(GameState gameState, BoardPosition from, BoardPosition to, bool strict=true) {
            var newGameState = GameState.FromString(gameState.ToString());
            Piece piece = newGameState.board.GetPiece(from);

            if (strict) {
                // The move is illegal if it is not the current player's turn
                if (piece == null || piece.Color != newGameState.player) {
                    return newGameState;
                }

                List<BoardPosition> legalMoves = GetLegalMoves(newGameState, from, piece);
                // If the move is not legal, do not apply it
                if (!legalMoves.Contains(to)) {
                    return newGameState;
                }
            }

            newGameState.board.SetPiece(null, from);
            Piece captured = newGameState.board.CapturePiece(to);
            newGameState.board.SetPiece(piece, to);
            piece.hasMoved = true;

            // Check if a pawn is captured due to en passant
            if (to == newGameState.enPassant) {
                BoardPosition enPassantCapture;
                enPassantCapture.column = to.column;

                switch (newGameState.player) {
                    case ChessColors.Black:
                        enPassantCapture.row = to.row + 1;
                        break;
                    default:
                        enPassantCapture.row = to.row - 1;
                        break;
                }

                captured = newGameState.board.CapturePiece(enPassantCapture);
            }

            // Update en passant
            if (piece.Type == PieceTypes.Pawn && Math.Abs(from.row - to.row) == 2) {
                newGameState.enPassant = new BoardPosition(from.column, (from.row + to.row) / 2);
            } else {
                newGameState.enPassant = null;
            }

            //promote a pawn
            if (piece.Type == PieceTypes.Pawn && (to.row == 0 || to.row == gameState.board.Size - 1)) {
                piece.Promote(PieceTypes.Queen);
            }

            // Advance the game clocks.
            if (captured == null && piece.Type != PieceTypes.Pawn) {
                newGameState.halfmoveClock++;
            } else {
                newGameState.halfmoveClock = 0;
            }
            if (newGameState.player == ChessColors.Black) {
                newGameState.fullmoveClock++;
            }

            newGameState.player = newGameState.player.Other();

            return newGameState;
        }

        //checks the PseudoLegal moves if the piece is a pawn
        public static List<BoardPosition> GetPseudoLegalPawnMoves(GameState gameState, BoardPosition pos, Piece piece) {
            Stack<BoardPosition> possibleMoves = new Stack<BoardPosition>();

            //checks PseudoLegal moves without capture for the pawn
            BoardPosition here = pos;
            if (gameState.player == ChessColors.White) {
                here.row++;
                if (here.row < gameState.board.Size && !gameState.board.IsTileOccupied(here)) {
                    possibleMoves.Push(here);
                    if(!piece.hasMoved) {
                        here.row++;
                        if (here.row < gameState.board.Size && !gameState.board.IsTileOccupied(here)) {
                            possibleMoves.Push(here);
                        }
                    }
                }   
            }

            if (gameState.player == ChessColors.Black) {
                here = pos;
                here.row--;
                if (here.row >= 0 && !gameState.board.IsTileOccupied(here)) {
                    possibleMoves.Push(here);
                    if(!piece.hasMoved) {
                        here.row--;
                        if (here.row >= 0 && !gameState.board.IsTileOccupied(here)) {
                            possibleMoves.Push(here);
                        }
                    }
                }   
            }

            //checks PseudoLegal captures for the pawn
            if (gameState.player == ChessColors.White) {
                here = pos;
                here.row++;
                here.column++;
                if(here.row < gameState.board.Size && here.column < gameState.board.Size) {
                    if (gameState.board.IsTileOccupied(here) && gameState.board.GetPiece(here).Color != gameState.player) {
                        possibleMoves.Push(here);
                    } else if (gameState.enPassant.HasValue && gameState.enPassant.Value == here) {
                        possibleMoves.Push(here);
                    }
                }
                here.column -= 2;
                if(here.row < gameState.board.Size && here.column >= 0) {
                    if (gameState.board.IsTileOccupied(here) && gameState.board.GetPiece(here).Color != gameState.player) {
                        possibleMoves.Push(here);
                    } else if (gameState.enPassant.HasValue && gameState.enPassant.Value == here) {
                        possibleMoves.Push(here);
                    }
                }
            }

            if (gameState.player == ChessColors.Black) {
                here = pos;
                here.row--;
                here.column++;
                if(here.row >= 0 && here.column < gameState.board.Size) {
                    if (gameState.board.IsTileOccupied(here) && gameState.board.GetPiece(here).Color != gameState.player) {
                        possibleMoves.Push(here);
                    } else if (gameState.enPassant.HasValue && gameState.enPassant.Value == here) {
                        possibleMoves.Push(here);
                    }
                }
                here.column -= 2;
                if(here.row >= 0 && here.column >= 0) {
                    if (gameState.board.IsTileOccupied(here) && gameState.board.GetPiece(here).Color != gameState.player) {
                        possibleMoves.Push(here);
                    } else if (gameState.enPassant.HasValue && gameState.enPassant.Value == here) {
                        possibleMoves.Push(here);
                    }
                }
            }
            
            return new List<BoardPosition>(possibleMoves);
        }

        //checks the PseudoLegal moves if the piece is a rook
        public static List<BoardPosition> GetPseudoLegalRookMoves(GameState gameState, BoardPosition pos) {
            Stack<BoardPosition> possibleMoves = new Stack<BoardPosition>();

            //checks PseudoLegal moves in the positive vertical direction from the piece's position
            BoardPosition here = pos;
            while (true) {
                here.row++;
                if (here.row >= gameState.board.Size) {
                    break;
                }
                possibleMoves.Push(here);
                if (gameState.board.IsTileOccupied(here)) {
                    if(gameState.board.GetPiece(here).Color == gameState.player) {
                        possibleMoves.Pop();
                    }
                    break;
                }
            }

            //checks PseudoLegal moves in the negative vertical direction from the piece's position
            here = pos;
            while (true) {
                here.row--;
                if (here.row < 0) {
                    break;
                }
                possibleMoves.Push(here);
                if (gameState.board.IsTileOccupied(here)) {
                    if(gameState.board.GetPiece(here).Color == gameState.player) {
                        possibleMoves.Pop();
                    }
                    break;
                }
            }
            
            //checks PseudoLegal moves in the positive horizontal direction from the piece's position
            here = pos;
            while (true) {
                here.column++;
                if (here.column >= gameState.board.Size) {
                    break;
                }
                possibleMoves.Push(here);
                if (gameState.board.IsTileOccupied(here)) {
                    if(gameState.board.GetPiece(here).Color == gameState.player) {
                        possibleMoves.Pop();
                    }
                    break;
                }
            }

            //checks PseudoLegal moves in the negative horizontal direction from the piece's position
            here = pos;
            while (true) {
                here.column--;
                if (here.column < 0) {
                    break;
                }
                possibleMoves.Push(here);
                if (gameState.board.IsTileOccupied(here)) {
                    if(gameState.board.GetPiece(here).Color == gameState.player) {
                        possibleMoves.Pop();
                    }
                    break;
                }
            }
            return new List<BoardPosition>(possibleMoves);
         }

        //checks the PseudoLegal moves if the piece is a knight
         public static List<BoardPosition> GetPseudoLegalKnightMoves(GameState gameState, BoardPosition pos) {
            Stack<BoardPosition> possibleMoves = new Stack<BoardPosition>();

            //The knight can have a maximum of 8 possible moves, and the only factor is if the move is outside the board or is occupied by the players own piece
            BoardPosition here = pos;
            for (int i = 1; i < 9; i++) {
                here = pos;
                here.row += (int)Math.Round(2*Math.Sin(i*0.8 + 1));
                here.column += (int)Math.Round(2*Math.Sin(i*0.8 - 0.5));;
                possibleMoves.Push(here);
                if (here.row >= gameState.board.Size || here.row < 0 || here.column >= gameState.board.Size || here.column < 0) {
                    possibleMoves.Pop();
                } else if (gameState.board.IsTileOccupied(here) && gameState.board.GetPiece(here).Color == gameState.player) {
                    possibleMoves.Pop();
                }

            }

            return new List<BoardPosition>(possibleMoves);
        }

        //checks the PseudoLegal moves if the piece is a bishop
        public static List<BoardPosition> GetPseudoLegalBishopMoves(GameState gameState, BoardPosition pos) {
            Stack<BoardPosition> possibleMoves = new Stack<BoardPosition>();

            //checks PseudoLegal moves in the positive vertical and horizontal direction from the piece's position
            BoardPosition here = pos;
            while (true) {
                here.row++;
                here.column++;
                if (here.row >= gameState.board.Size || here.column >= gameState.board.Size) {
                    break;
                }
                possibleMoves.Push(here);
                if (gameState.board.IsTileOccupied(here)) {
                    if(gameState.board.GetPiece(here).Color == gameState.player) {
                        possibleMoves.Pop();
                    }
                    break;
                }
            }

            //checks PseudoLegal moves in the negative vertical and positive horizontal direction from the piece's position
            here = pos;
            while (true) {
                here.row--;
                here.column++;
                if (here.row < 0 || here.column >= gameState.board.Size) {
                    break;
                }
                possibleMoves.Push(here);
                if (gameState.board.IsTileOccupied(here)) {
                    if(gameState.board.GetPiece(here).Color == gameState.player) {
                        possibleMoves.Pop();
                    }
                    break;
                }
            }
            
            //checks PseudoLegal moves in the negative vertical and horizontal direction from the piece's position
            here = pos;
            while (true) {
                here.row--;
                here.column--;
                if (here.row < 0 || here.column < 0) {
                    break;
                }
                possibleMoves.Push(here);
                if (gameState.board.IsTileOccupied(here)) {
                    if(gameState.board.GetPiece(here).Color == gameState.player) {
                        possibleMoves.Pop();
                    }
                    break;
                }
            }

            //checks PseudoLegal moves in the positive vertical and negative horizontal direction from the piece's position
            here = pos;
            while (true) {
                here.row++;
                here.column--;
                if (here.row >= gameState.board.Size || here.column < 0) {
                    break;
                }
                possibleMoves.Push(here);
                if (gameState.board.IsTileOccupied(here)) {
                    if(gameState.board.GetPiece(here).Color == gameState.player) {
                        possibleMoves.Pop();
                    }
                    break;
                }
            }
            return new List<BoardPosition>(possibleMoves);
        }

        //checks the PseudoLegal moves if the piece is a queen
        public static List<BoardPosition> GetPseudoLegalQueenMoves(GameState gameState, BoardPosition pos) {

            //Using the rook and bishop move checkers as they will cover all the queens movement options
            List<BoardPosition> fakeRook = GetPseudoLegalRookMoves(gameState, pos);
            List<BoardPosition> fakeBishop = GetPseudoLegalBishopMoves(gameState, pos);
            return fakeRook.Concat(fakeBishop).ToList();
        }

        //checks the PseudoLegal moves if the piece is a king
        public static List<BoardPosition> GetPseudoLegalKingMoves(GameState gameState, BoardPosition pos) {

            Stack<BoardPosition> possibleMoves = new Stack<BoardPosition>();

            //The king can have a maximum of 8 possible moves
            BoardPosition here = pos;
            for (int i = 1; i < 9; i++) {
                here = pos;
                here.row += (int)Math.Round(Math.Sin(i*0.8 + 0.6));
                here.column += (int)Math.Round(Math.Sin(i*0.8 - 0.7));;
                possibleMoves.Push(here);
                if (here.row >= gameState.board.Size || here.row < 0 || here.column >= gameState.board.Size || here.column < 0) {
                    possibleMoves.Pop();
                } else if (gameState.board.IsTileOccupied(here) && gameState.board.GetPiece(here).Color == gameState.player) {
                    possibleMoves.Pop();
                }

            }

            return new List<BoardPosition>(possibleMoves);
        }

        public static bool IsTileAttacked(GameState gameState, BoardPosition pos) {
            foreach (PieceTypes type in Enum.GetValues(typeof(PieceTypes))) {
                var piece = new Piece(gameState.player, type);
                List<BoardPosition> moves = GetPseudoLegalMoves(gameState, pos, piece);
                foreach (BoardPosition move in moves) {
                    Piece attackedPiece = gameState.board.GetPiece(move);
                    if (attackedPiece != null && attackedPiece.Type == type) {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsCheck(GameState gameState) {
            BoardPosition kingPos = new BoardPosition();
            bool kingFound = false;
            for (int i = 0; i < gameState.board.Size; i++) {
                for (int j = 0; j < gameState.board.Size; j++) {
                    kingPos.column = i;
                    kingPos.row = j;
                    Piece piece = gameState.board.GetPiece(kingPos);
                    if (piece != null && piece.Type == PieceTypes.King && piece.Color == gameState.player) {
                        kingFound = true;
                        break;
                    }
                }
                if(kingFound) {
                    break;
                }
            }
            if (!kingFound) {
                //throw new ChessException("No king found");
                return false;
            }
            return IsTileAttacked(gameState, kingPos);
        }

        //checks if the game is a tie
        public static bool IsTie(GameState gameState) {

            //50 moves without capture or moving a pawn
            if(gameState.halfmoveClock >= 50) {
                Console.WriteLine("for mange moves");
                return true;
            }

            //If there are only 2 kings 
            int pieceCount = 0;
            for (int i = 0; i < gameState.board.Size; i++) {
                for (int j = 0; j < gameState.board.Size; j++) {
                    if (gameState.board.GetPiece(new BoardPosition(i, j)) != null) {
                        pieceCount++;
                    }
                }
            }
            if (pieceCount <= 2) {
                Console.WriteLine("for få brikker");
                return true;
            }

            //if there are no legal moves for the current player
            List<BoardPosition> legalMoves = GetAllLegalMoves(gameState);
            if (legalMoves.Count == 0) {
                Console.WriteLine("for få træk");
                return true;
            }
            Console.WriteLine("gode tider");
            return false;
        }

        public static bool IsCheckmate(GameState gameState) {
            if (IsCheck(gameState) && IsTie(gameState)) {
                return true;
            }
            return false;
        }
    }

    public class ChessException : Exception {
        public ChessException(string message) : base(message) {
        }
    }
}