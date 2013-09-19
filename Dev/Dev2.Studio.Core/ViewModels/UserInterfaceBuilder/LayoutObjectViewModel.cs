using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using Caliburn.Micro;
using Dev2;
using Dev2.Composition;
using Dev2.Providers.Logs;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Actions;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels.Base;

namespace Unlimited.Framework
{

    public class LayoutObjectViewModel : SimpleBaseViewModel, ILayoutObjectViewModel
    {

        #region Locals

        private RelayCommand _deleteCommand;
        private RelayCommand _addRowAboveCommand;
        private RelayCommand _addRowBelowCommand;
        private RelayCommand _addColumnRightCommand;
        private RelayCommand _addColumnLeftCommand;
        private RelayCommand _deleteRowCommand;
        private RelayCommand _deleteColumnCommand;
        private RelayCommand _deleteCellCommand;
        private RelayCommand _editCommand;
        private RelayCommand _clearAllCommand;

        private ILayoutGridViewModel _layoutViewModel;

        private int _gridColumn = 0;
        private int _gridRow = 0;
        private int _columnSpan = 1;
        private int _rowSpan = 1;
        private double _leftBorderThickness = 1;
        private double _topBorderThickness = 1;
        private double _rightBorderThickness = 1;
        private double _bottomBorderThickness = 1;
        private bool _isSelected = false;
        private string _webpartServiceName = string.Empty;
        private string _iconPath = string.Empty;
        private string _xmlConfig = string.Empty;
        private string _displayName = string.Empty;
        private string _webpartServiceDisplayName = string.Empty;
        readonly IEventAggregator _eventPublisher;

        #endregion

        #region Events

        public string FetchData(string args)
        {
            return null;
        }

        public string GetIntellisenseResults(string searchTerm, int caretPosition)
        {
            return null;
        }

        public event NavigateRequestedEventHandler NavigateRequested;

        public delegate void LayoutObjectChangedEventHandler(LayoutObjectViewModel beforeChange, LayoutObjectViewModel afterChange);
        public event LayoutObjectChangedEventHandler LayoutObjectChanged;

        public void OnLayoutObjectChanged(LayoutObjectViewModel afterchange)
        {
            LayoutObjectChangedEventHandler handler = LayoutObjectChanged;
            if(handler != null)
            {
                handler(this, afterchange);
            }
        }

        #endregion Events

        #region Constructor

        public LayoutObjectViewModel()
            : this(EventPublishers.Aggregator)
        {
        }

        public LayoutObjectViewModel(IEventAggregator eventPublisher)
        {
            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);
            _eventPublisher = eventPublisher;
            WebCommunication = ImportService.GetExportValue<IWebCommunication>();
        }

        #endregion


        #region Properties

        public Window Owner { get; set; }

        public IWebCommunication WebCommunication { get; set; }

        /// <summary>
        /// The Grid that this cell is bound to
        /// </summary>
        public ILayoutGridViewModel LayoutObjectGrid
        {
            get
            {
                return _layoutViewModel;
            }
        }

        /// <summary>
        /// The Name of this cell
        /// </summary>
        public string Name
        {
            get
            {
                return string.Format("Row {0} Col {1} RowSpan {2} ColSpan {3}", GridRow, GridColumn, GridRowSpan, GridColumnSpan);
            }
        }

        /// <summary>
        /// The xml configuration data for this cell. This data is sent to the webpart at runtime for binding purposes.
        /// </summary>
        public string XmlConfiguration
        {
            get
            {
                return _xmlConfig;
            }
            set
            {
                _xmlConfig = value;

                var displayNameNodeMatch =
                    UnlimitedObject.GetStringXmlDataAsUnlimitedObject(value).xmlData as XElement;

                //Attempt to retrieve this layout object (cell) display name from the xml configuration
                //only if we are dealing with a webpage. 
                //If we are working with a website then _layoutViewModel will be null
                if(_layoutViewModel != null)
                {
                    string displayNameNodeName = GetValueByElementName("Dev2elementName", value);
                    DisplayName = string.Format("[[{0}]]", displayNameNodeName);
                    if(DisplayName == "[[]]")
                    {
                        DisplayName = string.Empty;
                    }

                    string displayText = GetValueByElementName("displayText", value);
                    if(!string.IsNullOrEmpty(displayText))
                    {
                        WebpartServiceDisplayName = displayText;
                    }
                    else
                    {
                        WebpartServiceDisplayName = WebpartServiceName;
                    }
                }
                base.OnPropertyChanged("XmlConfiguration");
                base.OnPropertyChanged("DisplayName");



            }
        }

