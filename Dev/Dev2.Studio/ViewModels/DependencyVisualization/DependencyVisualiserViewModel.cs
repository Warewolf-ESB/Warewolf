
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
using System.Collections.ObjectModel;
using Caliburn.Micro;
using Dev2.AppResources.DependencyVisualization;
using Dev2.Common;
using Dev2.Services.Events;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels.WorkSurface;
using Dev2.ViewModels.DependencyVisualization;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.ViewModels.DependencyVisualization
// ReSharper restore CheckNamespace
{
    public class DependencyVisualiserViewModel : BaseWorkSurfaceViewModel
    {
        private IContextualResourceModel _resourceModel;
        private ObservableCollection<Graph> _graphs;

        public DependencyVisualiserViewModel(IEventAggregator eventAggregator)
            : base(eventAggregator)
        {
        } 
        
        public DependencyVisualiserViewModel()
            : base(EventPublishers.Aggregator)
        {
        }

        public ObservableCollection<Graph> Graphs
        {
            get { return _graphs ?? (_graphs = new ObservableCollection<Graph>()); }
        }

        private double _availableWidth;
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
                BuildGraphs();
                NotifyOfPropertyChange(() => AvailableWidth);
            }
        }

        private double _availableHeight;
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
                BuildGraphs();
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

        public bool GetDependsOnMe { get; set; }

        public override string DisplayName
        {
            get
            {
                return string.Format(GetDependsOnMe ? "{0}*Dependants" 
                    : "{0}*Dependencies", ResourceModel.ResourceName);
            }
        }

        // NOTE: This method is invoked from DependencyVisualiser.xaml
        public void BuildGraphs()
        {
            Graphs.Clear();

            var repo = ResourceModel.Environment.ResourceRepository;
            var graphData = repo.GetDependenciesXml(ResourceModel, GetDependsOnMe);

            if(graphData == null)
            {
                throw new Exception(string.Format(GlobalConstants.NetworkCommunicationErrorTextFormat, "GetDependenciesXml"));
            }

            var graphGenerator = new DependencyGraphGenerator();

            var graph = graphGenerator.BuildGraph(graphData.Message, ResourceModel.ResourceName, AvailableWidth, AvailableHeight);

            Graphs.Add(graph);

        }

        
    }
}
