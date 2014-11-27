
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Dev2.Common.Interfaces.Studio;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Core.AppResources;

// ReSharper disable CheckNamespace
namespace Dev2.Studio.ViewModels.Administration
{

    public class DialogueViewModel : IDialogueViewModel
    {

        #region Members

        public ClosedOperationEventHandler OnOkClick;
        private ICommand _okClicked;
        private ICommand _hyperLink;
        private ImageSource _imageSource;
        private string _description;
        private string _title;
        private string _descriptionTitleText;

        #endregion Members

        #region Properties

        public String Title
        {
            get { return _title; }
        }

        public ImageSource ImageSource
        {
            get { return _imageSource; }
        }


        public string DescriptionTitleText
        {
            get { return _descriptionTitleText; }
        }

        public String DescriptionText
        {
            get { return _description; }
        }

        public string Hyperlink { get; private set; }
        public string HyperlinkText { get; private set; }
        public Visibility HyperlinkVisibility { get; private set; }

        public ICommand HyperLinkCommand
        {
            get
            {
                return _hyperLink ?? (_hyperLink = new RelayCommand(p => Hyperlink_OnMouseDown()));
            }
        }

        public ICommand OkCommand
        {
            get
            {
                return _okClicked ?? (_okClicked = new RelayCommand(p =>
                    {
                        if(OnOkClick != null)
                        {
                            OnOkClick(this, null);
                        }
                    }, p => true));
            }
        }

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Show the EULA page ;)
        /// </summary>
        public void Hyperlink_OnMouseDown()
        {
            Process.Start(new Uri(Hyperlink).AbsoluteUri);
        }

        public void SetupDialogue(string title, string description, string imageSourceuri, string descriptionTitleText, string hyperlink = null, string linkText = null)
        {
            SetTitle(title);
            SetDescription(description);
            SetImage(imageSourceuri);
            SetDescriptionTitleText(descriptionTitleText);
            SetHyperlink(hyperlink, linkText);
        }

        #endregion

        #region Private Methods

        private void SetTitle(string title)
        {
            _title = string.IsNullOrEmpty(title) ? string.Empty : title;
        }

        private void SetDescription(string description)
        {
            _description = string.IsNullOrEmpty(description) ? string.Empty : description;
        }

        private void SetImage(string imageSource)
        {
            if(string.IsNullOrEmpty(imageSource))
            {
                _imageSource = null;
            }
            else
            {
                Uri imageUri;
                bool validUri = Uri.TryCreate(imageSource, UriKind.RelativeOrAbsolute, out imageUri);

                if(validUri)
                {

                    // Once initialized, the image must be released so that it is usable by other resources
                    var btMap = new BitmapImage();
                    btMap.BeginInit();
                    btMap.UriSource = imageUri;
                    btMap.CacheOption = BitmapCacheOption.OnLoad;
                    btMap.EndInit();
                    _imageSource = btMap;
                }
                else
                {
                    throw new UriFormatException(String.Format("Uri :{0} was not in the correct format", imageSource));
                }
            }

        }



        private void SetDescriptionTitleText(string text)
        {
            _descriptionTitleText = string.IsNullOrEmpty(text) ? string.Empty : text;
        }

        private void SetHyperlink(string link, string text)
        {
            if(!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(text))
            {
                Hyperlink = link;
                HyperlinkText = text;
                HyperlinkVisibility = Visibility.Visible;
            }

            HyperlinkVisibility = Visibility.Collapsed;
        }

        #endregion Private Methods

        #region Events

        //event ClosedOperationEventHandler IDialogueViewModel.OnOkClick {
        //    add { this.OnOkClick += value; }
        //    remove { this.OnOkClick -= value; }
        //}

        #endregion Events

        #region IDisposable Implementaton

        public void Dispose()
        {
            _imageSource = null;
            _description = null;
            _descriptionTitleText = null;
            _title = null;
        }

        #endregion IDisposable Implementation

    }
}
