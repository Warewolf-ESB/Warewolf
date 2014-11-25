
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Caliburn.Micro;

// ReSharper disable once CheckNamespace
// ReSharper disable CheckNamespace
namespace Dev2.Studio.Core.ViewModels.Base
// ReSharper restore CheckNamespace
{
    public enum ViewModelDialogResults
    {
        Okay,
        Cancel,
    }

    /// <summary>
    /// Base class for all ViewModel classes in the application.
    /// It provides support for property change notifications 
    /// and has a DisplayName property.  This class is abstract.
    /// </summary>
    public abstract class BaseViewModel : SimpleBaseViewModel
    {
        // ReSharper disable InconsistentNaming
        readonly protected IEventAggregator _eventPublisher;
        // ReSharper restore InconsistentNaming

        #region Constructor

        protected BaseViewModel(IEventAggregator eventPublisher)
        {
            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);
            _eventPublisher = eventPublisher;
            _eventPublisher.Subscribe(this);

            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            // ReSharper disable DoNotCallOverridableMethodsInConstructor
            SatisfyImports();
            // ReSharper restore DoNotCallOverridableMethodsInConstructor
        }

        public IEventAggregator EventPublisher
        {
            get
            {
                return _eventPublisher;
            }
        }

        #endregion // Constructor

        protected override void OnDispose()
        {
            _eventPublisher.Unsubscribe(this);
            base.OnDispose();
            
        }

        #region Protected Virtual Methods

        protected virtual void SatisfyImports()
        {
        }

        #endregion Protected Virtual Methods
    }
}
