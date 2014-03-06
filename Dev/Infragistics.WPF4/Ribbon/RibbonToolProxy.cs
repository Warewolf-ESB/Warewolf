using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Reflection;
using System.Windows.Markup.Primitives;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Collections;
using System.Diagnostics;
using Infragistics.Windows.Ribbon.Events;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Editors;
using Infragistics.Shared;
using System.Windows.Automation;

namespace Infragistics.Windows.Ribbon.Internal
{
	/// <summary>
	/// Base class used by implementors of custom tools to add support for advanced <see cref="XamRibbon"/> functionality such as the ability for a copy of the tool 
	/// to be added to <see cref="QuickAccessToolbar"/> or for a tool to appear in multiple locations on a <see cref="XamRibbon"/>, with tool state shared across all locations.  
	/// </summary>
	/// <remarks>
	/// <p class="body">The <b>RibbonToolProxy</b> class is the base class for custom tools in the <see cref="XamRibbon"/>. This class is used 
	/// to provide advanced tool functionality such as allowing the tool to be added to the <see cref="QuickAccessToolbar"/> and providing 
	/// synchronization of property values between multiple instance of the same logical tool.</p>
	/// <p class="note"><b>Note:</b> Developers of custom tool classes must derive from <see cref="RibbonToolProxy&lt;T&gt;"/> or if you are deriving 
	/// from an existing ribbon tool class (e.g. <see cref="ButtonTool"/>) then you should derive from the respective derived proxy class (e.g. 
	/// <see cref="ButtonTool.ButtonToolProxy"/>).</p>
	/// </remarks>
	/// <seealso cref="RibbonToolProxy&lt;T&gt;"/>
	/// <seealso cref="IRibbonTool"/>
	public abstract class RibbonToolProxy
	{
		#region Member Variables

		// AS 6/10/08
		private static Dictionary<Type, IList<DependencyProperty>> _cloneIgnoredProperties = new Dictionary<Type,IList<DependencyProperty>>();
		private static readonly object ClonePropertiesLock = new object();

		#endregion //Member Variables

		#region Constructor

		
		
		
		
		internal RibbonToolProxy()
		{
		} 
		#endregion //Constructor

		#region Properties

		#region Public Virtual Properties

			#region CanAddToQat
		/// <summary>
		/// Returns a boolean indicating whether the tool type can be added to the <see cref="QuickAccessToolbar"/>.
		/// </summary>
		public virtual bool CanAddToQat
		{
			get { return true; }
		}
			#endregion //CanAddToQat

		#region RetainFocusOnPerformAction
		/// <summary>
		/// Returns a boolean indicating whether the tool should retain focus when the <see cref="PerformAction"/>method is called. When false is returned, containing popups will be closed when the key tip is activated. By default, this returns true so that tools such as editor tools can be modified after they are activated.
		/// </summary>
		public virtual bool RetainFocusOnPerformAction
		{
			get { return true; }
		} 
		#endregion //RetainFocusOnPerformAction

		#endregion //Public Virtual Properties

		#endregion //Properties	
        
		#region Methods

		#region Public Abstract Methods
		/// <summary>
		/// Binds properties of the tool instance that implements the <see cref="IRibbonTool"/> interface to corresponding properties on the specified 
		/// source tool.  The specific properties that are bound are implementation details of the tool.  Generally, any property that 
		/// represents �tool state� and whose value is changeable and should be shared across instances of a given tool, should be bound 
		/// in tool�s implementation of this interface method.
		/// </summary>
		/// <param name="sourceTool">The tool that this tool is being bound to.</param>
		/// <param name="targetTool">The tool whose properties are being bound to the properties of <paramref name="sourceTool"/></param>
		public abstract void Bind(FrameworkElement sourceTool, FrameworkElement targetTool);

		/// <summary>
		/// Undoes the effects of .
		/// </summary>
		/// <param name="toolMenuItem">The container that wraps the tool.</param>
		/// <param name="tool">The tool that is being wrapped.</param>
		/// <seealso cref="PrepareToolMenuItem"/>
		public abstract void ClearToolMenuItem(ToolMenuItem toolMenuItem, FrameworkElement tool);

		/// <summary>
		/// Creates a copy of the specified tool element.
		/// </summary>
		/// <param name="sourceTool">The tool that is to be cloned.</param>
		/// <returns>A clone of the specified <paramref name="sourceTool"/></returns>
		public abstract FrameworkElement Clone(FrameworkElement sourceTool);

		/// <summary>
		/// Returns display mode for this tool when it is inside a menu.
		/// </summary>
		/// <param name="tool">The tool instance whose display mode is being queried.</param>
		public abstract ToolMenuItemDisplayMode GetMenuItemDisplayMode(FrameworkElement tool);

		
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)


		/// <summary>
		/// Returns true if the tool supports activation or false if the tool cannot be activated.
		/// </summary>
		/// <param name="tool">The tool instance whose activable state is being queried.</param>
		public abstract bool IsActivateable(FrameworkElement tool);

		/// <summary>
		/// Prepares the container <see cref="ToolMenuItem"/>to 'host' the tool.
		/// </summary>
		/// <param name="toolMenuItem">The container that wraps the tool.</param>
		/// <param name="tool">The tool that is being wrapped.</param>
		/// <seealso cref="ClearToolMenuItem"/>
		public abstract void PrepareToolMenuItem(ToolMenuItem toolMenuItem, FrameworkElement tool);

