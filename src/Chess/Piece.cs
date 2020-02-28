using System;

namespace skaktego.Chess {

    public enum PieceTypes {
        King, Queen, Bishop,
        Knight, Rook, Pawn,
        Unknown
    }

    public class Piece {
        public const int PIECE_COLOR_COUNT = 2;
        public const int PIECE_TYPE_COUNT = 7;

        public ChessColors Color { get; private set; }
        public PieceTypes Type { get; private set; }

        // This is only useful for pawns,
        // and will be ignored for all other pieces
        public bool hasMoved = true;

        /// <summary>
        /// Create a new piece from a character.
        /// </summary>
        /// <param name="c">The character representing the piece.</param>
        public static Piece FromChar(char c) {
            ChessColors color;
            PieceTypes type;

            // Check for pawn special case
            if (c == 'ℙ') {
                return new Piece(ChessColors.White, PieceTypes.Pawn, false);
            } else if (c == '℗') {
                return new Piece(ChessColors.Black, PieceTypes.Pawn, false);
            }

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
                case 'u':
                    type = PieceTypes.Unknown;
                    break;
                default:
                    throw new ArgumentException("Can't construct piece from character '" + c + "'");
            }

            return new Piece(color, type);
        }


        /// <summary>
        /// Create a new piece given a color and type.
        /// </summary>
        /// <param name="color">The color the piece belongs to.</param>
        /// <param name="type">The type of the piece.</param>
        public Piece(ChessColors color, PieceTypes type, bool hasMoved=true) {
            Color = color;
            Type = type;
            this.hasMoved = hasMoved;
        }


        public char ToChar() {

            // Check for pawn special case
            if (Type == PieceTypes.Pawn && !hasMoved) {
                if (Color == ChessColors.White) {
                    return 'ℙ';
                } else {
                    return '℗';
                }
            }

            char c;
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
                case PieceTypes.Unknown:
                    c = 'u';
                    break;
                default:
                    throw new InvalidOperationException("Piece type " + Type + " not recognized.");
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

        public void Promote(PieceTypes pieceType) {
            Type = pieceType;
        }
    }
}
