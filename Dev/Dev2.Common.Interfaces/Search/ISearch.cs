#pragma warning disable
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
using System.ComponentModel;

namespace Dev2.Common.Interfaces.Search
{
    public enum SearchItemType
    {
        [Description("Resource")]
        WorkflowName,
        [Description("Resource")]
        SourceName,
        [Description("Tool Title")]
        ToolTitle,
        [Description("Scalar")]
        Scalar,
        [Description("Scalar Input")]
        ScalarInput,
        [Description("Scalar Output")]
        ScalarOutput,
        [Description("Recordset")]
        RecordSet,
        [Description("Recordset Input")]
        RecordSetInput,
        [Description("Recordset Output")]
        RecordSetOutput,
        [Description("Object")]
        Object,
        [Description("Object Input")]
        ObjectInput,
        [Description("Object Output")]
        ObjectOutput,
        [Description("Test Name")]
        TestName
    }

    public interface ISearcher
    {
        List<ISearchResult> GetSearchResults(ISearch searchParameters);
    }

    public interface ISearchResult
    {
        Guid ResourceId { get; set; }
        string Name { get; set; }
        string Path { get; set; }
        SearchItemType Type { get; set; }
        string Match { get; set; }
    }

    public interface ISearch : INotifyPropertyChanged
    {
        string SearchInput { get; set; }
        ISearchOptions SearchOptions { get; set; }
        List<ISearchResult> GetSearchResults(List<ISearcher> searchers);
    }

    public interface ISearchOptions : INotifyPropertyChanged
    {
        bool IsAllSelected { get; set; }
        bool IsWorkflowNameSelected { get; set; }
        bool IsToolTitleSelected { get; set; }
        bool IsScalarNameSelected { get; set; }
        bool IsObjectNameSelected { get; set; }
        bool IsRecSetNameSelected { get; set; }
        bool IsInputVariableSelected { get; set; }
        bool IsOutputVariableSelected { get; set; }
        bool IsVariableSelected { get; }
        bool IsTestNameSelected { get; set; }
        bool IsMatchCaseSelected { get; set; }
        bool IsMatchWholeWordSelected { get; set; }
        void UpdateAllStates(bool value);
    }
}
