using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Windows.Markup;
using System.Collections.ObjectModel;


namespace Infragistics.Windows.Themes



{

	// JJD 5/22/07 - Added
	#region WashMode enum

	/// <summary>
	/// Determines the method used to wash colors. 
	/// </summary>
	/// <seealso cref="ResourceWasher"/>
	/// <seealso cref="ResourceWasher.WashMode"/>
	public enum WashMode
	{
		/// <summary>
		/// Blends each of the RGB color values with the corresponding wash color values. 
		/// </summary>
		SoftLightBlend = 0,

		/// <summary>
		/// Replaces the hue and saturation values with the corresponding values from the wash color but retains the brightness value.
		/// </summary>
		HueSaturationReplacement = 1,
	}

	#endregion //WashMode enum


    /// <summary>
	/// The ResourceWasher is a WPF resource dictionary that is used to clone brushes, pens and color resources defined in another �source resource dictionary� and optional wash their colors.  It exposes properties to specify the �source resource dictionary�, the default wash color along with a collection of wash groups. It also registers 2 attached properties that can be set on a Brush to specify its group name and whether its colors should be washed at all.
	/// </summary>
    /// <seealso cref="AutoWash"/>
	/// <seealso cref="IsExcludedFromWashProperty"/>
	/// <seealso cref="SourceDictionary"/>
	/// <seealso cref="WashColor"/>
	/// <seealso cref="WashGroupProperty"/>
	/// <seealso cref="WashGroups"/>


#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

	public class ResourceWasher : ResourceDictionary, ISupportInitialize
	{
		#region Private Members

		private bool _autoWash = true;
		private bool _themeWashInitialized;
		// AS 12/4/07
		// Changed from isInitialized to isInitializing and then changed the usage. I did this because
		// we don't want to skip autowashing just because someone didn't use the isupportinitialize
		// interface. Really what we want to prevent is unnecessary washing while we are being initialized
		// so we only need to know that we are in the middle of initializing.
		//
		private bool _isInitializing;
		private ResourceDictionary _sourceDictionary;
		private ResourceDictionary _clonedDictionary;
		private Color _washColor = Colors.Transparent;
		private WashMode _washMode = WashMode.SoftLightBlend;
		private WashGroupCollection _washGroups;
		private float _hue;
		private float _saturation;

        private Dictionary<ResourceDictionary, ResourceDictionary> _originalDictioanries;

		// AS 10/6/11 TFS90865
		// We need to be able to get from the original brush/pen/etc that is referenced 
		// in a setter to the clone of the resource before it was washed. I was going to 
		// use the originalDictionary but in SL the key can only be string or Type.
		//
		private Dictionary<object, object> _originalToCloneTable;

		#endregion //Private Members	
    
		#region Constructor

		/// <summary>
		/// Constructor
		/// </summary>
		public ResourceWasher()
		{
            this._originalDictioanries = new Dictionary<ResourceDictionary, ResourceDictionary>();
			this._originalToCloneTable = new Dictionary<object, object>();
		}

		#endregion //Constructor	

		#region Properties

			#region Attached Properties

				#region IsExcludedFromWash


		/// <summary>
		/// Identifies the IsExcludedFromWash attached dependency property
		/// </summary>
		/// <remarks>
		/// <para class="body">Attached property for Brushes and Pens. When set to true will prevent the brushes' colors from being washed.</para>
        /// </remarks>
        /// <seealso cref="GetIsExcludedFromWash"/>
        /// <seealso cref="SetIsExcludedFromWash"/>


#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		

		public static readonly DependencyProperty IsExcludedFromWashProperty = DependencyProperty.RegisterAttached("IsExcludedFromWash",
			typeof(bool), typeof(ResourceWasher), new FrameworkPropertyMetadata(false));






		/// <summary>
		/// Gets the value of the 'IsExcludedFromWash' attached property
		/// </summary>
		/// <remarks>
		/// <para class="body">Attached property for Brushes and Pens. When set to true will prevent the brushes' colors from being washed.</para>
        /// </remarks>
		/// <seealso cref="IsExcludedFromWashProperty"/>
		/// <seealso cref="SetIsExcludedFromWash"/>


#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		

		[AttachedPropertyBrowsableForType(typeof(Brush))]
		[AttachedPropertyBrowsableForType(typeof(Pen))]

		public static bool GetIsExcludedFromWash(DependencyObject d)
		{
			return (bool)d.GetValue(ResourceWasher.IsExcludedFromWashProperty);
		}


		/// <summary>
		/// Sets the value of the 'IsExcludedFromWash' attached property
		/// </summary>
		/// <remarks>
		/// <para class="body">Attached property for Brushes and Pens. When set to true will prevent the brushes' colors from being washed.</para>
        /// </remarks>
		/// <seealso cref="IsExcludedFromWashProperty"/>
		/// <seealso cref="GetIsExcludedFromWash"/>


#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		public static void SetIsExcludedFromWash(DependencyObject d, bool value)
		{
			d.SetValue(ResourceWasher.IsExcludedFromWashProperty, value);
		}

				#endregion //IsExcludedFromWash

				#region WashGroup


        /// <summary>
		/// Identifies the WashGroup attached dependency property
		/// </summary>
		/// <remarks>
        /// <para class="body">Attached property for Brushes and Pens to identify which group the brush or pen belongs to.</para>
        /// </remarks>
		/// <seealso cref="GetWashGroup"/>
		/// <seealso cref="SetWashGroup"/>
		/// <seealso cref="IsExcludedFromWashProperty"/>
		/// <seealso cref="WashGroups"/>
		/// <seealso cref="WashGroup"/>


#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

		

