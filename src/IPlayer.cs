namespace skaktego {

    public interface IPlayer {

        /// <summary>
        /// Allow the player to initialize
        /// </summary>
        /// <param name="gameState">The starting game state</param>
        /// <param name="color">The players color</param>
        void GameStart(GameState gameState, ChessColors color);

        /// <summary>
        /// Get a move from the player
        /// </summary>
        /// <param name="gameState">The current game state</param>
        /// <returns></returns>
        ChessMove GetMove(GameState gameState);
    }

}
