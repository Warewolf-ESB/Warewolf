//using System.Activities.Presentation.Model;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.Windows;
//using System.Windows.Data;
//using Dev2.Activities.Designers2.Core.Converters;
//using Dev2.Activities.Designers2.Core.QuickVariableInput;

//namespace Dev2.Activities.Designers2.Core
//{
//    public abstract class ActivityCollectionDesignerDecisionViewModel<T> : ActivityCollectionDesigner<T>
//        where T : ActivityCollectionDesignerViewModel
//    {
//        bool _isToggleCheckedChanged;

//        protected ActivityCollectionDesignerDecisionViewModel(ModelItem modelItem)
//            : base(modelItem)
//        {
//            QuickVariableInputViewModel = new QuickVariableInputViewModel(AddToCollection);

//            BindingOperations.SetBinding(QuickVariableInputViewModel, QuickVariableInputViewModel.IsClosedProperty, new Binding(ShowQuickVariableInputProperty.Name)
//            {
//                Source = this,
//                Mode = BindingMode.TwoWay,
//                Converter = new NegateBooleanConverter()
//            });

//            BindingOperations.SetBinding(this, ErrorsProperty, new Binding(QuickVariableInputViewModel.ErrorsProperty.Name)
//            {
//                Source = QuickVariableInputViewModel,
//                Mode = BindingMode.TwoWay
//            });
//        }

//        public virtual ObservableCollection<T> ModelItemCollection { get; protected set; }

//        public abstract void OnSelectionChanged(T oldItem, T newItem);

//        public abstract void UpdateDisplayName();

//        public abstract string CollectionName { get; }

//        public abstract bool CanRemoveAt(int indexNumber);

//        public abstract bool CanInsertAt(int indexNumber);

//        public abstract void RemoveAt(int indexNumber);

//        public abstract void InsertAt(int indexNumber);

//        protected abstract void AddToCollection(IEnumerable<string> source, bool overWrite);

//        public QuickVariableInputViewModel QuickVariableInputViewModel
//        {
//            get { return (QuickVariableInputViewModel)GetValue(QuickVariableInputViewModelProperty); }
//            set { SetValue(QuickVariableInputViewModelProperty, value); }
//        }

//        public static readonly DependencyProperty QuickVariableInputViewModelProperty =
//            DependencyProperty.Register("QuickVariableInputViewModel", typeof(QuickVariableInputViewModel), typeof(ActivityCollectionDesignerDecisionViewModel<T>), new PropertyMetadata(null));

//        public bool ShowQuickVariableInput
//        {
//            get { return (bool)GetValue(ShowQuickVariableInputProperty); }
//            set { SetValue(ShowQuickVariableInputProperty, value); }
//        }

//        public static readonly DependencyProperty ShowQuickVariableInputProperty =
//            DependencyProperty.Register("ShowQuickVariableInput", typeof(bool), typeof(ActivityCollectionDesignerDecisionViewModel<T>), new PropertyMetadata(false, OnTitleBarToggleChanged));

//        public override bool ShowSmall
//        {
//            get
//            {
//                return base.ShowSmall && !ShowQuickVariableInput;
//            }
//        }

//        public override void Collapse()
//        {
//            ShowQuickVariableInput = false;
//            base.Collapse();
//        }

//        public override void Restore()
//        {
//            if(PreviousView == ShowQuickVariableInputProperty.Name)
//            {
//                ShowLarge = false;
//                ShowQuickVariableInput = true;
//            }
//            else
//            {
//                base.Restore();
//            }
//        }

//        protected override void OnToggleCheckedChanged(string propertyName, bool isChecked)
//        {
//            if(!_isToggleCheckedChanged)
//            {
//                if(propertyName == ShowLargeProperty.Name)
//                {
//                    _isToggleCheckedChanged = true;
//                    ShowQuickVariableInput = false;
//                }
//                else if(propertyName == ShowQuickVariableInputProperty.Name)
//                {
//                    if(PreviousView == ShowLargeProperty.Name)
//                    {
//                        _isToggleCheckedChanged = true;
//                        ShowLarge = false;
//                    }
//                }
//                _isToggleCheckedChanged = false;

//                base.OnToggleCheckedChanged(propertyName, isChecked);
//            }
//        }
//        protected override void OnDispose()
//        {
//            CEventHelper.RemoveAllEventHandlers(ModelItemCollection);
//            CEventHelper.RemoveAllEventHandlers(QuickVariableInputViewModel);
//            CEventHelper.RemoveAllEventHandlers(ModelItem);
//        }
//        protected void AddTitleBarQuickVariableInputToggle()
//        {
//            var toggle = ActivityDesignerToggle.Create("pack://application:,,,/Dev2.Activities.Designers;component/Images/ServiceQuickVariableInput-32.png", "Close Quick Variable Input", "pack://application:,,,/Dev2.Activities.Designers;component/Images/ServiceQuickVariableInput-32.png", "Open Quick Variable Input", "QuickVariableInputToggle", this, ShowQuickVariableInputProperty
//                );
//            TitleBarToggles.AddMode(toggle);
//        }


//    }
//}