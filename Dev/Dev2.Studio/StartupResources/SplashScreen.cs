using System;
using System.Reflection;
using System.Windows;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.StartupResources
{
    public class Dev2SplashScreen
    {
        public static SplashScreen SplashScreen;

        public static void SetSplashScreen(string location)
        {
            SplashScreen = new SplashScreen(Assembly.GetExecutingAssembly(), location);
        }

        public static void Show()
        {
            if(SplashScreen == null)
            {
                SetSplashScreen(StringResources.SplashImage);
            }
            if(SplashScreen != null)
            {
                SplashScreen.Show(false);
            }
        }

        public static void Close(TimeSpan fadeoutTime)
        {
            SplashScreen.Close(fadeoutTime);
        }


    }
}
