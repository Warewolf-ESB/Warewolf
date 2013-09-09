using System.Collections.Generic;

namespace Dev2.Common
{
    public static class CustomIcons
    {
        private enum PopupIcons
        {
            WarningIcon,
            InformationIcon,
            QuestionIcon,
            ErrorIcon
        }

        private static readonly Dictionary<PopupIcons, string> CustomIconsDictionary = new Dictionary<PopupIcons, string>
        {
            {PopupIcons.ErrorIcon, "pack://application:,,,/Warewolf Studio;component/Images/PopupError-32.png"},
            {PopupIcons.WarningIcon, "pack://application:,,,/Warewolf Studio;component/Images/PopupNotSavedWarning-32.png"},
            {PopupIcons.QuestionIcon, "pack://application:,,,/Warewolf Studio;component/Images/GenericHelp-32.png"},
            {PopupIcons.InformationIcon, "pack://application:,,,/Warewolf Studio;component/Images/PopupInformation-32.png"}
        };

        public static string Error
        {
            get
            {
                return CustomIconsDictionary[PopupIcons.ErrorIcon];
            }
        }

        public static string Information
        {
            get
            {
                return CustomIconsDictionary[PopupIcons.InformationIcon];
            }
        }

        public static string Question
        {
            get
            {
                return CustomIconsDictionary[PopupIcons.QuestionIcon];
            }
        }

        public static string Warning
        {
            get
            {
                return CustomIconsDictionary[PopupIcons.WarningIcon];
            }
        }
    }

}
