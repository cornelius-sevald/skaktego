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

        public static List<BoardPosition> GetLegalMoves(GameState realState, BoardPosition pos, Piece piece) {
            GameState gameState = GameState.FromString(realState.ToString());
            List<BoardPosition> moves = GetPseudoLegalMoves(gameState, pos, piece);
            List<BoardPosition> legalMoves = new List<BoardPosition>();
            foreach (BoardPosition movePos in moves) {
                ChessMove move = new ChessMove(pos, movePos);
                GameState tempState = ApplyMove(gameState, move, false);
                tempState.player = tempState.player.Other();
                if (!IsCheck(tempState)) {
                    legalMoves.Add(movePos);
                }
            }

            if (piece != null && piece.Type == PieceTypes.King) {
                bool whiteRightRookCastle = false;
                bool whiteLeftRookCastle = false;
                bool blackRightRookCastle = false;
                bool blackLeftRookCastle = false;

                if (gameState.player == ChessColors.White && !IsCheck(gameState)) {
                    if (gameState.castling.whiteKing && pos.row == gameState.castling.whiteRightRook.row) {
                        BoardPosition here = pos;
                        whiteRightRookCastle = true;
                        for (int i = 0; i < Math.Abs(gameState.castling.whiteRightRook.column - pos.column); i++) {
                            if (pos.column < gameState.castling.whiteRightRook.column) {
                                here.column++;
                            } else {
                                here.column--;
                            }
                            if (IsTileAttacked(gameState, here) && i < 2) {
                                whiteRightRookCastle = false;
                                break;
                            }
                            if (gameState.board.IsTileOccupied(here) && here != gameState.castling.whiteRightRook) {
                                whiteRightRookCastle = false;
                                break;
                            }
                        }
                    }

                    if (gameState.castling.whiteQueen && pos.row == gameState.castling.whiteLeftRook.row) {
                        BoardPosition here = pos;
                        whiteLeftRookCastle = true;
                        for (int i = 0; i < Math.Abs(gameState.castling.whiteLeftRook.column - pos.column); i++) {
                            if (pos.column < gameState.castling.whiteLeftRook.column) {
                                here.column++;
                            } else {
                                here.column--;
                            }
                            if (IsTileAttacked(gameState, here) && i < 2) {
                                whiteLeftRookCastle = false;
                                break;
                            }
                            if (gameState.board.IsTileOccupied(here) && here != gameState.castling.whiteLeftRook) {
                                whiteLeftRookCastle = false;
                                break;
                            }
                        }
                    }
                } else if (gameState.player == ChessColors.Black && !IsCheck(gameState)) {
                    if (gameState.castling.blackKing && pos.row == gameState.castling.blackRightRook.row) {
                        BoardPosition here = pos;
                        blackRightRookCastle = true;
                        for (int i = 0; i < Math.Abs(gameState.castling.blackRightRook.column - pos.column); i++) {
                            if (pos.column < gameState.castling.blackRightRook.column) {
                                here.column++;
                            } else {
                                here.column--;
                            }
                            if (IsTileAttacked(gameState, here) && i < 2) {
                                blackRightRookCastle = false;
                                break;
                            }
                            if (gameState.board.IsTileOccupied(here) && here != gameState.castling.blackRightRook) {
                                blackRightRookCastle = false;
                                break;
                            }
                        }
                    }

                    if (gameState.castling.blackQueen && pos.row == gameState.castling.blackLeftRook.row) {
                        BoardPosition here = pos;
                        blackLeftRookCastle = true;
                        for (int i = 0; i < Math.Abs(gameState.castling.blackLeftRook.column - pos.column); i++) {
                            if (pos.column < gameState.castling.blackLeftRook.column) {
                                here.column++;
                            } else {
                                here.column--;
                            }
                            if (IsTileAttacked(gameState, here) && i < 2) {
                                blackLeftRookCastle = false;
                                break;
                            }
                            if (gameState.board.IsTileOccupied(here) && here != gameState.castling.blackLeftRook) {
                                blackLeftRookCastle = false;
                                break;
                            }
                        }
                    }
                }
                if (whiteRightRookCastle) {
                    legalMoves.Add(gameState.castling.whiteRightRook);
                }
                if (whiteLeftRookCastle) {
                    legalMoves.Add(gameState.castling.whiteLeftRook);
                }
                if (blackRightRookCastle) {
                    legalMoves.Add(gameState.castling.blackRightRook);
                }
                if (blackLeftRookCastle) {
                    legalMoves.Add(gameState.castling.blackLeftRook);
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
        /// <param name="move">The chess move to apply</param>
        /// <param name="strict">Should the moved be checked if it is legal
        /// before being applied?</param>
        /// <returns></returns>
        public static GameState ApplyMove(GameState gameState, ChessMove move, bool strict=true) {
            var newGameState = GameState.FromString(gameState.ToString());
            Piece piece = newGameState.board.GetPiece(move.from);

            if (strict) {
                // The move is illegal if it is not the current player's turn
                if (piece == null || piece.Color != newGameState.player) {
                    return gameState;
                }

                List<BoardPosition> legalMoves = GetLegalMoves(newGameState, move.from, piece);
                // If the move is not legal, do not apply it
                if (!legalMoves.Contains(move.to)) {
                    return gameState;
                }
            }

            newGameState.board.SetPiece(null, move.from);
            Piece captured = newGameState.board.CapturePiece(move.to);
            newGameState.board.SetPiece(piece, move.to);
            piece.hasMoved = true;

            // Check if a pawn is captured due to en passant
            if (move.to == newGameState.enPassant) {
                BoardPosition enPassantCapture;
                enPassantCapture.column = move.to.column;

                switch (newGameState.player) {
                    case ChessColors.Black:
                        enPassantCapture.row = move.to.row + 1;
                        break;
                    default:
                        enPassantCapture.row = move.to.row - 1;
                        break;
                }

                captured = newGameState.board.CapturePiece(enPassantCapture);
            }

            // Update en passant
            if (piece.Type == PieceTypes.Pawn && Math.Abs(move.from.row - move.to.row) == 2) {
                newGameState.enPassant = new BoardPosition(move.from.column, (move.from.row + move.to.row) / 2);
            } else {
                newGameState.enPassant = null;
            }

            //promote a pawn
            if (piece.Type == PieceTypes.Pawn && (move.to.row == 0 || move.to.row == gameState.board.Size - 1)) {
                piece.Promote(PieceTypes.Queen);
            }

            if (piece.Type == PieceTypes.King) {
                switch(newGameState.player) {
                    case ChessColors.Black:
                        newGameState.castling.blackKing = false;
                        newGameState.castling.blackQueen = false;
                        break;

                    default:
                        newGameState.castling.whiteKing = false;
                        newGameState.castling.whiteQueen = false;
                        break;
                    
                }
            }

            if (piece.Type == PieceTypes.Rook) {
                if (newGameState.player == ChessColors.White) {
                    if (move.from == newGameState.castling.whiteLeftRook) {
                        newGameState.castling.whiteQueen = false;
                    }
                    if (move.from == newGameState.castling.whiteRightRook) {
                        newGameState.castling.whiteKing = false;
                    }
                }
                if (newGameState.player == ChessColors.Black) {
                    if (move.from == newGameState.castling.blackLeftRook) {
                        newGameState.castling.blackQueen = false;
                    }
                    if (move.from == newGameState.castling.blackRightRook) {
                        newGameState.castling.blackKing = false;
                    }
                }
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

            //The king can have a maximum of 8 possible moves without castling
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

        public static bool IsCheck(GameState realState) {
            GameState gameState = GameState.FromString(realState.ToString());
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
        public static bool IsTie(GameState realState) {
            GameState gameState = GameState.FromString(realState.ToString());

            //50 moves without capture or moving a pawn
            if(gameState.halfmoveClock >= 50) {
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
                return true;
            }

            //if there are no legal moves for the current player
            List<BoardPosition> legalMoves = GetAllLegalMoves(gameState);
            if (legalMoves.Count == 0) {
                return true;
            }
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
