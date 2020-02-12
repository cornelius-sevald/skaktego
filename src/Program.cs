using System;
using System.IO;
using System.Text;
using SDL2;

namespace skaktego {
    class Program {
        // Screen size
        const int SCREEN_WIDTH = 640;
        const int SCREEN_HEIGHT = 480;
        // Size of background tiles
        const int TILE_SIZE = 160;
        // Path of game resources
        const string RESOURCE_PATH = "resources/";

        static TextWriter stdout = Console.Out;

        static int Main(string[] args) {
            DateTime start = DateTime.Now;

            // Initialize SDL
            if (SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING) != 0) {
                LogSDLError(stdout, "SDL_Init");
                return 1;
            }

            // Initialize SDL PNG image loading
            if ((SDL_image.IMG_Init(SDL_image.IMG_InitFlags.IMG_INIT_PNG) &
                               (int)SDL_image.IMG_InitFlags.IMG_INIT_PNG) !=
                               (int)SDL_image.IMG_InitFlags.IMG_INIT_PNG) {
                LogSDLError(stdout, "IMG_Init");
                SDL.SDL_Quit();
                return 1;
            }

            // Create window
            IntPtr window = SDL.SDL_CreateWindow("skaktego", 100, 100,
                    SCREEN_WIDTH, SCREEN_HEIGHT,
                    SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);
            if (window == IntPtr.Zero) {
                LogSDLError(stdout, "CreateWindow");
                SDL_image.IMG_Quit();
                SDL.SDL_Quit();
                return 1;
            }

            // Create renderer
            IntPtr renderer = SDL.SDL_CreateRenderer(window, -1,
                    SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED |
                    SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);
            if (renderer == IntPtr.Zero) {
                LogSDLError(stdout, "CreateRenderer");
                SDL.SDL_DestroyWindow(window);
                SDL_image.IMG_Quit();
                SDL.SDL_Quit();
                return 1;
            }

            // Load three images
            IntPtr background = LoadTexture("background.png", renderer);
            IntPtr image = LoadTexture("image.png", renderer);
            IntPtr pieceSprites = LoadTexture("pieces.png", renderer);
            if (background == IntPtr.Zero ||
                image == IntPtr.Zero ||
                pieceSprites == IntPtr.Zero) {
                if (background != IntPtr.Zero) {
                    SDL.SDL_DestroyTexture(background);
                }
                if (image != IntPtr.Zero) {
                    SDL.SDL_DestroyTexture(image);
                }
                if (pieceSprites != IntPtr.Zero) {
                    SDL.SDL_DestroyTexture(pieceSprites);
                }
                if (renderer != IntPtr.Zero) {
                    SDL.SDL_DestroyRenderer(renderer);
                }
                if (window != IntPtr.Zero) {
                    SDL.SDL_DestroyWindow(window);
                }
                SDL_image.IMG_Quit();
                SDL.SDL_Quit();
                return 1;
            }

            // Get the clips of the piece sprites.
            SDL.SDL_Rect[,] pieceClips = GetPieceClips(pieceSprites);

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


                SDL.SDL_RenderClear(renderer);

                {
                    // Determine how many tiles we'll need to fill the screen
                    int xTiles = SCREEN_WIDTH / TILE_SIZE;
                    int yTiles = SCREEN_HEIGHT / TILE_SIZE;

                    // Draw the tiles by calculating their positions
                    for (int i = 0; i < xTiles * yTiles; ++i) {
                        int xT = i % xTiles;
                        int yT = i / xTiles;
                        RenderTexture(background, renderer,
                                xT * TILE_SIZE, yT * TILE_SIZE, TILE_SIZE,
                                TILE_SIZE);
                    }
                }

                // Draw a moving chess piece.
                {
                    DateTime now = DateTime.Now;
                    SDL.SDL_Rect pieceDst;
                    pieceDst.x = ((int)(now.Subtract(start).TotalMilliseconds * 0.3) % (SCREEN_WIDTH + 100)) - 100;
                    pieceDst.y = (int)(now.Subtract(start).TotalMilliseconds * 0.25) % (SCREEN_HEIGHT + 100) - 100;
                    pieceDst.w = 200;
                    pieceDst.h = 200;
                    int t = ((int)now.Second / 2) % Piece.PIECE_TYPE_COUNT;
                    int c = (int) currentColor;
                    RenderTexture(pieceSprites, renderer, ref pieceDst, ref pieceClips[t, c]);
                }

                // Draw the foreground image
                {
                    int iW, iH;
                    SDL.SDL_QueryTexture(image, out _, out _, out iW, out iH);
                    int x = SCREEN_WIDTH / 2 - iW / 2 + xOffset;
                    int y = SCREEN_HEIGHT / 2 - iH / 2 + yOffset;
                    RenderTexture(image, renderer, x, y);
                }

