using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Windows.Design;
using Microsoft.Windows.Design.Interaction;
using Microsoft.Windows.Design.Model;
using System.Diagnostics;
using System.Windows.Controls.Primitives;

namespace Infragistics.Windows.Design.SmartTagFramework
{
    /// <summary>
    /// The GenericAdornerProvider class implements an adorner provider for the 
    /// adorned control. The adorner is a edit control, which changes some properties of the adorned control.
    /// </summary>
    public abstract class GenericAdornerProvider : PrimarySelectionAdornerProvider
    {
		#region Member Variables

        private readonly double			_maximizeDuration = 450;
		private readonly double			_minimizeDuration = 200;

        private GenericAdorner			_genericAdorner;
        private SmartButtonAdorner		_smartButtonAdorner;

        private AdornerPanel			_adornerPanel;
        private ModelItem				_adornedControlModel;
        private EditingContext			_context;
        private DesignerActionList		_customDesingerActionList;

        private ResourceDictionary		_customPropertyEditors;

		// JM 01-17-11 TFS32484, TFS33069 
		private Popup					_genericAdornerPopup;
		private double					_genericAdornerPopupLastVerticalOffset;
		private double					_genericAdornerPopupLastHorizontalOffset;


		#endregion //Member Variables

		#region Constructors

			#region Public Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="GenericAdornerProvider"/>
        /// </summary>
        public GenericAdornerProvider()
        {
            _smartButtonAdorner					= new SmartButtonAdorner();
            _adornerPanel						= new AdornerPanel();
            _adornerPanel.IsContentFocusable	= true;

            
            CommandBinding DeleteCmdBinding		= new CommandBinding(ApplicationCommands.Delete, OpenCmdExecuted, OpenCmdCanExecute);
            _adornerPanel.CommandBindings.Add(DeleteCmdBinding);

            
            RoutedCommand	tabRoutedCommand	= new RoutedCommand();
            KeyGesture		keyGesture			= new KeyGesture(Key.Tab);
            tabRoutedCommand.InputGestures.Add(keyGesture);

            CommandBinding	TabCmdBinding		= new CommandBinding(tabRoutedCommand, OpenCmdExecuted, OpenCmdCanExecute);
            _adornerPanel.CommandBindings.Add(TabCmdBinding);

            _smartButtonAdorner.tbtnSmart.Click += new RoutedEventHandler(tbtnSmart_Click);

            // Set the command targets
			//
			// ExecuteDesignerActionMethodItemCommand
            CommandBinding commandBinding = new CommandBinding(SmartTagFrameworkCommands.ExecuteDesignerActionMethodItemCommand, actionCmd_Executed, actionCmd_CanExecute);
            CommandManager.RegisterClassCommandBinding(typeof(GenericAdorner), commandBinding);
		}

			#endregion //Public Constructors

		#endregion //Constructors

        #region Properties

			#region Protected Properties

				#region CustomPropertyEditors

		/// <summary>
		/// Returns/sets a resourceDictionary that contains custom property editors.
		/// </summary>
		protected ResourceDictionary CustomPropertyEditors
		{
			get { return this._customPropertyEditors; }
			set { this._customPropertyEditors = value; }
		}

				#endregion //CustomPropertyEditors

			#endregion //Protected Properties

			#region Abstract Properties

		/// <summary>
		/// Returns an array of the item types that are supported by the Adorner.
		/// </summary>
		protected abstract Type [] SupportedItemTypes
		{
			get;
		}

			#endregion //Abstract Properties

			#region Public Properties

				#region CustomActionList

