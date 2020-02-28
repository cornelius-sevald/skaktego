using System;
using System.Linq;
using System.Text;

namespace skaktego.Chess {

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
                // Start from the left (from white's perspective).
                for (int i = 0; i < size; i++) {
                    char c = row[index++];
                    if (Char.IsDigit(c)) {
                        // Skip ahead if there are empty cells.
                        i += (int)Char.GetNumericValue(c) - 1;
                    } else {
                        // Otherwise, place a piece.
                        var pos = new BoardPosition(i, j);
                        var piece = Piece.FromChar(c);
                        board.SetPiece(piece, pos);
                    }
                }
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
            return board[pos.column, pos.row];
        }

        public Piece GetPiece(string strPos) {
            var pos = BoardPosition.FromString(strPos);
            return GetPiece(pos);
        }

        public void SetPiece(Piece piece, BoardPosition pos) {
            board[pos.column, pos.row] = piece;
        }

        public void SetPiece(Piece piece, string strPos) {
            var pos = BoardPosition.FromString(strPos);
            SetPiece(piece, pos);
        }

        public Piece CapturePiece(BoardPosition pos) {
            var captured = GetPiece(pos);
            SetPiece(null, pos);
            return captured;
        }

        public override string ToString() {
            StringBuilder rowStr = new StringBuilder(Size);

            string[] rowStrings = new string[Size];
            // Start from the top (from white's perspecitve).
            for (int j = Size - 1; j >= 0; j--) {
                // Build a string for this rank.
                // Record the number of consecutive empty cells.
                int emptyCells = 0;

                // Start from the left (from white's perspective).
                for (int i = 0; i < Size; i++) {
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

        public bool IsTileOccupied(BoardPosition pos) {
            return GetPiece(pos) != null;

        }

    }

}