/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Warewolf.Studio.Core;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ValueParameterNotUsed

namespace Warewolf.Studio.ViewModels
{
    public class EmailAttachmentVm : BindableBase, IEmailAttachmentVm
    {
        readonly Action _closeAction;

        public MessageBoxResult Result { get; private set; }

        public EmailAttachmentVm(IList<string> attachments, IEmailAttachmentModel model, Action closeAction)
            : this(model, closeAction)
        {
            Attachments = attachments;
            Expand(attachments);
        }

        EmailAttachmentVm(IEmailAttachmentModel model, Action closeAction)
        {
            _closeAction = closeAction;
            Attachments = new List<string>();
            Drives = model.FetchDrives().Select(a => new FileListingModel(model, a, () => OnPropertyChanged("DriveName"))).ToList();
            CancelCommand = new DelegateCommand(Cancel);
            SaveCommand = new DelegateCommand(Save);
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

        public DelegateCommand CancelCommand { get; set; }

        public DelegateCommand SaveCommand { get; set; }

        public IList<string> Attachments { get; set; }

        public string DriveName
        {
            get
            {
                return String.Join(";", Drives.SelectMany(a => a.FilterSelected(new List<string>())).ToList());
            }
            set
            {
                OnPropertyChanged(() => DriveName);
            }
        }

        public List<string> GetAttachments()
        {
            return Drives.SelectMany(a => a.FilterSelected(new List<string>())).ToList();
        }
    }

}