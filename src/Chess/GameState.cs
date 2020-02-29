using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace skaktego.Chess {

    /// <summary>
    /// The three different game types:
    ///  - Normal chess
    ///  - Skaktego chess
    ///  - Skakteo chess in the preparation phase
    /// </summary>
    public enum GameTypes {
        Normal, Skaktego, SkaktegoPrep
    }

    /// <summary>
    /// Information used to castle
    /// </summary>
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

    /// <summary>
    /// Complete state of a chess or skaktego game
    /// </summary>
    public class GameState {

        /// <summary>
        /// The game board
        /// </summary>
        public Board board;

        /// <summary>
        /// List of pieces that have been taken
        /// </summary>
        public List<Piece> taken;

        /// <summary>
        /// The current turn's player
        /// </summary>
        public ChessColors player;

        /// <summary>
        /// Castling information
        /// </summary>
        public CastlingInfo castling;

        /// <summary>
        /// Potential en passant position
        /// </summary>
        public Nullable<BoardPosition> enPassant;

        /// <summary>
        /// Half-moves since last capture or pawn move
        /// </summary>
        public int halfmoveClock = 0;

        /// <summary>
        /// Total full-moves
        /// </summary>
        public int fullmoveClock = 1;

        /// <summary>
        /// The game mode
        /// </summary>
        public GameTypes gameType;

        /// <summary>
        /// Construct a game state given only a board
        /// </summary>
        /// <param name="board">The game board</param>
        public GameState(Board board) {
            this.board = board;
            this.taken = new List<Piece>();
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

        /// <summary>
        /// Construct a game state given all of its parameters
        /// </summary>
        /// <param name="board"></param>
        /// <param name="taken"></param>
        /// <param name="player"></param>
        /// <param name="castling"></param>
        /// <param name="enPassant"></param>
        /// <param name="halfmoveClock"></param>
        /// <param name="fullmoveClock"></param>
        /// <param name="gameType"></param>
        public GameState(Board board, List<Piece> taken, ChessColors player,
        CastlingInfo castling, Nullable<BoardPosition> enPassant,
        int halfmoveClock, int fullmoveClock, GameTypes gameType) {
            this.board = board;
            this.taken = taken;
            this.player = player;
            this.castling = castling;
            this.enPassant = enPassant;
            this.halfmoveClock = halfmoveClock;
            this.fullmoveClock = fullmoveClock;
            this.gameType = gameType;
        }

        /// <summary>
        /// Obfuscate the board so <c>obfPlayer</c> does not see the enemies pieces,
        /// castling information, or en passant.
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

            if (obfPlayer == ChessColors.White) {
                castling.blackKing = false;
                castling.blackQueen = false;
                castling.blackLeftRook = new BoardPosition(-1, -1);
                castling.blackRightRook = new BoardPosition(-1, -1);
            } else {
                castling.whiteKing = false;
                castling.whiteQueen = false;
                castling.whiteLeftRook = new BoardPosition(-1, -1);
                castling.whiteRightRook = new BoardPosition(-1, -1);
            }

            enPassant = null;
        }

        /// <summary>
        /// Replace all unknown pieces with a random piece
        /// </summary>
        /// <para>
        /// This method never replaces a piece with a king.
        /// This is due to a bug in <c>ChessAI</c>
        /// </para>
        /// <param name="rand">The random number generator</param>
        public void DeObfuscate(Random rand) {
            for (int i = 0; i < board.Size; i++) {
                for (int j = 0; j < board.Size; j++) {
                    var piece = board.GetPiece(new BoardPosition(i, j));
                    if (piece != null && piece.Type == PieceTypes.Unknown) {
                        PieceTypes randType = (PieceTypes) rand.Next(Piece.PIECE_TYPE_COUNT - 2);
                        piece.Promote(randType);
                    }
                }
            }
        }

        /// <summary>
        /// Convert a string to a game state
        /// </summary>
        /// <para>
        /// The string notation is very close to FEN,
        /// but modified to accomidate for skaktego rules.
        /// </para>
        /// <seealso cref="ToString"/>
        public static GameState FromString(string stateStr) {
            string[] splitStr = stateStr.Split(' ');
            var boardStr     = splitStr[0];
            var takenStr     = splitStr[1];
            var playerStr    = splitStr[2];
            var castlingStr  = splitStr[3];
            var enPassantStr = splitStr[4];
            var halfmoveStr  = splitStr[5];
            var fullmoveStr  = splitStr[6];
            var gameTypeStr  = splitStr[7];

            // Construct all of the elements of the game state from the
            // string parts.
            Board board = Board.FromString(boardStr);

            List<Piece> taken = new List<Piece>();
            if (takenStr != "-") {
                foreach (char pieceChar in takenStr) {
                    Piece piece = Piece.FromChar(pieceChar);
                    taken.Add(piece);
                }
            }

            ChessColors player = ChessColorsMethods.FromChar(playerStr[0]);
            CastlingInfo castling = CastlingInfo.FromString(castlingStr);

            Nullable<BoardPosition> enPassant = null;
            if (enPassantStr != "-") {
                enPassant = BoardPosition.FromString(enPassantStr);
            }

            int halfmoveClock = int.Parse(halfmoveStr);
            int fullmoveClock = int.Parse(fullmoveStr);

            GameTypes gameType;
            switch (gameTypeStr) {
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
                    throw new ArgumentException("'" + gameTypeStr + "' is not a valid string");
            }

            return new GameState(board, taken, player,
            castling, enPassant, halfmoveClock,
            fullmoveClock, gameType);
        }

        /// <summary>
        /// Convert the game state to a string
        /// </summary>
        /// <para>
        /// The string notation is very close to FEN,
        /// but modified to accomidate for skaktego rules.
        /// </para>
        /// <seealso cref="FromString"/>
        public override string ToString() {
            string boardStr = board.ToString();
            string takenStr = "";
            foreach (Piece piece in taken) {
                takenStr += piece.ToString();
            }
            takenStr = string.IsNullOrEmpty(takenStr) ? "-" : takenStr;
            string playerStr = player.ToChar().ToString();
            string castlingStr = castling.ToString();
            string enPassantStr = enPassant == null ? "-" : enPassant.ToString();
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
           
            return string.Join(' ', boardStr, takenStr, playerStr,
            castlingStr, enPassantStr, halfmoveClockStr,
            fullmoveClockStr, gameTypeStr);
        }
    }

}
