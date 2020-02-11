using System;

namespace skaktego {

    public enum PieceTypes {
        King, Queen, Bishop,
        Knight, Rook, Pawn
    }

    public class Piece {
        public const int PIECE_COLOR_COUNT = 2;
        public const int PIECE_TYPE_COUNT = 6;

        public ChessColors Color { get; private set; }
        public PieceTypes Type { get; private set; }
        //public int Index { get => (int)Type + (int)Color * PIECE_TYPE_COUNT; }

        /// <summary>
        /// Create a new piece from a character.
        /// </summary>
        /// <param name="c">The character representing the piece.</param>
        public static Piece FromChar(char c) {
            ChessColors color;
            PieceTypes type;

            color = Char.IsLower(c) ? ChessColors.Black : ChessColors.White;

            switch (Char.ToLower(c)) {
                case 'k':
                    type = PieceTypes.King;
                    break;
                case 'q':
                    type = PieceTypes.Queen;
                    break;
                case 'b':
                    type = PieceTypes.Bishop;
                    break;
                case 'n':
                    type = PieceTypes.Knight;
                    break;
                case 'r':
                    type = PieceTypes.Rook;
                    break;
                case 'p':
                    type = PieceTypes.Pawn;
                    break;
                default:
                    // TODO: Throw an error instead.
                    Console.WriteLine("Can't construct piece from character '{0}'.", c);
                    return null;
            }

            return new Piece(color, type);
        }


        /// <summary>
        /// Create a new piece given a color and type.
        /// </summary>
        /// <param name="color">The color the piece belongs to.</param>
        /// <param name="type">The type of the piece.</param>
        public Piece(ChessColors color, PieceTypes type) {
            Color = color;
            Type = type;
        }


        public char ToChar() {
            char c = '\0';
            switch (Type) {
                case PieceTypes.King:
                    c = 'k';
                    break;
                case PieceTypes.Queen:
                    c = 'q';
                    break;
                case PieceTypes.Bishop:
                    c = 'b';
                    break;
                case PieceTypes.Knight:
                    c = 'n';
                    break;
                case PieceTypes.Rook:
                    c = 'r';
                    break;
                case PieceTypes.Pawn:
                    c = 'p';
                    break;
            }

            // TODO: Throw exception if c == '\0'.

            if (Color == ChessColors.White) {
                c = Char.ToUpper(c);
            }

            return c;
        }

        public override string ToString() {
            return ToChar().ToString();
        }
    }
}