		/// <summary>
        /// Returns predefined DesignerActionList
        /// </summary>        
        public DesignerActionList CustomActionList
        {
            get
            {
                if (null == this._customDesingerActionList)
                {
                    Type designerActionListType = this.GetDesignerActionListTypeFromControlType(this._adornedControlModel.ItemType);
                    if (designerActionListType != null)
                    {
                        object[] args = new object[] { this._context, this._adornedControlModel, null, string.Empty };
                        _customDesingerActionList = (DesignerActionList)Activator.CreateInstance(designerActionListType, args);
                    }
                    else
                    {
                        throw new Exception(string.Format(DesignResources.LST_SmartTagFramework_GenericContextMenuProvider_GetCustomActionListMSG, this._adornedControlModel.ItemType.Name));
                    }
                }
                return _customDesingerActionList;
            }
        }

				#endregion //CustomActionList

			#endregion //Public Properties

		#endregion //Properties

		#region Methods

			#region Public Methods

				#region GetDesignerActionListTypeFromControlType

        /// <summary>
        /// An abstract method wich is used for association between predefined DesignerActionList and an Infragistics control
        /// </summary>
        /// <param name="controlType">The control's type</param>
        /// <returns>Predefined DesignerActionList type</returns>
        public abstract Type GetDesignerActionListTypeFromControlType(Type controlType);

				#endregion //GetDesignerActionListTypeFromControlType

			#endregion //Public Methods

			#region Private Methods

				#region InitializeGenericAdorner
		private void InitializeGenericAdorner()
		{
			if (_genericAdorner != null)
				return;

			// JM 01-17-11 TFS32484, TFS33069 
			this._genericAdornerPopup					= new Popup();
			this._genericAdornerPopup.PlacementTarget	= this._smartButtonAdorner;
			this._genericAdornerPopup.HorizontalOffset	= this._genericAdornerPopupLastHorizontalOffset;
			this._genericAdornerPopup.VerticalOffset	= this._genericAdornerPopupLastVerticalOffset;
			
			this._genericAdorner						= new GenericAdorner(this.CustomActionList, this._adornedControlModel, this.Context, this._genericAdornerPopup);
			this._genericAdornerPopup.Child				= this._genericAdorner;

			if (this._customPropertyEditors != null)
				_genericAdorner.Resources.MergedDictionaries.Add(this._customPropertyEditors);

			this._genericAdorner.CloseGenericAdorner	+= new RoutedEventHandler(this.genericAdorner_ClosePopup);
		}
				#endregion //InitializeGenericAdorner

				#region IsItemTypeSupported
		private bool IsItemTypeSupported(Type itemType)
		{
			foreach (Type type in this.SupportedItemTypes)
			{
				if (type == itemType)
					return true;
			}

			return false;
		}
				#endregion //IsItemTypeSupported

				#region RemoveGenericAdorner
		private void RemoveGenericAdorner()
		{
			if (this._genericAdorner != null)
			{
				this._genericAdorner.CloseGenericAdorner -= new RoutedEventHandler(this.genericAdorner_ClosePopup);

				if (this._adornerPanel != null)
				{
					if (this._adornerPanel.Children.Contains(_genericAdornerPopup))
						_adornerPanel.Children.Remove(_genericAdornerPopup);
				}

				// JM 01-17-11 TFS32484, TFS33069 
				this._genericAdornerPopup.IsOpen				= false;
				this._genericAdornerPopup.Child					= null;
				this._genericAdornerPopupLastHorizontalOffset	= _genericAdornerPopup.HorizontalOffset;
				this._genericAdornerPopupLastVerticalOffset		= _genericAdornerPopup.VerticalOffset;
				this._genericAdornerPopup						= null;
				this._genericAdorner							= null;
			}
		}
				#endregion //RemoveGenericAdorner

				#region Maximize

		private void Maximize()
        {
			this.InitializeGenericAdorner();

			// JM 01-17-11 TFS32484, TFS33069 
			this._genericAdornerPopup.IsOpen			= true;
			this._genericAdorner.InitializeKeyboardFocus();
			this._genericAdornerPopup.PopupAnimation	= PopupAnimation.Fade;
		}

				#endregion //Maximize

				#region Minimize

        private void Minimize()
        {
			// JM 01-17-11 TFS32484, TFS33069 
			this.RemoveGenericAdorner();
		}

