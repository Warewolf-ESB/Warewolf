/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Dev2.Common;
using Dev2.Common.Common;
using Dev2.DynamicServices.Objects;
using System;
using System.Activities;
using System.Activities.XamlIntegration;
using System.Text;
using System.Xaml;

namespace Warewolf.Execution.Lightweight.Mcp;

/// <summary>
/// Loads a workflow's stored XAML into an <see cref="ActivityBuilder"/> — the shape
/// <c>WorkflowToX6Converter.ConvertToX6Json</c> requires — for <c>get_workflow_definition</c>.
///
/// <para>
/// <b>Why this exists instead of reusing <c>Dev2.Runtime.ESB.WF.XamlActivityHelper</c>
/// .GetXamlActivityBuilderAsDataActivities</b>: that helper lives in <c>Dev2.Runtime</c>, a
/// dependency <c>Warewolf.Execution.Lightweight</c> has deliberately avoided (log4net,
/// ServiceStack, System.Data.SQLite, System.ServiceModel.Federation, the F# language-parser
/// project, etc. — a footprint far beyond what a single XAML→<see cref="ActivityBuilder"/>
/// load needs). This mirrors its essential technique
/// (<see cref="ActivityXamlServices.CreateBuilderReader"/> over a <see cref="XamlXmlReader"/>,
/// then <see cref="XamlServices.Load(System.Xaml.XamlReader)"/>) and its namespace-cleaning
/// steps (<see cref="Dev2XamlCleaner.CleanServiceDef"/>, <see cref="Dev2XamlLoader.RemoveWindowsElements"/>) —
/// the same steps <see cref="WorkflowExecutor.LoadDynamicActivity"/> already applies
/// successfully in this same process via a plain <see cref="ActivityXamlServices.Load(System.IO.Stream)"/>.
/// </para>
///
/// <para>
/// Deliberately omits <c>XamlActivityHelper</c>'s custom <c>Dev2XamlSchemaContext</c>: that
/// type exists to resolve type-identity conflicts when the same activity assembly is loaded
/// into multiple contexts (a legacy multi-AppDomain concern). This is a single, non-AppDomain
/// .NET 8 isolated-worker process with one copy of <c>Dev2.Activities</c> loaded — the same
/// assumption <see cref="WorkflowExecutor.LoadDynamicActivity"/> already relies on without a
/// custom schema context. If this assumption ever proves wrong (XAML that loads via
/// <see cref="WorkflowExecutor.LoadDynamicActivity"/> but fails via <see cref="Load"/>), that
/// is the first place to look.
/// </para>
/// </summary>
internal static class XamlActivityBuilderLoader
{
    /// <summary>
    /// Compiles <paramref name="xamlDefinition"/> into an <see cref="ActivityBuilder"/>.
    /// Returns <c>null</c> when the XAML has no root <c>ActivityBuilder</c> (e.g. it directly
    /// declares an <see cref="Activity"/> without one) rather than throwing, so callers can
    /// report a structured <c>nonEditableReason</c> instead of an unhandled exception.
    /// </summary>
    internal static ActivityBuilder? Load(StringBuilder xamlDefinition)
    {
        if (xamlDefinition is null || xamlDefinition.Length == 0)
        {
            return null;
        }

        if (GlobalConstants.RuntimeNamespaceClean)
        {
            xamlDefinition = new Dev2XamlCleaner().CleanServiceDef(xamlDefinition);
        }

        Dev2XamlLoader.RemoveWindowsElements(ref xamlDefinition);

        using var xamlStream = xamlDefinition.EncodeForXmlDocument();
        using var xamlReader = new XamlXmlReader(xamlStream);
        using var builderReader = ActivityXamlServices.CreateBuilderReader(xamlReader);
        return XamlServices.Load(builderReader) as ActivityBuilder;
    }
}
