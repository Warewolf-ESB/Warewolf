using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Shapes;
using System.Collections.Generic;
using System;
using System.Windows.Media;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Media.Effects;
using Infragistics.Controls.Interactions;

// MD 6/14/11
// Fixed build errors after moving schedules DialogManager to the Windows shared assembly.
//namespace Infragistics
//{
//    internal class DialogManager
//    {
namespace Infragistics.Controls.Interactions
{
	public partial class XamDialogWindow 
	{
    private class DialogManager
    {
		#region Static

		private static DialogManager _manager = new DialogManager();

		/// <summary>
		/// A global Manager that manages all <see cref="XamDialogWindow"/> objects for an application.
		/// </summary>
		public static DialogManager Manager
		{
			get { return _manager; }
		}

		#endregion // Static

		#region Members





        Effect _rootVisualEffect;
		WeakReferenceHelper<XamDialogWindow> _modalDialogs, _dialogs;
		Dictionary<FrameworkElement, WeakReferenceHelper<XamDialogWindow>> _containerDialogs;
		Popup _contextPopup;
		WeakReference _activeDialog;

		#endregion // Members

		#region Constructor

		public DialogManager()
        {




            this._modalDialogs = new WeakReferenceHelper<XamDialogWindow>();
			this._dialogs = new WeakReferenceHelper<XamDialogWindow>();
            






            if (Application.Current != null)
            {
                
                UIElement element = PlatformProxy.GetRootVisual(null);
                if (element != null)
                {
                    element.LayoutUpdated += new EventHandler(Element_LayoutUpdated);
                }
            }

            

			this._containerDialogs = new Dictionary<FrameworkElement, WeakReferenceHelper<XamDialogWindow>>();



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		}


        void Element_LayoutUpdated(object sender, EventArgs e)
        {
            foreach(WeakReference wr in this._modalDialogs.Values)
            {
                if (wr.IsAlive)
                {
                    
                    XamDialogWindow dialog = wr.Target as XamDialogWindow;
                    if (dialog != null)
                    {
                        FrameworkElement container = XamDialogWindow.ResolveContainer(dialog);
                        FrameworkElement rootVisual = PlatformProxy.GetRootVisual(dialog) as FrameworkElement;
                        if (container != null)
                        {
                            dialog.ModalLayer.Height = container.ActualHeight;
                            dialog.ModalLayer.Width = container.ActualWidth;
                        }
                    }
                }
            }
        }


		#endregion // Constructor

		#region Methods

		#region Public

		public void SetActiveDialogWindow(XamDialogWindow dialog)
		{
			if (dialog == null)
				this._activeDialog = null;
			else
				this._activeDialog = new WeakReference(dialog);
		}

		public XamDialogWindow GetActiveDialogWindow()
		{
			if (this._activeDialog == null || !this._activeDialog.IsAlive)
				return null;
			else
				return (XamDialogWindow)this._activeDialog.Target;
		}		

		public void BringToFront(XamDialogWindow frontDialog)
		{
			if (this._dialogs.Contains(frontDialog))
			{
				this._dialogs.Remove(frontDialog);
				this._dialogs.Add(frontDialog);
				int zIndex = 0;



#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)


				int count = this._dialogs.Count;

				if (this._modalDialogs.Count == 0)
				{
					for (int i = 0; i < count; i++)
					{
						XamDialogWindow dialog = this._dialogs[i];
						if (dialog != null)
							Canvas.SetZIndex(dialog.RootElement, zIndex += 2);
					}
				}
				else
				{
					if(this._modalDialogs.Contains(frontDialog))
					{
						for (int i= 0; i < count; i++)
						{
							XamDialogWindow dialog = this._dialogs[i];
                            if (dialog != null)
                            {
                                Canvas.SetZIndex(dialog.RootElement, zIndex += 2);

                                Canvas.SetZIndex(dialog.ModalLayer, zIndex - 1);

                            }
						}

						Canvas.SetZIndex(frontDialog.RootElement, zIndex);

                        Canvas.SetZIndex(frontDialog.ModalLayer, zIndex - 1);

					}
				}
			}
			else
			{
				FrameworkElement container = XamDialogWindow.ResolveContainer(frontDialog); ;
				if (container != null)
				{
					WeakReferenceHelper<XamDialogWindow> dialogs = new WeakReferenceHelper<XamDialogWindow>();
					if (!this._containerDialogs.ContainsKey(container))
					{
						this._containerDialogs.Add(container, dialogs);
					}
					else
						dialogs = this._containerDialogs[container];

					dialogs.Remove(frontDialog);
					dialogs.Add(frontDialog);

					int zIndex = 0;

					int count = dialogs.Count;

					for (int i = 0; i < count; i++)
					{
						XamDialogWindow dialog = dialogs[i];
						if (dialog != null)
							Canvas.SetZIndex(dialog, zIndex += 2);
					}
				}
			}
		}

