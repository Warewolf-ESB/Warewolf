using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Studio.Core.Interfaces;
using System;
using System.Activities.Presentation.Model;
using System.Linq;
using Dev2.Studio.Core.Messages;

namespace Dev2.Studio.Core.Activities.Services
{
    public class DesignerManagementService : IDesignerManagementService
    {
        #region Events

        #region ExpandAllRequested

        public event EventHandler ExpandAllRequested;

        protected void OnExpandAllRequested()
        {
            if (ExpandAllRequested != null)
            {
                ExpandAllRequested(this, new EventArgs());
            }
        }

        #endregion ExpandAllRequested

        #region CollapseAllRequested

        public event EventHandler CollapseAllRequested;

        protected void OnCollapseAllRequested()
        {
            if (CollapseAllRequested != null)
            {
                CollapseAllRequested(this, new EventArgs());
            }
        }

        #endregion CollapseAllRequested

        #region RestoreAllRequested

        public event EventHandler RestoreAllRequested;

        protected void OnRestoreAllRequested()
        {
            if (RestoreAllRequested != null)
            {
                RestoreAllRequested(this, new EventArgs());
            }
        }

        #endregion RestoreAllRequested

        #endregion Events

        #region Class Members

        private IResourceRepository _resourceRepository;
        private bool disposed;

        #endregion Class Members

        #region Properties

        public IEventAggregator EventAggregator { get; set; }

        #endregion

        #region Constructor

        public DesignerManagementService(IResourceRepository resourceRepository)
        {
            if (resourceRepository == null)
            {
                throw new ArgumentNullException("resourceRepository");
            }

            _resourceRepository = resourceRepository;


            EventAggregator = ImportService.GetExportValue<IEventAggregator>();
            if (EventAggregator != null)
                EventAggregator.Subscribe(this);
            
        }

        #endregion Constructor

        #region Methods

        public IContextualResourceModel GetResourceModel(ModelItem modelItem)
        {
            if (modelItem == null)
            {
                return null;
            }

            IContextualResourceModel resource = null;
            ModelProperty modelProperty = modelItem.Properties.FirstOrDefault(mp => mp.Name == "ServiceName");

            // Get active enviroment
            Action<IEnvironmentModel> callback = delegate(IEnvironmentModel model)
            {

                var resRepo = model.ResourceRepository;

                //2013.03.13: Ashley Lewis - BUG 8846 - added modelproperty.computedvalue to null check
                if (modelProperty != null && modelProperty.ComputedValue != null && resRepo != null)
                {
                    resource = resRepo.FindSingle(c => c.ResourceName == modelProperty.ComputedValue.ToString()) as IContextualResourceModel;
                }
                      
            };

            EventAggregator.Publish(new GetContextualEnvironmentCallbackMessage(callback));
            
            return resource;
        }

        public IContextualResourceModel GetRootResourceModel(ModelItem modelItem)
        {
            if (modelItem == null)
            {
                return null;
            }

            IContextualResourceModel resource = null;
            ModelProperty modelProperty = modelItem.Root.Properties.Where(mp => mp.Name == "Name").FirstOrDefault();

            if (modelProperty != null && _resourceRepository != null)
            {
                resource = _resourceRepository.FindSingle(c => c.ResourceName == modelProperty.ComputedValue.ToString()) as IContextualResourceModel;
            }

            return resource;
        }

        public void RequestExpandAll()
        {
            OnExpandAllRequested();
        }

        public void RequestCollapseAll()
        {
            OnCollapseAllRequested();
        }

        public void RequestRestoreAll()
        {
            OnRestoreAllRequested();
        }

        #endregion Methods

        #region IDisposable Impl

        // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    _resourceRepository = null;
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.

                // Note disposing has been done.
                disposed = true;
            }
        }


        #endregion

    }
}
