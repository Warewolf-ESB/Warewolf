using Dev2.Data.Interfaces;

namespace Dev2.DataList.Contract
{
    public class SystemTag : ISystemTag {

        #region Attributes
        private readonly string _startTag;
        private readonly string _endTag;
        #endregion

        #region Ctor
        internal SystemTag(string tagName) {
            _startTag = string.Concat("<", tagName, ">");
            _endTag = string.Concat("</", tagName, ">");
        }
        #endregion

        #region Properties
        public string StartTag {
            get {
                return _startTag;
            }
        }

        public string EndTag{
            get{
                return _endTag;
            }
        }
        #endregion
    }
}
