using System;

using SDL2;

namespace skaktego.Graphical {

    public class Window {
        public IntPtr WinPtr { get; private set; }

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

        // Set the window icon
        public void SetWindowIcon(Surface surf) {
            SDL.SDL_SetWindowIcon(WinPtr, surf.SurfPtr);
        }

        ~Window() {
            SDL.SDL_DestroyWindow(WinPtr);
        }
    }

}