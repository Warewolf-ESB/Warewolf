using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Composition;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Actions;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.TO;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.ViewModels.WorkSurface;
using Unlimited.Applications.BusinessDesignStudio.Undo;
using Unlimited.Framework;

namespace Dev2.Studio.ViewModels.Web
{

    public class LayoutGridViewModel : BaseWorkSurfaceViewModel, ILayoutGridViewModel
    {
        #region Locals

        private ObservableCollection<ILayoutObjectViewModel> _layoutObject;
        private int _rows = 5;
        private int _columns = 5;
        private readonly IWebActivity _webPageModelItem;
        private readonly IContextualResourceModel _resourceModel;
        //private IEnvironmentModel _resourceEnvironment;
        private ObservableCollection<IResourceModel> _websites;
        private ActionManager _actionManager;

        private IResourceModel _selectedWebsite = null;
        private RelayCommand<string> _openWebsiteCommand;
        private RelayCommand _copyCommand;
        private RelayCommand _pasteCommand;
        private RelayCommand _cutCommand;
        private RelayCommand _moveUpCommand;
        private RelayCommand _moveDownCommand;
        private RelayCommand _moveRightCommand;
        private RelayCommand _moveLeftCommand;
        private RelayCommand _undoCommand;
        private RelayCommand _redoCommand;

        private bool isCopy = false;

        public delegate void SelectionChangedEventHandler(ILayoutObjectViewModel selected);

        #endregion

        #region Ctor

        public LayoutGridViewModel(IWebActivity webActivity)
            : this(EventPublishers.Aggregator, webActivity)
        {
        }

        public LayoutGridViewModel(IEventAggregator eventPublisher, IWebActivity webActivity)
            : this(eventPublisher)
        {
            if(webActivity == null)
            {
                throw new ArgumentNullException("Webpage cannot be null", "Webpage ModelItem");
            }

            _webPageModelItem = webActivity;
            _resourceModel = webActivity.ResourceModel;

            InitializeGrid();

            var websiteResource = webActivity.ResourceModel.Environment.ResourceRepository.All().FirstOrDefault(c => c.ResourceName.Equals((_webPageModelItem as dynamic).WebsiteServiceName, StringComparison.InvariantCultureIgnoreCase));
            if(websiteResource != null)
            {
                SelectedWebsite = websiteResource;
            }

            _websites = webActivity.ResourceModel.Environment.ResourceRepository.All().Where(c => c.Category.ToUpper().Equals("WEBSITE")).ToObservableCollection();
            ImportService.SatisfyImports(this);
        }

        public LayoutGridViewModel()
            : this(EventPublishers.Aggregator)
        {
        }

        public LayoutGridViewModel(IEventAggregator eventPublisher)
            : base(eventPublisher)
        {
            _layoutObject = new ObservableCollection<ILayoutObjectViewModel>();
        }

        #endregion

        #region Commands

        public ICommand UndoCommand
        {
            get
            {
                if(_undoCommand == null)
                {
                    _undoCommand = new RelayCommand(c => Undo(), c => _actionManager.CanUndo);
                }
                return _undoCommand;
            }
        }

        public ICommand RedoCommand
        {
            get
            {
                if(_redoCommand == null)
                {
                    _redoCommand = new RelayCommand(c => Redo(), c => _actionManager.CanRedo);
                }

                return _redoCommand;
            }
        }

        public ICommand MoveUpCommand
        {
            get
            {
                if(_moveUpCommand == null)
                {
                    _moveUpCommand = new RelayCommand(c => MoveUp());
                }
                return _moveUpCommand;
            }
        }

        public ICommand MoveDownCommand
        {
            get
            {
                if(_moveDownCommand == null)
                {
                    _moveDownCommand = new RelayCommand(c => MoveDown());
                }
                return _moveDownCommand;
            }
        }

        public ICommand MoveRightCommand
        {
            get
            {
                if(_moveRightCommand == null)
                {
                    _moveRightCommand = new RelayCommand(c => MoveRight());
                }

                return _moveRightCommand;
            }
        }

