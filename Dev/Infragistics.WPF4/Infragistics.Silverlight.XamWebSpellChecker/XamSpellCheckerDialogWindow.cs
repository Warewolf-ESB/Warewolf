using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Collections.ObjectModel;
using Infragistics;
using Infragistics.Controls.Interactions.Primitives;
using System.Collections.Generic;
using System.ComponentModel;


namespace Infragistics.Controls.Interactions
{
    /// <summary>
    /// The Dialog displayed by the <see cref="XamSpellChecker"/>.
    /// </summary>
    [TemplatePart(Name = "Context", Type = typeof(TextBlock))]
    [TemplatePart(Name = "Suggestions", Type = typeof(Selector))]
    [TemplatePart(Name = "ChangeTo", Type = typeof(TextBox))]
    [TemplateVisualState(Name = "Normal")]
    [TemplateVisualState(Name = "SpellCheckComplete")]

    [DesignTimeVisible(false)]

    public class XamSpellCheckerDialogWindow : XamDialogWindow, ICommandTarget
    {
        #region Members
        internal XamSpellChecker _checker;
        internal ICheckerEngine _spellCheckerEngine;
        TextBlock _context;
        Selector _suggestions;
        TextBox _changeTo;
        List<BadWord> _ignoredWords;
        int _checkerZIndex; 
        #endregion //Members

        #region Constructors
        /// <summary>
        /// Creates an instance of the XamSpellCheckerDialog control. 
        /// </summary>
        public XamSpellCheckerDialogWindow()
        {            
            this.DefaultStyleKey = typeof(XamSpellCheckerDialogWindow);
            this._ignoredWords = new List<BadWord>();            
            this.WindowStateChanged += new EventHandler<WindowStateChangedEventArgs>(XamSpellCheckerDialogWindow_WindowStateChanged);         
        }

        void XamSpellCheckerDialogWindow_WindowStateChanged(object sender, WindowStateChangedEventArgs e)
        {
            if (e.NewWindowState == WindowState.Normal)
            {
                if (this._checker != null && this._checker.SpellCheckTargetElements.Count > 0 && this._checker.CurrentSpellCheckTargetIndex < this._checker.SpellCheckTargetElements.Count)
                {
                    object value = this._checker.SpellCheckTargetElements[this._checker.CurrentSpellCheckTargetIndex].Value ?? string.Empty;

                    this._spellCheckerEngine.Check(value.ToString());

                    this.GetBadWords();

                    this.UpdateDialog();
                }

                

                if (this._checker != null)
                {
                    if (e.NewWindowState == WindowState.Hidden)
                    {
                        Canvas.SetZIndex(this._checker, this._checkerZIndex);
                    }
                    else
                    {
                        this._checkerZIndex = Canvas.GetZIndex(this._checker);
                        Canvas.SetZIndex(this._checker, int.MaxValue);
                    }
                }

            }

            XamSpellCheckerDialogWindow.RefreshCommands();
        }
      

        #endregion //Constructors

        #region Overrides
        /// <summary>
		/// Builds the visual tree for the <see cref="XamSpellCheckerDialogWindow"/> when a new template is applied. 
        /// </summary>
        public override void OnApplyTemplate()
        {
            _context = GetTemplateChild("Context") as TextBlock;
            _suggestions = GetTemplateChild("Suggestions") as Selector;
            _changeTo = GetTemplateChild("ChangeTo") as TextBox;

            if (this._checker != null)
            {
                if (this._checker.SpellCheckTargetElements.Count > 0 && this._checker.CurrentSpellCheckTargetIndex < this._checker.SpellCheckTargetElements.Count)
                {
                    object value = this._checker.SpellCheckTargetElements[this._checker.CurrentSpellCheckTargetIndex].Value ?? string.Empty;

                    this._spellCheckerEngine.Check(value.ToString());

                    this.GetBadWords();

                    this.UpdateDialog();
                }
            }
            base.OnApplyTemplate();
        }
        
        

        #endregion //Overrides

        #region Properties

        #region Public
        /// <summary>
        /// Reference to the XamSpellChecker dialog window.
        /// </summary>
        public XamSpellChecker SpellChecker
        {
            get { return this._checker; }
            internal set 
			{ 
				this._checker =  value; 
				if(this._checker != null)
					this._spellCheckerEngine = this._checker.SpellCheckerWrapper;

			}
        }       

        
        #endregion //Public

        #region Internal
        		       
