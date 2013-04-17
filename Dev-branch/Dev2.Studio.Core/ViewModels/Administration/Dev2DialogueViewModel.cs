using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Studio.Core.AppResources;
using Dev2.Studio.Core.Interfaces;
using System.Windows.Input;
using Unlimited.Framework;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dev2.Studio.Core.ViewModels.Base;

namespace Dev2.Studio.Core.ViewModels.Administration {
    
    // Sashen Naidoo - 29-08-2012 - PBI 5037
    [Export(typeof(IDev2DialogueViewModel))]
    public class Dev2DialogueViewModel : IDev2DialogueViewModel {

        #region Members

        public ClosedOperationEventHandler OnOkClick;
        private ICommand _okClicked;
        private ImageSource _imageSource;
        private string _description;
        private string _title;
        private string _descriptionTitleText;

        #endregion Members

        #region Properties

        public String Title {
            get { return _title; }
        }

        public ImageSource ImageSource {
            get { return _imageSource; }
        }


        public string DescriptionTitleText {
            get { return _descriptionTitleText; }
        }

        public String DescriptionText {
            get { return _description; }
        }

        public ICommand OKCommand {
            get {
                if(_okClicked == null) {
                    _okClicked = new RelayCommand(p => { if(OnOkClick != null) OnOkClick(this, null); }, p => true);
                }
                return _okClicked;
                ; }
        }

        #endregion Properties

        #region Public Methods

        public void SetupDialogue(string title, string description, string imageSourceuri, string DescriptionTitleText) {
            SetTitle(title);
            SetDescription(description);
            SetImage(imageSourceuri);
            SetDescriptionTitleText(DescriptionTitleText);
        }

        #endregion

        #region Private Methods

        private void SetTitle(string title) {
            if(string.IsNullOrEmpty(title)) {
                _title = string.Empty;
            }
            else {
                _title = title;
            }
        }

        private void SetDescription(string description) {
            if(string.IsNullOrEmpty(description)) {
                _description = string.Empty;
            }
            else {
                _description = description;
            }
        }

        private void SetImage(string imageSource) {
            if(string.IsNullOrEmpty(imageSource)) {
                _imageSource = null;
            }
            else {
                Uri imageUri;
                bool validUri = Uri.TryCreate(imageSource, UriKind.RelativeOrAbsolute, out imageUri);

                if(validUri) {

                    // Once initialized, the image must be released so that it is usable by other resources
                    var btMap = new BitmapImage();
                    btMap.BeginInit();
                    btMap.UriSource = imageUri;
                    btMap.CacheOption = BitmapCacheOption.OnLoad;
                    btMap.EndInit();
                    _imageSource = btMap;
                }
                else {
                    throw new UriFormatException(String.Format("Uri :{0} was not in the correct format", imageSource));
                }
            }

        }

        private void SetDescriptionTitleText(string text) {
            if(string.IsNullOrEmpty(text)) {
                _descriptionTitleText = string.Empty;
            }
            else {
                _descriptionTitleText = text;
            }
        }

        #endregion Private Methods

        #region Events 

        event ClosedOperationEventHandler IDev2DialogueViewModel.OnOkClick {
            add { this.OnOkClick += value; }
            remove { this.OnOkClick -= value; }
        }

        #endregion Events

        #region IDisposable Implementaton

        public void Dispose() {
            _imageSource = null;
            _description = null;
            _descriptionTitleText = null;
            _title = null;
        }

        #endregion IDisposable Implementation

    }
}