		public void UpdateModalLayer(XamDialogWindow dialog)
		{

            dialog.ModalLayer.Fill = dialog.ModalBackground;
            dialog.ModalLayer.Opacity = dialog.ModalBackgroundOpacity;




        }

		public void UpdateModalLayerEffect(XamDialogWindow dialog)
		{
			if (this.GetActiveDialogWindow() == dialog && dialog.IsModal)
			{
				this.AttachModalLayerBackgroundEffect(dialog);
			}
		}

		public void Register(XamDialogWindow dialog)
		{







			this.UnregisterRestricted(dialog);

			if (!this._dialogs.Contains(dialog))
			{

				// AS 6/10/11 TFS75729
				using (new Infragistics.Windows.Helpers.TempValueReplacement(dialog.RootElement, FrameworkElement.DataContextProperty))

				{
					if (dialog.ContainerContext.Children.Contains(dialog.RootElement))
					{
						dialog.ContainerContext.Children.Remove(dialog.RootElement);
					}

					this._dialogs.Add(dialog);

					Canvas contextPanel;

					contextPanel = dialog.ContextPanel;




					contextPanel.Children.Add(dialog.RootElement);


					

					if (dialog.ContainerContext != null)
					{
						dialog.ContainerContext.Children.Add(contextPanel);
					}

				}
            }
		}

		public void Unregister(XamDialogWindow dialog)
		{







			if (this._dialogs.Contains(dialog))
				this._dialogs.Remove(dialog);

            Canvas contextPanel;

            contextPanel = dialog.ContextPanel;





			// AS 6/10/11 TFS75729
			using (new Infragistics.Windows.Helpers.TempValueReplacement(dialog.RootElement, FrameworkElement.DataContextProperty))

			{

				

				if (dialog.ContainerContext != null)
				{
					dialog.ContainerContext.Children.Remove(contextPanel);
				}

				if (contextPanel.Children.Contains(dialog.RootElement))
				{
					contextPanel.Children.Remove(dialog.RootElement);
					dialog.ContainerContext.Children.Add(dialog.RootElement);
				}
			}
		}

		public void RegisterRestricted(XamDialogWindow dialog)
		{







			this.Unregister(dialog);

			FrameworkElement container = XamDialogWindow.ResolveContainer(dialog);

			if(container != null)
			{
				WeakReferenceHelper<XamDialogWindow> dialogs = new WeakReferenceHelper<XamDialogWindow>();
				if (!this._containerDialogs.ContainsKey(container))
				{
					this._containerDialogs.Add(container, dialogs);
				}
				else
					dialogs = this._containerDialogs[container];

				if (!dialogs.Contains(dialog))
				{
					dialogs.Add(dialog);
				}
			}
		}

		public void UnregisterRestricted(XamDialogWindow dialog)
		{







			FrameworkElement container = XamDialogWindow.ResolveContainer(dialog);

            if (container == null)
            {
                container = dialog.ContainerElement;
            }

			if (container != null)
			{
				WeakReferenceHelper<XamDialogWindow> dialogs = new WeakReferenceHelper<XamDialogWindow>();
                if (this._containerDialogs.ContainsKey(container))
                    dialogs = this._containerDialogs[container];

                if (dialogs.Contains(dialog))
                    dialogs.Remove(dialog);

                if (dialogs.Count == 0 && this._containerDialogs.ContainsKey(container))
                {
                    this._containerDialogs.Remove(container);
                }
			}
		}

		public void RegisterModal(XamDialogWindow dialog, bool showModalLayer)
		{







			if(this._modalDialogs.Contains(dialog))
				this._modalDialogs.Remove(dialog);

            this._modalDialogs.Add(dialog);

			if (showModalLayer)
				this.ShowModalLayer(dialog);
		}

		public void UnregisterModal(XamDialogWindow dialog)
		{







            if (this._modalDialogs.Contains(dialog))
                this._modalDialogs.Remove(dialog);

            this.HideModalLayer(dialog);

            if (this._modalDialogs.Count > 0)
            {
                XamDialogWindow window = null;

                for (int i = this._modalDialogs.Count - 1; i >= 0; i--)
                {
                    window = this._modalDialogs[i];
                    if (window != null && window.Visibility == Visibility.Visible)
                    {

                        this.BringToFront(window);

                        this.ShowModalLayer(window);
                        break;
                    }
                }

            }            
            
        }

		#endregion // Public

		#region Private

