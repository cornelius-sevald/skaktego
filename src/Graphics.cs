using System;
using System.IO;
using SDL2;

namespace skaktego {

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
                throw new SDLException("SDL_CreateRenderer");
            }

            SDL.SDL_SetRenderDrawBlendMode (renderer, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);
            RenPtr = renderer;
        }

        public Rect OutputRect() {
            int w, h;
            SDL.SDL_GetRendererOutputSize(RenPtr, out w, out h);
            return new Rect(0, 0, w, h);
        }


        public void Clear() {
            SDL.SDL_RenderClear(RenPtr);
        }

        public void Present() {
            SDL.SDL_RenderPresent(RenPtr);
        }

        public void SetColor(Color color) {
            SDL.SDL_SetRenderDrawColor(RenPtr, color.R, color.G, color.B, color.A);
        }

        /// <summary>
        /// Fill a rectangle with the current color.
        /// </summary>
        /// <param name="rect"></param>
        public void FillRect(Rect rect) {
            SDL.SDL_Rect _rect = rect.Rct;
            SDL.SDL_RenderFillRect(RenPtr, ref _rect);
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

    public class Texture : IDisposable {
        public IntPtr TexPtr { get; private set; }

        // Create a texture from an image path
        public Texture(Renderer renderer, string name) {
            // Construct the full path
            string path = Path.Combine(Graphics.RESOURCE_PATH, name);

            IntPtr texture = SDL_image.IMG_LoadTexture(renderer.RenPtr, path);
            if (texture == IntPtr.Zero) {
                throw new SDLException("IMG_LoadTexture");
            }

            TexPtr = texture;
        }

        // Create a texture from a surface
        public Texture(Renderer renderer, Surface surface) {
            IntPtr texture = SDL.SDL_CreateTextureFromSurface(renderer.RenPtr, surface.SurfPtr);
            if (texture == IntPtr.Zero) {
                throw new SDLException("SDL_CreateTextureFromSurface");
            }

            TexPtr = texture;
        }

        public void Query(out int w, out int h) {
            SDL.SDL_QueryTexture(TexPtr, out _, out _, out w, out h);
        }

        protected virtual void Dispose(bool disposing) {
            if (TexPtr != IntPtr.Zero) // only dispose once!
            {
                SDL.SDL_DestroyTexture(TexPtr);
            }
        }

        public void Dispose() {
            Dispose(true);
            // tell the GC not to finalize
            GC.SuppressFinalize(this);
        }

        ~Texture() {
            Dispose(false);
        }
    }

    public class Surface : IDisposable {
        public IntPtr SurfPtr { get; private set; }

        // Render some text to a surface
        public Surface(Font font, string text, Color color) {
            IntPtr surface = SDL_ttf.TTF_RenderText_Solid(font.FontPtr, text, color.SDLColor());
            if (surface == IntPtr.Zero) {
                throw new SDLException("TTF_RenderText_Solid");
            }

            SurfPtr = surface;
        }

        protected virtual void Dispose(bool disposing) {
            if (SurfPtr != IntPtr.Zero) // only dispose once!
            {
                SDL.SDL_FreeSurface(SurfPtr);
            }
        }

        public void Dispose() {
            Dispose(true);
            // tell the GC not to finalize
            GC.SuppressFinalize(this);
        }

        ~Surface() {
            Dispose(false);
        }
    }

    public class Font : IDisposable {
        public IntPtr FontPtr { get; private set; }

        public Font(string name, int ptsize) {
            // Construct the full path
            string path = Path.Combine(Graphics.RESOURCE_PATH, name);

            // Load the font
            IntPtr font = SDL_ttf.TTF_OpenFont(path, ptsize);
            if (font == IntPtr.Zero) {
                throw new SDLException("TTF_OpenFont");
            }

            FontPtr = font;
        }

        public Surface TextSurface(string text, Color color) {
            Surface textSurface = new Surface(this, text, color);
            return textSurface;
        }

        protected virtual void Dispose(bool disposing) {
            if (FontPtr != IntPtr.Zero) // only dispose once!
            {
                SDL_ttf.TTF_CloseFont(FontPtr);
            }
        }

        public void Dispose() {
            Dispose(true);
            // tell the GC not to finalize
            GC.SuppressFinalize(this);
        }

        ~Font() {
            Dispose(false);
        }
        
    }

    public class Rect {

        public int X { get; set; }
        public int Y { get; set; }
        public int W { get; set; }
        public int H { get; set; }

        public Rect(int x, int y, int w, int h) {
            X = x;
            Y = y;
            W = w;
            H = h;
        }

        public SDL.SDL_Rect Rct {
            get {
                return new SDL.SDL_Rect { x=X, y=Y, w=W, h=H };
            }
        }
    }

    public class Color {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public byte A { get; set; }

        public Color(byte r, byte g, byte b, byte a) {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public Color(byte r, byte g, byte b) {
            R = r;
            G = g;
            B = b;
            A = 255;
        }

        public Color(long color) {
            R = (byte)((color & 0xFF000000) >> 24);
            G = (byte)((color & 0x00FF0000) >> 16);
            B = (byte)((color & 0x0000FF00) >> 8);
            A = (byte)((color & 0x000000FF) >> 0);
        }

        // Get a SDL_Color struct.
        public SDL.SDL_Color SDLColor() {
            SDL.SDL_Color color;
            color.r = R;
            color.g = G;
            color.b = B;
            color.a = A;

            return color;
        }
    }

    public class SDLException : Exception
    {
        public SDLException(string message)
           : base(message + " error: " + SDL.SDL_GetError())
        {
        }
    }

}