				#endregion //Minimize

			#endregion //Private Methods

        #endregion //Methods

		#region Base Class Overrides

			#region Activate

        /// <summary>
        /// The following method is called when the adorner is activated.
        /// It creates the adorner control, sets up the adorner panel,
        /// and attaches a ModelItem to the adorned control.
        /// </summary>
        /// <param name="item">A Microsoft.Windows.Design.Model.ModelItem representing the adorned element.</param>
        protected override void Activate(ModelItem item)
        {
			Debug.Assert(this.SupportedItemTypes != null && this.SupportedItemTypes.Length > 0, "SupportedItemTypes not supplied in AdornerProvider!!  Override GenericAdornerprovider.SupportedItemTypes to provide an array of supported item Types");
			if (false == this.IsItemTypeSupported(item.ItemType))
				return;

            #region Intialization

            // Save the ModelItem and hook into when it changes.            
            _adornedControlModel	= item;
            _context				= this.Context;

            #endregion //Intialization

            #region Positioning

            AdornerPlacementCollection smartBtnPlacement		= new AdornerPlacementCollection();

            //The adorner has the same size as its content
            smartBtnPlacement.SizeRelativeToAdornerDesiredHeight(1.0, 0);
            smartBtnPlacement.SizeRelativeToAdornerDesiredWidth(1.0, 0);

            //Set x = panel's width
            smartBtnPlacement.PositionRelativeToContentHeight(0.0, 0);
            smartBtnPlacement.PositionRelativeToContentWidth(1.0, 0);

            //Set y = adorner's height, x = adorner's width
            smartBtnPlacement.PositionRelativeToAdornerHeight(-0.2, -2);
            smartBtnPlacement.PositionRelativeToAdornerWidth(-1.0, -2);

            AdornerPanel.SetPlacements(_smartButtonAdorner, smartBtnPlacement);

            AdornerProperties.SetOrder(_adornerPanel, AdornerOrder.Foreground);

            #endregion //Positioning

            _adornerPanel.Children.Add(_smartButtonAdorner);
            this.Adorners.Add(_adornerPanel);
            base.Activate(item);
        }

			#endregion //Activate

			#region Deactivate

        /// <summary>
        /// Called when an adorner provider is about to be discarded by the designer.
        /// </summary>
        protected override void Deactivate()
        {
			if (this._adornerPanel != null)
				_adornerPanel.Children.Remove(_smartButtonAdorner);

			this.RemoveGenericAdorner();

            this.Adorners.Clear();

            base.Deactivate();
        }

			#endregion //Deactivate

		#endregion //Base Class Overrides

		#region Events

			#region ExecutedCommand

        private void OpenCmdExecuted(object sender, ExecutedRoutedEventArgs e)
        {
        }

        private void OpenCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;
        }

			#endregion //ExecutedCommand

		#endregion //Events

		#region Event Handlers

			#region actionCmd_Executed

        private static void actionCmd_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            GenericAdorner genericAdorner = sender as GenericAdorner;
            foreach (DesignerActionMethodItem item in genericAdorner.CurrentActionList.MethodItems)
            {
                if (item.DisplayName.Equals(e.Parameter.ToString()))
                {
                    item.Invoke();
                    break;
                }
            }
        }

			#endregion //actionCmd_Executed

			#region actionCmd_CanExecute

        private static void actionCmd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

			#endregion //actionCmd_CanExecute

			#region tbtnSmart_Click

		private void tbtnSmart_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)_smartButtonAdorner.tbtnSmart.IsChecked)
                Maximize();
            else
                Minimize();
        }

			#endregion //tbtnSmart_Click

			#region genericAdorner_ClosePopup

        private void genericAdorner_ClosePopup(object sender, RoutedEventArgs e)
        {
            _smartButtonAdorner.tbtnSmart.IsChecked = false;
            Minimize();
        }

			#endregion //genericAdorner_ClosePopup

		#endregion //Event Handlers
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