        public ICommand MoveLeftCommand
        {
            get
            {
                if(_moveLeftCommand == null)
                {
                    _moveLeftCommand = new RelayCommand(c => MoveLeft());
                }
                return _moveLeftCommand;
            }
        }

        public ICommand CopyCommand
        {
            get
            {
                if(_copyCommand == null)
                {
                    _copyCommand = new RelayCommand(c => Copy(), c => CanCopyOrCut);
                }
                return _copyCommand;
            }

        }

        public ICommand CutCommand
        {
            get
            {
                if(_cutCommand == null)
                {
                    _cutCommand = new RelayCommand(c => Cut(), c => CanCopyOrCut);
                }
                return _cutCommand;
            }
        }

        public ICommand PasteCommand
        {
            get
            {
                if(_pasteCommand == null)
                {
                    _pasteCommand = new RelayCommand(c => Paste(), c => CanPaste);
                }
                return _pasteCommand;
            }
        }

        public ICommand OpenWebsiteCommand
        {
            get
            {
                if(_openWebsiteCommand == null)
                {
                    _openWebsiteCommand = new RelayCommand<string>(_ => EventPublisher.Publish(new AddWorkSurfaceMessage(_resourceModel)));
                }
                return _openWebsiteCommand;
            }
        }
        #endregion

        #region Properties

        public bool IsAnyCellSelected
        {
            get
            {
                return _layoutObject.Any(c => c.IsSelected);
            }
        }

        public ActionManager UndoFramework
        {
            get
            {
                return _actionManager;
            }
            set
            {
                _actionManager = value;
            }
        }


        public bool CanPaste
        {
            get
            {
                return CopiedCell != null;
            }
        }

        public bool CanCopyOrCut
        {
            get
            {
                if((_activeCell != null) && !string.IsNullOrEmpty(_activeCell.WebpartServiceName))
                {
                    return true;
                }
                return false;
            }
        }

        public ObservableCollection<IResourceModel> Websites
        {
            get
            {
                return _websites;
            }
        }

        public IResourceModel SelectedWebsite
        {
            get
            {
                return _selectedWebsite;
            }
            set
            {
                _selectedWebsite = value;
                if(_webPageModelItem != null)
                {
                    _webPageModelItem.WebsiteServiceName = SelectedWebsite == null ? StringResources.Webpage_Default_Website : _selectedWebsite.ResourceName;
                    UpdateModelItem();
                }
                base.OnPropertyChanged("SelectedWebsite");
            }
        }

        public string MetaTags
        {
            get
            {
                if(_webPageModelItem != null)
                {
                    return (_webPageModelItem as dynamic).MetaTags;
                }
                return string.Empty;
            }
            set
            {
                if(_webPageModelItem != null)
                {
                    _webPageModelItem.MetaTags = value;
                    base.OnPropertyChanged("MetaTags");
                }

            }
        }

        public string FormEncodingType
        {
            get
            {
                if(_webPageModelItem != null)
                {
                    return (_webPageModelItem as dynamic).FormEncodingType;
                }
                return string.Empty;
            }
            set
            {
                if(_webPageModelItem != null)
                {
                    _webPageModelItem.FormEncodingType = value;
                    base.OnPropertyChanged("FormEncodingType");

                }
            }
        }

        public IWebActivity ActivityModelItem
        {
            get
            {
                return _webPageModelItem;
            }
        }

        //public IEnvironmentModel ResourceEnvironment 
        //{
        //    get 
        //    {
        //        IEnvironmentModel environment = null;

        //        if (_resourceModel != null)
        //        {
        //            environment = _resourceModel.Environment;
        //        }

        //        return environment;
        //    }
        //}

        public ObservableCollection<ILayoutObjectViewModel> Selection
        {
            get
            {
                return _layoutObject.Where(c => c.IsSelected).ToObservableCollection();
            }
        }

        public int Rows
        {
            get
            {
                return _rows;
            }
            set
            {
                _rows = value;
                UpdateModelItem();
                base.OnPropertyChanged("Rows");
            }
        }

