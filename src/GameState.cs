using System;
using System.Linq;
using System.Text;

namespace skaktego {

    public struct CastlingInfo {
        public bool whiteKing;
        public bool whiteQueen;
        public bool blackKing;
        public bool blackQueen;

        public static CastlingInfo FromString(string s) {
            CastlingInfo info = new CastlingInfo();
            if (s.Contains('K')) {
                info.whiteKing = true;
            }
            if (s.Contains('Q')) {
                info.whiteQueen = true;
            }
            if (s.Contains('k')) {
                info.blackKing = true;
            }
            if (s.Contains('q')) {
                info.blackQueen = true;
            }
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
            return strInfo.ToString();
        }
    }

    public class GameState {
        public Board board;
        public ChessColors player;
        public CastlingInfo castling;
        public Nullable<BoardPosition> enPassant;
        public int halfmoveClock = 0;
        public int fullmoveClock = 1;

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
        }

        public GameState(Board board, ChessColors player, CastlingInfo castling,
        Nullable<BoardPosition> enPassant, int halfmoveClock, int fullmoveClock) {
            this.board = board;
            this.player = player;
            this.castling = castling;
            this.enPassant = enPassant;
            this.halfmoveClock = halfmoveClock;
            this.fullmoveClock = fullmoveClock;
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

            int halfmoveClock = int.Parse(splitStr[4]);
            int fullmoveClock = int.Parse(splitStr[5]);

            return new GameState(board, player, castling,
            enPassant, halfmoveClock, fullmoveClock);
        }

        public override string ToString() {
            string boardStr = board.ToString();
            string playerStr = player.ToChar().ToString();
            string castlingStr = castling.ToString();
            string enPassantStr = enPassant == null ? "-" : enPassant.ToString();
            string halfmoveClockStr = halfmoveClock.ToString();
            string fullmoveClockStr = fullmoveClock.ToString();

            return string.Join(' ', boardStr, playerStr, castlingStr,
            enPassantStr, halfmoveClockStr, fullmoveClockStr);
        }
    }

}