		public static readonly DependencyProperty WashGroupProperty = DependencyProperty.RegisterAttached("WashGroup",
			typeof(string), typeof(ResourceWasher), new FrameworkPropertyMetadata(null));






		/// <summary>
		/// Gets the value of the 'WashGroup' attached property
		/// </summary>
		/// <remarks>
        /// <para class="body">Attached property for Brushes and Pens to identify which group the brush or pen belongs to.</para>
        /// </remarks>
		/// <seealso cref="WashGroupProperty"/>
		/// <seealso cref="SetWashGroup"/>
		/// <seealso cref="IsExcludedFromWashProperty"/>
		/// <seealso cref="WashGroups"/>
		/// <seealso cref="WashGroup"/>


#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

		

		[AttachedPropertyBrowsableForType(typeof(Brush))]
		[AttachedPropertyBrowsableForType(typeof(Pen))]

		public static string GetWashGroup(DependencyObject d)
		{
			return (string)d.GetValue(ResourceWasher.WashGroupProperty);
		}


		
        /// <summary>
		/// Sets the value of the 'WashGroup' attached property
		/// </summary>
		/// <remarks>
        /// <para class="body">Attached property for Brushes and Pens to identify which group the brush or pen belongs to.</para>
        /// </remarks>
		/// <seealso cref="WashGroupProperty"/>
		/// <seealso cref="GetWashGroup"/>
		/// <seealso cref="IsExcludedFromWashProperty"/>
		/// <seealso cref="WashGroups"/>
		/// <seealso cref="WashGroup"/>


#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

		
		public static void SetWashGroup(DependencyObject d, string value)
		{
			d.SetValue(ResourceWasher.WashGroupProperty, value);
		}

				#endregion //WashGroup

            #endregion //Attached Properties

       #region Public Properties

       #region AutoWash

       /// <summary>
		/// Gets/sets whether the wash will be triggered automatically.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// If true will call the <see cref="WashResources"/> method automatically on ISupportInitialize.EndInit or afterward when any of the <see cref="SourceDictionary"/>, <see cref="WashColor"/> or <see cref="WashGroup"/> property values has changed.
		/// </para>
		/// </remarks>
		//[Description("Determines whether the wash will be triggered automatically.")]
		//[Category("Behavior")]
		[DefaultValue(true)]
		public bool AutoWash
		{
			get { return this._autoWash; }
			set
			{
				if (value != this._autoWash)
				{
					// AS 12/3/07
					this.VerifyCanBeModified();

					this._autoWash = value;
					this.OnCriteriaChanged();
				}
			}
		}

				#endregion //AutoWash	
    
				#region SourceDictionary

		/// <summary>
		/// Gets/sets adictionary that contains the resources to clone. 
		/// </summary>
		//[Description("A dictionary that contains the resources to clone.")]
		//[Category("Data")]
		[DefaultValue(null)]
		public ResourceDictionary SourceDictionary
		{
			get { return this._sourceDictionary; }
			set
			{
				if (value != this._sourceDictionary)
				{
					// AS 12/3/07
					this.VerifyCanBeModified();

					this._sourceDictionary = value;
					this.OnCriteriaChanged();
				}
			}
		}

				#endregion //SourceDictionary	
    
				#region WashColor

		/// <summary>
		/// Gets/sets the default color to use to wash the resources in the SourceDictionary 
		/// </summary>
		/// <remarks>
		/// <para class="body">The color to use to wash any resources not identified to a group in the <see cref="WashGroups"/> collection</para>
		/// <para class="note"><b>Note:</b> if this property is left to its default value of <b>Colors.Transparent</b> then any affected resources will not be cloned and washed. Instead they will be copied over without cloning or modification.</para>
		/// </remarks>
		/// <seealso cref="WashGroups"/>
		/// <seealso cref="WashGroup"/>
		//[Description("The default color to use to wash the resources in the SourceDictionary")]
		//[Category("Appearance")]
		public Color WashColor
		{
			get { return this._washColor; }
			set
			{
				if (value != this._washColor)
				{
					// AS 12/3/07
					this.VerifyCanBeModified();

					this._washColor = value;
					this._hue			= GetHue(value);
					this._saturation	= GetSaturation(value);

					this.OnCriteriaChanged();
				}
			}
		}

		/// <summary>
		/// Determines if the <see cref="WashColor"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeWashColor() { return this._washColor != Colors.Transparent; }

		/// <summary>
		/// Resets the <see cref="WashColor"/> property to its default state
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetWashColor()
		{
			this._washColor = Colors.Transparent;
		}

				#endregion //WashColor	
    
				#region WashMode

		/// <summary>
		/// Gets/sets the method used to wash colors in the resources in the SourceDictionary.
		/// </summary>
		/// <seealso cref="WashGroups"/>
		/// <seealso cref="WashGroup"/>
		/// <seealso cref="WashColor"/>
		//[Description("The method used to wash colors in the resources in the SourceDictionary")]
		//[Category("Appearance")]
		[DefaultValue(WashMode.SoftLightBlend)]
		public WashMode WashMode
		{
			get { return this._washMode; }
			set
			{
				if (value != this._washMode)
				{
					// AS 12/3/07
					this.VerifyCanBeModified();

                    if (!Enum.IsDefined(typeof(WashMode), value))
                    {

                        throw new InvalidEnumArgumentException("WashMode", (int)value, typeof(WashMode));



                    }

					this._washMode		= value;
					this.OnCriteriaChanged();
				}
			}
		}

				#endregion //WashMode	
    
				#region WashGroups

