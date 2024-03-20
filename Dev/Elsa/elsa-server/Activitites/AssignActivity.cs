using System.ComponentModel;
using System.Runtime.CompilerServices;
using Elsa.Expressions.Models;
using Elsa.Extensions;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Management.Activities.SetOutput;
using Elsa.Workflows.Memory;
using Elsa.Workflows.Models;
using Elsa.Workflows.UIHints;
using JetBrains.Annotations;

namespace Elsa.Workflows.Activities;

/// <summary>
/// Assign a workflow variable, input or output a value.
/// </summary>
[Activity("WareWolf", "WareWolf", "Assign a value to a workflow variable, input or output.")]
[PublicAPI]
public class AssignVariable : CodeActivity
{
    /// <inheritdoc />
    public AssignVariable([CallerFilePath] string? source = default, [CallerLineNumber] int? line = default) : base(source, line)
    {
    }

    /// <summary>
    /// The variable to assign the value to.
    /// </summary>
    [Input(DisplayName= "Variable or Input or Output", Description = "The name of variable, input or output to assign the value to.", UIHint = "assign-variable-picker")]
    public Input<string> VariableName { get; set; } = default!;

    /// <summary>
    /// The value to assign.
    /// </summary>
    [Input(Description = "The value to assign.")]
    public Input<object?> Value { get; set; } = new(default(object));
}