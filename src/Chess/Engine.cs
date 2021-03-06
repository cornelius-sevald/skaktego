using System;
using System.Linq;
using System.Collections.Generic;

namespace skaktego.Chess {

    /// <summary>
    /// The enigne class is a collection of functions which handles all the chess logic and rules
    /// </summary>
    public static class Engine {

        /// <summary>
        /// Find the legal moves for every piece of the current players turn
        /// </summary>
        /// <para>
        /// As opposed to pseudo-legal moves, this also ensures
        /// the king does not move into check, and considers castling.
        /// </para>
        /// <param name="gameState">
        /// The state of the game
        /// </param>
        /// <returns>
        /// All possible moves for the current player
        /// </returns>
        public static List<ChessMove> GetAllLegalMoves(GameState gameState) {
            List<ChessMove> allMoves = new List<ChessMove>();
            for (int i = 0; i < gameState.board.Size; i++) {
                for (int j = 0; j < gameState.board.Size; j++) {
                    BoardPosition here = new BoardPosition(i, j);
                    List<BoardPosition> legalMoves = GetLegalMoves(gameState, here);
                    foreach (BoardPosition legalMove in legalMoves) {
                        ChessMove move = new ChessMove(here, legalMove);
                        allMoves.Add(move);
                    }
                }
            }
            return allMoves;
        }

        /// <summary>
        /// Find all legal moves for the piece at this position
        /// </summary>
        /// <para>
        /// As opposed to pseudo-legal moves, this also ensures
        /// the king does not move into check, and considers castling.
        /// </para>
        /// <param name="gameState">
        /// The state of the game
        /// </param>
        /// <param name="pos">
        /// The position of the chess piece selected
        /// </param>
        /// <returns>
        /// All possible, legal moves for the piece on the selected position
        /// </returns>
        public static List<BoardPosition> GetLegalMoves(GameState gameState, BoardPosition pos) {
            Piece piece = gameState.board.GetPiece(pos);
            return GetLegalMoves(gameState, pos, piece);
        }

