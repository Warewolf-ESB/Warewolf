using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Threading;
using System.Diagnostics;
using System.Windows.Input;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Infragistics.Windows.Helpers;

namespace Infragistics.Windows.Ribbon
{





	internal class KeyTipManager
	{
		#region Member variables

		private IKeyTipContainer _currentKeyTipContainer;
		private KeyTipCollection _currentKeyTips;
		private List<KeyTipAdorner> _currentAdorners;

		private bool isHiding;
		private Stack<IKeyTipContainer> _parentContainers;
		private Stack<KeyTipCollection> _parentKeyTips;
		private Stack<List<KeyTipAdorner>> _parentAdorners;
		private IKeyTipContainer _startingKeyTipContainer;
		private XamRibbon _ribbon;

		private string _currentKeyTipValue = string.Empty;

		private IKeyTipProvider _providerPendingActivation;

		// MD 10/3/06 - BR16378
		private DispatcherTimer _showKeyTipsTimer;
		// AS 10/8/07 Not used
		//private bool _ignoreNextHiding;

		private object _minKeyTipWidth = null;

		// AS 11/1/07 BR27964
		private IKeyTipContainer _keyTipContainerBeingDeactivated;

		// AS 12/8/09 TFS25326
		private HashSet _registeredAccessKeys = new HashSet();
		private int _accessKeyVersion;
		private int _processedAccessKeyVersion = -1;

		#endregion Member variables

		#region Constants

		// MD 10/3/06 - BR16378
		private const int ShowDelay = 1000;

		#endregion Constants

		#region Constructor

		internal KeyTipManager(IKeyTipContainer startingKeyTipContainer, XamRibbon ribbon)
		{
			this._startingKeyTipContainer = startingKeyTipContainer;
			this._ribbon = ribbon;
		}

		#endregion Constructor

		#region Methods

		#region Public Methods

		#region ActivateKeyTipProvider



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		public void ActivateKeyTipProvider(IKeyTipProvider provider)
		{
			this.ActivateKeyTipProvider(provider, true);
		}

		#endregion ActivateKeyTipProvider

		// AS 11/1/07 BR27964
		#region ActivateKeyTipProviderIfNotPending
		/// <summary>
		/// Helper method to be called by a keytip container to ensure that its owning keytip provider has been activated.
		/// </summary>
		/// <param name="container">The keytip container whose items are being displayed</param>
		public void ActivateKeyTipProviderIfNotPending(IKeyTipContainer container)
		{
			IKeyTipProvider provider = null;

			// AS 11/13/07 BR28414
			// Don't force the keytip collection to be created if its not yet created.
			//
			if (this._currentKeyTips == null)
				return;

			foreach (KeyTip keyTip in this.CurrentKeyTips)
			{
				if (keyTip.Provider.GetContainer() == container)
				{
					provider = keyTip.Provider;
					break;
				}
			}

			if (null != provider && this.ProviderPendingActivation != provider)
				this.ActivateKeyTipProvider(provider);
		}
		#endregion //ActivateKeyTipProviderIfNotPending

		// AS 11/1/07 BR27964
		#region DeactivateKeyTipContainer
		/// <summary>
		/// Helper method used by a keytip container to activate the parent keytip container.
		/// </summary>
		public void DeactivateKeyTipContainer(IKeyTipContainer container)
		{
			if (this.IsActive &&
				this.isHiding == false &&
				this._currentKeyTipContainer == container &&
				this._keyTipContainerBeingDeactivated != container)
			{
				this.BackUpToParentContainer(true);

				if (this._currentKeyTipContainer != null)
					this.ShowKeyTipsInCurrentContainer();

				// AS 12/8/09 TFS25326
				this.RefreshAccessKeys();
			}
		} 
		#endregion //DeactivateKeyTipContainer

		#region HideKeyTips






		public void HideKeyTips()
		{
			this.HideKeyTips(false, false);
		}

		#endregion HideKeyTips

		// AS 12/19/07 BR29199
		// Added a helper method that popups can call so the keytipmanager can hide the keytips
		// if necessary. This is really for menus to use so that we end keytip mode when you open
		// a keytip using the mouse.
		//
		#region NotifyPopupOpened


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		public static void NotifyPopupOpened(PopupOpeningReason reason, DependencyObject d)
		{
			switch (reason)
			{
				case PopupOpeningReason.Mouse:
				case PopupOpeningReason.None:
					XamRibbon ribbon = XamRibbon.GetRibbon(d);

					if (null != ribbon &&
						ribbon.HasKeyTipManager &&
						ribbon.KeyTipManager.IsActive)
					{
						ribbon.KeyTipManager.HideKeyTips(false, true);
					}
					break;
			}
		} 
		#endregion //NotifyPopupOpened

		#region ProcessEscKey






		public void ProcessEscKey()
		{
			if (this.IsActive == false)
			{
				Debug.Fail("This should not have been called when the key tip manager was inactive");
				return;
			}

			// MD 9/27/06 - BR16285
			//this.BackUpToParentContainer();
			this.BackUpToParentContainer(true);

			if (this._currentKeyTipContainer != null)
				this.ShowKeyTipsInCurrentContainer();

			// AS 12/8/09 TFS25326
			this.RefreshAccessKeys();
		}

		#endregion ProcessEscKey

		#region ProcessKey

		// MD 7/16/07 - BR24959
		// Created to overload for processing a key which takes another parameter and returns a boolean
		public bool ProcessKey(char charCode, bool beepOnFail)
		{
			return this.EvaluateKey(charCode, beepOnFail, true);
		}

		#endregion ProcessKey

		#region EvaluateKeyDuringPendingShow

		public bool EvaluateKeyDuringPendingShow(char charCode)
		{
			this.VerifyKeyTipsAreInitialized();

			Debug.Assert(this._currentKeyTipContainer == this._startingKeyTipContainer);

			this.CancelPendingShow();

			if (this.EvaluateKey(charCode, false, false))
				return true;
			else
				this.HideCurrentAdorners();

			return false;
		}

		#endregion EvaluateKeyDuringPendingShow

