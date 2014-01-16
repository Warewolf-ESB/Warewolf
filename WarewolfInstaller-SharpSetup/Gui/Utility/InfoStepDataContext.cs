using System.Windows.Input;

namespace Gui
{
    public class InfoStepDataContext
    {
        readonly int _stepNumber;
        readonly int _totalSteps;

        public string StepsString
        {
            get
            {
                if(_stepNumber == 0)
                {
                    return string.Empty;
                }
                return string.Format("{0} of {1} Steps", _stepNumber, _totalSteps);
            }
        }

        public ICommand WarewolfIconCommand { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public InfoStepDataContext(int stepNumber = 0, int totalSteps = 0)
        {
            _stepNumber = stepNumber;
            _totalSteps = totalSteps;
            WarewolfIconCommand = new WarewolfCommand(OnMouseDownThing);
        }

        public void OnMouseDownThing()
        {
            ProcessHost.Invoke(string.Empty, "http://www.warewolf.io", string.Empty, false);
        }
    }
}