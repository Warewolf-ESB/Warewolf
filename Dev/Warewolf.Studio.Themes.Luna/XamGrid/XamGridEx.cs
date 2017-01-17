namespace Warewolf.Studio.Themes.Luna.XamGrid
{
    public class XamGridEx : Infragistics.Controls.Grids.XamGrid
    {
        private ContextMenuSettings _contextMenuSettings;

        /// <summary>
        ///     Gets a reference to the <see cref="ContextMenuSettings" /> object that
        ///     controls all the properties concerning the display of a context menu
        ///     in this <see cref="XamGrid" />.
        /// </summary>
        public ContextMenuSettings ContextMenuSettings
        {
            get
            {
                if (_contextMenuSettings == null)
                {
                    _contextMenuSettings = new ContextMenuSettings();
                    _contextMenuSettings.Grid = this;
                }

                return _contextMenuSettings;
            }
            set
            {
                // ReSharper disable PossibleUnintendedReferenceComparison
                if (value != _contextMenuSettings)
                // ReSharper restore PossibleUnintendedReferenceComparison
                {
                    _contextMenuSettings = value;
                    _contextMenuSettings.Grid = this;

                    OnPropertyChanged("ContextMenuSettings");
                }
            }
        }

        internal void OnContextMenuOpening(object sender, ContextMenuOpeningEventArgs e)
        {
            ContextMenuOpening?.Invoke(sender, e);
            ActiveItem = e.Cell.Row.Data;
        }

        public delegate void OpeningEventHandler(object sender, ContextMenuOpeningEventArgs e);

        public new event OpeningEventHandler ContextMenuOpening;
    }
}