		#region ShowKeyTips






		public void ShowKeyTips()
		{
			this.CancelPendingShow();
			this.VerifyKeyTipsAreInitialized();
			this.ShowKeyTipsInCurrentContainer(true);
		}

		// MD 7/16/07 - BR24959
		// Created new overload that takes another parameter
		private void ShowKeyTips(bool showKeyTipWindow)
		{
			// MD 10/3/06 - BR16378
			this.CancelPendingShow();

			// AS 12/8/09 TFS25326
			//this._currentKeyTips = null;
			this.CurrentKeyTips = null;

			if (this._parentContainers != null)
			{
				this._parentContainers.Clear();
				this._parentKeyTips.Clear();
			}

			// MD 7/16/07 - BR24959
			// Pass in new parameter to new ShowKeyTipsInContainer overload
			//this.ShowKeyTipsInContainer( this.startingKeyTipContainer );
			this.ShowKeyTipsInContainer(this._startingKeyTipContainer, showKeyTipWindow);
		}

		#endregion ShowKeyTips

		#endregion Public Methods

		#region Internal Methods

		// MD 10/3/06 - BR16378
		#region ShowKeyTipsAfterDelay

		internal void ShowKeyTipsAfterDelay()
		{
			if (this.ShowKeyTipsTimer.IsEnabled == false)
			{
				this.ShowKeyTipsTimer.Start();

				// AS 12/8/09 TFS25326
				// We need to create the keytips when the pending state is started 
				// so we can register access keys in case the end user presses the 
				// key before entering active keytip mode.
				//
				this.VerifyKeyTipsAreInitialized();
			}
		}

		#endregion ShowKeyTipsAfterDelay

		// MD 10/3/06 - BR16378
		#region TryHideKeyTips

		//internal bool TryHideKeyTips()
		//{
		//    if (this._ignoreNextHiding)
		//    {
		//        this._ignoreNextHiding = false;
		//        return false;
		//    }

		//    this.HideKeyTips();
		//    return true;
		//}

		#endregion TryHideKeyTips

		// MD 10/16/06
		#region UpdateVisibleKeyTips

		internal void UpdateVisibleKeyTips()
		{
			if (this._currentAdorners != null)
			{
				for (int i = 0, count = this._currentAdorners.Count; i < count; i++)
					this._currentAdorners[i].RefreshKeyTipVisibility();
			}
		}

		#endregion UpdateVisibleKeyTips

		#endregion Internal Methods

		#region Private Methods

		#region ActivateKeyTipProvider

		public void ActivateKeyTipProvider(IKeyTipProvider provider, bool verifyProviderExists)
		{
			// AS 11/13/07 BR28414
			// Don't foruce the keytips collection to be created.
			//
			//if (verifyProviderExists && this.CurrentKeyTips.Search(provider) == null)
			if (verifyProviderExists && (this._currentKeyTips == null || this.CurrentKeyTips.Search(provider) == null))
			{
				Debug.Fail("The key tip provider could not be found.");
				return;
			}

			IKeyTipContainer nextContainer = provider.GetContainer();

			// hide the current adorners
			if (null != this._currentAdorners)
			{
				foreach (KeyTipAdorner adorner in this._currentAdorners)
					adorner.Visibility = Visibility.Collapsed;
			}

			// If the new provider is not a container, hide the key tips
			if (nextContainer == null)
				this.HideKeyTips(provider.DeactivateParentContainersOnActivate, true);

			bool activated = provider.Activate();

			// If the pending provider has been activated, clear it
			if (provider == this._providerPendingActivation)
				this._providerPendingActivation = null;

			// If activation failed, exit now and don't show the key tips 
			// in this provider if its a container
			if (activated == false)
				return;

			// If the provider is also a container, shwo the key tips
			if (nextContainer != null)
			{
				this.ShowKeyTipsInContainer(nextContainer);

				
#region Infragistics Source Cleanup (Region)



























#endregion // Infragistics Source Cleanup (Region)

			}
		}

		#endregion ActivateKeyTipProvider

		#region BackUpToParentContainer

		// MD 9/27/06 - BR16285
		//private void BackUpToParentContainer()
		private void BackUpToParentContainer(bool deactivateCurrentContainer)
		{
			// MD 9/27/06 - BR16285
			// We don't always ant to deactivate the current container
			if (deactivateCurrentContainer)
			{
				// AS 11/1/07 BR27964
				// Store the container we're activating so we can ignore requests to deactivate
				// a container that we are deactivating.
				//
				//this._currentKeyTipContainer.Deactivate();

				Debug.Assert(this._keyTipContainerBeingDeactivated == null);
				IKeyTipContainer oldContainer = this._keyTipContainerBeingDeactivated;
				this._keyTipContainerBeingDeactivated = this._currentKeyTipContainer;

				try
				{
					this._currentKeyTipContainer.Deactivate();
				}
				finally
				{
					this._keyTipContainerBeingDeactivated = oldContainer;
				}
			}

			this.RemoveCurrentAdorners();

			if (this._parentContainers == null || this._parentContainers.Count == 0)
			{
				this._minKeyTipWidth = null;

				// AS 12/8/09 TFS25326
				//this._currentKeyTips = null;
				this.CurrentKeyTips = null;
				this._currentKeyTipContainer = null;
				this._currentAdorners = null;

				return;
			}

			this._currentKeyTipContainer = this.ParentContainers.Pop();
			// AS 12/8/09 TFS25326
			//this._currentKeyTips = this.ParentKeyTips.Pop();
			this.CurrentKeyTips = this.ParentKeyTips.Pop();
			this._currentAdorners = this.ParentAdorners.Pop();
		}

		#endregion BackUpToParentContainer

		#region Beep

		// MD 1/10/07 - Optimization
		// Made static
		//private void Beep()
		private static void Beep()
		{
			System.Media.SystemSounds.Beep.Play();
		}

		#endregion Beep

		// MD 10/3/06 - BR16378
		#region CancelPendingShow

		private void CancelPendingShow()
		{
			if (this._showKeyTipsTimer != null && this._showKeyTipsTimer.IsEnabled)
				this._showKeyTipsTimer.IsEnabled = false;
		}

