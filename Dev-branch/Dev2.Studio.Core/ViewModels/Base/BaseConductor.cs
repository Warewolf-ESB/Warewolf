using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using Dev2.Composition;

namespace Dev2.Studio.Core.ViewModels.Base
{
    public class BaseConductor<T> : Conductor<T>.Collection.OneActive
        where T : IScreen
    {
        [Import]
        public IEventAggregator EventAggregator { get; set; }
        
        protected BaseConductor()
        {
            SatisfyImports();

            EventAggregator = ImportService.GetExportValue<IEventAggregator>();
            if (EventAggregator != null)
                EventAggregator.Subscribe(this);
        }

        #region Protected Virtual Methods

        protected virtual void SatisfyImports()
        {
            //For testing scenarios - ability to fail silently when everythings not imported
            ImportService.TrySatisfyImports(this);
        }

        #endregion Protected Virtual Methods
    }
}
