using System;
using System.Windows.Input;

namespace Dev2.Common.Interfaces.Studio.ViewModels.Dialogues
{
    public interface IInnerDialogueTemplate
    {
        /// <summary>
        /// called by outer when validating
        /// </summary>
        /// <returns></returns>
        string Validate();
        /// <summary>
        /// Is valid 
        /// </summary>
        bool IsValid { get; set; }

        /// <summary>
        /// Command for save/ok
        /// </summary>
        ICommand OkCommand { get; set; }
        /// <summary>
        /// Command for cancel
        /// </summary>
        ICommand CancelCommand { get; set; }

        bool CanClickOk { get; }

        string SubHeaderText { get;  }

        string HeaderText { get; }
    }
}