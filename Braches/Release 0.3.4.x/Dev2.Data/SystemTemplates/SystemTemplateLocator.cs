using System.IO;
using System.Reflection;

namespace Dev2.Data.SystemTemplates
{
    //public static class SystemTemplateLocator
    //{
    //    // ReSharper disable InconsistentNaming
    //    private const string _asmNamespace = "Dev2.Data.SystemTemplates.XML";
    //    // ReSharper restore InconsistentNaming

    //    public static ISystemTemplate FetchSystemTemplate(enSystemTemplate template)
    //    {
    //        ISystemTemplate result = null;
    //        string rName = string.Join(".", new[] {_asmNamespace, template.ToString(), "xml"});
    //        //string rName = "Dev2.Data.SystemTemplates.XML." + template.ToString() + ".xml";
    //        Assembly asm = Assembly.GetExecutingAssembly();
    //        Stream  s = asm.GetManifestResourceStream(rName);

    //        if (s != null)
    //        {
    //            using (StreamReader sr = new StreamReader(s))
    //            {
    //                string fileData = sr.ReadToEnd();
    //                result = new SystemTemplate(fileData);
    //            }

    //            s.Close();
    //            s.Dispose();
    //        }


    //        return result;
    //    }
    //}
}
