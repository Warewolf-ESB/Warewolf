
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


namespace Dev2.Integration.Tests.Enums
{
    public enum enViewModelType
    {
        // DataList
        DataListItemViewModel = 1,
        DataListViewModel = 2,
        DataListViewModelFactory = 3,
        Popup = 10,
        // Data Mapping
        DataMappingViewModelFactory = 30,
        InputOutputViewModelFactory = 31,
        AutoMappingInputAction = 32,
        AutoMappingOutputAction = 33,
        // Environment
        ConnectViewModel = 40,
        // Navigation
        ApplicationCommandViewModel = 50,
        CategoryViewModel = 51,
        TreeViewItemViewModel = 52,
        // Navigation
        NavigationItemVIewModelBase = 53,
        NavigationViewModelBase = 54,
        NavigationItemViewModel = 55,
        NavigationViewModel = 56,
        // Resource Management
        CreateResourceViewModel = 60,
        ResourceDesignerViewModel = 61,
        // User Interface Builders
        LayoutGridViewModel = 70,
        LayoutObjectViewModel = 71,
        WebResourceViewModel = 72,
        WebsiteEditorViewModel = 73,
        // Workflow
        ConfigureDecisionViewModel = 80,
        WorkflowDesignerViewModel = 81,
        WorkflowInputDataViewModel = 82
    }
}
