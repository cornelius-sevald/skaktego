using System;
using System.Text;

namespace skaktego {

    public class Board {
        public int Size { get; private set; }

        private Piece[,] board;

        public static Board FromString(int size, string s) {
            int index = 0;
            var board = new Board(size);
            // Start from the top (from white's perspecitve).
            for (int j = size - 1; j >= 0; j--) {
                // Start from the right (from white's perspective).
                for (int i = size - 1; i >= 0; i--) {
                    char c = s[index++];
                    if (Char.IsDigit(c)) {
                        // Skip ahead if there are empty cells.
                        i -= (int)Char.GetNumericValue(c) - 1;
                    } else {
                        // Otherwise, place a piece.
                        var pos = new BoardPosition(i, j);
                        var piece = Piece.FromChar(c);
                        board.SetPiece(piece, pos);
                    }
                }
                // Skip the '/' character.
                index++;
            }

            return board;
        }

        /// <summary>
        /// Initialize a new empty board.
        /// </summary>
        /// <param name="size"></param>
        public Board(int size) {
            this.board = new Piece[size, size];
            this.Size = size;
        }

        public Board(Piece[,] pieces) {
            // TODO: Throw error if 
            int size = pieces.GetLength(0);
            this.Size = size;
            this.board = new Piece[size, size];
            Array.Copy(pieces, board, pieces.Length);
        }

        public Piece GetPiece(BoardPosition pos) {
            return board[pos.Column, pos.Row];
        }

        public Piece GetPiece(string strPos) {
            var pos = BoardPosition.FromString(strPos);
            return GetPiece(pos);
        }

        public void SetPiece(Piece piece, BoardPosition pos) {
            board[pos.Column, pos.Row] = piece;
        }

        public void SetPiece(Piece piece, string strPos) {
            var pos = BoardPosition.FromString(strPos);
            SetPiece(piece, pos);
        }

        public override string ToString() {
            // More than enough for an 8 x 8 board.
            const int boardStrCapacity = 64;
            const int rankStrCapacity = 8;
            StringBuilder boardStr = new StringBuilder(boardStrCapacity);
            StringBuilder rankStr = new StringBuilder(rankStrCapacity);

            // Start from the top (from white's perspecitve).
            for (int j = Size - 1; j >= 0; j--) {
                // Build a string for this rank.
                // Record the number of consecutive empty cells.
                int emptyCells = 0;

                // Start from the right (from white's perspective).
                for (int i = Size - 1; i >= 0; i--) {
                    var pos = new BoardPosition(i, j);
                    var piece = GetPiece(pos);
                    if (piece == null) {
                        emptyCells++;
                    } else {
                        if (emptyCells != 0) {
                            rankStr.Append(emptyCells);
                            emptyCells = 0;
                        }
                        var pieceChar = piece.ToChar();
                        rankStr.Append(pieceChar);
                    }
                }
                if (emptyCells != 0) {
                    rankStr.Append(emptyCells);
                }
                if (j != 0) {
                    rankStr.Append('/');
                }
                boardStr.Append(rankStr);
                rankStr.Clear();
            }
            return boardStr.ToString();
        }
    }

}
