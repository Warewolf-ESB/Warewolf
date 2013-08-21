using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.Text.RegularExpressions;

namespace Dev2 {

  public class Format {

    #region Public Properties

    public IEnumerable<Field> Header {
      get;
      set;
    }

    public IEnumerable<Field> Body {
      get;
      set;
    }

    public IEnumerable<Field> Footer {
      get;
      set;
    }

    private string _rowDelimiter = "\n";
    public string RowDelimiter { 
        get{return _rowDelimiter;}
        set{
        _rowDelimiter = value;

        }
    }

    #endregion

    #region Constructors

    public Format(string format) {

      try {
        XElement xFormat = XElement.Parse(format);

        if (xFormat.DescendantsAndSelf("Header").Count() > 0) {

          this.Header = ParseXmlFields(xFormat.DescendantsAndSelf("Header").First());
        }

        if (xFormat.DescendantsAndSelf("Body").Count() > 0) {

          this.Body = ParseXmlFields(xFormat.DescendantsAndSelf("Body").First());
        }
        else {
          throw new ArgumentException("The xml passed contains no Body element.");
        }

        if (xFormat.DescendantsAndSelf("Footer").Count() > 0) {

          this.Footer = ParseXmlFields(xFormat.DescendantsAndSelf("Footer").First());
        }
      }
      catch (XmlException xmlException) {
        throw new ArgumentException("The xml passed is not a valid xml document.", xmlException);
      }
    }

    #endregion

    #region Private Methods

    private IEnumerable<Field> ParseXmlFields(XElement elem) {

        var rowDelimiterAttribute = elem.Attribute("RowDelimiter");

        if (rowDelimiterAttribute != null) {
            this.RowDelimiter = rowDelimiterAttribute.Value;
        }

        

      var fields = from field in elem.DescendantsAndSelf("Field")
                   select field;

      int counter = 1;
      foreach (XElement field in fields) {

        Field thisField = new Field();
        thisField.Name = string.IsNullOrEmpty(field.Value) ? string.Format("column{0}", counter) : field.Value;

        int len = -1;

        if (field.Attribute("ValidationRegex") != null) {
            string regex = string.Empty;

            regex = field.Attribute("ValidationRegex").Value;

            if(!string.IsNullOrEmpty(regex)){

                thisField.RegularExpressionValidator = regex;
            }
        }

        if (field.Attribute("Length") != null) {

          int.TryParse(field.Attribute("Length").Value, out len);
        }
        thisField.Length = len;

        if (field.Attribute("Delimiter") != null) {
          thisField.Delimiter = field.Attribute("Delimiter").Value;
        }

        if (field.Attribute("PaddingCharacter") != null) {
            thisField.PaddingCharacter = field.Attribute("PaddingCharacter").Value[0];
        }
        else {
            thisField.PaddingCharacter = ' ';
        }

        if (field.Attribute("PaddingDirection") != null) {
          switch (field.Attribute("PaddingDirection").Value) {
            case "Left":
              thisField.Padding = PaddingDirection.Left;
              break;
            case "Right":
              thisField.Padding = PaddingDirection.Right;
              break;
            case "None":
              thisField.Padding = PaddingDirection.None;
              break;
            default:
              thisField.Padding = PaddingDirection.None;
              break;
          }
        }
        counter++;
        yield return thisField;
      }
    }

    #endregion

    #region Public Methods

