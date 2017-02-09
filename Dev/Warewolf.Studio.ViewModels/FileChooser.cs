/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Microsoft.Practices.Prism.Mvvm;
using Warewolf.Studio.Core;

namespace Warewolf.Studio.ViewModels
{
    public class FileChooser : BindableBase, IFileChooser
    {
        readonly Action _closeAction;
        private bool _allowMultipleSelection;
        private string _selectedDriveName;
        private string _filesTooltip;
        private string _selectTooltip;

        public MessageBoxResult Result { get; private set; }

        public FileChooser(IList<string> attachments, IFileChooserModel model, Action closeAction, bool allowMultipleSelection)
            : this(model, closeAction, allowMultipleSelection)
        {
            Attachments = attachments;
            Expand(attachments);
        }

        FileChooser(IFileChooserModel model, Action closeAction, bool allowMultipleSelection)
        {
            _closeAction = closeAction;
            Attachments = new List<string>();
            Drives = model.FetchDrives().Select(a => new FileListingModel(model, a, () => OnPropertyChanged("DriveName"), !allowMultipleSelection)).ToList();
            CancelCommand = new DelegateCommand(o => Cancel());
            SaveCommand = new DelegateCommand(o => Save());
            AllowMultipleSelection = allowMultipleSelection;
            FilesTooltip = Resources.Languages.Tooltips.ManagePluginSourceFilesConfigTooltip;
            SelectTooltip = Resources.Languages.Tooltips.ManagePluginSourceSelectConfigFileTooltip;
            if (AllowMultipleSelection)
            {
                FilesTooltip = Resources.Languages.Tooltips.ManageEmailAttachmentDriveNameTooltip;
                SelectTooltip = Resources.Languages.Tooltips.ManageEmailAttachmentAttachTooltip;
            }
        }

        public IList<FileListingModel> Drives { get; set; }

        void Expand(IEnumerable<string> attachments)
        {
            foreach (var attachment in attachments)
            {
                SelectAttachment(attachment, Drives);
            }
        }

        public void SelectAttachment(string name, IEnumerable<IFileListingModel> model)
        {
            if (name.Contains("\\"))
            {
                string node = name.Contains(":") ? name.Substring(0, name.IndexOf("\\", StringComparison.Ordinal) + 1) : name.Substring(0, name.IndexOf("\\", StringComparison.Ordinal));
                var toExpand = model.FirstOrDefault(a => a.Name == node);
                if (toExpand != null)
                {
                    toExpand.IsExpanded = true;
                    SelectAttachment(name.Substring(name.IndexOf("\\", StringComparison.Ordinal) + 1), toExpand.Children);
                }
            }
            else
            {
                var toExpand = model.FirstOrDefault(a => a.Name == name);
                if (toExpand != null)
                {
                    toExpand.IsChecked = true;
                }
            }
        }

        public void Cancel()
        {
            Result = MessageBoxResult.Cancel;
            _closeAction();
        }

        public void Save()
        {
            Result = MessageBoxResult.OK;
            _closeAction();
        }

        public ICommand CancelCommand { get; set; }

        public ICommand SaveCommand { get; set; }

        public IList<string> Attachments { get; set; }

        public string DriveName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(SelectedDriveName))
                {
                    return SelectedDriveName;
                }
                var driveName = string.Join(";", Drives.SelectMany(a => a.FilterSelected(new List<string>())).ToList());
                return driveName;
            }
            set
            {
                OnPropertyChanged(() => DriveName);
            }
        }
        public string FilesTooltip
        {
            get
            {
                return _filesTooltip;
            }
            set
            {
                if (!value.Equals(_filesTooltip))
                {
                    _filesTooltip = value;
                    OnPropertyChanged(() => FilesTooltip);
                }
            }
        }
        public string SelectTooltip
        {
            get
            {
                return _selectTooltip;
            }
            set
            {
                if (!value.Equals(_selectTooltip))
                {
                    _selectTooltip = value;
                    OnPropertyChanged(() => SelectTooltip);
                }
            }
        }
        public string SelectedDriveName
        {
            get { return _selectedDriveName; }
            set
            {
                if (value != null)
                {
                    _selectedDriveName = value;
                    OnPropertyChanged(() => SelectedDriveName);
                    OnPropertyChanged(() => DriveName);
                }
            }
        }

        public List<string> GetAttachments()
        {
            if (!string.IsNullOrWhiteSpace(SelectedDriveName))
            {
                return new List<string> { SelectedDriveName };
            }
            var attachments = Drives.SelectMany(a => a.FilterSelected(new List<string>())).ToList();
            return attachments;
        }

        public bool AllowMultipleSelection
        {
            get { return _allowMultipleSelection; }
            set
            {
                _allowMultipleSelection = value;
                OnPropertyChanged(() => AllowMultipleSelection);
            }
        }
    }
}
