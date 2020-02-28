using System;
using System.IO;

using SDL2;

namespace skaktego.Graphical {

    public class Surface : IDisposable {
        public IntPtr SurfPtr { get; private set; }

        // Load a surface from an image file
        public Surface(string name) {
            string path = Path.Combine(Graphics.RESOURCE_PATH, name);
            IntPtr surface = SDL_image.IMG_Load(path);
            if (surface == IntPtr.Zero) {
                throw new SDLException("IMG_Load");
            }

            SurfPtr = surface;
        }

        // Render some text to a surface
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