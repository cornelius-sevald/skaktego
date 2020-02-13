using System;
using System.IO;
using System.Text;
using SDL2;

namespace skaktego {
    class Program {
        private const int SCREEN_WIDTH = 640;
        private const int SCREEN_HEIGHT = 480;

        // Size of background tiles
        const int TILE_SIZE = 160;
        // Path of game resources

        static int Main(string[] args) {
            DateTime start = DateTime.Now;

            Graphics.InitGraphics();

            Window window = new Window("skaktego", 100, 100, SCREEN_WIDTH, SCREEN_HEIGHT);
            Renderer renderer = new Renderer(window);

            // Load three images
            Texture background   = new Texture(renderer, "background.png");
            Texture image        = new Texture(renderer, "image.png");
            Texture pieceSprites = new Texture(renderer, "pieces.png");

            // Get the clips of the piece sprites.
            Rect[,] pieceClips = GetPieceClips(pieceSprites);

            // The image offset
            int xOffset = 0, yOffset = 0;
            ChessColors currentColor = ChessColors.White;

            // The main loop
            SDL.SDL_Event e;
            bool quit = false;
            while (!quit) {
                // Poll events
                while (SDL.SDL_PollEvent(out e) != 0) {
                    switch (e.type) {
                        // Check for quit event
                        case SDL.SDL_EventType.SDL_QUIT:
                            quit = true;
                            break;
                        case SDL.SDL_EventType.SDL_KEYDOWN:
                            switch (e.key.keysym.sym) {
                                // Check if user pressed 'q'
                                case SDL.SDL_Keycode.SDLK_ESCAPE:
                                case SDL.SDL_Keycode.SDLK_q:
                                    quit = true;
                                    break;
                                // Check wasd & arrow keys
                                // and offset image accordingly
                                case SDL.SDL_Keycode.SDLK_UP:
                                case SDL.SDL_Keycode.SDLK_w:
                                    yOffset -= 10;
                                    break;
                                case SDL.SDL_Keycode.SDLK_LEFT:
                                case SDL.SDL_Keycode.SDLK_a:
                                    xOffset -= 10;
                                    break;
                                case SDL.SDL_Keycode.SDLK_DOWN:
                                case SDL.SDL_Keycode.SDLK_s:
                                    yOffset += 10;
                                    break;
                                case SDL.SDL_Keycode.SDLK_RIGHT:
                                case SDL.SDL_Keycode.SDLK_d:
                                    xOffset += 10;
                                    break;
                                case SDL.SDL_Keycode.SDLK_SPACE:
                                    currentColor = currentColor == ChessColors.White ? ChessColors.Black : ChessColors.White;
                                    break;
                            }
                            break;
                    }
                }

                renderer.Clear();

                {
                    // Determine how many tiles we'll need to fill the screen
                    int xTiles = SCREEN_WIDTH / TILE_SIZE;
                    int yTiles = SCREEN_HEIGHT / TILE_SIZE;

                    // Draw the tiles by calculating their positions
                    for (int i = 0; i < xTiles * yTiles; ++i) {
                        int xT = i % xTiles;
                        int yT = i / xTiles;
                        renderer.RenderTexture(background,
                                xT * TILE_SIZE, yT * TILE_SIZE, TILE_SIZE,
                                TILE_SIZE);
                    }
                }

                // Draw a moving chess piece.
                {
                    DateTime now = DateTime.Now;
                    Rect pieceDst = new Rect(0, 0, 200, 200);
                    pieceDst.XPos = ((int)(now.Subtract(start).TotalMilliseconds * 0.3) % (SCREEN_WIDTH + 100)) - 100;
                    pieceDst.YPos = (int)(now.Subtract(start).TotalMilliseconds * 0.25) % (SCREEN_HEIGHT + 100) - 100;
                    int t = ((int)now.Second / 2) % Piece.PIECE_TYPE_COUNT;
                    int c = (int) currentColor;
                    renderer.RenderTexture(pieceSprites, pieceDst, pieceClips[t, c]);
                }

                // Draw the foreground image
                {
                    int iW, iH;
                    image.Query(out iW, out iH);
                    int x = SCREEN_WIDTH / 2 - iW / 2 + xOffset;
                    int y = SCREEN_HEIGHT / 2 - iH / 2 + yOffset;
                    renderer.RenderTexture(image, x, y);
                }

                renderer.Present();
            }

            return 0;
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


    }
}
