using System;
using System.Linq;
using System.Text;

namespace skaktego {

    /// <summary>
    /// The three different game types:
    ///  - Normal chess
    ///  - Skaktego chess
    ///  - Skakteo chess in the preparation phase
    /// </summary>
    public enum GameTypes {
        Normal, Skaktego, SkaktegoPrep
    }


    public struct CastlingInfo {
        public bool whiteKing;
        public bool whiteQueen;
        public bool blackKing;
        public bool blackQueen;

        public BoardPosition whiteLeftRook;
        public BoardPosition whiteRightRook;
        public BoardPosition blackLeftRook;
        public BoardPosition blackRightRook;

        public static CastlingInfo FromString(string s) {
            CastlingInfo info = new CastlingInfo();
            string[] sArr = s.Split(':');
            if (sArr[0].Contains('K')) {
                info.whiteKing = true;
            }
            if (sArr[0].Contains('Q')) {
                info.whiteQueen = true;
            }
            if (sArr[0].Contains('k')) {
                info.blackKing = true;
            }
            if (sArr[0].Contains('q')) {
                info.blackQueen = true;
            }
            info.whiteLeftRook = BoardPosition.FromString(sArr[1]);
            info.whiteRightRook = BoardPosition.FromString(sArr[2]);
            info.blackLeftRook = BoardPosition.FromString(sArr[3]);
            info.blackRightRook = BoardPosition.FromString(sArr[4]);
            return info;
        }

        public override string ToString() {
            StringBuilder strInfo = new StringBuilder(4);
            if (whiteKing) {
                strInfo.Append('K');
            }
            if (whiteQueen) {
                strInfo.Append('Q');
            }
            if (blackKing) {
                strInfo.Append('k');
            }
            if (blackQueen) {
                strInfo.Append('q');
            }
            // If strInfo is empty, replace it with a dash.
            if (strInfo.Length == 0) {
                strInfo.Append('-');
            }
            strInfo.Append(':');
            strInfo.Append(whiteLeftRook.ToString());
            strInfo.Append(':');
            strInfo.Append(whiteRightRook.ToString());
            strInfo.Append(':');
            strInfo.Append(blackLeftRook.ToString());
            strInfo.Append(':');
            strInfo.Append(blackRightRook.ToString());

            return strInfo.ToString();
        }
    }

    public class GameState {
        public Board board;
        public ChessColors player;
        public CastlingInfo castling;
        public Nullable<BoardPosition> enPassant;
        public Nullable<ChessColors> kingTaken = null;
        public int halfmoveClock = 0;
        public int fullmoveClock = 1;
        public GameTypes gameType;

        public GameState(Board board) {
            this.board = board;
            this.player = ChessColors.White;
            this.castling = new CastlingInfo {
                whiteKing = true,
                whiteQueen = true,
                blackKing = true,
                blackQueen = true
            };
            enPassant = null;
            gameType = GameTypes.Normal;
        }

        public GameState(Board board, ChessColors player, CastlingInfo castling,
        Nullable<BoardPosition> enPassant, Nullable<ChessColors> kingTaken,
        int halfmoveClock, int fullmoveClock, GameTypes gameType) {
            this.board = board;
            this.player = player;
            this.castling = castling;
            this.enPassant = enPassant;
            this.kingTaken = kingTaken;
            this.halfmoveClock = halfmoveClock;
            this.fullmoveClock = fullmoveClock;
            this.gameType = gameType;
        }

        /// <summary>
        /// Obfuscate the board so <c>obfPlayer</c> does not see the enemies pieces.
        /// </summary>
        /// <param name="obfPlayer">The player to hide the opponents pieces from</param>
        public void Obfuscate(ChessColors obfPlayer) {
            for (int i = 0; i < board.Size; i++) {
                for (int j = 0; j < board.Size; j++) {
                    var piece = board.GetPiece(new BoardPosition(i, j));
                    if (piece != null && piece.Color != obfPlayer) {
                        piece.Promote(PieceTypes.Unknown);
                    }
                }
            }
        }

        public static GameState FromString(string stateStr) {
            string[] splitStr = stateStr.Split(' ');

            // Construct all of the elements of the game state from the
            // string parts.
            Board board = Board.FromString(splitStr[0]);
            ChessColors player = ChessColorsMethods.FromChar(splitStr[1][0]);
            CastlingInfo castling = CastlingInfo.FromString(splitStr[2]);

            Nullable<BoardPosition> enPassant = null;
            if (splitStr[3] != "-") {
                enPassant = BoardPosition.FromString(splitStr[3]);
            }

            Nullable<ChessColors> kingTaken;
            switch (splitStr[4]) {
                case "w":
                    kingTaken = ChessColors.White;
                    break;
                case "b":
                    kingTaken = ChessColors.Black;
                    break;
                case "-":
                    kingTaken = null;
                    break;
                default:
                    throw new ArgumentException("'" + splitStr[4] + "' is not a valid string");
            }

            int halfmoveClock = int.Parse(splitStr[5]);
            int fullmoveClock = int.Parse(splitStr[6]);

            GameTypes gameType;
            switch (splitStr[7]) {
                case "s":
                    gameType = GameTypes.Normal;
                    break;
                case "st":
                    gameType = GameTypes.Skaktego;
                    break;
                case "stp":
                    gameType = GameTypes.SkaktegoPrep;
                    break;
                default:
                    throw new ArgumentException("'" + splitStr[7] + "' is not a valid string");
            }

            return new GameState(board, player, castling,
            enPassant, kingTaken, halfmoveClock,
            fullmoveClock, gameType);
        }

        public override string ToString() {
            string boardStr = board.ToString();
            string playerStr = player.ToChar().ToString();
            string castlingStr = castling.ToString();
            string enPassantStr = enPassant == null ? "-" : enPassant.ToString();
            string kingTakenStr;
            if (kingTaken.HasValue) {
                kingTakenStr = kingTaken.Value == ChessColors.White ? "w" : "b";
            } else {
                kingTakenStr = "-";
            }
            string halfmoveClockStr = halfmoveClock.ToString(); string fullmoveClockStr = fullmoveClock.ToString();
            string gameTypeStr;
            switch (gameType) {
                case GameTypes.Skaktego:
                    gameTypeStr = "st";
                    break;
                case GameTypes.SkaktegoPrep:
                    gameTypeStr = "stp";
                    break;
                default:
                    gameTypeStr = "s";
                    break;
            }
           
            return string.Join(' ', boardStr, playerStr, castlingStr,
            enPassantStr, kingTakenStr, halfmoveClockStr,
            fullmoveClockStr, gameTypeStr);
        }
    }

}
