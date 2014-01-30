using Dev2.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;
using Dev2.Providers.Errors;
using Dev2.Providers.Validation;
using Dev2.Providers.Validation.Rules;
using Dev2.Util;

namespace Dev2
{
    public class BaseConvertTO : IDev2TOFn, IPerformsValidation
    {
        #region Fields

        private string _fromType;
        private string _toType;
        private string _fromExpression;
        private string _toExpression;

        #endregion

        #region Ctor

        public BaseConvertTO()
        {

        }

        public BaseConvertTO(string fromExpression, string fromType, string toType, string toExpression, int indexNumber,bool inserted = false)
        {
            Inserted = inserted;
            ToType = string.IsNullOrEmpty(toType) ? "Base 64" : toType;
            FromType = string.IsNullOrEmpty(fromType) ? "Text" : fromType; ;
            ToExpression = string.IsNullOrEmpty(toExpression) ? string.Empty : toExpression; ;
            FromExpression = fromExpression;
            IndexNumber = indexNumber;
        }

        #endregion

        #region Properties

        public bool Inserted { get; set; }

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
                if (value != null)
                {
                    _fromType = value;
                    OnPropertyChanged("FromType");
                }
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
                if (value != null)
                {
                    _toType = value;
                    OnPropertyChanged("ToType");
                }
            }
        }

        /// <summary>
        /// The Input to use for the from
        /// </summary>
        [FindMissing]
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
                RaiseCanAddRemoveChanged();
            }
        }

        /// <summary>
        /// Where to place the result, will be the same as From until wizards are created
        /// </summary>
        [FindMissing]
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

        #region Implementation of IDataErrorInfo

        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        /// <returns>
        /// The error message for the property. The default is an empty string ("").
        /// </returns>
        /// <param name="columnName">The name of the property whose error message to get. </param>
        public string this[string columnName]
        {
            get
            {
                return "";
            }
        }

        /// <summary>
        /// Gets an error message indicating what is wrong with this object.
        /// </summary>
        /// <returns>
        /// An error message indicating what is wrong with this object. The default is an empty string ("").
        /// </returns>
        public string Error { get; private set; }

        #endregion

        void RaiseCanAddRemoveChanged()
        {
            OnPropertyChanged("CanRemove");
            OnPropertyChanged("CanAdd");
        }
        #region Implementation of IPerformsValidation

        public Dictionary<string, List<IActionableErrorInfo>> Errors { get; set; }
        public bool Validate(string propertyName, RuleSet ruleSet)
        {
            return false;
        }

        public bool Validate(string propertyName)
        {
            return false;
        }

        public void Validate()
        {
        }

        #endregion
    }
}