		#endregion CancelPendingShow

		#region CreateKeyTips






		private void CreateKeyTipsForContainer(IKeyTipContainer container)
		{
			foreach (IKeyTipProvider provider in container.GetKeyTipProviders())
				this.CurrentKeyTips.Add(new KeyTip(this, provider));

			// Reuse key tips in the parent container if necessary
			if (container.ReuseParentKeyTips)
				this.ReuseKeyTipsInParent();
			// MD 11/21/06
			// Put the following code in an else block - if we resure parent key tips,
			// no new key tips should be created

			// AS 9/13/07
			// Technically there could be keytips that still need to be resolved
			// so let it go through the following logic. Of course if that does happen
			// and the keytip conflicts with that of the parent keytip then the keytip
			// in the "child" would be different then the keytip of the parent so we'll
			// have to add logic in the resolve conflicts to handle that should it
			// ever come up.
			//
			//else
			{
				// Initialize keys tips for the providers with valid key tip values
				this.InitializeKeyTipsUsingKeyTips();

				// MD 9/27/06 - BR16291
				// Don't leave if we shouldn't auto-generate key tips because we always need to resolve conflicts
				// Instead, only auto-generate when requested to and continue on no matter what.
				//
				//// If we shouldn't auto-generate key tips, return, the remaining providers 
				//// will not have key tips created for them
				//if ( this.ToolbarsManager.AutoGenerateKeyTipsResolved == false )
				//    return;
				if (this.Ribbon.AutoGenerateKeyTips)
				{
					// Initialize keys tips for the providers with valid mnemonics
					this.InitializeKeyTipsUsingMnemonics();

					// Initialize keys tips for the providers with captions
					this.InitializeKeyTipsUsingCaptions();

					// Initialize keys tips for the other providers
					this.InitializeRemainingKeyTips();
				}
			}

			// MD 9/27/06 - BR16291
			// If some keytips were not auto-generated, remove them from the current key tips collection
			this.RemoveInvalidKeyTips();

			// Make sure all key tips are unique
			this.ResolveConflicts();
		}

		#endregion CreateKeyTips

		#region CreateAdornersForKeyTips
		private void CreateAdornersForKeyTips()
		{
			// create the adorners
			Dictionary<AdornerLayer, KeyTipAdorner> _adorners = new Dictionary<AdornerLayer, KeyTipAdorner>();

			for (int i = 0, count = this.CurrentKeyTips.Count; i < count; i++)
			{
				KeyTip keyTip = this.CurrentKeyTips[i];
				UIElement adornedElement = keyTip.Provider.AdornedElement;

				// make sure the keytip meets the minimum size requirements
				keyTip.SetValue(FrameworkElement.MinWidthProperty, this.MinKeyTipWidth);

				AdornerLayer layer = AdornerLayer.GetAdornerLayer(adornedElement);
				KeyTipAdorner adorner = null;

				if (null != layer)
				{
					AdornerLayer previous = null;

					while (layer != null && layer != previous)
					{
						previous = layer;
						Visual parent = VisualTreeHelper.GetParent(layer) as Visual;

						if (parent == null)
							break;

						layer = AdornerLayer.GetAdornerLayer(parent);

						// AS 10/2/09 TFS23024
						// We don't want to use the ancestor adorner layer if the adorner layer 
						// we have found at this point is within a scrollviewer because we want the 
						// keytips to be able to scroll with the scroll viewer so they stay in position 
						// relative to the adorned element.
						//
						Visual newLayerParent = layer != null ? VisualTreeHelper.GetParent(layer) as Visual : null;

						if (newLayerParent != null)
						{
							// AS 10/20/10 TFS57641
							// We'll go past the scrollviewer if the scrollviewer isn't scrollable.
							//
							//if (parent is ScrollViewer || Utilities.GetAncestorFromType(parent, typeof(ScrollViewer), true, newLayerParent) != null)
							ScrollViewer sv = parent as ScrollViewer ?? Utilities.GetAncestorFromType(parent, typeof(ScrollViewer), true, newLayerParent) as ScrollViewer;

							if (sv != null && (sv.ViewportHeight < sv.ExtentHeight || sv.ViewportWidth < sv.ExtentWidth))
								break;
						}
					}

					// use the previous one
					layer = previous;

					if (false == _adorners.TryGetValue(layer, out adorner))
					{
						UIElement adornerAdornedElement = GetElementToAdorn(layer, adornedElement);

						if (null != adornerAdornedElement)
						{
							adorner = new KeyTipAdorner(adornerAdornedElement);
							adorner.Visibility = Visibility.Collapsed;
							this.CurrentAdorners.Add(adorner);
							_adorners.Add(layer, adorner);
							layer.Add(adorner);
						}
					}
				}

				if (null != adorner)
					adorner.Add(keyTip);
			}
		} 
		#endregion //CreateAdornersForKeyTips

