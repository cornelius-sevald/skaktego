using System;

namespace skaktego.Chess {
    public enum ChessColors {
        White, Black
    }

    public static class ChessColorsMethods {
        public static char ToChar(this ChessColors color) {
            switch (color) {
                case ChessColors.Black:
                    return 'b';
                default:
                    return 'w';
            }
        }

        public static ChessColors FromChar(char s) {
            switch (s) {
                case 'b':
                    return ChessColors.Black;
                case 'w':
                    return ChessColors.White;
                default:
                    throw new ArgumentException();
            }
        }

        /// <summary>
        /// Return the other color
        /// </summary>
        /// <returns></returns>
        public static ChessColors Other(this ChessColors color) {
            switch (color) {
                case ChessColors.Black:
                    return ChessColors.White;
                default:
                    return ChessColors.Black;
            }
        }
    }
}