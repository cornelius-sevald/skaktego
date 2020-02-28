using System;

using SDL2;

namespace skaktego.Graphical {

    public static class Graphics {
        public const string RESOURCE_PATH = "resources/";

        public static Color white = new Color(0xFFFFFFFF);
        public static Color gray  = new Color(0x888888FF);
        public static Color black = new Color(0x000000FF);

        public static void InitGraphics() {
            // Initialize SDL
            if (SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING) != 0) {
                throw new SDLException("SDL_Init");
            }

            // Initialize SDL PNG image loading
            if ((SDL_image.IMG_Init(SDL_image.IMG_InitFlags.IMG_INIT_PNG) &
                               (int)SDL_image.IMG_InitFlags.IMG_INIT_PNG) !=
                               (int)SDL_image.IMG_InitFlags.IMG_INIT_PNG) {
                throw new SDLException("IMG_Init");
            }

            // Initialize SDL TTF rendering
            if (SDL_ttf.TTF_Init() != 0) {
                throw new SDLException("TTF_Init");
            }
        }

        public static void QuitGraphics() {
            // Quit SDL
            SDL.SDL_Quit();

            // Quit SDL_image
            SDL_image.IMG_Quit();

            // Quit SDL_ttf
            SDL_ttf.TTF_Quit();
        }

    }

}