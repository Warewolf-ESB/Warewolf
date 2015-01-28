using System;
using System.Windows.Input;
using Dev2;
using Dev2.Common.Interfaces.Studio.ViewModels.Dialogues;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels
{
    public class DialogueViewModel : BindableBase, IDialogueTemplate
    {
        string _header;
        string _headerDetail;
        string _validationMessage;
        bool _okEnabled;

        public DialogueViewModel(IInnerDialogueTemplate innerTemplate)
        {
            VerifyArgument.IsNotNull("innerTemplate",innerTemplate);
            InnerDialogue = innerTemplate;
            Validate = new DelegateCommand(()=>
            {
                var output = InnerDialogue.Validate();
                OkEnabled = String.IsNullOrEmpty(output);
                ValidationMessage = output;
            });
        }
        public DialogueViewModel() { }
        #region Implementation of IDialogueTemplate

        /// <summary>
        /// The Outer header field
        /// </summary>
        public string Header
        {
            get
            {
                return _header;
            }
            set
            {
                _header = value;
                OnPropertyChanged(()=>Header);
            }
        }
        /// <summary>
        /// The inner Header Field
        /// </summary>
        public string HeaderDetail
        {
            get
            {
                return _headerDetail;
            }
            set
            {
                _headerDetail = value;
                OnPropertyChanged(() => HeaderDetail);
            }
        }
        /// <summary>
        /// Command to Validate if the dialogue can be saves
        /// </summary>
        public ICommand Validate { get; set; }
        /// <summary>
        ///  validateion message to left of ok cancel
        /// </summary>
        public string ValidationMessage
        {
            get
            {
                return _validationMessage;
            }
            set
            {
                _validationMessage = value;
                OnPropertyChanged(ValidationMessage);
            }
        }
        /// <summary>
        /// Nested inner dialogue
        /// </summary>
        public IInnerDialogueTemplate InnerDialogue { get; set; }
        /// <summary>
        /// Can the user click ok
        /// </summary>
        public bool OkEnabled
        {
            get
            {
                return _okEnabled;
            }
            set
            {
                _okEnabled = value;
                OnPropertyChanged(()=>OkEnabled);
            }
        }

        #endregion
    }
}
