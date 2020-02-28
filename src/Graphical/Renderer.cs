using System;

using SDL2;

namespace skaktego.Graphical {

    /// <summary>
    /// Wrapper of an SDL renderer
    /// 
    /// <para>The renderer can render textures
    /// to its associated window</para>
    /// </summary>
    public class Renderer {

        /// <summary>
        /// Pointer to the internal SDL_Renderer
        /// </summary>
        public IntPtr RenPtr { get; private set; }

        /// <summary>
        /// Construct a new renderer with an associated window
        /// </summary>
        /// <param name="window">The window to render onto</param>
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

        /// <summary>
        /// Get the size of the screen
        /// </summary>
        public Rect OutputRect() {
            int w, h;
            SDL.SDL_GetRendererOutputSize(RenPtr, out w, out h);
            return new Rect(0, 0, w, h);
        }

        /// <summary>
        /// Clear the screen with the current color
        /// </summary>
        public void Clear() {
            SDL.SDL_RenderClear(RenPtr);
        }

        /// <summary>
        /// Present the backbuffer to the screen
        /// </summary>
        public void Present() {
            SDL.SDL_RenderPresent(RenPtr);
        }

        /// <summary>
        /// Set the rendering color
        /// </summary>
        public void SetColor(Color color) {
            SDL.SDL_SetRenderDrawColor(RenPtr, color.R, color.G, color.B, color.A);
        }

        /// <summary>
        /// Fill a rectangle with the current color.
        /// </summary>
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

}