		/// <summary>
		/// Called by a <b>ToolMenuItem</b> when it is clicked.
		/// </summary>
		/// <param name="tool">The tool represented by the ToolMenuItem.</param>
		/// <remarks>
		/// <para class="body">The method is called from the <see cref="ToolMenuItem"/>'s OnClick method.</para>
		/// </remarks>
		/// <seealso cref="ToolMenuItem"/>
		public abstract void OnMenuItemClick(FrameworkElement tool);

		/// <summary>
		/// Called by the <b>Ribbon</b> to raise one of the common tool events. 
		/// </summary>
		/// <remarks>
		/// <para class="body">This method will be called to raise a commmon tool event, e.g. <see cref="ButtonTool.Cloned"/>, <see cref="ButtonTool.CloneDiscarded"/>.</para>
		/// <para class="note"><b>Note:</b> the implementation of this method calls a protected virtual method named <see cref="ButtonTool.OnRaiseToolEvent"/> that simply calls the RaiseEvent method. This allows derived classes the opportunity of adding custom logic.</para>
		/// </remarks>
		/// <param name="sourceTool">The tool for which the event should be raised.</param>
		/// <param name="args">The event arguments</param>
		/// <seealso cref="XamRibbon"/>
		/// <seealso cref="ToolClonedEventArgs"/>
		/// <seealso cref="ToolCloneDiscardedEventArgs"/>
		public abstract void RaiseToolEvent(FrameworkElement sourceTool, RoutedEventArgs args);

		/// <summary>
		/// Performs a tool's default action.
		/// </summary>
		/// <param name="tool">The tool whose action should be performed.</param>
		/// <returns>A boolean indicating whether the action was performed.</returns>
		public abstract bool PerformAction(FrameworkElement tool);

		#endregion //Public Abstract Methods

		#region Protected Virtual Methods
    
			#region ProcessKeyDown
		
#region Infragistics Source Cleanup (Region)






















































































#endregion // Infragistics Source Cleanup (Region)

			#endregion //ProcessKeyDown

		#endregion //Protected Virtual Methods

		#region Static Methods

		#region BindTool
		internal static void BindTool(FrameworkElement sourceTool, FrameworkElement targetTool)
		{
			IRibbonTool irt = targetTool as IRibbonTool;
			RibbonToolProxy proxy = irt != null ? irt.ToolProxy : null;

			if (null != proxy)
				proxy.Bind(sourceTool, targetTool);
		}
		#endregion //BindTool

		#region BindToolProperty


#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		internal static void BindToolProperties(DependencyProperty[] properties, FrameworkElement sourceTool, FrameworkElement destinationTool)
		{
			if (null == sourceTool)
				throw new ArgumentNullException("sourceTool");

			if (null == destinationTool)
				throw new ArgumentNullException("destinationTool");

			if (null == properties)
				throw new ArgumentNullException("properties");

			int count = properties.Length;

			for (int i = 0; i < count; i++)
			{
				DependencyProperty property = properties[i];

				if (null == property)
					continue;

				Binding b = new Binding();
				b.Source = sourceTool;
				b.Path = new PropertyPath(property);
				b.Mode = BindingMode.TwoWay;
				b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
				BindingOperations.SetBinding(destinationTool, property, b);
			}
		}
		#endregion //BindToolProperty

		#region CloneHelper
        
#region Infragistics Source Cleanup (Region)














































































































































































































#endregion // Infragistics Source Cleanup (Region)

#region Infragistics Source Cleanup (Region)






















#endregion // Infragistics Source Cleanup (Region)

		#endregion //CloneHelper

		#region CloneItems
        
#region Infragistics Source Cleanup (Region)
















































#endregion // Infragistics Source Cleanup (Region)

#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		internal static IList<object> CloneItems(IList items, bool bindRibbonTools, IList<FrameworkElement> clonedTools)
		{
			IList<object> newItems = new List<object>();
            ToolCloneManager manager = new ToolCloneManager(null, false, false, bindRibbonTools, clonedTools);

			foreach (object item in items)
			{
				// AS 10/1/09 TFS22197
				if (!manager.ShouldClone(item))
					continue;

                object newItem = manager.Clone(item);

				newItems.Add(newItem);
			}

			return newItems;
		} 
		#endregion //CloneItems

		#region GetValue
        
#region Infragistics Source Cleanup (Region)
















#endregion // Infragistics Source Cleanup (Region)

		#endregion //GetValue

		#region IsActivatableTool
		internal static bool IsActivatableTool(FrameworkElement tool)
		{
			IRibbonTool irt = tool as IRibbonTool;
			RibbonToolProxy proxy = irt != null ? irt.ToolProxy : null;

			return proxy != null && proxy.IsActivateable(tool);
		}
				#endregion //IsActivatableTool

		#region RaiseToolEvent
		internal static void RaiseToolEvent(IRibbonTool tool, RoutedEventArgs e)
		{
			RibbonToolProxy proxy = tool != null ? tool.ToolProxy : null;

			if (null != proxy)
				proxy.RaiseToolEvent(tool as FrameworkElement, e);
		}
		#endregion //RaiseToolEvent

