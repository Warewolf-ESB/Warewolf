using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Automation.Peers;
using Infragistics.Windows.Editors;
using System.Windows.Automation;

namespace Infragistics.Windows.Automation.Peers.Editors
{
	/// <summary>
	/// Exposes a <see cref="SectionsList"/> to UI Automation
	/// </summary>
	public class SectionsListAutomationPeer : FrameworkElementAutomationPeer
	{
		#region Constructor
		/// <summary>
		/// Creates a new instance of the <see cref="ValueEditorAutomationPeer"/> class
		/// </summary>
		/// <param name="owner">The <see cref="ValueEditor"/> for which the peer is being created</param>
		public SectionsListAutomationPeer(SectionsList owner)
			: base(owner)
		{
		}
		#endregion //Constructor

		#region Base class overrides

		/// <summary>
		/// Returns an enumeration indicating the type of control represented by the automation peer.
		/// </summary>
		/// <returns>The <b>Edit</b> enumeration value</returns>
		protected override AutomationControlType GetAutomationControlTypeCore()
		{
			return AutomationControlType.Edit;
		}

		#region GetClassNameCore
		/// <summary>
		/// Returns the name of the <see cref="SectionsList"/> class
		/// </summary>
		/// <returns>A string that contains 'SectionsList'</returns>
		protected override string GetClassNameCore()
		{
			return "SectionsList";
		}

		#endregion //GetClassNameCore

		#region GetNameCore

		/// <summary>
		/// Returns the name of the asscoiated element.
		/// </summary>
		/// <returns>A string that contains the name of the associated element</returns>
		protected override string GetNameCore()
		{
			string name = base.GetNameCore();
			if (string.IsNullOrEmpty(name))
			{
				// return the name of the editor peer
				AutomationPeer editorPeer = this.EditorPeer;

				if ( editorPeer != null )
					return editorPeer.GetName();
			}

			return (name ?? string.Empty);
		}

		#endregion //GetNameCore	
    
		#region GetPattern
		/// <summary>
		/// Returns the control pattern associated with the specified <see cref="PatternInterface"/> for the element that is associated with this <see cref="LabelAutomationPeer"/>.
		/// </summary>
		/// <param name="patternInterface">The pattern being requested</param>
		public override object GetPattern(PatternInterface patternInterface)
		{
			if (patternInterface == PatternInterface.Text ||
				 patternInterface == PatternInterface.Value)
			{
				AutomationPeer editorPeer = this.EditorPeer;

				if (editorPeer != null)
					return editorPeer.GetPattern(patternInterface);
			}

			return base.GetPattern(patternInterface);
		}

		#endregion //GetPattern
    
		#endregion //Base class overrides

		#region Properties

		#region Private Properties

		#region EditorPeer

		private AutomationPeer EditorPeer
		{
			get
			{
				SectionsList slist = this.Owner as SectionsList;

				ValueEditor editor = slist.TemplatedParent as ValueEditor;

				if (editor != null)
				{
					AutomationPeer editorPeer = editor != null
						? FrameworkElementAutomationPeer.FromElement(editor)
						: null;

					return editorPeer;
				}

				return null;
			}
		}

		#endregion //EditorPeer

		#endregion //Private Properties	
    
		#endregion //Properties
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