using System.Collections.Generic;
using System.ComponentModel;
using Dev2.DataList.Contract;
using Dev2.Interfaces;
using Dev2.Util;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public class XPathDTO : INotifyPropertyChanged, IDev2TOFn
    {
        private string _outputVariable;
        private string _xPath;
        private int _indexNum;
        private List<string> _outList;

        public XPathDTO()
        {

        }

        public XPathDTO(string outputVariable, string xPath, int indexNum, bool include = false,bool inserted = false)
        {
            Inserted = inserted;
            OutputVariable = outputVariable;
            XPath = xPath;
            IndexNumber = indexNum;
            _outList = new List<string>();
        }

        public string WatermarkTextVariable { get; set; }

        public int IndexNumber
        {
            get
            {
                return _indexNum;
            }
            set
            {
                _indexNum = value;
                OnPropertyChanged("IndexNum");
            }
        }

        [FindMissing]
        public string OutputVariable
        {
            get
            {
                return _outputVariable;
            }
            set
            {
                _outputVariable = value;
                OnPropertyChanged("OutputVariable");
            }
        }

        [FindMissing]
        public string XPath
        {
            get
            {
                return _xPath;
            }
            set
            {
                _xPath = value;
                OnPropertyChanged("XPath");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public bool CanRemove()
        {
                if (string.IsNullOrEmpty(OutputVariable) && string.IsNullOrEmpty(XPath))
                {
                    return true;
                }
            return false;
        }

        public bool CanAdd()
        {
            var result = !string.IsNullOrEmpty(OutputVariable);
            return result;
        }

        public void ClearRow()
        {
            OutputVariable = string.Empty;
            XPath = "";
        }

        public bool Inserted { get; set; }
    }
}