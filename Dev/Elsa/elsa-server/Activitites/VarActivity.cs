using Elsa.Workflows;
using Elsa.Workflows.Contracts;
using Elsa.Workflows.UIHints;
using Elsa.Workflows.UIHints.Dropdown;
using System.Reflection;

public class VarActivity : CodeActivity, IPropertyUIHandler
{
    [Elsa.Workflows.Attributes.Input(Description = "Select a brand", UIHint = InputUIHints.DropDown,
        UIHandler = typeof(VarActivity))]
    public string? Brands { get; set; }

    public ValueTask<IDictionary<string, object>> GetUIPropertiesAsync(PropertyInfo propertyInfo, object? context, CancellationToken cancellationToken = default)
    {
        var brands = new[] { "BMW", "Kia", "Honda", DateTime.Now.ToString("hh-mm-ss-fff") };
        var items = brands.Select(b=> new Elsa.Workflows.UIHints.Dropdown.SelectListItem(b, b)).ToList();

        var props = new DropDownProps() { SelectList = new Elsa.Workflows.UIHints.Dropdown.SelectList(items) };
        var options = new Dictionary<string, object>() { [InputUIHints.DropDown] = props };

        return new ValueTask<IDictionary<string, object>>(options);
    }
}


//using System;
//using System.Linq;
//using System.Reflection;
//using System.Threading;
//using System.Threading.Tasks;
//using Elsa.ActivityResults;
//using Elsa.Attributes;
//using Elsa.Design;
//using Elsa.Expressions;
//using Elsa.Metadata;
//using Elsa.Workflows;
//using Elsa.Workflows.Models;
////using Elsa.Services;

//namespace Elsa.Samples.Server.Host.Activities
//{
//    public class VehicleActivity : Activity, IActivityPropertyOptionsProvider, IRuntimeSelectListProvider
//    {
//        private readonly Random _random;

//        public VehicleActivity()
//        {
//            _random = new Random();
//        }

//        [ActivityInput(
//            UIHint = ActivityInputUIHints.Dropdown,
//            OptionsProvider = typeof(VehicleActivity),
//            DefaultSyntax = SyntaxNames.Literal,
//            SupportedSyntaxes = new[] { SyntaxNames.Literal, SyntaxNames.JavaScript, SyntaxNames.Liquid }
//        )]
//        public Input<string> Brand { get; set; }

//        [ActivityInput(
//            UIHint = ActivityInputUIHints.Dropdown,
//            DefaultSyntax = SyntaxNames.Literal,
//            SupportedSyntaxes = new[] { SyntaxNames.Literal, SyntaxNames.JavaScript, SyntaxNames.Liquid }
//        )]
//        public Input<string> Model { get; set; }

//        /// <summary>
//        /// Return options to be used by the designer. The designer will pass back whatever context is provided here.
//        /// </summary>
//        public object GetOptions(PropertyInfo property) => new RuntimeSelectListProviderSettings(GetType(), new VehicleContext(_random.Next(100)));

//        /// <summary>
//        /// Invoked from an API endpoint that is invoked by the designer every time an activity editor for this activity is opened.
//        /// </summary>
//        /// <param name="context">The context from GetOptions</param>
//        public ValueTask<SelectList> GetSelectListAsync(object? context = default, CancellationToken cancellationToken = default)
//        {
//            var vehicleContext = (VehicleContext)context!;
//            var brands = new[] { "Kia", "Peugeot", "Tesla", vehicleContext.RandomNumber.ToString() };
//            var items = brands.Select(x => new SelectListItem(x)).ToList();
//            return new ValueTask<SelectList>(new SelectList(items));
//        }

//        //protected override IActivityExecutionResult OnExecute()
//        //{
//        //    return Done();
//        //}

//        protected override void Execute(ActivityExecutionContext context)
//        {
//            base.Execute(context);
//        }


//        protected override ValueTask ExecuteAsync(ActivityExecutionContext context)
//        {
//            return base.ExecuteAsync(context);
//        }
//    }

//    public record VehicleContext(int RandomNumber);
//}