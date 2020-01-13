namespace skaktego {

    public enum PieceColors {
        White, Black
    }

    public enum PieceTypes {
        King, Queen, Bishop,
        Knight, Rook, Pawn
    }

    public class Piece {
        public const int PIECE_COLOR_COUNT = 2;
        public const int PIECE_TYPE_COUNT = 6;

        public PieceColors Color { get; }
        public PieceTypes Type { get; }
        //public int Index { get => (int)Type + (int)Color * PIECE_TYPE_COUNT; }

        public Piece(PieceColors color, PieceTypes type) {
            Color = color;
            Type = type;
        }
    }

}