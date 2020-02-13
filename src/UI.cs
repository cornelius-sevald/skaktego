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
        private const int SCREEN_HEIGHT = 480;

        private const string RESOURCE_PATH = "resources/";

        private static UI instance = null;
        private static readonly object padlock = new object();

        private Window window;
        private Renderer renderer;
        private Texture background;
        private Texture pieceSprites;
        private Rect[,] pieceClips;
        private List<SDL.SDL_Event> events;

        public bool Quit { get; private set; }

        UI()
        {
            Graphics.InitGraphics();

            window = new Window("skaktego", 100, 100, SCREEN_WIDTH, SCREEN_HEIGHT);
            renderer = new Renderer(window);

            events = new List<SDL.SDL_Event>();

            background = new Texture(renderer, "background.png");
            pieceSprites = new Texture(renderer, "pieces.png");
            pieceClips = UI.GetPieceClips(pieceSprites);
            Quit = false;
        }

        public void Update(GameState gameState)
        {
            PollEvents();

            // Check if the user wants to quit.
            if (events.Any(e => e.type == SDL.SDL_EventType.SDL_QUIT)) {
                Quit = true;
            } else if (events.Any(e => e.type == SDL.SDL_EventType.SDL_KEYDOWN &&
            e.key.keysym.sym == SDL.SDL_Keycode.SDLK_ESCAPE ||
            e.key.keysym.sym == SDL.SDL_Keycode.SDLK_q)) {
                Quit = true;
            }

            Draw(gameState);

        }

        public void Draw(GameState gameState) {
            renderer.Clear();

            DrawBoard(gameState.board, renderer.OutputRect());

            renderer.Present();
        }

        private void DrawBoard(Board board, Rect dst) {
            // Fill the rectangle with white
            renderer.SetColor(Graphics.white);
            renderer.FillRect(dst);

            // Draw the black squares & pieces
            renderer.SetColor(Graphics.black);
            int x, y;
            int w = (int)Math.Ceiling(dst.W / (double)board.Size);
            int h = (int)Math.Ceiling(dst.H / (double)board.Size);
            for (int i = 0; i < board.Size; i++) {
                y = dst.H - h * i - h;
                for (int j = 0; j < board.Size; j++) {
                    bool fillSquare = (i + j & 1) == 0;
                    x = w * j;
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

        private void PollEvents() {
            events.Clear();

            SDL.SDL_Event e;
            while (SDL.SDL_PollEvent(out e) != 0)
            {
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