		#region ReleaseToolClone
		internal static void ReleaseToolClone(FrameworkElement clonedTool)
		{
			// Raise the ToolCloneDiscarded event
			FrameworkElement clonedFromTool = XamRibbon.GetClonedFromTool(clonedTool);
			ToolCloneDiscardedEventArgs args = new ToolCloneDiscardedEventArgs(clonedTool, clonedFromTool);
			args.RoutedEvent = MenuTool.CloneDiscardedEvent;
			args.Source = clonedFromTool;

			RibbonToolProxy.RaiseToolEvent(clonedFromTool as IRibbonTool, args);

			RoutedEventManager.ClearAllEventHandlers(clonedTool);
		} 
		#endregion //ReleaseToolClone

		#endregion //Static Methods

		#region Internal

		// AS 6/10/08
		#region GetAllCloneIgnoredProperties
		internal static DependencyProperty[] GetAllCloneIgnoredProperties(Type proxyType)
		{
			List<DependencyProperty> props = new List<DependencyProperty>();
			IList<DependencyProperty> tempProps;

			lock (ClonePropertiesLock)
			{
				while (proxyType != null)
				{
					if (_cloneIgnoredProperties.TryGetValue(proxyType, out tempProps) && null != tempProps)
					{
						foreach (DependencyProperty prop in tempProps)
						{
							Debug.Assert(props.Contains(prop) == false);

							if (props.IndexOf(prop) < 0)
								props.Add(prop);
						}
					}

					proxyType = proxyType.BaseType;
				}
			}

			return props.ToArray();
		}
		#endregion //GetAllCloneIgnoredProperties

		#region GetKeyTipProviders
		internal IKeyTipProvider[] GetKeyTipProviders(FrameworkElement tool)
		{


#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

			
			return this.GetKeyTipProviders(tool, null);
		}

		internal virtual IKeyTipProvider[] GetKeyTipProviders(FrameworkElement tool, ToolMenuItem menuItem)
		{
			return new IKeyTipProvider[0];
		}
		#endregion //GetKeyTipProviders

		#region GetRootSourceTool
		/// <summary>
		/// Returns the original source tool from which the specified tool has been cloned.
		/// </summary>
		/// <param name="tool">The tool to evaluate</param>
		/// <returns><paramref name="tool"/> if the tool is not a clone otherwise the root clonedfromtool</returns>
		internal static FrameworkElement GetRootSourceTool(FrameworkElement tool)
		{
			FrameworkElement previous = null;

			while (null != tool)
			{
				previous = tool;
				tool = XamRibbon.GetClonedFromTool(tool);
			}

			return previous;
		} 
		#endregion //GetRootSourceTool

		// AS 6/10/08
		#region RegisterPropertiesToIgnore
		internal static void RegisterPropertiesToIgnore(Type proxyType, params DependencyProperty[] properties)
		{
			if (null != properties)
			{
				lock (ClonePropertiesLock)
				{
					Debug.Assert(false == _cloneIgnoredProperties.ContainsKey(proxyType));
					_cloneIgnoredProperties[proxyType] = properties;
				}
			}
		} 
		#endregion //RegisterPropertiesToIgnore

		#endregion //Internal
    
		#endregion //Methods

		#region Enumerations

			#region Public Enumerations

				// JM 11-08-07
				#region RecentItemFocusBehavior
		
#region Infragistics Source Cleanup (Region)


















#endregion // Infragistics Source Cleanup (Region)


				#endregion //RecentItemFocusBehavior

		#region ToolMenuItemDisplayMode

		/// <summary>
		/// Determines how a Ribbon tool will be displayed in a menu
		/// </summary>
		public enum ToolMenuItemDisplayMode
		{
			/// <summary>
			/// The Caption and LargeImage or SmallImage properties will be used to construct a standard menu item.
			/// </summary>
			Standard = 0,

			/// <summary>
			/// The LargeImage or SmallImage properties will be used to construct a standard menu. However, in the caption area the tool will be embedded. This is used primarilty for editor tools.
			/// </summary>
			EmbedToolInCaptionArea = 1,

			/// <summary>
			/// The tool itself will be used by itself. This is useful for e.g. with GalleryTools.
			/// </summary>
			UseToolForEntireArea = 2,
		}

				#endregion //ToolMenuItemDisplayMode

			#endregion //Public Enumerations

			#region Internal Enumerations

				#region CloneOptions
        
#region Infragistics Source Cleanup (Region)






















#endregion // Infragistics Source Cleanup (Region)

                #endregion //CloneOptions

            #endregion //Internal Enumerations

        #endregion //Enumerations

        // AS 11/13/07 BR28346
		// Added the CloneInfo class so we could pass more information along to the cloning process
		// in the future if needed.
		//
		#region CloneInfo
        
#region Infragistics Source Cleanup (Region)













































#endregion // Infragistics Source Cleanup (Region)

