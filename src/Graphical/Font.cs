using System;
using System.IO;

using SDL2;

namespace skaktego.Graphical {

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
    
}