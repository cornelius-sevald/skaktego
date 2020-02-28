using System;

using SDL2;

namespace skaktego.Graphical {
    
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

}