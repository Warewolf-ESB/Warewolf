#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Caliburn.Micro;
using Dev2.Activities.Designers2.Core.Help;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.Search;
using Dev2.Studio.ViewModels.WorkSurface;
using Microsoft.Practices.Prism.Mvvm;
using System.Diagnostics.CodeAnalysis;

namespace Dev2.ViewModels
{
    public class SearchModel : BaseWorkSurfaceViewModel, IHelpSource, IStudioTab
    {
        readonly IShellViewModel _shellViewModel;

        [ExcludeFromCodeCoverage]
        public SearchModel(IEventAggregator eventPublisher, ISearchViewModel vm, IView view)
            : this(eventPublisher, vm, view, CustomContainer.Get<IShellViewModel>())
        {
        }
        public SearchModel(IEventAggregator eventPublisher, ISearchViewModel vm, IView view, IShellViewModel shellViewModel)
            : base(eventPublisher)
        {
            _shellViewModel = shellViewModel;
            ViewModel = vm;
            View = view;
            ViewModel.PropertyChanged += (sender, args) =>
            {
                ViewModelUtils.RaiseCanExecuteChanged(_shellViewModel.SaveCommand);

                if (args.PropertyName == "DisplayName")
                {
                    NotifyOfPropertyChange(() => DisplayName);
                }
            };
        }

        public override bool HasVariables => false;
        public override bool HasDebugOutput => false;

        protected override void OnDispose()
        {
            _eventPublisher.Unsubscribe(this);
            base.OnDispose();
            ViewModel.Dispose();
        }

        public override object GetView(object context = null) => View;

        [ExcludeFromCodeCoverage]
        protected override void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, ViewModel);
        }

        public override string DisplayName => ViewModel.DisplayName;
        [ExcludeFromCodeCoverage]
        protected override void OnViewLoaded(object view)
        {
            if (view is IView loadedView)
            {
                loadedView.DataContext = ViewModel;
                base.OnViewLoaded(loadedView);
            }
        }

        public string ResourceType => "Search";

        public string HelpText { get; set; }
        public ISearchViewModel ViewModel { get; set; }
        public IView View { get; set; }

        public bool IsDirty => false;

        [ExcludeFromCodeCoverage]
        public void CloseView()
        {
        }

        public bool DoDeactivate(bool showMessage)
        {
            ViewModel.UpdateHelpDescriptor(string.Empty);
            return true;
        }
    }
}