        public string WebpartServiceDisplayName
        {
            get
            {
                return _webpartServiceDisplayName;
            }
            set
            {
                _webpartServiceDisplayName = value;
                base.OnPropertyChanged("WebpartServiceDisplayName");
            }
        }

        public string WebpartServiceName
        {
            get
            {
                return _webpartServiceName;
            }
            set
            {
                if(!_webpartServiceName.Equals(value))
                {
                    _webpartServiceName = value;
                    base.OnPropertyChanged("WebpartServiceName");
                }
            }
        }

        public string IconPath
        {
            get
            {
                return _iconPath;
            }
            set
            {
                if(!_iconPath.Equals(value))
                {
                    _iconPath = value;
                    base.OnPropertyChanged("IconPath");
                    base.OnPropertyChanged("IconPathObject");
                }
            }
        }

        public LayoutObjectViewModel IconPathObject
        {
            get
            {
                return this;
            }
        }

        public int GridColumn
        {
            get
            {
                return _gridColumn;
            }
            set
            {

                _gridColumn = value;
                base.OnPropertyChanged("GridColumn");
                base.OnPropertyChanged("Name");

            }
        }

        public int GridRow
        {
            get
            {
                return _gridRow;
            }
            set
            {

                _gridRow = value;
                base.OnPropertyChanged("GridRow");
                base.OnPropertyChanged("Name");
            }
        }

        public int GridColumnSpan
        {
            get
            {
                return _columnSpan;
            }
            set
            {
                if(_columnSpan != value)
                {
                    _columnSpan = value;
                    base.OnPropertyChanged("GridColumnSpan");
                    base.OnPropertyChanged("Name");
                }
            }
        }

        public int GridRowSpan
        {
            get
            {
                return _rowSpan;
            }
            set
            {
                if(_rowSpan != value)
                {
                    _rowSpan = value;
                    base.OnPropertyChanged("GridRowSpan");
                    base.OnPropertyChanged("Name");
                }
            }
        }

        public double LeftBorderThickness
        {
            get
            {
                return _leftBorderThickness;
            }
            set
            {
                _leftBorderThickness = value;
                base.OnPropertyChanged("LeftBorderThickness");
            }
        }

        public double TopBorderThickness
        {
            get
            {
                return _topBorderThickness;
            }
            set
            {
                _topBorderThickness = value;
                base.OnPropertyChanged("TopBorderThickness");
            }
        }

        public double RightBorderThickness
        {
            get
            {
                return _rightBorderThickness;
            }
            set
            {
                _rightBorderThickness = value;
                base.OnPropertyChanged("RightBorderThickness");
            }
        }

        public double BottomBorderThickness
        {
            get
            {
                return _bottomBorderThickness;
            }
            set
            {
                _bottomBorderThickness = value;
                base.OnPropertyChanged("BottomBorderThickness");
            }
        }

        public bool HasRowBelow
        {
            get
            {
                return _layoutViewModel.LayoutObjects.Count(rows => rows.GridRow > this.GridRow) > 0;
            }
        }

        public bool HasRowAbove
        {
            get
            {
                return _layoutViewModel.LayoutObjects.Count(rows => rows.GridRow < this.GridRow) > 0;
            }
        }

        public bool HasColumnLeft
        {
            get
            {
                return _layoutViewModel.LayoutObjects.Count(cols => cols.GridColumn < this.GridColumn) > 0;
            }
        }

        public bool HasColumnRight
        {
            get
            {
                return _layoutViewModel.LayoutObjects.Count(cols => cols.GridColumn > this.GridColumn) > 0;
            }
        }

        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
                if(LayoutObjectGrid != null)
                {
                    LayoutObjectGrid.SetActiveCell(this);
                }

