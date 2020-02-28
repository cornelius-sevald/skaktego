using System;

using SDL2;

namespace skaktego.Graphical {

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
    
}