		/// <summary>
		/// Returns a collection of WashGroup objects.
		/// </summary>
		/// <seealso cref="WashGroup"/>
		public WashGroupCollection WashGroups
		{
			get
			{






				return this._washGroups;
			}
			set
			{
				if (this._washGroups != value)
				{
					// AS 12/3/07
					this.VerifyCanBeModified();

					//unwire the old collection's CollectionChanged event
					if ( this._washGroups != null )
						this._washGroups.CollectionChanged -= new System.Collections.Specialized.NotifyCollectionChangedEventHandler(OnWashGroupsCollectionChanged);
					
					this._washGroups = value;

					//wire up the new collection's CollectionChanged event
					if ( this._washGroups != null )
						this._washGroups.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(OnWashGroupsCollectionChanged);
	
					this.OnCriteriaChanged();
				}
			}
		}

		void OnWashGroupsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			this.OnCriteriaChanged();
		}

		/// <summary>
		/// Determines if the <see cref="WashGroups"/> property needs to be serialized.
		/// </summary>
		/// <returns>True if the property should be serialized</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerializeWashGroups() { return this._washGroups != null && this._washGroups.Count > 0; }

		/// <summary>
		/// Resets the <see cref="WashGroups"/> property to its default state
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void ResetWashGroups()
		{
			if (this._washGroups != null)
				this._washGroups.Clear();
		}

				#endregion //WashGroups	
    
			#endregion //Public Properties	
    
		#endregion //Properties

		#region Methods

			#region Public Methods

				#region BeginInit

		/// <summary>
		/// Begins the initialization process.
		/// </summary>
		public new void BeginInit()
		{
			this._isInitializing = true;

			base.BeginInit();

		}

				#endregion //BeginInit	
    
				#region EndInit

		/// <summary>
		/// Ends the initialization process.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note: </b> If the <see cref="AutoWash"/> property is true then the <see cref="WashResources"/> method will be called automatically by this method.</para>
		/// </remarks>
		public new void EndInit()
		{
			this._isInitializing = false;

			if (this._autoWash)
				this.WashResources();


			base.EndInit();

		}

				#endregion //EndInit	

				#region WashResources

		/// <summary>
		/// Clones and washes the resources from the <see cref="SourceDictionary"/>
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// Loops thru the source collection (and recursively thru its merged dictionaries) and clones any brushes that are not excluded from the wash via the attached <see cref="IsExcludedFromWashProperty"/>. It washes the cloned resources colors with the appropriate wash color. All other resources are just placed in the collection without cloning them.
		/// </para>
		/// </remarks>
		public void WashResources()
		{
			// AS 12/4/07
			// This flag is used to indicate if the themes have been initialized so we should
			// set that flag here when we actually wash the resources.
			//
			this._themeWashInitialized = true;

			if (this._clonedDictionary != null)
			{
				// remove the cloned dictionary from the merged dictionary so we don't trigger
				// a bunch of change notifications
				if (this.MergedDictionaries.Contains(this._clonedDictionary))
					this.MergedDictionaries.Remove(this._clonedDictionary);

				// clear the coned dictionary
				this._clonedDictionary.Clear();
				this._clonedDictionary.MergedDictionaries.Clear();
			}
			else
			{
				this._clonedDictionary = new ResourceDictionary();
			}

			// if we don't have a source dictionary then just return
			if (this._sourceDictionary == null)
				return;

			// If the wash color is transparent and there are no wash groups
			// just add the source dictionary to the cloned merged dictionary collection 
			if (this._washColor == Colors.Transparent &&
				// AS 2/23/12 TFS102032
				// Because we manipulate the values of the source dictionary we cannot simply use the 
				// source dictionary. We need to rewash if we have washed at least once which will end 
				// up using the original copies we made while processing the source the first time.
				//
				_originalDictioanries.Count == 0 &&
				 (this._washGroups == null || this._washGroups.Count == 0))
			{
				// AS 12/3/07
				// We cannot add the dictionary directly in case someone already added this to
				// the same resources collection to which this resource will be added.
				//
				//this._clonedDictionary.MergedDictionaries.Add(this._sourceDictionary);

				this._clonedDictionary.MergedDictionaries.Add(ThemeManager.Clone(this._sourceDictionary));



			}
			else
			{
				// AS 10/7/11 TFS91093
				// Setters of the source dictionary (or its merged dictionaries) may have styles as a 
				// value. If they do and that style is sealed, then they will continue to reference 
				// the sealed style. We need them to reference the "washed" style that we create so 
				// we need a dictionary that allows us to get from the style within the source 
				// dictionary to the "wrapper" one we create.
				//
				Dictionary<Style, Style> clonedStyles = new Dictionary<Style, Style>();

				this.WashResourcesHelper(this._sourceDictionary, this._clonedDictionary, clonedStyles);
			}

			// add the cloned dictionary to the merged dictionaries collection
			this.MergedDictionaries.Add(this._clonedDictionary);

		}

