using System;
using System.IO;
using System.Text;
using SDL2;

namespace skaktego {
    class Program {
        const int SCREEN_WIDTH = 640;
        const int SCREEN_HEIGHT = 480;
        const string RESOURCE_PATH = "resources/";

        static TextWriter stdout = Console.Out;

        static int Main(string[] args) {
            if (SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING) != 0) {
                LogSDLError(stdout, "SDL_Init");
                return 1;
            }

            IntPtr window = SDL.SDL_CreateWindow("skaktego", 100, 100, SCREEN_WIDTH,
                SCREEN_HEIGHT, SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);
            if (window == IntPtr.Zero) {
                LogSDLError(stdout, "CreateWindow");
                SDL.SDL_Quit();
                return 1;
            }
            IntPtr renderer = SDL.SDL_CreateRenderer(window, -1,
                SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED | SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);
            if (renderer == IntPtr.Zero) {
                LogSDLError(stdout, "CreateRenderer");
                SDL.SDL_DestroyWindow(window);
                SDL.SDL_Quit();
                return 1;
            }

            IntPtr background = LoadTexture("background.bmp", renderer);
            IntPtr image = LoadTexture("image.bmp", renderer);
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
                SDL.SDL_Quit();
                return 1;
            }

            // Put the image coordinates at the center of the screen.
            int iW, iH;
            SDL.SDL_QueryTexture(image, out _, out _, out iW, out iH);

            SDL.SDL_Event e;
            bool quit = false;
            while (!quit) {
                while (SDL.SDL_PollEvent(out e) != 0) {
                    switch (e.type) {
                        case SDL.SDL_EventType.SDL_QUIT:
                            quit = true;
                            break;
                        case SDL.SDL_EventType.SDL_KEYDOWN:
                            switch (e.key.keysym.sym) {
                                case SDL.SDL_Keycode.SDLK_q:
                                    quit = true;
                                    break;
                                case SDL.SDL_Keycode.SDLK_UP:
                                case SDL.SDL_Keycode.SDLK_w:
                                    iH += 10;
                                    break;
                                case SDL.SDL_Keycode.SDLK_LEFT:
                                case SDL.SDL_Keycode.SDLK_a:
                                    iW += 10;
                                    break;
                                case SDL.SDL_Keycode.SDLK_DOWN:
                                case SDL.SDL_Keycode.SDLK_s:
                                    iH -= 10;
                                    break;
                                case SDL.SDL_Keycode.SDLK_RIGHT:
                                case SDL.SDL_Keycode.SDLK_d:
                                    iW -= 10;
                                    break;
                            }
                            break;
                    }
                }


                SDL.SDL_RenderClear(renderer);

                // Render 4 copies of the background.
                int bW, bH;
                SDL.SDL_QueryTexture(background, out _, out _, out bW, out bH);
                renderTexture(background, renderer, 0, 0);
                renderTexture(background, renderer, bW, 0);
                renderTexture(background, renderer, 0, bH);
                renderTexture(background, renderer, bW, bH);

                int x = SCREEN_WIDTH / 2 - iW / 2;
                int y = SCREEN_HEIGHT / 2 - iH / 2;
                renderTexture(image, renderer, x, y);

                SDL.SDL_RenderPresent(renderer);
            }


            SDL.SDL_FreeSurface(background);
            SDL.SDL_DestroyTexture(image);
            SDL.SDL_DestroyRenderer(renderer);
            SDL.SDL_DestroyWindow(window);
            SDL.SDL_Quit();
            return 0;
        }

        /**
         * Log an SDL error with some error message to the output stream of our choice
         * @param os The output stream to write the message to
         * @param msg The error message to write, format will be msg error: SDL_GetError()
         */
        static void LogSDLError(TextWriter writer, string msg) {
            using (writer) {
                writer.WriteLine(msg + " error: " + SDL.SDL_GetError());
            }
        }

        /// <summary>
        /// Load a BMP image into a texture on the rendering device
        /// </summary>
        /// <param name="file">The BMP image file to load</param>
        /// <param name="ren">The renderer to load the texture onto</param>
        /// <returns>The loaded texture, or <c>IntPtr.Zero</c> if something went wrong</returns>
        static IntPtr LoadTexture(string file, IntPtr ren) {
            // Construct the full path
            string path = Path.Combine(RESOURCE_PATH, file);
            //Initialize to null to avoid dangling pointer issues
            IntPtr texture = IntPtr.Zero;
            //Load the image
            IntPtr loadedImage = SDL.SDL_LoadBMP(path);
            //If the loading went ok, convert to texture and return the texture
            if (loadedImage != IntPtr.Zero) {
                texture = SDL.SDL_CreateTextureFromSurface(ren, loadedImage);
                SDL.SDL_FreeSurface(loadedImage);
                //Make sure converting went ok too
                if (texture == IntPtr.Zero) {
                    LogSDLError(stdout, "CreateTextureFromSurface");
                }
            } else {
                LogSDLError(stdout, "LoadBMP");
            }
            return texture;
        }

        /// <summary>
        /// Draw an SDL_Texture to an SDL_Renderer at position x, y, preserving
        /// the texture's width and height
        /// </summary>
        /// <param name="tex">The source texture we want to draw</param>
        /// <param name="ren">The renderer we want to draw to</param>
        /// <param name="x">The x coordinate to draw to</param>
        /// <param name="y">The y coordinate to draw to</param>
        static void renderTexture(IntPtr tex, IntPtr ren, int x, int y) {
            //Setup the destination rectangle to be at the position we want
            SDL.SDL_Rect dst;
            dst.x = x;
            dst.y = y;
            //Query the texture to get its width and height to use
            SDL.SDL_QueryTexture(tex, out _, out _, out dst.w, out dst.h);
            SDL.SDL_RenderCopy(ren, tex, IntPtr.Zero, ref dst);
        }
    }
}
