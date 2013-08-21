using System.Collections.Generic;
using System.ComponentModel;
using Dev2.Data.Enums;
using Dev2.Interfaces;
using Dev2.Util;

namespace Dev2
{
    public class GatherSystemInformationTO : IDev2TOFn
    {
        #region Fields

        enTypeOfSystemInformationToGather _enTypeOfSystemInformation;
        string _result;

        #endregion

        #region Ctor

        public GatherSystemInformationTO()
        {

        }

        public GatherSystemInformationTO(enTypeOfSystemInformationToGather enTypeOfSystemInformation, string result, int indexNumber,bool inserted = false)
        {
            Inserted = inserted;
            EnTypeOfSystemInformation = enTypeOfSystemInformation;
            Result = result;
            IndexNumber = indexNumber;
        }

        #endregion

        #region Properties

        public bool Inserted { get; set; }
        
        /// <summary>
        /// Type of system information to gather
        /// </summary>       
        public enTypeOfSystemInformationToGather EnTypeOfSystemInformation
        {
            get
            {
                return _enTypeOfSystemInformation;
            }
            set
            {
                _enTypeOfSystemInformation = value;
                OnPropertyChanged("EnTypeOfSystemInformation");
            }
        }


        /// <summary>
        /// Where to place the result, will be the same as From until wizards are created
        /// </summary>
        [FindMissing]
        public string Result
        {
            get
            {
                return _result;
            }
            set
            {
                _result = value;
                OnPropertyChanged("Result");
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
            return string.IsNullOrWhiteSpace(Result);
        }

        public bool CanAdd()
        {
            return !string.IsNullOrWhiteSpace(Result);
        }

        public void ClearRow()
        {
            Result = "";
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