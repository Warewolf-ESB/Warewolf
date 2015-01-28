using System;
using System.Windows.Input;

namespace Dev2.Common.Interfaces.Studio.ViewModels.Dialogues
{
    public interface IDialogueTemplate
    {
        /// <summary>
        /// The Outer header field
        /// </summary>
        string Header { get; set; }
        /// <summary>
        /// The inner Header Field
        /// </summary>
        string HeaderDetail { get; set; }
        /// <summary>
        /// Command to Validate if the dialogue can be saves
        /// </summary>
        ICommand Validate { get; set; }
        /// <summary>
        /// Command for save/ok
        /// </summary>
        ICommand OkCommand {get;set;}
        /// <summary>
        /// Command for cancel
        /// </summary>
        ICommand CancelCommand { get; set; }
         /// <summary>
         ///  validateion message to left of ok cancel
         /// </summary>
        string ValidationMessage { get; set; }

        /// <summary>
        /// Nested inner dialogue
        /// </summary>
        IInnerDialogueTemplate InnerDialogue { get; set; }

        /// <summary>
        /// Can the user click ok
        /// </summary>
        bool OkEnabled { get; set; }

    }
}