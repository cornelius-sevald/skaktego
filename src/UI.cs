using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using SDL2;

namespace skaktego
{
    public enum GameResults
    {
        WhiteWin, BlackWin, Tie, StillGaming
    }

    public sealed class UI : IPlayer
    {
        // Screen size
        private const int SCREEN_WIDTH = 800;
        private const int SCREEN_HEIGHT = 450;

        private const string RESOURCE_PATH = "resources/";

        private static UI instance = null;
        private static readonly object padlock = new object();

        private Window window;
        private Renderer renderer;
        private Font font;
        private Texture background;
        private Texture menuLogo;
        private Texture menuBG;
        private Texture pieceSprites;
        private Texture[] endTextTextures;
        private Texture overlayText1;
        private Texture overlayText2;
        private Rect[,] pieceClips;
        private Nullable<BoardPosition> highlightedTile = null;
        private Nullable<BoardPosition> selectedTile = null;
        private List<BoardPosition> legalMoves;
        private List<SDL.SDL_Event> events;
        private Rect screenRect = null;
        private bool isMenuActive = false;
        private bool isGaming = false;
        private bool doneGaming = false;
        private bool screenHidden = false;
        private Button[] buttons;
        private Button[] menuButtons;
        private Button[] gameButtons;
        private Button[] endButtons;
        private GameResults gameResult = GameResults.StillGaming;

        private MVar<ChessMove> storedMove;
        private GameState gameState = null;
        private Game game = null;
        private Thread gameThread;

        public bool quit = false;

        UI()
        {
            Graphics.InitGraphics();

            window = new Window("skaktego", 100, 100, SCREEN_WIDTH, SCREEN_HEIGHT);
            renderer = new Renderer(window);

            using (Surface icon = new Surface("skaktegoIcon.png")) {
                window.SetWindowIcon(icon);
            } 

            font = new Font("playfair-display/PlayfairDisplay-Regular.ttf", 128);

            legalMoves = new List<BoardPosition>();

            events = new List<SDL.SDL_Event>();

            buttons = new Button[]{
                new Button(3/8.0, 8/24.0, 1/4.0, 1/12.0, "Continiue", font, () => isMenuActive = false),
                new Button(3/8.0, 11/24.0, 1/4.0, 1/12.0, "New Game", font, () => { StopGaming(); BeginGaming(); }),
                new Button(3/8.0, 14/24.0, 1/4.0, 1/12.0, "Main Menu", font, StopGaming)
            };

            menuButtons = new Button[]{
                new Button(3/8.0, 11/24.0, 1/4.0, 1/12.0, "Play Game", font, BeginGaming),
                new Button(3/8.0, 14/24.0, 1/4.0, 1/12.0, "  Exit  ", font, () => quit = true)
            };

            gameButtons = new Button[]{
                new Button(0, 0, 1, 1, "X", font, () => isMenuActive = true)
            };

            endButtons = new Button[]{
                new Button(3/12.0, 14/24.0, 1/6.0, 1/12.0, "Rematch", font, () => { StopGaming(); BeginGaming(); }),
                new Button(7/12.0, 14/24.0, 1/6.0, 1/12.0, "Main Menu", font, StopGaming)
            };

            background = new Texture(renderer, "background.png");
            menuLogo = new Texture(renderer, "skaktegoLogo.png");
            menuBG = new Texture(renderer, "skaktegoMain.png");
            pieceSprites = new Texture(renderer, "pieces.png");
            pieceClips = UI.GetPieceClips(pieceSprites);

            Color[] endColors = new Color[] { Graphics.white, Graphics.black, Graphics.gray };
            string[] endTexts = new string[] { "White Wins", "Black Wins", "   Tie   " };
            endTextTextures = new Texture[3];
            for (int i = 0; i < endTexts.Length; i++)
            {
                using (Surface textSurf = font.TextSurface(endTexts[i], endColors[i]))
                {
                    endTextTextures[i] = new Texture(renderer, textSurf);
                }
            }

            using (Surface textSurf = font.TextSurface("Next turn", Graphics.white)) {
                overlayText1 = new Texture(renderer, textSurf);
            }
            using (Surface textSurf = font.TextSurface("Press any button to continiue", Graphics.white)) {
                overlayText2 = new Texture(renderer, textSurf);
            }
        }

        public void GameStart(GameState gameState)
        {
            this.gameState = gameState;
            isGaming = true;
            doneGaming = false;
        }

        public ChessMove GetMove(GameState gameState)
        {
            this.gameState = gameState;
            screenHidden = true;
            DrawOverlay();
            return storedMove.Var;
        }

