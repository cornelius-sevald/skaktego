using System;
using System.Linq;
using System.Collections.Generic;

namespace skaktego {

    public static class Engine{

        //Finds the piece of the current tile, and then redirects to specific move checker
        public static List<BoardPosition> GetPseudoLegalMoves(GameState gameState, BoardPosition pos) {
            Piece piece = gameState.board.GetPiece(pos);
            // There are no PseudoLegal moves, if there is no piece
            // or it is not the piece's colors turn
            if (piece == null || piece.Color != gameState.player) {
                return new List<BoardPosition>();
            }
            switch (piece.Type) {
                case PieceTypes.Pawn:
                    return GetPseudoLegalPawnMoves(gameState, pos);

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

        //checks the PseudoLegal moves if the piece is a pawn
        public static List<BoardPosition> GetPseudoLegalPawnMoves(GameState gameState, BoardPosition pos) {
            Stack<BoardPosition> possibleMoves = new Stack<BoardPosition>();
            Piece piece = gameState.board.GetPiece(pos);

            //checks PseudoLegal moves without capture for the pawn
            BoardPosition here = pos;
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

            //checks PseudoLegal captures for the pawn
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
    }
}