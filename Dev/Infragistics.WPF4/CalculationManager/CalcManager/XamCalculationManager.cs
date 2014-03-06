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
using System.Windows.Automation.Peers;
using Infragistics.AutomationPeers;
using System.Windows.Threading;
using System.Threading;
using Infragistics.Collections;
using Infragistics.Controls;
using Infragistics.Controls.Primitives;
using System.Windows.Data;
using System.Linq;


using Infragistics.Windows.Licensing;


using Infragistics.Calculations;
using Infragistics.Calculations.Engine;
using Infragistics.Calculations.Primitives;

namespace Infragistics.Calculations
{

	/// <summary>
	/// Provides formula calculation functionality to controls.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// The <b>XamCalculationManager</b> creates a calculation network of objects and controls that provide source
	/// values for formula calculations as well as can be targets of formula calculation results.
	/// <list type="bullet">
	/// <listheader>The following types of objects can participate in the calculation network.</listheader>
	/// <item>
	/// A <see cref="NamedReference"/> specified via XamCalculationManager's <see cref="XamCalculationManager.NamedReferences"/>
	/// collection lets you define a value that can then be referred to in formulas of other objects using its name. A NamedReference
	/// can also have a formula associated with it in which case its value will be the formula calculation result.
	/// </item>
	/// <item>
	/// A control like a TextBox (or any DependencyObject) can be a source of values for other formulas as well as target of a
	/// formula calculation result. By default the control's Name (FrameworkElement.Name) is used to refer to it in formulas
	/// of other objects. To include a control in calculation network you need to specify XamCalculationManager's 
	/// <see cref="XamCalculationManager.CalculationManagerProperty"/> and <see cref="XamCalculationManager.ControlSettingsProperty"/> attached 
	/// properties on the control.
	/// </item>
	/// <item>
	/// XamDataPresenter's and XamDataGrid's fields/columns and their summaries can participate in calculation network. 
	/// A field/column/summary can be assigned a formula that derives its values from other field/column/summary values,
	/// as well as values of external controls and named references.
	/// </item>
	/// </list>
	/// </para>
	/// <para class="body">
	/// Note that formulas of objects in the calculation network can refer to values of other objects (even those
	/// objects that are also targets of other formula calculations) using their reference path, which usually
	/// is the name of the control or named reference or the name of the field/column/summary. The path used
	/// in formulas is referred to as reference path, which can be a relative path or an absoluate path.
	/// Relative path of controls and named references take the form of "[Name]" and absolute path takes the
	/// form of "[//Name]". In case of field/column/summary, relative path is again just the "[Name]" where as
	/// the absolute path includes name of the data presenter/grid along with other parts that fully identify
	/// a particular field/column/summary.
	/// </para>
	/// </remarks>
	/// <seealso cref="XamCalculationManager.NamedReferences"/>
	/// <seealso cref="XamCalculationManager.CalculationManagerProperty"/>
	/// <seealso cref="XamCalculationManager.ControlSettingsProperty"/>

	
	

	public class XamCalculationManager : FrameworkElement, ICalculationManager, ISupportPropertyChangeNotifications, IPropertyChangeListener



	{
		#region Member Variables


		private UltraLicense _license;


		private static readonly List<IFormulaProviderResolver> g_formulaProviderResolvers = new List<IFormulaProviderResolver>( );

		private UltraCalcEngine _calcEngine;
		private NamedReferenceCollection _namedReferences;
		private Dictionary<string, ICalculationParticipant> _participants = new Dictionary<string,ICalculationParticipant>( );
		private ControlReferenceManagerCollection _controlReferences;
		private HashSet<string> _circularFormulasCache;
		private PropertyChangeListenerList _listeners = new PropertyChangeListenerList( );
		internal static readonly PropertyChangeListenerList AttachedPropertyListeners = new PropertyChangeListenerList( );
		private readonly bool _isDerived;
		private bool _isCalcDirty;
		private bool _isInitialized;
		private int _calculationsSuspendedCount;
		private bool _inPerformCalculations;
		private DispatcherTimer _timer;

		#endregion //Member Variables

		#region Constructor

		static XamCalculationManager()
		{

			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(XamCalculationManager), new FrameworkPropertyMetadata(typeof(XamCalculationManager)));


			// MD 10/10/11 - TFS91199
			PerCederberg.Grammatica.Parser.Resources.SRCalc.ResourceCustomizer = 
				new PerCederberg.Grammatica.Parser.Resources.SRCalc.StringResourceCallback(XamCalculationManager.GetGrammaticaResourceString);
		}

		/// <summary>
		/// Initializes a new <see cref="XamCalculationManager"/>
		/// </summary>
		public XamCalculationManager()
		{
			_isDerived = typeof( XamCalculationManager ) != this.GetType( );


			// verify and cache the license
			//
			// Wrapped in a try/catch for a FileNotFoundException.
			// When the assembly is loaded dynamically, VS seems 
			// to be trying to reload a copy of Shared even though 
			// one is in memory. This generates a FileNotFoundException
			// when the dll is not in the gac and not in the AppBase
			// for the AppDomain.
			//
			try
			{
				// We need to pass our type into the method since we do not want to pass in 
				// the derived type.
				this._license = LicenseManager.Validate(typeof(XamCalculationManager), this) as UltraLicense;
			}
			catch (System.IO.FileNotFoundException) { }


			// SSP 5/10/12 TFS111115
			// Hook into the named references collection so the formula editor can get notified of changes to it.
			// 
			//_controlReferences = new ControlReferenceManagerCollection( this );
			PropertyChangeListenerList.ManageListenerHelper( ref _controlReferences, new ControlReferenceManagerCollection( this ), this, false );

			_calcEngine = new UltraCalcEngine( );
			_calcEngine.AddListener( this, false );

			this.NamedReferences = new NamedReferenceCollection( );

			// In WPF, OnInitialized will get called only when the control is in visual tree. Since
			// XamCalculationManager doesn't have any visual representation it will be common for it
			// to be used without being in visual tree. Therefore we cannot rely on OnInitialized
			// or BeginInit/EndInit to get called. Likewise in Silverlight ISupportInitialize.EndEdit will
			// only get called if corresponding BeginEdit was called. So we are setting _isInitialized to 
			// true in the constructor in case the control does go through some initialization process
			// then we are setting it to false in BeginEdit.
			// 
			_isInitialized = true;
		}

		#endregion //Constructor

		#region Base Class Overrides


		// For Silverlight we are implementing ISupportInitialize.

		#region BeginInit

		/// <summary>
		/// Called when the initialization starts.
		/// </summary>
		public override void BeginInit( )
		{
			_isInitialized = false;

			base.BeginInit( );
		} 

		#endregion // BeginInit


		#region OnInitialized


		/// <summary>
		/// Overridden.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnInitialized( EventArgs e )



		{

			base.OnInitialized( e );


			// SSP 10/19/11 TFS92057
			// Compile formulas asynchronously so any logic to register user defined functions in 
			// the constructor of the window will have had a chance to execute before we compile
			// the formulas.
			// 
			if ( null != this.Dispatcher )
				this.Dispatcher.BeginInvoke( new Action( this.InitializeAsyncCallback ) );
			else
				this.InitializeAsyncCallback( );
		}

		#endregion // OnInitialized 

		#endregion // Base Class Overrides

		#region Events

		#region CalculationsCompleted

		/// <summary>
		/// Used to invoke the <see cref="CalculationsCompleted"/> event.
		/// </summary>
		/// <param name="args">Event args for raising the event.</param>
		/// <seealso cref="CalculationsCompleted"/>
		protected virtual void OnCalculationsCompleted( EventArgs args )
		{
			EventHandler<EventArgs> handler = this.CalculationsCompleted;

			if ( null != handler )
				handler( this, args );
		}

		/// <summary>
		/// Raised after all calculations are complete.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>CalculationsCompleted</b> event is raised after all calculations are completed. Note that this event 
		/// will be raised again if a value of a reference that's a source of a formula calculation changes and causes
		/// one or more formulas to get recalculated.
		/// </para>
		/// </remarks>
		/// <seealso cref="OnCalculationsCompleted"/>
		public event EventHandler<EventArgs> CalculationsCompleted;

		private void RaiseCalculationsCompleted( )
		{
			if ( _isDerived || null != this.CalculationsCompleted )
			{
				this.OnCalculationsCompleted( EventArgs.Empty );
			}
		}

		#endregion // CalculationsCompleted

		#region FormulaCalculationError

		/// <summary>
		/// Used to invoke the <see cref="FormulaCalculationError"/> event.
		/// </summary>
		/// <param name="args">Event args for raising the event.</param>
		/// <seealso cref="FormulaCalculationError"/>
		protected virtual void OnFormulaCalculationError( FormulaCalculationErrorEventArgs args )
		{
			EventHandler<FormulaCalculationErrorEventArgs> handler = this.FormulaCalculationError;

			if ( null != handler )
				handler( this, args );
		}

		/// <summary>
		/// Raised when an error occurs while solving a formula.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>FormulaCalculationError</b> event is raised when solving a formula results in an error. The event arguments contain
		/// the error value to be set on the target of the formula. This error value can be changed but the new value will not affect
		/// other calculations which reference the erroneous formula.
		/// </para>
		/// </remarks>
		/// <seealso cref="FormulaCalculationErrorEventArgs"/>
		/// <seealso cref="OnFormulaCalculationError"/>
		public event EventHandler<FormulaCalculationErrorEventArgs> FormulaCalculationError;

		#endregion // FormulaCalculationError

		#region FormulaCircularityError

