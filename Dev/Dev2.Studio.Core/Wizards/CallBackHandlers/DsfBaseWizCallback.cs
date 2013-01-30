using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.DataList.Contract;
using Dev2.Studio.Core.Wizards.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;


namespace Dev2.Studio.Core.Wizards.CallBackHandlers
{
    public abstract class DsfBaseWizCallback<T> : IActivitySpecificSettingsWizardCallbackHandler<T>
    {
        #region Fields
        private ModelItem _activity;
        private Guid _datalistID;
        private Func<IDataListCompiler> _createCompiler;

        #endregion Fields

        #region Ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="DsfBaseWizCallback" /> class.
        /// </summary>
        public DsfBaseWizCallback()
        {
        }
        #endregion Ctor

        #region Properties

        /// <summary>
        /// Gets or sets the activity.
        /// </summary>
        public ModelItem Activity
        {
            get
            {
                return _activity;
            }
            set
            {
                _activity = value;
            }
        }

        /// <summary>
        /// Gets or sets the wizard datalist ID.
        /// </summary>
        public Guid DatalistID
        {
            get
            {
                return _datalistID;
            }
            set
            {
                _datalistID = value;
            }
        }

        public Func<IDataListCompiler> CreateCompiler
        {
            get
            {
                if (_createCompiler == null) return new Func<IDataListCompiler>(() => null);
                return _createCompiler;
            }
            set
            {
                _createCompiler = value;
            }
        }

        #endregion Properties

        #region Methods

        public void CompleteCallback()
        {
            IDataListCompiler compiler = CreateCompiler();

            if (_activity != null && _datalistID != null && compiler != null)
            {                    
                //Sets the properties from the datalist
                ActivityInputOutputUtils.SetValues<Inputs>(Activity, _datalistID, compiler);
                ActivityInputOutputUtils.SetValues<Outputs>(Activity, _datalistID, compiler);
                //Deletes the data list being kept on the server.
                compiler.DeleteDataListByID(_datalistID);                                  
            }
        }

        public void CancelCallback()
        {
            IDataListCompiler compiler = CreateCompiler();

            if (compiler != null)
            {
                //Deletes the data list being kept on the server.
                compiler.DeleteDataListByID(_datalistID);
            }
        }

        #endregion Methods

        #region Private Methods


        #endregion Private Methods
    }
}
