using System;
using System.IO;
using System.Text;

using skaktego.UserInterace;

namespace skaktego {
    class Program {

        static int Main(string[] args) {
            // Initialize
            UI ui = UI.Instance;

            // Run the UI.
            while (!ui.quit) {
                ui.Update();
            }

            // Clean up
            ui.Quit();

            return 0;
        }
    }
}
