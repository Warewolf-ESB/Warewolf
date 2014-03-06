using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Specialized;
using System.Collections;
using System.Windows.Media;
using System.Windows.Markup;
using System.Windows.Controls;
using Infragistics.Controls.Editors;


using Infragistics.Windows.Licensing;


using System.Windows.Automation.Peers;
using System.Windows.Threading;
using System.Threading;
using Infragistics.Collections;
using Infragistics.Controls;
using Infragistics.Controls.Primitives;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Infragistics.Controls.Editors.Primitives;

namespace Infragistics.Controls.Editors.Primitives
{
	/// <summary>
	/// Used inside the template of the <see cref="XamMaskedInput"/>
	/// </summary>

	[DesignTimeVisible(false)]	// JJD 4/14/11 - DO NOT MOVE TO DESIGN ASSEMBLY!!!

	public class MaskedInputTextBox : EmbeddedTextBox
	{
		#region Member Vars

		private int _lastSelectionStart;
		private int _lastSelectionLength;
		private string _lastTextValue;
		private XamMaskedInput _editor;
		internal DateTime? _enteringEditModeViaMouseDown;

		// SSP 11/23/11 - IME
		// 
		private bool _justProcessedTextInput;

		#endregion // Member Vars

		#region Constructor

		static MaskedInputTextBox( )
		{
		}

		/// <summary>
		/// Constructor. Creates a new instance of <see cref="MaskedInputTextBox"/> class.
		/// </summary>
		public MaskedInputTextBox( )
		{
			// SSP 10/18/11 TFS90147
			// We decided to force the flow direction to left-to-right.
			// 
			this.FlowDirection = System.Windows.FlowDirection.LeftToRight;

			this.SelectionChanged += new RoutedEventHandler( OnSelectionChanged );
			this.TextChanged += new TextChangedEventHandler( OnTextChanged );
		} 

		#endregion // Constructor

		#region Properties

		#region Private Properties

		#region EditInfo

		private EditInfo EditInfo
		{
			get
			{
				return null != _editor ? _editor.EditInfo : null;
			}
		}

		#endregion // EditInfo  

		#endregion // Private Properties

		#endregion // Properties

		#region Methods

		#region Internal Methods

		#region Initialize

		internal void Initialize( XamMaskedInput editor )
		{
			_editor = editor;

			// AS 8/2/11
			// Initialize the InputScope based on the current sections.
			//
			this.OnSectionsChanged();
		}

		#endregion // Initialize

		#region OnSectionsChanged
		internal void OnSectionsChanged()
		{
			if (_editor == null)
				return;



#region Infragistics Source Cleanup (Region)














































































#endregion // Infragistics Source Cleanup (Region)

		}
		#endregion //OnSectionsChanged

		#region SetText
		internal void OnSyncComplete()
		{
			// snapshot out current state
			_lastTextValue = this.Text;
			_lastSelectionStart = this.SelectionStart;
			_lastSelectionLength = this.SelectionLength;
		}
		#endregion //SetText

		#endregion // Internal Methods

		#region Private Methods

		#region OnSelectionChanged

		private void OnSelectionChanged( object sender, RoutedEventArgs e )
		{
			if ( null != this.EditInfo )
				this.EditInfo.SyncWithIMETextBox( );

			// if something is changing the selection and not a result of a paste/cut
			// then update the cached selection start/length. if the text is different 
			// then the editinfo didn't do that or it will tell us to snapshot the info
			// when it is done
			if (string.Equals(this.Text, _lastTextValue))
				this.OnSyncComplete();
		}

		#endregion // OnSelectionChanged