		private void WashResourcesHelper(ResourceDictionary source, ResourceDictionary target, Dictionary<Style, Style> clonedStyles)
        {
			// AS 10/7/11 TFS91093
			// Moved up the processing of the merged dictionaries.
			//
			// clear the target's cloned dictionaries
			target.MergedDictionaries.Clear();

			// loop over the source's merged dictionaries cloning each one
			foreach (ResourceDictionary mergedDictionary in source.MergedDictionaries)
			{
				ResourceDictionary clone = new ResourceDictionary();

				this.WashResourcesHelper(mergedDictionary, clone, clonedStyles);

				target.MergedDictionaries.Add(clone);
			}

            bool storeValues = false;

            // The first time we ever wash a RD, we should store off it's orignal values. 
            // That way, if a user changes the WashColor of an existing RD, we don't wash the
            // current colors, but the originals.
            ResourceDictionary originalDictionary = null;
            if (!this._originalDictioanries.ContainsKey(source))
            {
                originalDictionary = new ResourceDictionary();
                this._originalDictioanries.Add(source, originalDictionary);
                storeValues = true;
            }
            else
            {
                originalDictionary = this._originalDictioanries[source];
            }

            // Loop through the entire dictionary and identify all of the brushes
            Dictionary<object, Brush> brushKeysToUpdate = new Dictionary<object, Brush>();

            Dictionary<object, Pen> penKeysToUpdate = new Dictionary<object, Pen>();

            foreach (DictionaryEntry entry in source)
            {
                // If this is the first time, we're going to store off the brushes.
                if (storeValues) 
                {
					#region Store Values

                    Brush val = entry.Value as Brush;
                    if (val != null)
                    {                        
                        val = this.CloneBrush(val);
                        

						// AS 10/6/2011
						// To try and reduce overhead we should freeze this copy of the 
						// original brush/pen. This resource will only be used to obtain 
						// the original values so it should be safe to freeze them.
						//
						if (val.CanFreeze)
							val.Freeze();


                        originalDictionary.Add(entry.Key, val);

						// AS 10/6/2011 TFS90865
						// If this brush is referenced by a setter (e.g. via a staticresource) 
						// then we will need to be able to get from the original brush (which is 
						// all that we will have in the setter as there is no way to find out that 
						// it was a static resource or what the key of that was) to the clone 
						// of the brush that contains the original brush's values.
						//
						_originalToCloneTable[entry.Value] = val;
                    }
                    else
                    {

                        Pen pen = entry.Value as Pen;
                        if (pen != null)
                        {
                            string groupName = GetWashGroup(pen);
                            pen = pen.Clone();
                            SetWashGroup(pen, groupName);

							// AS 10/6/2011
							if (pen.CanFreeze)
								pen.Freeze();

                            originalDictionary.Add(entry.Key, pen);
                        }
                        else

                        {
							// AS 2/22/12 TFS102321
							var brushes = entry.Value as IList<Brush>;

							if (brushes != null && !brushes.IsReadOnly)
							{
								#region IList<Brush>

								var originalBrushes = new List<Brush>();

								foreach (Brush item in brushes)
								{
									var clonedItem = this.CloneBrush(item);


									// AS 10/6/2011
									// To try and reduce overhead we should freeze this copy of the 
									// original brush/pen. This resource will only be used to obtain 
									// the original values so it should be safe to freeze them.
									//
									if (clonedItem.CanFreeze)
										clonedItem.Freeze();


									originalBrushes.Add(clonedItem);
								}

								originalDictionary.Add(entry.Key, originalBrushes);

								#endregion //IList<Brush>
							}
							else
							{
								originalDictionary.Add(entry.Key, entry.Value);
							}
                        }
                    }
					#endregion //Store Values
                }

                object valueToAdd;

                valueToAdd = entry.Value;

                Brush brush = valueToAdd as Brush;
                if (brush != null)
                {
                    // Found a brush, update it and store it off.
                    brush = (Brush)originalDictionary[entry.Key];
                    valueToAdd = brush = this.WashBrushHelper(brush, valueToAdd);
                    brushKeysToUpdate.Add(entry.Key, brush);
                }
                else
                {

                    Pen pen = valueToAdd as Pen;
                    if (pen != null)
                    {
                        // Found a pen, update it and store it off.
                        pen = (Pen)originalDictionary[entry.Key];
						valueToAdd = pen = this.WashPenHelper(pen, valueToAdd);
                        penKeysToUpdate.Add(entry.Key, pen);
                    }
                    else

                    {
                        // We need to take styles into account, so that we can handle StaticResources
                        Style style = valueToAdd as Style;
                        if (style != null)
						{
							#region Style
							style = (Style)originalDictionary[entry.Key];
 
                            // First create a new style, as you can't modify existing
                            // styles once they've been applied.
							// AS 10/7/11 TFS91093
							// The style may have been (and will be if the key is a string) referenced 
							// within the resource dictionary. If we had encountered a setter that referenced 
							// this style then we want to get the style we created to wrap that setter 
							// value and use that here instead of creating a new style. In this way those 
							// styles referencing the original style will instead be referencing this 
							// "washed" style.
							//
							//Style newStyle = new Style(style.TargetType);
							//newStyle.BasedOn = style;

							Style newStyle;

							if (!clonedStyles.TryGetValue(style, out newStyle))
							{
								newStyle = new Style(style.TargetType);
								newStyle.BasedOn = style;
								clonedStyles[style] = newStyle;
							}

							// AS 10/10/11 TFS91068
							// The problem was that we added a setter for one of the BasedOn setters because its 
							// value was a brush but there was another setter for the property in the outer 
							// style that was a dynamic resource. In any case we don't need to include a setter 
							// for the inner/basedon styles if there was one for the outer. Note, I'm going over 
							// the setters backwards since in theory one can have multiple setters for the same 
							// property in which case the last one wins so that is the one we will log.
							//
							Dictionary<DependencyProperty, Setter> setterValues = new Dictionary<DependencyProperty, Setter>();

							while (style != null)
							{
								for (int i = style.Setters.Count - 1; i >= 0; i--)
								{
									var setter = style.Setters[i] as Setter;

									if (null != setter && null != setter.Property &&  !setterValues.ContainsKey(setter.Property))
										setterValues[setter.Property] = setter;
								}

								style = style.BasedOn;
							}

							// now just process the unique setter values
							foreach (var pair in setterValues)
							{
								var setter = pair.Value;

								// AS 2/23/12 TFS102032
								if (setter.Value is IResourceWasherTarget)
									this.InitializeResourceTarget(setter.Value as IResourceWasherTarget, storeValues);

                                // If the setter's value is a brush, 
                                // Store it off and update it, and add it to our new style
                                Brush styleBrush = setter.Value as Brush;
                                if (styleBrush != null)
                                {
									// AS 10/6/11 TFS90865
									// The value of the setter is likely to have been a staticresource 
									// to a brush in the resourcedictionary. In that case we would have 
									// manipulated the original brush and so the value of the original 
									// setter would contain the previously washed values (which if the 
									// style was in the application's resources would likely have been 
									// frozen so would contain the originally washed colors). We want to 
									// wash based on the original brush's values. If we have washed that 
									// brush (because it was in the rd) then we would have added it to 
									// the original dictionary above so we'll look for that and use 
									// that brush as the basis for the wash operation and use the 
									// value of the setter otherwise.
									//
									object clone;

									if (_originalToCloneTable.TryGetValue(styleBrush, out clone))
										styleBrush = (Brush)clone;

									Setter newSetter = new Setter();
                                    newSetter.Property = setter.Property;
                                    newSetter.Value = WashBrushHelper(styleBrush, setter.Value);
                                    newStyle.Setters.Add(newSetter);
                                }
								else if (setter.Value is Style)
								{
									// AS 10/7/11 TFS91093
									// if this is a style, it is likely the result of a staticresource 
									// to a style and probably has been or will be washed. at least in 
									// wpf the style will be sealed if it is added to the application's 
									// resources so we can't rely on the brushes within that style 
									// being washed. instead we have to rely on the fact that we will 
									// eventually be creating a new style that wraps that style and has 
									// washed setter values. so if we have encountered that style in 
									// this wash operation then it will be in our dictionary and we 
									// can get and use the replacement. if it hasn't then we'll create 
									// one and when we get to the style we'll use that instead of 
									// creating a new style (above when we hit a style as the value 
									// in the dictionary we are enumerating).
									//
									Style setterStyle = setter.Value as Style;
									Style washedStyle;

									if (!clonedStyles.TryGetValue(setterStyle, out washedStyle))
									{
										washedStyle = new Style(setterStyle.TargetType);
										washedStyle.BasedOn = setterStyle;
										clonedStyles[setterStyle] = washedStyle;
									}

									Setter newSetter = new Setter(setter.Property, washedStyle);
									newStyle.Setters.Add(newSetter);
								}


#region Infragistics Source Cleanup (Region)










































#endregion // Infragistics Source Cleanup (Region)

                            }

                            // We will use the newStyle now instead of the original
                            valueToAdd = newStyle;
							#endregion //Style
						}
						// AS 2/22/12 TFS102321
						else if (valueToAdd is IList<Brush>)
						{
							var brushes = valueToAdd as IList<Brush>;

							if (!brushes.IsReadOnly)
							{
								var originalBrushes = (IList<Brush>)originalDictionary[entry.Key];

								for (int i = 0, count = Math.Min(brushes.Count, originalBrushes.Count); i < count; i++)
								{
									brushes[i] = this.WashBrushHelper(originalBrushes[i], brushes[i]);
								}
							}
						}
                    }
                }

                // Update the new RD with the modified values.
                target.Add(entry.Key, valueToAdd);
            }

            // Now update the original source dictionary with the modified
            // brushes, so they also are displayed correctly.
            foreach (KeyValuePair<object, Brush> entry in brushKeysToUpdate)
            {
				Brush sourceBrush = (Brush)source[entry.Key];

				// AS 10/6/11 TFS90865
				// In Silverlight and even WPF, we want to copy the values of the 
				// modified brush back into the brush in the source dictionary. This 
				// is needed in case there were staticresources referencing the 
				// brushes since they would still maintain references to the 
				// original brush instances. The only thing that we should not do 
				// is try to manipulate a frozen brush so if the source is 
				// frozen we'll skip it.
				//

				if (sourceBrush.IsFrozen == false)

	                this.CopyBrush(entry.Value, sourceBrush);

            }


			foreach (KeyValuePair<object, Pen> entry in penKeysToUpdate)
			{
				Pen sourcePen = (Pen)source[entry.Key];

				// AS 10/6/11 TFS90865
				// Assuming the pen has not been frozen then we need to update the brush 
				// that it is referencing in case something has a staticresource 
				// referencing it.
				//
				if (sourcePen.IsFrozen == false)
					sourcePen.Brush = entry.Value.Brush;
			}


			// AS 10/7/11 TFS91093
			// This isn't absolutely necessary but technically the merged dictionaries 
			// should be processed first since things in this rd could be referencing 
			// them (e.g. if they used a staticresource to something within its merged 
			// dictionaries). Therefore I am moving this above the processing of the 
			// entries in the source dictionary.
			//
			//// clear the target's cloned dictionaries
			//target.MergedDictionaries.Clear();
			//
			//// loop over the source's merged dictionaries cloning each one
			//foreach (ResourceDictionary mergedDictionary in source.MergedDictionaries)
			//{
			//    ResourceDictionary clone = new ResourceDictionary();
			//
			//    this.WashResourcesHelper(mergedDictionary, clone);
			//
			//    target.MergedDictionaries.Add(clone);
			//}
        }
				#endregion //WashResources

