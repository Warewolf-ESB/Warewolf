using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Gui
{
    public class InfoStepDataContext
    {
        readonly int _stepNumber;
        readonly int _totalSteps;
        List<StepTO> _stepsCollection;

        public List<StepTO> StepsCollection
        {
            get
            {
                return _stepsCollection;
            }
        }

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

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public InfoStepDataContext(int stepNumber = 0, List<string> listOfStepNames = null)
        {
            if(listOfStepNames != null)
            {
                _stepsCollection = new List<StepTO>();
                _stepsCollection.Clear();
                for(int i = 0; i < listOfStepNames.Count(); i++)
                {
                    if(i < stepNumber - 1)
                    {
                        _stepsCollection.Add(new StepTO { StepName = listOfStepNames[i], SpinnerVisibility = Visibility.Collapsed, TickVisibility = Visibility.Visible, StepNameFontWeight = FontWeights.Normal });
                    }
                    else if(i == stepNumber - 1)
                    {
                        StepTO spetToAdd = new StepTO { StepName = listOfStepNames[i], SpinnerVisibility = Visibility.Collapsed, TickVisibility = Visibility.Collapsed, StepNameFontWeight = FontWeights.Bold };
                        if((listOfStepNames[i] == "Installation" || listOfStepNames[i] == "Uninstall"))
                        {
                            spetToAdd.SpinnerVisibility = Visibility.Visible;
                            spetToAdd.TickVisibility = Visibility.Collapsed;
                        }
                        _stepsCollection.Add(spetToAdd);
                    }
                    else
                    {
                        _stepsCollection.Add(new StepTO { StepName = listOfStepNames[i], SpinnerVisibility = Visibility.Collapsed, TickVisibility = Visibility.Collapsed, StepNameFontWeight = FontWeights.Normal });
                    }
                }
                _stepNumber = stepNumber;
                _totalSteps = listOfStepNames.Count;
            }
        }
    }

    public class StepTO
    {
        public string StepName { get; set; }
        public Visibility SpinnerVisibility { get; set; }
        public Visibility TickVisibility { get; set; }
        public FontWeight StepNameFontWeight { get; set; }
    }
}