        private void BeginGaming()
        {
            storedMove = new MVar<ChessMove>();
            isGaming = true;
            gameResult = GameResults.StillGaming;
            isMenuActive = false;
            game = new Game(this, this);
            gameThread = new Thread(new ThreadStart(() =>
            {
                gameState = game.PlayGame();
                if (Engine.IsCheckmate(gameState))
                {
                    if (gameState.player == ChessColors.White)
                    {
                        gameResult = GameResults.BlackWin;
                    }
                    else
                    {
                        gameResult = GameResults.WhiteWin;
                    }
                }
                else if (Engine.IsTie(gameState))
                {
                    gameResult = GameResults.Tie;
                }
            }));
            gameThread.Start();
        }

        private void StopGaming()
        {
            isGaming = false;
            if (game != null) {
                game.quit = true;
            }
            if (storedMove != null && !storedMove.HasValue) {
                storedMove.Var = new ChessMove();
            }
            if (gameThread != null && gameThread.IsAlive) {
                gameThread.Join();
            }
        }

        public void Update()
        {
            PollEvents();

            // Update screen rect
            screenRect = renderer.OutputRect();

            // Check if the user wants to quit.
            if (events.Any(e => e.type == SDL.SDL_EventType.SDL_QUIT))
            {
                quit = true;
            }
            else if (events.Any(e => e.type == SDL.SDL_EventType.SDL_KEYDOWN &&
          e.key.keysym.sym == SDL.SDL_Keycode.SDLK_q))
            {
                quit = true;
            }

            if (events.Any(e => e.type == SDL.SDL_EventType.SDL_KEYDOWN || 
            e.type == SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN)) {
                screenHidden = false;
            }


            if (isGaming)
            {
                if (gameState != null && !screenHidden) {
                    UpdateGame();
                }
            }
            else
            {
                UpdateMainMenu();
            }

        }

        private void UpdateGame()
        {
            int mouseX, mouseY;
            SDL.SDL_GetMouseState(out mouseX, out mouseY);

            if (events.Any(e => e.type == SDL.SDL_EventType.SDL_KEYDOWN &&
          e.key.keysym.sym == SDL.SDL_Keycode.SDLK_ESCAPE))
            {
                isMenuActive = !isMenuActive;
            }

            Rect boardRect = new Rect(0, 0, 0, 0);
            {
                // Board size
                int bs = gameState.board.Size;
                boardRect.W = Math.Min(screenRect.H / bs * bs, screenRect.W / bs * bs);
                boardRect.H = Math.Min(screenRect.H / bs * bs, screenRect.W / bs * bs);
            }

            boardRect.X = (int)Math.Round((screenRect.W - boardRect.W) * 0.5);
            boardRect.Y = (int)Math.Round((screenRect.H - boardRect.H) * 0.5);

            int boardMouseX = (int)Math.Floor((mouseX - boardRect.X) / (double)boardRect.W * gameState.board.Size);
            int boardMouseY = -1 + gameState.board.Size - ((int)Math.Floor((mouseY - boardRect.Y) / (double)boardRect.H * gameState.board.Size));

            if (isMenuActive)
            {
                foreach (Button button in buttons)
                {
                    button.Update(mouseX, mouseY, boardRect, events);
                }
            }
            else
            {
                Rect xButtonRect = new Rect(0, 0, 0, 0);
                xButtonRect.W = (Math.Min(screenRect.H, screenRect.W) / 16);
                xButtonRect.H = (Math.Min(screenRect.H, screenRect.W) / 16);

                xButtonRect.X = screenRect.W - (Math.Min(screenRect.H, screenRect.W) / 12);
                xButtonRect.Y = (Math.Min(screenRect.H, screenRect.W) / 50);
                foreach (Button button in gameButtons)
                {
                    button.Update(mouseX, mouseY, xButtonRect, events);
                }

                if (!doneGaming)
                {
                    if (0 <= boardMouseX && boardMouseX < gameState.board.Size)
                    {
                        if (0 <= boardMouseY && boardMouseY < gameState.board.Size)
                        {
                            highlightedTile = new BoardPosition(boardMouseX, boardMouseY);
                        }
                        else
                        {
                            highlightedTile = null;
                        }
                    }
                    else
                    {
                        highlightedTile = null;
                    }
                    if (events.Any(e => e.type == SDL.SDL_EventType.SDL_MOUSEBUTTONUP
                    && e.button.button == SDL.SDL_BUTTON_LEFT))
                    {
                        // If a tile is already selected, attempt to apply the move
                        if (selectedTile.HasValue && highlightedTile.HasValue)
                        {
                            ChessMove move = new ChessMove(selectedTile.Value, highlightedTile.Value);
                            bool isMoveLegal = false;
                            foreach (BoardPosition legalPos in legalMoves) {
                                if (move.to == legalPos) {
                                    isMoveLegal = true;
                                }
                            }
                            if (isMoveLegal) {
                                if (storedMove.HasValue)
                                {
                                    storedMove.TakeMVar(x => x);
                                }
                                storedMove.Var = move;
                            }
                        }
                        SelectTile(gameState, highlightedTile);
                    }
                    else if (events.Any(e => e.type == SDL.SDL_EventType.SDL_MOUSEBUTTONUP
                  && e.button.button == SDL.SDL_BUTTON_RIGHT))
                    {
                        selectedTile = null;
                        legalMoves = null;
                    }
                }
                if (doneGaming)
                {
                    foreach (Button button in endButtons)
                    {
                        button.Update(mouseX, mouseY, screenRect, events);
                    }
                }
            }
            DrawGame(gameState);
        }