			#endregion //Public Methods	
		
			#region Protected Methods

		/// <summary>
		/// Washes a color. 
		/// </summary>
		/// <param name="startColor">The starting color.</param>
		/// <param name="washColor">The color to wash the starting color with.</param>
		/// <param name="group">The associated wash group which may be null.</param>
		/// <returns></returns>
		protected virtual Color PerformColorWash(Color startColor, Color washColor, WashGroup group)
		{
			if (washColor == Colors.Transparent)
				return startColor;

			WashMode mode;

			// JJD 10/23/07 
			// Use the group setting 1st if supplied
			if (group != null && group.WashMode.HasValue)
				mode = group.WashMode.Value;
			else
				mode = this._washMode;

			// JJD 10/23/07 
			//if (this._washMode == WashMode.SoftLightBlend)
			if (mode == WashMode.SoftLightBlend)
			{
				int r = AdjustColorComponentValue(startColor.R, washColor.R);
				int g = AdjustColorComponentValue(startColor.G, washColor.G);
				int b = AdjustColorComponentValue(startColor.B, washColor.B);

				return Color.FromArgb(startColor.A, (byte)(r & 0xff), (byte)(g & 0xff), (byte)(b & 0xff));
			}

			float hue;
			float saturation;

			if (group != null)
			{
				hue = group.Hue;
				saturation = group.Saturation;
			}
			else
			{
				hue = this._hue;
				saturation = this._saturation;
			}

			return ColorFromHLS(startColor.A, hue, GetBrightness(startColor), saturation);
		}
			#endregion Protected Methods
		
