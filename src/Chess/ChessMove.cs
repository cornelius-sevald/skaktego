namespace skaktego.Chess {

    /// <summary>
    /// A move from one position to another
    /// </summary>
    public struct ChessMove {
        public BoardPosition from;
        public BoardPosition to;

        public ChessMove (BoardPosition from, BoardPosition to) {
            this.from = from;
            this.to = to;
        }
    }

}