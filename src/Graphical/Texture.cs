using System;
using System.IO;

using SDL2;

namespace skaktego.Graphical {

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

}