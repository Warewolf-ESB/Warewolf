using Elsa.Samples.AspNet.CustomUIHandler;
using Elsa.Workflows;
using Elsa.Workflows.Activities.Flowchart.Models;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Contracts;
using Elsa.Workflows.Models;
using Elsa.Workflows.UIHints;
using Elsa.Workflows.UIHints.Dropdown;
using System.Reflection;


public class CountryActivity : Activity<string>
{
    [Elsa.Workflows.Attributes.Input(Description = "Select a Country", UIHint = InputUIHints.DropDown,
        UIHandlers = [typeof(CountryUIHandler), typeof(CountryRefreshUIHandler)])]
    public Input<string>? Country { get; set; }

    
    [Elsa.Workflows.Attributes.Input(Description = "Select a State from variable-picker", UIHint = "variable-picker"
        )]
    public Input<string>? State { get; set; }


    [Elsa.Workflows.Attributes.Input(Description = "Select a City from assign-variable-picker", UIHint = "assign-variable-picker"
        )]
    public Input<string>? City { get; set; }


}


