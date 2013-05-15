//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Windows;
//using System.Windows.Input;
//using System.Windows.Threading;
//using Dev2.Studio.Core.Interfaces;
//using Dev2.Studio.Core.ViewModels;
//using ICSharpCode.AvalonEdit.CodeCompletion;
//using ICSharpCode.AvalonEdit.Document;
//using ICSharpCode.AvalonEdit.Folding;
//using ICSharpCode.AvalonEdit.Indentation;
//using Unlimited.Applications.BusinessDesignStudio;
//using Dev2.Studio.Core.Factories;
//using ICSharpCode.AvalonEdit;
//using Dev2.Studio.Core.Interfaces.DataList;

//namespace Dev2.Studio.AppResources.AttachedProperties {
//    public static class AvalonEditAutocompleteBehaviour {
//        private static TextEditor _textEditor;

//        public static readonly DependencyProperty IsDev2AutoCompleteEnabled =
//            DependencyProperty.RegisterAttached("AutoComplete", typeof (bool), typeof (AvalonEditAutocompleteBehaviour), new PropertyMetadata(default(bool), PropertyChangedCallback));

//        public static void SetAutomComplete(UIElement element, bool value) {
//            element.SetValue(IsDev2AutoCompleteEnabled, value);
//        }

//        public static bool GetAutomComplete(UIElement element) {
//            return (bool)element.GetValue(IsDev2AutoCompleteEnabled);
//        }

//        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs) {
            
//        }

//        static CompletionWindow completionWindow;
//        static void SearchDoubleBracket(TextDocument document, int offset) {

//            if (offset > 0) {

//                for (int i = offset; i > 0; i--) {
//                    char ch = document.GetCharAt(i - 1);
//                    if (ch == ']')
//                        return;
//                    if (ch == '[') {
//                        if ((i - 1) > 0) {
//                            char nextChar = document.GetCharAt(i - 2);
//                            if (nextChar == ch) {
//                                List<IDataListItemModel> searchResults = new List<IDataListItemModel>();

//                                var searchPattern = document.GetText(i, offset - i);
//                                if (searchPattern.Contains(".")) {
//                                    string[] searchCriteria = searchPattern.Split('.');
//                                    if (searchCriteria.Length == 3) {
//                                        List<IDataListItemModel> items = new List<IDataListItemModel>();



//                                        //items.Add(_dataListItemViewModelFactory.CreateDataListModel("All()", String.Empty));
//                                        //items.Add(_dataListItemViewModelFactory.CreateDataListModel("First()", String.Empty));
//                                        //items.Add(_dataListItemViewModelFactory.CreateDataListModel("Last()", String.Empty));
//                                        //items.Add(_dataListItemViewModelFactory.CreateDataListModel("Row()", String.Empty));

//                                        searchResults = items;
//                                    }
//                                }
//                                else {
//                                    //if (MainViewModel.ActiveDataList != null) {
//                                    //    searchResults = MainViewModel.ActiveDataList.GetFilteredList(searchPattern).ToList();
//                                    //}
//                                }

//                                if (searchResults.Any()) {
//                                    // open code completion after the user has pressed dot:
//                                    completionWindow = new CompletionWindow(_textEditor.TextArea);
//                                    // provide AvalonEdit with the data:.
//                                    IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;

//                                    foreach (var result in searchResults) {
//                                        data.Add(new TextEditorCompletionData(result.DisplayName, String.Empty));
//                                    }
//                                    completionWindow.Show();

//                                    completionWindow.Closed += delegate {
//                                        completionWindow = null;

//                                    };
//                                }
//                            }
//                        }
//                    }
//                }
//            }
//        }




//        static void SetupTextEditor() {

//            //foldingStrategy = new XmlFoldingStrategy();
//            //foldingManager = FoldingManager.Install(txtResourceDef.TextArea);
//            //foldingStrategy.UpdateFoldings(foldingManager, txtResourceDef.Document);
//            //txtResourceDef.TextArea.IndentationStrategy = new DefaultIndentationStrategy();

//            //DispatcherTimer foldingUpdateTimer = new DispatcherTimer();
//            //foldingUpdateTimer.Interval = TimeSpan.FromSeconds(2);
//            //foldingUpdateTimer.Tick += (sender, e) => {
//            //    if (foldingStrategy != null && foldingManager != null) {
//            //        foldingStrategy.UpdateFoldings(foldingManager, txtResourceDef.Document);

//            //    }

//            //};
//            //foldingUpdateTimer.Start();

//            ////Auto Completion wiring
//            //TextCompositionEventHandler textEntered = (sender, e) => {
//            //    int cursor = txtResourceDef.CaretOffset;
//            //    var currentLine = txtResourceDef.Document.GetLineByOffset(cursor);
//            //    string lineText = txtResourceDef.Document.GetText(currentLine.Offset, cursor - currentLine.Offset);

//            //    SearchDoubleBracket(txtResourceDef.Document, cursor);
//            //};

//            //TextCompositionEventHandler textEntering = (sender, e) => {
//            //    if (e.Text.Length > 0 && completionWindow != null) {
//            //        if (!Char.IsLetterOrDigit(e.Text[0])) {
//            //            // Whenever a non-letter is typed while the completion window is open,
//            //            // insert the currently selected element.
//            //            completionWindow.CompletionList.RequestInsertion(e);


//            //        }
//            //    }

//            //};
//            //txtResourceDef.TextArea.TextEntered += textEntered;
//            //txtResourceDef.TextArea.TextEntering += textEntering;
//        }

//    }
//}
