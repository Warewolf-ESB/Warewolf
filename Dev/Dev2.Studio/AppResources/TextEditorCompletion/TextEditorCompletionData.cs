
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
using ICSharpCode.AvalonEdit.CodeCompletion;

// ReSharper disable once CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio
{
    public class TextEditorCompletionData : ICompletionData
    {

        public TextEditorCompletionData(string text, string description = "")
        {
            Text = text;
            Description = description;
        }

        public void Complete(ICSharpCode.AvalonEdit.Editing.TextArea textArea, ICSharpCode.AvalonEdit.Document.ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, Text);
        }

        public object Content
        {
            get { return Text; }
        }

        public object Description
        {
            get;
            private set;
        }

        public System.Windows.Media.ImageSource Image
        {
            get { return null; }
        }

        public double Priority
        {
            get { return 0; }
        }

        public string Text
        {
            get;
            private set;
        }
    }
}
