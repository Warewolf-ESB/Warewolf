using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Dev2;
using Unlimited.Framework;

namespace Dev2 {
  public class DataFormatter : IFrameworkDataFormatter {

    public static string Parse(string format, string fileContent) {
      Format f = new Format(format);
      return f.DataFileToXml(ref fileContent);
    }

    public static string Format(string format, string xmlData) {
      Format f = new Format(format);
      return f.XmlToDataFile(ref xmlData);
    }

    public static string UnlimitedFormat(object data) {
        var dataArray = data as object[];
        string format=string.Empty;;
        string dataString=string.Empty;
        string toDataFile=string.Empty;
        bool dataFile=false;
        if(dataArray != null){
            if(dataArray[0] != null){
                format = dataArray[0].ToString();

            }
            if(dataArray[1] != null){
                dataString = dataArray[1].ToString();
            }

            if(dataArray[2] != null){
                toDataFile = dataArray[2].ToString();
                bool.TryParse(toDataFile, out dataFile);

                if (!dataFile) {
                    var item = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(dataString);
                    string items = item.GetValue("DataToFormat");
                    

                }
            }

        }

        return UnlimitedFormat(format, dataString, dataFile);
    }

    public static string UnlimitedFormat(string format, string data, bool ToDataFile = false) {

        if (ToDataFile) {
            return "<formatterResult>" + FormatToBase64File(format, data) + "</formatterResult>";

        }
        else {

            return "<formatterResult>" + Parse(format, data) + "</formatterResult>";
        }

    }



    public  string FormatData(string format, string data, bool ToDataFile = false) {

        return UnlimitedFormat(format, data, ToDataFile);

    }

    public static string MapFormats(string fromFormat, string toFormat, string fileContent) {
      return Dev2.Format.MapFormats(fromFormat, toFormat, ref fileContent);
    }

    public static string ParseBase64File(string format, string base64fileContent) {
   

        byte[] fileBytes = Convert.FromBase64String(base64fileContent);
        return Parse(format, Encoding.Default.GetString(fileBytes));
    }

    public static string FormatToBase64File(string format, string xmlData) {
      return Convert.ToBase64String(Encoding.Default.GetBytes(Format(format, xmlData)));
    }

    public static string MapFormatsToBase64File(string fromFormat, string toFormat, string base64fileContent) {
      string fileContent = Encoding.Default.GetString(Convert.FromBase64String(base64fileContent));
      return Convert.ToBase64String(Encoding.Default.GetBytes(MapFormats(fromFormat, toFormat, fileContent)));
    }
  }
}
