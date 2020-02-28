using System;

namespace skaktego.Chess {

    /// <summary>
    /// The two colors in chess
    /// </summary>
    public enum ChessColors {
        White, Black
    }

    /// <summary>
    /// Extention methods for chess colors
    /// </summary>
    /// <seealso>skaktego.Game.ChessColors</seealso>
    public static class ChessColorsMethods {

        /// <summary>
        /// Convert a chess color to a character
        /// </summary>
        /// <para>
        /// White becomes 'w' and black becomes 'b'
        /// </para>
        /// <seealso>skaktego.Game.ChessColorsMethods.FromChar</seealso>
        public static char ToChar(this ChessColors color) {
            switch (color) {
                case ChessColors.Black:
                    return 'b';
                default:
                    return 'w';
            }
        }

        /// <summary>
        /// Convert a character to a chess color
        /// </summary>
        /// <seealso>skaktego.Game.ChessColorsMethods.ToChar</seealso>
        /// <returns></returns>
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
        /// Return the color that is not this one
        /// </summary>
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