                SDL.SDL_RenderPresent(renderer);
            }

            // Clean up
            SDL.SDL_FreeSurface(background);
            SDL.SDL_DestroyTexture(image);
            SDL.SDL_DestroyRenderer(renderer);
            SDL.SDL_DestroyWindow(window);
            SDL_image.IMG_Quit();
            SDL.SDL_Quit();
            return 0;
        }

        static SDL.SDL_Rect[,] GetPieceClips(IntPtr tex) {
            int tW, tH;
            SDL.SDL_QueryTexture(tex, out _, out _, out tW, out tH);
            SDL.SDL_Rect[,] clips = new SDL.SDL_Rect[Piece.PIECE_TYPE_COUNT, Piece.PIECE_COLOR_COUNT];

            int w = tW / Piece.PIECE_TYPE_COUNT;
            int h = tH / Piece.PIECE_COLOR_COUNT;
            for (int y = 0; y < Piece.PIECE_COLOR_COUNT; y++) {
                for (int x = 0; x < Piece.PIECE_TYPE_COUNT; x++) {
                    SDL.SDL_Rect clip;
                    clip.x = x * w;
                    clip.y = y * w;
                    clip.w = w;
                    clip.h = h;
                    clips[x, y] = clip;
                }
            }

            return clips;
        }

        /// <summary>
        /// Log an SDL error with some error message to the output stream of our choice
        /// <param name="writer">The TextWriter to write to</param>
        /// <param name="msg">The message to write</param>
        static void LogSDLError(TextWriter writer, string msg) {
            using (writer) {
                writer.WriteLine(msg + " error: " + SDL.SDL_GetError());
            }
        }

        /// <summary>
        /// Load an image into a texture on the rendering device
        /// </summary>
        /// <param name="file">The image file to load</param>
        /// <param name="ren">The renderer to load the texture onto</param>
        /// <returns>The loaded texture, or <c>IntPtr.Zero</c> if something went wrong</returns>
        static IntPtr LoadTexture(string file, IntPtr ren) {
            // Construct the full path
            string path = Path.Combine(RESOURCE_PATH, file);

            IntPtr texture = SDL_image.IMG_LoadTexture(ren, path);
            if (texture == IntPtr.Zero) {
                LogSDLError(stdout, "LoadTexture");
            }
            return texture;
        }

        /// <summary>
        /// Draw an SDL_Texture to an SDL_Renderer at some destination rect
        /// taking a clip of the texture if desired
        /// </summary>
        /// <param name="tex">The source texture we want to draw</param>
        /// <param name="ren">The renderer we want to draw to</param>
        /// <param name="dst">The destination rectangle to render the texture to</param>
        /// <param name="clip">The sub-section of the texture to draw (clipping rect)
        /// default of nullptr draws the entire texture</param>
        static void RenderTexture(IntPtr tex, IntPtr ren, ref SDL.SDL_Rect dst,
                ref SDL.SDL_Rect clip) {
            SDL.SDL_RenderCopy(ren, tex, ref clip, ref dst);
        }

        /// <summary>
        /// Draw an SDL_Texture to an SDL_Renderer at position x, y, with some desired
        /// width and height
        /// </summary>
        /// <param name="tex">The source texture we want to draw</param>
        /// <param name="ren">The renderer we want to draw to</param>
        /// <param name="x">The x coordinate to draw to</param>
        /// <param name="y">The y coordinate to draw to</param>
        /// <param name="w">The width of the texture to draw</param>
        /// <param name="h">The height of the texture to draw</param>
        static void RenderTexture(IntPtr tex, IntPtr ren, int x, int y, int w, int h) {
            //Setup the destination rectangle to be at the position we want
            SDL.SDL_Rect dst;
            dst.x = x;
            dst.y = y;
            dst.w = w;
            dst.h = h;
            SDL.SDL_RenderCopy(ren, tex, IntPtr.Zero, ref dst);
        }

        /// <summary>
        /// Draw an SDL_Texture to an SDL_Renderer at position x, y, preserving
        /// the texture's width and height
        /// </summary>
        /// <param name="tex">The source texture we want to draw</param>
        /// <param name="ren">The renderer we want to draw to</param>
        /// <param name="x">The x coordinate to draw to</param>
        /// <param name="y">The y coordinate to draw to</param>
        static void RenderTexture(IntPtr tex, IntPtr ren, int x, int y) {
            int w, h;
            SDL.SDL_QueryTexture(tex, out _, out _, out w, out h);
            RenderTexture(tex, ren, x, y, w, h);
        }
    }
}