		#region EvaluateKey
		private bool EvaluateKey(char charCode, bool beepOnFail, bool processKey)
		{
			if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
			{
				if (beepOnFail)
					Beep();

				return false;
			}

			// ignore control codes like escape
			if (char.IsControl(charCode))
				return false;

			if (IsValidKeyTipChar(charCode) == false)
			{
				if (beepOnFail)
					Beep();

				return false;
			}

			string newKeyTipValue = _currentKeyTipValue + charCode.ToString();
			newKeyTipValue = newKeyTipValue.ToUpper(CultureInfo.CurrentCulture);

			bool foundMatch = false;

			foreach (KeyTip keyTip in this.CurrentKeyTips)
			{
				if (keyTip.Provider.IsEnabled == false)
					continue;

				// AS 6/11/12 TFS112682
				// Skip collapsed items. Note we cannot check the IsVisible state since some items 
				// may be "out of view" but should be keyboard interactive. e.g. the items in the 
				// qat that are in the overflow panel.
				//
				var element = keyTip.Provider.AdornedElement;
				if (element != null && element.Visibility == Visibility.Collapsed)
					continue;

				if (keyTip.Value == newKeyTipValue)
				{
					if (processKey)
					{
						this._providerPendingActivation = keyTip.Provider;
						this.ActivateKeyTipProvider(this._providerPendingActivation, false);
					}

					// MD 7/16/07 - BR24959
					// If we were able to activate a key tip provider, return true
					//return;
					return true;
				}
				else if (keyTip.Value.StartsWith(newKeyTipValue))
				{
					foundMatch = true;
					break;
				}
			}

			if (foundMatch == false)
			{
				// MD 1/10/07 - Optimization
				// The method is now static
				//this.Beep();
				// MD 7/16/07 - BR24959
				// We may not always beep now, and we have to return false on a fail too
				//Beep();
				//return;
				if (beepOnFail)
					Beep();

				return false;
			}

			if (processKey)
			{
				// AS 12/8/09 TFS25326
				//this._currentKeyTipValue = newKeyTipValue;
				this.CurrentKeyTipValue = newKeyTipValue;

				// AS 12/8/09 TFS25326
				// When a key is typed, we want to register new access keys for the 
				// remaining visible keytips based on the next character that can be 
				// typed.
				// 
				this.RefreshAccessKeys();

				// MD 10/16/06
				// Moved this code to the UpdateVisibleKeyTips method
				//this.keyTipForm.Visible = false;
				//this.keyTipForm.RefreshVisibleKeyTips();
				//this.keyTipForm.Show();
				this.UpdateVisibleKeyTips();
			}

			// MD 7/16/07 - BR24959
			// Return true if the ley was processed correctly
			return true;
		} 
		#endregion //EvaluateKey

		#region GetElementToAdorn
		private static UIElement GetElementToAdorn(AdornerLayer layer, UIElement adornedElement)
		{
			DependencyObject parent = VisualTreeHelper.GetParent(layer);

			for (int i = 0, count = VisualTreeHelper.GetChildrenCount(parent); i < count; i++)
			{
				Visual child = VisualTreeHelper.GetChild(parent, i) as Visual;
				if (null != child && child.IsAncestorOf(adornedElement))
					return child as UIElement;
			}

			return null;
		} 
		#endregion //GetElementToAdorn

		#region GetValidAutoGeneratePrefix







		private string GetValidAutoGeneratePrefix(IKeyTipProvider provider)
		{
			string autoGeneratePrefix = provider.AutoGeneratePrefix;

			string validPrefix = string.Empty;

			// If the provider returns no auto generate prefix, 
			// return the empty string
			if (autoGeneratePrefix == null)
				return validPrefix;

			// Otherwise, append up to 2 valid characters to the prefix
			foreach (char c in autoGeneratePrefix.ToCharArray())
			{
				if (validPrefix.Length == 2)
					break;

				// MD 1/10/07 - Optimization
				// The method is now static
				//if ( this.IsValidKeyTipChar( c ) )
				if (IsValidKeyTipChar(c))
					validPrefix += c;
			}

			return validPrefix;
		}

		#endregion GetValidAutoGeneratePrefix

		#region HideCurrentAdorners
		private void HideCurrentAdorners()
		{
			if (null != this._currentAdorners)
			{
				foreach (KeyTipAdorner adorner in this._currentAdorners)
					adorner.Visibility = Visibility.Collapsed;
			}
		}
		#endregion //HideCurrentAdorners

		#region HideKeyTips



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		private void HideKeyTips(bool deactivateParentContainers, bool resetMode)
		{
			// MD 10/3/06 - BR16378
			this.CancelPendingShow();

			// MD 10/9/06
			// This no longer applies as it could be called when the showing of key tips is delayed, 
			// at which point IsActive is false, and this would fail
			//if ( this.IsActive == false )
			//{
			//    Debug.Fail( "This should not have been called when the key tip manager was inactive" );
			//    return;
			//}

			if (this.isHiding)
				return;

			this.isHiding = true;

			while (this._currentKeyTipContainer != null)
			{
				// MD 9/27/06 - BR16285
				//this.BackUpToParentContainer();
				this.BackUpToParentContainer(deactivateParentContainers);
			}

			this.isHiding = false;

			// AS 12/8/09 TFS25326
			// When the keytips are removed we want to remove the access keys.
			//
			this.RefreshAccessKeys();

			// let the ribbon know so it can restore focus now if needed
			this.Ribbon.OnKeyTipsHidden(deactivateParentContainers, resetMode);
		}

		#endregion HideKeyTips

		#region InitializeKeyTipsUsingCaptions






		private void InitializeKeyTipsUsingCaptions()
		{
			// Create a key tip collection to hold all uninitialized key tips with captions
			KeyTipCollection keyTipsWithCaptions = new KeyTipCollection(this);

			// Initialize the key tips whose providers have captions
			for (int i = 0; i < this.CurrentKeyTips.Count; i++)
			{
				KeyTip keyTip = this.CurrentKeyTips[i];

				// If the key tip value has already been set, go to the next one
				if (false == string.IsNullOrEmpty(keyTip.Value))
					continue;

				// AS 1/5/10 TFS25626
				if (!keyTip.Provider.CanAutoGenerateKeyTip)
					continue;

				string captionValue = keyTip.Provider.Caption;
				if (string.IsNullOrEmpty(captionValue))
					continue;

				// If the provider has a valid caption, add the key tip to the collection of
				// key tips to be assigned values from their providers' captions
				keyTipsWithCaptions.Add(keyTip);
			}

			// Loop over each key tip in the collection and try to initialize them with a unique, 
			// single character where the character exists within their caption.  If this 
			// is not possible, initialize the key tip to the first character of the caption
			// that is a valid char.  It will later be appended with numbers to make it unique.
			for (int i = 0; i < keyTipsWithCaptions.Count; i++)
			{
				KeyTip keyTip = keyTipsWithCaptions[i];

				string prefix = this.GetValidAutoGeneratePrefix(keyTip.Provider);

				string firstValidKeyTip = string.Empty;

				// Get the caption from the key tip provider
				string caption = keyTip.Provider.Caption;
				int currentCharIndex = 0;

				// Loop over each character in the caption
				while (currentCharIndex < caption.Length)
				{
					char currentChar = caption[currentCharIndex++];

					// Make sure the current character is a valid key tip character
					// MD 1/10/07 - Optimization
					// The method is now static
					//if ( this.IsValidKeyTipChar( currentChar ) == false )
					if (IsValidKeyTipChar(currentChar) == false)
						continue;

					string desiredValue = (prefix + currentChar).ToUpper(CultureInfo.CurrentCulture);

					// Save the first valid key tip so we don't need to loop over them again 
					// if we need it later
					if (firstValidKeyTip.Length == 0)
						firstValidKeyTip = desiredValue;

					// Make sure the key tip is not already used and there is no key tip that 
					// starts with the desired value
					bool alreadyUsed = false;
					foreach (KeyTip existingKeyTip in this.CurrentKeyTips)
					{
						if (existingKeyTip.Value == null)
							continue;

						if (existingKeyTip.Value.StartsWith(desiredValue))
						{
							alreadyUsed = true;
							break;
						}
					}

					if (alreadyUsed == false)
					{
						// If the key tip has not yet been used, set it on this key tip
						keyTip.Value = desiredValue;
						break;
					}
				}

				// If no unique key tip could be found, use the first valid key tip
				if (keyTip.Value == null)
				{
					if (firstValidKeyTip.Length == 0)
						keyTip.Value = prefix;
					else
						keyTip.Value = firstValidKeyTip;
				}
			}
		}