		#region OnTextChanged
		private void OnTextChanged(object sender, TextChangedEventArgs e)
		{
			// SSP 12/1/11 TFS96629
			// 


#region Infragistics Source Cleanup (Region)




















#endregion // Infragistics Source Cleanup (Region)


			if (!string.Equals(_lastTextValue, this.Text))
			{
				// SSP 11/23/11 - IME
				// 
				if ( _justProcessedTextInput )
				{
					if ( null != this.EditInfo )
						this.EditInfo.SyncIMETextBox( );

					return;
				}

				// asynchronously process the text change since it could be the 
				// edit info or other information may be updated. i'm using the 
				// DSC because its Post is a Normal priority so its better than 
				// waiting for a background priority using a dispatcher invoke
				var syncContext = new DispatcherSynchronizationContext();
				syncContext.Post(new SendOrPostCallback(this.VerifyText), null);
			}
		} 
		#endregion //OnTextChanged

		// SSP 12/1/11 TFS96629
		// 


#region Infragistics Source Cleanup (Region)





























#endregion // Infragistics Source Cleanup (Region)


		#region VerifyText
		private void VerifyText(object param)
		{
			if (string.Equals(this.Text, _lastTextValue))
				return;


			// SSP 12/1/11 TFS96629
			// Moved this here from below.
			// 
			// try to figure out what happened
			string newText = this.Text ?? string.Empty;
			string oldText = _lastTextValue ?? string.Empty;
			int newLength = string.IsNullOrEmpty( newText ) ? 0 : newText.Length;
			int oldLength = string.IsNullOrEmpty( oldText ) ? 0 : oldText.Length;

			// SSP 12/1/11 TFS96629
			// 


#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)



			// SSP 11/22/11 - IME inputting
			// 
			//if (_editor != null)
			EditInfo editInfo = this.EditInfo;
			if ( null != editInfo && ! editInfo.IsTextInputInProgress )
			{
				// if we're out of edit mode then the text difference could be 
				// as a result of ending edit mode, etc.
				if (_editor.IsInEditMode)
				{
					// SSP 12/1/11 TFS96629
					// Moved this above.
					// 
					//// try to figure out what happened
					//string newText = this.Text ?? string.Empty;
					//string oldText = _lastTextValue ?? string.Empty;
					//int newLength = string.IsNullOrEmpty(newText) ? 0 : newText.Length;
					//int oldLength = string.IsNullOrEmpty(oldText) ? 0 : oldText.Length;

					if (_lastSelectionStart == 0 && _lastSelectionLength == oldLength)
					{
						// the entire text was selected so just overwrite it
						editInfo.SetText(newText);
					}
					else if (string.Equals(newText, oldText.Remove(_lastSelectionStart, _lastSelectionLength)))
					{
						// if the selection was removed/cut then just clear the selected text
						editInfo.Delete();
					}
					else if (newLength >= _lastSelectionStart)
					{
						// the strings have a similar start...
						if (_lastSelectionStart == 0 || string.Equals(newText.Substring(0, _lastSelectionStart), oldText.Substring(0, _lastSelectionStart)))
						{
							string oldEnd = oldText.Substring(_lastSelectionStart + _lastSelectionLength);
							int oldEndLength = string.IsNullOrEmpty(oldEnd) ? 0 : oldEnd.Length;

							// the strings have a similar end
							if (oldEndLength == 0 || newText.EndsWith(oldEnd))
							{
								string newSelected = newText.Substring(_lastSelectionStart, newLength - (_lastSelectionStart + oldEndLength));

								// restore the original state
								this.Text = _lastTextValue;
								this.SelectionStart = _lastSelectionStart;
								this.SelectionLength = _lastSelectionLength;

								// make sure the editinfo has the same selection info
								editInfo.SyncWithIMETextBox();

								// then replace the selection with the calculated replacement text
								editInfo.SelectedText = newSelected;
							}
						}
					}
				}

				// finally push what the edit info has
				editInfo.SyncIMETextBox();
			}
		} 
		#endregion //VerifyText

		#endregion // Private Methods 

		#endregion // Methods

		#region Base Overrides

		#region OnGotFocus