		#endregion //CloneInfo
	}

    // AS 9/22/08
    // I refactored the CloneHelper routine into a base CloneManager class and a 
    // derived ToolCloneManager class so we can reuse this logic elsewhere.
    //
    internal class ToolCloneManager : CloneManager
    {
        #region Member Variables

        private IList<DependencyProperty> _propertiesToSkip;
        private bool _skipRoot;
        private bool _cloneAndBindTools;
        private bool _cloneItems;
        private IList<FrameworkElement> _clonedTools;

        #endregion //Member Variables

        #region Constructor
        public ToolCloneManager(IList<DependencyProperty> propertiesToSkip, bool cloneItems, bool skipRoot, bool cloneAndBindTools, IList<FrameworkElement> clonedTools)
        {
            this._propertiesToSkip = propertiesToSkip;
            this._skipRoot = skipRoot;
            this._cloneAndBindTools = cloneAndBindTools;
            this._clonedTools = clonedTools;
            this._cloneItems = cloneItems;
        }

        #endregion //Constructor

        #region Base class overrides
        protected override bool ShouldSkipProperty(MarkupProperty property)
        {
            if (null != this._propertiesToSkip && property.DependencyProperty != null)
                return this._propertiesToSkip.Contains(property.DependencyProperty);

            return base.ShouldSkipProperty(property);
        }

        protected override object Clone(MarkupObject mo, CloneInfo cloneInfo)
        {
            // if we're supposed to clone & bind ribbon tools using the proxy...
            if (this._cloneAndBindTools)
            {
                // see if its an element
                FrameworkElement fe = mo.Instance as FrameworkElement;

                if (null != fe)
                {
                    // make sure we should be using the proxy - if this was called by 
                    // the proxy then we want to skip the root object
                    if (false == this._skipRoot || mo.Instance != cloneInfo.RootSource)
                    {
                        object clone;

                        // if its already been cloned then just return that clone
                        if (cloneInfo.TryGetClonedObject(fe, out clone))
                            return clone;

                        // AS 10/11/07 BR27304
                        // Use the root source tool as the tool to be cloned. In this way, we won't have to deal with
                        // something being removed from the "middle" of a chain. I.e. Tool A is root. Tool B is clone of 
                        // Tool A. Tool C is clone of Tool B. Tool B is removed.
                        //
                        fe = RibbonToolProxy.GetRootSourceTool(fe);

                        IRibbonTool irt = fe as IRibbonTool;
                        RibbonToolProxy proxy = irt != null ? irt.ToolProxy : null;

                        if (null != proxy)
                        {
                            FrameworkElement feClone = proxy.Clone(fe);

                            // store the object so we don't clone it again
                            cloneInfo.AddClone(mo.Instance, feClone);

                            // bind the tool
                            proxy.Bind(fe, feClone);

                            if (null != this._clonedTools)
                                this._clonedTools.Add(feClone);

                            return feClone;
                        }
                    }
                }
            }

            return base.Clone(mo, cloneInfo);
        }

        protected override void CloneItems(object clone, MarkupProperty mp, CloneInfo cloneInfo)
        {
            if (false == this._cloneItems)
            {
                // get the collection from the new object
                object collection = mp.PropertyDescriptor.GetValue(clone);
                IList list = collection as IList;

                // We don't want to clone the Items collection. That should be set in the Bind method instead
                if (list is ItemCollection && mp.Name == "Items")
                    return;
            }

            base.CloneItems(clone, mp, cloneInfo);
        }

		public override bool ShouldClone(object source)
		{
			// AS 10/1/09 TFS22197
			if (source is System.Windows.Interop.HwndHost)
				return false;

			return base.ShouldClone(source);
		}
        #endregion //Base class overrides
    }

	/// <summary>
	/// Represents a strongly typed <see cref="RibbonToolProxy"/>.
	/// </summary>
	/// <remarks>
	/// <p class="note">The abstract base methods of the base class have been overriden and sealed. The overriden methods 
	/// ensure that the parameters are non-null and that the tools specified for the various methods are of the 
	/// correct type. It then calls to virtual methods with parameters of the type based on generic type for which 
	/// the class has been created. For example, the <see cref="Bind(FrameworkElement, FrameworkElement)"/> has been 
	/// overriden and if the parameters are valid will call the virtual <see cref="Bind(T,T)"/> method.</p>
	/// </remarks>
	/// <seealso cref="RibbonToolProxy"/>
	/// <typeparam name="T">A type that derives from <see cref="FrameworkElement"/> and implements the <see cref="IRibbonTool"/> interface</typeparam>
	public abstract class RibbonToolProxy<T> : RibbonToolProxy
		where T : FrameworkElement, IRibbonTool
	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="RibbonToolProxy"/>
		/// </summary>
		protected RibbonToolProxy()
		{
		}

		static RibbonToolProxy()
		{
			// AS 6/10/08
			RibbonToolProxy.RegisterPropertiesToIgnore(typeof(RibbonToolProxy<T>),
				ValueEditor.IsInEditModeProperty

				// AS 7/10/09 TFS18328
				, FrameworkElement.ContextMenuProperty
				, FrameworkElement.ToolTipProperty
				, FrameworkElement.DataContextProperty


				// JM 10-4-10 TFS50129
				, Infragistics.Controls.Commanding.CommandProperty
				, Infragistics.Controls.Commanding.CommandsProperty

				);
		}
		#endregion //Constructor

		#region Methods

		#region Bind
		/// <summary>
		/// Binds properties of the target tool that implements the <see cref="IRibbonTool"/> interface to corresponding properties on the specified 
		/// source tool.  The specific properties that are bound are implementation details of the tool.  Generally, any property that 
		/// represents �tool state� and whose value is changeable and should be shared across instances of a given tool, should be bound 
		/// in tool�s implementation of this interface method.
		/// </summary>
		/// <param name="sourceTool">The tool that this tool is being bound to.</param>
		/// <param name="targetTool">The tool whose properties are being bound to the properties of <paramref name="sourceTool"/></param>
		public sealed override void Bind(FrameworkElement sourceTool, FrameworkElement targetTool)
		{
			T sourceT = GetToolAsType(sourceTool, "sourceTool");
			T targetT = GetToolAsType(targetTool, "targetTool");

			this.Bind(sourceT, targetT);
		}
		
		private static DependencyProperty[] _baseProperties = 
		{ 
			RibbonToolHelper.LargeImageProperty,
			RibbonToolHelper.SmallImageProperty,
			RibbonToolHelper.CaptionProperty,
			RibbonToolHelper.IsQatCommonToolProperty,
			UIElement.VisibilityProperty,
			UIElement.IsEnabledProperty,
			// AS 7/10/09 TFS18328
			FrameworkElement.DataContextProperty,
			FrameworkElement.ContextMenuProperty,
			FrameworkElement.ToolTipProperty,
		};

		/// <summary>
		/// Binds properties of the tool instance that implements the IRibbonTool interface to corresponding properties on the specified 
		/// source tool.  The specific properties that are bound are implementation details of the tool.  Generally, any property that 
		/// represents �tool state� and whose value is changeable and should be shared across instances of a given tool, should be bound 
		/// in tool�s implementation of this interface method.
		/// </summary>
		/// <param name="sourceTool">The tool that this tool is being bound to.</param>
		/// <param name="targetTool">The tool whose properties are being bound to the properties of <paramref name="sourceTool"/></param>
		protected virtual void Bind(T sourceTool, T targetTool)
		{
			RibbonToolProxy.BindToolProperties(_baseProperties, sourceTool, targetTool);
		} 
		#endregion //Bind

		#region ClearToolMenuItem

		/// <summary>
		/// Undoes the effects of PrepareToolMenuItem.
		/// </summary>
		/// <param name="toolMenuItem">The container that wraps the tool.</param>
		/// <param name="tool">The tool that is being wrapped.</param>
		/// <seealso cref="PrepareToolMenuItem(ToolMenuItem, FrameworkElement)"/>
		public sealed override void ClearToolMenuItem(ToolMenuItem toolMenuItem, FrameworkElement tool)
		{
			if (toolMenuItem == null)
				throw new ArgumentNullException("toolMenuItem");

			T sourceT = GetToolAsType(tool, "tool");

			this.ClearToolMenuItem(toolMenuItem, sourceT);
		}

		/// <summary>
		/// Undoes the effects of PrepareToolMenuItem.
		/// </summary>
		/// <param name="toolMenuItem">The container that wraps the tool.</param>
		/// <param name="tool">The tool that is being wrapped.</param>
		/// <seealso cref="PrepareToolMenuItem(ToolMenuItem, T)"/>
		protected virtual void ClearToolMenuItem(ToolMenuItem toolMenuItem, T tool)
		{
			// AS 5/4/10 TFS30711
			// I'm not sure why we used a two way binding between the Caption of a tool and the Header of the 
			// menu item but rather than change that to a OneWay binding, we'll just make sure that we clean 
			// up (i.e. remove) the binding before the base ClearContainerForItemOverride gets called. In 
			// WPF3, this wasn't an issue because there was no default impl for ClearContainerForItemOverride 
			// in ItemsControl. However now in the WPF 4, the ItemsControl reverses any changes that it made 
			// in the PrepareContainerForItemOverride. For ToolMenuItem this ultimately that calls 
			// ClearHeaderedItemsControl. In that routine they set the Header of the element to 
			// BindingExpressionBase.DisconnectedItem so the Caption ends up getting set to that value.
			//
			RibbonToolProxy.ToolMenuItemDisplayMode displayMode = this.GetMenuItemDisplayMode(tool);

			if (displayMode == ToolMenuItemDisplayMode.Standard)
			{
				BindingOperations.ClearBinding(toolMenuItem, ToolMenuItem.HeaderProperty);
			}
		}

		#endregion //ClearToolMenuItem	
    
		#region Clone

		
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)


		/// <summary>
		/// Creates a copy of the specified tool element.
		/// </summary>
		/// <param name="sourceTool">The tool that is to be cloned.</param>
		/// <returns>A clone of the specified <paramref name="sourceTool"/></returns>
		public sealed override FrameworkElement Clone(FrameworkElement sourceTool)
		{
			// AS 10/11/07 BR27304
			Debug.Assert(sourceTool == RibbonToolProxy.GetRootSourceTool(sourceTool), "The ToolInstanceManager is expecting that cloned tools are the root source tool. If this is not the case then we may not fix up the bindings when the source of this tool is removed.");
			sourceTool = RibbonToolProxy.GetRootSourceTool(sourceTool);

			T sourceT = GetToolAsType(sourceTool, "sourceTool");

			T cloneT = this.Clone(sourceT);

			
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)


			// copy the events
			bool copiedEvents = RoutedEventManager.CopyAllEventHandlers(sourceT, cloneT);

			// AS 10/11/07
			// Instead of doing this in other places (and also so that its set when we raise the clone event)
			// set the backward pointer now.
			//
			XamRibbon.SetClonedFromTool(cloneT, sourceT);

			// then raised the cloned event
			IRibbonTool irt = cloneT as IRibbonTool;
			RibbonToolProxy proxy = irt != null ? irt.ToolProxy : null;

			if (proxy != null)
			{
				// Raise the cloned event
				ToolClonedEventArgs args = new ToolClonedEventArgs(cloneT, sourceT, copiedEvents);
				args.RoutedEvent = MenuTool.ClonedEvent;
				args.Source = sourceT;
				proxy.RaiseToolEvent(sourceTool, args);
			}

			return cloneT;
		}

		/// <summary>
		/// Creates a copy of the specified tool element.
		/// </summary>
		/// <param name="sourceTool">The tool that is to be cloned.</param>
		/// <returns>A clone of the specified <paramref name="sourceTool"/></returns>
		protected virtual T Clone(T sourceTool)
		{
			
#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)

			// AS 6/10/08
			// Rather than have each type have to override the Clone method and also know to pick
			// up the dependency properties to be ignored by the base class, I've changed this
			// to a registration mechanism and then we just build the aggregate list once for
			// the type. The root issue being reported was that the CommandTarget property of a 
			// button tool was being cloned when it really should have been bound between the 
			// two tools. So the button tools (and menutool) had to not clone the command
			// related properties but instead bind them between the tool instances.
			//
			//return this.CloneHelper(sourceTool, _ignoreDuringCloneProperties);
			return this.CloneHelper(sourceTool, this.GetCloneIgnoredProperties());
		}

		// AS 11/13/07 BR28346
		// Added a helper method so derived classes could prevent cloning of certain properties.
		//
		private T CloneHelper(T sourceTool, IList<DependencyProperty> propertiesToSkip)
		{
            // AS 9/22/08
            // Refactored CloneHelper into new (Tool)CloneManager
            //
			//return (T)RibbonToolProxy.CloneHelper(sourceTool, false, new CloneInfo(CloneOptions.CloneAndBindRibbonTools, propertiesToSkip), null);
            object clone = new ToolCloneManager(propertiesToSkip, false, true, true, null).Clone(sourceTool);
			return (T)clone;
		}
		#endregion //Clone

		#region GetMenuItemDisplayMode

		/// <summary>
		/// Returns display mode for this tool when it is inside a menu.
		/// </summary>
		/// <param name="tool">The tool instance whose display mode is being queried.</param>
		public override sealed ToolMenuItemDisplayMode GetMenuItemDisplayMode(FrameworkElement tool)
		{
			T sourceT = GetToolAsType(tool, "tool");

			return this.GetMenuItemDisplayMode(sourceT);
		}

		/// <summary>
		/// Returns display mode for this tool when it is inside a menu.
		/// </summary>
		/// <returns>The default implementation returns 'Standard'.</returns>
		/// <param name="tool">The tool instance whose display mode is being queried.</param>
		protected virtual ToolMenuItemDisplayMode GetMenuItemDisplayMode(T tool)
		{
			return ToolMenuItemDisplayMode.Standard;
		}

		#endregion //GetMenuItemDisplayMode	

		// AS 6/10/08
		#region GetCloneIgnoredProperties
		private static DependencyProperty[] _ignoreDuringCloneProperties;

		private DependencyProperty[] GetCloneIgnoredProperties()
		{
			DependencyProperty[] props = _ignoreDuringCloneProperties;

			if (null == props)
			{
				props = _ignoreDuringCloneProperties = RibbonToolProxy.GetAllCloneIgnoredProperties(this.GetType());
			}

			return props;
		} 
		#endregion //GetCloneIgnoredProperties

		// JM 11-08-07
		#region GetRecentItemFocusBehavior
		