		#endregion InitializeKeyTipsUsingCaptions

		#region InitializeKeyTipsUsingKeyTips






		private void InitializeKeyTipsUsingKeyTips()
		{
			// Initialize the key tips whose providers have key tip values
			for (int i = 0; i < this.CurrentKeyTips.Count; i++)
			{
				KeyTip keyTip = this.CurrentKeyTips[i];

				// If the key tip value has already been set, go to the next one
				if (false == string.IsNullOrEmpty(keyTip.Value))
					continue;

				string keyTipValue = keyTip.Provider.KeyTipValue;
				if (string.IsNullOrEmpty(keyTipValue))
					continue;

				// If the provider has a valid key tip value, initialze the key tip
				// MD 11/21/06 - Use the key tip value which was already obtained
				//keyTip.Value = keyTip.Provider.KeyTipValue;
				keyTip.Value = keyTipValue;
			}
		}

		#endregion InitializeKeyTipsUsingKeyTips

		#region InitializeKeyTipsUsingMnemonics






		private void InitializeKeyTipsUsingMnemonics()
		{
			// Initialize the key tips whose providers have mnemonics
			for (int i = 0; i < this.CurrentKeyTips.Count; i++)
			{
				KeyTip keyTip = this.CurrentKeyTips[i];

				// If the key tip value has already been set, go to the next one
				if (false == string.IsNullOrEmpty(keyTip.Value))
					continue;

				// AS 1/5/10 TFS25626
				if (!keyTip.Provider.CanAutoGenerateKeyTip)
					continue;

				// MD 11/21/06 - Reserving Menu Mnemonics
				// Changed the logic to perform some additional checks
				//if ( Char.IsLetterOrDigit( keyTip.Provider.Mnemonic ) == false )
				//    continue;
				//
				//// If the provider has a valid mnemonic, initialize the key tip
				//keyTip.Value = this.GetValidAutoGeneratePrefix( keyTip.Provider ) + keyTip.Provider.Mnemonic;
				string prefix = this.GetValidAutoGeneratePrefix(keyTip.Provider);

				Char mnemonic = keyTip.Provider.Mnemonic;

				// MD 1/10/07 - Optimization
				// The method is now static
				//if ( this.IsValidKeyTipChar( mnemonic ) == false )
				if (IsValidKeyTipChar(mnemonic) == false)
					continue;

				// If the provider has a valid mnemonic, initialize the key tip
				keyTip.Value = prefix + mnemonic;
			}
		}

		#endregion InitializeKeyTipsUsingMnemonics

		#region InitializeRemainingKeyTips






		private void InitializeRemainingKeyTips()
		{
			// Initialize key tip value for the remaining key tips
			for (int i = 0; i < this.CurrentKeyTips.Count; i++)
			{
				KeyTip keyTip = this.CurrentKeyTips[i];

				// If the key tip value has already been set, go to the next one
				if (false == string.IsNullOrEmpty(keyTip.Value))
					continue;

				// AS 1/5/10 TFS25626
				if (!keyTip.Provider.CanAutoGenerateKeyTip)
					continue;

				// Use the auto generate prefix as the initial key tip
				keyTip.Value = this.GetValidAutoGeneratePrefix( keyTip.Provider );

				// AS 10/3/07 BR27022
				// If there is no caption, mnemonic or prefix then use an additional
				// char that we get from the provider.
				//
				if (string.IsNullOrEmpty(keyTip.Value))
				{
					char prefixChar = keyTip.Provider.DefaultPrefix;

					if (IsValidKeyTipChar(prefixChar))
						keyTip.Value = new string(prefixChar, 1);
				}
			}
		}

		#endregion InitializeRemainingKeyTips

		#region IsValidKeyTipChar






		// MD 1/10/07 - Optimization
		// Made static
		//private bool IsValidKeyTipChar( char c )
		// AS 10/10/07 BR27204
		//private static bool IsValidKeyTipChar(char c)
		internal static bool IsValidKeyTipChar(char c)
		{
			// AS 10/10/07 BR27204
			// Office seems to accept more than just letters and numbers.
			//
			//return Char.IsLetterOrDigit(c);
			return Char.IsLetterOrDigit(c)	||
				Char.IsPunctuation(c)		||
				(Char.IsSeparator(c) && Char.IsWhiteSpace(c) == false)	||
				Char.IsSymbol(c);
		}

		#endregion IsValidKeyTipChar

