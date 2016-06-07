using System.Windows;

namespace Dev2.Common.Interfaces.PopupController
{
    public interface IDev2MessageBoxViewModel
    {
        /// <summary>
        /// Focus the okay button when default
        /// </summary>
        bool FocusOk { get; }
        /// <summary>
        /// Focus the yes button when default
        /// </summary>
        bool FocusYes { get; }
        /// <summary>
        /// Focus the no button when default
        /// </summary>
        bool FocusNo { get; }
        /// <summary>
        /// Focus the cancel button when default
        /// </summary>
        bool FocusCancel { get; }

        /// <summary>
        /// message to display
        /// </summary>
        IPopupMessage Message { get; set; }
      
        /// <summary>
        /// result
        /// </summary>
        MessageBoxResult Result { get; set; }

        /// <summary>
        /// owning object
        /// </summary>
        object Parent { get; set; }
        /// <summary>
        /// is this popup active
        /// </summary>
        bool IsActive { get; set; }

        /// <summary>
        /// show thy self
        /// </summary>
        /// <returns></returns>
        MessageBoxResult Show();
    }
}