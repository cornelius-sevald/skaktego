using System;

namespace skaktego {

    public class BoardPosition : Tuple<int, int> {

        public int Column {
            get { return Item1; }
        }

        public int Row {
            get { return Item2; }
        }

        public static BoardPosition FromString(string pos) {
            // TODO: Throw error if px > 'i' or py > 9
            char px = pos[0];
            char py = pos[1];
            int i = (int)(px - 'a');
            int j = (int)(py - '1');
            return new BoardPosition(i, j);
        }

        public BoardPosition(int column, int row) : base(column, row) { }

        public override string ToString() {
            // TODO: Throw error if px > 'i' or py > 9
            char px = (char)('a' + Column);
            char py = (char)('1' + Row);
            char[] charArr = { px, py };
            return new string(charArr);
        }
    }

}