		// AS 12/8/09 TFS25326
		#region OnKeyTipAccessKeyPressed
		private void OnKeyTipAccessKeyPressed(object sender, AccessKeyPressedEventArgs e)
		{
			Debug.Assert(IsActive || IsShowPending);

			if (e.Scope == null && e.Target == null)
			{
				// AS 2/3/10 TFS27107
				// The Key is sometimes null when the framework is trying to 
				// find out what key is associated with a given target element.
				// 
				if (string.IsNullOrEmpty(e.Key))
					return;

				if (IsActive || IsShowPending)
				{
					if (_registeredAccessKeys.Exists(e.Key))
					{
						e.Target = _ribbon;
					}
				}
			}
		}
		#endregion //OnKeyTipAccessKeyPressed

		// AS 12/8/09 TFS25326
		// Normally when you press something like Alt + F, we get the Alt key and enter 
		// KeyTipPending mode. At that point we start a timer and wait to see if a 
		// character is entered within that time frame. If we get a key press, we see 
		// if its a keytip. To do that we handle the TextInput event. Unfortunately 
		// when focus is within an HwndHost such as a WindowsFormsHost, this event is 
		// not raised. However, controls like a menu continue to process mnemonics 
		// like Alt+F because they are registered access keys and the OnMnemonic of the 
		// main window's IKeyboardInputSink is still invoked for that mnemonic. So in 
		// order to support mnemonics when using keytips we have to register keys for 
		// the next character in the keytip.
		// 
		// 
		#region RefreshAccessKeys
		private void RefreshAccessKeys()
		{
			if (_accessKeyVersion == _processedAccessKeyVersion)
				return;

			if (_registeredAccessKeys.Count > 0)
			{
				Debug.WriteLine("Removing KeyTip AccessKeys", DateTime.Now.ToString("hh:mm:ss:ffffff"));

				AccessKeyManager.RemoveAccessKeyPressedHandler(_ribbon, new AccessKeyPressedEventHandler(OnKeyTipAccessKeyPressed));

				foreach (string accessKey in _registeredAccessKeys)
				{
					AccessKeyManager.Unregister(accessKey, _ribbon);
				}
			}

			_registeredAccessKeys.Clear();
			int charIndex = _currentKeyTipValue == null ? 0 : _currentKeyTipValue.Length;

			if (_currentKeyTips != null && _currentKeyTips.Count > 0)
			{
				Debug.WriteLine("Adding KeyTip AccessKeys", DateTime.Now.ToString("hh:mm:ss:ffffff"));

				foreach (KeyTip tip in this.CurrentKeyTips)
				{
					if (!tip.IsKeyTipVisible)
						continue;

					if (!tip.IsEnabled)
						continue;

					string value = tip.Value;

					if (string.IsNullOrEmpty(value) || value.Length <= charIndex)
						continue;

					string accessKey = value.Substring(charIndex, 1);

					// there can be multiple items with the same starting character
					// so only register a character once
					if (_registeredAccessKeys.Exists(accessKey))
						continue;

					_registeredAccessKeys.Add(accessKey);

					Debug.WriteLine("\t" + accessKey);
					AccessKeyManager.Register(accessKey, _ribbon);
				}

				if (_registeredAccessKeys.Count > 0)
					AccessKeyManager.AddAccessKeyPressedHandler(_ribbon, new AccessKeyPressedEventHandler(OnKeyTipAccessKeyPressed));
			}
		}
		#endregion //RefreshAccessKeys

		#region RemoveCurrentAdorners
		private void RemoveCurrentAdorners()
		{
			if (this._currentAdorners != null)
			{
				foreach (KeyTipAdorner adorner in this._currentAdorners)
				{
					AdornerLayer layer = adorner.Parent as AdornerLayer;

					if (null != layer)
						layer.Remove(adorner);
				}
			}
		}
		#endregion //RemoveCurrentAdorners

		// MD 9/27/06 - BR16291
		#region RemoveInvalidKeyTips






		private void RemoveInvalidKeyTips()
		{
			// Remove the key tips with no values from the current key tips
			for (int i = this.CurrentKeyTips.Count - 1; i >= 0 ; i--)
			{
				KeyTip keyTip = this.CurrentKeyTips[i];

				// AS 1/5/10 TFS25626
				// I found this while debugging. Essentially if we had set the KeyTip to 
				// null then it would have been coerced to an empty string and that is 
				// not a valid keytip so we need to remove it.
				//
				//if (keyTip.Value == null)
				if (string.IsNullOrEmpty(keyTip.Value))
					this.CurrentKeyTips.RemoveAt(i);
			}
		}

		#endregion RemoveInvalidKeyTips

		#region ResolveConflicts






