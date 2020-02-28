using System;
using System.Linq;
using System.Collections.Generic;

using SDL2;

using skaktego.Graphical;

namespace skaktego.UserInterace {

    /// <summary>
    /// The three different states a button can be in
    /// </summary>
    public enum ButtonStates{
        /// <summary>
        /// The buttons is able to be moused over (active)
        /// </summary>
        Mouseable,

        /// <summary>
        /// The user is mousing over the button
        /// </summary>
        Moused, 

        /// <summary>
        /// The user has pressed the left mouse button while hovering over it
        /// </summary>
        Pressed 
    }

    /// <summary>
    /// A UI button that can execute an action when pressed
    /// </summary>
    public class Button {
        public double x, y, w, h;
        public string text;
        public ButtonStates State {get ; private set;}
        private Font font;
        private Action action;
        private Texture textTexture = null;

        /// <summary>
        /// Construct a new button
        /// </summary>
        /// <param name="x">The horizontal offset of the button [0 ; 1]</param>
        /// <param name="y">The vertical offset of the button [0 ; 1]</param>
        /// <param name="w">The width of the button [0 ; 1]</param>
        /// <param name="h">The height of the button [0 ; 1]</param>
        /// <param name="text">The label of the button</param>
        /// <param name="font">The font of the label</param>
        /// <param name="action">The action to execute when pressed and released</param>
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

        /// <summary>
        /// Update the button
        /// </summary>
        /// <param name="mouseX">The x-position of the mouse</param>
        /// <param name="mouseY">The y-position of the mouse</param>
        /// <param name="panelRect">The rectangle the button resides in</param>
        /// <param name="events">A lsit of user-based events</param>
        public bool Update(int mouseX, int mouseY, Rect panelRect, List<SDL.SDL_Event> events){

            bool hasPressed = false;

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
                hasPressed = true;
                State = ButtonStates.Moused;
            }
            return hasPressed;
        }

        /// <summary>
        /// Draw the button
        /// </summary>
        /// <param name="renderer">The screen renderer</param>
        /// <param name="dst">The rectangle to draw the button upon</param>
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
            if (textTexture == null) {
                using (Surface textSurf = font.TextSurface(text, Graphics.black)) {
                    textTexture = new Texture(renderer, textSurf);
                }
            }
            renderer.RenderTexture(textTexture, textRect, null);
        }
    }
}