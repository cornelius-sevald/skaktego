using System;
using System.Linq;
using System.Collections.Generic;
using SDL2;

namespace skaktego
{

    public sealed class UI
    {
        // Screen size
        private const int SCREEN_WIDTH = 640;
        private const int SCREEN_HEIGHT = 640;

        private const string RESOURCE_PATH = "resources/";

        private static UI instance = null;
        private static readonly object padlock = new object();

        private Window window;
        private Renderer renderer;
        private Font font;
        private Texture background;
        private Texture pieceSprites;
        private Rect[,] pieceClips;
        private BoardPosition highlightedTile = null;
        private BoardPosition selectedTile = null;
        private List<BoardPosition> legalMoves;
        private List<SDL.SDL_Event> events;
        private bool isMenuActive = true;
        private Button[] buttons;

        public bool quit = false;

        UI()
        {
            Graphics.InitGraphics();

            window = new Window("skaktego", 100, 100, SCREEN_WIDTH, SCREEN_HEIGHT);
            renderer = new Renderer(window);

            font = new Font("playfair-display/PlayfairDisplay-Regular.ttf", 128);

            legalMoves = new List<BoardPosition>();

            events = new List<SDL.SDL_Event>();

            buttons = new Button[]{
                new Button(3/8.0, 11/24.0, 1/4.0, 1/12.0, "ruth", font, () => Console.WriteLine("ruth"))
            };

            background = new Texture(renderer, "background.png");
            pieceSprites = new Texture(renderer, "pieces.png");
            pieceClips = UI.GetPieceClips(pieceSprites);
        }

        public void Update(GameState gameState)
        {
            PollEvents();

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
            if (events.Any(e => e.type == SDL.SDL_EventType.SDL_KEYDOWN &&
          e.key.keysym.sym == SDL.SDL_Keycode.SDLK_ESCAPE))
            {
                isMenuActive = !isMenuActive;
            }

            int mouseX, mouseY;
            SDL.SDL_GetMouseState(out mouseX, out mouseY);

            Rect screenRect = renderer.OutputRect();

            Rect boardRect = new Rect(0, 0, 0, 0);
            boardRect.W = Math.Min(screenRect.H, screenRect.W);
            boardRect.H = Math.Min(screenRect.H, screenRect.W);

            boardRect.X = (int)Math.Round((screenRect.W - boardRect.W) * 0.5);
            boardRect.Y = (int)Math.Round((screenRect.H - boardRect.H) * 0.5);

            if (!isMenuActive)
            {
                int boardMouseX = (int)Math.Floor((mouseX - boardRect.X) / (double)boardRect.W * gameState.board.Size);
                int boardMouseY = -1 + gameState.board.Size - ((int)Math.Floor((mouseY - boardRect.Y) / (double)boardRect.H * gameState.board.Size));

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

                if (events.Any(e => e.type == SDL.SDL_EventType.SDL_MOUSEBUTTONUP))
                {
                    SelectTile(gameState, highlightedTile);
                }
            }
            else
            {
                foreach (Button button in buttons)
                {
                    button.Update(mouseX, mouseY, boardRect, events);
                }
            }
            Draw(gameState);
        }

        private void SelectTile(GameState gameState, BoardPosition tile) {
            selectedTile = highlightedTile;
            legalMoves = Engine.GetLegalMoves(gameState, selectedTile);
        }

        public void Draw(GameState gameState)
        {
            // Draw the background
            // and clear the screen
            renderer.SetColor(new Color(0X111111));
            renderer.Clear();

            // Set up screen geometry
            Rect screenRect = renderer.OutputRect();
            Rect boardRect = new Rect(0, 0, 0, 0);
            boardRect.W = Math.Min(screenRect.H, screenRect.W);
            boardRect.H = Math.Min(screenRect.H, screenRect.W);

            // Draw the board
            boardRect.X = (int)Math.Round((screenRect.W - boardRect.W) * 0.5);
            boardRect.Y = (int)Math.Round((screenRect.H - boardRect.H) * 0.5);
            DrawBoard(gameState.board, boardRect);

            // Draw the higlighted, selected and legal tiles
            if (highlightedTile != null)
            {
                HighlightTile(new Color(0XFFFF0055), gameState.board, boardRect, highlightedTile);
            }
            if (selectedTile != null)
            {
                HighlightTile(new Color(0X0000FF55), gameState.board, boardRect, selectedTile);
            }
            if (legalMoves != null) {
                foreach (BoardPosition legalTile in legalMoves) {
                    HighlightTile(new Color(0X11FF1155), gameState.board, boardRect, legalTile);
                }
            }

            // Draw the menu, if it is active
            if (isMenuActive)
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

            renderer.Present();
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

        private void HighlightTile(Color color, Board board, Rect boardRect, BoardPosition pos)
        {
            int w = (int)Math.Round(boardRect.W / (double)board.Size);
            int h = (int)Math.Round(boardRect.H / (double)board.Size);
            int x = boardRect.X + pos.Column * w;
            int y = boardRect.H + boardRect.Y - pos.Row * h - h;

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

        public void Quit () {
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