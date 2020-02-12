using System;
using System.Linq;
using System.Text;

namespace skaktego {

    public class Board {
        public int Size { get; private set; }

        private Piece[,] board;

        public static Board FromString(string s) {
            string[] rows = s.Split('/');
            int size = rows.Length;
            var board = new Board(size);
            // Start from the top (from white's perspecitve).
            for (int j = size - 1; j >= 0; j--) {
                string row = rows[size - j - 1];
                int index = 0;
                // Start from the right (from white's perspective).
                for (int i = size - 1; i >= 0; i--) {
                    char c = row[index++];
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
            StringBuilder rowStr = new StringBuilder(Size);

            string[] rowStrings = new string[Size];
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
                            rowStr.Append(emptyCells);
                            emptyCells = 0;
                        }
                        var pieceChar = piece.ToChar();
                        rowStr.Append(pieceChar);
                    }
                }
                if (emptyCells != 0) {
                    rowStr.Append(emptyCells);
                }
                rowStrings[Size - j - 1] = rowStr.ToString();
                rowStr.Clear();
            }
            return string.Join('/', rowStrings);
        }
    }

}
