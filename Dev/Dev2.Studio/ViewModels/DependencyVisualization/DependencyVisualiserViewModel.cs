/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Interfaces;
using Dev2.Services.Events;
using Dev2.Studio.Controller;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.Studio.Views.DependencyVisualization;
using Microsoft.Practices.Prism.Mvvm;
using Warewolf.Studio.ViewModels;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.ViewModels.DependencyVisualization
// ReSharper restore CheckNamespace
{
    public class DependencyVisualiserViewModel : BaseWorkSurfaceViewModel
    {
        readonly DependencyVisualiserView _view;
        private IContextualResourceModel _resourceModel;
        public string ResourceType { get; set; }
        private double _availableWidth;
        private double _availableHeight;
        ObservableCollection<IExplorerItemNodeViewModel> _allNodes;
        bool _getDependsOnMe;
        bool _getDependsOnOther;
        string _nestingLevel;
        public Guid EnvironmentId { get; set; }
        private readonly IServer _server;
        private readonly IPopupController _popupController;
        private readonly IDependencyGraphGenerator _graphGenerator;

        public DependencyVisualiserViewModel(IDependencyGraphGenerator generator, IEventAggregator aggregator, IServer server)
            : this(aggregator)
        {
            _graphGenerator = generator;
            _server = server;
        }
        public DependencyVisualiserViewModel(IEventAggregator eventAggregator)
            : base(eventAggregator)
        {
            _popupController = new PopupController();
        }

        public DependencyVisualiserViewModel(DependencyVisualiserView view, IServer server, bool getDependsOnMe = false)
            : base(EventPublishers.Aggregator)
        {
            _view = view;
            _server = server;
            GetDependsOnMe = getDependsOnMe;
            GetDependsOnOther = !GetDependsOnMe;
            NestingLevel = "0";
            _popupController = new PopupController();
        }

        public double AvailableWidth
        {
            get
            {
                return _availableWidth;
            }
            set
            {
                if (_availableWidth.CompareTo(value) == 0)
                {
                    return;
                }

                _availableWidth = value;

                NotifyOfPropertyChange(() => AvailableWidth);
            }
        }

        public double AvailableHeight
        {
            get
            {
                return _availableHeight;
            }
            set
            {
                if (_availableHeight.CompareTo(value) == 0)
                {
                    return;
                }
                _availableHeight = value;
                NotifyOfPropertyChange(() => AvailableHeight);
            }
        }

        public IContextualResourceModel ResourceModel
        {
            get
            {
                return _resourceModel;
            }
            set
            {
                if (_resourceModel == value) return;

                _resourceModel = value;
                BuildGraphs();
                NotifyOfPropertyChange(() => ResourceModel);
                if (value != null)
                    NotifyOfPropertyChange(() => DisplayName);
            }
        }

        public bool GetDependsOnMe
        {
            get
            {
                return _getDependsOnMe;
            }
            set
            {
                _getDependsOnMe = value;
                NotifyOfPropertyChange(() => GetDependsOnMe);
                if (ResourceModel != null)
                {
                    BuildGraphs();
                }
            }
        }

        public bool GetDependsOnOther
        {
            get
            {
                return _getDependsOnOther;
            }
            set
            {
                _getDependsOnOther = value;
                NotifyOfPropertyChange(() => GetDependsOnOther);
                if (_getDependsOnOther)
                {
                    NotifyOfPropertyChange(() => GetDependsOnMe);
                }
            }
        }

        public string NestingLevel
        {
            get { return _nestingLevel; }
            set
            {
                _nestingLevel = value;
                NotifyOfPropertyChange(() => NestingLevel);
                if (ResourceModel != null && !string.IsNullOrEmpty(_nestingLevel) && NestingLevel.IsNumeric())
                {
                    BuildGraphs();
                }
            }
        }

        public override string DisplayName => string.Format(GetDependsOnMe ? "Dependency - {0}" : "{0}*Dependencies", ResourceModel.ResourceName);

        // NOTE: This method is invoked from DependencyVisualiser.xaml
        private async void BuildGraphs()
        {
            var repo = ResourceModel.Environment.ResourceRepository;
            var graphData = await repo.GetDependenciesXmlAsync(ResourceModel, GetDependsOnMe);

            if (graphData == null)
            {
                throw new Exception(string.Format(GlobalConstants.NetworkCommunicationErrorTextFormat, "GetDependenciesXml"));
            }

            int nestingLevel = int.Parse(NestingLevel ?? "0");
            var graphGenerator = _graphGenerator ?? new DependencyGraphGenerator();
            var graph = graphGenerator.BuildGraph(graphData.Message, ResourceModel.ResourceName, AvailableWidth, AvailableHeight, nestingLevel);

            if (graph.Nodes.Count > 0)
            {
                var seenResource = new List<Guid>();
                var acc = new List<ExplorerItemNodeViewModel>();
                GetItems(new List<IDependencyVisualizationNode> { graph.Nodes.FirstOrDefault() }, null, acc, seenResource);
                if (acc.Count == 0 || acc.LastOrDefault() == null)
                {
                    AllNodes = new ObservableCollection<IExplorerItemNodeViewModel>();
                }
                else
                {
                    AllNodes = new ObservableCollection<IExplorerItemNodeViewModel>(acc.Last().AsNodeList());
                }
            }
        }


        public string FavoritesLabel => "Show what depends on " + ResourceModel.ResourceName;

        public string DependantsLabel => "Show what " + ResourceModel.ResourceName + " depends on";


        public ObservableCollection<IExplorerItemNodeViewModel> AllNodes
        {
            get
            {
                return _allNodes;
            }
            set
            {
                _allNodes = value;
                NotifyOfPropertyChange(() => AllNodes);
            }
        }

        public ICollection<ExplorerItemNodeViewModel> GetItems(List<IDependencyVisualizationNode> nodes, IExplorerItemNodeViewModel parent, List<ExplorerItemNodeViewModel> acc, List<Guid> seenResource)
        {
            List<ExplorerItemNodeViewModel> items = new List<ExplorerItemNodeViewModel>(nodes.Count);
            foreach (var node in nodes)
            {
                if (!seenResource.Contains(Guid.Parse(node.ID)))
                {
                    var mainViewModel = CustomContainer.Get<IMainViewModel>();
                    var env = mainViewModel?
                        .ExplorerViewModel.Environments.FirstOrDefault(model => model.ResourceId == ResourceModel.Environment.ID);
                    var exploreritem = env?.UnfilteredChildren.Flatten(model => model.UnfilteredChildren).FirstOrDefault(model => model.ResourceId == Guid.Parse(node.ID));
                    if (exploreritem != null)
                    {
                        ExplorerItemNodeViewModel item = new ExplorerItemNodeViewModel(_server, parent, _popupController)
                        {
                            ResourceName = exploreritem.ResourceName,
                            TextVisibility = true,
                            ResourceType = exploreritem.ResourceType,
                            IsMainNode = exploreritem.ResourceName.Equals(ResourceModel.ResourceName),
                            ResourceId = Guid.Parse(node.ID)
                        };
                        if (node.NodeDependencies != null && node.NodeDependencies.Count > 0)
                        {
                            seenResource.Add(Guid.Parse(node.ID));
                            item.Children = new ObservableCollection<IExplorerItemViewModel>(GetItems(node.NodeDependencies, item, acc, seenResource).Select(a => a as IExplorerItemViewModel));
                        }
                        else
                        {
                            seenResource.Add(Guid.Parse(node.ID));
                            item.Children = new ObservableCollection<IExplorerItemViewModel>();
                        }
                        items.Add(item);
                        acc.Add(item);
                    }
                }
            }
            return items;
        }

        public bool TextVisibility { get; set; }
        public override object GetView(object context = null)
        {
            return _view;
        }

        protected override void OnViewLoaded(object view)
        {
            var loadedView = view as IView;
            if (loadedView != null)
            {
                base.OnViewLoaded(loadedView);
            }
        }

        protected override void OnViewAttached(object view, object context)
        {
            base.OnViewAttached(view, this);
        }

        public override bool HasVariables => false;
        public override bool HasDebugOutput => false;
    }
}