        private void UpdateMainMenu()
        {
            int mouseX, mouseY;
            SDL.SDL_GetMouseState(out mouseX, out mouseY);


            Rect mainMenuRect = new Rect(0, 0, 0, 0);
            mainMenuRect.W = Math.Min(screenRect.H, screenRect.W);
            mainMenuRect.H = Math.Min(screenRect.H, screenRect.W);

            mainMenuRect.X = (int)Math.Round((screenRect.W - mainMenuRect.W) * 0.5);
            mainMenuRect.Y = (int)Math.Round((screenRect.H - mainMenuRect.H) * 0.5);

            foreach (Button button in menuButtons)
            {
                button.Update(mouseX, mouseY, mainMenuRect, events);
            }

            DrawMainMenu();
        }

        private void SelectTile(GameState gameState, Nullable<BoardPosition> tile)
        {
            selectedTile = highlightedTile;
            if (selectedTile.HasValue)
            {
                legalMoves = Engine.GetLegalMoves(gameState, selectedTile.Value);
            }
            else
            {
                legalMoves = null;
            }
        }

        public void DrawGame(GameState gameState)
        {
            // Draw the background
            // and clear the screen
            renderer.SetColor(new Color(0X111111));
            renderer.Clear();

            //Screen geometry only during gameplay
            Rect boardRect = new Rect(0, 0, 0, 0);
            {
                // Board size
                int bs = gameState.board.Size;
                boardRect.W = Math.Min(screenRect.H / bs * bs, screenRect.W / bs * bs);
                boardRect.H = Math.Min(screenRect.H / bs * bs, screenRect.W / bs * bs);
            }

            // Draw the board
            boardRect.X = (int)Math.Round((screenRect.W - boardRect.W) * 0.5);
            boardRect.Y = (int)Math.Round((screenRect.H - boardRect.H) * 0.5);
            DrawBoard(gameState.board, boardRect);

            // Draw the higlighted, selected and legal tiles
            if (highlightedTile != null)
            {
                HighlightTile(new Color(0XFFFF0055), gameState.board, boardRect, highlightedTile.Value);
            }
            if (selectedTile != null)
            {
                HighlightTile(new Color(0X0000FF55), gameState.board, boardRect, selectedTile.Value);
            }
            if (legalMoves != null)
            {
                foreach (BoardPosition legalTile in legalMoves)
                {
                    HighlightTile(new Color(0X11FF1155), gameState.board, boardRect, legalTile);
                }
            }


            //Draw game button
            Rect xButtonRect = new Rect(0, 0, 0, 0);
            xButtonRect.W = (Math.Min(screenRect.H, screenRect.W) / 16);
            xButtonRect.H = (Math.Min(screenRect.H, screenRect.W) / 16);

            xButtonRect.X = screenRect.W - (Math.Min(screenRect.H, screenRect.W) / 12);
            xButtonRect.Y = (Math.Min(screenRect.H, screenRect.W) / 50);

            Rect xButtonRectOutline = new Rect(0, 0, 0, 0);
            xButtonRectOutline.W = (Math.Min(screenRect.H, screenRect.W) / 15);
            xButtonRectOutline.H = (Math.Min(screenRect.H, screenRect.W) / 15);

            xButtonRectOutline.X = (screenRect.W - (Math.Min(screenRect.H, screenRect.W) / 12)) - ((xButtonRectOutline.W - xButtonRect.W) / 2);
            xButtonRectOutline.Y = (Math.Min(screenRect.H, screenRect.W) / 50) - ((xButtonRectOutline.W - xButtonRect.W) / 2);

            renderer.SetColor(new Color(0XffBBBBBBFF));
            renderer.FillRect(xButtonRectOutline);

            foreach (Button button in gameButtons)
            {
                button.Draw(renderer, xButtonRect);
            }
            renderer.SetColor(new Color(0Xff002277));
            renderer.FillRect(xButtonRect);

            DrawEndScreen(screenRect);

            // Draw the in game menu, if it is active
            if (isMenuActive)
            {
                DrawGameMenu(boardRect);
            }

            renderer.Present();
        }

