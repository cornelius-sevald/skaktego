namespace skaktego {

    public interface IPlayer {
        void GameStart(GameState gameState);
        ChessMove GetMove(GameState gameState);
    }

}
