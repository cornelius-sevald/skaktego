using System;
using System.Linq;
using System.Collections.Generic;

namespace skaktego {

    public static class Engine{

        //Finds the piece of the current tile, and then redirects to specific move checker
        public static List<BoardPosition> GetLegalMoves(GameState gameState, BoardPosition pos) {
            Piece piece = gameState.board.GetPiece(pos);
            // There are no legal moves, if there is no piece
            // or it is not the piece's colors turn
            if (piece == null || piece.Color != gameState.player) {
                return new List<BoardPosition>();
            }
            switch (piece.Type) {
                case PieceTypes.Pawn:
                    return GetLegalPawnMoves(gameState, pos);

                case PieceTypes.Rook:
                    return GetLegalRookMoves(gameState, pos);

                case PieceTypes.Knight:
                    return GetLegalKnightMoves(gameState, pos);

                case PieceTypes.Bishop:
                    return GetLegalBishopMoves(gameState, pos);

                case PieceTypes.Queen:
                    return GetLegalQueenMoves(gameState, pos);

                case PieceTypes.King:
                    return GetLegalKingMoves(gameState, pos);

                default:
                    throw new ArgumentException("Unknown piece type " + piece.Type);

            }
        }

        //checks the legal moves if the piece is a pawn
        public static List<BoardPosition> GetLegalPawnMoves(GameState gameState, BoardPosition pos) {
            Stack<BoardPosition> possibleMoves = new Stack<BoardPosition>();

            //checks legal moves for the pawn assuming it have never moved
            BoardPosition here = pos;
            here.row++;
            if (!gameState.board.IsTileOccupied(here)) {
                possibleMoves.Push(here);
                here.row++;
                if (!gameState.board.IsTileOccupied(here)) {
                    possibleMoves.Push(here);
                }
            }

            here = pos;
            here.row++;
            here.column++;
            if (gameState.board.IsTileOccupied(here) && gameState.board.GetPiece(here).Color != gameState.player) {
                possibleMoves.Push(here);
            }
            here.column -= 2;
            if (gameState.board.IsTileOccupied(here) && gameState.board.GetPiece(here).Color != gameState.player) {
                possibleMoves.Push(here);
            }
            
            return new List<BoardPosition>(possibleMoves);
        }

        //checks the legal moves if the piece is a rook
        public static List<BoardPosition> GetLegalRookMoves(GameState gameState, BoardPosition pos) {
            Stack<BoardPosition> possibleMoves = new Stack<BoardPosition>();

            //checks legal moves in the positive vertical direction from the piece's position
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

            //checks legal moves in the negative vertical direction from the piece's position
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
            
            //checks legal moves in the positive horizontal direction from the piece's position
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

            //checks legal moves in the negative horizontal direction from the piece's position
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

        //checks the legal moves if the piece is a knight
         public static List<BoardPosition> GetLegalKnightMoves(GameState gameState, BoardPosition pos) {
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

        //checks the legal moves if the piece is a bishop
        public static List<BoardPosition> GetLegalBishopMoves(GameState gameState, BoardPosition pos) {
            Stack<BoardPosition> possibleMoves = new Stack<BoardPosition>();

            //checks legal moves in the positive vertical and horizontal direction from the piece's position
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

            //checks legal moves in the negative vertical and positive horizontal direction from the piece's position
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
            
            //checks legal moves in the negative vertical and horizontal direction from the piece's position
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

            //checks legal moves in the positive vertical and negative horizontal direction from the piece's position
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

        //checks the legal moves if the piece is a queen
        public static List<BoardPosition> GetLegalQueenMoves(GameState gameState, BoardPosition pos) {

            //Using the rook and bishop move checkers as they will cover all the queens movement options
            List<BoardPosition> fakeRook = GetLegalRookMoves(gameState, pos);
            List<BoardPosition> fakeBishop = GetLegalBishopMoves(gameState, pos);
            return fakeRook.Concat(fakeBishop).ToList();
        }

        //checks the legal moves if the piece is a king
        public static List<BoardPosition> GetLegalKingMoves(GameState gameState, BoardPosition pos) {

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
    }
}