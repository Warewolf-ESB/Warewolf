using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace ProductComponetsScrubber
{
    /// <summary>
    /// Used to scrub the Product.Componets.wxs file after adding all new references because 
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {

            var f = @"..\..\..\Package\Product.Components.wxs";

            var data = File.ReadAllText(f);

           
        }
    }
}
