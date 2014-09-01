using System;
using System.Linq;
using System.Windows;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Studio.Core.AppResources.Converters;

namespace Dev2.Diagnostics
{
    public class DebugOutputFilterStrategy
    {
        #region Class Members

        private readonly DateTimeToStringConverter _dateTimeToStringConverter;
        private readonly TimeSpanToStringConverter _timeSpanToStringConverter;
        private readonly EnumToStringConverter _enumToStringConverter;

        #endregion Class Members

        #region Constructor

        public DebugOutputFilterStrategy()
        {
            if(Application.Current != null)
            {
                _timeSpanToStringConverter = Application.Current.Resources["TimeSpanToStringConverter"] as TimeSpanToStringConverter;
                _dateTimeToStringConverter = Application.Current.Resources["DateTimeToStringConverter"] as DateTimeToStringConverter;
                _enumToStringConverter = Application.Current.Resources["EnumToStringConverter"] as EnumToStringConverter;
            }

            if(_timeSpanToStringConverter == null)
            {
                _timeSpanToStringConverter = new TimeSpanToStringConverter();
            }

            if(_dateTimeToStringConverter == null)
            {
                _dateTimeToStringConverter = new DateTimeToStringConverter();
            }

            if(_enumToStringConverter == null)
            {
                _enumToStringConverter = new EnumToStringConverter();
            }
        }

        #endregion Constructor

        #region Methods

        /// <summary>
        /// Filters the specified content.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="filterText"></param>
        public bool Filter(object content, string filterText)
        {
            if(filterText == null)
            {
                return false;
            }

            filterText = filterText.ToLower();
            IDebugState debugState = content as IDebugState;

            if(debugState != null)
            {
                string convertedActivityType = Convert.ToString(_enumToStringConverter.Convert(debugState.ActivityType, null, null, null));
                if(convertedActivityType.ToLower().Contains(filterText)) return true;

                if(debugState.ActivityType.ToString().ToLower().Contains(filterText)) return true;
                if(debugState.DisplayName != null && debugState.DisplayName.ToLower().Contains(filterText)) return true;
                if(debugState.ActivityType == ActivityType.Step && debugState.Name != null && debugState.Name.ToLower().Contains(filterText)) return true;
                if(debugState.ActivityType == ActivityType.Workflow && debugState.Server != null && debugState.Server.ToLower().Contains(filterText)) return true;
                if(debugState.Version != null && debugState.Version.ToLower().Contains(filterText)) return true;

                if(debugState.ActivityType == ActivityType.Step)
                {
                    string convertedDuration = Convert.ToString(_timeSpanToStringConverter.Convert(debugState.Duration, null, null, null));
                    if(convertedDuration.ToLower().Contains(filterText)) return true;
                }

                if(debugState.ActivityType == ActivityType.Workflow)
                {
                    string convertedStartTime = Convert.ToString(_dateTimeToStringConverter.Convert(debugState.StartTime, null, null, null));
                    if(debugState.StateType == StateType.Before && convertedStartTime.ToLower().Contains(filterText)) return true;

                    string convertedEndTime = Convert.ToString(_dateTimeToStringConverter.Convert(debugState.EndTime, null, null, null));
                    if(debugState.StateType == StateType.After && convertedEndTime.ToLower().Contains(filterText)) return true;
                }

                if(debugState.Inputs != null && debugState.Inputs.Any(o => o.Contains(filterText))) return true;
                if(debugState.Outputs != null && debugState.Outputs.Any(o => o.Contains(filterText))) return true;
            }
            else if(content is string && content.ToString().ToLower().Contains(filterText))
            {
                return true;
            }

            return false;
        }

        #endregion Methods
    }
}