		private void ResolveConflicts()
		{
			// Sort a clone of the collection so the original list maintains its order
			KeyTipCollection keyTipsToSort = this.CurrentKeyTips.Clone();

			int beginSearchAt = 0;
			bool conflictOccurred;

			do
			{
				// Reset the flag when the next pass occurs
				conflictOccurred = false;

				// Sort the current key tips based on their values
				keyTipsToSort.Sort();

				// MD 8/17/07 - 7.3 Performance
				// Use generics
				//ArrayList conflictKeyTips = null;
				List<KeyTip> conflictKeyTips = null;

				for (int i = beginSearchAt; i < keyTipsToSort.Count - 1; i++)
				{
					// Get the first ans second key tip starting at the current index
					KeyTip firstKeyTip = keyTipsToSort[i];
					KeyTip secondKeyTip = keyTipsToSort[i + 1];

					// If their values match, gather all key tips that match these
					if (firstKeyTip.Value == secondKeyTip.Value)
					{
						// Initialize the collection of matching key tips
						if (conflictKeyTips == null)
						{
							// MD 8/17/07 - 7.3 Performance
							// Use generics
							//conflictKeyTips = new ArrayList();
							conflictKeyTips = new List<KeyTip>();
						}
						else
							conflictKeyTips.Clear();

						// Add the known matches
						conflictKeyTips.Add(firstKeyTip);
						conflictKeyTips.Add(secondKeyTip);

						// Add in any other matches in the collection
						for (i += 2; i < keyTipsToSort.Count; i++)
						{
							KeyTip keyTip = (KeyTip)keyTipsToSort[i];

							// After the first non-match, bail out, there are no others in the collection
							if (keyTip.Value != firstKeyTip.Value)
								break;

							conflictKeyTips.Add(keyTip);
						}

						// Construct the number format to use when appending the index to the matching key tips
						// Form the format, use the number of digits in one less than the last number so when there 
						// are only ten matching keytips, they will be numbered 1-9 and then 0.
						int formatDigits = (conflictKeyTips.Count - 1).ToString(CultureInfo.CurrentCulture).Length;
						string format = new string('0', formatDigits);

						// Append an increasing number to each matching keytip, starting with 1
						for (int conflictIndex = 0; conflictIndex < conflictKeyTips.Count; conflictIndex++)
						{
							// MD 8/17/07 - 7.3 Performance
							// Use generics
							//KeyTip keyTip = (KeyTip)conflictKeyTips[ conflictIndex ];
							KeyTip keyTip = conflictKeyTips[conflictIndex];

							string uniqueSuffix = (conflictIndex + 1).ToString(format, CultureInfo.CurrentCulture);

							// If there are only 10 matching keytips, the last one should just be '0'
							if (uniqueSuffix.Length > formatDigits)
								uniqueSuffix = uniqueSuffix.Substring(uniqueSuffix.Length - formatDigits);

							keyTip.Value += uniqueSuffix;
						}

						// Mark that a conflict has occurred so another pass is made, just in case resolving 
						// this conflict has resulted in any other conflict.
						conflictOccurred = true;
						break;
					}
					// If the second key tip supersedes the first, append a '1' to the value of the first
					else if (secondKeyTip.Value.StartsWith(firstKeyTip.Value))
					{
						firstKeyTip.Value += 1.ToString(CultureInfo.CurrentCulture);
						conflictOccurred = true;
						break;
					}
					// If there were no conflicts at this index, there will never be any before this,
					// so always start subsequent passes after this item.
					else
						beginSearchAt = i + 1;
				}
			}
			// Continue resolving conflicts until all has been resolved
			while (conflictOccurred);
		}

		#endregion ResolveConflicts

		#region ReuseKeyTipsInParent







		private void ReuseKeyTipsInParent()
		{
			if (this._parentKeyTips == null || this._parentKeyTips.Count == 0)
				return;

			KeyTipCollection parentKeyTips = this._parentKeyTips.Peek() as KeyTipCollection;

			if (parentKeyTips == null)
			{
				Debug.Fail("The parent key tips collection is invalid");
				return;
			}

			// Create a key tip collection to hold all new key tips in this pass
			// MD 1/10/07 - Optimization
			// Not used
			//KeyTipCollection currentPassKeyTips = new KeyTipCollection( this );

			// Add key tips from the providers which have associated key tips in the parent container
			for (int i = 0; i < this.CurrentKeyTips.Count; i++)
			{
				KeyTip newKeyTip = this.CurrentKeyTips[i];

				// If the key tip value has already been set, go to the next one
				if (newKeyTip.Value != null)
					continue;

				KeyTip existingKeyTip = parentKeyTips.Search(newKeyTip.Provider);
				if (existingKeyTip == null)
					continue;

				// If the key tip was found in the parent, add a new key tip to the current key tips collection
				newKeyTip.Value = existingKeyTip.Value;
			}
		}

		#endregion ReuseKeyTipsInParent

		#region ShowKeyTipsInContainer






		private void ShowKeyTipsInContainer(IKeyTipContainer container)
		{
			// MD 7/16/07 - BR24959
			// Moved all code no new overload
			this.ShowKeyTipsInContainer(container, true);
		}

		// MD 7/16/07 - BR24959
		// Created new overload that takes another parameter
		private void ShowKeyTipsInContainer(IKeyTipContainer container, bool showKeyTipWindow)
		{
			Debug.Assert(container != _currentKeyTipContainer);

			// If there is already a container showing its key tips, push it onto the beadcrumb stack of containers
			if (this._currentKeyTipContainer != null)
			{
				this.ParentContainers.Push(this._currentKeyTipContainer);
				this.ParentKeyTips.Push(this._currentKeyTips);
				this.ParentAdorners.Push(this._currentAdorners);
				
				// Reset the key tip collection so we create a new instance of it next time
				// AS 12/8/09 TFS25326
				//this._currentKeyTips = null;
				this.CurrentKeyTips = null;
				this._currentAdorners = null;
			}

			this._currentKeyTipContainer = container;

			// Generate all key tips
			this.CreateKeyTipsForContainer(this._currentKeyTipContainer);

			// create the adorners for the keytips and add them into it
			this.CreateAdornersForKeyTips();

			// MD 7/16/07 - BR24959
			// Pass in the new parameter to the new overload of ShowKeyTipsInCurrentContainer
			//this.ShowKeyTipsInCurrentContainer();
			this.ShowKeyTipsInCurrentContainer(showKeyTipWindow);
		}

		#endregion ShowKeyTipsInContainer

		#region ShowKeyTipsInCurrentContainer

		private void ShowKeyTipsInCurrentContainer()
		{
			// MD 7/16/07 - BR24959
			// Moved all code no new overload
			this.ShowKeyTipsInCurrentContainer(true);
		}

		// MD 7/16/07 - BR24959
		// Created new overload that takes another parameter
		private void ShowKeyTipsInCurrentContainer(bool showKeyTipWindow)
		{
			// AS 12/8/09 TFS25326
			//this._currentKeyTipValue = string.Empty;
			this.CurrentKeyTipValue = string.Empty;
			this.RefreshAccessKeys();

			// MD 7/16/07 - BR24959
			// If we shouldn't show the key tip window, just return
			if (showKeyTipWindow == false)
				return;

			if (null != this._currentAdorners)
			{
				foreach (KeyTipAdorner adorner in this._currentAdorners)
				{
					adorner.Visibility = Visibility.Visible;
					adorner.RefreshKeyTipVisibility();
				}
			}
		}

		#endregion ShowKeyTipsInCurrentContainer

