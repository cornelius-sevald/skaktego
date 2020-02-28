using System;
using System.IO;

using SDL2;

namespace skaktego.Graphical {

    /// <summary>
    /// Wrapper of an SDL surface
    /// 
    /// <para>A surface holds pixel data</para>
    /// </summary>
    public class Surface : IDisposable {

        /// <summary>
        /// Pointer to the internal SDL_Surface
        /// </summary>
        public IntPtr SurfPtr { get; private set; }

        /// <summary>
        /// Load a surface from an image file
        /// </summary>
        /// <param name="name">The path to the image from the resource folder</param>
        public Surface(string name) {
            string path = Path.Combine(Graphics.RESOURCE_PATH, name);
            IntPtr surface = SDL_image.IMG_Load(path);
            if (surface == IntPtr.Zero) {
                throw new SDLException("IMG_Load");
            }

            SurfPtr = surface;
        }

        /// <summary>
        /// Render some text to a surface
        /// </summary>
        /// <param name="font">The font of the text</param>
        /// <param name="text">A UTF-8 string</param>
        /// <param name="color">The color of the text</param>
        public Surface(Font font, string text, Color color) {
            IntPtr surface = SDL_ttf.TTF_RenderUTF8_Solid(font.FontPtr, text, color.SDLColor());
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

        /// <summary>
        /// Dispose of this object, freeing used resources
        /// </summary>
        public void Dispose() {
            Dispose(true);
            // tell the GC not to finalize
            GC.SuppressFinalize(this);
        }

        ~Surface() {
            Dispose(false);
        }
    }
    
}