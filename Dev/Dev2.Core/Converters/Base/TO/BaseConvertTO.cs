using Dev2.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;

namespace Dev2
{
    public class BaseConvertTO : IDev2TOFn
    {
        #region Fields

        private string _fromType;
        private string _toType;
        private string _fromExpression;
        private string _toExpression;
        private string _indexNumber;

        #endregion

        #region Ctor

        public BaseConvertTO()
        {

        }

        public BaseConvertTO(string fromExpression, string fromType, string toType, string toExpression, int indexNumber)
        {
            ToType = toType;
            FromType = fromType;
            ToExpression = toExpression;
            FromExpression = fromExpression;
            IndexNumber = indexNumber;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Current base type
        /// </summary>
        public string FromType
        {
            get
            {
                return _fromType;
            }
            set
            {
                _fromType = value;
                OnPropertyChanged("FromType");
            }
        }

        /// <summary>
        /// Target base conversion type
        /// </summary>
        public string ToType
        {
            get
            {
                return _toType;
            }
            set
            {
                _toType = value;
                OnPropertyChanged("ToType");
            }
        }

        /// <summary>
        /// The Input to use for the from
        /// </summary>
        public string FromExpression
        {
            get
            {
                return _fromExpression;
            }
            set
            {
                _fromExpression = value;
                OnPropertyChanged("FromExpression");
            }
        }

        /// <summary>
        /// Where to place the result, will be the same as From until wizards are created
        /// </summary>
        public string ToExpression
        {
            get
            {
                return _toExpression;
            }
            set
            {
                _toExpression = value;
                OnPropertyChanged("ToExpression");
            }
        }

        public IList<string> Expressions { get; set; }

        public string WatermarkTextVariable { get; set; }

        public string WatermarkText { get; set; }

        public int IndexNumber { get; set; }

        #endregion

        #region Public Methods

        public bool CanRemove()
        {
            return string.IsNullOrWhiteSpace(FromExpression);
        }

        public bool CanAdd()
        {
            return !string.IsNullOrWhiteSpace(FromExpression);
        }

        public void ClearRow()
        {
            FromType = "";
            ToType = "";
            FromExpression = string.Empty;
            ToExpression = string.Empty;
        }

        #endregion

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
