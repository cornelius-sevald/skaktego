using System;
using System.IO;
using SDL2;

namespace skaktego {

    public static class Graphics {
        public const string RESOURCE_PATH = "resources/";

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
        }

    }

    public class Window {
        public IntPtr WinPtr { get; private set; }

        public Window(string title, int xPos, int yPos, int width, int height) {
            // Create window
            IntPtr window = SDL.SDL_CreateWindow(title, xPos, yPos, width, height,
                    SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN |
                    SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
            if (window == IntPtr.Zero) {
                throw new SDLException("CreateWindow");
            }

            WinPtr = window;
        }

        ~Window() {
            SDL.SDL_DestroyWindow(WinPtr);
        }
    }

    public class Renderer {
        public IntPtr RenPtr { get; private set; }

        public Renderer(Window window) {
            // Create renderer
            IntPtr renderer = SDL.SDL_CreateRenderer(window.WinPtr, -1,
                    SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED |
                    SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);
            if (renderer == IntPtr.Zero) {
                throw new SDLException("CreateRenderer");
            }

            RenPtr = renderer;
        }

        public void Clear() {
            SDL.SDL_RenderClear(RenPtr);
        }

        public void Present() {
            SDL.SDL_RenderPresent(RenPtr);
        }

        /// <summary>
        /// Draw a Texture to this Renderer.
        /// </summary>
        /// <param name="texture">The source texture we want to draw</param>
        public void RenderTexture(Texture texture) {
            RenderTexture(texture, null, null);
        }

        /// <summary>
        /// Draw a Texture to this Renderer at some destination Rect
        /// taking a clip of the texture if desired
        /// </summary>
        /// <param name="texture">The source texture we want to draw</param>
        /// <param name="dst">The destination rectangle to render the texture to</param>
        /// <param name="clip">The sub-section of the texture to draw (clipping rect)
        /// default of null draws the entire texture</param>
        public void RenderTexture(Texture texture, Rect dst, Rect clip) {
            if (dst != null) {
                SDL.SDL_Rect _dst = dst.Rct;
                if (clip != null) {
                    SDL.SDL_Rect _clip = clip.Rct;
                    SDL.SDL_RenderCopy(RenPtr, texture.TexPtr, ref _clip, ref _dst);
                } else {
                    SDL.SDL_RenderCopy(RenPtr, texture.TexPtr, IntPtr.Zero, ref _dst);
                }
            } else {
                if (clip != null) {
                    SDL.SDL_Rect _clip = clip.Rct;
                    SDL.SDL_RenderCopy(RenPtr, texture.TexPtr, ref _clip, IntPtr.Zero);
                } else {
                    SDL.SDL_RenderCopy(RenPtr, texture.TexPtr, IntPtr.Zero, IntPtr.Zero);
                }
            }
        }

        /// <summary>
        /// Draw a Texture to this Renderer at position x, y, with some desired
        /// width and height
        /// </summary>
        /// <param name="tex">The source texture we want to draw</param>
        /// <param name="x">The x coordinate to draw to</param>
        /// <param name="y">The y coordinate to draw to</param>
        /// <param name="w">The width of the texture to draw</param>
        /// <param name="h">The height of the texture to draw</param>
        public void RenderTexture(Texture texture, int x, int y, int w, int h) {
            //Setup the destination rectangle to be at the position we want
            Rect dst = new Rect(x, y, h, w);
            RenderTexture(texture, dst, null);
        }

        /// <summary>
        /// Draw a Texture to this Renderer at position x, y, preserving
        /// the texture's width and height
        /// </summary>
        /// <param name="texture">The source texture we want to draw</param>
        /// <param name="x">The x coordinate to draw to</param>
        /// <param name="y">The y coordinate to draw to</param>
        public void RenderTexture(Texture texture, int x, int y) {
            int w, h;
            texture.Query(out w, out h);
            RenderTexture(texture, x, y, w, h);
        }

        ~Renderer() {
            SDL.SDL_DestroyRenderer(RenPtr);
        }
    }

    public class Texture {
        public IntPtr TexPtr { get; private set; }

        public Texture(Renderer renderer, string name) {
            // Construct the full path
            string path = Path.Combine(Graphics.RESOURCE_PATH, name);

            IntPtr texture = SDL_image.IMG_LoadTexture(renderer.RenPtr, path);
            if (texture == IntPtr.Zero) {
                throw new SDLException("LoadTexture");
            }

            TexPtr = texture;
        }

        public void Query(out int w, out int h) {
            SDL.SDL_QueryTexture(TexPtr, out _, out _, out w, out h);
        }

        ~Texture() {
            SDL.SDL_DestroyTexture(TexPtr);
        }
    }

    public class Rect {
        public SDL.SDL_Rect Rct { get; private set; }
        public int XPos {
            get { return Rct.x; }
            set { Rct = new SDL.SDL_Rect { x = value, y = YPos, w = Width, h = Height }; }
        }

        public int YPos {
            get { return Rct.y; }
            set { Rct = new SDL.SDL_Rect { x = XPos, y = value, w = Width, h = Height }; }
        }

        public int Width {
            get { return Rct.w; }
            set { Rct = new SDL.SDL_Rect { x = XPos, y = YPos, w = value, h = Height }; }
        }

        public int Height {
            get { return Rct.h; }
            set { Rct = new SDL.SDL_Rect { x = XPos, y = YPos, w = Width, h = value }; }
        }

        public Rect(int xPos, int yPos, int width, int height) {
            XPos   = xPos;
            YPos   = yPos;
            Width  = width;
            Height = height;
        }
    }
    
    public class SDLException : Exception
    {
        public SDLException(string message)
           : base(message + "error: " + SDL.SDL_GetError())
        {
        }
    }

}