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
        /// commands touted to go here if ok clicked
        /// </summary>
        void OkClicked();
        /// <summary>
        /// commands touted to go here if yes clicked
        /// </summary>
        void YesClicked();
        /// <summary>
        /// commands touted to go here if no clicked
        /// </summary>
        void NoClicked();
        /// <summary>
        /// commands touted to go here if cancel clicked
        /// </summary>
        void CancelClicked();
        /// <summary>
        /// commands touted to go here if closed clicked
        /// </summary>
        void ClosedClicked();

        /// <summary>
        /// show thy self
        /// </summary>
        /// <returns></returns>
        MessageBoxResult Show();
    }
}