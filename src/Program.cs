using System;
using System.IO;
using System.Text;

namespace skaktego {
    class Program {

        static int Main(string[] args) {
            UI ui = UI.Instance;
            while (!ui.Quit) {
                ui.Update();
            }

            return 0;
        }
    }
}
