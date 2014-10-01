
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using Dev2.Services.Events;
using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Activities.Services
{
    public class DesignerManagementService : IDesignerManagementService
    {
        #region Events

        #region ExpandAllRequested

        public event EventHandler ExpandAllRequested;

        protected void OnExpandAllRequested()
        {
            if(ExpandAllRequested != null)
            {
                ExpandAllRequested(this, new EventArgs());
            }
        }

        #endregion ExpandAllRequested

        #region CollapseAllRequested

        public event EventHandler CollapseAllRequested;

        protected void OnCollapseAllRequested()
        {
            if(CollapseAllRequested != null)
            {
                CollapseAllRequested(this, new EventArgs());
            }
        }

        #endregion CollapseAllRequested

        #region RestoreAllRequested

        public event EventHandler RestoreAllRequested;

        protected void OnRestoreAllRequested()
        {
            if(RestoreAllRequested != null)
            {
                RestoreAllRequested(this, new EventArgs());
            }
        }

        #endregion RestoreAllRequested

        #endregion Events

        #region Class Members

        readonly IContextualResourceModel _rootModel;
        private bool _disposed;

        #endregion Class Members

        #region Constructor

        public DesignerManagementService(IContextualResourceModel rootModel, IResourceRepository resourceRepository)
        {
            if(rootModel == null)
            {
                throw new ArgumentNullException("rootModel");
            }
            if(resourceRepository == null)
            {
                throw new ArgumentNullException("resourceRepository");
            }
            //VerifyArgument.IsNotNull("rootModel", rootModel);
            //VerifyArgument.IsNotNull("resourceRepository", resourceRepository);

            _rootModel = rootModel;

            EventPublishers.Aggregator.Subscribe(this);
        }

        #endregion Constructor

        #region Methods

        public IContextualResourceModel GetRootResourceModel()
        {
            return _rootModel;
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
            if(!_disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if(disposing)
                {
                    EventPublishers.Aggregator.Unsubscribe(this);
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.

                // Note disposing has been done.
                _disposed = true;
            }
        }


        #endregion

    }
}
