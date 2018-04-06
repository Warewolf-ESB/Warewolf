﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;

namespace Dev2.Common.Interfaces.Search
{
    public enum SearchItemType
    {
        WorkflowName,
        SourceName,
        ToolTitle,
        Scalar,
        ScalarInput,
        ScalarOutput,
        RecordSet,
        RecordSetInput,
        RecordSetOutput,
        Object,
        ObjectInput,
        ObjectOutput,
        TestName
    }

    public interface ISearchResult
    {
        Guid ResourceId { get; set; }
        string Name { get; set; }
        string Path { get; set; }
        SearchItemType Type { get; set; }
        string Match { get; set; }
    }

    public interface ISearchValue
    {
        string SearchInput { get; set; }
        ISearchOptions SearchOptions { get; set; }
    }

    public interface ISearchOptions
    {
        bool IsAllSelected { get; set; }
        bool IsWorkflowNameSelected { get; set; }
        bool IsToolTitleSelected { get; set; }
        bool IsScalarNameSelected { get; set; }
        bool IsObjectNameSelected { get; set; }
        bool IsRecSetNameSelected { get; set; }
        bool IsInputVariableSelected { get; set; }
        bool IsOutputVariableSelected { get; set; }
        bool IsTestNameSelected { get; set; }
        bool IsMatchCaseSelected { get; set; }
        bool IsMatchWholeWordSelected { get; set; }
    }
}