        private void DrawMainMenu()
        {
            renderer.SetColor(new Color(0XdfedecFF));
            renderer.FillRect(screenRect);

            Rect mainMenuRect = new Rect(0, 0, 0, 0);
            mainMenuRect.W = Math.Min(screenRect.H, screenRect.W);
            mainMenuRect.H = Math.Min(screenRect.H, screenRect.W);

            mainMenuRect.X = (int)Math.Round((screenRect.W - mainMenuRect.W) * 0.5);
            mainMenuRect.Y = (int)Math.Round((screenRect.H - mainMenuRect.H) * 0.5);

            Rect bgRect = new Rect(0, 0, 0, 0);
            int bgW, bgH;
            menuBG.Query(out bgW, out bgH);
            bgRect.W = (int)(Math.Max(screenRect.H / (double)bgH, screenRect.W / (double)bgW) * bgW);
            bgRect.H = (int)(Math.Max(screenRect.H / (double)bgH, screenRect.W / (double)bgW) * bgH);

            bgRect.X = (screenRect.W - bgRect.W) / 2;
            bgRect.Y = 0;
            renderer.RenderTexture(menuBG, bgRect, null);

            Rect logoRect = new Rect(0, 0, 0, 0);
            int logoW, logoH;
            menuLogo.Query(out logoW, out logoH);
            logoRect.W = (int)((Math.Min(screenRect.H / (double)logoH, screenRect.W / (double)logoW) * logoW) * 0.4);
            logoRect.H = (int)((Math.Min(screenRect.H / (double)logoH, screenRect.W / (double)logoW) * logoH) * 0.4);

            logoRect.X = (int)Math.Round((screenRect.W - logoRect.W) * 0.5);
            logoRect.Y = (int)Math.Round((screenRect.H - logoRect.H) * 0.15);

            renderer.RenderTexture(menuLogo, logoRect, null);

            // Draw the buttons in the main menu
            foreach (Button button in menuButtons)
            {
                button.Draw(renderer, mainMenuRect);
            }

            renderer.Present();
        }

        private void DrawGameMenu(Rect boardRect)
        {
            // Draw a dark overlay
            renderer.SetColor(new Color(0X00000077));
            renderer.FillRect(screenRect);

            // Draw the menu rectangle
            Rect menuRect1 = new Rect(0, 0, 11, 11);
            Rect menuRect2 = new Rect(0, 0, 10, 10);

            menuRect1.W = (int)Math.Round(boardRect.W * 0.41);
            menuRect1.H = (int)Math.Round(boardRect.H * 0.51);
            menuRect2.W = (int)Math.Round(boardRect.W * 0.4);
            menuRect2.H = (int)Math.Round(boardRect.H * 0.5);

            menuRect1.X = (int)Math.Round((screenRect.W - menuRect1.W) * 0.5);
            menuRect1.Y = (int)Math.Round((screenRect.H - menuRect1.H) * 0.5);
            menuRect2.X = (int)Math.Round((screenRect.W - menuRect2.W) * 0.5);
            menuRect2.Y = (int)Math.Round((screenRect.H - menuRect2.H) * 0.5);

            renderer.SetColor(new Color(0XFFFFFFAA));
            renderer.FillRect(menuRect1);
            renderer.SetColor(new Color(0X000000DD));
            renderer.FillRect(menuRect2);

            // Draw the buttons in the menu
            foreach (Button button in buttons)
            {
                button.Draw(renderer, boardRect);
            }
        }

        private void DrawBoard(Board board, Rect dst)
        {
            // Fill the rectangle with white
            renderer.SetColor(Graphics.white);
            renderer.FillRect(dst);

            // Draw the black squares & pieces
            renderer.SetColor(Graphics.black);
            int x, y;
            int w = (int)Math.Round(dst.W / (double)board.Size);
            int h = (int)Math.Round(dst.H / (double)board.Size);
            for (int i = 0; i < board.Size; i++)
            {
                y = dst.H - h * i - h + dst.Y;
                for (int j = 0; j < board.Size; j++)
                {
                    bool fillSquare = (i + j & 1) == 0;
                    x = w * j + dst.X;
                    Rect square = new Rect(x, y, w, h);

                    if (fillSquare)
                    {
                        renderer.FillRect(square);
                    }

                    Piece piece = board.GetPiece(new BoardPosition(j, i));
                    if (piece != null)
                    {
                        DrawPiece(piece, square);
                    }
                }
            }
        }

