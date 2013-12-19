using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Dev2.Studio.Core;
using System.Reflection;

namespace Dev2.Studio.StartupResources {
    public class Dev2SplashScreen {
        public static SplashScreen splashScreen;

        public static void SetSplashScreen(string location) {
            splashScreen = new SplashScreen(Assembly.GetExecutingAssembly(), location);
        }

        public static void Show() {
            if(splashScreen == null) {
                SetSplashScreen(StringResources.SplashImage);
            }
            try {
                splashScreen.Show(false);
            }
            catch(TypeInitializationException ex) {
                throw ex;
            }
        }

        public static void Close(TimeSpan fadeoutTime) {
            splashScreen.Close(fadeoutTime);
        }


    }
}