		/// <summary>
		/// Used to invoke the <see cref="FormulaCircularityError"/> event.
		/// </summary>
		/// <param name="args">Event args for raising the event.</param>
		/// <seealso cref="FormulaCircularityError"/>
		protected virtual void OnFormulaCircularityError( FormulaCircularityErrorEventArgs args )
		{
			EventHandler<FormulaCircularityErrorEventArgs> handler = this.FormulaCircularityError;

			if ( null != handler )
				handler( this, args );
		}

		/// <summary>
		/// Raised when a circular reference is detected in a formula.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>FormulaCircularityError</b> event is raised when it is determined that a formula references itself, either directly or indirectly.
		/// </para>
		/// </remarks>
		/// <seealso cref="FormulaCircularityErrorEventArgs"/>
		/// <seealso cref="OnFormulaCircularityError"/>
		public event EventHandler<FormulaCircularityErrorEventArgs> FormulaCircularityError;

		#endregion // FormulaCircularityError

		#region FormulaReferenceError

		/// <summary>
		/// Used to invoke the <see cref="FormulaReferenceError"/> event.
		/// </summary>
		/// <param name="args">Event args for raising the event.</param>
		/// <seealso cref="FormulaReferenceError"/>
		protected virtual void OnFormulaReferenceError( FormulaReferenceErrorEventArgs args )
		{
			EventHandler<FormulaReferenceErrorEventArgs> handler = this.FormulaReferenceError;

			if ( null != handler )
				handler( this, args );
		}

		/// <summary>
		/// Raised when a reference within a formula cannot be resolved.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>FormulaReferenceError</b> event is raised when a formula references a source which cannot be found in the calculation network.
		/// </para>
		/// </remarks>
		/// <seealso cref="FormulaReferenceErrorEventArgs"/>
		/// <seealso cref="OnFormulaReferenceError"/>
		public event EventHandler<FormulaReferenceErrorEventArgs> FormulaReferenceError;

		#endregion // FormulaReferenceError

		#region FormulaSyntaxError

		/// <summary>
		/// Used to invoke the <see cref="FormulaSyntaxError"/> event.
		/// </summary>
		/// <param name="args">Event args for raising the event.</param>
		/// <seealso cref="FormulaSyntaxError"/>
		protected virtual void OnFormulaSyntaxError( FormulaSyntaxErrorEventArgs args )
		{
			EventHandler<FormulaSyntaxErrorEventArgs> handler = this.FormulaSyntaxError;

			if ( null != handler )
				handler( this, args );
		}

		/// <summary>
		/// Raised when a formula has a syntax error.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>FormulaSyntaxError</b> event is raised when a formula is added to the calculation network which contains one or more errors.
		/// </para>
		/// </remarks>
		/// <seealso cref="FormulaSyntaxErrorEventArgs"/>
		/// <seealso cref="OnFormulaSyntaxError"/>
		public event EventHandler<FormulaSyntaxErrorEventArgs> FormulaSyntaxError;

		#endregion // FormulaSyntaxError

		#region ValueDirtied

		/// <summary>
		/// Used to invoke the <see cref="ValueDirtied"/> event.
		/// </summary>
		/// <param name="args">Event args for raising the event.</param>
		/// <seealso cref="ValueDirtied"/>
		protected virtual void OnValueDirtied( ValueDirtiedEventArgs args )
		{
			EventHandler<ValueDirtiedEventArgs> handler = this.ValueDirtied;

			if ( null != handler )
				handler( this, args );
		}

		/// <summary>
		/// Raised when a target value is dirty and needs to be recalculated.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>ValueDirtied</b> event is raised when a value needs to be recalculated due to various reasons, such a formula being 
		/// added or removed or referenced value being changed.
		/// </para>
		/// </remarks>
		/// <seealso cref="ValueDirtiedEventArgs"/>
		/// <seealso cref="OnValueDirtied"/>
		public event EventHandler<ValueDirtiedEventArgs> ValueDirtied;

		private void RaiseValueDirtiedEvent( object context, ValueDirtiedAction action )
		{
			if ( _isDerived || null != this.ValueDirtied )
			{
				var args = new ValueDirtiedEventArgs( context, action );
				this.OnValueDirtied( args );
			}
		}

		#endregion // ValueDirtied

		#endregion // Events

		#region Properties

		#region Public Properties

		#region AreCalculationsSuspended

		private static readonly DependencyPropertyKey AreCalculationsSuspendedPropertyKey = DependencyPropertyUtilities.RegisterReadOnly(
			"AreCalculationsSuspended",
			typeof( bool ),
			typeof( XamCalculationManager ),
			KnownBoxes.FalseBox,
			null
		);

		/// <summary>
		/// Identifies the read-only <see cref="AreCalculationsSuspended"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AreCalculationsSuspendedProperty = AreCalculationsSuspendedPropertyKey.DependencyProperty;

		/// <summary>
		/// Indicates whether calculations have been suspended via <see cref="SuspendCalculations"/> method.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>AreCalculationsSuspended</b> property returns <i>true</i> if calculations have been suspended using 
		/// <see cref="SuspendCalculations"/> method. Once the calculations are resumed using 
		/// <see cref="ResumeCalculations"/> method, this property will return <i>false</i>.
		/// </para>
		/// </remarks>
		/// <seealso cref="SuspendCalculations"/>
		/// <seealso cref="ResumeCalculations"/>
		public bool AreCalculationsSuspended
		{
			get
			{
				return _calculationsSuspendedCount > 0;
			}
			internal set
			{
				this.SetValue( AreCalculationsSuspendedPropertyKey, value );
			}
		}

		#endregion // AreCalculationsSuspended

		#region AsynchronousCalculationDuration

