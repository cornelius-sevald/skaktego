using System;
using System.Linq;
using System.Text;

namespace skaktego.Chess {

    /// <summary>
    /// Chess board class
    /// </summary>
    public class Board {

        /// <summary>
        /// The width and height of the board.
        /// 
        /// <para>This is usually 8</para>
        /// </summary>
        public int Size { get; private set; }

        // The internal board
        private Piece[,] board;

        /// <summary>
        /// Create a board from a FEN string
        /// 
        /// <para>The notation is a little different from FEN,
        /// <see>skaktego.Chess.Piece.FromChar</see> for why.</para>
        /// </summary>
        /// <param name="s">The board in Forsyth–Edwards Notation</param>
        /// <returns></returns>
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

        /// <summary>
        /// Initialize a board from a 2d array of pieces.
        /// </summary>
        public Board(Piece[,] pieces) {
            // TODO: Throw error if 
            int size = pieces.GetLength(0);
            this.Size = size;
            this.board = new Piece[size, size];
            Array.Copy(pieces, board, pieces.Length);
        }

        /// <summary>
        /// Get a piece at a position on the board
        /// </summary>
        public Piece GetPiece(BoardPosition pos) {
            return board[pos.column, pos.row];
        }

        /// <summary>
        /// Get a piece at a position using a string.
        /// </summary>
        public Piece GetPiece(string strPos) {
            var pos = BoardPosition.FromString(strPos);
            return GetPiece(pos);
        }

        /// <summary>
        /// Set a piece at a position on the board
        /// </summary>
        public void SetPiece(Piece piece, BoardPosition pos) {
            board[pos.column, pos.row] = piece;
        }

        /// <summary>
        /// Set a piece at a positon on the board using a string
        /// </summary>
        /// <param name="piece"></param>
        /// <param name="strPos"></param>
        public void SetPiece(Piece piece, string strPos) {
            var pos = BoardPosition.FromString(strPos);
            SetPiece(piece, pos);
        }

        /// <summary>
        /// Capture a piece at a position and return it
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public Piece CapturePiece(BoardPosition pos) {
            var captured = GetPiece(pos);
            SetPiece(null, pos);
            return captured;
        }

        /// <summary>
        /// Returns true if a piece occipies the position
        /// </summary>
        public bool IsTileOccupied(BoardPosition pos) {
            return GetPiece(pos) != null;

        }

        /// <summary>
        /// Convert this board into a FEN string
        /// </summary>
        /// <para>
        /// The notation is a little different from FEN,
        /// <see>skaktego.Chess.Piece.FromString</see> for why.
        /// </para>
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

    }

}
