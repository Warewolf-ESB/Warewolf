
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Microsoft.Practices.Prism.Commands;

namespace Dev2.Common.Interfaces
{
    public interface IEmailAttachmentVm
    {
        void Cancel();
        void Save();

        DelegateCommand CancelCommand { get; set; }
        DelegateCommand SaveCommand { get; set; }
        IList<string> Attachments { get; set; }
        string DriveName { get; set; }
        List<string> GetAttachments();
    }
}