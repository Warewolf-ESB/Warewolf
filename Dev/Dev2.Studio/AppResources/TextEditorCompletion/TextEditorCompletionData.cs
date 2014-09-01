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