        private void DrawPiece(Piece piece, Rect dst)
        {
            Rect clip = pieceClips[(int)piece.Type, (int)piece.Color];
            renderer.RenderTexture(pieceSprites, dst, clip);
        }

        private void DrawEndScreen(Rect dst)
        {
            if (gameResult == GameResults.StillGaming)
            {
                return;
            }

            doneGaming = true;
            highlightedTile = null;
            selectedTile = null;

            Rect endOverlayRect = new Rect(0, 0, 0, 0);
            endOverlayRect.W = dst.W - dst.W / 6;
            endOverlayRect.H = dst.H - dst.H / 6;
            endOverlayRect.X = (dst.W - endOverlayRect.W) / 2;
            endOverlayRect.Y = (dst.H - endOverlayRect.H) / 2;

            Rect endTextRect = new Rect(0, 0, 0, 0);
            endTextRect.W = (int)(dst.W - dst.W * 0.5);
            endTextRect.H = (int)(dst.H - dst.H * 0.75);
            endTextRect.X = (dst.W - endTextRect.W) / 2;
            endTextRect.Y = (dst.H - endTextRect.H) / 4;

            renderer.SetColor(new Color(0X00000077));
            renderer.FillRect(screenRect);
            renderer.SetColor(new Color(0X55555577));
            renderer.FillRect(endOverlayRect);

            Texture textTexture = endTextTextures[(int)gameResult];
            renderer.RenderTexture(textTexture, endTextRect, null);

            foreach (Button button in endButtons)
            {
                button.Draw(renderer, screenRect);
            }
        }

        private void DrawOverlay() {
            renderer.SetColor(new Color(0X000000FF));
            renderer.FillRect(screenRect);

            Rect overlayTextRect1 = new Rect(0, 0, 0, 0);
            overlayTextRect1.W = (int)(screenRect.W - screenRect.W * 0.5);
            overlayTextRect1.H = (int)(screenRect.H - screenRect.H * 0.75);
            overlayTextRect1.X = (screenRect.W - overlayTextRect1.W) / 2;
            overlayTextRect1.Y = (screenRect.H - overlayTextRect1.H) / 4;

            Rect overlayTextRect2 = new Rect(0, 0, 0, 0);
            overlayTextRect2.W = (int)(screenRect.W - screenRect.W * 0.5);
            overlayTextRect2.H = (int)(screenRect.H - screenRect.H * 0.9);
            overlayTextRect2.X = (int)((screenRect.W - overlayTextRect2.W) * 0.5);
            overlayTextRect2.Y = (int)((screenRect.H - overlayTextRect2.H) * 0.6);

            renderer.RenderTexture(overlayText1, overlayTextRect1, null);
            renderer.RenderTexture(overlayText2, overlayTextRect2, null);

            renderer.Present();
        }

        private void HighlightTile(Color color, Board board, Rect boardRect, BoardPosition pos)
        {
            int w = (int)Math.Round(boardRect.W / (double)board.Size);
            int h = (int)Math.Round(boardRect.H / (double)board.Size);
            int x = boardRect.X + pos.column * w;
            int y = boardRect.H + boardRect.Y - pos.row * h - h;

            Rect dst = new Rect(x, y, w, h);

            renderer.SetColor(color);
            renderer.FillRect(dst);
        }

        private void PollEvents()
        {
            events.Clear();

            SDL.SDL_Event e;
            while (SDL.SDL_PollEvent(out e) != 0)
            {
                events.Add(e);
            }

        }

        static Rect[,] GetPieceClips(Texture texture)
        {
            int tW, tH;
            texture.Query(out tW, out tH);
            Rect[,] clips = new Rect[Piece.PIECE_TYPE_COUNT, Piece.PIECE_COLOR_COUNT];

            int w = tW / Piece.PIECE_TYPE_COUNT;
            int h = tH / Piece.PIECE_COLOR_COUNT;
            for (int y = 0; y < Piece.PIECE_COLOR_COUNT; y++)
            {
                for (int x = 0; x < Piece.PIECE_TYPE_COUNT; x++)
                {
                    clips[x, y] = new Rect(x * w, y * w, w, h);
                }
            }

            return clips;
        }

        public void Quit()
        {
            StopGaming();
            Graphics.QuitGraphics();
            instance = null;
        }

        public static UI Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new UI();
                    }
                    return instance;
                }
            }
        }
    }
}
