using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Studio.Core.Interfaces;

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

        #endregion Class Members

        #region Constructor

        public DesignerManagementService(IResourceRepository resourceRepository)
        {
            if (resourceRepository == null)
            {
                throw new ArgumentNullException("resourceRepository");
            }

            _resourceRepository = resourceRepository;
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
            ModelProperty modelProperty = modelItem.Properties.Where(mp => mp.Name == "ServiceName").FirstOrDefault();

            if (modelProperty != null)
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
    }
}