        /// <summary>
        /// Get all legal moves for the piece at that position
        /// </summary>
        /// <para>
        /// As opposed to pseudo-legal moves, this also ensures
        /// the king does not move into check, and considers castling.
        /// </para>
        /// <param name="gameState">
        /// The state of the game
        /// </param>
        /// <param name="pos">
        /// The position of the chess piece selected, from 0-7 in both axis
        /// </param>
        /// <param name="piece">
        /// The piece in the selected position
        /// </param>
        /// <returns>
        /// All possible, legal moves for the piece on the selected position
        /// </returns>
        public static List<BoardPosition> GetLegalMoves(GameState realState, BoardPosition pos, Piece piece) {
            // If in the preperation phase of skaktego, other moves are legal
            if (realState.gameType == GameTypes.SkaktegoPrep) {
                return GetSkaktegoPrepMoves(realState, pos, piece);
            }

            // Otherwise it is just like normal chess
            GameState gameState = GameState.FromString(realState.ToString());
            List<BoardPosition> moves = GetPseudoLegalMoves(gameState, pos, piece);
            List<BoardPosition> legalMoves = new List<BoardPosition>();

            // When playing skaktego, it is not illegal to put yourself in check
            if (gameState.gameType == GameTypes.Skaktego) {
                legalMoves = moves;
            } else {
                foreach (BoardPosition movePos in moves) {
                    ChessMove move = new ChessMove(pos, movePos);
                    GameState tempState = ApplyMove(gameState, move, false);
                    tempState.player = tempState.player.Other();
                    if (!IsCheck(tempState)) {
                        legalMoves.Add(movePos);
                    }
                }
            }

            // Checks for castling avaliablity
            if (piece != null && piece.Type == PieceTypes.King) {
                bool whiteRightRookCastle = false;
                bool whiteLeftRookCastle = false;
                bool blackRightRookCastle = false;
                bool blackLeftRookCastle = false;

                if (gameState.player == ChessColors.White && !IsCheck(gameState)) {
                    if (gameState.castling.whiteKing && pos.row == gameState.castling.whiteRightRook.row) {
                        BoardPosition here = pos;
                        whiteRightRookCastle = true;
                        if ((gameState.castling.whiteRightRook.column == 0 &&
                             gameState.castling.whiteRightRook.column - here.column == -1) ||
                            (gameState.castling.whiteRightRook.column == 7 &&
                            gameState.castling.whiteRightRook.column - here.column == 1)
                        ) {
                                whiteRightRookCastle = false;
                        }

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
                        if (pos.column + 1 == gameState.castling.whiteRightRook.column && whiteRightRookCastle) {
                            Piece kingPosPiece = gameState.board.GetPiece(new BoardPosition(pos.column + 2, pos.row));
                            if (kingPosPiece != null && kingPosPiece.Color == gameState.player) {
                                whiteRightRookCastle = false;
                            }
                        } else if (pos.column - 1 == gameState.castling.whiteRightRook.column && whiteRightRookCastle) {
                            Piece kingPosPiece = gameState.board.GetPiece(new BoardPosition(pos.column - 2, pos.row));
                            if (kingPosPiece != null && kingPosPiece.Color == gameState.player) {
                                whiteRightRookCastle = false;
                            }
                        }
                    }

                    if (gameState.castling.whiteQueen && pos.row == gameState.castling.whiteLeftRook.row) {
                        BoardPosition here = pos;
                        whiteLeftRookCastle = true;
                        if ((gameState.castling.whiteLeftRook.column == 0 &&
                             gameState.castling.whiteLeftRook.column - here.column == -1) ||
                            (gameState.castling.whiteLeftRook.column == 7 &&
                            gameState.castling.whiteLeftRook.column - here.column == 1)
                        ) {
                                whiteLeftRookCastle = false;
                        }
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
                        if (pos.column + 1 == gameState.castling.whiteLeftRook.column && whiteLeftRookCastle) {
                            Piece kingPosPiece = gameState.board.GetPiece(new BoardPosition(pos.column + 2, pos.row));
                            if (kingPosPiece != null && kingPosPiece.Color == gameState.player) {
                                whiteLeftRookCastle = false;
                            }
                        } else if (pos.column - 1 == gameState.castling.whiteLeftRook.column && whiteLeftRookCastle) {
                            Piece kingPosPiece = gameState.board.GetPiece(new BoardPosition(pos.column - 2, pos.row));
                            if (kingPosPiece != null && kingPosPiece.Color == gameState.player) {
                                whiteLeftRookCastle = false;
                            }
                        }
                    }
                } else if (gameState.player == ChessColors.Black && !IsCheck(gameState)) {
                    if (gameState.castling.blackKing && pos.row == gameState.castling.blackRightRook.row) {
                        BoardPosition here = pos;
                        blackRightRookCastle = true;
                        if ((gameState.castling.blackRightRook.column == 0 &&
                             gameState.castling.blackRightRook.column - here.column == -1) ||
                            (gameState.castling.blackRightRook.column == 7 &&
                            gameState.castling.blackRightRook.column - here.column == 1)
                        ) {
                                blackRightRookCastle = false;
                        }
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
                        if (pos.column + 1 == gameState.castling.blackRightRook.column && blackRightRookCastle) {
                            Piece kingPosPiece = gameState.board.GetPiece(new BoardPosition(pos.column + 2, pos.row));
                            if (kingPosPiece != null && kingPosPiece.Color == gameState.player) {
                                blackRightRookCastle = false;
                            }
                        } else if (pos.column - 1 == gameState.castling.blackRightRook.column && blackRightRookCastle) {
                            Piece kingPosPiece = gameState.board.GetPiece(new BoardPosition(pos.column - 2, pos.row));
                            if (kingPosPiece != null && kingPosPiece.Color == gameState.player) {
                                blackRightRookCastle = false;
                            }
                        }
                    }

                    if (gameState.castling.blackQueen && pos.row == gameState.castling.blackLeftRook.row) {
                        BoardPosition here = pos;
                        blackLeftRookCastle = true;
                        if ((gameState.castling.blackLeftRook.column == 0 &&
                             gameState.castling.blackLeftRook.column - here.column == -1) ||
                            (gameState.castling.blackLeftRook.column == 7 &&
                            gameState.castling.blackLeftRook.column - here.column == 1)
                        ) {
                                blackLeftRookCastle = false;
                        }
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
                        if (pos.column + 1 == gameState.castling.blackLeftRook.column && blackLeftRookCastle) {
                            Piece kingPosPiece = gameState.board.GetPiece(new BoardPosition(pos.column + 2, pos.row));
                            if (kingPosPiece != null && kingPosPiece.Color == gameState.player) {
                                blackLeftRookCastle = false;
                            }
                        } else if (pos.column - 1 == gameState.castling.blackLeftRook.column && blackLeftRookCastle) {
                            Piece kingPosPiece = gameState.board.GetPiece(new BoardPosition(pos.column - 2, pos.row));
                            if (kingPosPiece != null && kingPosPiece.Color == gameState.player) {
                                blackLeftRookCastle = false;
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

        /// <summary>
        /// Find all pseudo-legal moves of the piece located at the given position
        /// </summary>
        /// <seealso cref="GetLegalMoves"/>
        /// <param name="gameState">
        /// The state of the game
        /// </param>
        /// <param name="pos">
        /// The position of the chess piece selected
        /// </param>
        /// <returns>
        /// All possible moves for the piece on the selected position
        /// </returns>
        private static List<BoardPosition> GetPseudoLegalMoves(GameState gameState, BoardPosition pos) {
            Piece piece = gameState.board.GetPiece(pos);
            return GetPseudoLegalMoves(gameState, pos, piece);
        }

        /// <summary>
        /// Find all pseudo-legal moves for the given piece at the given position
        /// </summary>
        /// <seealso cref="GetLegalMoves"/>
        /// <param name="gameState">
        /// The state of the game
        /// </param>
        /// <param name="pos">
        /// The position of the chess piece selected
        /// </param>
        /// <param name="piece">
        /// The piece in the selected position
        /// </param>
        /// <returns>
        /// All possible moves for the piece on the selected position
        /// </returns>
        private static List<BoardPosition> GetPseudoLegalMoves(GameState gameState, BoardPosition pos, Piece piece) {
            // There are no pseudo-legal moves, if there is no piece
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

                case PieceTypes.Unknown:
                    return new List<BoardPosition>();

                default:
                    throw new ArgumentException("Unknown piece type " + piece.Type);

            }
        }

        /// <summary>
        /// Applies the given move and gives the turn to the opponent
        /// while advancing the game state
        /// </summary>
        /// <param name="gameState">
        /// The state of the game
        /// </param>
        /// <param name="move">
        /// The move to apply
        /// </param>
        /// <param name="strict">
        /// Should the moved be checked if it is legal before being applied?
        /// </param>
        /// <returns>
        /// A new gamestate where the move have been made
        /// </returns>
        public static GameState ApplyMove(GameState gameState, ChessMove move, bool strict = true) {
            // If the move is out of bounds, ignore it
            if (move.from.column < 0 || move.from.row < 0 ||
                move.from.column >= gameState.board.Size || move.from.row >= gameState.board.Size ||
                move.to.column < 0 || move.to.row < 0 ||
                move.to.column >= gameState.board.Size || move.to.row >= gameState.board.Size) {
                return gameState;
            }

            // If in the preperation phase of skaktego, pieces move differently
            if (gameState.gameType == GameTypes.SkaktegoPrep) {
                return ApplySkaktegoPrepMove(gameState, move, strict);
            }

            Piece piece = gameState.board.GetPiece(move.from);

            // If the move is useless, or no piece is selected return early
            if (move.from == move.to || piece == null) {
                return gameState;
            }

            var newGameState = GameState.FromString(gameState.ToString());
            if (strict) {
                // The move is illegal if it is not the current player's turn
                if (piece.Color != newGameState.player) {
                    return gameState;
                }

                List<BoardPosition> legalMoves = GetLegalMoves(newGameState, move.from, piece);
                // If the move is not legal, do not apply it
                if (!legalMoves.Contains(move.to)) {
                    return gameState;
                }
            }
            Piece captured = null;

            // Do the actual move if the piece is not a king - more advanced king movement under castling
            if (piece.Type != PieceTypes.King) {
                newGameState.board.SetPiece(null, move.from);
                captured = newGameState.board.CapturePiece(move.to);
                newGameState.board.SetPiece(piece, move.to);
                piece.hasMoved = true;
            }

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

            // Update en passant location
            if (piece.Type == PieceTypes.Pawn && Math.Abs(move.from.row - move.to.row) == 2) {
                newGameState.enPassant = new BoardPosition(move.from.column, (move.from.row + move.to.row) / 2);
            } else {
                newGameState.enPassant = null;
            }

            // Promote a pawn if on the last row
            if (piece.Type == PieceTypes.Pawn && (move.to.row == 0 || move.to.row == gameState.board.Size - 1)) {
                piece.Promote(PieceTypes.Queen);
            }

            // Handles the kings moves, either if castling or moving normally
            if (piece.Type == PieceTypes.King) {
                BoardPosition newKingPosition;

                if (move.to == gameState.castling.whiteLeftRook && gameState.castling.whiteQueen) {
                    //castling to the white king side
                    if (move.from.column < gameState.castling.whiteLeftRook.column) {
                        newKingPosition = new BoardPosition(move.from.column + 2, move.from.row);
                        Piece rook = newGameState.board.GetPiece(gameState.castling.whiteLeftRook);
                        newGameState.board.SetPiece(null, gameState.castling.whiteLeftRook);
                        newGameState.board.SetPiece(rook, new BoardPosition(move.from.column + 1, move.from.row));
                    } else {
                        newKingPosition = new BoardPosition(move.from.column - 2, move.from.row);
                        Piece rook = newGameState.board.GetPiece(gameState.castling.whiteLeftRook);
                        newGameState.board.SetPiece(null, gameState.castling.whiteLeftRook);
                        newGameState.board.SetPiece(rook, new BoardPosition(move.from.column - 1, move.from.row));
                    }

                } else if (move.to == gameState.castling.whiteRightRook && gameState.castling.whiteKing) {
                    //castling to the white queen side
                    if (move.from.column < gameState.castling.whiteRightRook.column) {
                        newKingPosition = new BoardPosition(move.from.column + 2, move.from.row);
                        Piece rook = newGameState.board.GetPiece(gameState.castling.whiteRightRook);
                        newGameState.board.SetPiece(null, gameState.castling.whiteRightRook);
                        newGameState.board.SetPiece(rook, new BoardPosition(move.from.column + 1, move.from.row));
                    } else {
                        newKingPosition = new BoardPosition(move.from.column - 2, move.from.row);
                        Piece rook = newGameState.board.GetPiece(gameState.castling.whiteRightRook);
                        newGameState.board.SetPiece(null, gameState.castling.whiteRightRook);
                        newGameState.board.SetPiece(rook, new BoardPosition(move.from.column - 1, move.from.row));
                    }

                } else if (move.to == gameState.castling.blackLeftRook && gameState.castling.blackQueen) {
                    //castling to the black king side
                    if (move.from.column < gameState.castling.blackLeftRook.column) {
                        newKingPosition = new BoardPosition(move.from.column + 2, move.from.row);
                        Piece rook = newGameState.board.GetPiece(gameState.castling.blackLeftRook);
                        newGameState.board.SetPiece(null, gameState.castling.blackLeftRook);
                        newGameState.board.SetPiece(rook, new BoardPosition(move.from.column + 1, move.from.row));
                    } else {
                        newKingPosition = new BoardPosition(move.from.column - 2, move.from.row);
                        Piece rook = newGameState.board.GetPiece(gameState.castling.blackLeftRook);
                        newGameState.board.SetPiece(null, gameState.castling.blackLeftRook);
                        newGameState.board.SetPiece(rook, new BoardPosition(move.from.column - 1, move.from.row));
                    }

                } else if (move.to == gameState.castling.blackRightRook && gameState.castling.blackKing) {
                    //castling to the black queen side
                    if (move.from.column < gameState.castling.blackRightRook.column) {
                        newKingPosition = new BoardPosition(move.from.column + 2, move.from.row);
                        Piece rook = newGameState.board.GetPiece(gameState.castling.blackRightRook);
                        newGameState.board.SetPiece(null, gameState.castling.blackRightRook);
                        newGameState.board.SetPiece(rook, new BoardPosition(move.from.column + 1, move.from.row));
                    } else {
                        newKingPosition = new BoardPosition(move.from.column - 2, move.from.row);
                        Piece rook = newGameState.board.GetPiece(gameState.castling.blackRightRook);
                        newGameState.board.SetPiece(null, gameState.castling.blackRightRook);
                        newGameState.board.SetPiece(rook, new BoardPosition(move.from.column - 1, move.from.row));
                    }

                } else {
                    newKingPosition = move.to;
                }
                captured = newGameState.board.CapturePiece(newKingPosition);
                newGameState.board.SetPiece(piece, newKingPosition);
                newGameState.board.SetPiece(null, move.from);
                piece.hasMoved = true;
            }

            //remove option of castling if the king is moved
            if (piece.Type == PieceTypes.King) {
                switch (newGameState.player) {
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

            //remove option of castling if the rook is moved
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

            // If a piece was captured, record it
            if (captured != null) {
                newGameState.taken.Add(captured);
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

            // Change the current players turn to the opponent
            newGameState.player = newGameState.player.Other();

            return newGameState;
        }

        /// <summary>
        /// Find all pseudo-legal moves for a pawn at this position
        /// </summary>
        /// <param name="gameState">
        /// The state of the game
        /// </param>
        /// <param name="pos">
        /// The position of the chess piece
        /// </param>
        /// <param name="piece">
        /// The piece. This is used to check if the pawn has moved before.
        /// </param>
        /// <returns>
        /// A list of possible moves for the given pawn
        /// </returns>
        private static List<BoardPosition> GetPseudoLegalPawnMoves(GameState gameState, BoardPosition pos, Piece piece) {
            Stack<BoardPosition> possibleMoves = new Stack<BoardPosition>();

            // Checks pseudo-legal moves without capture for the white pawn
            BoardPosition here = pos;
            if (gameState.player == ChessColors.White) {
                here.row++;
                if (here.row < gameState.board.Size && !gameState.board.IsTileOccupied(here)) {
                    possibleMoves.Push(here);
                    if (!piece.hasMoved) {
                        here.row++;
                        if (here.row < gameState.board.Size && !gameState.board.IsTileOccupied(here)) {
                            possibleMoves.Push(here);
                        }
                    }
                }
            }

            // Checks pseudo-legal moves without capture for the black pawn
            if (gameState.player == ChessColors.Black) {
                here = pos;
                here.row--;
                if (here.row >= 0 && !gameState.board.IsTileOccupied(here)) {
                    possibleMoves.Push(here);
                    if (!piece.hasMoved) {
                        here.row--;
                        if (here.row >= 0 && !gameState.board.IsTileOccupied(here)) {
                            possibleMoves.Push(here);
                        }
                    }
                }
            }

            // Checks pseudo-legal captures for the white pawn
            if (gameState.player == ChessColors.White) {
                here = pos;
                here.row++;
                here.column++;
                if (here.row < gameState.board.Size && here.column < gameState.board.Size) {
                    if (gameState.board.IsTileOccupied(here) && gameState.board.GetPiece(here).Color != gameState.player) {
                        possibleMoves.Push(here);
                    } else if (gameState.enPassant.HasValue && gameState.enPassant.Value == here) {
                        possibleMoves.Push(here);
                    }
                }
                here.column -= 2;
                if (here.row < gameState.board.Size && here.column >= 0) {
                    if (gameState.board.IsTileOccupied(here) && gameState.board.GetPiece(here).Color != gameState.player) {
                        possibleMoves.Push(here);
                    } else if (gameState.enPassant.HasValue && gameState.enPassant.Value == here) {
                        possibleMoves.Push(here);
                    }
                }
            }

            // Checks pseudo-legal captures for the black pawn
            if (gameState.player == ChessColors.Black) {
                here = pos;
                here.row--;
                here.column++;
                if (here.row >= 0 && here.column < gameState.board.Size) {
                    if (gameState.board.IsTileOccupied(here) && gameState.board.GetPiece(here).Color != gameState.player) {
                        possibleMoves.Push(here);
                    } else if (gameState.enPassant.HasValue && gameState.enPassant.Value == here) {
                        possibleMoves.Push(here);
                    }
                }
                here.column -= 2;
                if (here.row >= 0 && here.column >= 0) {
                    if (gameState.board.IsTileOccupied(here) && gameState.board.GetPiece(here).Color != gameState.player) {
                        possibleMoves.Push(here);
                    } else if (gameState.enPassant.HasValue && gameState.enPassant.Value == here) {
                        possibleMoves.Push(here);
                    }
                }
            }

            return new List<BoardPosition>(possibleMoves);
        }


        /// <summary>
        /// Find all pseudo-legal moves for a rook at this position
        /// </summary>
        /// <param name="gameState">
        /// The state of the game
        /// </param>
        /// <param name="pos">
        /// The position of the chess piece
        /// </param>
        /// <returns>
        /// A list of possible moves for the given rook
        /// </returns>
        private static List<BoardPosition> GetPseudoLegalRookMoves(GameState gameState, BoardPosition pos) {
            Stack<BoardPosition> possibleMoves = new Stack<BoardPosition>();

            // Checks pseudo-legal moves in the positive vertical direction from the piece's position
            BoardPosition here = pos;
            while (true) {
                here.row++;
                if (here.row >= gameState.board.Size) {
                    break;
                }
                possibleMoves.Push(here);
                if (gameState.board.IsTileOccupied(here)) {
                    if (gameState.board.GetPiece(here).Color == gameState.player) {
                        possibleMoves.Pop();
                    }
                    break;
                }
            }

            // Checks pseudo-legal moves in the negative vertical direction from the piece's position
            here = pos;
            while (true) {
                here.row--;
                if (here.row < 0) {
                    break;
                }
                possibleMoves.Push(here);
                if (gameState.board.IsTileOccupied(here)) {
                    if (gameState.board.GetPiece(here).Color == gameState.player) {
                        possibleMoves.Pop();
                    }
                    break;
                }
            }

            // Checks pseudo-legal moves in the positive horizontal direction from the piece's position
            here = pos;
            while (true) {
                here.column++;
                if (here.column >= gameState.board.Size) {
                    break;
                }
                possibleMoves.Push(here);
                if (gameState.board.IsTileOccupied(here)) {
                    if (gameState.board.GetPiece(here).Color == gameState.player) {
                        possibleMoves.Pop();
                    }
                    break;
                }
            }

            // Checks pseudo-legal moves in the negative horizontal direction from the piece's position
            here = pos;
            while (true) {
                here.column--;
                if (here.column < 0) {
                    break;
                }
                possibleMoves.Push(here);
                if (gameState.board.IsTileOccupied(here)) {
                    if (gameState.board.GetPiece(here).Color == gameState.player) {
                        possibleMoves.Pop();
                    }
                    break;
                }
            }
            return new List<BoardPosition>(possibleMoves);
        }

        /// <summary>
        /// Find all pseudo-legal moves for a knight at this position
        /// </summary>
        /// <param name="gameState">
        /// The state of the game
        /// </param>
        /// <param name="pos">
        /// The position of the chess piece
        /// </param>
        /// <returns>
        /// A list of possible moves for the given knight
        /// </returns>
        private static List<BoardPosition> GetPseudoLegalKnightMoves(GameState gameState, BoardPosition pos) {
            Stack<BoardPosition> possibleMoves = new Stack<BoardPosition>();

            //The knight can have a maximum of 8 possible moves, and the only factor is if the move is outside the board or is occupied by the players own piece
            BoardPosition here = pos;
            for (int i = 1; i < 9; i++) {
                here = pos;
                here.row += (int)Math.Round(2 * Math.Sin(i * 0.8 + 1));
                here.column += (int)Math.Round(2 * Math.Sin(i * 0.8 - 0.5)); ;
                possibleMoves.Push(here);
                if (here.row >= gameState.board.Size || here.row < 0 || here.column >= gameState.board.Size || here.column < 0) {
                    possibleMoves.Pop();
                } else if (gameState.board.IsTileOccupied(here) && gameState.board.GetPiece(here).Color == gameState.player) {
                    possibleMoves.Pop();
                }

            }

            return new List<BoardPosition>(possibleMoves);
        }

        /// <summary>
        /// Find all pseudo-legal moves for a bishop at this position
        /// </summary>
        /// <param name="gameState">
        /// The state of the game
        /// </param>
        /// <param name="pos">
        /// The position of the chess piece
        /// </param>
        /// <returns>
        /// A list of possible moves for the given bishop
        /// </returns>
        private static List<BoardPosition> GetPseudoLegalBishopMoves(GameState gameState, BoardPosition pos) {
            Stack<BoardPosition> possibleMoves = new Stack<BoardPosition>();

            // Checks pseudo-legal moves in the positive vertical and horizontal direction from the piece's position
            BoardPosition here = pos;
            while (true) {
                here.row++;
                here.column++;
                if (here.row >= gameState.board.Size || here.column >= gameState.board.Size) {
                    break;
                }
                possibleMoves.Push(here);
                if (gameState.board.IsTileOccupied(here)) {
                    if (gameState.board.GetPiece(here).Color == gameState.player) {
                        possibleMoves.Pop();
                    }
                    break;
                }
            }

            // Checks pseudo-legal moves in the negative vertical and positive horizontal direction from the piece's position
            here = pos;
            while (true) {
                here.row--;
                here.column++;
                if (here.row < 0 || here.column >= gameState.board.Size) {
                    break;
                }
                possibleMoves.Push(here);
                if (gameState.board.IsTileOccupied(here)) {
                    if (gameState.board.GetPiece(here).Color == gameState.player) {
                        possibleMoves.Pop();
                    }
                    break;
                }
            }

            // Checks pseudo-legal moves in the negative vertical and horizontal direction from the piece's position
            here = pos;
            while (true) {
                here.row--;
                here.column--;
                if (here.row < 0 || here.column < 0) {
                    break;
                }
                possibleMoves.Push(here);
                if (gameState.board.IsTileOccupied(here)) {
                    if (gameState.board.GetPiece(here).Color == gameState.player) {
                        possibleMoves.Pop();
                    }
                    break;
                }
            }

            // Checks pseudo-legal moves in the positive vertical and negative horizontal direction from the piece's position
            here = pos;
            while (true) {
                here.row++;
                here.column--;
                if (here.row >= gameState.board.Size || here.column < 0) {
                    break;
                }
                possibleMoves.Push(here);
                if (gameState.board.IsTileOccupied(here)) {
                    if (gameState.board.GetPiece(here).Color == gameState.player) {
                        possibleMoves.Pop();
                    }
                    break;
                }
            }
            return new List<BoardPosition>(possibleMoves);
        }

        /// <summary>
        /// Find all pseudo-legal moves for a queen at this position
        /// </summary>
        /// <param name="gameState">
        /// The state of the game
        /// </param>
        /// <param name="pos">
        /// The position of the chess piece
        /// </param>
        /// <returns>
        /// A list of possible moves for the given queen
        /// </returns>
        private static List<BoardPosition> GetPseudoLegalQueenMoves(GameState gameState, BoardPosition pos) {

            //Using the rook and bishop move checkers as they will cover all the queens movement options
            List<BoardPosition> fakeRook = GetPseudoLegalRookMoves(gameState, pos);
            List<BoardPosition> fakeBishop = GetPseudoLegalBishopMoves(gameState, pos);
            return fakeRook.Concat(fakeBishop).ToList();
        }

        /// <summary>
        /// Find all pseudo-legal moves for a king at this position
        /// </summary>
        /// <param name="gameState">
        /// The state of the game
        /// </param>
        /// <param name="pos">
        /// The position of the chess piece
        /// </param>
        /// <returns>
        /// A list of possible moves for the given king
        /// </returns>
        private static List<BoardPosition> GetPseudoLegalKingMoves(GameState gameState, BoardPosition pos) {

            Stack<BoardPosition> possibleMoves = new Stack<BoardPosition>();

            //The king can have a maximum of 8 possible moves without castling
            BoardPosition here = pos;
            for (int i = 1; i < 9; i++) {
                here = pos;
                here.row += (int)Math.Round(Math.Sin(i * 0.8 + 0.6));
                here.column += (int)Math.Round(Math.Sin(i * 0.8 - 0.7)); ;
                possibleMoves.Push(here);
                if (here.row >= gameState.board.Size || here.row < 0 || here.column >= gameState.board.Size || here.column < 0) {
                    possibleMoves.Pop();
                } else if (gameState.board.IsTileOccupied(here) && gameState.board.GetPiece(here).Color == gameState.player) {
                    possibleMoves.Pop();
                }

            }

            return new List<BoardPosition>(possibleMoves);
        }

        /// <summary>
        /// Find all moves available in the preperation phase of skaktego
        /// </summary>
        /// <para>
        /// This is the two rows 'closest' to the player excluding <c>pos</c>
        /// </para>
        /// <param name="gameState">The state of the game</param>
        /// <param name="pos">The position to move from</param>
        /// <param name="piece">The piece at the selected position</param>
        /// <returns>All available preperation moves</returns>
        private static List<BoardPosition> GetSkaktegoPrepMoves(GameState gameState, BoardPosition pos, Piece piece) {
            List<BoardPosition> possibleMoves = new List<BoardPosition>();

            if (piece == null || piece.Color != gameState.player) {
                return possibleMoves;
            }

            if (gameState.player == ChessColors.White) {
                for (int i = 0; i < gameState.board.Size; i++) {
                    for (int j = 0; j < 2; j++) {
                        BoardPosition here = new BoardPosition(i, j);
                        if (here == pos) {
                            continue;
                        }
                        possibleMoves.Add(here);
                    }
                }
            } else {
                for (int i = 0; i < gameState.board.Size; i++) {
                    for (int j = 0; j < 2; j++) {
                        BoardPosition here = new BoardPosition(i, gameState.board.Size - j - 1);
                        if (here == pos) {
                            continue;
                        }
                        possibleMoves.Add(here);
                    }
                }
            }

            return possibleMoves;
        }

        /// <summary>
        /// Swap the two pieces at moveing to / from
        /// and update castling info
        /// </summary>
        /// <param name="gameState">The state of the game</param>
        /// <param name="move">The move to apply</param>
        /// <param name="strict">Should the moved be checked if it is legal before being applied?</param>
        /// <returns>A new gamestate where the move have been made</returns>
        public static GameState ApplySkaktegoPrepMove(GameState gameState, ChessMove move, bool strict = true) {
            var newGameState = GameState.FromString(gameState.ToString());
            Piece fromPiece = newGameState.board.GetPiece(move.from);

            if (strict) {
                // The move is illegal if it is not the current player's turn
                if (fromPiece == null || fromPiece.Color != newGameState.player) {
                    return gameState;
                }

                List<BoardPosition> legalMoves = GetSkaktegoPrepMoves(newGameState, move.from, fromPiece);
                // If the move is not legal, do not apply it
                if (!legalMoves.Contains(move.to)) {
                    return gameState;
                }
            }

            Piece toPiece = newGameState.board.GetPiece(move.to);
            newGameState.board.SetPiece(fromPiece, move.to);
            newGameState.board.SetPiece(toPiece, move.from);

            // Update castling info
            BoardPosition leftRook;
            BoardPosition rightRook;
            switch (newGameState.player) {
                case ChessColors.Black:
                    leftRook = newGameState.castling.blackLeftRook;
                    rightRook = newGameState.castling.blackRightRook;
                    break;
                default:
                    leftRook = newGameState.castling.whiteLeftRook;
                    rightRook = newGameState.castling.whiteRightRook;
                    break;
            }
            if (fromPiece.Type == PieceTypes.Rook && toPiece.Type == PieceTypes.Rook) {
                // No need to do anything, the rooks have switched places
            } else if (fromPiece.Type == PieceTypes.Rook) {
                if (move.from == leftRook) {
                    leftRook = move.to;
                } else {
                    rightRook = move.to;
                }
            } else if (toPiece.Type == PieceTypes.Rook) {
                if (move.to == leftRook) {
                    leftRook = move.from;
                } else {
                    rightRook = move.from;
                }
            }
            // Check if `leftRook` is still to the left of `rightRook`
            if (leftRook.column > rightRook.column) {
                BoardPosition tmpPos = leftRook;
                leftRook = rightRook;
                rightRook = tmpPos;
            }
            switch (newGameState.player) {
                case ChessColors.Black:
                    newGameState.castling.blackLeftRook = leftRook;
                    newGameState.castling.blackRightRook = rightRook;
                    break;
                default:
                    newGameState.castling.whiteLeftRook = leftRook;
                    newGameState.castling.whiteRightRook = rightRook;
                    break;
            }

            return newGameState;
        }

        /// <summary>
        /// Check if the given tile is attacked by any enemy piece
        /// </summary>
        /// <param name="gameState">
        /// The state of the game
        /// </param>
        /// <param name="pos">
        /// A position on the board
        /// </param>
        /// <returns>
        /// True if the tile is attacked and false if it is not
        /// </returns>
        private static bool IsTileAttacked(GameState gameState, BoardPosition pos) {
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

        /// <summary>
        /// Check if the player's king is in check
        /// </summary>
        /// <param name="realState">
        /// The state of the game
        /// </param>
        /// <returns>
        /// True if the king is in check, false if it is not
        /// </returns>
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
                if (kingFound) {
                    break;
                }
            }
            if (!kingFound) {
                return false;
            }
            return IsTileAttacked(gameState, kingPos);
        }

        /// <summary>
        /// Does not actually check if the game is a tie,
        /// but instead checks if the game is over.
        /// </summary>
        /// <para>
        /// To check if the game actually is a tie,
        /// use <c>IsTie AND (NOT IsCheck)</c>
        /// </para>
        /// <seealso cref="IsGameOver"/>
        /// <param name="realState">
        /// The state of the game
        /// </param>
        public static bool IsTie(GameState realState) {
            GameState gameState = GameState.FromString(realState.ToString());

            //50 moves without capture or moving a pawn
            if (gameState.halfmoveClock >= 50) {
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
            List<ChessMove> legalMoves = GetAllLegalMoves(gameState);
            if (legalMoves.Count == 0) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check if the game is a checkmate
        /// by checking if the game is a tie and the king is in check
        /// </summary>
        /// <para>
        /// This is technically incorrect, as a tie due to
        /// the fifty-move rule might still result in checkmate
        /// </para>
        /// <param name="gameState">
        /// The state of the game
        /// </param>
        /// <returns>
        /// True if the king is in check mate
        /// </returns>
        public static bool IsCheckmate(GameState gameState) {
            return IsCheck(gameState) && IsTie(gameState);
        }

        /// <summary>
        /// Check if the game is over
        /// </summary>
        /// <para>
        /// This function is equivelant to <c>IsTie</c>
        /// </para>
        /// <seealso cref="IsTie"/>
        /// <param name="gameState"></param>
        /// <returns>True if the game is over</returns>
        public static bool IsGameOver(GameState gameState) {
            if (gameState.gameType == GameTypes.Skaktego) {
                return gameState.taken.Any(p => p.Type == PieceTypes.King);
            }
            return IsTie(gameState);
        }
    }
}
