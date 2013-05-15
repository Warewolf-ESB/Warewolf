using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;

namespace Unlimited.Applications.BusinessDesignStudio {
    public class TextEditorCompletionData : ICompletionData{

        public TextEditorCompletionData(string text, string description="") {
            this.Text = text;
            this.Description = description;
        }

        public void Complete(ICSharpCode.AvalonEdit.Editing.TextArea textArea, ICSharpCode.AvalonEdit.Document.ISegment completionSegment, EventArgs insertionRequestEventArgs) {
            textArea.Document.Replace(completionSegment, this.Text);
        }

        public object Content {
            get { return this.Text; }
        }

        public object Description {
            get;
            private set;
        }

        public System.Windows.Media.ImageSource Image {
            get { return null; }
        }

        public double Priority {
            get { return 0; }
        }

        public string Text {
            get;
            private set;
        }
    }
}
