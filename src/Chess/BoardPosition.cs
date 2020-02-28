using System;

namespace skaktego.Chess {

    /// <summary>
    /// Struct holding a column and row value
    /// </summary>
    public struct BoardPosition {

        /// <summary>
        /// The column of a board
        /// </summary>
        public int column;

        /// <summary>
        /// The row of a board
        /// </summary>
        public int row;

        /// <summary>
        /// Convert this object from a FEN string
        /// </summary>
        /// <param name="pos">The FEN string position</param>
        public static BoardPosition FromString(string pos) {
            char px = pos[0];
            string py = pos.Substring(1);
            int i = (int)(px - 'a');
            int j = int.Parse(py) - 1;

            return new BoardPosition(i, j);
        }

        /// <summary>
        /// Construct a new board position given a column and row
        /// </summary>
        /// <param name="column">The column of the board</param>
        /// <param name="row">The row of the board</param>
        public BoardPosition(int column, int row) {
            this.column = column;
            this.row = row;
        }

        /// <summary>
        /// Convert this board position to a FEN string
        /// </summary>
        public override string ToString() {
            // TODO: Throw error if px > 'i' or py > 9
            char px = (char)('a' + column);
            char py = (char)('1' + row);
            char[] charArr = { px, py };
            return new string(charArr);
        }

        public override bool Equals(Object obj) {
            return obj is  BoardPosition && this == (BoardPosition)obj;
        }

        public override int GetHashCode() {
            return column.GetHashCode() ^ row.GetHashCode();
        }


        public static bool operator ==(BoardPosition pos1, BoardPosition pos2) {
            return pos1.column == pos2.column && pos1.row == pos2.row;
        }
        public static bool operator !=(BoardPosition pos1, BoardPosition pos2) {
            return !(pos1 == pos2);
        }
    }

}
