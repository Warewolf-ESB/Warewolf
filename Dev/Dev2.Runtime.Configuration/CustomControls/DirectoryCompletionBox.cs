
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Windows;
using System.Windows.Controls;
using Dev2.Runtime.Configuration.ComponentModel;

namespace Dev2.Runtime.Configuration.CustomControls
{
    public class DirectoryCompletionBox : AutoCompleteBox
    {

        #region public string Text
        /// <summary>
        /// Gets or sets the text in the text box portion of the
        /// <see cref="T:System.Windows.Controls.AutoCompleteBox" /> control.
        /// </summary>
        /// <value>The text in the text box portion of the
        /// <see cref="T:System.Windows.Controls.AutoCompleteBox" /> control.</value>
        public ComputerDrive CurrentDrive
        {
            get { return GetValue(CurrentDriveProperty) as ComputerDrive; }
            set { SetValue(CurrentDriveProperty, value); }
        }

        /// <summary>
        /// Identifies the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.Text" />
        /// dependency property.
        /// </summary>
        /// <value>The identifier for the
        /// <see cref="P:System.Windows.Controls.AutoCompleteBox.Text" />
        /// dependency property.</value>
        public static readonly DependencyProperty CurrentDriveProperty =
            DependencyProperty.Register(
                "CurrentDrive",
                typeof(ComputerDrive),
                typeof(DirectoryCompletionBox),
                new PropertyMetadata(null, OnCurrentDrivePropertyChanged));

        /// <summary>
        /// TextProperty property changed handler.
        /// </summary>
        /// <param name="d">AutoCompleteBox that changed its Text.</param>
        /// <param name="e">Event arguments.</param>
        private static void OnCurrentDrivePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var source = d as DirectoryCompletionBox;
            if(source != null) source.DirectoryUpdated((ComputerDrive)e.NewValue);
        }

        /// <summary>
        /// Handle the update of the text for the control from any source,
        /// including the TextBox part and the Text dependency property.
        /// </summary>
        /// <param name="newDirectory">The new directory.</param>
        private void DirectoryUpdated(ComputerDrive newDirectory)
        {
            // Update the interface and values only as necessary
            UpdateDirectoryValue();
        }

        private void UpdateDirectoryValue()
        {

        }

        #endregion public string Text

        static DirectoryCompletionBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DirectoryCompletionBox), new FrameworkPropertyMetadata(typeof(DirectoryCompletionBox)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if(TextBox != null)
            {
                TextBox.GotFocus += TextBoxOnGotFocus;
            }
        }

        private void TextBoxOnGotFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            IsDropDownOpen = true;
            routedEventArgs.Handled = true;
        }
    }
}