    public string DataFileToXml(ref string fileContent) {

      XElement dataTable = XElement.Parse("<DataTable/>");

      int startToken = 0;
      int endToken = 0;
      int rowNumber = 1;
      int tokenLength = 0;
      int contentLength = fileContent.Length;

      bool endOfFile = false;
      string token = null;

      #region Process Header
      if (Header != null) {
        if (Header.Count() > 0) {

          XElement headerRow = XElement.Parse("<HeaderRow/>");

          foreach (Field field in Header) {

            endToken = ((field.Delimiter != null) ? fileContent.IndexOf(field.Delimiter, startToken) : startToken + field.Length);
            tokenLength = endToken - startToken;
            endOfFile = endToken < 0 || contentLength < endToken;

            if (endOfFile) {
              break;
            }

            token = fileContent.Substring(startToken, tokenLength);

            XElement column = new XElement(field.Name, field.ParseValue(token));
            headerRow.Add(column);

            startToken = endToken + ((field.Delimiter != null) ? field.Delimiter.Length : 0);
          }

          dataTable.Add(headerRow);
        }
      }
      #endregion

      #region Process Footer

      XElement footerRow = null;

      if (Footer != null) {
        int endFooterToken = 0;
        int startFooterToken = fileContent.Length;

        List<Field> fields = Footer.Reverse().ToList();
        Field lastBodyField = Body.Last();
        List<XElement> footerRows = new List<XElement>();

        for (int i = 0; i < fields.Count; i++) {

          Field field = fields[i];

          endFooterToken = field.Delimiter != null
            ? fileContent.LastIndexOf(field.Delimiter, startFooterToken)
            : startFooterToken;

          if (i + 1 < fields.Count) {
            startFooterToken = field.Delimiter != null
              ? fileContent.LastIndexOf(field.Delimiter, endFooterToken) + field.Delimiter.Length
              : endFooterToken - field.Length;
          }
          else {
            startFooterToken = lastBodyField.Delimiter != null
              ? fileContent.LastIndexOf(lastBodyField.Delimiter, endFooterToken) + lastBodyField.Delimiter.Length
              : endFooterToken - lastBodyField.Length;
          }

          token = fileContent.Substring(startFooterToken, endFooterToken - startFooterToken);
          footerRows.Add(new XElement(field.Name, field.ParseValue(token)));
        }

        footerRow = XElement.Parse("<FooterRow/>");
        IEnumerable<XElement> footer = ((IEnumerable<XElement>)footerRows).Reverse();

        footerRow.Add(footer);
        contentLength = startFooterToken;
      }
      #endregion

      #region Process Body

      do {
        XElement dataRow = XElement.Parse("<DataRow/>");

        if (Body.Count() == 0)
          break;



            foreach (Field field in Body) {

                endToken = ((field.Delimiter != null) ? fileContent.IndexOf(field.Delimiter, startToken) : startToken + field.Length);
                tokenLength = endToken - startToken;
                endOfFile = endToken < 0 || contentLength < endToken;

                if (endOfFile) {
                    break;
                }

                token = fileContent.Substring(startToken, tokenLength);

                if (!string.IsNullOrEmpty(field.RegularExpressionValidator)) {
                    Regex regex = new Regex(field.RegularExpressionValidator, RegexOptions.Singleline);
                    var match = regex.Match(token);
                    if (!match.Success) {
                        XElement error = new XElement("Error", string.Format("'{0}' does not match regular expression '{1}' specified", field.Name, field.RegularExpressionValidator));
                        dataRow.Add(error);
                        //break;
                    }
                    else {
                        XElement column = new XElement(field.Name, field.ParseValue(token));
                        dataRow.Add(column);
                    }
                }
                else {

                    XElement column = new XElement(field.Name, field.ParseValue(token));
                    dataRow.Add(column);
                }

                startToken = endToken + ((field.Delimiter != null) ? field.Delimiter.Length : 0);
            }


            if (dataRow.HasElements) {

                dataTable.Add(dataRow);
            }
        rowNumber++;
      }
      while (!endOfFile);

      #endregion

      dataTable.Add(footerRow);
      return dataTable.ToString();
    }

    public string XmlToDataFile(ref string xmlData) {

      XElement xData = XElement.Parse(xmlData);
      StringBuilder buffer = new StringBuilder();

      #region Write Header

      if (Header != null) {
        if (xData.DescendantsAndSelf("HeaderRow").Count() > 0 && Header.Count() > 0) {
          XElement header = xData.DescendantsAndSelf("HeaderRow").First();
          foreach (Field headerField in Header) {
            if (header.DescendantsAndSelf(headerField.Name).Count() > 0) {
              string fieldValue = header.DescendantsAndSelf(headerField.Name).First().Value;
              buffer.Append(headerField.FormatValue(fieldValue));
              buffer.Append(headerField.Delimiter);
            }
          }
        }
      }

      #endregion

      #region Write Body

      IEnumerable<XElement> body = xData.DescendantsAndSelf("DataRow");
      foreach (XElement bodyData in body) {
        foreach (Field bodyField in Body) {
          if (bodyData.DescendantsAndSelf(bodyField.Name).Count() > 0) {
            string fieldValue = bodyData.DescendantsAndSelf(bodyField.Name).First().Value;
            buffer.Append(bodyField.FormatValue(fieldValue));
            buffer.Append(bodyField.Delimiter);
          }
        }
      }

      #endregion

      #region Footer

      if (Footer != null) {
        if (xData.DescendantsAndSelf("FooterRow").Count() > 0 && Footer.Count() > 0) {
          XElement footer = xData.DescendantsAndSelf("FooterRow").First();
          foreach (Field footerField in Footer) {
            if (footer.DescendantsAndSelf(footerField.Name).Count() > 0) {
              string fieldValue = footer.DescendantsAndSelf(footerField.Name).First().Value;
              buffer.Append(footerField.FormatValue(fieldValue));
              buffer.Append(footerField.Delimiter);
            }
          }
        }
      }

      #endregion

      return buffer.ToString();
    }

    #endregion

    #region Static Methods

    public static string MapFormats(string fromFormat, string toFormat, ref string fileContent) {
      Format from = new Format(fromFormat);
      Format to = new Format(toFormat);
      string xmlData = from.DataFileToXml(ref fileContent);
      return to.XmlToDataFile(ref xmlData);
    }

    #endregion
  }
}
