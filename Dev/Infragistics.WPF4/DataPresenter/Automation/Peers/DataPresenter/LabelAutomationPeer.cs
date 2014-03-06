using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Windows.DataPresenter;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Automation;
using System.Windows;
using System.ComponentModel;
using Infragistics.AutomationPeers;

namespace Infragistics.Windows.Automation.Peers.DataPresenter
{
	/// <summary>
	/// Exposes the label of a particular <see cref="Field"/> to UI Automation
	/// </summary>
	public class LabelAutomationPeer : AutomationPeerProxy,
		// AS 1/19/10 TFS26545
		//IInvokeProvider,
		ILabelPeer,
		IWeakEventListener
	{
		#region Member Variables

		private Field _field;
		private HeaderAutomationPeer _headerAutomationPeer;
        // AS 2/3/09 Remove unused members
		//private bool _isVisibleInCellArea;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="LabelAutomationPeer"/>
		/// </summary>
		/// <param name="field">Field that this header represents</param>
		/// <param name="headerAutomationPeer">The header automation peer that contains the automation peer</param>
		public LabelAutomationPeer(Field field, HeaderAutomationPeer headerAutomationPeer)
		{
			if (field == null)
				throw new ArgumentNullException("field");

			if (headerAutomationPeer == null)
				throw new ArgumentNullException("headerAutomationPeer");

			this._field = field;
			this._headerAutomationPeer = headerAutomationPeer;

			// listen for property changes so we know when a field was made visible/hidden
			PropertyChangedEventManager.AddListener(field, this, string.Empty);

            // AS 2/3/09 Remove unused members
            //this._isVisibleInCellArea = field.IsVisibleInCellArea;
		} 
		#endregion //Constructor

		#region Base class overrides

		#region GetChildrenCore
		/// <summary>
		/// Returns the collection of child elements of the <see cref="LabelPresenter"/> that is associated with this <see cref="LabelAutomationPeer"/>
		/// </summary>
		/// <returns>The collection of child elements</returns>
		protected override List<AutomationPeer> GetChildrenCore()
		{
			// AS 5/31/11 TFS76934
			AutomationPeerHelper.RemovePendingChildrenInvalidation(this);

			return base.GetChildrenCore();
		}
		#endregion //GetChildrenCore

		#region GetUnderlyingPeer
		/// <summary>
		/// Returns the automation peer for which this proxy is associated.
		/// </summary>
		/// <returns>A <see cref="LabelPresenterAutomationPeer"/></returns>
		protected override System.Windows.Automation.Peers.AutomationPeer GetUnderlyingPeer()
		{
			// AS 1/19/10 TFS26545
			// Moved impl to get the LabelPresenter to a helper method.
			//
			//HeaderPresenter header = this._headerAutomationPeer.GetHeaderElement();
			//
			//if (null != header)
			//{
			//    Utilities.DependencyObjectSearchCallback<LabelPresenter> callback = new Utilities.DependencyObjectSearchCallback<LabelPresenter>(delegate(LabelPresenter element)
			//    {
			//        return element.Field == this._field;
			//    });
			//
			//    // AS 9/1/09 Optimization
			//    // Added stopAtType parameter to call to avoid traversing into a 
			//    // labelpresenter.
			//    //
			//    LabelPresenter label = Utilities.GetDescendantFromType<LabelPresenter>(header, true, callback, new Type[] { typeof(LabelPresenter) } );
			//
			//    if (null != label)
			//    {
			//        // JM 09-10-09 TFS21947
			//        //return UIElementAutomationPeer.CreatePeerForElement(label);
			//        AutomationPeer peer = UIElementAutomationPeer.CreatePeerForElement(label);
			//        if (null != peer)
			//            peer.EventsSource = this;
			//
			//        return peer;
			//    }
			//}
			//
			//return null;
			LabelPresenter label = this.GetLabelPresenter();

			if (null == label)
				return null;

			// JM 09-10-09 TFS21947
			//return UIElementAutomationPeer.CreatePeerForElement(label);
			AutomationPeer peer = UIElementAutomationPeer.CreatePeerForElement(label);
			if (null != peer)
				peer.EventsSource = this;

			return peer;
		} 
		#endregion //GetUnderlyingPeer

		#region GetAutomationControlTypeCore
		/// <summary>
		/// Returns an enumeration indicating the type of control represented by the automation peer.
		/// </summary>
		/// <returns>The <b>HeaderItem</b> enumeration value</returns>
		protected override System.Windows.Automation.Peers.AutomationControlType GetAutomationControlTypeCore()
		{
			return System.Windows.Automation.Peers.AutomationControlType.HeaderItem;
		} 
		#endregion //GetAutomationControlTypeCore

		#region GetClassNameCore
		/// <summary>
		/// Returns the name of the class
		/// </summary>
		/// <returns>A string that contains 'Label'</returns>
		protected override string GetClassNameCore()
		{
			return "Label";
		} 
		#endregion //GetClassNameCore

		// AS 8/28/09 TFS21509
		// When we cannot get to a labelpresenter we're not returning any meaningful text. We should 
		// at least return the text representation of the label.
		//
		#region GetNameCore
		/// <summary>
		/// Returns the text label for the <see cref="UIElement"/> that corresponds with the object that is associated with this <see cref="AutomationPeer"/>.
		/// </summary>
		/// <returns>The text label</returns>
		protected override string GetNameCore()
		{
			string name = base.GetNameCore();
			
			if (string.IsNullOrEmpty(name))
			{
				name = ClipboardOperationInfo.GetText(this._field.Label);
			}

			return name;
		}
		#endregion //GetNameCore
	
		#region GetPattern
		/// <summary>
		/// Returns the control pattern associated with the specified <see cref="PatternInterface"/> for the element that is associated with this <see cref="LabelAutomationPeer"/>.
		/// </summary>
		/// <param name="patternInterface">The pattern being requested</param>
		public override object GetPattern(PatternInterface patternInterface)
		{
			if (patternInterface == PatternInterface.Invoke)
			{
				// AS 1/19/10 TFS26545
				//return this;
				return this.PatternProvider;
			}

			return null;
		}
		#endregion //GetPattern

		#endregion //Base class overrides

		#region Properties

		// AS 1/19/10 TFS26545
		#region PatternProvider
		private LabelPatternProvider _patternProvider;

		private LabelPatternProvider PatternProvider
		{
			get
			{
				if (_patternProvider == null)
					_patternProvider = new LabelPatternProvider(this);

				return _patternProvider;
			}
		}
		#endregion //PatternProvider

		#endregion //Properties

		#region Methods

		// AS 1/19/10 TFS26545
		// Moved here from the GetUnderlyingPeer method.
		//
		#region GetLabelPresenter
		private LabelPresenter GetLabelPresenter()
		{
			HeaderPresenter header = this._headerAutomationPeer.GetHeaderElement();

			if (null != header)
			{
				Utilities.DependencyObjectSearchCallback<LabelPresenter> callback = new Utilities.DependencyObjectSearchCallback<LabelPresenter>(delegate(LabelPresenter element)
				{
					return element.Field == this._field;
				});

				// AS 9/1/09 Optimization
				// Added stopAtType parameter to call to avoid traversing into a 
				// labelpresenter.
				//
				return Utilities.GetDescendantFromType<LabelPresenter>(header, true, callback, new Type[] { typeof(LabelPresenter) });
			}

			return null;
		}
		#endregion //GetLabelPresenter

		#endregion //Methods

		#region IInvokeProvider

		// AS 1/19/10 TFS26545
		// Moved to a helper class.
		//
		//void IInvokeProvider.Invoke()
		//{
		//    if (false == this.IsEnabled())
		//        throw new ElementNotEnabledException();
		//
		//    this._field.PerformLabelClickAction();
		//}

		#endregion //IInvokeProvider

		#region IWeakEventListener

		bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
		{
			if (managerType == typeof(PropertyChangedEventManager))
			{
				PropertyChangedEventArgs propArgs = e as PropertyChangedEventArgs;

				switch (propArgs.PropertyName)
				{
					case "IsExpandable":
					case "Visibility":
						this._headerAutomationPeer.OnVisibleFieldsChanged();
						break;
				}

				return true;
			}

			return false;
		}

		#endregion // IWeakEventListener

		// AS 1/19/10 TFS26545
		#region ILabelPeer
		Field ILabelPeer.Field
		{
			get { return this._field; }
		}

		LabelPresenter ILabelPeer.Label
		{
			get { return this.GetLabelPresenter(); }
		}
		#endregion //ILabelPeer
	}

	// AS 1/19/10 TFS26545
	#region ILabelPeer interface
	internal interface ILabelPeer
	{
		bool IsEnabled();
		Field Field { get; }
		LabelPresenter Label { get; }
	}
	#endregion //ILabelPeer interface

	// AS 1/19/10 TFS26545
	#region LabelPatternProvider class
	internal class LabelPatternProvider : IInvokeProvider
	{
		#region Member Variables

		private ILabelPeer _labelPeer;

		#endregion //Member Variables

		#region Constructor
		internal LabelPatternProvider(ILabelPeer labelPeer)
		{
			_labelPeer = labelPeer;
		}
		#endregion //Constructor

		#region IInvokeProvider

		void IInvokeProvider.Invoke()
		{
			if (false == _labelPeer.IsEnabled())
				throw new ElementNotEnabledException();

			Field field = _labelPeer.Field;

			if (field == null)
				throw new ElementNotEnabledException();

			field.PerformLabelClickAction();
		}

		#endregion //IInvokeProvider
	}
	#endregion //LabelPatternProvider class
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