			#region Internal Methods
    
				// AS 2/23/12 TFS102032
				// Helper method that the IResourceWasherTarget objects can use to obtain 
				// washed copies of resources. Letting the ResourceWasher do the logic is 
				// best because it already has tightly integrated logic for getting the 
				// wash group, wash color, etc. Plus it has the logic needed for processing 
				// a Style which we need - at least for the ResourceProviderWithOverrides<T>
				// 
				#region CreateWashedResource
		internal object CreateWashedResource(object value)
		{
			if (value is Brush)
				return this.WashBrushHelper(value as Brush, null);

			else if (value is Pen)
				return this.WashPenHelper(value as Pen, null);

			else if (value is Color)
				return this.WashColorHelper((Color)value, null);
			else if (value is ResourceDictionary)
			{
				ResourceDictionary clone = new ResourceDictionary();
				this.WashResourcesHelper(value as ResourceDictionary, clone, new Dictionary<Style, Style>());
				value = clone;
			}

			return value;
		}
				#endregion //CreateWashedResource

				// AS 5/7/08
				#region ForceAutoWashResources
		internal static void ForceAutoWashResources(ResourceDictionary rd)
		{
			ResourceWasher washer = rd as ResourceWasher;

			if (washer != null)
			{
				// we only need to do anything if autowash is false
				if (washer.AutoWash == false)
					washer.WashResources();
			}
			else
			{
				foreach (ResourceDictionary child in rd.MergedDictionaries)
					ForceAutoWashResources(child);
			}
		}
				#endregion //ForceAutoWashResources

				#region GetBrightness

		internal static float GetBrightness(Color color)
		{
			float redPercent = ((float)color.R) / 255f;
			float greenPercent = ((float)color.G) / 255f;
			float bluePercent = ((float)color.B) / 255f;

			float maxPercent = redPercent > greenPercent ? redPercent : greenPercent;

			if (bluePercent > maxPercent)
				maxPercent = bluePercent;

			float minPercent = redPercent < greenPercent ? redPercent : greenPercent;

			if (bluePercent < minPercent)
				minPercent = bluePercent;

			float sum = maxPercent + minPercent;

			return sum / 2;
		}

				#endregion //GetBrightness	
    
				#region GetHue

		internal static float GetHue(Color color)
		{
			float redPercent = ((float)color.R) / 255f;
			float greenPercent = ((float)color.G) / 255f;
			float bluePercent = ((float)color.B) / 255f;

			// if they are all the same return 0
			if (redPercent == greenPercent)
			{
				if (redPercent == bluePercent)
					return 0f;
			}

			float maxPercent = redPercent > greenPercent ? redPercent : greenPercent;

			if (bluePercent > maxPercent)
				maxPercent = bluePercent;

			float minPercent = redPercent < greenPercent ? redPercent : greenPercent;

			if (bluePercent < minPercent)
				minPercent = bluePercent;

			float diff = maxPercent - minPercent;
			float hue = 0f;

			if (redPercent == maxPercent)
			{
				hue = ((greenPercent - bluePercent) / diff) * 60;
			}
			else
				if (greenPercent == maxPercent)
				{
					hue = ((bluePercent - redPercent) / diff) * 60;
					hue += 120;
				}
				else
					if (bluePercent == maxPercent)
					{
						hue = ((redPercent - greenPercent) / diff) * 60;
						hue += 240;
					}

			// make sure the calulated value is from 0 to 360
			if (hue < 0f)
				hue += 360f;

			return hue;
		}

				#endregion //GetHue	
    
				#region GetSaturation

		internal static float GetSaturation(Color color)
		{
			float redPercent = ((float)color.R) / 255f;
			float greenPercent = ((float)color.G) / 255f;
			float bluePercent = ((float)color.B) / 255f;

			float maxPercent = redPercent > greenPercent ? redPercent : greenPercent;

			if (bluePercent > maxPercent)
				maxPercent = bluePercent;

			float minPercent = redPercent < greenPercent ? redPercent : greenPercent;

			if (bluePercent < minPercent)
				minPercent = bluePercent;

			double diff = maxPercent - minPercent;

			if (diff == 0)
				return 0f;

			double sum = (maxPercent + minPercent);
			double average = sum / 2d;

			if (average > .5d)
				return (float)(diff / (2d - sum));

			return (float)(diff / sum);
		}

