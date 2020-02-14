using System;

namespace skaktego {

    public class BoardPosition {

        public int Column { get; set; }

        public int Row { get; set; }

        public static BoardPosition FromString(string pos) {
            // TODO: Throw error if px > 'i' or py > 9
            char   px = pos[0];
            string py = pos.Substring(1);
            int i = (int)(px - 'a');
            int j = int.Parse(py) - 1;
            return new BoardPosition(i, j);
        }

        public BoardPosition(int column, int row) {
            Column = column;
            Row = row;
        }

        public override string ToString() {
            // TODO: Throw error if px > 'i' or py > 9
            char px = (char)('a' + Column);
            char py = (char)('1' + Row);
            char[] charArr = { px, py };
            return new string(charArr);
        }
    }

}
