using System.Windows.Input;

namespace Gui
{
    public class InfoStepDataContext
    {

        public ICommand WarewolfIconCommand { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public InfoStepDataContext()
        {
            WarewolfIconCommand = new WarewolfCommand(OnMouseDownThing);
        }

        public void OnMouseDownThing()
        {
            ProcessHost.Invoke(string.Empty, "http://www.warewolf.io", string.Empty, false);
        }
    }
}