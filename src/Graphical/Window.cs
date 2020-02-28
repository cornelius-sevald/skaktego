using System;

using SDL2;

namespace skaktego.Graphical {

    /// <summary>
    /// Wrapper of an SDL window
    /// </summary>
    public class Window {

        /// <summary>
        /// Pointer to the internal SDL_Window
        /// </summary>
        public IntPtr WinPtr { get; private set; }

        /// <summary>
        /// Construct a new window given a title and starting pixel dimentions
        /// </summary>
        /// <param name="title">The title of the window</param>
        /// <param name="x">The horizontal offset from the left edge of the screen</param>
        /// <param name="y">The vertical offset from the top edge of the screen</param>
        /// <param name="w">The width of the window</param>
        /// <param name="h">The height of the window</param>
        public Window(string title, int x, int y, int w, int h) {
            // Create window
            IntPtr window = SDL.SDL_CreateWindow(title, x, y, w, h,
                    SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN |
                    SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
            if (window == IntPtr.Zero) {
                throw new SDLException("SDL_CreateWindow");
            }

            WinPtr = window;
        }

        /// <summary>
        /// Set the window icon
        /// </summary>
        /// <param name="surf">Surface containing the pixel data</param>
        public void SetWindowIcon(Surface surf) {
            SDL.SDL_SetWindowIcon(WinPtr, surf.SurfPtr);
        }

        ~Window() {
            SDL.SDL_DestroyWindow(WinPtr);
        }
    }

}