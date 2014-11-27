
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/



namespace Dev2.CustomControls.Progress
{
    public interface IProgressDialog : IProgressNotifier
    {
        string Label { get; set; }
        string SubLabel { get; set; }
        double ProgressValue { get; set; }
        bool IsCancelButtonEnabled { get; set; }
    }

    public interface IProgressNotifier
    {
        void StartCancel();
        void Show();
        void Close();
        void StatusChanged(string fileName, int progressPercent, long totalBytes);
    }

}
