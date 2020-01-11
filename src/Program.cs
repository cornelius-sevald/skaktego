using System;
using System.IO;
using System.Text;
using SDL2;

namespace skaktego {
    class Program {
        const int SCREEN_WIDTH = 640;
        const int SCREEN_HEIGHT = 480;
        // Size of background tiles.
        const int TILE_SIZE = 160;
        const string RESOURCE_PATH = "resources/";

        static TextWriter stdout = Console.Out;

        static int Main(string[] args) {
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

            // Load two images
            IntPtr background = LoadTexture("background.png", renderer);
            IntPtr image = LoadTexture("image.png", renderer);
            if (background == IntPtr.Zero || image == IntPtr.Zero) {
                if (background != IntPtr.Zero) {
                    SDL.SDL_FreeSurface(background);
                }
                if (image != IntPtr.Zero) {
                    SDL.SDL_DestroyTexture(image);
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

            // The image offset
            int xOffset = 0, yOffset = 0;

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
                            }
                            break;
                    }
                }


                SDL.SDL_RenderClear(renderer);

                // Determine how many tiles we'll need to fill the screen
                int xTiles = SCREEN_WIDTH / TILE_SIZE;
                int yTiles = SCREEN_HEIGHT / TILE_SIZE;

                // Draw the tiles by calculating their positions
                for (int i = 0; i < xTiles * yTiles; ++i){
                    int xT = i % xTiles;
                    int yT = i / xTiles;
                    RenderTexture(background, renderer,
                            xT * TILE_SIZE, yT * TILE_SIZE, TILE_SIZE,
                            TILE_SIZE);
                }

                // Draw the foreground image
                int iW, iH;
                SDL.SDL_QueryTexture(image, out _, out _, out iW, out iH);
                int x = SCREEN_WIDTH / 2 - iW / 2 + xOffset;
                int y = SCREEN_HEIGHT / 2 - iH / 2 + yOffset;
                RenderTexture(image, renderer, x, y);

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
