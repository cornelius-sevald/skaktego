using System;
using System.IO;

using SDL2;

namespace skaktego.Graphical {

    /// <summary>
    /// Wrapper of a SDL_ttf TrueType font
    /// </summary>
    public class Font : IDisposable {

        /// <summary>
        /// Pointer to the internal TTF_Font
        /// </summary>
        public IntPtr FontPtr { get; private set; }

        /// <summary>
        /// Construct a font from a name and point size
        /// </summary>
        /// <param name="name">The path to the font from the resource folder</param>
        /// <param name="ptsize">The point size of the font</param>
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

        /// <summary>
        /// Render a string to a surface
        /// </summary>
        /// <param name="text">A UTF-8 string</param>
        /// <param name="color">The color of the rendered text</param>
        /// <returns>A surface containing the pixel data</returns>
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

        /// <summary>
        /// Dispose of this object, freeing used resources
        /// </summary>
        public void Dispose() {
            Dispose(true);
            // tell the GC not to finalize
            GC.SuppressFinalize(this);
        }

        // Deconstructor
        ~Font() {
            Dispose(false);
        }
        
    }
    
}