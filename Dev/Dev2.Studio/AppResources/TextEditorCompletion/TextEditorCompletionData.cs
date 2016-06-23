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
using System.Windows.Media;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

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

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, Text);
        }

        public object Content => Text;

        public object Description
        {
            get;
            private set;
        }

        public ImageSource Image => null;

        public double Priority => 0;

        public string Text
        {
            get;
            private set;
        }
    }
}