		private void ShowModalLayer(XamDialogWindow dialog)
		{



			// AS 6/22/12 TFS115111
			//// AS 6/22/11 TFS74670
			//// Steve added this class so the xamRibbonWindow has a way of detecting that there is an open modal xamDialogWindow.
			////
			//Window window = Window.GetWindow(dialog);
			//if (window != null)
			//    Infragistics.Windows.Internal.ModalWindowHelper.SetIsModalWindowOpen(window, true);
			FrameworkElement container = XamDialogWindow.ResolveContainer(dialog);
			dialog.ModalLayerContainer = container;

            FrameworkElement rootVisual = PlatformProxy.GetRootParent(dialog) as FrameworkElement;

            if (rootVisual != null)
			{
				if (dialog.RestrictInContainer)
				{
					this.Register(dialog);
				}

            Rectangle modalLayer = null;
            Canvas contextPanel = null;

            modalLayer = dialog.ModalLayer;
            contextPanel = dialog.ContextPanel;





            if (!contextPanel.Children.Contains(modalLayer))
                contextPanel.Children.Add(modalLayer);

				// Hook up the SizeChanged event, so that if the parent resizes, the ModalLayer's size can be updated as well
				rootVisual.SizeChanged += new SizeChangedEventHandler(this.Modal_RootVisual_SizeChanged);

				dialog.IsActive = true;
                
				this.AttachModalLayerBackgroundEffect(dialog);

                modalLayer.Visibility = Visibility.Visible;

				this.BringToFront(dialog);
                int currZIndex = 0;
                if (dialog.RootElement != null)
                {
                    currZIndex = Canvas.GetZIndex(dialog.RootElement);
                }
                Canvas.SetZIndex(modalLayer, currZIndex - 1);





                

				// AS 6/22/12 TFS115111
                //FrameworkElement container = XamDialogWindow.ResolveContainer(dialog);
                if (container != null)
                {
                    modalLayer.Height = container.ActualHeight;
                    modalLayer.Width = container.ActualWidth;
                }

            }
						
		}

		private void HideModalLayer(XamDialogWindow dialog)
		{

			// AS 6/22/12 TFS115111
			//// AS 6/22/11 TFS74670
			//Window window = Window.GetWindow(dialog);
			//if (window != null)
			//    Infragistics.Windows.Internal.ModalWindowHelper.SetIsModalWindowOpen(window, false);
			dialog.ModalLayerContainer = null;


			if (dialog.RestrictInContainer)
			{
				this.RegisterRestricted(dialog);
			}

            Rectangle modalLayer = null;

            modalLayer = dialog.ModalLayer;




			modalLayer.Visibility = Visibility.Collapsed;

			FrameworkElement rootVisual = PlatformProxy.GetRootVisual(dialog) as FrameworkElement;

			if (rootVisual != null)
			{
				// Hook up the SizeChanged event, so that if the parent resizes, the ModalLayer's size can be updated as well
				rootVisual.SizeChanged -= this.Modal_RootVisual_SizeChanged;
			}
			this.RemoveModalLayerBackgroundEffect(dialog);
		}

		private void AttachModalLayerBackgroundEffect(XamDialogWindow dialog)
		{

            
            this.ApplyModalEffect(dialog, false);


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)


            this.UpdateModalLayer(dialog);
		}

		private void RemoveModalLayerBackgroundEffect(XamDialogWindow dialog)
		{

            
            this.ApplyModalEffect(dialog, true);


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        }


        
        private void ApplyModalEffect(XamDialogWindow dialog, bool restoreOriginal)
        {
            Rectangle modalLayer = null;

            modalLayer = dialog.ModalLayer;




            if (!restoreOriginal)
            {
                modalLayer.Effect = dialog.ModalBackgroundEffect;
            }
            else
            {
                modalLayer.Effect = null;
            }

            FrameworkElement container = XamDialogWindow.ResolveContainer(dialog); ;
            if (container != null && this._containerDialogs.ContainsKey(container))
            {
                WeakReferenceHelper<XamDialogWindow> dialogs = this._containerDialogs[container];

                int count = dialogs.Count;

                for (int i = 0; i < count; i++)
                {
                    XamDialogWindow nonModalDialog = dialogs[i];
                    if (nonModalDialog != dialog)
                    {
                        if (!restoreOriginal)
                        {
                            nonModalDialog.SetEffect(dialog.ModalBackgroundEffect);
                        }
                        else
                        {
                            nonModalDialog.RestoreEffect();
                        }

                    }
                }
            }
        }


        #endregion // Private

		#endregion // Methods

		#region EventHandlers

        #region ContextPopup_Opened


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        #endregion // ContextPopup_Opened

		#region Modal_RootVisual_SizeChanged
		void Modal_RootVisual_SizeChanged(object sender, SizeChangedEventArgs e)
		{




        }
		#endregion // Modal_RootVisual_SizeChanged

		#endregion // EventHandlers
	}

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