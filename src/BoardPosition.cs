using System;

namespace skaktego {

    public struct BoardPosition {

        public int column;

        public int row;

        public static BoardPosition FromString(string pos) {
            // TODO: Throw error if px > 'i' or py > 9
            char   px = pos[0];
            string py = pos.Substring(1);
            int i = (int)(px - 'a');
            int j = int.Parse(py) - 1;
            return new BoardPosition(i, j);
        }

        public BoardPosition(int column, int row) {
            this.column = column;
            this.row = row;
        }

        public override string ToString() {
            // TODO: Throw error if px > 'i' or py > 9
            char px = (char)('a' + column);
            char py = (char)('1' + row);
            char[] charArr = { px, py };
            return new string(charArr);
        }
    }

}