		#region VerifyKeyTipsAreInitialized
		private void VerifyKeyTipsAreInitialized()
		{
			// AS 11/13/07 BR28414
			// Just in case something references the currentkeytips property which caused it to 
			// be allocated, also get in here if we don't have a current keytip container.
			//
			//if (this._currentKeyTips == null)
			if (this._currentKeyTips == null || (this._currentKeyTips.Count == 0 && this._currentKeyTipContainer == null))
			{
				this.ShowKeyTipsInContainer(this._startingKeyTipContainer, false);
			}
		}
		#endregion //VerifyKeyTipsAreInitialized

		#endregion Private Methods

		#endregion Methods

		#region Properties

		#region Public Properties

		#region CurrentAdorners

		public List<KeyTipAdorner> CurrentAdorners
		{
			get
			{
				if (this._currentAdorners == null)
					this._currentAdorners = new List<KeyTipAdorner>();

				return this._currentAdorners;
			}
		}

		#endregion CurrentAdorners

		// MD 6/6/07 - BR23548
		#region CurrentContainerLevel

		public int CurrentContainerLevel
		{
			get
			{
				return this._parentContainers == null ? 0 : this._parentContainers.Count;
			}
		}

		#endregion CurrentContainerLevel

		#region CurrentKeyTips

		// AS 11/13/07 BR28414
		// Make this private since someone outside could reference this forcing the collection
		// to be created which would cause problems when we go to show the keytips later because
		// we'll think we don't need to do anything because the current keytips property is
		// non-null.
		//
		//public KeyTipCollection CurrentKeyTips
		private KeyTipCollection CurrentKeyTips
		{
			get
			{
				if (this._currentKeyTips == null)
				{
					// AS 12/8/09 TFS25326
					this._currentKeyTips = new KeyTipCollection(this);

					// AS 12/8/09 TFS25326
					_accessKeyVersion++;
				}

				return this._currentKeyTips;
			}
			// AS 12/8/09 TFS25326
			set
			{
				if (_currentKeyTips != value)
				{
					_currentKeyTips = value;
					_accessKeyVersion++;
				}
			}
		}

		#endregion CurrentKeyTips

		#region CurrentKeyTipValue

		public string CurrentKeyTipValue
		{
			get { return this._currentKeyTipValue; }
			// AS 12/8/09 TFS25326
			// Added setter and updated places that were setting member
			// so we can dirty the access key version to know if we need 
			// to register/unregister the access keys for the keytips.
			//
			private set
			{
				if (value != _currentKeyTipValue)
				{
					_currentKeyTipValue = value;
					_accessKeyVersion++;
				}
			}
		}

		#endregion CurrentKeyTipValue

		#region IsActive

		public bool IsActive
		{
			get { return this._currentKeyTipContainer != null; }
		}

		#endregion IsActive

		// MD 7/16/07 - BR24959
		#region IsShowPending

		public bool IsShowPending
		{
			get
			{
				return this._showKeyTipsTimer != null && this._showKeyTipsTimer.IsEnabled;
			}
		}

		#endregion IsShowPending

		#region ProviderPendingActivation

		public IKeyTipProvider ProviderPendingActivation
		{
			get { return this._providerPendingActivation; }
		}

		#endregion ProviderPendingActivation

		#region Ribbon

		public XamRibbon Ribbon
		{
			get { return this._ribbon; }
		}

		#endregion //Ribbon

		// MD 10/3/06 - BR16378
		#region ShowKeyTipsTimer

		private DispatcherTimer ShowKeyTipsTimer
		{
			get
			{
				if (this._showKeyTipsTimer == null)
				{
					this._showKeyTipsTimer = new DispatcherTimer(DispatcherPriority.Normal, this.Ribbon.Dispatcher);
					this._showKeyTipsTimer.Interval = TimeSpan.FromMilliseconds(KeyTipManager.ShowDelay);
					this._showKeyTipsTimer.Tick += new EventHandler(this.OnShowKeyTipsTimerTick);
				}

				return this._showKeyTipsTimer;
			}
		}

		#endregion ShowKeyTipsTimer

		#endregion Public Properties

		#region Private Properties

		#region MinKeyTipWidth
		private object MinKeyTipWidth
		{
			get
			{
				if (this._minKeyTipWidth == null)
					this._minKeyTipWidth = this.Ribbon.CalcKeyTipMinWidth();

				return this._minKeyTipWidth;
			}
		} 
		#endregion //MinKeyTipWidth

		#region ParentAdorners

		private Stack<List<KeyTipAdorner>> ParentAdorners
		{
			get
			{
				if (this._parentAdorners == null)
					this._parentAdorners = new Stack<List<KeyTipAdorner>>();

				return this._parentAdorners;
			}
		}

		#endregion ParentAdorners

		#region ParentContainers

		private Stack<IKeyTipContainer> ParentContainers
		{
			get
			{
				if (this._parentContainers == null)
					this._parentContainers = new Stack<IKeyTipContainer>();

				return this._parentContainers;
			}
		}

		#endregion ParentContainers

		#region ParentKeyTips

		private Stack<KeyTipCollection> ParentKeyTips
		{
			get
			{
				if (this._parentKeyTips == null)
					this._parentKeyTips = new Stack<KeyTipCollection>();

				return this._parentKeyTips;
			}
		}

		#endregion ParentKeyTips

		#endregion Private Properties

		#endregion Properties

		#region Event Handlers

		// MD 10/3/06 - BR16378
		#region OnShowKeyTipsTimerTick

		private void OnShowKeyTipsTimerTick(object sender, EventArgs e)
		{
			// MD 10/11/06 - BR16609
			// Only show the key tips after th edelay if the form is still the active form
			//if ( this.IsActive == false )
			if (this.Ribbon.Mode == RibbonMode.KeyTipsPending)
			{
				this.ShowKeyTips();
				// AS 10/8/07 Not used
				//this._ignoreNextHiding = true;
			}

			// MD 10/11/06 - BR16609
			// After the key tips show, the delay timer should stop running
			this.ShowKeyTipsTimer.IsEnabled = false;
		}

		#endregion OnShowKeyTipsTimerTick

		#endregion Event Handlers
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