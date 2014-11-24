
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
