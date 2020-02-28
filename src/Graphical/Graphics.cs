using System;

using SDL2;

namespace skaktego.Graphical {

    /// <summary>
    /// General graphics class
    /// 
    /// <para>This class takes care of initializing
    /// and de-initializing SDL, as well as other general
    /// graphics-related things.</para>
    /// </summary>
    public static class Graphics {

        /// <summary>
        /// The path to the resource folder.
        /// 
        /// <para>This is where all textures, fonts, etc.
        /// be located</para>
        /// </summary>
        public const string RESOURCE_PATH = "resources/";

        /// <summary>
        /// A pure white color
        /// </summary>
        public static Color white = new Color(0xFFFFFFFF);

        /// <summary>
        /// A mid-gray color
        /// </summary>
        /// <returns></returns>
        public static Color gray  = new Color(0x888888FF);
        
        /// <summary>
        /// A completely black color
        /// </summary>
        /// <returns></returns>
        public static Color black = new Color(0x000000FF);

        /// <summary>
        /// Initialize SDL, SDL_image and SDL_ttf
        /// </summary>
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

        /// <summary>
        /// De-initialize SDL, SDL_image and SDL_ttf
        /// </summary>
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