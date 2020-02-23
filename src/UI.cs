using System;
using System.Linq;
using System.Collections.Generic;
using SDL2;

namespace skaktego {

    public sealed class UI {
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
        private Rect[,] pieceClips;
        private Nullable<BoardPosition> highlightedTile = null;
        private Nullable<BoardPosition> selectedTile = null;
        private List<BoardPosition> legalMoves;
        private List<SDL.SDL_Event> events;
        private Rect screenRect = null;
        private bool isMenuActive = false;
        private bool isGaming = false;
        private Button[] buttons;
        private Button[] menuButtons;
        private GameState gameState = null;

        public bool quit = false;

        UI() {
            Graphics.InitGraphics();

            window = new Window("skaktego", 100, 100, SCREEN_WIDTH, SCREEN_HEIGHT);
            renderer = new Renderer(window);

            font = new Font("playfair-display/PlayfairDisplay-Regular.ttf", 128);

            legalMoves = new List<BoardPosition>();

            events = new List<SDL.SDL_Event>();

            buttons = new Button[]{
                new Button(3/8.0, 8/24.0, 1/4.0, 1/12.0, "Continiue", font, () => isMenuActive = false),
                new Button(3/8.0, 11/24.0, 1/4.0, 1/12.0, "New Game", font, () => Console.WriteLine("Unimplemented")),
                new Button(3/8.0, 14/24.0, 1/4.0, 1/12.0, "Main Menu", font, () => isGaming = false)
            };

            menuButtons = new Button[]{
                new Button(3/8.0, 11/24.0, 1/4.0, 1/12.0, "Play Game", font, () => {isGaming = true; isMenuActive = false;}),
                new Button(3/8.0, 14/24.0, 1/4.0, 1/12.0, "  Exit  ", font, () => quit = true)
            };

            background = new Texture(renderer, "background.png");
            menuLogo = new Texture(renderer, "skaktegoLogo.png");
            menuBG = new Texture(renderer, "skaktegoMain.png");
            pieceSprites = new Texture(renderer, "pieces.png");
            pieceClips = UI.GetPieceClips(pieceSprites);
        }

        public void GameStart(GameState gameState) {
            this.gameState = GameState.FromString(gameState.ToString());
        }

        public void Update() {
            PollEvents();

            // Update screen rect
            screenRect = renderer.OutputRect();

            // Check if the user wants to quit.
            if (events.Any(e => e.type == SDL.SDL_EventType.SDL_QUIT)) {
                quit = true;
            } else if (events.Any(e => e.type == SDL.SDL_EventType.SDL_KEYDOWN &&
            e.key.keysym.sym == SDL.SDL_Keycode.SDLK_q)) {
                quit = true;
            }

            if (isGaming) {
                gameState = UpdateGame(gameState);
            } else {
                UpdateMainMenu();
            }

        }

        private GameState UpdateGame(GameState gameState) {
            int mouseX, mouseY;
            SDL.SDL_GetMouseState(out mouseX, out mouseY);

            GameState newGameState = gameState;

            if (events.Any(e => e.type == SDL.SDL_EventType.SDL_KEYDOWN &&
          e.key.keysym.sym == SDL.SDL_Keycode.SDLK_ESCAPE)) {
                isMenuActive = !isMenuActive;
            }

            Rect boardRect = new Rect(0, 0, 0, 0);
            boardRect.W = Math.Min(screenRect.H, screenRect.W);
            boardRect.H = Math.Min(screenRect.H, screenRect.W);

            boardRect.X = (int)Math.Round((screenRect.W - boardRect.W) * 0.5);
            boardRect.Y = (int)Math.Round((screenRect.H - boardRect.H) * 0.5);

            int boardMouseX = (int)Math.Floor((mouseX - boardRect.X) / (double)boardRect.W * gameState.board.Size);
            int boardMouseY = -1 + gameState.board.Size - ((int)Math.Floor((mouseY - boardRect.Y) / (double)boardRect.H * gameState.board.Size));

            if (isMenuActive) {
                foreach (Button button in buttons) {
                    button.Update(mouseX, mouseY, boardRect, events);
                }
            } else {
                if (0 <= boardMouseX && boardMouseX < gameState.board.Size) {
                    if (0 <= boardMouseY && boardMouseY < gameState.board.Size) {
                        highlightedTile = new BoardPosition(boardMouseX, boardMouseY);
                    } else {
                        highlightedTile = null;
                    }
                } else {
                    highlightedTile = null;
                }

                if (events.Any(e => e.type == SDL.SDL_EventType.SDL_MOUSEBUTTONUP)) {
                    // If a tile is already selected, attempt to apply the move
                    if (selectedTile.HasValue && highlightedTile.HasValue) {
                        ChessMove move = new ChessMove(selectedTile.Value, highlightedTile.Value);
                        newGameState = Engine.ApplyMove(gameState, move, true);
                        if (Engine.IsCheckmate(newGameState)) {
                            Console.WriteLine("der er checkmate bros - du vinder :)");
                        } else if (Engine.IsTie(newGameState)) {
                            Console.WriteLine("det står lige - du vinder ikke :(");
                        }
                    }
                    SelectTile(newGameState, highlightedTile);
                }
            }
            DrawGame(newGameState);
            return newGameState;
        }

