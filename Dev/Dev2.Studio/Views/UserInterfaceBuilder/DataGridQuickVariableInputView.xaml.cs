#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Windows.Controls;


namespace Dev2.Studio.Views.UserInterfaceBuilder
{
    /// <summary>
    /// Interaction logic for DataGridQuickVariableInputView.xaml
    /// </summary>
    public partial class DataGridQuickVariableInputView : IDisposable
    {
        // Track whether Dispose has been called.
        bool _disposed;

        public DataGridQuickVariableInputView()
        {
            InitializeComponent();
        }

        // Implement IDisposable.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if(!_disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if(disposing)
                {
                    // Dispose managed resources.

                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.


                // Note disposing has been done.
                _disposed = true;

            }
        }

        // This destructor will run only if the Dispose method
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~DataGridQuickVariableInputView()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(SplitTypeCbx.SelectedValue != null)
            {
                var val = SplitTypeCbx.SelectedValue.ToString();
                if (val == "Index" || val == "Chars")
                {
                    SplitTokenTxt.IsEnabled = true;
                }
                else
                {
                    SplitTokenTxt.Text = string.Empty;
                    SplitTokenTxt.IsEnabled = false;
                }
            }

        }

        void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                if (string.IsNullOrWhiteSpace(TxtVariableList.Text))
                {
                    QuickVariableInputAddBtn.IsEnabled = false;
                    QuickVariableInputPreviewBtn.IsEnabled = false;
                }
                else
                {
                    QuickVariableInputAddBtn.IsEnabled = true;
                    QuickVariableInputPreviewBtn.IsEnabled = true;
                }
            }
        }
    }
}