        public int Columns
        {
            get
            {
                return _columns;
            }
            set
            {
                _columns = value;
                UpdateModelItem();
                base.OnPropertyChanged("Columns");
            }
        }

        public IContextualResourceModel ResourceModel
        {
            get
            {
                return _resourceModel;
            }
        }

        public string XmlConfiguration
        {
            get
            {
                if(_webPageModelItem != null)
                {
                    return (_webPageModelItem as dynamic).XMLConfiguration;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        ILayoutObjectViewModel _activeCell = null;
        public ILayoutObjectViewModel ActiveCell
        {
            get
            {
                return _activeCell;
            }
        }

        public ILayoutObjectViewModel CopiedCell
        {
            get;
            set;
        }
        public ILayoutObjectViewModel CutCell
        {
            get;
            set;
        }


        public ObservableCollection<ILayoutObjectViewModel> LayoutObjects
        {
            get
            {
                return _layoutObject;
            }
        }
        #endregion

        #region Methods

        internal void InitializeGrid()
        {

            if(SelectedWebsite == null)
            {
                SelectedWebsite =
                    _resourceModel.Environment.ResourceRepository.All().FirstOrDefault(
                        c =>
                        c.ResourceName.Equals(StringResources.Webpage_Default_Website,
                                              StringComparison.InvariantCultureIgnoreCase));
            }


            dynamic xmlConfig = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(XmlConfiguration);

            if(xmlConfig.Rows is UnlimitedObject)
            {
                xmlConfig.Rows = 4;
            }

            if(xmlConfig.Cols is UnlimitedObject)
            {
                xmlConfig.Cols = 4;
            }

            int.TryParse(xmlConfig.Rows, out _rows);
            int.TryParse(xmlConfig.Cols, out _columns);

            if(string.IsNullOrEmpty(XmlConfiguration) || (XmlConfiguration == "<WebParts/>"))
            {

                for(int row = 0; row < _rows; row++)
                {
                    for(int col = 0; col < _columns; col++)
                    {
                        ILayoutObjectViewModel obj = LayoutObjectViewModelFactory.CreateLayoutObject(this, col, row);
                        _layoutObject.Add(obj);
                    }
                }
            }
            else
            {
                BindXmlConfigurationToGrid();
            }

            _actionManager = new ActionManager();
        }

        public void BindXmlConfigurationToGrid()
        {
            if(!string.IsNullOrEmpty(_webPageModelItem.XMLConfiguration))
            {
                dynamic xmlconfig = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(_webPageModelItem.XMLConfiguration);
                dynamic objWebpartList = xmlconfig.WebPart;
                //IF WEBPARTS EXIST THEN REBIND GRID
                if(objWebpartList.GetType() != typeof(UnlimitedObject))
                {
                    foreach(dynamic wp in objWebpartList)
                    {
                        BindLayoutObjectToXml(wp);
                    }
                }
                if(_layoutObject.Any())
                {
                    _rows = _layoutObject.Max(c => c.GridRow) + 1;
                    _columns = _layoutObject.Max(c => c.GridColumn) + 1;
                }
            }
        }

        private void BindLayoutObjectToXml(dynamic wp)
        {
            ILayoutObjectViewModel LayoutObject = LayoutObjectViewModelFactory.CreateLayoutObject(this);

            string config = string.Empty;

            LayoutObject.WebpartServiceName = wp.WebPartServiceName;
            LayoutObject.GridColumn = int.Parse(wp.ColumnIndex);
            LayoutObject.GridRow = int.Parse(wp.RowIndex);

            IEnumerable<IResourceModel> resourceMatch = _resourceModel.Environment.ResourceRepository.All().Where(c => c.ResourceName.Equals(LayoutObject.WebpartServiceName, StringComparison.InvariantCultureIgnoreCase));
            if(resourceMatch.Any())
            {
                LayoutObject.IconPath = resourceMatch.First().IconPath;
            }
            if(wp is UnlimitedObject)
            {
                if(wp is string)
                {
                    config = wp;
                }
                else
                {
                    config = wp.XmlString;
                }
            }


            LayoutObject.XmlConfiguration = config;

            _layoutObject.Add(LayoutObject);
        }

        public void UpdateModelItem()
        {
            if(_layoutObject.Count == 0) { return; }
            StringBuilder configuration = new StringBuilder();
            configuration.Append("<WebPage>");
            configuration.Append(string.Format("<WebPageServiceName>{0}</WebPageServiceName>", _resourceModel.ResourceName));
            configuration.Append("<WebParts>");
            //Tiles will contain all configuration from the popup/property editor windows
            _layoutObject.ToList().ForEach(tle =>
            {
                configuration.Append("<WebPart>");
                configuration.Append("<WebPartServiceName>" + tle.WebpartServiceName + "</WebPartServiceName>");
                configuration.Append("<ColumnIndex>" + tle.GridColumn + "</ColumnIndex>");
                configuration.Append("<RowIndex>" + tle.GridRow + "</RowIndex>");
                if(!string.IsNullOrEmpty(tle.XmlConfiguration) && (tle.XmlConfiguration.Contains("<Dev2WebpartConfig>")))
                {
                    string theConfig = tle.XmlConfiguration;
                    theConfig = theConfig.Remove(0, theConfig.IndexOf("<Dev2WebpartConfig>"));
                    theConfig = theConfig.Substring(0, theConfig.IndexOf("</Dev2WebpartConfig>") + 20);
                    configuration.Append(theConfig);
                }
                else if(!string.IsNullOrEmpty(tle.XmlConfiguration) && (tle.XmlConfiguration.Contains("<Dev2XMLResult>")))
                {
                    string theConfig = tle.XmlConfiguration;
                    theConfig = theConfig.Remove(0, theConfig.IndexOf("<Dev2XMLResult>"));
                    theConfig = theConfig.Substring(0, theConfig.IndexOf("</Dev2XMLResult>") + 16);
                    configuration.Append(theConfig);
                }
                configuration.Append("</WebPart>");
            });
            configuration.Append(string.Format("<Rows>{0}</Rows><Cols>{1}</Cols>", _layoutObject.Max(c => c.GridRow) + 1, _layoutObject.Max(c => c.GridColumn) + 1));
            configuration.Append("</WebParts>");
            configuration.Append("</WebPage>");

            if(_webPageModelItem != null)
            {
                _webPageModelItem.XMLConfiguration = configuration.ToString();
                Deploy();
            }
            Navigate();
        }

        public void Deploy()
        {
            EventPublisher.Publish(new SaveResourceMessage(_resourceModel, false));
        }

        public void RemoveRow(int rowToDelete)
        {
            if(LayoutObjects.Max(c => c.GridRow) > 0)
            {

                LayoutObjects
                    .Where(row => row.GridRow == rowToDelete)
                    .ToList()
                    .ForEach(row => LayoutObjects.Remove(row));

                LayoutObjects
                    .Where(row => row.GridRow > rowToDelete)
                    .ToList()
                    .ForEach(row => row.GridRow -= 1);

                Rows -= 1;

                DebugAssertion();

            }
        }

        public void SetDefaultSelected()
        {
            LayoutObjects.ToList().ForEach(c => c.IsSelected = false);
            ILayoutObjectViewModel first = LayoutObjects.FirstOrDefault();
            if(first != null)
            {
                first.IsSelected = true;
            }

            DebugAssertion();
        }

        private void DebugAssertion()
        {
            var t = from c in LayoutObjects
                    group c by new { c.GridRow, c.GridColumn } into grp
                    where grp.Count() > 1
                    select new { grp.Key, Duplicates = grp.Count() };


            Debug.Assert(!t.Any(), "Duplicate Objects found at grid cells");
        }

        public void RemoveColumn(int columnToDelete)
        {
            if(LayoutObjects.Max(c => c.GridColumn) > 0)
            {

                LayoutObjects
                    .Where(col => col.GridColumn == columnToDelete)
                    .ToList()
                    .ForEach(col => LayoutObjects.Remove(col));

                LayoutObjects
                    .Where(col => col.GridColumn > columnToDelete)
                    .ToList()
                    .ForEach(col => col.GridColumn -= 1);

                Columns -= 1;
            }
        }

        public void MoveUp()
        {
            if(!IsAnyCellSelected)
            {
                return;
            }
            ILayoutObjectViewModel currentCell = ActiveCell;
            if(currentCell.CellAbove != null)
            {
                currentCell.IsSelected = false;
                currentCell.CellAbove.IsSelected = true;
            }
        }

        public void MoveDown()
        {
            if(!IsAnyCellSelected)
            {
                return;
            }
            ILayoutObjectViewModel currentCell = ActiveCell;
            if(currentCell.CellBelow != null)
            {
                currentCell.IsSelected = false;
                currentCell.CellBelow.IsSelected = true;
            }

        }

        public void MoveLeft()
        {
            if(!IsAnyCellSelected)
            {
                return;
            }
            ILayoutObjectViewModel currentCell = ActiveCell;
            if(currentCell.CellLeft != null)
            {
                currentCell.IsSelected = false;
                currentCell.CellLeft.IsSelected = true;
            }
        }

        public void MoveRight()
        {
            if(!IsAnyCellSelected)
            {
                return;
            }
            ILayoutObjectViewModel currentCell = ActiveCell;
            if(currentCell.CellRight != null)
            {
                currentCell.IsSelected = false;
                currentCell.CellRight.IsSelected = true;
            }
        }

        public void Copy()
        {
            isCopy = true;
            CopiedCell = LayoutObjectViewModelFactory.CreateLayoutObject(this);
            CopiedCell.CopyFrom(ActiveCell, true); ;
        }

        public void Cut()
        {
            isCopy = false;
            CutAction cut = new CutAction(ActiveCell);
            _actionManager.RecordAction(cut);
        }

        public void Paste()
        {
            PasteAction paste = new PasteAction(this, isCopy);
            _actionManager.RecordAction(paste);
        }

        public void Move(ILayoutObjectViewModel source, ILayoutObjectViewModel target)
        {
            MoveAction move = new MoveAction(source, target);
            _actionManager.RecordAction(move);
        }

        public void Undo()
        {
            if(_actionManager.CanUndo)
            {
                _actionManager.Undo();
            }
        }

        public void Redo()
        {
            if(_actionManager.CanRedo)
            {
                _actionManager.Redo();
            }
        }

        public void SetActiveCell(ILayoutObjectViewModel cell)
        {
            _activeCell = cell;
            EventPublisher.Publish(new SetActivePageMessage(cell));
            //Mediator.SendMessage(MediatorMessages.SetActivePage, cell);
        }

        public void UpdateLayout()
        {
            base.OnPropertyChanged("LayoutObjects");
        }

        public void AddNewUIElement(ILayoutObjectViewModel targetCell, string webPartServiceName, string iconPath)
        {
            var add = new AddNewAction(targetCell, webPartServiceName, iconPath);
            _actionManager.RecordAction(add);
            targetCell.EditCommand.Execute(null);
        }

        public void Navigate()
        {
            dynamic postData = UnlimitedObject.GetStringXmlDataAsUnlimitedObject(XmlConfiguration);
            postData.RemoveElementsByTagName("WebsiteServiceName");
            if(SelectedWebsite != null)
            {
                postData.WebsiteServiceName = SelectedWebsite.ResourceName;
            }

            Uri uri;
            if(!Uri.TryCreate(_resourceModel.Environment.Connection.WebServerUri, "/services/Web Preview", out uri))
            {
                Uri.TryCreate(new Uri(StringResources.Uri_WebServer), "/services/Web Preview", out uri);
            }

            //Browser.Navigate(uri, string.Empty, postData.XmlString);
            WebBrowserNavigateRequestTO webBrowserNavigateRequestTO = new WebBrowserNavigateRequestTO(this, uri.AbsoluteUri, postData.XmlString);
            //Mediator.SendMessage(MediatorMessages.UpdateWebpagePreview, webBrowserNavigateRequestTO);
            EventPublisher.Publish(new UpdateWebpagePreviewMessage(webBrowserNavigateRequestTO));
        }

        #endregion
    }

}