                base.OnPropertyChanged("IsSelected");
            }
        }

        public ILayoutObjectViewModel CellRight
        {
            get
            {

                var cellRight = _layoutViewModel.LayoutObjects
                                    .Where(cols => cols.GridColumn == this.GridColumn + 1 && cols.GridRow == this.GridRow);
                if(cellRight.Any())
                {
                    return cellRight.First();
                }

                return null;
            }
        }

        public ILayoutObjectViewModel CellLeft
        {
            get
            {

                var cellRight = _layoutViewModel.LayoutObjects
                                    .Where(cols => cols.GridColumn == this.GridColumn - 1 && cols.GridRow == this.GridRow);
                if(cellRight.Any())
                {
                    return cellRight.First();
                }

                return null;
            }
        }

        public ILayoutObjectViewModel CellAbove
        {
            get
            {

                var cellRight = _layoutViewModel.LayoutObjects
                                    .Where(cols => cols.GridColumn == this.GridColumn && cols.GridRow == this.GridRow - 1);
                if(cellRight.Any())
                {
                    return cellRight.First();
                }

                return null;
            }
        }

        public ILayoutObjectViewModel CellBelow
        {
            get
            {

                var cellRight = _layoutViewModel.LayoutObjects
                                    .Where(cols => cols.GridColumn == this.GridColumn && cols.GridRow == this.GridRow + 1);
                if(cellRight.Any())
                {
                    return cellRight.First();
                }

                return null;
            }
        }

        public ILayoutObjectViewModel SelectedLayoutObject
        {
            get { return this; }

        }

        public bool HasContent
        {
            get
            {
                return !string.IsNullOrEmpty(WebpartServiceName);
            }
        }

        public string PreviousXmlConfig { get; set; }

        public string PreviousWebpartServiceName { get; set; }
        public string PreviousIconPath { get; set; }

        #endregion

        #region Commands

        public ICommand ClearAllCommand
        {
            get
            {

                if(_clearAllCommand == null)
                {
                    _clearAllCommand = new RelayCommand(c => ClearAll());
                }

                return _clearAllCommand;
            }
        }

        public ICommand EditCommand
        {
            get
            {
                if(_editCommand == null)
                {
                    _editCommand = new RelayCommand(c => OpenPropertyEditor(), c => !string.IsNullOrEmpty(XmlConfiguration));
                }
                return _editCommand;
            }

        }

        public ICommand CopyCommand
        {
            get
            {
                return _layoutViewModel.CopyCommand;
            }
        }

        public ICommand PasteCommand
        {
            get
            {
                return _layoutViewModel.PasteCommand;
            }
        }

        public ICommand CutCommand
        {
            get
            {
                return _layoutViewModel.CutCommand;
            }
        }

        public ICommand DeleteCommand
        {
            get
            {
                if(_deleteCommand == null)
                {
                    _deleteCommand = new RelayCommand(c => _layoutViewModel.LayoutObjects.Remove(this));
                }
                return _deleteCommand;
            }
        }

        public ICommand AddRowAboveCommand
        {
            get
            {
                if(_addRowAboveCommand == null)
                {
                    _addRowAboveCommand = new RelayCommand(c => AddRowAbove());
                }
                return _addRowAboveCommand;
            }
        }

        public ICommand AddRowBelowCommand
        {
            get
            {
                if(_addRowBelowCommand == null)
                {
                    _addRowBelowCommand = new RelayCommand(c => AddRowBelow());
                }
                return _addRowBelowCommand;
            }
        }

        public ICommand AddColumnRightCommand
        {
            get
            {
                if(_addColumnRightCommand == null)
                {
                    _addColumnRightCommand = new RelayCommand(c => AddColumnRight());
                }
                return _addColumnRightCommand;
            }
        }

        public ICommand AddColumnLeftCommand
        {
            get
            {
                if(_addColumnLeftCommand == null)
                {
                    _addColumnLeftCommand = new RelayCommand(c => AddColumnLeft());
                }
                return _addColumnLeftCommand;
            }
        }

        public ICommand DeleteRowCommand
        {
            get
            {
                if(_deleteRowCommand == null)
                {
                    _deleteRowCommand = new RelayCommand(c => DeleteRow());
                }
                return _deleteRowCommand;
            }
        }

        public ICommand DeleteColumnCommand
        {
            get
            {
                if(_deleteColumnCommand == null)
                {
                    _deleteColumnCommand = new RelayCommand(c => DeleteColumn());
                }
                return _deleteColumnCommand;
            }
        }

        public ICommand DeleteCellCommand
        {
            get
            {
                if(_deleteCellCommand == null)
                {
                    _deleteCellCommand = new RelayCommand(c => Delete(true));
                }
                return _deleteCellCommand;
            }
        }

        #endregion

        #region Public Methods

        public void Save(string value, bool closeBrowserWindow = true)
        {
        }

        public void NavigateTo(string uri, string args, string returnUri)
        {

        }

        public void OpenPropertyEditor()
        {
            if(!string.IsNullOrEmpty(SelectedLayoutObject.WebpartServiceName))
            {
                Logger.TraceInfo("Publish message of type - " + typeof(ShowWebpartWizardMessage), GetType().Name);
                _eventPublisher.Publish(new ShowWebpartWizardMessage(this));
            }
        }

        public void Dev2Set(string data, string uri)
        {

            if(LayoutObjectGrid != null)
            {
                Uri postUri;
                if(!Uri.TryCreate(LayoutObjectGrid.ResourceModel.Environment.Connection.WebServerUri, uri, out postUri))
                {
                    if(!Uri.TryCreate(new Uri(StringResources.Uri_WebServer), uri, out postUri))
                    {
                        throw new Exception("Unable to create the URL to post wizard information to the server.");
                    }
                }

                IWebCommunicationResponse response = WebCommunication.Post(postUri.AbsoluteUri, data);
                if(response != null)
                {
                    switch(response.ContentType)
                    {
                        case "text/html":

                            //string html = ResourceHelper.PropertyEditorHtmlInject(response.Content, LayoutObjectGrid.MainViewModel.CurrentWebServer);
                            //if (NavigateRequested != null) {
                            //    NavigateRequested(html);
                            //}

                            if(NavigateRequested != null)
                            {
                                NavigateRequested(postUri.AbsoluteUri);
                            }
                            break;

                        case "text/xml":
                            UnlimitedObject xmlResult = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(response.Content);

                            if(xmlResult.HasError)
                            {
                                //Error in property service
                                Close();
                            }
                            else
                            {
                                xmlResult.RemoveElementsByTagName("Dev2System.InstanceId");
                                xmlResult.RemoveElementsByTagName("Dev2System.Bookmark");
                                xmlResult.RemoveElementsByTagName("Dev2System.ParentWorkflowInstanceId");
                                xmlResult.RemoveElementsByTagName("Dev2System.ParentServiceName");
                                xmlResult.RemoveElementsByTagName("Dev2System.FormView");
                                xmlResult.RemoveElementsByTagName("Dev2System.JSON");
                                xmlResult.RemoveElementByTagName("Dev2System.WebServerUrl");
                                // Travis : Added 20.07.2012
                                xmlResult.RemoveElementsByTagName("Dev2System.Dev2ResumeData");

                                dynamic xmlData = new UnlimitedObject("Dev2WebpartConfig");

                                // by ADL 
                                if(xmlResult.ElementExists("ADL"))
                                {
                                    xmlData.AddResponse(xmlResult.GetElement("ADL"));
                                }
                                else
                                {
                                    UnlimitedObject t1 = xmlResult.GetElement("DataList");
                                    xmlData.AddResponse(t1);
                                }

                                // <Dev2XMLResult>
                                //xmlData.AddResponse(xmlResult.GetElement("Dev2ResumeData"));
                                xmlResult = xmlData;
                            }

                            Dev2SetValue(xmlResult.XmlString.Replace("<DataList>", "").Replace("</DataList>", ""));
                            Close();
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public void Dev2SetValue(string value)
        {
            XmlConfiguration = value;
            if(LayoutObjectGrid != null)
            {
                LayoutObjectGrid.UpdateModelItem();
            }

        }

        public void Dev2Done()
        {
            throw new NotImplementedException();
        }

        public void Dev2ReloadResource(string resourceName, string resourceType)
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            Logger.TraceInfo("Publish message of type - " + typeof(CloseWizardMessage), GetType().Name);
            _eventPublisher.Publish(new CloseWizardMessage(this));
        }

        public void Cancel()
        {

            if(!string.IsNullOrEmpty(PreviousWebpartServiceName))
            {
                WebpartServiceName = PreviousWebpartServiceName;
                IconPath = PreviousIconPath;
                XmlConfiguration = PreviousXmlConfig;
                LayoutObjectGrid.UpdateModelItem();
                ClearPreviousContents();

            }
            else
            {
                if(string.IsNullOrEmpty(XmlConfiguration) || XmlConfiguration.ToUpper().Contains("EMPTY"))
                {
                    ClearCellContent(true);
                }
            }

            Close();
        }

        public void CopyFrom(ILayoutObjectViewModel copyObj, bool includeCoOrdinates = false)
        {
            IconPath = copyObj.IconPath;
            WebpartServiceName = copyObj.WebpartServiceName;
            XmlConfiguration = copyObj.XmlConfiguration;
            DisplayName = copyObj.DisplayName;
            if(includeCoOrdinates)
            {
                GridRow = copyObj.GridRow;
                GridColumn = copyObj.GridColumn;
            }

        }

        public void Clear(bool updateModelItem)
        {
            ClearCellContent(updateModelItem);
        }

        public void ClearCellContent(bool updateModelItem)
        {
            IconPath = string.Empty;
            WebpartServiceName = string.Empty;
            XmlConfiguration = string.Empty;
            DisplayName = string.Empty;

            if(updateModelItem)
            {
                LayoutObjectGrid.UpdateModelItem();
            }

        }

        public void Delete(bool updateModelItem)
        {
            var action = new DeleteAction(this, updateModelItem);
            _layoutViewModel.UndoFramework.RecordAction(action);

        }

        public void ClearPreviousContents()
        {
            PreviousWebpartServiceName = string.Empty;
            PreviousXmlConfig = string.Empty;
            PreviousIconPath = string.Empty;
        }

        public void ClearAll()
        {
            var clear = new ClearAllAction(_layoutViewModel);
            _layoutViewModel.UndoFramework.RecordAction(clear);
        }

        public void SetGrid(ILayoutGridViewModel grid)
        {
            _layoutViewModel = grid;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Gets a value from the xml configuration data based on a tag name that matches the element name
        /// chosen by the user.
        /// </summary>
        /// <param name="elementName">The name given to the element by the user</param>
        /// <param name="xmlData">The xml configuration data to traverse and extract the configuration for the element.</param>
        /// <returns></returns>
        private string GetValueByElementName(string elementName, string xmlData)
        {
            string returnValue = string.Empty;
            var displayNameNodeMatch = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(xmlData).xmlData as XElement;
            string displayNameNodeName = elementName;
            if(displayNameNodeMatch != null)
            {
                var displayNameNode = displayNameNodeMatch.DescendantsAndSelf().FirstOrDefault(c => c.Name.ToString().ToUpper().Contains(displayNameNodeName.ToUpper()));
                if(displayNameNode != null)
                {
                    returnValue = displayNameNode.Value;
                }
            }
            return returnValue;
        }

        /// <summary>
        /// Adds a row above this one
        /// </summary>
        private void AddRowAbove()
        {
            var addRowAbove = new AddRowAboveAction(this);
            _layoutViewModel.UndoFramework.RecordAction(addRowAbove);
        }

        /// <summary>
        /// Adds a row below this one
        /// </summary>
        private void AddRowBelow()
        {
            var addRowBelow = new AddRowBelowAction(this);
            _layoutViewModel.UndoFramework.RecordAction(addRowBelow);
        }

        /// <summary>
        /// Adds a column to the left of this one
        /// </summary>
        private void AddColumnLeft()
        {
            var addColumnLeft = new AddColumnLeftAction(this);
            _layoutViewModel.UndoFramework.RecordAction(addColumnLeft);
        }

        /// <summary>
        /// Adds a columns to the right of this one
        /// </summary>
        private void AddColumnRight()
        {
            var addColumnRight = new AddColumnRightAction(this);
            _layoutViewModel.UndoFramework.RecordAction(addColumnRight);
        }

        /// <summary>
        /// Deletes this row
        /// </summary>
        private void DeleteRow()
        {
            var deleteRow = new DeleteRowAction(this);
            _layoutViewModel.UndoFramework.RecordAction(deleteRow);
        }

        /// <summary>
        /// Deletes this column
        /// </summary>
        private void DeleteColumn()
        {
            var deleteColumn = new DeleteColumnAction(this);
            _layoutViewModel.UndoFramework.RecordAction(deleteColumn);
        }

        public void AddLayoutObject(int row, int column)
        {
            var layoutObject = LayoutObjectViewModelFactory.CreateLayoutObject(_layoutViewModel, column, row);
            _layoutViewModel.LayoutObjects.Add(layoutObject);
        }

        public void RemoveLayoutObject(int row, int column)
        {
            var removeItem = _layoutViewModel.LayoutObjects.FirstOrDefault(c => c.GridRow == row && c.GridColumn == column);
            if(removeItem != null)
            {
                int idx = _layoutViewModel.LayoutObjects.IndexOf(removeItem);
                if(idx >= 0)
                {
                    _layoutViewModel.LayoutObjects.RemoveAt(idx);
                }
            }
        }
        #endregion
    }


}
