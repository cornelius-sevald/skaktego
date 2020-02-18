using System;
using System.Linq;
using System.Collections.Generic;
using SDL2;

namespace skaktego {

    public enum ButtonStates{
        Mouseable, Moused, Pressed 
    }
    public class Button {
        public double x, y, w, h;
        public string text;
        private Font font;
        private Action action;
        public ButtonStates State {get ; private set;}

        public Button(double x, double y, double w, double h,
        string text, Font font, Action action){
            this.x = x;
            this.y = y;
            this.w = w;
            this.h = h;
            this.text = text;
            this.font = font;
            this.action = action;
            State = ButtonStates.Mouseable;
        }

        public void Update(int mouseX, int mouseY, Rect panelRect, List<SDL.SDL_Event> events){

            double _mouseX = (mouseX - panelRect.X) / (double)panelRect.W;
            double _mouseY = (mouseY - panelRect.Y) / (double)panelRect.H;

            bool hovering = false;
            bool mouseUp = (events.Any(e => e.type == SDL.SDL_EventType.SDL_MOUSEBUTTONUP));
            bool mouseDown = (events.Any(e => e.type == SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN));
            if (_mouseX > x && _mouseX < x + w && _mouseY > y && _mouseY < y + h) {
                hovering = true;
            }
            if (!hovering) {
                State = ButtonStates.Mouseable;
            }
            if (State == ButtonStates.Mouseable && hovering) {
                State = ButtonStates.Moused;
            }
            if (State == ButtonStates.Moused && mouseDown) {
                State = ButtonStates.Pressed;
            }
            if (State == ButtonStates.Pressed && mouseUp) {
                action();
                State = ButtonStates.Moused;
            }
        }
        public void Draw(Renderer renderer, Rect dst) {
            Rect buttonRect = new Rect((int)Math.Round(x * dst.W + dst.X),(int)Math.Round(y * dst.H + dst.Y),
            (int)Math.Round(w * dst.W),(int)Math.Round(h * dst.H));
            Rect textRect = new Rect((int)Math.Round(x * dst.W + dst.X + dst.W * 0.0125),(int)Math.Round(y * dst.H + dst.Y + dst.H * 0.003125),
            (int)Math.Round(w * dst.W * 0.9),(int)Math.Round(h * dst.H * 0.9));

            Color buttonColor = new Color(0XAAAAAADD);
            if (State == ButtonStates.Moused) {
                buttonColor = new Color(0X888888DD);
            } else if (State == ButtonStates.Pressed) {
                buttonColor = new Color(0X666666DD);
            }
            renderer.SetColor(buttonColor);
            renderer.FillRect(buttonRect);

            // Draw the text
            Surface textSurf = font.TextSurface(text, Graphics.black);
            Texture textTexture = new Texture(renderer, textSurf);
            renderer.RenderTexture(textTexture, textRect, null);
            textSurf.Dispose();
            textTexture.Dispose();
        }
    }
}