        private void UpdateMainMenu() {
            int mouseX, mouseY;
            SDL.SDL_GetMouseState(out mouseX, out mouseY);


            Rect mainMenuRect = new Rect(0, 0, 0, 0);
            mainMenuRect.W = Math.Min(screenRect.H, screenRect.W);
            mainMenuRect.H = Math.Min(screenRect.H, screenRect.W);

            mainMenuRect.X = (int)Math.Round((screenRect.W - mainMenuRect.W) * 0.5);
            mainMenuRect.Y = (int)Math.Round((screenRect.H - mainMenuRect.H) * 0.5);

            foreach (Button button in menuButtons) {
                button.Update(mouseX, mouseY, mainMenuRect, events);
            }

            DrawMainMenu();
        }

        private void SelectTile(GameState gameState, Nullable<BoardPosition> tile) {
            selectedTile = highlightedTile;
            if (selectedTile.HasValue) {
                legalMoves = Engine.GetLegalMoves(gameState, selectedTile.Value);
            }
        }

        public void DrawGame(GameState gameState) {
            // Draw the background
            // and clear the screen
            renderer.SetColor(new Color(0X111111));
            renderer.Clear();

            //Screen geometry only during gameplay
            Rect boardRect = new Rect(0, 0, 0, 0);
            boardRect.W = Math.Min(screenRect.H, screenRect.W);
            boardRect.H = Math.Min(screenRect.H, screenRect.W);

            // Draw the board
            boardRect.X = (int)Math.Round((screenRect.W - boardRect.W) * 0.5);
            boardRect.Y = (int)Math.Round((screenRect.H - boardRect.H) * 0.5);
            DrawBoard(gameState.board, boardRect);

            // Draw the higlighted, selected and legal tiles
            if (highlightedTile != null) {
                HighlightTile(new Color(0XFFFF0055), gameState.board, boardRect, highlightedTile.Value);
            }
            if (selectedTile != null) {
                HighlightTile(new Color(0X0000FF55), gameState.board, boardRect, selectedTile.Value);
            }
            if (legalMoves != null) {
                foreach (BoardPosition legalTile in legalMoves) {
                    HighlightTile(new Color(0X11FF1155), gameState.board, boardRect, legalTile);
                }
            }


            // Draw the in game menu, if it is active
            if (isMenuActive) {
                DrawGameMenu(boardRect);
            }
            renderer.Present();
        }

        private void DrawMainMenu() {
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
            foreach (Button button in menuButtons) {
                button.Draw(renderer, mainMenuRect);
            }

            renderer.Present();
        }

        private void DrawGameMenu(Rect boardRect) {
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
            foreach (Button button in buttons) {
                button.Draw(renderer, boardRect);
            }
        }

        private void DrawBoard(Board board, Rect dst) {
            // Fill the rectangle with white
            renderer.SetColor(Graphics.white);
            renderer.FillRect(dst);

            // Draw the black squares & pieces
            renderer.SetColor(Graphics.black);
            int x, y;
            int w = (int)Math.Round(dst.W / (double)board.Size);
            int h = (int)Math.Round(dst.H / (double)board.Size);
            for (int i = 0; i < board.Size; i++) {
                y = dst.H - h * i - h + dst.Y;
                for (int j = 0; j < board.Size; j++) {
                    bool fillSquare = (i + j & 1) == 0;
                    x = w * j + dst.X;
                    Rect square = new Rect(x, y, w, h);

                    if (fillSquare) {
                        renderer.FillRect(square);
                    }

                    Piece piece = board.GetPiece(new BoardPosition(j, i));
                    if (piece != null) {
                        DrawPiece(piece, square);
                    }
                }
            }
        }

        private void DrawPiece(Piece piece, Rect dst) {
            Rect clip = pieceClips[(int)piece.Type, (int)piece.Color];
            renderer.RenderTexture(pieceSprites, dst, clip);
        }

        private void HighlightTile(Color color, Board board, Rect boardRect, BoardPosition pos) {
            int w = (int)Math.Round(boardRect.W / (double)board.Size);
            int h = (int)Math.Round(boardRect.H / (double)board.Size);
            int x = boardRect.X + pos.column * w;
            int y = boardRect.H + boardRect.Y - pos.row * h - h;

            Rect dst = new Rect(x, y, w, h);

            renderer.SetColor(color);
            renderer.FillRect(dst);
        }

        private void PollEvents() {
            events.Clear();

            SDL.SDL_Event e;
            while (SDL.SDL_PollEvent(out e) != 0) {
                events.Add(e);
            }

        }

        static Rect[,] GetPieceClips(Texture texture) {
            int tW, tH;
            texture.Query(out tW, out tH);
            Rect[,] clips = new Rect[Piece.PIECE_TYPE_COUNT, Piece.PIECE_COLOR_COUNT];

            int w = tW / Piece.PIECE_TYPE_COUNT;
            int h = tH / Piece.PIECE_COLOR_COUNT;
            for (int y = 0; y < Piece.PIECE_COLOR_COUNT; y++) {
                for (int x = 0; x < Piece.PIECE_TYPE_COUNT; x++) {
                    clips[x, y] = new Rect(x * w, y * w, w, h);
                }
            }

            return clips;
        }

        public void Quit() {
            Graphics.QuitGraphics();
            instance = null;
        }

        public static UI Instance {
            get {
                lock (padlock) {
                    if (instance == null) {
                        instance = new UI();
                    }
                    return instance;
                }
            }
        }
    }


}
