﻿using System;
using System.IO;
using System.Text;

using skaktego.UserInterace;

namespace skaktego {
    
    /// <summary>
    /// The main program of skaktego
    /// 
    /// <para>Simply initializes the UI,
    /// and then runs the UI until the user quits,
    /// and finally cleans up</para>
    /// </summary>
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
