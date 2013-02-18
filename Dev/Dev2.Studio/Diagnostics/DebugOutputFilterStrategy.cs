using System;
using System.Globalization;
using System.Linq;
using Dev2.Diagnostics;
using Dev2.Enums;
using Dev2.Studio.Core.AppResources.Converters;


namespace Dev2.Studio.Diagnostics
{
    public class DebugOutputFilterStrategy
    {
        #region Class Members

        private DateTimeToStringConverter _dateTimeToStringConverter = null;
        private TimeSpanToStringConverter _timeSpanToStringConverter = null;
        private EnumToStringConverter _enumToStringConverter = null;

        #endregion Class Members

        #region Constructor

        public DebugOutputFilterStrategy()
        {
            if (App.Current != null)
            {
                _timeSpanToStringConverter = App.Current.Resources["TimeSpanToStringConverter"] as TimeSpanToStringConverter;
                _dateTimeToStringConverter = App.Current.Resources["DateTimeToStringConverter"] as DateTimeToStringConverter;
                _enumToStringConverter = App.Current.Resources["EnumToStringConverter"] as EnumToStringConverter;
            }

            if (_timeSpanToStringConverter == null)
            {
                _timeSpanToStringConverter = new TimeSpanToStringConverter();
            }

            if (_dateTimeToStringConverter == null)
            {
                _dateTimeToStringConverter = new DateTimeToStringConverter();
            }

            if (_enumToStringConverter == null)
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
        public bool Filter(object content, string filterText)
        {
            if (filterText == null)
            {
                return false;
            }

            filterText = filterText.ToLower();
            IDebugState debugState = content as IDebugState;

            if (debugState != null)
            {
                string convertedActivityType = Convert.ToString(_enumToStringConverter.Convert(debugState.ActivityType, null, null, null));
                if (convertedActivityType.ToLower().Contains(filterText)) return true;

                if (debugState.ActivityType.ToString().ToLower().Contains(filterText)) return true;
                if (debugState.DisplayName != null && debugState.DisplayName.ToString().ToLower().Contains(filterText)) return true;
                if (debugState.ActivityType == ActivityType.Step && debugState.Name != null && debugState.Name.ToString().ToLower().Contains(filterText)) return true;
                if (debugState.ActivityType == ActivityType.Workflow && debugState.Server != null && debugState.Server.ToString().ToLower().Contains(filterText)) return true;
                if (debugState.Version != null && debugState.Version.ToString().ToLower().Contains(filterText)) return true;

                if (debugState.ActivityType == ActivityType.Step)
                {
                    string convertedDuration = Convert.ToString(_timeSpanToStringConverter.Convert(debugState.Duration, null, null, null));
                    if (convertedDuration.ToLower().Contains(filterText)) return true;
                }

                if (debugState.ActivityType == ActivityType.Workflow)
                {
                    string convertedStartTime = Convert.ToString(_dateTimeToStringConverter.Convert(debugState.StartTime, null, null, null));
                    if (debugState.StateType == StateType.Before && convertedStartTime.ToLower().Contains(filterText)) return true;

                    string convertedEndTime = Convert.ToString(_dateTimeToStringConverter.Convert(debugState.EndTime, null, null,null));
                    if (debugState.StateType == StateType.After && convertedEndTime.ToLower().Contains(filterText)) return true;
                }

                if(debugState.Inputs != null && debugState.Inputs.Any(o => o.Contains(filterText))) return true;
                if(debugState.Outputs != null && debugState.Outputs.Any(o => o.Contains(filterText))) return true;
            }
            else if (content is string && content.ToString().ToLower().Contains(filterText))
            {
                return true;
            }

            return false;
        }

        #endregion Methods
    }
}