        #region BadWords

        
        ObservableCollection<BadWord> badWords;
        internal ObservableCollection<BadWord> BadWords
        {
            get
            {
                if (badWords == null)
                    badWords = new ObservableCollection<BadWord>();
                return badWords;
            }
            set
            {
                badWords = value;
            }
        }

        #endregion

        #endregion //Internal

        #region Private

        

        #endregion //Private

        #endregion //Properties

        #region Methods

        #region Private

        #region GetBadWords

        internal void GetBadWords()
        {
            BadWord bw = null;
            this.BadWords.Clear();
            while ((bw = this._spellCheckerEngine.NextBadWord()) != null)
            {
                bw.Suggestions = this._spellCheckerEngine.FindSuggestions() as List<string>;	//find suggestions                
                this.BadWords.Add(bw);
            }
            if (this.BadWords.Count > 0)
                this.CurrentBadWordIndex = 0;
            this._spellCheckerEngine.SetPosition(0);
        }

        #endregion

        #region RefreshListbox

        private void RefreshListbox(List<string> suggestions)
        {
            if (suggestions.Count > 0)
            {
                this._suggestions.ItemsSource = null;
                this._suggestions.SelectedIndex = -1;

                
                Dictionary<string, int> uniqueStore = new Dictionary<string, int>();
                List<string> finalList = new List<string>();

                foreach (string currValue in suggestions)
                {
                    if (!uniqueStore.ContainsKey(currValue))
                    {
                        uniqueStore.Add(currValue, 0);
                        finalList.Add(currValue);
                    }
                }

                this._suggestions.ItemsSource = finalList;
                ListBox suggestionBox = this._suggestions as ListBox;
                

                if (suggestionBox != null)
                {
                    suggestionBox.ScrollIntoView(suggestionBox.Items[0]);
                    suggestionBox.UpdateLayout();
                }
                this._changeTo.Text = suggestions[0];
                this._suggestions.SelectedIndex = 0;
            }
            else
            {
                this._suggestions.ItemsSource = null;
                this._suggestions.SelectedIndex = -1;
            }
        }

        #endregion

        #region RefreshCommands

        private static void RefreshCommands()
        {
            CommandSourceManager.NotifyCanExecuteChanged(typeof(PreviousFieldCommand));
            CommandSourceManager.NotifyCanExecuteChanged(typeof(NextFieldCommand));
            CommandSourceManager.NotifyCanExecuteChanged(typeof(ChangeCommand));
            CommandSourceManager.NotifyCanExecuteChanged(typeof(ChangeAllCommand));
            CommandSourceManager.NotifyCanExecuteChanged(typeof(IgnoreCommand));
            CommandSourceManager.NotifyCanExecuteChanged(typeof(IgnoreAllCommand));
        }

        #endregion //RefreshCommands

        #endregion //Private

        #endregion //Methods

        #region Commanding
        #region Commands
        internal void Ignore()
        {
            if (this.BadWords.Count > 0 && this.BadWords[this.CurrentBadWordIndex] != null)
            {                
                this.BadWords.Remove(this.BadWords[this.CurrentBadWordIndex]);               
            }

            this.UpdateDialog();            
        }

        internal void IgnoreAll()
        {
            if (this.BadWords.Count > 0 && this.BadWords[this.CurrentBadWordIndex] != null)
            {
               foreach(BadWord bw in this.BadWords)
               {
                   if (bw.Word == this.BadWords[this.CurrentBadWordIndex].Word)
                   {
                       this._ignoredWords.Add(bw);
                   }
               }

               foreach (BadWord bw in this._ignoredWords)
               {
                   this.BadWords.Remove(bw);
               }
               this._ignoredWords.Clear();
            }
           
            this.UpdateDialog();
        }

        private void MoveWordOffsets(int delta, int start)
        {
            for (int i = start; i < this.BadWords.Count; i++)
            {
                BadWord badWord = this.BadWords[i];
                badWord.StartPosition = badWord.StartPosition + delta;
                badWord.EndPosition = badWord.EndPosition + delta;
            }
        }

        internal int CurrentBadWordIndex
        {
            get;
            set;
        }

        internal void AddToDictionary()
        {
            if(this.BadWords[this.CurrentBadWordIndex] != null)
            {
                this._spellCheckerEngine.AddWord(this.BadWords[this.CurrentBadWordIndex].Word);
                this.IgnoreAll();                
            }
        }