#region Infragistics Source Cleanup (Region)


















#endregion // Infragistics Source Cleanup (Region)

		#endregion //GetRecentItemFocusBehavior

		#region GetToolAsType
		private T GetToolAsType(FrameworkElement tool, string parameterName)
		{
			if (tool == null)
				throw new ArgumentNullException(parameterName);

			T toolT = tool as T;

			if (toolT == null)
				throw new InvalidOperationException(XamRibbon.GetString("LE_InvalidParameterType", parameterName, typeof(T)));

			return toolT;
		} 
		#endregion //GetToolAsType

		#region GetKeyTipProviders
		internal sealed override IKeyTipProvider[] GetKeyTipProviders(FrameworkElement tool, ToolMenuItem menuItem)
		{
			T toolT = GetToolAsType(tool, "tool");

			IKeyTipProvider[] providers = this.GetKeyTipProviders(toolT, menuItem);

			if (null == providers)
				providers = new IKeyTipProvider[0];

			return providers;
		}

		internal virtual IKeyTipProvider[] GetKeyTipProviders(T tool, ToolMenuItem menuItem)
		{
			return new IKeyTipProvider[] { new ToolKeyTipProvider(tool, menuItem) };
		}
		#endregion //GetKeyTipProviders

		#region IsActivateable
		/// <summary>
		/// Returns true if the tool supports activation or false if the tool cannot be activated.
		/// </summary>
		/// <param name="tool">The tool instance whose activable state is being queried.</param>
		public sealed override bool IsActivateable(FrameworkElement tool)
		{
			T sourceT = GetToolAsType(tool, "tool");

			return this.IsActivatable(sourceT);
		} 

		/// <summary>
		/// Returns true if the tool supports activation or false if the tool cannot be activated.  (read-only)
		/// </summary>
		/// <param name="tool">The tool instance whose activable state is being queried.</param>
		protected virtual bool IsActivatable(T tool)
		{
			return tool.IsEnabled;
		}
		#endregion //IsActivateable

		#region OnMenuItemClick

		/// <summary>
		/// Called by a <b>ToolMenuItem</b> when it is clicked.
		/// </summary>
		/// <param name="tool">The tool represented by the ToolMenuItem.</param>
		/// <remarks>
		/// <para class="body">The method is called from the <see cref="ToolMenuItem"/>'s OnClick method.</para>
		/// <para class="note"><b>Note:</b> the implementation of this method calls a strongly typed protected virtual method with the same name. This allows derived classes the opportunity of adding custom logic, e.g. raising the Click event for a <see cref="ButtonTool"/>.</para>
		/// </remarks>
		/// <seealso cref="ToolMenuItem"/>
		public sealed override void OnMenuItemClick(FrameworkElement tool)
		{
			T sourceT = GetToolAsType(tool, "tool");

			this.OnMenuItemClick(sourceT);
		}

		/// <summary>
		/// Called by a <b>ToolMenuItem</b> when it is clicked.
		/// </summary>
		/// <param name="tool">The tool represented by the ToolMenuItem.</param>
		/// <remarks>
		/// <para class="body">The method is called from the <see cref="ToolMenuItem"/>'s OnClick method.</para>
		/// </remarks>
		/// <seealso cref="ToolMenuItem"/>
		protected virtual void OnMenuItemClick(T tool)
		{
		}

		#endregion //OnMenuItemClick	

		#region PerformAction
		/// <summary>
		/// Performs a tool's default action.
		/// </summary>
		/// <param name="tool">The tool whose action should be performed.</param>
		/// <returns>A boolean indicating whether the action was performed.</returns>
		public override sealed bool PerformAction(FrameworkElement tool)
		{
			T toolT = GetToolAsType(tool, "tool");

			return PerformAction(toolT);
		}

		/// <summary>
		/// Performs a tool's default action.
		/// </summary>
		/// <param name="tool">The tool whose action should be performed.</param>
		/// <returns>A boolean indicating whether the action was performed.</returns>
		public virtual bool PerformAction(T tool)
		{
			// AS 10/3/07 BR27037
			// Focus the menu item if the tool is not visible.
			//
			//if (tool.Focusable && tool.IsVisible)
			//	return tool.Focus();
			// AS 6/10/08 BR32772
			
			
			
			
			//FrameworkElement elementToFocus = tool.Focusable && tool.IsVisible ? (FrameworkElement)tool : MenuToolBase.GetToolMenuItem(tool);
			FrameworkElement elementToFocus = tool.Focusable && tool.IsVisible ? (FrameworkElement)tool : (FrameworkElement)tool.GetValue(MenuToolBase.ToolMenuItemProperty);

			if (elementToFocus != null && elementToFocus.Focusable && elementToFocus.IsVisible)
			{
				return elementToFocus.Focus();
			}

			return false;
		} 
		#endregion //PerformAction

		#region PrepareToolMenuItem

		// AS 9/16/09 TFS19803
		private static DependencyProperty[] _menuItemCopyProperties = 
		{ 
			AutomationProperties.AcceleratorKeyProperty,
			AutomationProperties.AccessKeyProperty,
			AutomationProperties.AutomationIdProperty,
			AutomationProperties.HelpTextProperty,
			AutomationProperties.IsRequiredForFormProperty,
			AutomationProperties.ItemTypeProperty,
			AutomationProperties.NameProperty,
		};

		// AS 9/16/09 TFS19803
		private static DependencyProperty[] _menuItemBindProperties = 
		{ 
			AutomationProperties.ItemStatusProperty,
		};

		/// <summary>
		/// Prepares the container <see cref="ToolMenuItem"/>to 'host' the tool.
		/// </summary>
		/// <param name="toolMenuItem">The container that wraps the tool.</param>
		/// <param name="tool">The tool that is being wrapped.</param>
		/// <seealso cref="ClearToolMenuItem(ToolMenuItem, FrameworkElement)"/>
		public sealed override void PrepareToolMenuItem(ToolMenuItem toolMenuItem, FrameworkElement tool)
		{
			if (toolMenuItem == null)
				throw new ArgumentNullException("toolMenuItem");

			T sourceT = GetToolAsType(tool, "tool");

			this.PrepareToolMenuItem(toolMenuItem, sourceT);
		}
		/// <summary>
		/// Prepares the container <see cref="ToolMenuItem"/>to 'host' the tool.
		/// </summary>
		/// <param name="toolMenuItem">The container that wraps the tool.</param>
		/// <param name="tool">The tool that is being wrapped.</param>
		/// <seealso cref="ClearToolMenuItem(ToolMenuItem, T)"/>
		protected virtual void PrepareToolMenuItem(ToolMenuItem toolMenuItem, T tool)
		{
			RibbonToolProxy.ToolMenuItemDisplayMode displayMode = this.GetMenuItemDisplayMode(tool);

			// AS 10/4/07
			// Binding the Enabled state of the menu item to that of the tool.
			//
			// AS 11/9/07
			// We cannot do this when the element is embedded in the visual tree of the item because
			// when it becomes disabled, the menu item will be disabled. And then later when someone tries
			// to reenable the child, it cannot become enable because its parent is disabled. We'll do this
			// below for tools not being embedded.
			//
			//toolMenuItem.SetBinding(FrameworkElement.IsEnabledProperty, Utilities.CreateBindingObject(FrameworkElement.IsEnabledProperty, BindingMode.OneWay, tool));

			if (displayMode == RibbonToolProxy.ToolMenuItemDisplayMode.UseToolForEntireArea)
			{
				toolMenuItem.Style = ToolMenuItem.ItemOnlyStyle;

				// AS 6/9/08 BR32242
				// The DataContext of the ToolMenuItem is set by the ItemContainerGenerator
				// to the item when it created the container element. We need to propogate the
				// datacontext of the ribbon down the chain. Developers are not expecting
				// that we are using itemssource or datatemplate generated elements within 
				// the tree.
				//
				Binding b = new Binding();
				b.Path = new PropertyPath("(0).(1)", XamRibbon.RibbonProperty, FrameworkElement.DataContextProperty);
				b.RelativeSource = RelativeSource.Self;
				toolMenuItem.SetBinding(FrameworkElement.DataContextProperty, b);
			}
			else
			{
				
#region Infragistics Source Cleanup (Region)

































#endregion // Infragistics Source Cleanup (Region)

					
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

				toolMenuItem.SetBinding(ToolMenuItem.ImageSourceProperty, Utilities.CreateBindingObject(RibbonToolHelper.ImageResolvedProperty, BindingMode.OneWay, tool));

				if (displayMode == RibbonToolProxy.ToolMenuItemDisplayMode.EmbedToolInCaptionArea)
				{
					toolMenuItem.Header = tool;

					// AS 6/9/08 BR32242
					// The DataContext of the ToolMenuItem is set by the ItemContainerGenerator
					// to the item when it created the container element. We need to propogate the
					// datacontext of the ribbon down the chain. Developers are not expecting
					// that we are using itemssource or datatemplate generated elements within 
					// the tree.
					//
					Binding b = new Binding();
					b.Path = new PropertyPath("(0).(1)", XamRibbon.RibbonProperty, FrameworkElement.DataContextProperty);
					b.RelativeSource = RelativeSource.Self;
					toolMenuItem.SetBinding(FrameworkElement.DataContextProperty, b);
				}
				else
				{
					// Bind the Header to the Caption
					toolMenuItem.SetBinding(ToolMenuItem.HeaderProperty, Utilities.CreateBindingObject(RibbonToolHelper.CaptionProperty, BindingMode.TwoWay, tool));

					// AS 11/9/07
					// Moved from above. Only bind the enabled state if the tool 
					toolMenuItem.SetBinding(FrameworkElement.IsEnabledProperty, Utilities.CreateBindingObject(FrameworkElement.IsEnabledProperty, BindingMode.OneWay, tool));

					// AS 9/16/09 TFS19803
					foreach (DependencyProperty prop in _menuItemCopyProperties)
					{
						toolMenuItem.SetValue(prop, tool.GetValue(prop));
					}

					foreach (DependencyProperty prop in _menuItemBindProperties)
					{
						toolMenuItem.SetBinding(prop, Utilities.CreateBindingObject(prop, BindingMode.OneWay, tool));
					}
				}

                // AS 10/16/08 TFS6447
				toolMenuItem.SetBinding(FrameworkElement.ContextMenuProperty, Utilities.CreateBindingObject(FrameworkElement.ContextMenuProperty, BindingMode.OneWay, tool));

				if ( !(tool is MenuToolBase) )
					toolMenuItem.InputGestureText = MenuToolBase.GetInputGestureText(tool);
			}

		}

		#endregion //PrepareToolMenuItem	
    
		#region RaiseToolEvent
		/// <summary>
		/// Called by the <b>Ribbon</b> to raise one of the common tool events. 
		/// </summary>
		/// <remarks>
		/// <para class="body">This method will be called to raise a commmon tool event, e.g. <see cref="ButtonTool.Cloned"/>, <see cref="ButtonTool.CloneDiscarded"/>.</para>
		/// <para class="note"><b>Note:</b> the implementation of this method calls a protected virtual method named <see cref="ButtonTool.OnRaiseToolEvent"/> that simply calls the RaiseEvent method. This allows derived classes the opportunity of adding custom logic.</para>
		/// </remarks>
		/// <param name="sourceTool">The tool for which the event should be raised.</param>
		/// <param name="args">The event arguments</param>
		/// <seealso cref="XamRibbon"/>
		/// <seealso cref="ToolClonedEventArgs"/>
		/// <seealso cref="ToolCloneDiscardedEventArgs"/>
		public sealed override void RaiseToolEvent(FrameworkElement sourceTool, RoutedEventArgs args)
		{
			if (args == null)
				throw new ArgumentNullException("args");

			T sourceT = GetToolAsType(sourceTool, "sourceTool");

			this.RaiseToolEvent(sourceT, args);
		} 

		/// <summary>
		/// Called by the <b>Ribbon</b> to raise one of the common tool events. 
		/// </summary>
		/// <remarks>
		/// <para class="body">This method will be called to raise a commmon tool event, e.g. <see cref="ButtonTool.Cloned"/>, <see cref="ButtonTool.CloneDiscarded"/>.</para>
		/// <para class="note"><b>Note:</b> the implementation of this method calls a protected virtual method named <see cref="ButtonTool.OnRaiseToolEvent"/> that simply calls the RaiseEvent method. This allows derived classes the opportunity of adding custom logic.</para>
		/// </remarks>
		/// <param name="sourceTool">The tool for which the event should be raised.</param>
		/// <param name="args">The event arguments</param>
		/// <seealso cref="XamRibbon"/>
		/// <seealso cref="ToolClonedEventArgs"/>
		/// <seealso cref="ToolCloneDiscardedEventArgs"/>
		protected virtual void RaiseToolEvent(T sourceTool, RoutedEventArgs args)
		{
			sourceTool.RaiseEvent(args);
		}
		#endregion //RaiseToolEvent

		#endregion //Methods
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