using System.Text;

namespace Dev2
{

    #region Field Enums

    public enum PaddingDirection
    {
        Left,
        Right,
        None
    }

    #endregion

    public class Field
    {

        #region Protected Fields

        protected string delimiter;

        #endregion

        #region Public Properties

        public string Name
        {
            get;
            set;
        }

        public int Length
        {
            get;
            set;
        }

        public string RegularExpressionValidator { get; set; }

        public PaddingDirection Padding
        {
            get;
            set;
        }

        public int PaddingLength
        {
            get;
            set;
        }

        public char PaddingCharacter
        {
            get;
            set;
        }

        public string Delimiter
        {
            get
            {
                return delimiter;
            }
            set
            {
                StringBuilder b = new StringBuilder(value);
                b.Replace(@"\0", "\0");
                b.Replace(@"\a", "\a");
                b.Replace(@"\b", "\b");
                b.Replace(@"\f", "\f");
                b.Replace(@"\n", "\n");
                b.Replace(@"\r", "\r");
                b.Replace(@"\t", "\t");
                b.Replace(@"\v", "\v");
                delimiter = b.ToString();
            }
        }

        #endregion

        #region Public Methods

        public string ParseValue(string value)
        {

            if(Padding == PaddingDirection.Left && PaddingCharacter != '\0')
            {
                value = value.TrimStart(new[] { PaddingCharacter });
            }

            if(Padding == PaddingDirection.Right && PaddingCharacter != '\0')
            {
                value = value.TrimEnd(new[] { PaddingCharacter });
            }

            if(Length > 0 && Length < value.Length)
            {


                return System.Security.SecurityElement.Escape(value.Substring(0, Length));
            }

            return System.Security.SecurityElement.Escape(value);
        }

        public string FormatValue(string value)
        {
            if(PaddingCharacter != '\0' && Length > 0)
            {
                switch(Padding)
                {
                    case PaddingDirection.Left:
                        value = value.PadLeft(Length, PaddingCharacter);
                        break;
                    case PaddingDirection.Right:
                        value = value.PadRight(Length, PaddingCharacter);
                        break;
                }
            }

            return value;
        }

        #endregion
    }
}