        internal void Change()
        {
            if (this.BadWords[this.CurrentBadWordIndex] != null && this._checker!= null)
            {                
                
                string newText = "";

                string newWord = this._changeTo.Text;
                int delta;

                newText += this._checker.CurrentSpellCheckBindingValue.Substring(0, this.BadWords[this.CurrentBadWordIndex].StartPosition);
                newText += newWord;
                newText += this._checker.CurrentSpellCheckBindingValue.Substring(this.BadWords[this.CurrentBadWordIndex].EndPosition);

                
                delta = this.BadWords[this.CurrentBadWordIndex].Word.Length - newWord.Length;
                this.MoveWordOffsets(-delta, this.CurrentBadWordIndex);                    
               

                this.BadWords.Remove(this.BadWords[this.CurrentBadWordIndex]);

                this._context.Text = newText;
                this._checker.CurrentSpellCheckBindingValue = newText; 

                this.UpdateDialog();
                
            }
            
        }

        internal void ChangeAll()
        {
            if (this.BadWords[this.CurrentBadWordIndex] != null && this._checker != null)
            {
                string newText = "";

                string newWord = this._changeTo.Text;
                int badWordIndex = this.CurrentBadWordIndex;
                int delta;
                BadWord currentBadWord = this.BadWords[this.CurrentBadWordIndex];

                newText += this._checker.CurrentSpellCheckBindingValue.Substring(0, currentBadWord.StartPosition);

                // We were doing a ReplaceAll before, which meant that if you did ChangeAll, and some other text in the string contained
                // that value inside of it, we replaced it as well. Which is bad. 
                // So now, just replace the current badword here. 
                int start = currentBadWord.StartPosition + currentBadWord.Word.Length;
                newText += newWord + this._checker.CurrentSpellCheckBindingValue.Substring(start, this._checker.CurrentSpellCheckBindingValue.Length - start);

                delta = currentBadWord.Word.Length;
                if (delta > newWord.Length)
                {
                    delta = newWord.Length;
                }

                foreach (BadWord bw in this.BadWords)
                {
                    if (bw.Word == this._checker.CurrentBadWord.Word)
                    {
                        this._ignoredWords.Add(bw);

                        if (bw != currentBadWord)
                        {
                            // Now, update every other badword that equals the new value
                            // We'll do this one word at a time. 
                            string newFullText = newText.Substring(0, bw.StartPosition);

                            start = bw.StartPosition + bw.Word.Length;
                            newFullText += newWord + newText.Substring(start, newText.Length - start);

                            newText = newFullText;
                        }

                        
                        if (bw.StartPosition + bw.Word.Length > newText.Length)
                        {

                            this.MoveWordOffsets(newWord.Length - newText.Substring(bw.StartPosition, delta).Length, badWordIndex);
                        }
                        else
                            this.MoveWordOffsets(newWord.Length - newText.Substring(bw.StartPosition, bw.Word.Length).Length, badWordIndex);
                    }
                    badWordIndex++;
                }

                foreach (BadWord bw in this._ignoredWords)
                {
                    if (bw.Word == this._checker.CurrentBadWord.Word)
                    {
                        this.BadWords.Remove(bw);
                    }
                }

                this._ignoredWords.Clear();

                this._context.Text = newText;
                this._checker.CurrentSpellCheckBindingValue = newText;              

                this.UpdateDialog();                

            }
        }

        internal void NextField()
        {
            //We only want to fire spellcheck completed if we're called by NextField directly, if we're called during update dialog
            //we should not fire the event, since update dialog will fire it on its own eventually.
            this.NextField(true);
        }

        internal void NextField(bool fireSpellCheckCompleteEvent)
        {
            if (this._checker != null)
            {
                this.CurrentBadWordIndex = 0;

                this._checker.SpellCheckTargetElements[this._checker.CurrentSpellCheckTargetIndex].SetBinding(TargetElement.ValueProperty, this._checker.SpellCheckTargets[this._checker.CurrentSpellCheckTargetIndex]);
                if (this._checker.CurrentSpellCheckTargetIndex++ < this._checker.SpellCheckTargetElements.Count - 1)
                {

                    object value = this._checker.SpellCheckTargetElements[this._checker.CurrentSpellCheckTargetIndex].Value ?? string.Empty;
                    this._context.Text = value.ToString();
                    this._spellCheckerEngine.Check(this._context.Text);
                    this.GetBadWords();

                    while (this.BadWords.Count <= 0 && this._checker.CurrentSpellCheckTargetIndex < this._checker.SpellCheckTargets.Count)
                    {
                        

                        this.NextField(false);
                    }

                    if (this.BadWords.Count > 0)
                    {
                        this._checker.CurrentBadWord = this.BadWords[0];
                        this.RefreshListbox(this._checker.CurrentBadWord.Suggestions);
                        this.ApplyBadWordContext(this._context);
                    }

                    else
                    {
                        this._checker.CurrentBadWord = null;
                        this._suggestions.ItemsSource = null;
                        this._suggestions.SelectedIndex = -1;
                    }
                }
                else
                {
                    if(fireSpellCheckCompleteEvent)
                        this._checker.OnSpellCheckCompleted(new SpellCheckCompletedEventArgs(null, false, null));
                    VisualStateManager.GoToState(this, "SpellCheckComplete", true);
                }
                XamSpellCheckerDialogWindow.RefreshCommands();
            }
        }

