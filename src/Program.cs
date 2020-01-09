using System;
using SDL2;

namespace skaktego
{
    class Program
    {
        const int SCREEN_WIDTH = 512;
        const int SCREEN_HEIGHT = 512;

        static void Main(string[] args)
        {
            var window = IntPtr.Zero;
            var screen = IntPtr.Zero;
            var image = IntPtr.Zero;

            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
            {
                Console.WriteLine("SDL could not initialize! SDL_Error: {0}", SDL.SDL_GetError());
                return;
            }

            image = SDL.SDL_LoadBMP("resources/hello.bmp");
            if (image == IntPtr.Zero)
            {
                System.Console.WriteLine("Unable to load image {0}! SDL Error: {1}",
                                         "resources/hello.bmp",
                                         SDL.SDL_GetError());
                return;
            }

            window = SDL.SDL_CreateWindow("skaktego",
                SDL.SDL_WINDOWPOS_CENTERED,
                SDL.SDL_WINDOWPOS_CENTERED,
                SCREEN_WIDTH,
                SCREEN_HEIGHT,
                SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL
            );
            if (window == IntPtr.Zero)
            {
                System.Console.WriteLine("Window could not be created! SDL_Error: {0}", SDL.SDL_GetError());
                return;
            }

            screen = SDL.SDL_GetWindowSurface(window);
            SDL.SDL_BlitSurface(image, IntPtr.Zero, screen, IntPtr.Zero);

            SDL.SDL_Event e;
            bool quit = false;
            while (!quit)
            {
                SDL.SDL_UpdateWindowSurface(window);
                while (SDL.SDL_PollEvent(out e) != 0)
                {
                    switch (e.type)
                    {
                        case SDL.SDL_EventType.SDL_QUIT:
                            quit = true;
                            break;
                        case SDL.SDL_EventType.SDL_KEYDOWN:
                            switch (e.key.keysym.sym)
                            {
                                case SDL.SDL_Keycode.SDLK_q:
                                    quit = true;
                                    break;
                            }
                            break;
                    }
                }
            }

            SDL.SDL_DestroyWindow(window);
            SDL.SDL_Quit();
        }
    }
}
