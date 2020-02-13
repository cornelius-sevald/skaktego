using System;
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

        public bool Quit { get; private set; }

        UI()
        {
            Graphics.InitGraphics();

            window = new Window("skaktego", 100, 100, SCREEN_WIDTH, SCREEN_HEIGHT);
            renderer = new Renderer(window);

            background = new Texture(renderer, "background.png");
            pieceSprites = new Texture(renderer, "pieces.png");
            pieceClips = UI.GetPieceClips(pieceSprites);
            Quit = false;
        }

        public void Update(GameState gameState)
        {
            // The main loop
            SDL.SDL_Event e;
            // Poll events
            while (SDL.SDL_PollEvent(out e) != 0)
            {
                switch (e.type)
                {
                    // Check for quit event
                    case SDL.SDL_EventType.SDL_QUIT:
                        Quit = true;
                        break;
                    case SDL.SDL_EventType.SDL_KEYDOWN:
                        switch (e.key.keysym.sym)
                        {
                            // Check if user pressed 'q' or escape
                            case SDL.SDL_Keycode.SDLK_ESCAPE:
                            case SDL.SDL_Keycode.SDLK_q:
                                Quit = true;
                                break;
                        }
                        break;
                }
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

            // Draw the black squares
            renderer.SetColor(Graphics.black);
            int x, y;
            int w = (int)Math.Ceiling(dst.W / (double)board.Size);
            int h = (int)Math.Ceiling(dst.H / (double)board.Size);
            for (int i = 0; i < board.Size; i++) {
                y = h * i;
                for (int j = 0; j < board.Size; j += 2) {
                    x = w * j;
                    // If it is an odd row, nudge the black squares
                    // one to the right.
                    if ((i & 1) == 0) {
                        x += w;
                    }
                    Rect square = new Rect(x, y, w, h);
                    renderer.FillRect(square);
                }
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