        internal void PreviousField()
        {
            if (this._checker != null)
            {
                this.CurrentBadWordIndex = 0;

                this._checker.SpellCheckTargetElements[this._checker.CurrentSpellCheckTargetIndex].SetBinding(TargetElement.ValueProperty, this._checker.SpellCheckTargets[this._checker.CurrentSpellCheckTargetIndex]);
                if (this._checker.CurrentSpellCheckTargetIndex-- > 0)
                {

                    object value =  this._checker.SpellCheckTargetElements[this._checker.CurrentSpellCheckTargetIndex].Value ?? string.Empty;
                    this._context.Text = value.ToString();
                    this._spellCheckerEngine.Check(this._context.Text);
                    this.GetBadWords();

                    while (this.BadWords.Count <= 0 && this._checker.CurrentSpellCheckTargetIndex >= 0 )
                    {
                        this.PreviousField();
                    }
                    if (this.BadWords.Count > 0)
                    {
                        this._checker.CurrentBadWord = this.BadWords[0];
                        this.RefreshListbox(this._checker.CurrentBadWord.Suggestions);
                        this.ApplyBadWordContext(this._context);
                    }

                    else
                    {
                        this._checker.CurrentBadWord = null;
                        this._suggestions.ItemsSource = null;
                        this._suggestions.SelectedIndex = -1;
                    }
                }
                else
                {
                    this._checker.CurrentSpellCheckTargetIndex = 0;
                    this.NextField();
                }
                XamSpellCheckerDialogWindow.RefreshCommands();
            }
        }

        #region ApplyBadWordContext

        /// <summary>
        /// Sets a TextBlock with the current bad word highlighted and with context.
        /// </summary>
        /// <param name="txt">The textbox that the context should be applied to.</param>
        private void ApplyBadWordContext(TextBlock txt)
        {
            if (txt == null || this.CurrentBadWordIndex >= this.BadWords.Count || this._checker == null)
                return;

            string context = this._checker.CurrentSpellCheckBindingValue;

            int start = Math.Max(this.BadWords[this.CurrentBadWordIndex].StartPosition - 100, 0);
            if (start != 0)
            {
                start = context.IndexOf(" ", start, StringComparison.Ordinal);
                if (start > this.BadWords[this.CurrentBadWordIndex].StartPosition || start == -1)
                {
                    start = this.BadWords[this.CurrentBadWordIndex].StartPosition;
                }
            }

            int end = Math.Min(this.BadWords[this.CurrentBadWordIndex].EndPosition + 100, context.Length);
            end = context.IndexOf(" ", end, StringComparison.Ordinal);
            if (end == -1)
            {
                end = context.Length;
            }

            txt.Inlines.Clear();
            if (start != 0)
            {
                txt.Inlines.Add("...");
            }       
            

            txt.Inlines.Add(context.Substring(start, this.BadWords[this.CurrentBadWordIndex].StartPosition - start));
            txt.Inlines.Add(new Run() { Text = this.BadWords[this.CurrentBadWordIndex].Word, Foreground = this._checker.DialogSettings.CurrentWordBrush });
            txt.Inlines.Add(context.Substring(this.BadWords[this.CurrentBadWordIndex].EndPosition, end - this.BadWords[this.CurrentBadWordIndex].EndPosition));
            if (end != context.Length)
            {
                txt.Inlines.Add("...");
            }

        }

        #endregion // ApplyBadWordContext


        #region UpdateDialog

