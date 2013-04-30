namespace Dev2.Studio.Views.Diagnostics
{
    /// <summary>
    /// Interaction logic for DebugOutputWindow.xaml
    /// </summary>
    public partial class DebugOutputView
    {
        //#region Class Members

        //private bool _isOptionsButtonDown = false;

        //#endregion Class Members

        public DebugOutputView()
        {
            InitializeComponent();
            //InitializeEditor();

            //OptionsButton.PreviewMouseUp += OptionsButton_PreviewMouseUp;
            //OptionsButton.PreviewMouseDown += OptionsButton_PreviewMouseDown;
        }

        //void OptionsButton_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        //{
        //    if (_isOptionsButtonDown == false)
        //    {
        //        DebugTreeViewModel debugTreeViewModel = DataContext as DebugTreeViewModel;
        //        if (e != null)
        //        {
        //            debugTreeViewModel.SkipOptionsCommandExecute = true;
        //        }
        //    }
        //    else
        //    {
        //        _isOptionsButtonDown = false;
        //    }
        //}

        
        //void OptionsButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        //{
        //    _isOptionsButtonDown = true;
        //}

        #region InitializeEditor

        //old
        //void InitializeEditor()
        //{
        //    _editor.TextArea.IndentationStrategy = new DefaultIndentationStrategy();

        //    var foldingUpdateTimer = new DispatcherTimer
        //    {
        //        Interval = TimeSpan.FromSeconds(2)
        //    };

        //    var foldingStrategy = new XmlFoldingStrategy();
        //    var foldingManager = FoldingManager.Install(_editor.TextArea);

        //    foldingUpdateTimer.Tick += (sender, e) =>
        //    {
        //        if(foldingManager != null)
        //        {
        //            foldingStrategy.UpdateFoldings(foldingManager, _editor.Document);

        //        }
        //    };
        //    foldingUpdateTimer.Start();
        //}

        #endregion

        #region ToXml

        //old
        //static XElement ToXml(IDebugState debugState)
        //{
        //    var result = new XElement("DebugState");
        //    result.Add(new XAttribute("ID", debugState.ID));
        //    result.Add(new XAttribute("ParentID", debugState.ParentID));
        //    result.Add(new XAttribute("StateType", debugState.StateType));
        //    result.Add(new XAttribute("DisplayName", debugState.DisplayName));
        //    result.Add(new XAttribute("Name", debugState.Name));
        //    result.Add(new XAttribute("ActivityType", debugState.ActivityType));
        //    result.Add(new XAttribute("Version", debugState.Version));
        //    result.Add(new XAttribute("StartTime", debugState.StartTime));
        //    result.Add(new XAttribute("EndTime", debugState.EndTime));
        //    result.Add(new XAttribute("Duration", debugState.Duration.ToString("c")));
        //    result.Add(new XAttribute("Server", debugState.Server));
        //    result.Add(new XAttribute("IsSimulation", debugState.IsSimulation));
        //    result.Add(ToXml("Inputs", debugState.Inputs));
        //    result.Add(ToXml("Outputs", debugState.Outputs));

        //    return result;
        //}

        //old
        //static XElement ToXml(string rootName, IList<IDebugItem> values)
        //{
        //    var root = new XElement(rootName);
        //    if(values != null && values.Count > 0)
        //    {
        //        for(var i = 0; i < values.Count; i++)
        //        {
        //            var def = new XElement("Value");
        //            def.Add(new XAttribute("Name", values[i].Name));
        //            def.Add(new XAttribute("Value", values[i].Value));
        //            root.Add(def);
        //        }
        //    }
        //    return root;
        //}

        #endregion
    }
}