		/// <summary>
		/// Identifies the <see cref="AsynchronousCalculationDuration"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AsynchronousCalculationDurationProperty = DependencyPropertyUtilities.Register(
			"AsynchronousCalculationDuration",
			typeof( int ),
			typeof( XamCalculationManager ),
			DependencyPropertyUtilities.CreateMetadata( 50, new PropertyChangedCallback( OnAsynchronousCalculationDurationChanged ) )
		);

		private static void OnAsynchronousCalculationDurationChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			XamCalculationManager item = (XamCalculationManager)d;

			int duration = (int)e.NewValue;
			if ( duration < 0 )
				throw new ArgumentOutOfRangeException( SRUtil.GetString( "LE_Exception_2" ), SRUtil.GetString( "LE_Exception_3" ) );
		}

		/// <summary>
		/// The duration in milliseconds that the XamCalculationManager will use to perform calculations each time the <see cref="AsynchronousCalculationInterval"/> passes. Default value is 50.
		/// </summary>
		/// <remarks><para class="body">This property only applies when <see cref="CalculationFrequency"/> is set to <b>Asynchronous</b>.</para></remarks>
		/// <remarks><para class="body">AsynchronousCalculationDuration determines how much time is used by the CalculationManager to perform pending calculations. Each time the interval specified by <see cref="AsynchronousCalculationInterval"/> passes, the CalculationManager will begin performing any neccessary calculations. If there are still calculations pending when the time expires, then some calculations will be left in a dirty state until the next time <see cref="PerformCalculations"/> is called or the Interval passes again.</para></remarks>
		/// <remarks><para class="body">To force all calculations to be completed each time the Interval passes, set this property to 0.</para></remarks>
		/// <seealso cref="AsynchronousCalculationDurationProperty"/>
		/// <seealso cref="CalculationFrequency"/>
		/// <seealso cref="AsynchronousCalculationInterval"/>
		public int AsynchronousCalculationDuration
		{
			get
			{
				return (int)this.GetValue( AsynchronousCalculationDurationProperty );
			}
			set
			{
				this.SetValue( AsynchronousCalculationDurationProperty, value );
			}
		}

		#endregion // AsynchronousCalculationDuration

		#region AsynchronousCalculationInterval

		/// <summary>
		/// Identifies the <see cref="AsynchronousCalculationInterval"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AsynchronousCalculationIntervalProperty = DependencyPropertyUtilities.Register(
			"AsynchronousCalculationInterval",
			typeof( int ),
			typeof( XamCalculationManager ),
			DependencyPropertyUtilities.CreateMetadata( 10, new PropertyChangedCallback( OnAsynchronousCalculationIntervalChanged ) )
		);

		private static void OnAsynchronousCalculationIntervalChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			XamCalculationManager item = (XamCalculationManager)d;

			int interval = (int)e.NewValue;
			if ( interval <= 0 )
				throw new ArgumentOutOfRangeException( SRUtil.GetString("LE_Exception_4"), SRUtil.GetString("LE_Exception_5") );
		}

		/// <summary>
		/// The interval in milliseconds that the XamCalculationManager will wait between performing calculations. The default value is 10.
		/// </summary>
		/// <remarks>
		/// <p class="body">This property only applies when <see cref="CalculationFrequency"/> is set to <b>Asynchronous</b>.</p>
		/// <p class="body">AsynchronousCalculationInterval determines how much time elapses between calculations. Each time the Interval passes the CalculationManager will perform pending calculations for a period of time specified by <see cref="AsynchronousCalculationDuration"/>.</p>
		/// </remarks>
		/// <seealso cref="AsynchronousCalculationIntervalProperty"/>
		/// <seealso cref="CalculationFrequency"/>
		/// <seealso cref="AsynchronousCalculationDuration"/>
		public int AsynchronousCalculationInterval
		{
			get
			{
				return (int)this.GetValue( AsynchronousCalculationIntervalProperty );
			}
			set
			{
				this.SetValue( AsynchronousCalculationIntervalProperty, value );
			}
		}

		#endregion // AsynchronousCalculationInterval

		#region CalculationManager

		/// <summary>
		/// Identifies the CalculationManager attached dependency property.
		/// </summary>
		/// <seealso cref="GetCalculationManager"/>
		/// <seealso cref="SetCalculationManager"/>
		public static readonly DependencyProperty CalculationManagerProperty = DependencyPropertyUtilities.RegisterAttached(
			"CalculationManager",
			typeof( XamCalculationManager ),
			typeof( XamCalculationManager ),
			DependencyPropertyUtilities.CreateMetadata( null, new PropertyChangedCallback( OnCalculationManagerChanged ) )
		);

		private static void OnCalculationManagerChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			XamCalculationManager oldCalcManager = (XamCalculationManager)e.OldValue;
			XamCalculationManager newCalcManager = (XamCalculationManager)e.NewValue;

			ControlReferenceManager crm = null;
			ItemCalculatorElementBase calcElement = d as ItemCalculatorElementBase;

			if ( null != oldCalcManager )
			{
				if (calcElement != null)
				{
					if (calcElement.CalculationManager == oldCalcManager)
						calcElement.CalculationManager = null;
				}
				else
				{
					crm = oldCalcManager._controlReferences.GetItem(d);
					if (null != crm)
						oldCalcManager._controlReferences.Remove(crm);
				}

			}

			if ( null != newCalcManager )
			{
				if (calcElement != null)
				{
					calcElement.CalculationManager = newCalcManager;
				}
				else
				{
					if (null == crm)
						crm = new ControlReferenceManager(d);

					newCalcManager._controlReferences.Add(crm);
				}
			}

			// Notify the formula editor so it can re-get the formula provider for the control.
			// 
			NotifyAttachedPropertyListeners( d, e );
		}

		/// <summary>
		/// Gets the value of the attached CalculationManager DependencyProperty.
		/// </summary>
		/// <param name="d">The object whose value is to be returned</param>
		/// <seealso cref="CalculationManagerProperty"/>
		/// <seealso cref="SetCalculationManager"/>
		public static XamCalculationManager GetCalculationManager( DependencyObject d )
		{
			return (XamCalculationManager)d.GetValue( CalculationManagerProperty );
		}

		/// <summary>
		/// Sets the value of the attached CalculationManager DependencyProperty.
		/// </summary>
		/// <param name="d">The object whose value is to be modified</param>
		/// <param name="value">The new value</param>
		/// <seealso cref="CalculationManagerProperty"/>
		/// <seealso cref="GetCalculationManager"/>
		public static void SetCalculationManager( DependencyObject d, XamCalculationManager value )
		{
			d.SetValue( CalculationManagerProperty, value );
		}

		#endregion // CalculationManager

		#region ControlSettings

		/// <summary>
		/// Identifies the ControlSettings attached dependency property. Used to specify calculation settings for a control.
		/// </summary>
		/// <remarks>
		/// See remarks of <see cref="SetControlSettings"/> for more information.
		/// </remarks>
		/// <seealso cref="GetControlSettings"/>
		/// <seealso cref="SetControlSettings"/>
		public static readonly DependencyProperty ControlSettingsProperty = DependencyPropertyUtilities.RegisterAttached(
			"ControlSettings",
			typeof( ControlCalculationSettings ),
			typeof( XamCalculationManager ),
			DependencyPropertyUtilities.CreateMetadata( null, new PropertyChangedCallback( OnControlSettingsChanged ) )
		);

		private static void OnControlSettingsChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			XamCalculationManager calcManager = GetCalculationManager( d );
			if ( null != calcManager )
			{
				ControlReferenceManager crm = calcManager._controlReferences.GetItem( d );
				Debug.Assert( null != crm );
				if ( null != crm )
					crm.Settings = (ControlCalculationSettings)e.NewValue;
			}

			NotifyAttachedPropertyListeners( d, e );
		}

		/// <summary>
		/// Gets the value of the attached ControlCalculationSettings DependencyProperty.
		/// </summary>
		/// <param name="d">The object whose value is to be returned</param>
		/// <remarks>
		/// See remarks of <see cref="SetControlSettings"/> for more information.
		/// </remarks>
		/// <seealso cref="ControlSettingsProperty"/>
		/// <seealso cref="SetControlSettings"/>
		public static ControlCalculationSettings GetControlSettings( DependencyObject d )
		{
			return (ControlCalculationSettings)d.GetValue( ControlSettingsProperty );
		}

		/// <summary>
		/// Sets the value of the attached ControlCalculationSettings DependencyProperty. 
		/// Used to specify calculation settings for a control.
		/// </summary>
		/// <param name="d">The object whose value is to be modified</param>
		/// <param name="value">The new value</param>
		/// <remarks>
		/// <para class="body">
		/// You can specify <see cref="ControlCalculationSettings"/> object which contains various calculation
		/// related settings, including the formula, for a control using this attached property. In order to
		/// have a control participate in calculations, you need to set the <see cref="CalculationManagerProperty"/>
		/// attached property on the control and optionally set the CalculationControlSettings attached property
		/// on the control. Setting CalculationControlSettings is necessary if Name is not set on the control
		/// or if you want to specify what property to use for retrieving or setting the value on the control. 
		/// Formula is also exposed on the CalculationControlSettings so if you want to assign a formula to
		/// the control you need to set this attached property as well.
		/// </para>
		/// </remarks>
		/// <seealso cref="ControlSettingsProperty"/>
		/// <seealso cref="GetControlSettings"/>
		/// <seealso cref="ControlCalculationSettings"/>
		public static void SetControlSettings( DependencyObject d, ControlCalculationSettings value )
		{
			d.SetValue( ControlSettingsProperty, value );
		}

		#endregion // ControlCalculationSettings

		#region Result

		private static readonly DependencyPropertyKey ResultPropertyKey = DependencyPropertyUtilities.RegisterAttachedReadOnly(
			"Result",
			typeof( CalculationResult ),
			typeof( XamCalculationManager ),
			null,
			new PropertyChangedCallback( OnResultChanged )
		);

		/// <summary>
		/// Identifies the read-only ControlResult attached dependency property. Used to retrieve calculation
		/// result of a control that has a formula assigned to it.
		/// </summary>
		/// <seealso cref="GetResult"/>
		public static readonly DependencyProperty ResultProperty = ResultPropertyKey.DependencyProperty;

		private static void OnResultChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			NotifyAttachedPropertyListeners( d, e );
		}

		/// <summary>
		/// Gets the calculation result of a control with a formula.
		/// </summary>
		/// <param name="d">The object whose value is to be returned</param>
		/// <remarks>
		/// <para class="body">
		/// <b>ControlResult</b> is used to get the result of the formula that's specified on a control via
		/// <see cref="ControlSettingsProperty"/> attached property.
		/// </para>
		/// </remarks>
		/// <seealso cref="ResultProperty"/>
		/// <seealso cref="CalculationResult"/>
		/// <seealso cref="ControlSettingsProperty"/>
		/// <seealso cref="ControlCalculationSettings.Formula"/>
		public static CalculationResult GetResult( DependencyObject d )
		{
			return (CalculationResult)d.GetValue( ResultProperty );
		}

		internal static void SetResult( DependencyObject d, CalculationResult value )
		{
			d.SetValue( ResultPropertyKey, value );
		}

		#endregion // Result

		#region CalculationFrequency

		/// <summary>
		/// Identifies the <see cref="CalculationFrequency"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CalculationFrequencyProperty = DependencyPropertyUtilities.Register(
			"CalculationFrequency",
			typeof( CalculationFrequency ),
			typeof( XamCalculationManager ),
			DependencyPropertyUtilities.CreateMetadata( CalculationFrequency.Asynchronous, new PropertyChangedCallback( OnCalculationFrequencyChanged ) )
		);

		private static void OnCalculationFrequencyChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			XamCalculationManager item = (XamCalculationManager)d;

		}

		/// <summary>
		/// Gets/sets the frequency with which the <see cref="XamCalculationManager"/> automatically performs calculations.
		/// </summary>
		/// <remarks>
		/// <p class="body">The CalculationFrequency property determines when calculations are performed. They can be performed asynchronously on a timer, synchronously every time a value changes, or manually only when the <see cref="XamCalculationManager.PerformCalculations"/> method is explicitly called in code.</p>
		/// <p class="body">By default, calculations are done <b>Asynchronously</b> on a timer. This ensures that the UI thread is not locked up when long or complex calculations are performed. The disadvantage to this mode is that a particular value may not be calculated when it is requested in code. When retrieving a calculated value using Asynchronous mode, you should call the <see cref="ICalculationManager.EnsureCalculated"/> method to ensure the value has been calculated.</p>
		/// <p class="body"><b>Synchronous</b> mode will updated all calculations immediately any time a source value of a calculation changes. Unlike Asynchronous mode, values can always be depended upon to be calculated when asked for. The disadvantage of this mode is that long or complex calculations may lock the UI thread until they are complete.</p>
		/// <p class="body">In <b>Manual</b> mode, no calculations are performed by the <see cref="XamCalculationManager"/> until the code explicitly calls a method to do so, such as <see cref="XamCalculationManager.PerformCalculations"/> or <see cref="ICalculationManager.EnsureCalculated"/>. This allows you complete control over when calculations take place.</p>
		/// </remarks>
		/// <seealso cref="AsynchronousCalculationDuration"/>
		/// <seealso cref="AsynchronousCalculationInterval"/>
		public CalculationFrequency CalculationFrequency
		{
			get
			{
				return (CalculationFrequency)this.GetValue( CalculationFrequencyProperty );
			}
			set
			{
				this.SetValue( CalculationFrequencyProperty, value );
			}
		}

		#endregion // CalculationFrequency

		#region DeferredCalculationsEnabled

		/// <summary>
		/// Identifies the <see cref="DeferredCalculationsEnabled"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DeferredCalculationsEnabledProperty = DependencyPropertyUtilities.Register(
			"DeferredCalculationsEnabled",
			typeof( bool ),
			typeof( XamCalculationManager ),
			DependencyPropertyUtilities.CreateMetadata( KnownBoxes.TrueBox, new PropertyChangedCallback( OnDeferredCalculationsEnabledChanged ) )
		);

		private static void OnDeferredCalculationsEnabledChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			XamCalculationManager calcManager = (XamCalculationManager)d;
			calcManager._calcEngine.EnableMarkedColumns = (bool)e.NewValue;
		}

		/// <summary>
		/// Gets / Sets whether certain calculations can be deferred to optimize calculation of visible objects. Default value is <b>True</b>.
		/// </summary>
		/// <remarks>
		/// <para class="body">When true, certain calculations will be deferred if they are not visible. This is mainly used for iterated calculations, such as XamDataGrid cells. If possible, cells that are in view will be calculated before cells that are out of view.</para>
		/// <para class="note">Note that in some cases, deferring calculations is not possible. For example, if there is a Summary on a calculated field, then all the values in the field must be calculated before the summary can be calculated.</para>
		/// </remarks>
		public bool DeferredCalculationsEnabled
		{
			get
			{
				return (bool)this.GetValue( DeferredCalculationsEnabledProperty );
			}
			set
			{
				this.SetValue( DeferredCalculationsEnabledProperty, value );
			}
		}

		#endregion // DeferredCalculationsEnabled

		#region NamedReferences

		/// <summary>
		/// Identifies the <see cref="NamedReferences"/> dependency property
		/// </summary>
		public static readonly DependencyProperty NamedReferencesProperty = DependencyPropertyUtilities.Register(
			"NamedReferences",
			typeof( NamedReferenceCollection ),
			typeof( XamCalculationManager ),
			DependencyPropertyUtilities.CreateMetadata( null, new PropertyChangedCallback( OnNamedReferencesChanged ) )
		);

		private static void OnNamedReferencesChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			XamCalculationManager calcManager = (XamCalculationManager)d;

			if ( null != calcManager._namedReferences )
				calcManager._namedReferences.UnregisterAllReferences( );

			// SSP 5/10/12 TFS111115
			// Hook into the named references collection so the formula editor can get notified of changes to it.
			// 
			//calcManager._namedReferences = (NamedReferenceCollection)e.NewValue;
			PropertyChangeListenerList.ManageListenerHelper( ref calcManager._namedReferences, (NamedReferenceCollection)e.NewValue, calcManager, false );

			if ( null != calcManager._namedReferences )
				calcManager._namedReferences.InitializeCalcManager( calcManager );
		}

		/// <summary>
		/// Specifies a collection of named references.
		/// </summary>
		/// <seealso cref="NamedReferencesProperty"/>
		/// <remarks>
		/// <para class="body">
		/// A Named reference allows you to define a value that can be referenced in formulas of other objects using
		/// its name, rather than having to repeat the same value in multiple formulas. Furthermore, a named reference
		/// can be a formula as well that refers to other named references and references in the calculation network.
		/// </para>
		/// <para class="body">
		/// Note that <i>NamedReferences</i> property returns <i>null</i> by default. You need to set it to an instance
		/// of <see cref="NamedReferenceCollection"/>.
		/// </para>
		/// </remarks>
		/// <seealso cref="NamedReference"/>
		/// <seealso cref="NamedReferenceCollection"/>
		public NamedReferenceCollection NamedReferences
		{
			get
			{
				return _namedReferences;
			}
			set
			{
				this.SetValue( NamedReferencesProperty, value );
			}
		}

		#endregion // NamedReferences

		#endregion // Public Properties 

		#region Internal Properties

		#region IsInitialized

		/// <summary>
		/// Indicates if the calc manager has been initialized.
		/// </summary>

		internal new bool IsInitialized



		{
			get
			{
				return _isInitialized;
			}
		}

		#endregion // IsInitialized 

		#endregion // Internal Properties

		#endregion // Properties

		#region Methods

		#region Static

		#region RegisterResources

		/// <summary>
		/// Adds an additonal Resx file in which the control will pull its resources from.
		/// </summary>
		/// <param name="name">The name of the embedded resx file that contains the resources to be used.</param>
		/// <param name="assembly">The assembly in which the resx file is embedded.</param>
		/// <remarks>Don't include the extension of the file, but prefix it with the default Namespace of the assembly.</remarks>
		public static void RegisterResources(string name, System.Reflection.Assembly assembly)
		{
#pragma warning disable 436
			SR.AddResource(name, assembly);
#pragma warning restore 436
		}

		#endregion // RegisterResources

		#region UnregisterResources

		/// <summary>
		/// Removes a previously registered resx file.
		/// </summary>
		/// <param name="name">The name of the embedded resx file that was used for registration.</param>
		/// <remarks>
		/// Note: this won't have any effect on controls that are already in view and are already displaying strings.
		/// It will only affect any new controls created.
		/// </remarks>
		public static void UnregisterResources(string name)
		{
#pragma warning disable 436
			SR.RemoveResource(name);
#pragma warning restore 436
		}

		#endregion // UnregisterResources

		#endregion // Static

		#region Public Methods

		#region CalculateFormula

		/// <summary>
		/// Calculates the specified formula and returns the value.
		/// </summary>
		/// <remarks>
		/// <p class="body">The Calculate method allows you to calculate a formula once without creating a <see cref="NamedReference"/>.</p>
		/// <p class="body">This method does not ensure that any dependants of the specified formula are calculated. When using a <see cref="CalculationFrequency"/> of <b>Asynchronous</b>, you should call the <see cref="XamCalculationManager.PerformCalculations"/> method before calling this method.</p>
		/// <p class="body">This method throws an exception if the formula has a syntax error. Any other kind of error is returned as an instance of <see cref="CalculationValue"/> that represents the error that occurred.</p>
		/// </remarks>
		/// <param name="formula">Formula to calculate.</param>
		/// <returns>The result of the formula.</returns>
		public CalculationValue CalculateFormula( string formula )
		{
			FormulaCalculationReference reference = new FormulaCalculationReference( this, formula );

			ICalculationFormula calcFormula = reference.Formula;

			if ( calcFormula.HasSyntaxError )
				throw new CalculationException( SRUtil.GetString( "LER_Exception_34", calcFormula.SyntaxError ) );

			return calcFormula.Evaluate( reference );
		}

		/// <summary>
		/// Similar to <see cref="CalculateFormula( string )"/> except that the references in the specified formula can also be
		/// properties of the specified data item.
		/// </summary>
		/// <param name="dataItem">Data item which can provide source values for formulas.</param>
		/// <param name="formula">Formula.</param>
		/// <returns>Returns the result of the formula.</returns>
		/// <remarks>
		/// As an example, let's say the data item exposes "A" and "B" public properties. The formula can be "[A] + [B]".
		/// </remarks>
		public CalculationValue CalculateFormula<T>( T dataItem, string formula )
		{
			ItemCalculator itemCalculator = new ItemCalculator( )
			{
				CalculationManager = this
			};

			itemCalculator.Calculations.Add(
					new ItemCalculation( )
					{
						ReferenceId = "Result",
						Formula = formula
					}
			);

			itemCalculator.Item = dataItem;
			itemCalculator.EnsureCalculated( );
			CalculationValue result = itemCalculator.Results["Result"].CalculationValue;

			itemCalculator.Item = null;
			itemCalculator.Calculations.Clear( );

			return result;
		}

		#endregion // CalculateFormula

		#region CalculateListFormula

		/// <summary>
		/// Similar to <see cref="CalculateFormula&lt;T&gt;( T, string )"/> except that the source of calculation values is a collection of data items.
		/// </summary>
		/// <param name="items">Collection of data item which can provide source values for formulas.</param>
		/// <param name="formula">Formula.</param>
		/// <returns>Returns the result of the formula.</returns>
		/// <remarks>
		/// As an example, let's say <typeparam name="T" /> exposes "A" and "B" public properties. The formula can be "avg([A]) + avg([B])".
		/// </remarks>
		public CalculationValue CalculateListFormula<T>( IList<T> items, string formula )
		{
			ListCalculator listCalculator = new ListCalculator( )
			{
				CalculationManager = this
			};

			listCalculator.ListCalculations.Add( 
				new ListCalculation( )
				{
					ReferenceId = "Result",
					Formula = formula
				}
			);

			listCalculator.ItemsSource = items;

			listCalculator.EnsureCalculated( );
			CalculationValue result = listCalculator.ListResults["Result"].CalculationValue;

			listCalculator.ItemsSource = null;
			listCalculator.ListCalculations.Clear( );

			return result;
		}

		#endregion // CalculateListFormula

		#region DirtyAllFormulas

		/// <summary>
		/// Dirties all the formulas so they get recalculated.
		/// </summary>
		/// <remarks>
		/// <p class="body">This method will cause the <see cref="XamCalculationManager"/> to mark all existing formulas as dirty. This means that the next time a calculation operation is performed, all formula will be recalculated. When the calculation occurs depends on the <see cref="CalculationFrequency"/> property.</p>
		/// </remarks>
		public void DirtyAllFormulas( )
		{
			_calcEngine.DirtyAllFormulas( );
		}

		#endregion // DirtyAllFormulas

		#region GetAllFunctions

		/// <summary>
		/// Returns all the functions that are available for use in formulas. It includes any user defined functions as well.
		/// </summary>
		public IEnumerable<CalculationFunction> GetAllFunctions( )
		{
			UltraCalcFunctionFactory factory = _calcEngine.FunctionFactory;

			return new TypedEnumerable<CalculationFunction>( factory );
		}

		#endregion GetAllFunctions 

		#region PerformCalculations

		/// <summary>
		/// Forces recalculation of all dirtied items in the Calculation Network.
		/// </summary>
		/// <param name="maxTimeInMillis">Number of milliseconds to allow for calculations before control returns to the caller (to wait indefinitely, specify 0.)</param>
		/// <returns>Returns a boolean indicating if anything in the calculation network remains dirty.</returns>
		/// <remarks>
		/// <p class="body">Developers can use this method to limit the amount of time in the time slice calcmanager spends
		/// performing it's calculations.  If all calculations have been completed before this time interval elapses, control
		/// returns immediately to the caller.  Setting this interval too small can lead to excessive task-switching that
		/// would make the calculation complete more slowly than it ordinarily would have completed, therefore developers
		/// are advised to allot as much time as the user-responsiveness of the application permits.</p>
		/// </remarks>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the value supplied for <i>millis</i>
		/// is either less than 0, or too great to convert into a number of clock ticks.  The precise upper bound
		/// of <i>millis</i> may vary depending upon the granularity of the machine's hardware clock, but it exceeds
		/// any amount of time which would be a sensible argument (instead, use 0 to indicate an indefinite wait).
		/// </exception>
		public bool PerformCalculations( long maxTimeInMillis = 0 )
		{
			CoreUtilities.ValidateIsNotNegative( maxTimeInMillis, "maxTimeInMillis" );

			if ( !_inPerformCalculations && !this.AreCalculationsSuspended )
			{
				_inPerformCalculations = true;

				try
				{
					// Calc engine's Recalc takes -1 for infinite.
					// 
					long ticks = 0 == maxTimeInMillis ? -1 : Utils.ConvertMillisecondsToTicks( maxTimeInMillis );

					_isCalcDirty = _calcEngine.Recalc( ticks );

					if ( !_isCalcDirty )
						this.RaiseCalculationsCompleted( );
				}
				finally
				{
					_inPerformCalculations = false;
				}
			}

			return _isCalcDirty;
		}

		#endregion // PerformCalculations

		#region RegisterUserDefinedFunction

		/// <summary>
		/// Registers a single <see cref="CalculationFunction"/> instance.
		/// </summary>
		/// <param name="userDefinedFunction">User defined function instance to register</param>
		/// <returns>Returns true if the type was registered successfully, else false if the registration failed</returns>
		/// <remarks>
		/// <p class="body">
		/// Users can build custom functions used in formulas by sub-classing the <see cref="CalculationFunction"/> class.  
		/// Once the derived class is instaniated it must be registered by using the <b>AddFunction</b> method before being avilable and referenced by a formulas.
		/// Users can build a library of functions packaged in an assembly and register all the functions within the assembly by using the <b>AddFunctionLibrary</b> method.
		/// </p>
		/// </remarks>
		public bool RegisterUserDefinedFunction( CalculationFunction userDefinedFunction )
		{
			return _calcEngine.AddFunction( userDefinedFunction );
		}

		/// <summary>
		/// This overload is to bring attention to the CustomCalculationFunction which is a helper method that lets
		/// the developer construct a custom function with minimal effort by supplying a name and a delegate that does
		/// the calculations. See <see cref="CustomCalculationFunction"/> for more information.
		/// </summary>
		/// <param name="userDefinedFunction"></param>
		/// <returns></returns>
		public bool RegisterUserDefinedFunction( CustomCalculationFunction userDefinedFunction )
		{
			return this.RegisterUserDefinedFunction( (CalculationFunction)userDefinedFunction );
		}

		#endregion RegisterUserDefinedFunction

		#region RegisterUserDefinedFunctionLibrary

		/// <summary>
		/// Registers an assembly containing <see cref="CalculationFunction"/> derived types.
		/// </summary>
		/// <param name="assembly">Loaded assembly to register.</param>
		/// <returns>Returns true if the assembly was registered successfully, else false if the registration failed.</returns>
		/// <remarks>
		/// <para class="body">
		/// All types within the registered assembly are enumerated and any that derive from <see cref="CalculationFunction"/> class are added to the list of available formula functions.
		/// </para>
		/// </remarks>
		public bool RegisterUserDefinedFunctionLibrary( System.Reflection.Assembly assembly )
		{
			return _calcEngine.AddFunctionLibrary( assembly );
		}

		#endregion RegisterUserDefinedFunctionLibrary

		#region ResumeCalculations

		/// <summary>
		/// Resumes automatic calculations after a preceding call to <see cref="SuspendCalculations"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">The <see cref="ResumeCalculations"/> method will resume automatic calculations after a preceding call to the <see cref="SuspendCalculations"/> method was made.</p>
		/// <p class="note">Note also that each time <b>SuspendCalculations</b> is called, a counter is incremented. So the same number of <see cref="ResumeCalculations"/> calls must be made before calculations will resume. If the call to <b>ResumeCalculations</b> results in the counter reaching 0, then calculations will resume immediately - the <see cref="XamCalculationManager"/> will not wait for a timer tick or change notification.</p>
		/// </remarks>
		/// <seealso cref="SuspendCalculations"/>
		/// <seealso cref="AreCalculationsSuspended"/>
		public void ResumeCalculations( )
		{
			if ( _calculationsSuspendedCount > 0 )
			{
				_calculationsSuspendedCount--;

				if ( 0 == _calculationsSuspendedCount )
				{
					this.AreCalculationsSuspended = false;

					this.VerifyReCalc( );
				}
			}
		}

		#endregion ResumeCalculations

		#region ResumeNotificationProcessing

		/// <summary>
		/// Resumes notification processing after a preceding call to <see cref="SuspendNotificationProcessing"/>.
		/// </summary>
		/// <param name="dirtyAllFormulas">Specify true to dirty all formulas so they will all be recalculated.</param>
		/// <remarks>
		/// <seealso cref="SuspendNotificationProcessing"/>
		/// </remarks>
		public void ResumeNotificationProcessing( bool dirtyAllFormulas )
		{
		}

		#endregion // ResumeNotificationProcessing

		#region SuspendCalculations

		/// <summary>
		/// Suspends calculations until <see cref="ResumeCalculations"/> is called.
		/// </summary>
		/// <remarks>
		/// <p class="body">Calling SuspendCalculations will temporarily suspend any automatic calculations performed by the <see cref="XamCalculationManager"/> until a subsequent call to <see cref="ResumeCalculations"/> is made.</p>
		/// <p class="note">Note that the suspension refers to automatic calculations only. This means that calculations that would normally be performed on a timer when <see cref="CalculationFrequency"/> is set to <b>Asynchronous</b> or on a change notification when the value of a control is dirtied when <see cref="CalculationFrequency"/> is set to <b>Synchronous</b> will not be performed. 
		/// Explicitly calling the <see cref="XamCalculationManager.PerformCalculations"/> method or the <see cref="ICalculationManager"/>'s <see cref="ICalculationManager.EnsureCalculated"/> method will still force a calculation.</p>
		/// <p class="note">Note also that each time <b>SuspendCalculations</b> is called, a counter is incremented. So the same number of <see cref="ResumeCalculations"/> calls must be made before calculations will resume.</p>
		/// </remarks>
		/// <seealso cref="AreCalculationsSuspended"/>
		public void SuspendCalculations( )
		{
			_calculationsSuspendedCount++;

			if ( _calculationsSuspendedCount > 0 )
				this.AreCalculationsSuspended = true;
		}

		#endregion SuspendCalculations

		#region SuspendNotificationProcessing

		/// <summary>
		/// Suspends notification processing.
		/// </summary>
		/// <remarks>
		/// <p class="body">This method is typically used when you are about to perform operations in
		/// XamDataGrid that will cause a large number of calculation-related notifications to be raised.
		/// Sometimes processing a large number of notifications can take a very long time. This method
		/// can be used to suspend such processing of notifications. <see cref="ResumeNotificationProcessing"/>
		/// must be called to resume the processing of notifications. Since not processing notifications
		/// can cause the calc manager to not enque formulas that need to be recalculated, you should call
		/// <b>ResumeNotificationProcessing</b> with <b>dirtyAllFormulas</b> parameter set to true to
		/// recalculate all the formulas.</p>
		/// <seealso cref="ResumeNotificationProcessing"/>
		/// </remarks>
		public void SuspendNotificationProcessing( )
		{
		}

		#endregion // SuspendNotificationProcessing

		#endregion // Public Methods 

		#region Internal Methods

		#region CompileFormula

		internal ICalculationFormula CompileFormula( ICalculationReference baseReference, string formula, bool suppressSyntaxErrorEvent )
		{
			CoreUtilities.ValidateNotNull( baseReference );
			CoreUtilities.ValidateNotEmpty( formula );

			UltraCalcFormula calcFormula = new UltraCalcFormula( baseReference, formula, _calcEngine.FunctionFactory );

			if ( calcFormula.HasSyntaxError && !suppressSyntaxErrorEvent )
				this.RaiseFormulaSyntaxError( baseReference, calcFormula.SyntaxError );

			return calcFormula;
		}  

		#endregion // CompileFormula

		#region GetFormulaProvider

		/// <summary>
		/// Gets the formula provider implementation for the specified formula target. 
		/// </summary>
		/// <param name="formulaTarget">The formula 
		/// target can be a control, a named reference, a data presenter field or a data grid column.
		/// </param>
		/// <returns>If the target is a valid formula target then returns the IFormulaProvider implementation
		/// for it. Otherwise it returns null. A valid formula target is considered to be one that is propertly
		/// configured for usage in calculation network. That is it must have been associated with a calculation 
		/// manager and have a valid reference name.</returns>
		internal static IFormulaProvider GetFormulaProvider( object formulaTarget )
		{
			Exception error = null;

			var namedReference = formulaTarget as NamedReference;
			if ( null != namedReference )
				return namedReference._referenceManager;

			lock ( g_formulaProviderResolvers )
			{
				foreach ( IFormulaProviderResolver ii in g_formulaProviderResolvers )
				{
					if ( ii.SupportsTarget( formulaTarget ) )
						return ii.GetFormulaProvider( formulaTarget, out error );
				}
			}

			var depObj = formulaTarget as DependencyObject;
			var calcManager = null != depObj ? XamCalculationManager.GetCalculationManager( depObj ) : null;
			var crm = null != calcManager ? calcManager._controlReferences.GetItem( formulaTarget ) : null;
			if ( null != crm )
				return crm._referenceManager;

			error = new Exception( "Target is not supported. If it's a control you need to set XamCalculationManager.CalculationManager attached property as well as either Name or ReferenceId on the settings." );

			return null;
		}

		#endregion // GetFormulaProvider

		#region GetReference

		/// <summary>
		/// Gets the reference with the specified reference name.
		/// </summary>
		/// <param name="referenceName">Reference's absolute name. If it's relative then it will be considered to be relative to the root level.</param>
		/// <returns>Found reference or null if none is found.</returns>
		internal ICalculationReference GetReference( string referenceName )
		{
			RefParser parser = RefParser.Parse( referenceName );
			string rootName = parser.RootName;

			var rootReference = this.FindRootReference( rootName );
			if ( null != rootReference )
				return rootReference.CreateReference( referenceName );

			return null;
		}

		#endregion // GetReference

		#region GetReferenceTree

		internal ReadOnlyObservableCollection<CalculationReferenceNode> GetReferenceTree(object formulaTarget)
		{
			string namedReferencesAllCategoryName = SRUtil.GetString("NamedReference_Category_All");
			string namedReferencesUnassignedCategoryName = SRUtil.GetString("NamedReference_Category_Unassigned");

			List<CalculationReferenceNode> mainList = new List<CalculationReferenceNode>();

			// NamedReferences
			ObservableCollection<CalculationReferenceNode> namedReferencesNodeList = new ObservableCollection<CalculationReferenceNode>();
			if (this.NamedReferences != null)
			{
				//Add Category Nodes
				CalculationReferenceNode allNode = new CalculationReferenceNode(namedReferencesAllCategoryName, false, ReferenceNodeType.NamedReferencesAllGroup);
				namedReferencesNodeList.Add(allNode);
				List<CalculationReferenceNode> allNodeList = new List<CalculationReferenceNode>();
				allNode.ChildReferences = this.GetNamedReferenceNodes(namedReferencesAllCategoryName, true);

				ReadOnlyObservableCollection<CalculationReferenceNode> unassignedReferences = this.GetNamedReferenceNodes(namedReferencesUnassignedCategoryName, false, true);
				if (unassignedReferences.Count > 0)
				{
					CalculationReferenceNode unassignedNode = new CalculationReferenceNode(namedReferencesUnassignedCategoryName, false, ReferenceNodeType.NamedReferencesUnassignedGroup);
					namedReferencesNodeList.Add(unassignedNode);
					unassignedNode.ChildReferences = unassignedReferences;
				}

				foreach (string category in XamCalculationManager.GetUniqueCategories(this.NamedReferences))
				{
					CalculationReferenceNode categoryNode = new CalculationReferenceNode(category, false, ReferenceNodeType.NamedReferencesOtherGroup);
					namedReferencesNodeList.Add(categoryNode);
					categoryNode.ChildReferences = this.GetNamedReferenceNodes(category);
				}
			}

			CalculationReferenceNode namedReferencesNode = new CalculationReferenceNode(SRUtil.GetString("Operand_Category_NamedReferences"), false, ReferenceNodeType.NamedReferencesGroup);
			mainList.Add(namedReferencesNode);
			namedReferencesNode.ChildReferences = new ReadOnlyObservableCollection<CalculationReferenceNode>(namedReferencesNodeList);

			// MD 3/23/12 - TFS98810
			// Gather up the Controls which are participants
			HashSet<Control> participantControls = new HashSet<Control>();
			foreach (var participant in _participants.Values)
			{
				ICalculationReference reference = participant.RootReference;
				if (reference == null)
					continue;

				Control control = reference.Context as Control;
				if (control == null)
					continue;

				participantControls.Add(control);
			}

			// ControlCalculationSettings Controls
			ObservableCollection<CalculationReferenceNode> controlsNodeList = new ObservableCollection<CalculationReferenceNode>();
			foreach (var controlReference in _controlReferences)
			{
				if (controlReference.CalcReference == null 
					|| ! controlReference.ShouldIncludeInReferenceTree )
					continue;

				// MD 3/23/12 - TFS98810
				// If any control is a participant, it should not be included in the reference tree.
				Control control = controlReference.CalcReference.Context as Control;
				if (control != null && participantControls.Contains(control))
					continue;

				CalculationReferenceNode calcSettingsRefNode = new CalculationReferenceNode(
					controlReference.CalcReference,
					controlReference.CalcReference.ElementName,
					true,
					ReferenceNodeType.Control);

				controlsNodeList.Add(calcSettingsRefNode);
			}

			// ICalculationParticipants
			foreach (var participant in _participants.Values)
			{
				CalculationReferenceNode refNode = participant.GetReferenceTree(formulaTarget);
				if (refNode != null)
					controlsNodeList.Add(refNode);
			}

			CalculationReferenceNode controlsNode = new CalculationReferenceNode(SRUtil.GetString("Operand_Category_Controls"), false, ReferenceNodeType.ControlsGroup);
			mainList.Add(controlsNode);
			controlsNode.ChildReferences = new ReadOnlyObservableCollection<CalculationReferenceNode>(controlsNodeList);

			return this.SortReferences(mainList);
		}

		#endregion  // GetReferenceTree

		#region GetRegisteredParticipants

		/// <summary>
		/// Gets all the registered participants.
		/// </summary>
		/// <returns>Registered participants enumerable.</returns>
		internal IEnumerable<ICalculationParticipant> GetRegisteredParticipants( )
		{
			return _participants.Values;
		} 

		#endregion // GetRegisteredParticipants

		#region InternalAddFormula

		internal void InternalAddFormula( ICalculationFormula formula )
		{
			CoreUtilities.ValidateNotNull( formula );

			_calcEngine.AddFormula( formula );
			this.DirtyReference( formula.BaseReference, ValueDirtiedAction.FormulaAdded );
		}

		#endregion // InternalAddFormula

		#region InternalAddParticipant

		internal void InternalAddParticipant( ICalculationParticipant participant )
		{
			string rootName = Utils.GetParticipantRootNameHelper( participant );

			if ( _participants.ContainsKey( rootName ) )
			{
				if ( _participants[rootName] != participant )
					throw new ArgumentException( SRUtil.GetString("ParticipantUsedMoreThanOnce", rootName), "participant" );
				else
				{
					Debug.Assert( false, "Attempted to add the same UltraCalcParticipant twice" );
					return;
				}
			}

			_participants[rootName] = participant;
		}

		#endregion // InternalAddParticipant

		#region InternalAddReference

		internal void InternalAddReference( ICalculationReference reference )
		{
			CoreUtilities.ValidateNotNull( reference, "reference" );

			_calcEngine.NotifyTopologicalChange( reference, ReferenceActionCode.Create );
			this.DirtyReference( reference, ValueDirtiedAction.ReferenceAdded );
		}

		#endregion // InternalAddReference

		#region InternalAddRowReference

		internal void InternalAddRowReference( ICalculationReference reference )
		{
			CoreUtilities.ValidateNotNull( reference, "reference" );

			_calcEngine.NotifyTopologicalChange( reference, ReferenceActionCode.Insert );
			this.DirtyReference( reference, ValueDirtiedAction.RowReferenceAdded );
		}

		#endregion // InternalAddRowReference

		#region InternalEnsureCalculated

		internal bool InternalEnsureCalculated( ICalculationReference reference, bool wait )
		{
			return _calcEngine.IsDirty( reference, wait );
		}

		#endregion // InternalEnsureCalculated

		#region InternalNotifyValueChanged

		internal void InternalNotifyValueChanged( ICalculationReference reference )
		{
			CoreUtilities.ValidateNotNull( reference, "reference" );

			_calcEngine.NotifyValueChanged( reference );
			this.DirtyReference( reference, ValueDirtiedAction.ValueChanged );
		}

		#endregion // InternalNotifyValueChanged

		#region InternalProcessEventQueue

		internal void InternalProcessEventQueue( )
		{
			_calcEngine.CleanTopologicalEventQueue( -1 );
		}

		#endregion // InternalProcessEventQueue

		#region InternalRaiseFormulaError

		internal FormulaCalculationErrorEventArgs InternalRaiseFormulaError( ICalculationReference reference, CalculationErrorValue errorInfo, object defaultReplacementValue )
		{
			return this.RaiseFormulaError( reference, errorInfo, defaultReplacementValue );
		}

		#endregion // InternalRaiseFormulaError

		#region InternalRemoveFormula

		internal void InternalRemoveFormula( ICalculationFormula formula )
		{
			CoreUtilities.ValidateNotNull( formula, "formula" );

			_calcEngine.DeleteFormula( formula );
			this.DirtyReference( formula.BaseReference, ValueDirtiedAction.FormulaRemoved );
		}

		#endregion // InternalRemoveFormula

		#region InternalRemoveParticipant

		internal void InternalRemoveParticipant( ICalculationParticipant participant )
		{
			CoreUtilities.ValidateNotNull( participant, "participant" );

			bool found = false;

			// The reason for looping is that in case participants key is changed prior to the remove call, we would
			// not find a match via hashing. Furthermore, get the key of the participant requires parsing its absolute
			// name (see add logic). So unless there are a huge number of participants which is highly unlikely, there
			// is no performance issue here either.
			// 
			foreach ( var ii in _participants )
			{
				if ( ii.Value == participant )
				{
					_participants.Remove( ii.Key );
					found = true;
					break;
				}
			}

			Debug.Assert( found, "Attempt to remove a participant that hasn't been added." );
		}

		#endregion // InternalRemoveParticipant

		#region InternalRemoveReference

		internal void InternalRemoveReference( ICalculationReference reference )
		{
			CoreUtilities.ValidateNotNull( reference, "reference" );

			// Use Remove instead of Delete.
			//
			_calcEngine.NotifyTopologicalChange( reference, ReferenceActionCode.Remove );

			this.DirtyReference( reference, ValueDirtiedAction.ReferenceRemoved );
		}

		#endregion // InternalRemoveReference

		#region InternalRemoveRowReference

		internal void InternalRemoveRowReference( ICalculationReference reference )
		{
			CoreUtilities.ValidateNotNull( reference, "reference" );

			_calcEngine.NotifyTopologicalChange( reference, ReferenceActionCode.Delete );
			this.DirtyReference( reference, ValueDirtiedAction.RowReferenceRemoved );
		}

		#endregion // InternalRemoveRowReference

		#region InternalRowsCollectionReferenceResynched

		internal void InternalRowsCollectionReferenceResynched( ICalculationReference reference )
		{
			CoreUtilities.ValidateNotNull( reference, "reference" );

			_calcEngine.NotifyTopologicalChange( reference, ReferenceActionCode.Resync );
			this.DirtyReference( reference, ValueDirtiedAction.RowsCollectionReferenceResynched );
		}

		#endregion // InternalRowsCollectionReferenceResynched

		#region InternalRowsCollectionReferenceSorted

		internal void InternalRowsCollectionReferenceSorted( ICalculationReference reference )
		{
			CoreUtilities.ValidateNotNull( reference, "reference" );

			_calcEngine.NotifyTopologicalChange( reference, ReferenceActionCode.Sort );
			this.DirtyReference( reference, ValueDirtiedAction.RowsCollectionReferenceSorted );
		}

		#endregion // InternalRowsCollectionReferenceSorted

		#region InternalRowsCollectionVisibilityChanged

		internal void InternalRowsCollectionVisibilityChanged( ICalculationReference reference )
		{
			CoreUtilities.ValidateNotNull( reference, "reference" );

			_calcEngine.NotifyTopologicalChange( reference, ReferenceActionCode.Visible );
			this.DirtyReference( reference, ValueDirtiedAction.RowsCollectionVisibilityChanged );
		}

		#endregion // InternalRowsCollectionVisibilityChanged

		#region RaiseFormulaError

		/// <summary>
		/// Raises either <see cref="FormulaReferenceError"/> or <see cref="FormulaCalculationError"/> events based on the type
		/// of error <paramref name="errorInfo"/> represents.
		/// </summary>
		/// <param name="reference">Reference associated with the error.</param>
		/// <param name="errorInfo">Error information.</param>
		/// <param name="defaultReplacementValue">The value to provide in place of an error to underlying data source item if any.</param>
		/// <returns>Event args passed into the event handlers.</returns>
		internal FormulaCalculationErrorEventArgs RaiseFormulaError( ICalculationReference reference, CalculationErrorValue errorInfo, object defaultReplacementValue )
		{
			if ( CalculationErrorCode.Reference == errorInfo.Code )
			{
				var args = new FormulaReferenceErrorEventArgs( reference.Context, errorInfo.Message, defaultReplacementValue, errorInfo );
				this.OnFormulaReferenceError( args );

				return args;
			}
			else
			{
				var args = new FormulaCalculationErrorEventArgs( reference.Context, errorInfo.Message, defaultReplacementValue, errorInfo );
				this.OnFormulaCalculationError( args );

				return args;
			}
		}

		#endregion // RaiseFormulaError

		#region RegisterFormulaProviderResolver

		/// <summary>
		/// Registers a resolver that is used to get IFormulaProvider instance from a target of a formula by
		/// the formula editor.
		/// </summary>
		/// <param name="resolver">Resolver implementation</param>
		internal static void RegisterFormulaProviderResolver( IFormulaProviderResolver resolver )
		{
			g_formulaProviderResolvers.Add( resolver );
		} 

		#endregion // RegisterFormulaProviderResolver

		#endregion // Internal Methods

		#region Private Methods

		#region DirtyReference

		/// <summary>
		/// Dirties the specified calculation reference so all the dependent formulas get re-calculated.
		/// </summary>
		/// <param name="reference">Reference to dirty.</param>
		/// <param name="action">The action that lead to dirtying of the reference</param>
		private void DirtyReference( ICalculationReference reference, ValueDirtiedAction action )
		{
			CoreUtilities.ValidateNotNull( reference );

			_isCalcDirty = true;

			this.RaiseValueDirtiedEvent( reference.Context, action );

			this.VerifyReCalc( );
		}

		#endregion DirtyReference

		#region FindRootReference

		/// <summary>
		/// Searches named references, calc settings references and participants for the reference with the specified
		/// root name. Root name is the unescaped name.
		/// </summary>
		/// <param name="rootName"></param>
		/// <returns></returns>
		internal ICalculationReference FindRootReference( string rootName )
		{
			ICalculationReference rootReference = this.FindNamedReference( rootName );

			if ( null == rootReference )
				rootReference = this.FindControlReference( rootName );

			if ( null == rootReference )
				rootReference = this.FindParticipantReference( rootName );

			return rootReference;
		}

		#endregion FindRootReference

		#region FindControlReference

		internal ICalculationReference FindControlReference( string rootName )
		{
			var cr = _controlReferences.GetItem( rootName );

			return null != cr ? cr._referenceManager._calcReference : null;
		}

		#endregion FindControlReference

		#region FindParticipantReference

		internal ICalculationReference FindParticipantReference( string rootName )
		{
			ICalculationParticipant participant;
			if ( _participants.TryGetValue( rootName, out participant ) )
				return participant.RootReference;

			return null;
		}

		#endregion FindParticipantReference

		#region FindNamedReference

		/// <summary>
		/// Gets the calc reference for the named reference with the specified name.
		/// </summary>
		/// <param name="name">ReferenceId of the named reference. Note that this is not the calc reference name but the name of the named reference.</param>
		/// <returns>If a matching named reference is found, returns it. Otherwise returns null.</returns>
		internal ICalculationReference FindNamedReference( string name )
		{
			NamedReference nr = null != _namedReferences ? _namedReferences.GetItem( name ) : null;
			
			return null != nr ? nr.CalcReference : null;
		}

		#endregion FindNamedReference

		// MD 10/10/11 - TFS91199
		#region GetGrammaticaResourceString

		private static string GetGrammaticaResourceString(string resourceName)
		{
#pragma warning disable 436
			return SR.GetString(resourceName);
#pragma warning restore 436
		}

		#endregion  // GetGrammaticaResourceString

		#region GetNamedReferenceNodes

		private ReadOnlyObservableCollection<CalculationReferenceNode> GetNamedReferenceNodes(string category, 
			bool isCategoryAll = false, 
			bool isCategoryUnassigned = false)
		{
			if (this.NamedReferences == null)
				return null;

			ObservableCollection<CalculationReferenceNode> referenceList = new ObservableCollection<CalculationReferenceNode>();

			foreach (NamedReference namedReference in this.NamedReferences)
			{
				bool isReferenceUnassigned = namedReference.Category == null || namedReference.Category.Trim().Length == 0;
				bool isMatchingCategory = (isReferenceUnassigned) ? false : string.Compare(category, namedReference.Category, StringComparison.CurrentCultureIgnoreCase) == 0;

				if (isCategoryAll ||
					isMatchingCategory ||
					(isCategoryUnassigned &&
					(isReferenceUnassigned)))
				{
					CalculationReferenceNode refNode = new CalculationReferenceNode(namedReference.CalcReference, null, true, ReferenceNodeType.NamedReference);
					referenceList.Add(refNode);
				}
			}

			return new ReadOnlyObservableCollection<CalculationReferenceNode>(referenceList);
		}

		#endregion GetNamedReferenceNodes

		#region GetUniqueCategories

		internal static List<string> GetUniqueCategories(NamedReferenceCollection namedReferences)
		{
			List<string> categories = new List<string>();

			foreach (NamedReference namedReference in namedReferences)
			{
				string category = namedReference.Category;

				if (category != null &&
					category.Length > 0 &&
					!categories.Contains(category))
				{
					categories.Add(category);
				}
			}

			categories.Sort();
			return categories;
		}

		#endregion GetUniqueCategories

		#region InitializeAsyncCallback

		// SSP 10/19/11 TFS92057
		// Refactored. Moved existing code from OnInitialized to here.
		// 
		private void InitializeAsyncCallback( )
		{
			if ( !_isInitialized )
			{
				_isInitialized = true;

				if ( null != _namedReferences )
					_namedReferences.ReregisterAllReferences( );

				if ( null != _controlReferences )
					_controlReferences.ReregisterAllReferences( );

				this._listeners.OnPropertyValueChanged( this, "IsInitialized", null );
			}
		}

		#endregion // InitializeAsyncCallback

		#region NotifyAttachedPropertyListeners

		/// <summary>
		/// Notifies listeners added to g_attachedPropertyListeners collection.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private static void NotifyAttachedPropertyListeners( object sender, DependencyPropertyChangedEventArgs args )
		{
			lock ( AttachedPropertyListeners )
			{
				AttachedPropertyListeners.OnPropertyValueChanged( sender, DependencyPropertyUtilities.GetName( args.Property ), args );
			}
		}

		#endregion // NotifyAttachedPropertyListeners

		#region OnCalcEngine_Circularity

		private void OnCalcEngine_Circularity( object sender, ICalculationReference reference )
		{
			if ( reference == null )
				return;

			if ( null == _circularFormulasCache )
				_circularFormulasCache = new HashSet<string>( );

			// Circularity error should only fire once for each formula. 
			string formula = reference.Formula.FormulaString;
			if ( _circularFormulasCache.Contains( formula ) )
				return;

			_circularFormulasCache.Add( formula );

			string errorMessage = SRUtil.GetString( "FormulaCircularityError_Default_Message", reference.AbsoluteName, formula ); // "A Formula Circularity has been detected in the formula for [" + e.Reference.AbsoluteName + "]. No calculations can be performed by the CalcEngine while a Circularity exists.";
			FormulaCircularityErrorEventArgs args = new FormulaCircularityErrorEventArgs( reference.Context, errorMessage, true );

			this.OnFormulaCircularityError( args );
		} 

		#endregion // OnCalcEngine_Circularity

		#region OnTimerTick

		/// <summary>
		/// Tick event handler of the timer.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnTimerTick( object sender, EventArgs e )
		{
			this.PerformCalculations( this.AsynchronousCalculationDuration );

			if ( !_isCalcDirty )
				this.StopTimer( );
		}

		#endregion // OnTimerTick

		#region RaiseFormulaSyntaxError

		private void RaiseFormulaSyntaxError( ICalculationReference reference, string errorMessage )
		{
			if ( _isDerived || null != this.FormulaSyntaxError )
			{
				string errorText = SRUtil.GetString( "FormulaSyntaxError_Default_Message", reference.AbsoluteName, errorMessage ); //string.Format("A syntax error occured while compiling a formula for: {0}\nThe error is:\n{1}", reference.AbsoluteName, errorMessage);
				FormulaSyntaxErrorEventArgs args = new FormulaSyntaxErrorEventArgs( reference.Context, errorText );

				this.OnFormulaSyntaxError( args );
			}
		}

		#endregion RaiseFormulaSyntaxError 

		#region SortReferences

		private ReadOnlyObservableCollection<CalculationReferenceNode> SortReferences(IEnumerable<CalculationReferenceNode> referenceNodes)
		{
			if (referenceNodes == null || referenceNodes.Count() == 0)
				return null;

			var retVal = new ReadOnlyObservableCollection<CalculationReferenceNode>(
				new ObservableCollection<CalculationReferenceNode>(
					referenceNodes.OrderBy(node => node.DisplayNameResolved, StringComparer.CurrentCultureIgnoreCase)
					));

			foreach (CalculationReferenceNode refNode in retVal)
				refNode.ChildReferences = this.SortReferences(refNode.ChildReferences);

			return retVal;
		}

		#endregion SortReferences

		#region StartTimer

		/// <summary>
		/// Starts the timer.
		/// </summary>
		private void StartTimer( )
		{
			this.VerifyTimer( );

			if ( null != _timer )
				_timer.Start( );
		} 

		#endregion // StartTimer

		#region StopTimer

		/// <summary>
		/// Stops the timer.
		/// </summary>
		private void StopTimer( )
		{
			if ( null != _timer )
				_timer.Stop( );
		} 

		#endregion // StopTimer

		#region VerifyReCalc

		/// <summary>
		/// If calculations are dirty then performs them or starts a timer to perform them based on CalculationFrequency setting.
		/// </summary>
		private void VerifyReCalc( )
		{
			if ( _isInitialized )
			{
				switch ( this.CalculationFrequency )
				{
					case CalculationFrequency.Asynchronous:
						this.StartTimer( );
						break;
					case CalculationFrequency.Synchronous:
						this.PerformCalculations( );
						break;
					case CalculationFrequency.Manual:
						// Manual means the user will call PerformCalculations method.
						break;
					default:
						Debug.Assert( false );
						break;
				}
			}
		}

		#endregion VerifyReCalc

		#region VerifyTimer

		/// <summary>
		/// Ensures timer is allocated and resets its interval to AsynchronousCalculationInterval.
		/// </summary>
		private void VerifyTimer( )
		{
			if ( null == _timer )
			{
				_timer = new DispatcherTimer( );
				_timer.Tick += new EventHandler( OnTimerTick );
			}

			_timer.Interval = TimeSpan.FromMilliseconds( this.AsynchronousCalculationInterval );
		}

		#endregion // VerifyTimer

		#endregion // Private Methods

		#endregion // Methods

		#region ICalculationManager Interface Implementation

		object ICalculationManager.PerformAction( CalculationEngineAction action, object data )
		{
			switch ( action )
			{
				case CalculationEngineAction.Recalc:
					return this.PerformCalculations( (long)data );
				default:
					return _calcEngine.PerformActionPartial( action, data );
			}
		}

		ICalculationFormula ICalculationManager.CompileFormula( ICalculationReference baseReference, string formula, bool suppressSyntaxErrorEvent )
		{
			return this.CompileFormula( baseReference, formula, suppressSyntaxErrorEvent );
		} 

		bool ICalculationManager.EnsureCalculated( ICalculationReference reference, bool wait )
		{
			return this.InternalEnsureCalculated( reference, wait );
		}

		void ICalculationManager.AddFormula( ICalculationFormula formula )
		{
			this.InternalAddFormula( formula );
		}

		void ICalculationManager.RemoveFormula( ICalculationFormula formula )
		{
			this.InternalRemoveFormula( formula );
		}

		void ICalculationManager.NotifyValueChanged( ICalculationReference reference )
		{
			this.InternalNotifyValueChanged( reference );
		}

		void ICalculationManager.RemoveReference( ICalculationReference reference )
		{
			this.InternalRemoveReference( reference );
		}

		void ICalculationManager.RemoveRowReference( ICalculationReference reference )
		{
			this.InternalRemoveRowReference( reference );
		}

		void ICalculationManager.AddReference( ICalculationReference reference )
		{
			this.InternalAddReference( reference );
		}

		void ICalculationManager.AddRowReference( ICalculationReference reference )
		{
			this.InternalAddRowReference( reference );
		}		

		void ICalculationManager.RowsCollectionReferenceSorted( ICalculationReference reference )
		{
			this.InternalRowsCollectionReferenceSorted( reference );
		}

		void ICalculationManager.RowsCollectionReferenceResynched( ICalculationReference reference )
		{
			this.InternalRowsCollectionReferenceResynched( reference );
		}

		void ICalculationManager.RowsCollectionVisibilityChanged( ICalculationReference reference )
		{
			this.InternalRowsCollectionVisibilityChanged( reference );
		}

		/// <summary>
		/// Gets the reference with the specified reference name.
		/// </summary>
		/// <param name="referenceName">Reference's absolute name. If it's relative then it will be considered to be relative to the root level.</param>
		/// <returns>Found reference or null if none is found.</returns>
		ICalculationReference ICalculationManager.GetReference( string referenceName )
		{
			return this.GetReference( referenceName );
		}

		void ICalculationManager.AddParticipant( ICalculationParticipant participant )
		{
			this.InternalAddParticipant( participant );
		}

		void ICalculationManager.RemoveParticipant( ICalculationParticipant participant )
		{
			this.InternalRemoveParticipant( participant );
		}

		FormulaCalculationErrorEventArgs ICalculationManager.RaiseFormulaError( ICalculationReference reference, CalculationErrorValue errorInfo, object defaultReplacementValue )
		{
			return this.InternalRaiseFormulaError( reference, errorInfo, defaultReplacementValue );
		}

		void ICalculationManager.ProcessEventQueue( )
		{
			this.InternalProcessEventQueue( );
		}

		#endregion // ICalculationManager Interface Implementation

		#region IPropertyChangeListener Interface Implementation

		void ITypedPropertyChangeListener<object, string>.OnPropertyValueChanged( object dataItem, string property, object extraInfo )
		{
			if ( dataItem == _calcEngine )
			{
				switch ( property )
				{
					case "DirtyValue":
					case "RecalcValue":
						// Propagate the prop changes to listeners. Data presenter looks for these two notifications.
						// 
						_listeners.OnPropertyValueChanged( this, property, extraInfo );
						break;
					case "Circularity":
						this.OnCalcEngine_Circularity( dataItem, (ICalculationReference)extraInfo );
						break;
					default:
						Debug.Assert( false, "Uknown property." );
						break;
				}
			}

			// SSP 5/10/12 TFS111115
			// Notify the formula editor so it can repopulate the reference tree.
			// 
			if ( dataItem == _namedReferences || dataItem == _controlReferences )
			{
				_listeners.OnPropertyValueChanged( dataItem, property, extraInfo );
			}
		}

		#endregion // IPropertyChangeListener Interface Implementation

		#region ISupportPropertyChangeNotifications Interface Implementation

		void ITypedSupportPropertyChangeNotifications<object, string>.AddListener( ITypedPropertyChangeListener<object, string> listener, bool useWeakReference )
		{
			_listeners.Add( listener, useWeakReference );
		}

		void ITypedSupportPropertyChangeNotifications<object, string>.RemoveListener( ITypedPropertyChangeListener<object, string> listener )
		{
			_listeners.Remove( listener );
		}

		#endregion // ISupportPropertyChangeNotifications Interface Implementation

		#region ISupportInitialize Members


#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		#endregion // ISupportInitialize Members
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