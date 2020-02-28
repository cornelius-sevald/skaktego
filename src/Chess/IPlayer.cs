namespace skaktego.Chess {

    /// <summary>
    /// Generic player interface.
    /// </summary>
    /// <para>
    /// If a class implements this interface,
    /// it will be able to play chess using the Game class.
    /// </para>
    public interface IPlayer {

        /// <summary>
        /// Set the game state for the player to see
        /// </summary>
        /// <param name="gameState"></param>
        void SetGameState(GameState gameState);

        /// <summary>
        /// Get a move from the player
        /// </summary>
        /// <param name="playerColor">The color of the player</param>
        /// <returns></returns>
        ChessMove GetMove(GameState gameState, ChessColors playerColor);
    }

}