				#endregion //GetSaturation	
    
				#region VerifyThemeAccess

		internal void VerifyThemeAccess()
		{
			// AS 12/4/07
			// If autowash is true then the resources will not get washed unless someone
			// happens to call the ISupportInitialize.EndInit. Really all we want to do
			// is to wash the resources if they have not yet been washed because we're
			// going to use them from the theming infrastructure. So we only need to check
			// the themewashinitialized flag and then make sure we set that flag when we
			// do the wash.
			//
			//if (this._themeWashInitialized ||
			//	 this._autoWash)
			//
			//this._themeWashInitialized = true;
			if (this._themeWashInitialized)
				return;

			this.WashResources();
		}

				#endregion //VerifyThemeAccess	

                #region Clone



#region Infragistics Source Cleanup (Region)




















#endregion // Infragistics Source Cleanup (Region)


                #endregion // Clone

            #endregion //Internal Methods

        #region Private Methods

        #region AdjustColorComponentValue

        private static int AdjustColorComponentValue(int startValue, int baseValue)
		{
			int multR = (startValue * baseValue) / 255;
			return multR + startValue * (255 - ((255 - startValue) * (255 - baseValue) / 255) - multR) / 255;
		}

				#endregion //AdjustColorComponentValue	
    
				#region ColorFromHLS



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		private static Color ColorFromHLS(byte alpha, float hue, float luminance, float saturation)
		{
			int red, green, blue;
			if (saturation == 0.0)
			{
				red = green = blue = (int)(luminance * 255.0);
			}
			else
			{
				float rm1, rm2;

				if (luminance <= 0.5f) rm2 = luminance + luminance * saturation;
				else rm2 = luminance + saturation - luminance * saturation;
				rm1 = 2.0f * luminance - rm2;
				red = (int)ToRGBHelper(rm1, rm2, hue + 120.0f);
				green = (int)ToRGBHelper(rm1, rm2, hue);
				blue = (int)ToRGBHelper(rm1, rm2, hue - 120.0f);
			}

			red = Math.Max(0, Math.Min(255, red));
			green = Math.Max(0, Math.Min(255, green));
			blue = Math.Max(0, Math.Min(255, blue));

			return Color.FromArgb(alpha, (byte)red, (byte)green, (byte)blue);
		}

				#endregion //ColorFromHLS	
		
				#region GetGroupFromName

		private WashGroup GetGroupFromName(string groupName)
		{
			if (groupName == null ||
				 groupName.Length == 0 ||
				this._washGroups == null)
				return null;

			int count = this._washGroups.Count;

			if (count == 0)
				return null;

			for (int i = 0; i < count; i++)
			{
				WashGroup group = this._washGroups[i];

				if (group.Name == groupName)
					return group;
			}

			return null;
		}

				#endregion //GetGroupFromName	
        
				// AS 12/18/07 Changed to nullable
				#region GetWashColor
		private Color GetWashColor(WashGroup group)
		{
			if (group != null && group.WashColor != null)
				return group.WashColor.Value;

			return this.WashColor;
		} 
				#endregion //GetWashColor

				// AS 2/23/12 TFS102032
				#region InitializeResourceTarget
		private void InitializeResourceTarget(IResourceWasherTarget target, bool storeValues)
		{
			DependencyObject d = target as DependencyObject;

			if (d != null)
			{

				if (d is Freezable && ((Freezable)d).IsFrozen)
					return;


				if (GetIsExcludedFromWash(d))
					return;
			}

			target.ResourceWasher = this;
		} 
				#endregion //InitializeResourceTarget

				#region OnCriteriaChanged

		private void OnCriteriaChanged()
		{
			if (this._isInitializing || this._autoWash == false)
			{
				this._themeWashInitialized = false;
				return;
			}

			this.WashResources();
		}

				#endregion //OnCriteriaChanged	

				#region ToRGBHelper



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		private static float ToRGBHelper(float rm1, float rm2, float rh)
		{
			if (rh > 360.0f) rh -= 360.0f;
			else if (rh < 0.0f) rh += 360.0f;

			if (rh < 60.0f) rm1 = rm1 + (rm2 - rm1) * rh / 60.0f;
			else if (rh < 180.0f) rm1 = rm2;
			else if (rh < 240.0f) rm1 = rm1 + (rm2 - rm1) * (240.0f - rh) / 60.0f;

			return (rm1 * 255);
		}

				#endregion //ToRGBHelper	

				#region VerifyCanBeModified
		private void VerifyCanBeModified()
		{
			// AS 5/7/08
			//if (this.IsReadOnly)
			//	throw new InvalidOperationException("ResourceDictionary is read-only and cannot be modified.");

			Utilities.VerifyCanBeModified(this);




		} 
				#endregion //VerifyCanBeModified

				#region WashColorHelper

		private Color WashColorHelper(Color startColor, WashGroup group)
		{
			
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

			Color wc = this.GetWashColor(group);

			return this.PerformColorWash(startColor, wc, group);
		}

				#endregion //WashColorHelper	

				#region WashBrushHelper

		private Brush WashBrushHelper(Brush sourceBrush, object sourceObject)
		{
			if (GetIsExcludedFromWash(sourceBrush))
				return sourceBrush;

			string groupName = GetWashGroup(sourceBrush);

			WashGroup group = this.GetGroupFromName(groupName);

			var brush = this.WashBrushHelper(sourceBrush, group);


			// AS 10/6/11
			// Added a parameter so we could determine if the object 
			// being cloned was frozen so we could determine whether we 
			// should freeze the brush being created.
			//
			var sourceFreezable = sourceObject as Freezable;

			if (null != sourceFreezable && sourceFreezable.IsFrozen)
				brush.Freeze();


			return brush;
		}

