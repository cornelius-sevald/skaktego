using System;
using System.Text;

namespace skaktego {

    public class Board {
        public int Size { get; private set; }

        private Piece[,] board;

        public static Tuple<int, int> StringToPosition(string pos) {
            // TODO: Throw error if px > 'i' or py > 9
            char px = pos[0];
            char py = pos[1];
            int i = (int)(px - 'a');
            int j = (int)(py - '1');
            return new Tuple<int, int>(i, j);
        }

        public static string PositionToString(int i, int j) {
            // TODO: Throw error if px > 'i' or py > 9
            char px = (char)('a' + i);
            char py = (char)('1' + j);
            char[] charArr = { px, py };
            return new string(charArr);
        }

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
                        var piece = Piece.FromChar(c);
                        board.SetPiece(piece, i, j);
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

        public Piece GetPiece(int i, int j) {
            return board[i, j];
        }

        public Piece GetPiece(string strPos) {
            var pos = StringToPosition(strPos);
            int i = pos.Item1;
            int j = pos.Item2;
            return GetPiece(i, j);
        }

        public void SetPiece(Piece piece, int i, int j) {
            board[i, j] = piece;
        }

        public void SetPiece(Piece piece, string strPos) {
            var pos = StringToPosition(strPos);
            int i = pos.Item1;
            int j = pos.Item2;
            SetPiece(piece, i, j);
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
                    var piece = GetPiece(i, j);
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