using System;
using System.Linq;
using System.Collections.Generic;

namespace skaktego {

    public class Engine{

        //Finds the piece of the current tile, and then redirects to specific move checker
        public List<BoardPosition> GetLegalMoves(GameState gameState, BoardPosition pos) {
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
        public List<BoardPosition> GetLegalPawnMoves(GameState gameState, BoardPosition pos) {

            return null;
        }

        //checks the legal moves if the piece is a rook
        public List<BoardPosition> GetLegalRookMoves(GameState gameState, BoardPosition pos) {
            Stack<BoardPosition> possibleMoves = new Stack<BoardPosition>();

            //checks legal moves in the positive vertical direction from the piece's position
            BoardPosition here = new BoardPosition(pos.Column, pos.Row);
            while (true) {
                here.Row++;
                if (here.Row >= gameState.board.Size) {
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
            here = new BoardPosition(pos.Column, pos.Row);
            while (true) {
                here.Row--;
                if (here.Row < 0) {
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
            here = new BoardPosition(pos.Column, pos.Row);
            while (true) {
                here.Column++;
                if (here.Column >= gameState.board.Size) {
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
            here = new BoardPosition(pos.Column, pos.Row);
            while (true) {
                here.Column--;
                if (here.Column < 0) {
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
         public List<BoardPosition> GetLegalKnightMoves(GameState gameState, BoardPosition pos) {

            return null;
        }

        //checks the legal moves if the piece is a bishop
        public List<BoardPosition> GetLegalBishopMoves(GameState gameState, BoardPosition pos) {
            Stack<BoardPosition> possibleMoves = new Stack<BoardPosition>();

            //checks legal moves in the positive vertical and horizontal direction from the piece's position
            BoardPosition here = new BoardPosition(pos.Column, pos.Row);
            while (true) {
                here.Row++;
                here.Column++;
                if (here.Row >= gameState.board.Size || here.Column >= gameState.board.Size) {
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
            here = new BoardPosition(pos.Column, pos.Row);
            while (true) {
                here.Row--;
                here.Column++;
                if (here.Row < 0 || here.Column >= gameState.board.Size) {
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
            here = new BoardPosition(pos.Column, pos.Row);
            while (true) {
                here.Row--;
                here.Column--;
                if (here.Row < 0 || here.Column < 0) {
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
            here = new BoardPosition(pos.Column, pos.Row);
            while (true) {
                here.Row++;
                here.Column--;
                if (here.Row >= gameState.board.Size || here.Column < 0) {
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
        public List<BoardPosition> GetLegalQueenMoves(GameState gameState, BoardPosition pos) {

            //Using the rook and bishop move checkers as they will cover all the queens movement options
            List<BoardPosition> fakeRook = GetLegalRookMoves(gameState, pos);
            List<BoardPosition> fakeBishop = GetLegalBishopMoves(gameState, pos);
            return fakeRook.Concat(fakeBishop).ToList();
        }

        //checks the legal moves if the piece is a king
        public List<BoardPosition> GetLegalKingMoves(GameState gameState, BoardPosition pos) {

            return null;
        }
    }
}