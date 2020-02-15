using System;
using System.Linq;
using System.Collections.Generic;

namespace skaktego {

    public static class Engine{

        //Finds the piece of the current tile, and then redirects to specific move checker
        public static List<BoardPosition> GetLegalMoves(GameState gameState, BoardPosition pos) {
            Piece piece = gameState.board.GetPiece(pos);
            if (piece == null) {
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

            return null;
        }

        //checks the legal moves if the piece is a rook
        public static List<BoardPosition> GetLegalRookMoves(GameState gameState, BoardPosition pos) {
            Stack<BoardPosition> possibleMoves = new Stack<BoardPosition>();

            //checks legal moves in the positive vertical direction from the piece's position
            BoardPosition here = pos.Copy();
            while (true) {
                here.Row++;
                if (here.Row >= gameState.board.Size) {
                    break;
                }
                possibleMoves.Push(here.Copy());
                if (gameState.board.IsTileOccupied(here)) {
                    if(gameState.board.GetPiece(here).Color == gameState.player) {
                        possibleMoves.Pop();
                    }
                    break;
                }
            }

            //checks legal moves in the negative vertical direction from the piece's position
            here = pos.Copy();
            while (true) {
                here.Row--;
                if (here.Row < 0) {
                    break;
                }
                possibleMoves.Push(here.Copy());
                if (gameState.board.IsTileOccupied(here)) {
                    if(gameState.board.GetPiece(here).Color == gameState.player) {
                        possibleMoves.Pop();
                    }
                    break;
                }
            }
            
            //checks legal moves in the positive horizontal direction from the piece's position
            here = pos.Copy();
            while (true) {
                here.Column++;
                if (here.Column >= gameState.board.Size) {
                    break;
                }
                possibleMoves.Push(here.Copy());
                if (gameState.board.IsTileOccupied(here)) {
                    if(gameState.board.GetPiece(here).Color == gameState.player) {
                        possibleMoves.Pop();
                    }
                    break;
                }
            }

            //checks legal moves in the negative horizontal direction from the piece's position
            here = pos.Copy();
            while (true) {
                here.Column--;
                if (here.Column < 0) {
                    break;
                }
                possibleMoves.Push(here.Copy());
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
            BoardPosition here = pos.Copy();
            for (int i = 1; i < 9; i++) {
                here = pos.Copy();
                here.Row += (int)Math.Round(2*Math.Sin(i*0.8 + 1));
                here.Column += (int)Math.Round(2*Math.Sin(i*0.8 - 0.5));;
                possibleMoves.Push(here.Copy());
                if (here.Row >= gameState.board.Size || here.Row < 0 || here.Column >= gameState.board.Size || here.Column < 0) {
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
            BoardPosition here = pos.Copy();
            while (true) {
                here.Row++;
                here.Column++;
                if (here.Row >= gameState.board.Size || here.Column >= gameState.board.Size) {
                    break;
                }
                possibleMoves.Push(here.Copy());
                if (gameState.board.IsTileOccupied(here)) {
                    if(gameState.board.GetPiece(here).Color == gameState.player) {
                        possibleMoves.Pop();
                    }
                    break;
                }
            }

            //checks legal moves in the negative vertical and positive horizontal direction from the piece's position
            here = pos.Copy();
            while (true) {
                here.Row--;
                here.Column++;
                if (here.Row < 0 || here.Column >= gameState.board.Size) {
                    break;
                }
                possibleMoves.Push(here.Copy());
                if (gameState.board.IsTileOccupied(here)) {
                    if(gameState.board.GetPiece(here).Color == gameState.player) {
                        possibleMoves.Pop();
                    }
                    break;
                }
            }
            
            //checks legal moves in the negative vertical and horizontal direction from the piece's position
            here = pos.Copy();
            while (true) {
                here.Row--;
                here.Column--;
                if (here.Row < 0 || here.Column < 0) {
                    break;
                }
                possibleMoves.Push(here.Copy());
                if (gameState.board.IsTileOccupied(here)) {
                    if(gameState.board.GetPiece(here).Color == gameState.player) {
                        possibleMoves.Pop();
                    }
                    break;
                }
            }

            //checks legal moves in the positive vertical and negative horizontal direction from the piece's position
            here = pos.Copy();
            while (true) {
                here.Row++;
                here.Column--;
                if (here.Row >= gameState.board.Size || here.Column < 0) {
                    break;
                }
                possibleMoves.Push(here.Copy());
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
            BoardPosition here = pos.Copy();
            for (int i = 1; i < 9; i++) {
                here = pos.Copy();
                here.Row += (int)Math.Round(Math.Sin(i*0.8 + 0.6));
                here.Column += (int)Math.Round(Math.Sin(i*0.8 - 0.7));;
                possibleMoves.Push(here.Copy());
                if (here.Row >= gameState.board.Size || here.Row < 0 || here.Column >= gameState.board.Size || here.Column < 0) {
                    possibleMoves.Pop();
                } else if (gameState.board.IsTileOccupied(here) && gameState.board.GetPiece(here).Color == gameState.player) {
                    possibleMoves.Pop();
                }

            }

            return new List<BoardPosition>(possibleMoves);
        }
    }
}