		private Brush WashBrushHelper(Brush sourceBrush, WashGroup group)
		{
			
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

			Color washColor = this.GetWashColor(group);

			if ( washColor == Colors.Transparent )
				return sourceBrush;


			Brush clone = sourceBrush.Clone();

			SolidColorBrush solid = clone as SolidColorBrush;

			if (solid != null)
			{
				solid.Color = this.WashColorHelper(solid.Color, group);
				return solid;
			}

			GradientBrush gradient = clone as GradientBrush;

			if (gradient != null)
			{
				GradientStopCollection stops = gradient.GradientStops;
				int count = stops.Count;

				for (int i = 0; i < count; i++)
				{
					GradientStop stop = stops[i];
					stop.Color = this.WashColorHelper(stop.Color, group);
				}

				return gradient;
			}

			Debug.Assert(clone is TileBrush, "Unknown Brush type in ResourceWasher");


			return clone;



		}
				#endregion //WashBrushHelper	

				#region WashPenHelper

		private Pen WashPenHelper(Pen sourcePen, object sourceObject)
		{
			if (GetIsExcludedFromWash(sourcePen))
				return sourcePen;

			string groupName = GetWashGroup(sourcePen);

			WashGroup group = this.GetGroupFromName(groupName);

			
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

			Color washColor = this.GetWashColor(group);

			if ( washColor == Colors.Transparent )
				return sourcePen;

			Pen clone = sourcePen.Clone();

			clone.Brush = WashBrushHelper(sourcePen.Brush, group);


			var sourceFreezable = sourceObject as Freezable;

			if (null != sourceFreezable && sourceFreezable.IsFrozen)
				clone.Freeze();


			return clone;
		}


				#endregion //WashPenHelper	

              #region CloneBrush


        private Brush CloneBrush(Brush sourceBrush)
        {
            return this.CloneBrush(sourceBrush, false, null);
        }

        private Brush CloneBrush(Brush sourceBrush, bool washColor, WashGroup group)
        {

            return sourceBrush.Clone();


#region Infragistics Source Cleanup (Region)














#endregion // Infragistics Source Cleanup (Region)

        }

              #endregion // CloneBrush

              #region CopyBrush


        private void CopyBrush(Brush sourceBrush, Brush destinationBrush)
        {
            this.CopyBrush(sourceBrush, destinationBrush, false, null);
        }

        private void CopyBrush(Brush sourceBrush, Brush destinationBrush, bool washColor, WashGroup group)
        {
            SolidColorBrush solidSource = sourceBrush as SolidColorBrush;

            SetWashGroup(destinationBrush, GetWashGroup(sourceBrush));
            SetIsExcludedFromWash(destinationBrush, GetIsExcludedFromWash(sourceBrush));

            if (solidSource != null)
            {
                SolidColorBrush solid = destinationBrush as SolidColorBrush;
                if (washColor)
                    solid.Color = this.WashColorHelper(solidSource.Color, group);
                else
                    solid.Color = solidSource.Color;
            }
            else
            {
                GradientBrush gradientSource = sourceBrush as GradientBrush;

                if (gradientSource != null)
                {
                    GradientBrush gradient = destinationBrush as GradientBrush;

                    if (gradient != null)
                    {
                        gradient.GradientStops.Clear();

                        GradientStopCollection stops = gradientSource.GradientStops;
                        int count = stops.Count;

                        for (int i = 0; i < count; i++)
                        {
                            GradientStop stop = stops[i];
                            GradientStop newStop = new GradientStop();
                            if (washColor)
                                newStop.Color = this.WashColorHelper(stop.Color, group);
                            else
                                newStop.Color = stop.Color;
                            newStop.Offset = stop.Offset;
                            gradient.GradientStops.Add(newStop);
                        }

                        if (gradientSource is LinearGradientBrush && destinationBrush is LinearGradientBrush)
                        {
                            LinearGradientBrush linearSourceBrush = (LinearGradientBrush)gradientSource;
                            LinearGradientBrush linearDestinationBrush = (LinearGradientBrush)destinationBrush;

                            linearDestinationBrush.StartPoint = linearSourceBrush.StartPoint;
                            linearDestinationBrush.EndPoint = linearSourceBrush.EndPoint;
                        }
                        else if (gradientSource is RadialGradientBrush && destinationBrush is RadialGradientBrush)
                        {
                            RadialGradientBrush radialSourceBrush = (RadialGradientBrush)gradientSource;
                            RadialGradientBrush radialDestinationBrush = (RadialGradientBrush)destinationBrush;

                            radialDestinationBrush.Center = radialSourceBrush.Center;
                            radialDestinationBrush.RadiusX = radialSourceBrush.RadiusX;
                            radialDestinationBrush.RadiusY = radialSourceBrush.RadiusY;
                            radialDestinationBrush.GradientOrigin = radialSourceBrush.GradientOrigin;
                        }
                    }
                }
            }
        }


                #endregion // CopyBrush

			#endregion //Private Methods

		#endregion //Methods

		#region ISupportInitialize Members

		void ISupportInitialize.BeginInit()
		{
			this.BeginInit();
		}

		void ISupportInitialize.EndInit()
		{
			this.EndInit();
		}

		#endregion

	}

	// AS 2/23/12 TFS102032
	internal interface IResourceWasherTarget
	{
		ResourceWasher ResourceWasher { get; set; }
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