		// SSP 9/19/11 TFS87057
		// If the embedded text box recieves focus directly then we need to notify the editor
		// that has focus within.
		// 
		/// <summary>
		/// Called when the control recieves focus.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnGotFocus( RoutedEventArgs e )
		{
			base.OnGotFocus( e );

			_editor.OnIsFocusWithinChangedHelper( true );
		} 

		#endregion // OnGotFocus

		#region OnKeyDown

		/// <summary>
		/// Overridden. Called before KeyDown event is raised.
		/// </summary>
		/// <param name="e">Key event args.</param>
		protected override void OnKeyDown( KeyEventArgs e )
		{
			// If we get key event then reset the _lastMouseDown as we don't need to
			// prevent caret synchronization which is what it's used for.
			// 
			_enteringEditModeViaMouseDown = null;

			if ( null != _editor )
				_editor.ProcessKeyDown( e );

			if ( !e.Handled )
				base.OnKeyDown( e );

			if ( null != this.EditInfo )
				this.EditInfo.SyncIMETextBox( );
		}

		#endregion // OnKeyDown

		#region OnMouseDown



		/// <summary>
		/// Called when a mouse button is pressed down.
		/// </summary>
		protected override void OnMouseDown( MouseButtonEventArgs e )
		{
			bool isBeingEditedAndFocused = _editor.MaskInfo.IsBeingEditedAndFocused;
			if ( ! isBeingEditedAndFocused )
				_enteringEditModeViaMouseDown = DateTime.Now;

			try
			{
				base.OnMouseDown( e );
			}
			finally
			{
				_enteringEditModeViaMouseDown = null;
			}
		}

		// SSP 9/19/11 TFS87057
		// Apparently OnIsKeyboardFocusedChanged on the editor doesn't get called when calendar is 
		// closed up in XamDateTimeInput.
		// 
		/// <summary>
		/// Called when the keyboard focus shifts into or out of the visual tree of this element.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnIsKeyboardFocusedChanged( DependencyPropertyChangedEventArgs e )
		{
			base.OnIsKeyboardFocusedChanged( e );

			_editor.OnIsFocusWithinChangedHelper( (bool)e.NewValue );
		}



		#endregion // OnMouseDown

		#region OnMouseLeftButtonDown



#region Infragistics Source Cleanup (Region)

















#endregion // Infragistics Source Cleanup (Region)


		#endregion // OnMouseLeftButtonDown

		#region OnPreviewKeyDown



		/// <summary>
		/// Overridden. Called to preview key down event.
		/// </summary>
		/// <param name="e">Key event args.</param>
		protected override void OnPreviewKeyDown( KeyEventArgs e )
		{
			bool hasCommand = false;

			switch ( e.Key )
			{
				case Key.Left:
				case Key.Right:
				case Key.Home:
				case Key.End:
					hasCommand = true;
					break;
			}

			// SSP 5/11/12 TFS99806
			// Apparently the base class ends up handling Ctrl+Z and we don't get OnKeyDown so we have to
			// handle the OnPreviewKey.
			// 
			if ( !hasCommand && null != e.KeyboardDevice )
				hasCommand = CoreUtilities.HasItems( _editor.GetMatchingCommands( e.Key, e.KeyboardDevice.Modifiers ) );

			if ( hasCommand )
			{
				this.OnKeyDown( e );
				if ( e.Handled )
					return;
			}

			base.OnPreviewKeyDown( e );
		}



		#endregion // OnPreviewKeyDown

		#region OnTextInput

		/// <summary>
		/// Overridden. Called when there is text input in the text box.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnTextInput( TextCompositionEventArgs e )
		{
			if ( null != _editor )
			{
				if ( null != this.EditInfo )
					this.EditInfo.ImeTextBox_TextInput( this, e, null );

				// SSP 11/23/11 - IME
				// This issue only happens in silverlight
				// 


#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)

			}
		}

		#endregion // OnTextInput



#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)


		#endregion // Base Overrides
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