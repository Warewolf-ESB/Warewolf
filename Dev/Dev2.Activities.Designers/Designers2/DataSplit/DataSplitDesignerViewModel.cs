using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Dev2.Activities.Designers2.Core;
using Dev2.Providers.Errors;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Core.Activities.Utils;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers2.DataSplit
{
    public class DataSplitDesignerViewModel : ActivityCollectionDesignerViewModel<DataSplitDTO>
    {
        public IList<string> ItemsList { get; private set; }

        public DataSplitDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            AddTitleBarLargeToggle();
            AddTitleBarQuickVariableInputToggle();
            AddTitleBarHelpToggle();

            ItemsList = new List<string> { "Index", "Chars", "New Line", "Space", "Tab", "End" };
            SplitTypeUpdatedCommand = new RelayCommand(OnSplitTypeChanged, o => true);

            dynamic mi = ModelItem;
            InitializeItems(mi.ResultsCollection);

            for(var i = 0; i < mi.ResultsCollection.Count; i++)
            {
                OnSplitTypeChanged(i);
            }
        }

        public override string CollectionName { get { return "ResultsCollection"; } }

        public ICommand SplitTypeUpdatedCommand { get; private set; }

        public bool IsSourceStringFocused { get { return (bool)GetValue(IsSourceStringFocusedProperty); } set { SetValue(IsSourceStringFocusedProperty, value); } }
        public static readonly DependencyProperty IsSourceStringFocusedProperty = DependencyProperty.Register("IsSourceStringFocused", typeof(bool), typeof(DataSplitDesignerViewModel), new PropertyMetadata(default(bool)));

        string SourceString { get { return GetProperty<string>(); } }

        void OnSplitTypeChanged(object indexObj)
        {
            var index = (int)indexObj;
            if(index < 0 || index >= ItemCount)
            {
                return;
            }

            var mi = ModelItemCollection[index];
            var splitType = mi.GetProperty("SplitType") as string;

            if(splitType == "Index" || splitType == "Chars")
            {
                mi.SetProperty("EnableAt", true);
            }
            else
            {
                mi.SetProperty("At", string.Empty);
                mi.SetProperty("EnableAt", false);
            }
        }

        public override void Validate()
        {
            base.Validate();

            var baseErrors = Errors;

            Errors = null;
            var errors = new List<IActionableErrorInfo>();

            System.Action onError = () => IsSourceStringFocused = true;

            const string SourceLabel = "String To Split";
            string sourceValue;
            errors.AddRange(SourceString.TryParseVariables(out sourceValue, onError));
            foreach(var error in errors)
            {
                error.Message = SourceLabel + " - " + error.Message;
            }

            if(string.IsNullOrWhiteSpace(sourceValue))
            {
                errors.Add(new ActionableErrorInfo(onError) { ErrorType = ErrorType.Critical, Message = SourceLabel + " must have a value" });
            }

            if(baseErrors != null)
            {
                errors.AddRange(baseErrors);
            }

            // Always assign property otherwise binding does not update!
            Errors = errors;
        }
    }
}