        internal void UpdateDialog()
        {
            if (this._checker != null)
            {
                if (this.BadWords.Count > 0)
                {
                    this._checker.CurrentBadWord = this.BadWords[this.CurrentBadWordIndex];
                    this.RefreshListbox(this.BadWords[this.CurrentBadWordIndex].Suggestions);
                    this.ApplyBadWordContext(this._context);
                }
                else
                {
                    while (this.BadWords.Count <= 0 && this._checker.CurrentSpellCheckTargetIndex < this._checker.SpellCheckTargets.Count)
                    {
                        //We only want to fire spellcheck completed if we're called by NextField directly, if we're called during update dialog
                        //we should not fire the event, since update dialog will fire it on its own eventually.
                        this.NextField(false);
                    }

                    if (this.BadWords.Count > 0)
                    {
                        VisualStateManager.GoToState(this, "SpellCheckNormal", true);
                        this._checker.CurrentBadWord = this.BadWords[this.CurrentBadWordIndex];
                        this.RefreshListbox(this.BadWords[this.CurrentBadWordIndex].Suggestions);
                        this.ApplyBadWordContext(this._context);
                    }
                    else
                    {
                        this._checker.OnSpellCheckCompleted(new SpellCheckCompletedEventArgs(null, false, null));
                        VisualStateManager.GoToState(this, "SpellCheckComplete", true);
                        this._checker.CurrentBadWord = null;
                    }

                }

                if (this.Style != this._checker.DialogSettings.SpellCheckDialogStyle)
                {

                    // In WPF, this.Style gets set for the Implicit Style, so we have to add a null check as well.
                    if(this._checker.DialogSettings.SpellCheckDialogStyle != null)

                        this.Style = this._checker.DialogSettings.SpellCheckDialogStyle;
                }

                this.IsModal = this._checker.DialogSettings.Mode == SpellCheckingMode.ModalDialog ? true : false;

                CommandSourceManager.NotifyCanExecuteChanged(typeof(PreviousFieldCommand));
                CommandSourceManager.NotifyCanExecuteChanged(typeof(NextFieldCommand));
            }
        }
            
        #endregion //UpdateDialog

        #endregion //Commands

        #region  GetParameter
        /// <summary>
        /// Returns the object that defines the parameters necessary to execute the command.
        /// </summary>
        /// <param name="source">The CommandSource object which defines the command to be executed.</param>
        /// <returns>The object necessary for the command to complete.</returns>
        protected override object GetParameter(CommandSource source)
        {

            if (source.Command is XamSpellCheckerCommandBase)
                return this;

            return null;
        }
        #endregion // GetParameter

        #region SupportsCommand

        /// <summary>
        /// Returns if the object will support a given command type.
        /// </summary>
        /// <param name="command">The command to be validated.</param>
        /// <returns>True if the object recognizes the command as actionable against it.</returns>
        protected override bool SupportsCommand(ICommand command)
        {
            return (command is XamSpellCheckerCommandBase);
        }
        #endregion // SupportsCommand

        #region ICommandTarget Members

        bool ICommandTarget.SupportsCommand(ICommand command)
        {
            return this.SupportsCommand(command);
        }

        object ICommandTarget.GetParameter(CommandSource source)
        {
            return this.GetParameter(source);
        }

        #endregion

        #endregion //Commanding
    }    
}

#region Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved
/* ---------------------------------------------------------------------*
*                           Infragistics, Inc.                          *
*              Copyright (c) 2001-2012 All Rights reserved               *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* THE SOURCE CODE CONTAINED HEREIN AND IN RELATED FILES IS PROVIDED     *
* TO THE REGISTERED DEVELOPER FOR THE PURPOSES OF EDUCATION AND         *
* TROUBLESHOOTING. UNDER NO CIRCUMSTANCES MAY ANY PORTION OF THE SOURCE *
* CODE BE DISTRIBUTED, DISCLOSED OR OTHERWISE MADE AVAILABLE TO ANY     *
* THIRD PARTY WITHOUT THE EXPRESS WRITTEN CONSENT OF INFRAGISTICS, INC. *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY INFRAGISTICS PRODUCT.    *
*                                                                       *
* THE REGISTERED DEVELOPER ACKNOWLEDGES THAT THIS SOURCE CODE           *
* CONTAINS VALUABLE AND PROPRIETARY TRADE SECRETS OF INFRAGISTICS,      *
* INC.  THE REGISTERED DEVELOPER AGREES TO EXPEND EVERY EFFORT TO       *
* INSURE ITS CONFIDENTIALITY.                                           *
*                                                                       *
* THE END USER LICENSE AGREEMENT (EULA) ACCOMPANYING THE PRODUCT        *
* PERMITS THE REGISTERED DEVELOPER TO REDISTRIBUTE THE PRODUCT IN       *
* EXECUTABLE FORM ONLY IN SUPPORT OF APPLICATIONS WRITTEN USING         *
* THE PRODUCT.  IT DOES NOT PROVIDE ANY RIGHTS REGARDING THE            *
* SOURCE CODE CONTAINED HEREIN.                                         *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/
#endregion Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved