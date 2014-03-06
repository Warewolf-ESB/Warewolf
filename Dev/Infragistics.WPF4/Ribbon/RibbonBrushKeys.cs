using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Infragistics.Windows.Themes;

namespace Infragistics.Windows.Ribbon
{

	/// <summary>
	/// Static class that exposes the resource keys used by the ribbon elements. 
	/// </summary>
	/// <remarks>
	/// <para class="body">These keys are referenced in the templates of the ribbon elements via dynamic references. Therefore, the default brushes can be easily changed
	/// by defining replacement brushes, keyed with these keys, in resources anywhere within the resolution scope of their use.
	/// <para class="note"><b>Note:</b> These brushes are normally added to the Resources collection of the window or the application.</para>  
	/// </para></remarks>
	public static class RibbonBrushKeys
	{
		#region XamRibbonWindowBrushKeys

		#region ActiveWindowBorderOuterBrushKey

		/// <summary>
		/// The key used to identify the brush used to draw the outer border pixel of the XamRibbonWindow when it is activated
		/// </summary>
		public static readonly ResourceKey ActiveWindowBorderOuterBrushKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ActiveWindowBorderOuterBrushKey");

		#endregion ActiveWindowBorderOuterBrushKey

		#region ActiveWindowBorderInner1BrushKey

		/// <summary>
		/// The key used to identify one of the brushes used to draw the inner border of the XamRibbonWindow when it is activated
		/// </summary>
		public static readonly ResourceKey ActiveWindowBorderInner1BrushKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ActiveWindowBorderInner1BrushKey");

		#endregion ActiveWindowBorderInner1BrushKey

		#region ActiveWindowBorderInner2BrushKey

		/// <summary>
		/// The key used to identify one of the brushes used to draw the inner border of the XamRibbonWindow when it is activated
		/// </summary>
		public static readonly ResourceKey ActiveWindowBorderInner2BrushKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ActiveWindowBorderInner2BrushKey");

		#endregion ActiveWindowBorderInner2BrushKey

		#region ActiveWindowBorderInner3BrushKey

		/// <summary>
		/// The key used to identify one of the brushes used to draw the inner border of the XamRibbonWindow when it is activated
		/// </summary>
		public static readonly ResourceKey ActiveWindowBorderInner3BrushKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ActiveWindowBorderInner3BrushKey");

		#endregion ActiveWindowBorderInner3BrushKey

		#region WindowBackgroundBrushKey

		/// <summary>
		/// The key used to identify the brush used for the content area of a XamRibbonWindow
		/// </summary>
		public static readonly ResourceKey WindowBackgroundBrushKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "WindowBackgroundBrushKey");

		#endregion WindowBackgroundBrushKey

		#region WindowBorderOuterBrushKey

		/// <summary>
		/// The key used to identify the brush used to draw the outer border pixel of the XamRibbonWindow when it is deactivated
		/// </summary>
		public static readonly ResourceKey WindowBorderOuterBrushKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "WindowBorderOuterBrushKey");

		#endregion WindowBorderOuterBrushKey

		#region WindowBorderInner1BrushKey

		/// <summary>
		/// The key used to identify one of the brushes used to draw the inner border of the XamRibbonWindow when it is deactivated
		/// </summary>
		public static readonly ResourceKey WindowBorderInner1BrushKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "WindowBorderInner1BrushKey");

		#endregion WindowBorderInner1BrushKey

		#region WindowBorderInner2BrushKey

		/// <summary>
		/// The key used to identify one of the brushes used to draw the inner border of the XamRibbonWindow when it is deactivated
		/// </summary>
		public static readonly ResourceKey WindowBorderInner2BrushKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "WindowBorderInner2BrushKey");

		#endregion WindowBorderInner2BrushKey

		#region WindowBorderInner3BrushKey

		/// <summary>
		/// The key used to identify one of the brushes used to draw the inner border of the XamRibbonWindow when it is deactivated
		/// </summary>
		public static readonly ResourceKey WindowBorderInner3BrushKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "WindowBorderInner3BrushKey");

		#endregion WindowBorderInner3BrushKey

		#endregion XamRibbonWindowBrushKeys

        // JJD 5/13/10 - NA 2010 volumne 2 - Scenic Ribbon
		#region XamRibbonWindowBrushKeys for scenic theme

		#region ScenicActiveWindowBorderOuterBrushKey

		/// <summary>
        /// The key used to identify the brush used to draw the outer border pixel of the XamRibbonWindow when it is activated
        /// </summary>
		public static readonly ResourceKey ScenicActiveWindowBorderOuterBrushKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ScenicActiveWindowBorderOuterBrushKey");

		#endregion ScenicActiveWindowBorderOuterBrushKey

		#region ScenicActiveWindowBorderOuterShadowBrushKey

		/// <summary>
        /// The key used to identify the brush used to draw the outer shadow border pixel (along the bottom and right edge) of the XamRibbonWindow when it is activated
		/// </summary>
		public static readonly ResourceKey ScenicActiveWindowBorderOuterShadowBrushKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ScenicActiveWindowBorderOuterShadowBrushKey");

		#endregion ScenicActiveWindowBorderOuterShadowBrushKey

		#region ScenicActiveWindowBorderInner1BrushKey

		/// <summary>
		/// The key used to identify one of the brushes used to draw the inner border of the XamRibbonWindow when it is activated
		/// </summary>
		public static readonly ResourceKey ScenicActiveWindowBorderInner1BrushKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ScenicActiveWindowBorderInner1BrushKey");

		#endregion ScenicActiveWindowBorderInner1BrushKey

		#region ScenicActiveWindowBorderInner1ShadowBrushKey

		/// <summary>
        /// The key used to identify the brush used to draw the inner shadow border pixel (along the bottom and right edge) of the XamRibbonWindow when it is activated
        /// </summary>
		public static readonly ResourceKey ScenicActiveWindowBorderInner1ShadowBrushKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ScenicActiveWindowBorderInner1ShadowBrushKey");

		#endregion ScenicActiveWindowBorderInner1ShadowBrushKey

		#region ScenicActiveWindowBorderInner2BrushKey

		/// <summary>
		/// The key used to identify one of the brushes used to draw the inner border of the XamRibbonWindow when it is activated
		/// </summary>
		public static readonly ResourceKey ScenicActiveWindowBorderInner2BrushKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ScenicActiveWindowBorderInner2BrushKey");

		#endregion ScenicActiveWindowBorderInner2BrushKey

		#region ScenicActiveWindowBorderInner3BrushKey

		/// <summary>
		/// The key used to identify one of the brushes used to draw the inner border of the XamRibbonWindow when it is activated
		/// </summary>
		public static readonly ResourceKey ScenicActiveWindowBorderInner3BrushKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ScenicActiveWindowBorderInner3BrushKey");

		#endregion ScenicActiveWindowBorderInner3BrushKey

		#region ScenicWindowBorderOuterBrushKey

		/// <summary>
		/// The key used to identify the brush used to draw the outer border pixel of the XamRibbonWindow when it is deactivated
		/// </summary>
		public static readonly ResourceKey ScenicWindowBorderOuterBrushKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ScenicWindowBorderOuterBrushKey");

		#endregion ScenicWindowBorderOuterBrushKey

		#region ScenicWindowBorderOuterShadowBrushKey

		/// <summary>
        /// The key used to identify the brush used to draw the outer shadow border pixel (along the bottom and right edge) of the XamRibbonWindow when it is deactivated
        /// </summary>
		public static readonly ResourceKey ScenicWindowBorderOuterShadowBrushKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ScenicWindowBorderOuterShadowBrushKey");

		#endregion ScenicWindowBorderOuterShadowBrushKey

		#region ScenicWindowBorderInner1BrushKey

		/// <summary>
		/// The key used to identify one of the brushes used to draw the inner border of the XamRibbonWindow when it is deactivated
		/// </summary>
		public static readonly ResourceKey ScenicWindowBorderInner1BrushKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ScenicWindowBorderInner1BrushKey");

		#endregion ScenicWindowBorderInner1BrushKey

		#region ScenicWindowBorderInner1ShadowBrushKey

		/// <summary>
        /// The key used to identify the brush used to draw the inner shadow border pixel (along the bottom and right edge) of the XamRibbonWindow when it is deactivated
        /// </summary>
		public static readonly ResourceKey ScenicWindowBorderInner1ShadowBrushKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ScenicWindowBorderInner1ShadowBrushKey");

		#endregion ScenicWindowBorderInner1ShadowBrushKey

		#region ScenicWindowBorderInner2BrushKey

		/// <summary>
		/// The key used to identify one of the brushes used to draw the inner border of the XamRibbonWindow when it is deactivated
		/// </summary>
		public static readonly ResourceKey ScenicWindowBorderInner2BrushKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ScenicWindowBorderInner2BrushKey");

		#endregion ScenicWindowBorderInner2BrushKey

		#region ScenicWindowBorderInner3BrushKey

		/// <summary>
		/// The key used to identify one of the brushes used to draw the inner border of the XamRibbonWindow when it is deactivated
		/// </summary>
		public static readonly ResourceKey ScenicWindowBorderInner3BrushKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ScenicWindowBorderInner3BrushKey");

		#endregion ScenicWindowBorderInner3BrushKey

		#endregion //XamRibbonWindowBrushKeys for scenic theme
        
        // JJD 4/29/10 - NA 2010 volumne 2 - Scenic Ribbon

        #region ScenicCaptionAreaBorderKey

        /// <summary>
        /// The key that identifies a resource to be used as the ScenicCaptionAreaBorderKey.  Look here <see cref="RibbonBrushKeys"/> for 
        /// an explanation of how these keys are used. 
        /// </summary>
        public static readonly ResourceKey ScenicCaptionAreaBorderKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ScenicCaptionAreaBorderKey");

        #endregion ScenicCaptionAreaBorderKey

        #region ScenicCaptionAreaFillKey

        /// <summary>
		/// The key that identifies a resource to be used as the ScenicCaptionAreaFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ScenicCaptionAreaFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ScenicCaptionAreaFillKey");

		#endregion ScenicCaptionAreaFillKey

        #region ScenicCaptionAreaInactiveBorderKey

        /// <summary>
        /// The key that identifies a resource to be used as the ScenicCaptionAreaInactiveBorderKey.  Look here <see cref="RibbonBrushKeys"/> for 
        /// an explanation of how these keys are used. 
        /// </summary>
        public static readonly ResourceKey ScenicCaptionAreaInactiveBorderKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ScenicCaptionAreaInactiveBorderKey");

        #endregion ScenicCaptionAreaInactiveBorderKey

        #region ScenicCaptionAreaInactiveFillKey

        /// <summary>
		/// The key that identifies a resource to be used as the ScenicCaptionAreaInactiveFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ScenicCaptionAreaInactiveFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ScenicCaptionAreaInactiveFillKey");

		#endregion ScenicCaptionAreaInactiveFillKey

        #region ScenicResizeGrippersForegroundFillKey

        /// <summary>
        /// The key that identifies a resource to be used as the ScenicResizeGrippersForegroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
        /// an explanation of how these keys are used. 
        /// </summary>
        public static readonly ResourceKey ScenicResizeGrippersForegroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ScenicResizeGrippersForegroundFillKey");

        #endregion ScenicResizeGrippersForegroundFillKey

        #region ScenicResizeGrippersBackgroundFillKey

        /// <summary>
        /// The key that identifies a resource to be used as the ScenicResizeGrippersBackgroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
        /// an explanation of how these keys are used. 
        /// </summary>
        public static readonly ResourceKey ScenicResizeGrippersBackgroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ScenicResizeGrippersBackgroundFillKey");

        #endregion ScenicResizeGrippersBackgroundFillKey

	   #region ScenicCaptionBtnGlyphBorderFillKey

	   /// <summary>
	   /// The key that identifies a resource to be used as the ScenicCaptionBtnGlyphBorderFillKey.  Look here <see cref="RibbonBrushKeys"/> for 
	   /// an explanation of how these keys are used. 
	   /// </summary>
	   public static readonly ResourceKey ScenicCaptionBtnGlyphBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ScenicCaptionBtnGlyphBorderFillKey");

	   #endregion ScenicCaptionBtnGlyphBorderFillKey

	   #region ScenicCaptionBtnGlyphBackgroundFillKey

	   /// <summary>
	   /// The key that identifies a resource to be used as the ScenicCaptionBtnGlyphBackgroundFillKey.  Look here <see cref="RibbonBrushKeys"/> for 
	   /// an explanation of how these keys are used. 
	   /// </summary>
	   public static readonly ResourceKey ScenicCaptionBtnGlyphBackgroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ScenicCaptionBtnGlyphBackgroundFillKey");

	   #endregion ScenicCaptionBtnGlyphBackgroundFillKey

	   #region ScenicCaptionBtnNormalBorderFillKey

	   /// <summary>
	   /// The key that identifies a resource to be used as the ScenicCaptionBtnNormalBorderFillKey.  Look here <see cref="RibbonBrushKeys"/> for 
	   /// an explanation of how these keys are used. 
	   /// </summary>
	   public static readonly ResourceKey ScenicCaptionBtnNormalBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ScenicCaptionBtnNormalBorderFillKey");

	   #endregion ScenicCaptionBtnNormalBorderFillKey

	   #region ScenicCaptionBtnNormalBackgroundFillKey

	   /// <summary>
	   /// The key that identifies a resource to be used as the ScenicCaptionBtnNormalBackgroundFillKey.  Look here <see cref="RibbonBrushKeys"/> for 
	   /// an explanation of how these keys are used. 
	   /// </summary>
	   public static readonly ResourceKey ScenicCaptionBtnNormalBackgroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ScenicCaptionBtnNormalBackgroundFillKey");

	   #endregion ScenicCaptionBtnNormalBackgroundFillKey

	   #region ScenicCaptionBtnNormalInnerBorderFillKey

	   /// <summary>
	   /// The key that identifies a resource to be used as the ScenicCaptionBtnNormalInnerBorderFillKey.  Look here <see cref="RibbonBrushKeys"/> for 
	   /// an explanation of how these keys are used. 
	   /// </summary>
	   public static readonly ResourceKey ScenicCaptionBtnNormalInnerBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ScenicCaptionBtnNormalInnerBorderFillKey");

	   #endregion ScenicCaptionBtnNormalInnerBorderFillKey

	   #region ScenicCaptionBtnHoverBorderFillKey

	   /// <summary>
	   /// The key that identifies a resource to be used as the ScenicCaptionBtnHoverBorderFillKey.  Look here <see cref="RibbonBrushKeys"/> for 
	   /// an explanation of how these keys are used. 
	   /// </summary>
	   public static readonly ResourceKey ScenicCaptionBtnHoverBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ScenicCaptionBtnHoverBorderFillKey");

	   #endregion ScenicCaptionBtnHoverBorderFillKey

	   #region ScenicCaptionBtnHoverBackgroundFillKey

	   /// <summary>
	   /// The key that identifies a resource to be used as the ScenicCaptionBtnHoverBackgroundFillKey.  Look here <see cref="RibbonBrushKeys"/> for 
	   /// an explanation of how these keys are used. 
	   /// </summary>
	   public static readonly ResourceKey ScenicCaptionBtnHoverBackgroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ScenicCaptionBtnHoverBackgroundFillKey");

	   #endregion ScenicCaptionBtnHoverBackgroundFillKey

	   #region ScenicCaptionBtnHoverInnerBorderFillKey

	   /// <summary>
	   /// The key that identifies a resource to be used as the ScenicCaptionBtnHoverInnerBorderFillKey.  Look here <see cref="RibbonBrushKeys"/> for 
	   /// an explanation of how these keys are used. 
	   /// </summary>
	   public static readonly ResourceKey ScenicCaptionBtnHoverInnerBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ScenicCaptionBtnHoverInnerBorderFillKey");

	   #endregion ScenicCaptionBtnHoverInnerBorderFillKey

	   #region ScenicCaptionBtnPressedBackgroundFillKey

	   /// <summary>
	   /// The key that identifies a resource to be used as the ScenicCaptionBtnPressedBackgroundFillKey.  Look here <see cref="RibbonBrushKeys"/> for 
	   /// an explanation of how these keys are used. 
	   /// </summary>
	   public static readonly ResourceKey ScenicCaptionBtnPressedBackgroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ScenicCaptionBtnPressedBackgroundFillKey");

	   #endregion ScenicCaptionBtnPressedBackgroundFillKey

	   #region ScenicCaptionBtnPressedBorderFillKey

	   /// <summary>
	   /// The key that identifies a resource to be used as the ScenicCaptionBtnPressedBorderFillKey.  Look here <see cref="RibbonBrushKeys"/> for 
	   /// an explanation of how these keys are used. 
	   /// </summary>
	   public static readonly ResourceKey ScenicCaptionBtnPressedBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ScenicCaptionBtnPressedBorderFillKey");

	   #endregion ScenicCaptionBtnPressedBorderFillKey

	   #region ScenicCaptionBtnPressedInnerBorderFillKey

	   /// <summary>
	   /// The key that identifies a resource to be used as the ScenicCaptionBtnPressedInnerBorderFillKey.  Look here <see cref="RibbonBrushKeys"/> for 
	   /// an explanation of how these keys are used. 
	   /// </summary>
	   public static readonly ResourceKey ScenicCaptionBtnPressedInnerBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ScenicCaptionBtnPressedInnerBorderFillKey");

	   #endregion ScenicCaptionBtnPressedInnerBorderFillKey

	   #region ScenicCaptionCloseBtnNormalBorderFillKey

	   /// <summary>
	   /// The key that identifies a resource to be used as the ScenicCaptionCloseBtnNormalBorderFillKey.  Look here <see cref="RibbonBrushKeys"/> for 
	   /// an explanation of how these keys are used. 
	   /// </summary>
	   public static readonly ResourceKey ScenicCaptionCloseBtnNormalBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ScenicCaptionCloseBtnNormalBorderFillKey");

	   #endregion ScenicCaptionCloseBtnNormalBorderFillKey

	   #region ScenicCaptionCloseBtnNormalBackgroundFillKey

	   /// <summary>
	   /// The key that identifies a resource to be used as the ScenicCaptionCloseBtnNormalBackgroundFillKey.  Look here <see cref="RibbonBrushKeys"/> for 
	   /// an explanation of how these keys are used. 
	   /// </summary>
	   public static readonly ResourceKey ScenicCaptionCloseBtnNormalBackgroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ScenicCaptionCloseBtnNormalBackgroundFillKey");

	   #endregion ScenicCaptionCloseBtnNormalBackgroundFillKey

	   #region ScenicCaptionCloseBtnNormalInnerBorderFillKey

	   /// <summary>
	   /// The key that identifies a resource to be used as the ScenicCaptionCloseBtnNormalInnerBorderFillKey.  Look here <see cref="RibbonBrushKeys"/> for 
	   /// an explanation of how these keys are used. 
	   /// </summary>
	   public static readonly ResourceKey ScenicCaptionCloseBtnNormalInnerBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ScenicCaptionCloseBtnNormalInnerBorderFillKey");

	   #endregion ScenicCaptionCloseBtnNormalInnerBorderFillKey

	   #region ScenicCaptionCloseBtnHoverBorderFillKey

	   /// <summary>
	   /// The key that identifies a resource to be used as the ScenicCaptionCloseBtnHoverBorderFillKey.  Look here <see cref="RibbonBrushKeys"/> for 
	   /// an explanation of how these keys are used. 
	   /// </summary>
	   public static readonly ResourceKey ScenicCaptionCloseBtnHoverBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ScenicCaptionCloseBtnHoverBorderFillKey");

	   #endregion ScenicCaptionCloseBtnHoverBorderFillKey

	   #region ScenicCaptionCloseBtnHoverBackgroundFillKey

	   /// <summary>
	   /// The key that identifies a resource to be used as the ScenicCaptionCloseBtnHoverBackgroundFillKey.  Look here <see cref="RibbonBrushKeys"/> for 
	   /// an explanation of how these keys are used. 
	   /// </summary>
	   public static readonly ResourceKey ScenicCaptionCloseBtnHoverBackgroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ScenicCaptionCloseBtnHoverBackgroundFillKey");

	   #endregion ScenicCaptionCloseBtnHoverBackgroundFillKey

	   #region ScenicCaptionCloseBtnHoverInnerBorderFillKey

	   /// <summary>
	   /// The key that identifies a resource to be used as the ScenicCaptionCloseBtnHoverInnerBorderFillKey.  Look here <see cref="RibbonBrushKeys"/> for 
	   /// an explanation of how these keys are used. 
	   /// </summary>
	   public static readonly ResourceKey ScenicCaptionCloseBtnHoverInnerBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ScenicCaptionCloseBtnHoverInnerBorderFillKey");

	   #endregion ScenicCaptionCloseBtnHoverInnerBorderFillKey

	   #region ScenicCaptionCloseBtnPressedBorderFillKey

	   /// <summary>
	   /// The key that identifies a resource to be used as the ScenicCaptionCloseBtnPressedBorderFillKey.  Look here <see cref="RibbonBrushKeys"/> for 
	   /// an explanation of how these keys are used. 
	   /// </summary>
	   public static readonly ResourceKey ScenicCaptionCloseBtnPressedBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ScenicCaptionCloseBtnPressedBorderFillKey");

	   #endregion ScenicCaptionCloseBtnPressedBorderFillKey

	   #region ScenicCaptionCloseBtnPressedBackgroundFillKey

	   /// <summary>
	   /// The key that identifies a resource to be used as the ScenicCaptionCloseBtnPressedBackgroundFillKey.  Look here <see cref="RibbonBrushKeys"/> for 
	   /// an explanation of how these keys are used. 
	   /// </summary>
	   public static readonly ResourceKey ScenicCaptionCloseBtnPressedBackgroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ScenicCaptionCloseBtnPressedBackgroundFillKey");

	   #endregion ScenicCaptionCloseBtnPressedBackgroundFillKey

	   #region ScenicCaptionCloseBtnPressedInnerBorderFillKey

	   /// <summary>
	   /// The key that identifies a resource to be used as the ScenicCaptionCloseBtnPressedInnerBorderFillKey.  Look here <see cref="RibbonBrushKeys"/> for 
	   /// an explanation of how these keys are used. 
	   /// </summary>
	   public static readonly ResourceKey ScenicCaptionCloseBtnPressedInnerBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ScenicCaptionCloseBtnPressedInnerBorderFillKey");

	   #endregion ScenicCaptionCloseBtnPressedInnerBorderFillKey

	   #region ScenicCaptionBtnDisabledBackgroundFillKey

	   /// <summary>
	   /// The key that identifies a resource to be used as the ScenicCaptionBtnDisabledBackgroundFillKey.  Look here <see cref="RibbonBrushKeys"/> for 
	   /// an explanation of how these keys are used. 
	   /// </summary>
	   public static readonly ResourceKey ScenicCaptionBtnDisabledBackgroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ScenicCaptionBtnDisabledBackgroundFillKey");

	   #endregion ScenicCaptionBtnDisabledBackgroundFillKey


       #region ScenicStatusBarTextForegroundFillKey

       /// <summary>
       /// The key that identifies a resource to be used as the ScenicStatusBarTextForegroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
       /// an explanation of how these keys are used. 
       /// </summary>
       public static readonly ResourceKey ScenicStatusBarTextForegroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ScenicStatusBarTextForegroundFillKey");

       #endregion ScenicStatusBarTextForegroundFillKey

       #region ScenicStatusBarFillKey

       /// <summary>
       /// The key that identifies a resource to be used as the ScenicStatusBarFill.  Look here <see cref="RibbonBrushKeys"/> for 
       /// an explanation of how these keys are used. 
       /// </summary>
       public static readonly ResourceKey ScenicStatusBarFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ScenicStatusBarFillKey");

       #endregion ScenicStatusBarFillKey

       #region ScenicStatusBarBorderFillKey

       /// <summary>
       /// The key that identifies a resource to be used as the ScenicStatusBarBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
       /// an explanation of how these keys are used. 
       /// </summary>
       public static readonly ResourceKey ScenicStatusBarBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ScenicStatusBarBorderFillKey");

       #endregion ScenicStatusBarBorderFillKey

       #region ScenicStatusBarSeparatorGradientLeftKey

       /// <summary>
       /// The key that identifies a resource to be used as the ScenicStatusBarSeparatorGradientLeft.  Look here <see cref="RibbonBrushKeys"/> for 
       /// an explanation of how these keys are used. 
       /// </summary>
       public static readonly ResourceKey ScenicStatusBarSeparatorGradientLeftKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ScenicStatusBarSeparatorGradientLeftKey");

       #endregion ScenicStatusBarSeparatorGradientLeftKey

       #region ScenicStatusBarSeparatorGradientRightKey

       /// <summary>
       /// The key that identifies a resource to be used as the ScenicStatusBarSeparatorGradientRight.  Look here <see cref="RibbonBrushKeys"/> for 
       /// an explanation of how these keys are used. 
       /// </summary>
       public static readonly ResourceKey ScenicStatusBarSeparatorGradientRightKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ScenicStatusBarSeparatorGradientRightKey");

       #endregion ScenicStatusBarSeparatorGradientRightKey

	  #region ScenicRibbonGroupSeparatorFillKey

	  /// <summary>
	  /// The key used to identify one of the brushes used to draw the inner border of the XamRibbonWindow when it is deactivated
	  /// </summary>
	  public static readonly ResourceKey ScenicRibbonGroupSeparatorFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ScenicRibbonGroupSeparatorFillKey");

	  #endregion ScenicRibbonGroupSeparatorFillKey

		// AS 10/19/10 TFS57563
		#region ApplicationMenuButtonForegroundKey

		/// <summary>
		/// The key that identifies the brush to be used as the foreground for the application menu.
		/// </summary>
		public static readonly ResourceKey ApplicationMenuButtonForegroundKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ApplicationMenuButtonForegroundKey");

		#endregion //ApplicationMenuButtonForegroundKey

	  #region Generated Keys

	  #region CaptionPanelFillKey

	  /// <summary>
		/// The key that identifies a resource to be used as the CaptionPanelFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey CaptionPanelFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "CaptionPanelFillKey");

		#endregion CaptionPanelFillKey

		#region CaptionPanelInactiveFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the CaptionPanelInactiveFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey CaptionPanelInactiveFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "CaptionPanelInactiveFillKey");

		#endregion CaptionPanelInactiveFillKey

		#region ContainerFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ContainerFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ContainerFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ContainerFillKey");

		#endregion ContainerFillKey

		#region ContainerBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ContainerBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ContainerBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ContainerBorderFillKey");

		#endregion ContainerBorderFillKey

		#region ContainerBorderLightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ContainerBorderLightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ContainerBorderLightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ContainerBorderLightFillKey");

		#endregion ContainerBorderLightFillKey

		#region ContainerInContextualBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ContainerInContextualBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ContainerInContextualBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ContainerInContextualBorderFillKey");

		#endregion ContainerInContextualBorderFillKey

		#region ContainerInContextualBaseFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ContainerInContextualBaseFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ContainerInContextualBaseFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ContainerInContextualBaseFillKey");

		#endregion ContainerInContextualBaseFillKey

		#region ApplicationMenuOuterBorderLightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ApplicationMenuOuterBorderLightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ApplicationMenuOuterBorderLightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ApplicationMenuOuterBorderLightFillKey");

		#endregion ApplicationMenuOuterBorderLightFillKey

		#region ApplicationMenuOuterBorderDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ApplicationMenuOuterBorderDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ApplicationMenuOuterBorderDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ApplicationMenuOuterBorderDarkFillKey");

		#endregion ApplicationMenuOuterBorderDarkFillKey

		#region ApplicationMenuInnerBorderDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ApplicationMenuInnerBorderDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ApplicationMenuInnerBorderDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ApplicationMenuInnerBorderDarkFillKey");

		#endregion ApplicationMenuInnerBorderDarkFillKey

		#region ApplicationMenuInnerBorderLightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ApplicationMenuInnerBorderLightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ApplicationMenuInnerBorderLightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ApplicationMenuInnerBorderLightFillKey");

		#endregion ApplicationMenuInnerBorderLightFillKey

		#region ApplicationMenuLeftAreaBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ApplicationMenuLeftAreaBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ApplicationMenuLeftAreaBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ApplicationMenuLeftAreaBorderFillKey");

		#endregion ApplicationMenuLeftAreaBorderFillKey

		#region ApplicationMenuLeftAreaCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ApplicationMenuLeftAreaCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ApplicationMenuLeftAreaCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ApplicationMenuLeftAreaCenterFillKey");

		#endregion ApplicationMenuLeftAreaCenterFillKey

		#region ApplicationMenuRecentItemsBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ApplicationMenuRecentItemsBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ApplicationMenuRecentItemsBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ApplicationMenuRecentItemsBorderFillKey");

		#endregion ApplicationMenuRecentItemsBorderFillKey

		#region ApplicationMenuRecentItemsSeparatorDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ApplicationMenuRecentItemsSeparatorDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ApplicationMenuRecentItemsSeparatorDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ApplicationMenuRecentItemsSeparatorDarkFillKey");

		#endregion ApplicationMenuRecentItemsSeparatorDarkFillKey

		#region ApplicationMenuRecentItemsSeparatorLightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ApplicationMenuRecentItemsSeparatorLightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ApplicationMenuRecentItemsSeparatorLightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ApplicationMenuRecentItemsSeparatorLightFillKey");

		#endregion ApplicationMenuRecentItemsSeparatorLightFillKey

		#region ApplicationMenuRecentItemsCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ApplicationMenuRecentItemsCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ApplicationMenuRecentItemsCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ApplicationMenuRecentItemsCenterFillKey");

		#endregion ApplicationMenuRecentItemsCenterFillKey

		#region ApplicationMenuFooterToolbarButtonNormalCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ApplicationMenuFooterToolbarButtonNormalCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ApplicationMenuFooterToolbarButtonNormalCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ApplicationMenuFooterToolbarButtonNormalCenterFillKey");

		#endregion ApplicationMenuFooterToolbarButtonNormalCenterFillKey

		#region ApplicationMenuFooterToolbarButtonHoverCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ApplicationMenuFooterToolbarButtonHoverCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ApplicationMenuFooterToolbarButtonHoverCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ApplicationMenuFooterToolbarButtonHoverCenterFillKey");

		#endregion ApplicationMenuFooterToolbarButtonHoverCenterFillKey

		#region ApplicationMenuFooterToolbarButtonNormalBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ApplicationMenuFooterToolbarButtonNormalBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ApplicationMenuFooterToolbarButtonNormalBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ApplicationMenuFooterToolbarButtonNormalBorderFillKey");

		#endregion ApplicationMenuFooterToolbarButtonNormalBorderFillKey

		#region ApplicationMenuFooterToolbarButtonHoverBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ApplicationMenuFooterToolbarButtonHoverBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ApplicationMenuFooterToolbarButtonHoverBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ApplicationMenuFooterToolbarButtonHoverBorderFillKey");

		#endregion ApplicationMenuFooterToolbarButtonHoverBorderFillKey

		#region ApplicationMenuSideSelectBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ApplicationMenuSideSelectBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ApplicationMenuSideSelectBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ApplicationMenuSideSelectBorderFillKey");

		#endregion ApplicationMenuSideSelectBorderFillKey

		#region ApplicationMenuSideSelectBorderLightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ApplicationMenuSideSelectBorderLightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ApplicationMenuSideSelectBorderLightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ApplicationMenuSideSelectBorderLightFillKey");

		#endregion ApplicationMenuSideSelectBorderLightFillKey

		#region ApplicationMenuSideSelectCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ApplicationMenuSideSelectCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ApplicationMenuSideSelectCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ApplicationMenuSideSelectCenterFillKey");

		#endregion ApplicationMenuSideSelectCenterFillKey

		#region ApplicationMenuSideSelectCenterSecondaryFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ApplicationMenuSideSelectCenterSecondaryFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ApplicationMenuSideSelectCenterSecondaryFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ApplicationMenuSideSelectCenterSecondaryFillKey");

		#endregion ApplicationMenuSideSelectCenterSecondaryFillKey

		#region ApplicationMenuDisabledSideSelectBorderDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ApplicationMenuDisabledSideSelectBorderDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ApplicationMenuDisabledSideSelectBorderDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ApplicationMenuDisabledSideSelectBorderDarkFillKey");

		#endregion ApplicationMenuDisabledSideSelectBorderDarkFillKey

		#region ApplicationMenuDisabledSideSelectBorderLightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ApplicationMenuDisabledSideSelectBorderLightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ApplicationMenuDisabledSideSelectBorderLightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ApplicationMenuDisabledSideSelectBorderLightFillKey");

		#endregion ApplicationMenuDisabledSideSelectBorderLightFillKey

		#region ApplicationMenuDisabledSideSelectCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ApplicationMenuDisabledSideSelectCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ApplicationMenuDisabledSideSelectCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ApplicationMenuDisabledSideSelectCenterFillKey");

		#endregion ApplicationMenuDisabledSideSelectCenterFillKey

		#region RibbonGroupNormalBorderLightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupNormalBorderLightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupNormalBorderLightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupNormalBorderLightFillKey");

		#endregion RibbonGroupNormalBorderLightFillKey

		#region RibbonGroupNormalBorderDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupNormalBorderDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupNormalBorderDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupNormalBorderDarkFillKey");

		#endregion RibbonGroupNormalBorderDarkFillKey

		#region RibbonGroupNormalBottomGradientFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupNormalBottomGradientFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupNormalBottomGradientFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupNormalBottomGradientFillKey");

		#endregion RibbonGroupNormalBottomGradientFillKey

		#region RibbonGroupHoverCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupHoverCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupHoverCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupHoverCenterFillKey");

		#endregion RibbonGroupHoverCenterFillKey

		#region RibbonGroupHoverBottomGradientFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupHoverBottomGradientFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupHoverBottomGradientFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupHoverBottomGradientFillKey");

		#endregion RibbonGroupHoverBottomGradientFillKey

		#region RibbonGroupHoverBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupHoverBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupHoverBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupHoverBorderFillKey");

		#endregion RibbonGroupHoverBorderFillKey

		#region RibbonGroupIsInContextualHoverCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupIsInContextualHoverCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupIsInContextualHoverCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupIsInContextualHoverCenterFillKey");

		#endregion RibbonGroupIsInContextualHoverCenterFillKey

		#region QATCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the QATCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey QATCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "QATCenterFillKey");

		#endregion QATCenterFillKey

		#region QATCenterBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the QATCenterBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey QATCenterBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "QATCenterBorderFillKey");

		#endregion QATCenterBorderFillKey

		#region QATInnerBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the QATInnerBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey QATInnerBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "QATInnerBorderFillKey");

		#endregion QATInnerBorderFillKey

		#region QATBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the QATBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey QATBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "QATBorderFillKey");

		#endregion QATBorderFillKey

		#region QATInactiveCenterBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the QATInactiveCenterBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey QATInactiveCenterBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "QATInactiveCenterBorderFillKey");

		#endregion QATInactiveCenterBorderFillKey

		#region QATInactiveCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the QATInactiveCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey QATInactiveCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "QATInactiveCenterFillKey");

		#endregion QATInactiveCenterFillKey

		#region QATActiveGlassCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the QATActiveGlassCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey QATActiveGlassCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "QATActiveGlassCenterFillKey");

		#endregion QATActiveGlassCenterFillKey

		#region QATActiveGlassCenterBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the QATActiveGlassCenterBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey QATActiveGlassCenterBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "QATActiveGlassCenterBorderFillKey");

		#endregion QATActiveGlassCenterBorderFillKey

		#region QATActiveGlassInnerBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the QATActiveGlassInnerBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey QATActiveGlassInnerBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "QATActiveGlassInnerBorderFillKey");

		#endregion QATActiveGlassInnerBorderFillKey

		#region TabIsSelectedTextFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the TabIsSelectedTextFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey TabIsSelectedTextFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "TabIsSelectedTextFillKey");

		#endregion TabIsSelectedTextFillKey

		#region TabNotSelectedTextFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the TabNotSelectedTextFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey TabNotSelectedTextFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "TabNotSelectedTextFillKey");

		#endregion TabNotSelectedTextFillKey

		#region TabSeparatorFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the TabSeparatorFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey TabSeparatorFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "TabSeparatorFillKey");

		#endregion TabSeparatorFillKey

		#region TabActiveNormalLeftHighlightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the TabActiveNormalLeftHighlightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey TabActiveNormalLeftHighlightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "TabActiveNormalLeftHighlightFillKey");

		#endregion TabActiveNormalLeftHighlightFillKey

		#region TabActiveNormalRightHighlightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the TabActiveNormalRightHighlightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey TabActiveNormalRightHighlightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "TabActiveNormalRightHighlightFillKey");

		#endregion TabActiveNormalRightHighlightFillKey

		#region TabActiveNormalTopHighlightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the TabActiveNormalTopHighlightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey TabActiveNormalTopHighlightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "TabActiveNormalTopHighlightFillKey");

		#endregion TabActiveNormalTopHighlightFillKey

		#region TabActiveNormalBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the TabActiveNormalBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey TabActiveNormalBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "TabActiveNormalBorderFillKey");

		#endregion TabActiveNormalBorderFillKey

		#region TabActiveNormalCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the TabActiveNormalCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey TabActiveNormalCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "TabActiveNormalCenterFillKey");

		#endregion TabActiveNormalCenterFillKey

		#region TabActiveNormalInnerBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the TabActiveNormalInnerBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey TabActiveNormalInnerBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "TabActiveNormalInnerBorderFillKey");

		#endregion TabActiveNormalInnerBorderFillKey

		#region TabActiveNormalShadowFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the TabActiveNormalShadowFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey TabActiveNormalShadowFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "TabActiveNormalShadowFillKey");

		#endregion TabActiveNormalShadowFillKey

		#region TabActiveHottrackOuterGlowFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the TabActiveHottrackOuterGlowFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey TabActiveHottrackOuterGlowFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "TabActiveHottrackOuterGlowFillKey");

		#endregion TabActiveHottrackOuterGlowFillKey

		#region TabActiveHoverBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the TabActiveHoverBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey TabActiveHoverBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "TabActiveHoverBorderFillKey");

		#endregion TabActiveHoverBorderFillKey

		#region TabActiveHoverSideHighlightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the TabActiveHoverSideHighlightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey TabActiveHoverSideHighlightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "TabActiveHoverSideHighlightFillKey");

		#endregion TabActiveHoverSideHighlightFillKey

		#region TabActiveHoverTopHighlightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the TabActiveHoverTopHighlightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey TabActiveHoverTopHighlightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "TabActiveHoverTopHighlightFillKey");

		#endregion TabActiveHoverTopHighlightFillKey

		#region ContextualTabActiveBackgroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ContextualTabActiveBackgroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ContextualTabActiveBackgroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ContextualTabActiveBackgroundFillKey");

		#endregion ContextualTabActiveBackgroundFillKey

		#region ContextualTabActiveTopHighlightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ContextualTabActiveTopHighlightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ContextualTabActiveTopHighlightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ContextualTabActiveTopHighlightFillKey");

		#endregion ContextualTabActiveTopHighlightFillKey

		#region ContextualTabHoverCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ContextualTabHoverCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ContextualTabHoverCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ContextualTabHoverCenterFillKey");

		#endregion ContextualTabHoverCenterFillKey

		#region ContextualTabHoverLeftHighlightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ContextualTabHoverLeftHighlightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ContextualTabHoverLeftHighlightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ContextualTabHoverLeftHighlightFillKey");

		#endregion ContextualTabHoverLeftHighlightFillKey

		#region ContextualTabHoverRightHighlightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ContextualTabHoverRightHighlightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ContextualTabHoverRightHighlightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ContextualTabHoverRightHighlightFillKey");

		#endregion ContextualTabHoverRightHighlightFillKey

		#region ContextualTabHoverTopHighlightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ContextualTabHoverTopHighlightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ContextualTabHoverTopHighlightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ContextualTabHoverTopHighlightFillKey");

		#endregion ContextualTabHoverTopHighlightFillKey

		#region ContextualTabActiveNormalBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ContextualTabActiveNormalBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ContextualTabActiveNormalBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ContextualTabActiveNormalBorderFillKey");

		#endregion ContextualTabActiveNormalBorderFillKey

		#region TabInactiveHoverBorderDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the TabInactiveHoverBorderDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey TabInactiveHoverBorderDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "TabInactiveHoverBorderDarkFillKey");

		#endregion TabInactiveHoverBorderDarkFillKey

		#region TabInactiveHoverCenterBackgroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the TabInactiveHoverCenterBackgroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey TabInactiveHoverCenterBackgroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "TabInactiveHoverCenterBackgroundFillKey");

		#endregion TabInactiveHoverCenterBackgroundFillKey

		#region TabInactiveHoverBorderLightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the TabInactiveHoverBorderLightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey TabInactiveHoverBorderLightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "TabInactiveHoverBorderLightFillKey");

		#endregion TabInactiveHoverBorderLightFillKey

		#region TabInactiveHoverCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the TabInactiveHoverCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey TabInactiveHoverCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "TabInactiveHoverCenterFillKey");

		#endregion TabInactiveHoverCenterFillKey

		#region TabInactiveHoverCenterGlowKey

		/// <summary>
		/// The key that identifies a resource to be used as the TabInactiveHoverCenterGlow.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey TabInactiveHoverCenterGlowKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "TabInactiveHoverCenterGlowKey");

		#endregion TabInactiveHoverCenterGlowKey

		#region TabInactiveHoverBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the TabInactiveHoverBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey TabInactiveHoverBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "TabInactiveHoverBorderFillKey");

		#endregion TabInactiveHoverBorderFillKey

		#region TabInactiveHoverBackgroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the TabInactiveHoverBackgroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey TabInactiveHoverBackgroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "TabInactiveHoverBackgroundFillKey");

		#endregion TabInactiveHoverBackgroundFillKey

		#region TabInactiveHoverTopFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the TabInactiveHoverTopFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey TabInactiveHoverTopFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "TabInactiveHoverTopFillKey");

		#endregion TabInactiveHoverTopFillKey

		#region TabContextualHoverBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the TabContextualHoverBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey TabContextualHoverBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "TabContextualHoverBorderFillKey");

		#endregion TabContextualHoverBorderFillKey

		#region ButtonGroupNormalBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonGroupNormalBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonGroupNormalBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonGroupNormalBorderFillKey");

		#endregion ButtonGroupNormalBorderFillKey

		#region ButtonGroupNormalDividerFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonGroupNormalDividerFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonGroupNormalDividerFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonGroupNormalDividerFillKey");

		#endregion ButtonGroupNormalDividerFillKey

		#region ButtonGroupPressedCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonGroupPressedCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonGroupPressedCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonGroupPressedCenterFillKey");

		#endregion ButtonGroupPressedCenterFillKey

		#region ButtonGroupHoverCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonGroupHoverCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonGroupHoverCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonGroupHoverCenterFillKey");

		#endregion ButtonGroupHoverCenterFillKey

		#region ButtonGroupHoverOuterBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonGroupHoverOuterBorderFillKey.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonGroupHoverOuterBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonGroupHoverOuterBorderFillKey");

		#endregion ButtonGroupHoverOuterBorderFillKey

		#region ButtonGroupHoverBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonGroupHoverBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonGroupHoverBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonGroupHoverBorderFillKey");

		#endregion ButtonGroupHoverBorderFillKey

		#region ButtonGroupNormalInnerBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonGroupNormalInnerBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonGroupNormalInnerBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonGroupNormalInnerBorderFillKey");

		#endregion ButtonGroupNormalInnerBorderFillKey

		#region ButtonGroupNormalCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonGroupNormalCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonGroupNormalCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonGroupNormalCenterFillKey");

		#endregion ButtonGroupNormalCenterFillKey

		#region ButtonGroupPressedBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonGroupPressedBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonGroupPressedBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonGroupPressedBorderFillKey");

		#endregion ButtonGroupPressedBorderFillKey

		#region ButtonGroupCheckedCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonGroupCheckedCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonGroupCheckedCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonGroupCheckedCenterFillKey");

		#endregion ButtonGroupCheckedCenterFillKey

		#region ButtonGroupCheckedBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonGroupCheckedBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonGroupCheckedBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonGroupCheckedBorderFillKey");

		#endregion ButtonGroupCheckedBorderFillKey

		#region ButtonGroupCheckedHottrackBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonGroupCheckedHottrackBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonGroupCheckedHottrackBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonGroupCheckedHottrackBorderFillKey");

		#endregion ButtonGroupCheckedHottrackBorderFillKey

		#region ButtonGroupCheckedHottrackCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonGroupCheckedHottrackCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonGroupCheckedHottrackCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonGroupCheckedHottrackCenterFillKey");

		#endregion ButtonGroupCheckedHottrackCenterFillKey

		#region ButtonGroupSegmentedHoverDarkSeparatorFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonGroupSegmentedHoverDarkSeparatorFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonGroupSegmentedHoverDarkSeparatorFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonGroupSegmentedHoverDarkSeparatorFillKey");

		#endregion ButtonGroupSegmentedHoverDarkSeparatorFillKey

		#region ButtonGroupSegmentedHoverLightSeparatorFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonGroupSegmentedHoverLightSeparatorFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonGroupSegmentedHoverLightSeparatorFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonGroupSegmentedHoverLightSeparatorFillKey");

		#endregion ButtonGroupSegmentedHoverLightSeparatorFillKey

		#region ButtonGroupDisabledBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonGroupDisabledBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonGroupDisabledBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonGroupDisabledBorderFillKey");

		#endregion ButtonGroupDisabledBorderFillKey

		#region ButtonGroupDisabledInnerBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonGroupDisabledInnerBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonGroupDisabledInnerBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonGroupDisabledInnerBorderFillKey");

		#endregion ButtonGroupDisabledInnerBorderFillKey

		#region ButtonGroupDisabledCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonGroupDisabledCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonGroupDisabledCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonGroupDisabledCenterFillKey");

		#endregion ButtonGroupDisabledCenterFillKey

		#region ButtonGroupDisabledDarkSeparatorFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonGroupDisabledDarkSeparatorFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonGroupDisabledDarkSeparatorFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonGroupDisabledDarkSeparatorFillKey");

		#endregion ButtonGroupDisabledDarkSeparatorFillKey

		#region MenuHottrackBorderDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the MenuHottrackBorderDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey MenuHottrackBorderDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "MenuHottrackBorderDarkFillKey");

		#endregion MenuHottrackBorderDarkFillKey

		#region MenuHottrackBorderLightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the MenuHottrackBorderLightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey MenuHottrackBorderLightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "MenuHottrackBorderLightFillKey");

		#endregion MenuHottrackBorderLightFillKey

		#region MenuHottrackCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the MenuHottrackCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey MenuHottrackCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "MenuHottrackCenterFillKey");

		#endregion MenuHottrackCenterFillKey

		#region MenuDisabledHottrackBorderDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the MenuDisabledHottrackBorderDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey MenuDisabledHottrackBorderDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "MenuDisabledHottrackBorderDarkFillKey");

		#endregion MenuDisabledHottrackBorderDarkFillKey

		#region MenuDisabledHottrackBorderLightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the MenuDisabledHottrackBorderLightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey MenuDisabledHottrackBorderLightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "MenuDisabledHottrackBorderLightFillKey");

		#endregion MenuDisabledHottrackBorderLightFillKey

		#region MenuDisabledHottrackCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the MenuDisabledHottrackCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey MenuDisabledHottrackCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "MenuDisabledHottrackCenterFillKey");

		#endregion MenuDisabledHottrackCenterFillKey

		#region MenuHottrackCenterSecondaryFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the MenuHottrackCenterSecondaryFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey MenuHottrackCenterSecondaryFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "MenuHottrackCenterSecondaryFillKey");

		#endregion MenuHottrackCenterSecondaryFillKey

		#region CheckBoxNormalOuterBoxBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the CheckBoxNormalOuterBoxBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey CheckBoxNormalOuterBoxBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "CheckBoxNormalOuterBoxBorderFillKey");

		#endregion CheckBoxNormalOuterBoxBorderFillKey

		#region CheckBoxNormalOuterBoxCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the CheckBoxNormalOuterBoxCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey CheckBoxNormalOuterBoxCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "CheckBoxNormalOuterBoxCenterFillKey");

		#endregion CheckBoxNormalOuterBoxCenterFillKey

		#region CheckBoxNormalInnerBoxBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the CheckBoxNormalInnerBoxBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey CheckBoxNormalInnerBoxBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "CheckBoxNormalInnerBoxBorderFillKey");

		#endregion CheckBoxNormalInnerBoxBorderFillKey

		#region CheckBoxNormalInnerBoxCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the CheckBoxNormalInnerBoxCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey CheckBoxNormalInnerBoxCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "CheckBoxNormalInnerBoxCenterFillKey");

		#endregion CheckBoxNormalInnerBoxCenterFillKey

		#region CheckBoxHoverInnerBoxBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the CheckBoxHoverInnerBoxBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey CheckBoxHoverInnerBoxBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "CheckBoxHoverInnerBoxBorderFillKey");

		#endregion CheckBoxHoverInnerBoxBorderFillKey

		#region CheckBoxHoverInnerBoxCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the CheckBoxHoverInnerBoxCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey CheckBoxHoverInnerBoxCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "CheckBoxHoverInnerBoxCenterFillKey");

		#endregion CheckBoxHoverInnerBoxCenterFillKey

		#region CheckBoxHoverOuterBoxBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the CheckBoxHoverOuterBoxBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey CheckBoxHoverOuterBoxBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "CheckBoxHoverOuterBoxBorderFillKey");

		#endregion CheckBoxHoverOuterBoxBorderFillKey

		#region CheckBoxPressedOuterBoxBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the CheckBoxPressedOuterBoxBorderFillKey.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey CheckBoxPressedOuterBoxBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "CheckBoxPressedOuterBoxBorderFillKey");

		#endregion CheckBoxPressedOuterBoxBorderFillKey

		#region CheckBoxPressedInnerBoxCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the CheckBoxPressedInnerBoxCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey CheckBoxPressedInnerBoxCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "CheckBoxPressedInnerBoxCenterFillKey");

		#endregion CheckBoxPressedInnerBoxCenterFillKey

		#region CheckBoxPressedInnerBoxBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the CheckBoxPressedInnerBoxBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey CheckBoxPressedInnerBoxBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "CheckBoxPressedInnerBoxBorderFillKey");

		#endregion CheckBoxPressedInnerBoxBorderFillKey

		#region CheckBoxDisabledOuterBoxBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the CheckBoxDisabledOuterBoxBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey CheckBoxDisabledOuterBoxBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "CheckBoxDisabledOuterBoxBorderFillKey");

		#endregion CheckBoxDisabledOuterBoxBorderFillKey

		#region CheckBoxDisabledOuterBoxCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the CheckBoxDisabledOuterBoxCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey CheckBoxDisabledOuterBoxCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "CheckBoxDisabledOuterBoxCenterFillKey");

		#endregion CheckBoxDisabledOuterBoxCenterFillKey

		#region CheckBoxDisabledInnerBoxBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the CheckBoxDisabledInnerBoxBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey CheckBoxDisabledInnerBoxBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "CheckBoxDisabledInnerBoxBorderFillKey");

		#endregion CheckBoxDisabledInnerBoxBorderFillKey

		#region CheckBoxDisabledInnerBoxCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the CheckBoxDisabledInnerBoxCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey CheckBoxDisabledInnerBoxCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "CheckBoxDisabledInnerBoxCenterFillKey");

		#endregion CheckBoxDisabledInnerBoxCenterFillKey

		#region CheckBoxIndeterminateCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the CheckBoxIndeterminateCenterFillKey.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey CheckBoxIndeterminateCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "CheckBoxIndeterminateCenterFillKey");

		#endregion CheckBoxIndeterminateCenterFillKey

		#region ButtonHoverBorderDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonHoverBorderDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonHoverBorderDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonHoverBorderDarkFillKey");

		#endregion ButtonHoverBorderDarkFillKey

		#region ButtonHoverBorderLightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonHoverBorderLightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonHoverBorderLightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonHoverBorderLightFillKey");

		#endregion ButtonHoverBorderLightFillKey

		#region ButtonHoverCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonHoverCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonHoverCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonHoverCenterFillKey");

		#endregion ButtonHoverCenterFillKey

		#region ButtonPressedBorderDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonPressedBorderDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonPressedBorderDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonPressedBorderDarkFillKey");

		#endregion ButtonPressedBorderDarkFillKey

		#region ButtonPressedBorderLightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonPressedBorderLightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonPressedBorderLightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonPressedBorderLightFillKey");

		#endregion ButtonPressedBorderLightFillKey

		#region ButtonPressedCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonPressedCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonPressedCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonPressedCenterFillKey");

		#endregion ButtonPressedCenterFillKey

		#region ButtonCheckedCenterOverlayFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonCheckedCenterOverlayFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonCheckedCenterOverlayFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonCheckedCenterOverlayFillKey");

		#endregion ButtonCheckedCenterOverlayFillKey

		#region ButtonCheckedCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonCheckedCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonCheckedCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonCheckedCenterFillKey");

		#endregion ButtonCheckedCenterFillKey

		#region ButtonCheckedHottrackCenterOverlayFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonCheckedHottrackCenterOverlayFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonCheckedHottrackCenterOverlayFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonCheckedHottrackCenterOverlayFillKey");

		#endregion ButtonCheckedHottrackCenterOverlayFillKey

		#region ButtonCheckedHottrackCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonCheckedHottrackCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonCheckedHottrackCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonCheckedHottrackCenterFillKey");

		#endregion ButtonCheckedHottrackCenterFillKey

		#region ButtonCheckedHottrackInnerBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonCheckedHottrackInnerBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonCheckedHottrackInnerBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonCheckedHottrackInnerBorderFillKey");

		#endregion ButtonCheckedHottrackInnerBorderFillKey

		#region ButtonCheckedInnerBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonCheckedInnerBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonCheckedInnerBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonCheckedInnerBorderFillKey");

		#endregion ButtonCheckedInnerBorderFillKey

		#region ButtonDisabledBorderDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonDisabledBorderDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonDisabledBorderDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonDisabledBorderDarkFillKey");

		#endregion ButtonDisabledBorderDarkFillKey

		#region ButtonDisabledBorderLightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonDisabledBorderLightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonDisabledBorderLightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonDisabledBorderLightFillKey");

		#endregion ButtonDisabledBorderLightFillKey

		#region ButtonDisabledCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonDisabledCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonDisabledCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonDisabledCenterFillKey");

		#endregion ButtonDisabledCenterFillKey

		#region ButtonToolHoverBorderDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonToolHoverBorderDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonToolHoverBorderDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonToolHoverBorderDarkFillKey");

		#endregion ButtonToolHoverBorderDarkFillKey

		#region ButtonToolHoverBorderLightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonToolHoverBorderLightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonToolHoverBorderLightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonToolHoverBorderLightFillKey");

		#endregion ButtonToolHoverBorderLightFillKey

		#region ButtonToolHoverCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonToolHoverCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonToolHoverCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonToolHoverCenterFillKey");

		#endregion ButtonToolHoverCenterFillKey

		#region GalleryToolDropDownPresenterScrollBarTrackBorderDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolDropDownPresenterScrollBarTrackBorderDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolDropDownPresenterScrollBarTrackBorderDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolDropDownPresenterScrollBarTrackBorderDarkFillKey");

		#endregion GalleryToolDropDownPresenterScrollBarTrackBorderDarkFillKey

		#region GalleryToolDropDownPresenterScrollBarTrackBorderLightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolDropDownPresenterScrollBarTrackBorderLightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolDropDownPresenterScrollBarTrackBorderLightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolDropDownPresenterScrollBarTrackBorderLightFillKey");

		#endregion GalleryToolDropDownPresenterScrollBarTrackBorderLightFillKey

		#region GalleryToolDropDownPresenterScrollBarTrackCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolDropDownPresenterScrollBarTrackCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolDropDownPresenterScrollBarTrackCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolDropDownPresenterScrollBarTrackCenterFillKey");

		#endregion GalleryToolDropDownPresenterScrollBarTrackCenterFillKey

		#region ButtonToolPressedBorderDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonToolPressedBorderDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonToolPressedBorderDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonToolPressedBorderDarkFillKey");

		#endregion ButtonToolPressedBorderDarkFillKey

		#region ButtonToolPressedCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonToolPressedCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonToolPressedCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonToolPressedCenterFillKey");

		#endregion ButtonToolPressedCenterFillKey

		#region ButtonToolPressedInnerBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonToolPressedInnerBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonToolPressedInnerBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonToolPressedInnerBorderFillKey");

		#endregion ButtonToolPressedInnerBorderFillKey

		#region ButtonToolCheckedBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonToolCheckedBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonToolCheckedBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonToolCheckedBorderFillKey");

		#endregion ButtonToolCheckedBorderFillKey

		#region ButtonToolCheckedInnerBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonToolCheckedInnerBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonToolCheckedInnerBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonToolCheckedInnerBorderFillKey");

		#endregion ButtonToolCheckedInnerBorderFillKey

		#region ButtonToolCheckedCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonToolCheckedCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonToolCheckedCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonToolCheckedCenterFillKey");

		#endregion ButtonToolCheckedCenterFillKey

		#region ButtonToolCheckedHottrackBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonToolCheckedHottrackBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonToolCheckedHottrackBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonToolCheckedHottrackBorderFillKey");

		#endregion ButtonToolCheckedHottrackBorderFillKey

		#region ButtonToolCheckedHottrackInnerBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonToolCheckedHottrackInnerBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonToolCheckedHottrackInnerBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonToolCheckedHottrackInnerBorderFillKey");

		#endregion ButtonToolCheckedHottrackInnerBorderFillKey

		#region ButtonToolCheckedHottrackCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonToolCheckedHottrackCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonToolCheckedHottrackCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonToolCheckedHottrackCenterFillKey");

		#endregion ButtonToolCheckedHottrackCenterFillKey

		#region ButtonToolDisabledBorderDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonToolDisabledBorderDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonToolDisabledBorderDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonToolDisabledBorderDarkFillKey");

		#endregion ButtonToolDisabledBorderDarkFillKey

		#region ButtonToolDisabledBorderLightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonToolDisabledBorderLightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonToolDisabledBorderLightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonToolDisabledBorderLightFillKey");

		#endregion ButtonToolDisabledBorderLightFillKey

		#region ButtonToolDisabledCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ButtonToolDisabledCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ButtonToolDisabledCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ButtonToolDisabledCenterFillKey");

		#endregion ButtonToolDisabledCenterFillKey

		#region ToolTipNonScreenTipCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ToolTipNonScreenTipCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ToolTipNonScreenTipCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ToolTipNonScreenTipCenterFillKey");

		#endregion ToolTipNonScreenTipCenterFillKey

		#region ToolTipNonScreenTipBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ToolTipNonScreenTipBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ToolTipNonScreenTipBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ToolTipNonScreenTipBorderFillKey");

		#endregion ToolTipNonScreenTipBorderFillKey

		#region XamScreenTipBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the XamScreenTipBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey XamScreenTipBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "XamScreenTipBorderFillKey");

		#endregion XamScreenTipBorderFillKey

		#region XamScreenTipFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the XamScreenTipFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey XamScreenTipFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "XamScreenTipFillKey");

		#endregion XamScreenTipFillKey

		#region XamScreenTipSeparatorDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the XamScreenTipSeparatorDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey XamScreenTipSeparatorDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "XamScreenTipSeparatorDarkFillKey");

		#endregion XamScreenTipSeparatorDarkFillKey

		#region XamPagerButtonBlueGradientFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the XamPagerButtonBlueGradientFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey XamPagerButtonBlueGradientFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "XamPagerButtonBlueGradientFillKey");

		#endregion XamPagerButtonBlueGradientFillKey

		#region XamPagerButtonLeftInnerBorderGradientFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the XamPagerButtonLeftInnerBorderGradientFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey XamPagerButtonLeftInnerBorderGradientFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "XamPagerButtonLeftInnerBorderGradientFillKey");

		#endregion XamPagerButtonLeftInnerBorderGradientFillKey

		#region XamPagerButtonBorderDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the XamPagerButtonBorderDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey XamPagerButtonBorderDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "XamPagerButtonBorderDarkFillKey");

		#endregion XamPagerButtonBorderDarkFillKey

		#region XamPagerButtonCenterGrayGradientFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the XamPagerButtonCenterGrayGradientFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey XamPagerButtonCenterGrayGradientFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "XamPagerButtonCenterGrayGradientFillKey");

		#endregion XamPagerButtonCenterGrayGradientFillKey

		#region XamPagerButtonLeftOuterBorderGradientFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the XamPagerButtonLeftOuterBorderGradientFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey XamPagerButtonLeftOuterBorderGradientFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "XamPagerButtonLeftOuterBorderGradientFillKey");

		#endregion XamPagerButtonLeftOuterBorderGradientFillKey

		#region XamPagerButtonHoverFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the XamPagerButtonHoverFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey XamPagerButtonHoverFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "XamPagerButtonHoverFillKey");

		#endregion XamPagerButtonHoverFillKey

		#region XamPagerButtonHoverBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the XamPagerButtonHoverBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey XamPagerButtonHoverBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "XamPagerButtonHoverBorderFillKey");

		#endregion XamPagerButtonHoverBorderFillKey

		#region XamPagerButtonPressedBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the XamPagerButtonPressedBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey XamPagerButtonPressedBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "XamPagerButtonPressedBorderFillKey");

		#endregion XamPagerButtonPressedBorderFillKey

		#region XamPagerButtonPressedCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the XamPagerButtonPressedCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey XamPagerButtonPressedCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "XamPagerButtonPressedCenterFillKey");

		#endregion XamPagerButtonPressedCenterFillKey

		#region GalleryToolDropDownPresenterBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolDropDownPresenterBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolDropDownPresenterBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolDropDownPresenterBorderFillKey");

		#endregion GalleryToolDropDownPresenterBorderFillKey

		#region GalleryToolDropDownPresenterCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolDropDownPresenterCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolDropDownPresenterCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolDropDownPresenterCenterFillKey");

		#endregion GalleryToolDropDownPresenterCenterFillKey

		#region GalleryToolDropDownPresenterFooterBarStrokeKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolDropDownPresenterFooterBarStroke.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolDropDownPresenterFooterBarStrokeKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolDropDownPresenterFooterBarStrokeKey");

		#endregion GalleryToolDropDownPresenterFooterBarStrokeKey

		#region GalleryToolDropDownPresenterFooterBarGradientKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolDropDownPresenterFooterBarGradient.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolDropDownPresenterFooterBarGradientKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolDropDownPresenterFooterBarGradientKey");

		#endregion GalleryToolDropDownPresenterFooterBarGradientKey

		#region GalleryToolDropDownPresenterFooterBarGlyphFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolDropDownPresenterFooterBarGlyphFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolDropDownPresenterFooterBarGlyphFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolDropDownPresenterFooterBarGlyphFillKey");

		#endregion GalleryToolDropDownPresenterFooterBarGlyphFillKey

		#region GalleryToolDropDownPresenterScrollBarArrowFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolDropDownPresenterScrollBarArrowFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolDropDownPresenterScrollBarArrowFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolDropDownPresenterScrollBarArrowFillKey");

		#endregion GalleryToolDropDownPresenterScrollBarArrowFillKey

		#region GalleryToolDropDownPresenterScrollBarDisabledArrowFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolDropDownPresenterScrollBarDisabledArrowFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolDropDownPresenterScrollBarDisabledArrowFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolDropDownPresenterScrollBarDisabledArrowFillKey");

		#endregion GalleryToolDropDownPresenterScrollBarDisabledArrowFillKey

		#region GalleryToolDropDownPresenterScrollBarThumbGripperForegroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolDropDownPresenterScrollBarThumbGripperForegroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolDropDownPresenterScrollBarThumbGripperForegroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolDropDownPresenterScrollBarThumbGripperForegroundFillKey");

		#endregion GalleryToolDropDownPresenterScrollBarThumbGripperForegroundFillKey

		#region GalleryToolDropDownPresenterScrollBarThumbGripperBackgroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolDropDownPresenterScrollBarThumbGripperBackgroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolDropDownPresenterScrollBarThumbGripperBackgroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolDropDownPresenterScrollBarThumbGripperBackgroundFillKey");

		#endregion GalleryToolDropDownPresenterScrollBarThumbGripperBackgroundFillKey

		#region GalleryToolDropDownPresenterScrollBarPressedBorderDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolDropDownPresenterScrollBarPressedBorderDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolDropDownPresenterScrollBarPressedBorderDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolDropDownPresenterScrollBarPressedBorderDarkFillKey");

		#endregion GalleryToolDropDownPresenterScrollBarPressedBorderDarkFillKey

		#region GalleryToolDropDownPresenterScrollBarPressedBorderLightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolDropDownPresenterScrollBarPressedBorderLightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolDropDownPresenterScrollBarPressedBorderLightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolDropDownPresenterScrollBarPressedBorderLightFillKey");

		#endregion GalleryToolDropDownPresenterScrollBarPressedBorderLightFillKey

		#region GalleryToolDropDownPresenterScrollBarPressedCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolDropDownPresenterScrollBarPressedCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolDropDownPresenterScrollBarPressedCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolDropDownPresenterScrollBarPressedCenterFillKey");

		#endregion GalleryToolDropDownPresenterScrollBarPressedCenterFillKey

		#region GalleryToolDropDownPresenterScrollBarHoverBorderDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolDropDownPresenterScrollBarHoverBorderDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolDropDownPresenterScrollBarHoverBorderDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolDropDownPresenterScrollBarHoverBorderDarkFillKey");

		#endregion GalleryToolDropDownPresenterScrollBarHoverBorderDarkFillKey

		#region GalleryToolDropDownPresenterScrollBarHoverBorderLightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolDropDownPresenterScrollBarHoverBorderLightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolDropDownPresenterScrollBarHoverBorderLightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolDropDownPresenterScrollBarHoverBorderLightFillKey");

		#endregion GalleryToolDropDownPresenterScrollBarHoverBorderLightFillKey

		#region GalleryToolDropDownPresenterScrollBarHoverCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolDropDownPresenterScrollBarHoverCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolDropDownPresenterScrollBarHoverCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolDropDownPresenterScrollBarHoverCenterFillKey");

		#endregion GalleryToolDropDownPresenterScrollBarHoverCenterFillKey

		#region GalleryToolDropDownPresenterScrollBarRepeatButtonNormalBorderDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolDropDownPresenterScrollBarPressedBorderDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolDropDownPresenterScrollBarRepeatButtonNormalBorderDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolDropDownPresenterScrollBarRepeatButtonNormalBorderDarkFillKey");

		#endregion GalleryToolDropDownPresenterScrollBarRepeatButtonNormalBorderDarkFillKey

		#region GalleryToolDropDownPresenterScrollBarRepeatButtonNormalBorderLightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolDropDownPresenterScrollBarPressedBorderLightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolDropDownPresenterScrollBarRepeatButtonNormalBorderLightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolDropDownPresenterScrollBarRepeatButtonNormalBorderLightFillKey");

		#endregion GalleryToolDropDownPresenterScrollBarRepeatButtonNormalBorderLightFillKey

		#region GalleryToolDropDownPresenterScrollBarRepeatButtonNormalCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolDropDownPresenterScrollBarPressedCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolDropDownPresenterScrollBarRepeatButtonNormalCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolDropDownPresenterScrollBarRepeatButtonNormalCenterFillKey");

		#endregion GalleryToolDropDownPresenterScrollBarRepeatButtonNormalCenterFillKey

		#region GalleryToolDropDownPresenterScrollBarRepeatButtonHoverBorderDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolDropDownPresenterScrollBarHoverBorderDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolDropDownPresenterScrollBarRepeatButtonHoverBorderDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolDropDownPresenterScrollBarRepeatButtonHoverBorderDarkFillKey");

		#endregion GalleryToolDropDownPresenterScrollBarRepeatButtonHoverBorderDarkFillKey

		#region GalleryToolDropDownPresenterScrollBarRepeatButtonHoverBorderLightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolDropDownPresenterScrollBarHoverBorderLightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolDropDownPresenterScrollBarRepeatButtonHoverBorderLightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolDropDownPresenterScrollBarRepeatButtonHoverBorderLightFillKey");

		#endregion GalleryToolDropDownPresenterScrollBarRepeatButtonHoverBorderLightFillKey

		#region GalleryToolDropDownPresenterScrollBarRepeatButtonHoverCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolDropDownPresenterScrollBarHoverCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolDropDownPresenterScrollBarRepeatButtonHoverCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolDropDownPresenterScrollBarRepeatButtonHoverCenterFillKey");

		#endregion GalleryToolDropDownPresenterScrollBarRepeatButtonHoverCenterFillKey

		#region GalleryToolDropDownPresenterScrollBarRepeatButtonPressedBorderDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolDropDownPresenterScrollBarPressedBorderDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolDropDownPresenterScrollBarRepeatButtonPressedBorderDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolDropDownPresenterScrollBarRepeatButtonPressedBorderDarkFillKey");

		#endregion GalleryToolDropDownPresenterScrollBarRepeatButtonPressedBorderDarkFillKey

		#region GalleryToolDropDownPresenterScrollBarRepeatButtonPressedBorderLightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolDropDownPresenterScrollBarPressedBorderLightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolDropDownPresenterScrollBarRepeatButtonPressedBorderLightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolDropDownPresenterScrollBarRepeatButtonPressedBorderLightFillKey");

		#endregion GalleryToolDropDownPresenterScrollBarRepeatButtonPressedBorderLightFillKey

		#region GalleryToolDropDownPresenterScrollBarRepeatButtonPressedCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolDropDownPresenterScrollBarPressedCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolDropDownPresenterScrollBarRepeatButtonPressedCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolDropDownPresenterScrollBarRepeatButtonPressedCenterFillKey");

		#endregion GalleryToolDropDownPresenterScrollBarRepeatButtonPressedCenterFillKey

		#region GalleryToolPreviewPresenterBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolPreviewPresenterBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolPreviewPresenterBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolPreviewPresenterBorderFillKey");

		#endregion GalleryToolPreviewPresenterBorderFillKey

		#region GalleryToolPreviewPresenterScrollButtonNormalBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolPreviewPresenterScrollButtonNormalBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolPreviewPresenterScrollButtonNormalBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolPreviewPresenterScrollButtonNormalBorderFillKey");

		#endregion GalleryToolPreviewPresenterScrollButtonNormalBorderFillKey

		#region GalleryToolPreviewPresenterScrollButtonNormalCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolPreviewPresenterScrollButtonNormalCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolPreviewPresenterScrollButtonNormalCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolPreviewPresenterScrollButtonNormalCenterFillKey");

		#endregion GalleryToolPreviewPresenterScrollButtonNormalCenterFillKey

		#region GalleryToolPreviewPresenterScrollButtonNormalInnerBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolPreviewPresenterScrollButtonNormalInnerBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolPreviewPresenterScrollButtonNormalInnerBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolPreviewPresenterScrollButtonNormalInnerBorderFillKey");

		#endregion GalleryToolPreviewPresenterScrollButtonNormalInnerBorderFillKey

		#region GalleryToolPreviewPresenterScrollButtonHoverCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolPreviewPresenterScrollButtonHoverCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolPreviewPresenterScrollButtonHoverCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolPreviewPresenterScrollButtonHoverCenterFillKey");

		#endregion GalleryToolPreviewPresenterScrollButtonHoverCenterFillKey

		#region GalleryToolPreviewPresenterScrollButtonHoverInnerBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolPreviewPresenterScrollButtonHoverInnerBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolPreviewPresenterScrollButtonHoverInnerBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolPreviewPresenterScrollButtonHoverInnerBorderFillKey");

		#endregion GalleryToolPreviewPresenterScrollButtonHoverInnerBorderFillKey

		#region GalleryToolPreviewPresenterScrollButtonHoverBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolPreviewPresenterScrollButtonHoverBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolPreviewPresenterScrollButtonHoverBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolPreviewPresenterScrollButtonHoverBorderFillKey");

		#endregion GalleryToolPreviewPresenterScrollButtonHoverBorderFillKey

		#region GalleryToolPreviewPresenterScrollButtonDisabledBorderDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolPreviewPresenterScrollButtonDisabledBorderDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolPreviewPresenterScrollButtonDisabledBorderDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolPreviewPresenterScrollButtonDisabledBorderDarkFillKey");

		#endregion GalleryToolPreviewPresenterScrollButtonDisabledBorderDarkFillKey

		#region GalleryToolPreviewPresenterScrollUpButtonDisabledBorderLightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolPreviewPresenterScrollUpButtonDisabledBorderLightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolPreviewPresenterScrollUpButtonDisabledBorderLightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolPreviewPresenterScrollUpButtonDisabledBorderLightFillKey");

		#endregion GalleryToolPreviewPresenterScrollUpButtonDisabledBorderLightFillKey

		#region GalleryToolPreviewPresenterScrollUpButtonDisabledCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolPreviewPresenterScrollUpButtonDisabledCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolPreviewPresenterScrollUpButtonDisabledCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolPreviewPresenterScrollUpButtonDisabledCenterFillKey");

		#endregion GalleryToolPreviewPresenterScrollUpButtonDisabledCenterFillKey

		#region GalleryToolPreviewPresenterScrollDownButtonDisabledBorderLightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolPreviewPresenterScrollDownButtonDisabledBorderLightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolPreviewPresenterScrollDownButtonDisabledBorderLightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolPreviewPresenterScrollDownButtonDisabledBorderLightFillKey");

		#endregion GalleryToolPreviewPresenterScrollDownButtonDisabledBorderLightFillKey

		#region GalleryToolPreviewPresenterScrollDownButtonDisabledCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolPreviewPresenterScrollDownButtonDisabledCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolPreviewPresenterScrollDownButtonDisabledCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolPreviewPresenterScrollDownButtonDisabledCenterFillKey");

		#endregion GalleryToolPreviewPresenterScrollDownButtonDisabledCenterFillKey

		#region ApplicationMenuChromeFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ApplicationMenuChromeFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ApplicationMenuChromeFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ApplicationMenuChromeFillKey");

		#endregion ApplicationMenuChromeFillKey

		#region GalleryToolPreviewPresenterScrollUpButtonPressedCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolPreviewPresenterScrollUpButtonPressedCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolPreviewPresenterScrollUpButtonPressedCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolPreviewPresenterScrollUpButtonPressedCenterFillKey");

		#endregion GalleryToolPreviewPresenterScrollUpButtonPressedCenterFillKey

		#region GalleryToolPreviewPresenterScrollUpButtonPressedBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolPreviewPresenterScrollUpButtonPressedBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolPreviewPresenterScrollUpButtonPressedBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolPreviewPresenterScrollUpButtonPressedBorderFillKey");

		#endregion GalleryToolPreviewPresenterScrollUpButtonPressedBorderFillKey

		#region GalleryToolPreviewPresenterScrollUpButtonPressedInnerBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolPreviewPresenterScrollUpButtonPressedInnerBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolPreviewPresenterScrollUpButtonPressedInnerBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolPreviewPresenterScrollUpButtonPressedInnerBorderFillKey");

		#endregion GalleryToolPreviewPresenterScrollUpButtonPressedInnerBorderFillKey

		#region GalleryToolPreviewPresenterScrollDownButtonPressedCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolPreviewPresenterScrollDownButtonPressedCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolPreviewPresenterScrollDownButtonPressedCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolPreviewPresenterScrollDownButtonPressedCenterFillKey");

		#endregion GalleryToolPreviewPresenterScrollDownButtonPressedCenterFillKey

		#region GalleryToolPreviewPresenterScrollDownButtonPressedInnerBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolPreviewPresenterScrollDownButtonPressedInnerBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolPreviewPresenterScrollDownButtonPressedInnerBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolPreviewPresenterScrollDownButtonPressedInnerBorderFillKey");

		#endregion GalleryToolPreviewPresenterScrollDownButtonPressedInnerBorderFillKey

		#region DialogBoxLauncherToolTemplateHoverBorderDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the DialogBoxLauncherToolTemplateHoverBorderDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey DialogBoxLauncherToolTemplateHoverBorderDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "DialogBoxLauncherToolTemplateHoverBorderDarkFillKey");

		#endregion DialogBoxLauncherToolTemplateHoverBorderDarkFillKey

		#region DialogBoxLauncherToolTemplateHoverBorderLightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the DialogBoxLauncherToolTemplateHoverBorderLightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey DialogBoxLauncherToolTemplateHoverBorderLightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "DialogBoxLauncherToolTemplateHoverBorderLightFillKey");

		#endregion DialogBoxLauncherToolTemplateHoverBorderLightFillKey

		#region DialogBoxLauncherToolTemplateHoverCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the DialogBoxLauncherToolTemplateHoverCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey DialogBoxLauncherToolTemplateHoverCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "DialogBoxLauncherToolTemplateHoverCenterFillKey");

		#endregion DialogBoxLauncherToolTemplateHoverCenterFillKey

		#region DialogBoxLauncherToolTemplatePressedBorderDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the DialogBoxLauncherToolTemplatePressedBorderDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey DialogBoxLauncherToolTemplatePressedBorderDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "DialogBoxLauncherToolTemplatePressedBorderDarkFillKey");

		#endregion DialogBoxLauncherToolTemplatePressedBorderDarkFillKey

		#region DialogBoxLauncherToolTemplatePressedBorderLightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the DialogBoxLauncherToolTemplatePressedBorderLightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey DialogBoxLauncherToolTemplatePressedBorderLightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "DialogBoxLauncherToolTemplatePressedBorderLightFillKey");

		#endregion DialogBoxLauncherToolTemplatePressedBorderLightFillKey

		#region DialogBoxLauncherToolTemplatePressedCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the DialogBoxLauncherToolTemplatePressedCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey DialogBoxLauncherToolTemplatePressedCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "DialogBoxLauncherToolTemplatePressedCenterFillKey");

		#endregion DialogBoxLauncherToolTemplatePressedCenterFillKey

		#region DialogBoxLauncherToolTemplateGlyphNormalForegroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the DialogBoxLauncherToolTemplateGlyphNormalForegroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey DialogBoxLauncherToolTemplateGlyphNormalForegroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "DialogBoxLauncherToolTemplateGlyphNormalForegroundFillKey");

		#endregion DialogBoxLauncherToolTemplateGlyphNormalForegroundFillKey

		#region DialogBoxLauncherToolTemplateGlyphNormalBackgroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the DialogBoxLauncherToolTemplateGlyphNormalBackgroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey DialogBoxLauncherToolTemplateGlyphNormalBackgroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "DialogBoxLauncherToolTemplateGlyphNormalBackgroundFillKey");

		#endregion DialogBoxLauncherToolTemplateGlyphNormalBackgroundFillKey

		#region DialogBoxLauncherToolTemplateGlyphHoverForegroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the DialogBoxLauncherToolTemplateGlyphHoverForegroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey DialogBoxLauncherToolTemplateGlyphHoverForegroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "DialogBoxLauncherToolTemplateGlyphHoverForegroundFillKey");

		#endregion DialogBoxLauncherToolTemplateGlyphHoverForegroundFillKey

		#region DialogBoxLauncherToolTemplateGlyphHoverBackgroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the DialogBoxLauncherToolTemplateGlyphHoverBackgroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey DialogBoxLauncherToolTemplateGlyphHoverBackgroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "DialogBoxLauncherToolTemplateGlyphHoverBackgroundFillKey");

		#endregion DialogBoxLauncherToolTemplateGlyphHoverBackgroundFillKey

		#region DialogBoxLauncherToolTemplateGlyphPressedForegroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the DialogBoxLauncherToolTemplateGlyphPressedForegroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey DialogBoxLauncherToolTemplateGlyphPressedForegroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "DialogBoxLauncherToolTemplateGlyphPressedForegroundFillKey");

		#endregion DialogBoxLauncherToolTemplateGlyphPressedForegroundFillKey

		#region DialogBoxLauncherToolTemplateGlyphPressedBackgroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the DialogBoxLauncherToolTemplateGlyphPressedBackgroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey DialogBoxLauncherToolTemplateGlyphPressedBackgroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "DialogBoxLauncherToolTemplateGlyphPressedBackgroundFillKey");

		#endregion DialogBoxLauncherToolTemplateGlyphPressedBackgroundFillKey

		#region GalleryItemPresenterSelectedBorderDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryItemPresenterSelectedBorderDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryItemPresenterSelectedBorderDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryItemPresenterSelectedBorderDarkFillKey");

		#endregion GalleryItemPresenterSelectedBorderDarkFillKey

		#region GalleryItemPresenterSelectedBorderLightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryItemPresenterSelectedBorderLightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryItemPresenterSelectedBorderLightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryItemPresenterSelectedBorderLightFillKey");

		#endregion GalleryItemPresenterSelectedBorderLightFillKey

		#region GalleryItemPresenterSelectedBorderLightFillInnerKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryItemPresenterSelectedBorderLightFillInner.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryItemPresenterSelectedBorderLightFillInnerKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryItemPresenterSelectedBorderLightFillInnerKey");

		#endregion GalleryItemPresenterSelectedBorderLightFillInnerKey

		#region GalleryItemPresenterSelectedCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryItemPresenterSelectedCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryItemPresenterSelectedCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryItemPresenterSelectedCenterFillKey");

		#endregion GalleryItemPresenterSelectedCenterFillKey

		#region GalleryItemPresenterHoverBorderDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryItemPresenterHoverBorderDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryItemPresenterHoverBorderDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryItemPresenterHoverBorderDarkFillKey");

		#endregion GalleryItemPresenterHoverBorderDarkFillKey

		#region GalleryItemPresenterHoverBorderLightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryItemPresenterHoverBorderLightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryItemPresenterHoverBorderLightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryItemPresenterHoverBorderLightFillKey");

		#endregion GalleryItemPresenterHoverBorderLightFillKey

		#region GalleryItemPresenterHoverCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryItemPresenterHoverCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryItemPresenterHoverCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryItemPresenterHoverCenterFillKey");

		#endregion GalleryItemPresenterHoverCenterFillKey

		#region GalleryItemPresenterHoverBottomOverlayGradientKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryItemPresenterHoverBottomOverlayGradient.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryItemPresenterHoverBottomOverlayGradientKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryItemPresenterHoverBottomOverlayGradientKey");

		#endregion GalleryItemPresenterHoverBottomOverlayGradientKey

		#region GalleryItemPresenterSelectedBottomOverlayGradientKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryItemPresenterSelectedBottomOverlayGradient.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryItemPresenterSelectedBottomOverlayGradientKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryItemPresenterSelectedBottomOverlayGradientKey");

		#endregion GalleryItemPresenterSelectedBottomOverlayGradientKey

		#region RibbonGroupCollapsedInnerAreaNormalBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupCollapsedInnerAreaNormalBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupCollapsedInnerAreaNormalBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupCollapsedInnerAreaNormalBorderFillKey");

		#endregion RibbonGroupCollapsedInnerAreaNormalBorderFillKey

		#region RibbonGroupCollapsedInnerAreaNormalCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupCollapsedInnerAreaNormalCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupCollapsedInnerAreaNormalCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupCollapsedInnerAreaNormalCenterFillKey");

		#endregion RibbonGroupCollapsedInnerAreaNormalCenterFillKey

		#region RibbonGroupCollapsedInnerAreaNormalBottomGradientFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupCollapsedInnerAreaNormalBottomGradientFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupCollapsedInnerAreaNormalBottomGradientFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupCollapsedInnerAreaNormalBottomGradientFillKey");

		#endregion RibbonGroupCollapsedInnerAreaNormalBottomGradientFillKey

		#region RibbonGroupCollapsedInnerAreaHoverBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupCollapsedInnerAreaHoverBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupCollapsedInnerAreaHoverBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupCollapsedInnerAreaHoverBorderFillKey");

		#endregion RibbonGroupCollapsedInnerAreaHoverBorderFillKey

		#region RibbonGroupCollapsedInnerAreaHoverCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupCollapsedInnerAreaHoverCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupCollapsedInnerAreaHoverCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupCollapsedInnerAreaHoverCenterFillKey");

		#endregion RibbonGroupCollapsedInnerAreaHoverCenterFillKey

		#region RibbonGroupCollapsedInnerAreaHoverBottomGradientFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupCollapsedInnerAreaHoverBottomGradientFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupCollapsedInnerAreaHoverBottomGradientFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupCollapsedInnerAreaHoverBottomGradientFillKey");

		#endregion RibbonGroupCollapsedInnerAreaHoverBottomGradientFillKey

		#region RibbonGroupCollapsedInnerAreaPressedBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupCollapsedInnerAreaPressedBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupCollapsedInnerAreaPressedBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupCollapsedInnerAreaPressedBorderFillKey");

		#endregion RibbonGroupCollapsedInnerAreaPressedBorderFillKey

		#region RibbonGroupCollapsedInnerAreaPressedCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupCollapsedInnerAreaPressedCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupCollapsedInnerAreaPressedCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupCollapsedInnerAreaPressedCenterFillKey");

		#endregion RibbonGroupCollapsedInnerAreaPressedCenterFillKey

		#region RibbonGroupCollapsedInnerAreaPressedBottomGradientFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupCollapsedInnerAreaPressedBottomGradientFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupCollapsedInnerAreaPressedBottomGradientFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupCollapsedInnerAreaPressedBottomGradientFillKey");

		#endregion RibbonGroupCollapsedInnerAreaPressedBottomGradientFillKey

		#region RibbonGroupCollapsedInnerAreaIsActiveBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupCollapsedInnerAreaIsActiveBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupCollapsedInnerAreaIsActiveBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupCollapsedInnerAreaIsActiveBorderFillKey");

		#endregion RibbonGroupCollapsedInnerAreaIsActiveBorderFillKey

		#region RibbonGroupCollapsedInnerAreaIsActiveCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupCollapsedInnerAreaIsActiveCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupCollapsedInnerAreaIsActiveCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupCollapsedInnerAreaIsActiveCenterFillKey");

		#endregion RibbonGroupCollapsedInnerAreaIsActiveCenterFillKey

		#region RibbonGroupCollapsedInnerAreaIsActiveBottomGradientFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupCollapsedInnerAreaIsActiveBottomGradientFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupCollapsedInnerAreaIsActiveBottomGradientFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupCollapsedInnerAreaIsActiveBottomGradientFillKey");

		#endregion RibbonGroupCollapsedInnerAreaIsActiveBottomGradientFillKey

		#region RibbonGroupCollapsedNormalBorderDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupCollapsedNormalBorderDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupCollapsedNormalBorderDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupCollapsedNormalBorderDarkFillKey");

		#endregion RibbonGroupCollapsedNormalBorderDarkFillKey

		#region RibbonGroupCollapsedNormalBorderLightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupCollapsedNormalBorderLightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupCollapsedNormalBorderLightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupCollapsedNormalBorderLightFillKey");

		#endregion RibbonGroupCollapsedNormalBorderLightFillKey

		#region RibbonGroupCollapsedNormalCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupCollapsedNormalCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupCollapsedNormalCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupCollapsedNormalCenterFillKey");

		#endregion RibbonGroupCollapsedNormalCenterFillKey

		#region RibbonGroupCollapsedHoverBorderDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupCollapsedHoverBorderDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupCollapsedHoverBorderDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupCollapsedHoverBorderDarkFillKey");

		#endregion RibbonGroupCollapsedHoverBorderDarkFillKey

		#region RibbonGroupCollapsedHoverBorderLightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupCollapsedHoverBorderLightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupCollapsedHoverBorderLightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupCollapsedHoverBorderLightFillKey");

		#endregion RibbonGroupCollapsedHoverBorderLightFillKey

		#region RibbonGroupCollapsedHoverCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupCollapsedHoverCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupCollapsedHoverCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupCollapsedHoverCenterFillKey");

		#endregion RibbonGroupCollapsedHoverCenterFillKey

		#region RibbonGroupCollapsedPressedBorderDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupCollapsedPressedBorderDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupCollapsedPressedBorderDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupCollapsedPressedBorderDarkFillKey");

		#endregion RibbonGroupCollapsedPressedBorderDarkFillKey

		#region RibbonGroupCollapsedPressedBorderLightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupCollapsedPressedBorderLightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupCollapsedPressedBorderLightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupCollapsedPressedBorderLightFillKey");

		#endregion RibbonGroupCollapsedPressedBorderLightFillKey

		#region RibbonGroupCollapsedPressedCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupCollapsedPressedCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupCollapsedPressedCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupCollapsedPressedCenterFillKey");

		#endregion RibbonGroupCollapsedPressedCenterFillKey

		#region RibbonGroupCollapsedPressedHighlightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupCollapsedPressedHighlightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupCollapsedPressedHighlightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupCollapsedPressedHighlightFillKey");

		#endregion RibbonGroupCollapsedPressedHighlightFillKey

		#region RibbonGroupCollapsedIsActiveBorderDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupCollapsedIsActiveBorderDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupCollapsedIsActiveBorderDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupCollapsedIsActiveBorderDarkFillKey");

		#endregion RibbonGroupCollapsedIsActiveBorderDarkFillKey

		#region RibbonGroupCollapsedIsActiveBorderLightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupCollapsedIsActiveBorderLightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupCollapsedIsActiveBorderLightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupCollapsedIsActiveBorderLightFillKey");

		#endregion RibbonGroupCollapsedIsActiveBorderLightFillKey

		#region RibbonGroupCollapsedIsActiveCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupCollapsedIsActiveCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupCollapsedIsActiveCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupCollapsedIsActiveCenterFillKey");

		#endregion RibbonGroupCollapsedIsActiveCenterFillKey

		#region RibbonGroupCollapsedIsActiveHighlightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupCollapsedIsActiveHighlightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupCollapsedIsActiveHighlightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupCollapsedIsActiveHighlightFillKey");

		#endregion RibbonGroupCollapsedIsActiveHighlightFillKey

		#region RibbonGroupCollapsedIsInContextualPressedBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupCollapsedIsInContextualPressedBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupCollapsedIsInContextualPressedBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupCollapsedIsInContextualPressedBorderFillKey");

		#endregion RibbonGroupCollapsedIsInContextualPressedBorderFillKey

		#region RibbonGroupCollapsedIsInContextualPressedCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupCollapsedIsInContextualPressedCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupCollapsedIsInContextualPressedCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupCollapsedIsInContextualPressedCenterFillKey");

		#endregion RibbonGroupCollapsedIsInContextualPressedCenterFillKey

		#region RibbonGroupCollapsedIsInContextualHoverBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupCollapsedIsInContextualHoverBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupCollapsedIsInContextualHoverBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupCollapsedIsInContextualHoverBorderFillKey");

		#endregion RibbonGroupCollapsedIsInContextualHoverBorderFillKey

		#region RibbonGroupCollapsedIsInContextualNormalCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupCollapsedIsInContextualNormalCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupCollapsedIsInContextualNormalCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupCollapsedIsInContextualNormalCenterFillKey");

		#endregion RibbonGroupCollapsedIsInContextualNormalCenterFillKey

		#region RibbonGroupCollapsedIsInContextualNormalBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupCollapsedIsInContextualNormalBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupCollapsedIsInContextualNormalBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupCollapsedIsInContextualNormalBorderFillKey");

		#endregion RibbonGroupCollapsedIsInContextualNormalBorderFillKey

		#region RibbonGroupQATBorderDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupQATBorderDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupQATBorderDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupQATBorderDarkFillKey");

		#endregion RibbonGroupQATBorderDarkFillKey

		#region RibbonGroupQATNormalCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupQATNormalCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupQATNormalCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupQATNormalCenterFillKey");

		#endregion RibbonGroupQATNormalCenterFillKey

		#region RibbonGroupQATHoverBorderDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupQATHoverBorderDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupQATHoverBorderDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupQATHoverBorderDarkFillKey");

		#endregion RibbonGroupQATHoverBorderDarkFillKey

		#region RibbonGroupQATHover1InnerBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupQATHover1InnerBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupQATHover1InnerBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupQATHover1InnerBorderFillKey");

		#endregion RibbonGroupQATHover1InnerBorderFillKey

		#region RibbonGroupQATHover2InnerBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupQATHover2InnerBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupQATHover2InnerBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupQATHover2InnerBorderFillKey");

		#endregion RibbonGroupQATHover2InnerBorderFillKey

		#region RibbonGroupQATHover3InnerBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupQATHover3InnerBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupQATHover3InnerBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupQATHover3InnerBorderFillKey");

		#endregion RibbonGroupQATHover3InnerBorderFillKey

		#region RibbonGroupQATHoverCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupQATHoverCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupQATHoverCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupQATHoverCenterFillKey");

		#endregion RibbonGroupQATHoverCenterFillKey

		#region RibbonGroupQATPressed1InnerBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupQATPressed1InnerBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupQATPressed1InnerBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupQATPressed1InnerBorderFillKey");

		#endregion RibbonGroupQATPressed1InnerBorderFillKey

		#region RibbonGroupQATPressed2InnerBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupQATPressed2InnerBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupQATPressed2InnerBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupQATPressed2InnerBorderFillKey");

		#endregion RibbonGroupQATPressed2InnerBorderFillKey

		#region RibbonGroupQATPressedCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupQATPressedCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupQATPressedCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupQATPressedCenterFillKey");

		#endregion RibbonGroupQATPressedCenterFillKey

		#region RibbonGroupQATVistaNormalBorderDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupQATVistaNormalBorderDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupQATVistaNormalBorderDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupQATVistaNormalBorderDarkFillKey");

		#endregion RibbonGroupQATVistaNormalBorderDarkFillKey

		#region RibbonGroupQATVistaHoverBorderDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupQATVistaHoverBorderDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupQATVistaHoverBorderDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupQATVistaHoverBorderDarkFillKey");

		#endregion RibbonGroupQATVistaHoverBorderDarkFillKey

		#region RibbonGroupQATVistaHoverCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupQATVistaHoverCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupQATVistaHoverCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupQATVistaHoverCenterFillKey");

		#endregion RibbonGroupQATVistaHoverCenterFillKey

		#region RibbonGroupQATVistaHover1InnerBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupQATVistaHover1InnerBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupQATVistaHover1InnerBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupQATVistaHover1InnerBorderFillKey");

		#endregion RibbonGroupQATVistaHover1InnerBorderFillKey

		#region RibbonGroupQATVistaHover2InnerBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupQATVistaHover2InnerBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupQATVistaHover2InnerBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupQATVistaHover2InnerBorderFillKey");

		#endregion RibbonGroupQATVistaHover2InnerBorderFillKey

		#region RibbonGroupQATVistaHover3InnerBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupQATVistaHover3InnerBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupQATVistaHover3InnerBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupQATVistaHover3InnerBorderFillKey");

		#endregion RibbonGroupQATVistaHover3InnerBorderFillKey

		#region ApplicationMenuButtonNormalOuterBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ApplicationMenuButtonNormalOuterBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ApplicationMenuButtonNormalOuterBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ApplicationMenuButtonNormalOuterBorderFillKey");

		#endregion ApplicationMenuButtonNormalOuterBorderFillKey

		#region ApplicationMenuButtonNormalInnerBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ApplicationMenuButtonNormalInnerBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ApplicationMenuButtonNormalInnerBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ApplicationMenuButtonNormalInnerBorderFillKey");

		#endregion ApplicationMenuButtonNormalInnerBorderFillKey

		#region ApplicationMenuButtonNormalCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ApplicationMenuButtonNormalCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ApplicationMenuButtonNormalCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ApplicationMenuButtonNormalCenterFillKey");

		#endregion ApplicationMenuButtonNormalCenterFillKey

		#region ApplicationMenuButtonNormalHighlightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ApplicationMenuButtonNormalHighlightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ApplicationMenuButtonNormalHighlightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ApplicationMenuButtonNormalHighlightFillKey");

		#endregion ApplicationMenuButtonNormalHighlightFillKey

		#region ApplicationMenuButtonHoverOuterBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ApplicationMenuButtonHoverOuterBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ApplicationMenuButtonHoverOuterBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ApplicationMenuButtonHoverOuterBorderFillKey");

		#endregion ApplicationMenuButtonHoverOuterBorderFillKey

		#region ApplicationMenuButtonHoverInnerBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ApplicationMenuButtonHoverInnerBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ApplicationMenuButtonHoverInnerBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ApplicationMenuButtonHoverInnerBorderFillKey");

		#endregion ApplicationMenuButtonHoverInnerBorderFillKey

		#region ApplicationMenuButtonHoverCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ApplicationMenuButtonHoverCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ApplicationMenuButtonHoverCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ApplicationMenuButtonHoverCenterFillKey");

		#endregion ApplicationMenuButtonHoverCenterFillKey

		#region ApplicationMenuButtonHoverHighlightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ApplicationMenuButtonHoverHighlightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ApplicationMenuButtonHoverHighlightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ApplicationMenuButtonHoverHighlightFillKey");

		#endregion ApplicationMenuButtonHoverHighlightFillKey

		#region ApplicationMenuButtonPressedHighlightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ApplicationMenuButtonPressedHighlightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ApplicationMenuButtonPressedHighlightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ApplicationMenuButtonPressedHighlightFillKey");

		#endregion ApplicationMenuButtonPressedHighlightFillKey

		#region ApplicationMenuButtonPressedCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ApplicationMenuButtonPressedCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ApplicationMenuButtonPressedCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ApplicationMenuButtonPressedCenterFillKey");

		#endregion ApplicationMenuButtonPressedCenterFillKey

		#region ApplicationMenuButtonPressedInnerBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ApplicationMenuButtonPressedInnerBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ApplicationMenuButtonPressedInnerBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ApplicationMenuButtonPressedInnerBorderFillKey");

		#endregion ApplicationMenuButtonPressedInnerBorderFillKey

		#region ApplicationMenuButtonPressedOuterBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ApplicationMenuButtonPressedOuterBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ApplicationMenuButtonPressedOuterBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ApplicationMenuButtonPressedOuterBorderFillKey");

		#endregion ApplicationMenuButtonPressedOuterBorderFillKey

		#region XamRibbonCaptionButtonHoverBorderDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the XamRibbonCaptionButtonHoverBorderDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey XamRibbonCaptionButtonHoverBorderDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "XamRibbonCaptionButtonHoverBorderDarkFillKey");

		#endregion XamRibbonCaptionButtonHoverBorderDarkFillKey

		#region XamRibbonCaptionButtonHoverBorderLightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the XamRibbonCaptionButtonHoverBorderLightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey XamRibbonCaptionButtonHoverBorderLightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "XamRibbonCaptionButtonHoverBorderLightFillKey");

		#endregion XamRibbonCaptionButtonHoverBorderLightFillKey

		#region XamRibbonCaptionButtonHoverCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the XamRibbonCaptionButtonHoverCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey XamRibbonCaptionButtonHoverCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "XamRibbonCaptionButtonHoverCenterFillKey");

		#endregion XamRibbonCaptionButtonHoverCenterFillKey

		#region XamRibbonCaptionButtonHoverOverlayFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the XamRibbonCaptionButtonHoverOverlayFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey XamRibbonCaptionButtonHoverOverlayFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "XamRibbonCaptionButtonHoverOverlayFillKey");

		#endregion XamRibbonCaptionButtonHoverOverlayFillKey

		#region XamRibbonCaptionButtonPressedBorderDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the XamRibbonCaptionButtonPressedBorderDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey XamRibbonCaptionButtonPressedBorderDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "XamRibbonCaptionButtonPressedBorderDarkFillKey");

		#endregion XamRibbonCaptionButtonPressedBorderDarkFillKey

		#region XamRibbonCaptionButtonPressedBorderLightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the XamRibbonCaptionButtonPressedBorderLightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey XamRibbonCaptionButtonPressedBorderLightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "XamRibbonCaptionButtonPressedBorderLightFillKey");

		#endregion XamRibbonCaptionButtonPressedBorderLightFillKey

		#region XamRibbonCaptionButtonPressedCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the XamRibbonCaptionButtonPressedCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey XamRibbonCaptionButtonPressedCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "XamRibbonCaptionButtonPressedCenterFillKey");

		#endregion XamRibbonCaptionButtonPressedCenterFillKey

		#region XamRibbonCaptionButtonPressedBottomOverlayFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the XamRibbonCaptionButtonPressedBottomOverlayFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey XamRibbonCaptionButtonPressedBottomOverlayFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "XamRibbonCaptionButtonPressedBottomOverlayFillKey");

		#endregion XamRibbonCaptionButtonPressedBottomOverlayFillKey

		#region XamRibbonCaptionButtonPressedTopOverlayFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the XamRibbonCaptionButtonPressedTopOverlayFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey XamRibbonCaptionButtonPressedTopOverlayFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "XamRibbonCaptionButtonPressedTopOverlayFillKey");

		#endregion XamRibbonCaptionButtonPressedTopOverlayFillKey

		#region XamRibbonCaptionButtonIconMinimizeNormalForegroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the XamRibbonCaptionButtonIconMinimizeNormalForegroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey XamRibbonCaptionButtonIconMinimizeNormalForegroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "XamRibbonCaptionButtonIconMinimizeNormalForegroundFillKey");

		#endregion XamRibbonCaptionButtonIconMinimizeNormalForegroundFillKey

		#region XamRibbonCaptionButtonIconMinimizeNormalBackgroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the XamRibbonCaptionButtonIconMinimizeNormalBackgroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey XamRibbonCaptionButtonIconMinimizeNormalBackgroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "XamRibbonCaptionButtonIconMinimizeNormalBackgroundFillKey");

		#endregion XamRibbonCaptionButtonIconMinimizeNormalBackgroundFillKey

		#region XamRibbonCaptionButtonIconMaxCloseNormalBackgroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the XamRibbonCaptionButtonIconMaxCloseNormalBackgroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey XamRibbonCaptionButtonIconMaxCloseNormalBackgroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "XamRibbonCaptionButtonIconMaxCloseNormalBackgroundFillKey");

		#endregion XamRibbonCaptionButtonIconMaxCloseNormalBackgroundFillKey

		#region XamRibbonCaptionButtonIconMaxCloseNormalForegroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the XamRibbonCaptionButtonIconMaxCloseNormalForegroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey XamRibbonCaptionButtonIconMaxCloseNormalForegroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "XamRibbonCaptionButtonIconMaxCloseNormalForegroundFillKey");

		#endregion XamRibbonCaptionButtonIconMaxCloseNormalForegroundFillKey

		#region XamRibbonCaptionButtonIconMaxCloseNormalDisabledForegroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the XamRibbonCaptionButtonIconMaxCloseNormalDisabledForegroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey XamRibbonCaptionButtonIconMaxCloseNormalDisabledForegroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "XamRibbonCaptionButtonIconMaxCloseNormalDisabledForegroundFillKey");

		#endregion XamRibbonCaptionButtonIconMaxCloseNormalDisabledForegroundFillKey

		#region XamRibbonCaptionButtonIconMaxCloseInactiveDisabledForegroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the XamRibbonCaptionButtonIconMaxCloseInactiveDisabledForegroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey XamRibbonCaptionButtonIconMaxCloseInactiveDisabledForegroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "XamRibbonCaptionButtonIconMaxCloseInactiveDisabledForegroundFillKey");

		#endregion XamRibbonCaptionButtonIconMaxCloseInactiveDisabledForegroundFillKey

		#region XamRibbonCaptionButtonIconMinimizeHoverForegroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the XamRibbonCaptionButtonIconMinimizeHoverForegroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey XamRibbonCaptionButtonIconMinimizeHoverForegroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "XamRibbonCaptionButtonIconMinimizeHoverForegroundFillKey");

		#endregion XamRibbonCaptionButtonIconMinimizeHoverForegroundFillKey

		#region XamRibbonCaptionButtonIconMinimizeHoverBackgroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the XamRibbonCaptionButtonIconMinimizeHoverBackgroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey XamRibbonCaptionButtonIconMinimizeHoverBackgroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "XamRibbonCaptionButtonIconMinimizeHoverBackgroundFillKey");

		#endregion XamRibbonCaptionButtonIconMinimizeHoverBackgroundFillKey

		#region XamRibbonCaptionButtonIconMaxCloseHoverBackgroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the XamRibbonCaptionButtonIconMaxCloseHoverBackgroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey XamRibbonCaptionButtonIconMaxCloseHoverBackgroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "XamRibbonCaptionButtonIconMaxCloseHoverBackgroundFillKey");

		#endregion XamRibbonCaptionButtonIconMaxCloseHoverBackgroundFillKey

		#region XamRibbonCaptionButtonIconMaxCloseHoverForegroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the XamRibbonCaptionButtonIconMaxCloseHoverForegroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey XamRibbonCaptionButtonIconMaxCloseHoverForegroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "XamRibbonCaptionButtonIconMaxCloseHoverForegroundFillKey");

		#endregion XamRibbonCaptionButtonIconMaxCloseHoverForegroundFillKey

		#region XamRibbonCaptionButtonIconMinimizeInactiveForegroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the XamRibbonCaptionButtonIconMinimizeInactiveForegroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey XamRibbonCaptionButtonIconMinimizeInactiveForegroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "XamRibbonCaptionButtonIconMinimizeInactiveForegroundFillKey");

		#endregion XamRibbonCaptionButtonIconMinimizeInactiveForegroundFillKey

		#region XamRibbonCaptionButtonIconMaxCloseInactiveForegroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the XamRibbonCaptionButtonIconMaxCloseInactiveForegroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey XamRibbonCaptionButtonIconMaxCloseInactiveForegroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "XamRibbonCaptionButtonIconMaxCloseInactiveForegroundFillKey");

		#endregion XamRibbonCaptionButtonIconMaxCloseInactiveForegroundFillKey

		#region GalleryItemGroupCaptionFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryItemGroupCaptionFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryItemGroupCaptionFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryItemGroupCaptionFillKey");

		#endregion GalleryItemGroupCaptionFillKey

		#region GalleryItemGroupCaptionStrokeFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryItemGroupCaptionStrokeFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryItemGroupCaptionStrokeFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryItemGroupCaptionStrokeFillKey");

		#endregion GalleryItemGroupCaptionStrokeFillKey

		#region ApplicationMenuFooterToolbarFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ApplicationMenuFooterToolbarFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ApplicationMenuFooterToolbarFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ApplicationMenuFooterToolbarFillKey");

		#endregion ApplicationMenuFooterToolbarFillKey

		#region ApplicationMenuFooterToolbarOuterBorderDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ApplicationMenuFooterToolbarOuterBorderDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ApplicationMenuFooterToolbarOuterBorderDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ApplicationMenuFooterToolbarOuterBorderDarkFillKey");

		#endregion ApplicationMenuFooterToolbarOuterBorderDarkFillKey

		#region ApplicationMenuFooterToolbarOuterBorderLightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ApplicationMenuFooterToolbarOuterBorderLightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ApplicationMenuFooterToolbarOuterBorderLightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ApplicationMenuFooterToolbarOuterBorderLightFillKey");

		#endregion ApplicationMenuFooterToolbarOuterBorderLightFillKey

		#region GalleryToolDropDownPresenterScrollBarBorderDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolDropDownPresenterScrollBarBorderDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolDropDownPresenterScrollBarBorderDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolDropDownPresenterScrollBarBorderDarkFillKey");

		#endregion GalleryToolDropDownPresenterScrollBarBorderDarkFillKey

		#region GalleryToolDropDownPresenterScrollBarBorderLightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolDropDownPresenterScrollBarBorderLightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolDropDownPresenterScrollBarBorderLightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolDropDownPresenterScrollBarBorderLightFillKey");

		#endregion GalleryToolDropDownPresenterScrollBarBorderLightFillKey

		#region GalleryToolDropDownPresenterScrollBarCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolDropDownPresenterScrollBarCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolDropDownPresenterScrollBarCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolDropDownPresenterScrollBarCenterFillKey");

		#endregion GalleryToolDropDownPresenterScrollBarCenterFillKey

		#region StatusBarTextForegroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the StatusBarTextForegroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey StatusBarTextForegroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "StatusBarTextForegroundFillKey");

		#endregion StatusBarTextForegroundFillKey

		#region StatusBarFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the StatusBarFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey StatusBarFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "StatusBarFillKey");

		#endregion StatusBarFillKey

		#region StatusBarBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the StatusBarBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey StatusBarBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "StatusBarBorderFillKey");

		#endregion StatusBarBorderFillKey

		#region StatusBarSeparatorGradientLeftKey

		/// <summary>
		/// The key that identifies a resource to be used as the StatusBarSeparatorGradientLeft.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey StatusBarSeparatorGradientLeftKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "StatusBarSeparatorGradientLeftKey");

		#endregion StatusBarSeparatorGradientLeftKey

		#region StatusBarSeparatorGradientRightKey

		/// <summary>
		/// The key that identifies a resource to be used as the StatusBarSeparatorGradientRight.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey StatusBarSeparatorGradientRightKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "StatusBarSeparatorGradientRightKey");

		#endregion StatusBarSeparatorGradientRightKey

		#region TabActiveNormalMainCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the TabActiveNormalMainCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey TabActiveNormalMainCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "TabActiveNormalMainCenterFillKey");

		#endregion TabActiveNormalMainCenterFillKey

		#region XamPagerButtonGlyphFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the XamPagerButtonGlyphFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey XamPagerButtonGlyphFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "XamPagerButtonGlyphFillKey");

		#endregion XamPagerButtonGlyphFillKey

		#region KeyTipBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the TipBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey KeyTipBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "KeyTipBorderFillKey");

		#endregion KeyTipBorderFillKey

		#region KeyTipCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the TipCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey KeyTipCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "KeyTipCenterFillKey");

		#endregion KeyTipCenterFillKey

		#region LargeSegmentedTopHoverBottomNormalFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the LargeSegmentedTopHoverBottomNormalFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey LargeSegmentedTopHoverBottomNormalFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "LargeSegmentedTopHoverBottomNormalFillKey");

		#endregion LargeSegmentedTopHoverBottomNormalFillKey

		#region ContextualTabSeparatorFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ContextualTabSeparatorFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ContextualTabSeparatorFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ContextualTabSeparatorFillKey");

		#endregion ContextualTabSeparatorFillKey

		#region ContextualTabSideBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ContextualTabSideBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ContextualTabSideBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ContextualTabSideBorderFillKey");

		#endregion ContextualTabSideBorderFillKey

		#region DropdownEnabledBorderDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the DropdownEnabledBorderDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey DropdownEnabledBorderDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "DropdownEnabledBorderDarkFillKey");

		#endregion DropdownEnabledBorderDarkFillKey

		#region DropdownEnabledCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the DropdownEnabledCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey DropdownEnabledCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "DropdownEnabledCenterFillKey");

		#endregion DropdownEnabledCenterFillKey

		#region DropdownHoverBorderDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the DropdownHoverBorderDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey DropdownHoverBorderDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "DropdownHoverBorderDarkFillKey");

		#endregion DropdownHoverBorderDarkFillKey

		#region DropdownHoverBorderLightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the DropdownHoverBorderLightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey DropdownHoverBorderLightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "DropdownHoverBorderLightFillKey");

		#endregion DropdownHoverBorderLightFillKey

		#region DropdownHoverCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the DropdownHoverCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey DropdownHoverCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "DropdownHoverCenterFillKey");

		#endregion DropdownHoverCenterFillKey

		#region DropdownPressedBorderDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the DropdownPressedBorderDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey DropdownPressedBorderDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "DropdownPressedBorderDarkFillKey");

		#endregion DropdownPressedBorderDarkFillKey

		#region DropdownPressedBorderLightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the DropdownPressedBorderLightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey DropdownPressedBorderLightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "DropdownPressedBorderLightFillKey");

		#endregion DropdownPressedBorderLightFillKey

		#region DropdownPressedCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the DropdownPressedCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey DropdownPressedCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "DropdownPressedCenterFillKey");

		#endregion DropdownPressedCenterFillKey

		#region DropdownGlyphNormalForegroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the DropdownGlyphNormalForegroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey DropdownGlyphNormalForegroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "DropdownGlyphNormalForegroundFillKey");

		#endregion DropdownGlyphNormalForegroundFillKey

		#region DropdownGlyphNormalBackgroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the DropdownGlyphNormalBackgroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey DropdownGlyphNormalBackgroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "DropdownGlyphNormalBackgroundFillKey");

		#endregion DropdownGlyphNormalBackgroundFillKey

		#region DropdownGlyphHoverForegroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the DropdownGlyphHoverForegroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey DropdownGlyphHoverForegroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "DropdownGlyphHoverForegroundFillKey");

		#endregion DropdownGlyphHoverForegroundFillKey

		#region DropdownGlyphHoverBackgroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the DropdownGlyphHoverBackgroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey DropdownGlyphHoverBackgroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "DropdownGlyphHoverBackgroundFillKey");

		#endregion DropdownGlyphHoverBackgroundFillKey

		#region MenuScrollViewerNormalBorderDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the MenuScrollViewerNormalBorderDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey MenuScrollViewerNormalBorderDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "MenuScrollViewerNormalBorderDarkFillKey");

		#endregion MenuScrollViewerNormalBorderDarkFillKey

		#region MenuScrollViewerNormalCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the MenuScrollViewerNormalCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey MenuScrollViewerNormalCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "MenuScrollViewerNormalCenterFillKey");

		#endregion MenuScrollViewerNormalCenterFillKey

		#region MenuScrollViewerHoverBorderDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the MenuScrollViewerHoverBorderDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey MenuScrollViewerHoverBorderDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "MenuScrollViewerHoverBorderDarkFillKey");

		#endregion MenuScrollViewerHoverBorderDarkFillKey

		#region MenuScrollViewerHoverBorderLightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the MenuScrollViewerHoverBorderLightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey MenuScrollViewerHoverBorderLightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "MenuScrollViewerHoverBorderLightFillKey");

		#endregion MenuScrollViewerHoverBorderLightFillKey

		#region MenuScrollViewerHoverCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the MenuScrollViewerHoverCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey MenuScrollViewerHoverCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "MenuScrollViewerHoverCenterFillKey");

		#endregion MenuScrollViewerHoverCenterFillKey

		#region MenuScrollViewerPressedBorderDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the MenuScrollViewerPressedBorderDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey MenuScrollViewerPressedBorderDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "MenuScrollViewerPressedBorderDarkFillKey");

		#endregion MenuScrollViewerPressedBorderDarkFillKey

		#region MenuScrollViewerPressedBorderLightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the MenuScrollViewerPressedBorderLightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey MenuScrollViewerPressedBorderLightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "MenuScrollViewerPressedBorderLightFillKey");

		#endregion MenuScrollViewerPressedBorderLightFillKey

		#region MenuScrollViewerPressedCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the MenuScrollViewerPressedCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey MenuScrollViewerPressedCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "MenuScrollViewerPressedCenterFillKey");

		#endregion MenuScrollViewerPressedCenterFillKey

		#region MenuScrollViewerNormalGlyphFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the MenuScrollViewerNormalGlyphFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey MenuScrollViewerNormalGlyphFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "MenuScrollViewerNormalGlyphFillKey");

		#endregion MenuScrollViewerNormalGlyphFillKey

		#region MenuScrollViewerHoverGlyphFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the MenuScrollViewerHoverGlyphFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey MenuScrollViewerHoverGlyphFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "MenuScrollViewerHoverGlyphFillKey");

		#endregion MenuScrollViewerHoverGlyphFillKey

		#region TextHottrackFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the TextHottrackFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey TextHottrackFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "TextHottrackFillKey");

		#endregion TextHottrackFillKey

		#region CheckMarkStrokeFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the CheckMarkStrokeFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey CheckMarkStrokeFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "CheckMarkStrokeFillKey");

		#endregion CheckMarkStrokeFillKey

		#region RibbonTabControlHeaderPanelBackgroundKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonTabControlHeaderPanelBackground.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonTabControlHeaderPanelBackgroundKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonTabControlHeaderPanelBackgroundKey");

		#endregion RibbonTabControlHeaderPanelBackgroundKey

		#region GenericGlyphNormalForegroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GenericGlyphNormalForegroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GenericGlyphNormalForegroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GenericGlyphNormalForegroundFillKey");

		#endregion GenericGlyphNormalForegroundFillKey

		#region GenericGlyphNormalBackgroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GenericGlyphNormalBackgroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GenericGlyphNormalBackgroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GenericGlyphNormalBackgroundFillKey");

		#endregion GenericGlyphNormalBackgroundFillKey

		#region GenericGlyphHoverForegroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GenericGlyphHoverForegroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GenericGlyphHoverForegroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GenericGlyphHoverForegroundFillKey");

		#endregion GenericGlyphHoverForegroundFillKey

		#region GenericGlyphHoverBackgroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GenericGlyphHoverBackgroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GenericGlyphHoverBackgroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GenericGlyphHoverBackgroundFillKey");

		#endregion GenericGlyphHoverBackgroundFillKey

		#region GenericGlyphDisabledForegroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GenericGlyphDisabledForegroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GenericGlyphDisabledForegroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GenericGlyphDisabledForegroundFillKey");

		#endregion GenericGlyphDisabledForegroundFillKey

		#region GenericGlyphDisabledBackgroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GenericGlyphDisabledBackgroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GenericGlyphDisabledBackgroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GenericGlyphDisabledBackgroundFillKey");

		#endregion GenericGlyphDisabledBackgroundFillKey

		#region ContextualCaptionTextFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ContextualCaptionTextFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ContextualCaptionTextFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ContextualCaptionTextFillKey");

		#endregion ContextualCaptionTextFillKey

		#region ResizeGrippersForegroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ResizeGrippersForegroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ResizeGrippersForegroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ResizeGrippersForegroundFillKey");

		#endregion ResizeGrippersForegroundFillKey

		#region ResizeGrippersBackgroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ResizeGrippersBackgroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ResizeGrippersBackgroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ResizeGrippersBackgroundFillKey");

		#endregion ResizeGrippersBackgroundFillKey

		#region EditorsNormalBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the EditorsNormalBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey EditorsNormalBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "EditorsNormalBorderFillKey");

		#endregion EditorsNormalBorderFillKey

		#region EditorsNormalCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the EditorsNormalCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey EditorsNormalCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "EditorsNormalCenterFillKey");

		#endregion EditorsNormalCenterFillKey

		#region EditorsHoverBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the EditorsHoverBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey EditorsHoverBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "EditorsHoverBorderFillKey");

		#endregion EditorsHoverBorderFillKey

		#region EditorsHoverCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the EditorsHoverCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey EditorsHoverCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "EditorsHoverCenterFillKey");

		#endregion EditorsHoverCenterFillKey

		#region EditorsDisabledBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the EditorsDisabledBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey EditorsDisabledBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "EditorsDisabledBorderFillKey");

		#endregion EditorsDisabledBorderFillKey

		#region EditorsDisabledCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the EditorsDisabledCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey EditorsDisabledCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "EditorsDisabledCenterFillKey");

		#endregion EditorsDisabledCenterFillKey

		#region ApplicationMenuFooterToolbarButtonTextForegroundKey

		/// <summary>
		/// The key that identifies a resource to be used as the ApplicationMenuFooterToolbarButtonTextForeground.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ApplicationMenuFooterToolbarButtonTextForegroundKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ApplicationMenuFooterToolbarButtonTextForegroundKey");

		#endregion ApplicationMenuFooterToolbarButtonTextForegroundKey

		#region QATCustomizeDropdownNormalForegroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the QATCustomizeDropdownNormalForegroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey QATCustomizeDropdownNormalForegroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "QATCustomizeDropdownNormalForegroundFillKey");

		#endregion QATCustomizeDropdownNormalForegroundFillKey

		#region QATCustomizeDropdownNormalBackgroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the QATCustomizeDropdownNormalBackgroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey QATCustomizeDropdownNormalBackgroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "QATCustomizeDropdownNormalBackgroundFillKey");

		#endregion QATCustomizeDropdownNormalBackgroundFillKey

		#region QATBelowRibbonBorderDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the QATBelowRibbonBorderDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey QATBelowRibbonBorderDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "QATBelowRibbonBorderDarkFillKey");

		#endregion QATBelowRibbonBorderDarkFillKey

		#region QATBelowRibbonBorderLightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the QATBelowRibbonBorderLightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey QATBelowRibbonBorderLightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "QATBelowRibbonBorderLightFillKey");

		#endregion QATBelowRibbonBorderLightFillKey

		#region QATBelowRibbonCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the QATBelowRibbonCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey QATBelowRibbonCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "QATBelowRibbonCenterFillKey");

		#endregion QATBelowRibbonCenterFillKey

		#region QATBelowRibbonShadowFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the QATBelowRibbonShadowFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey QATBelowRibbonShadowFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "QATBelowRibbonShadowFillKey");

		#endregion QATBelowRibbonShadowFillKey

		#region CaptionActiveTextForegroundKey

		/// <summary>
		/// The key that identifies a resource to be used as the CaptionActiveTextForeground.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey CaptionActiveTextForegroundKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "CaptionActiveTextForegroundKey");

		#endregion CaptionActiveTextForegroundKey

		#region CaptionInactiveTextForegroundKey

		/// <summary>
		/// The key that identifies a resource to be used as the CaptionInactiveTextForeground.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey CaptionInactiveTextForegroundKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "CaptionInactiveTextForegroundKey");

		#endregion CaptionInactiveTextForegroundKey

		#region QATOverflowGlyphNormalForegroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the QATOverflowGlyphNormalForegroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey QATOverflowGlyphNormalForegroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "QATOverflowGlyphNormalForegroundFillKey");

		#endregion QATOverflowGlyphNormalForegroundFillKey

		#region QATOverflowGlyphNormalBackgroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the QATOverflowGlyphNormalBackgroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey QATOverflowGlyphNormalBackgroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "QATOverflowGlyphNormalBackgroundFillKey");

		#endregion QATOverflowGlyphNormalBackgroundFillKey

		#region QATOverflowGlyphHoverForegroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the QATOverflowGlyphHoverForegroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey QATOverflowGlyphHoverForegroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "QATOverflowGlyphHoverForegroundFillKey");

		#endregion QATOverflowGlyphHoverForegroundFillKey

		#region QATOverflowGlyphHoverBackgroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the QATOverflowGlyphHoverBackgroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey QATOverflowGlyphHoverBackgroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "QATOverflowGlyphHoverBackgroundFillKey");

		#endregion QATOverflowGlyphHoverBackgroundFillKey

		#region QATOverflowPanelBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the QATOverflowPanelBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey QATOverflowPanelBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "QATOverflowPanelBorderFillKey");

		#endregion QATOverflowPanelBorderFillKey

		#region QATOverflowPanelCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the QATOverflowPanelCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey QATOverflowPanelCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "QATOverflowPanelCenterFillKey");

		#endregion QATOverflowPanelCenterFillKey

		#region MenuToolPresenterLeftColumnFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the MenuToolPresenterLeftColumnFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey MenuToolPresenterLeftColumnFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "MenuToolPresenterLeftColumnFillKey");

		#endregion MenuToolPresenterLeftColumnFillKey

		#region MenuToolOverlayFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the MenuToolOverlayFillKey.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey MenuToolOverlayFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "MenuToolOverlayFillKey");

		#endregion MenuToolOverlayFillKey

		#region CaptionPanelBottomSeparatorStrokeKey

		/// <summary>
		/// The key that identifies a resource to be used as the CaptionPanelBottomSeparatorStroke.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey CaptionPanelBottomSeparatorStrokeKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "CaptionPanelBottomSeparatorStrokeKey");

		#endregion CaptionPanelBottomSeparatorStrokeKey

		#region SubMenuHeaderPopUpBorderDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the SubMenuHeaderPopUpBorderDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey SubMenuHeaderPopUpBorderDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "SubMenuHeaderPopUpBorderDarkFillKey");

		#endregion SubMenuHeaderPopUpBorderDarkFillKey

		#region SubMenuHeaderPopUpBorderLightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the SubMenuHeaderPopUpBorderLightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey SubMenuHeaderPopUpBorderLightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "SubMenuHeaderPopUpBorderLightFillKey");

		#endregion SubMenuHeaderPopUpBorderLightFillKey

		#region SubMenuHeaderPopUpCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the SubMenuHeaderPopUpCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey SubMenuHeaderPopUpCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "SubMenuHeaderPopUpCenterFillKey");

		#endregion SubMenuHeaderPopUpCenterFillKey

		#region ToolEnabledForegroundTextFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ToolEnabledForegroundTextFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ToolEnabledForegroundTextFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ToolEnabledForegroundTextFillKey");

		#endregion ToolEnabledForegroundTextFillKey

		#region ToolDisabledForegroundTextFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ToolDisabledForegroundTextFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ToolDisabledForegroundTextFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ToolDisabledForegroundTextFillKey");

		#endregion ToolDisabledForegroundTextFillKey

		#region ToolInQATForegroundTextFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ToolInQATForegroundTextFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ToolInQATForegroundTextFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ToolInQATForegroundTextFillKey");

		#endregion ToolInQATForegroundTextFillKey

		#region VistaWindowMaximizedTextFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the VistaWindowMaximizedTextFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey VistaWindowMaximizedTextFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "VistaWindowMaximizedTextFillKey");

		#endregion VistaWindowMaximizedTextFillKey

		#region TextNormalForegroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the TextNormalForegroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey TextNormalForegroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "TextNormalForegroundFillKey");

		#endregion TextNormalForegroundFillKey

		#region TextDisabledForegroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the TextDisabledForegroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey TextDisabledForegroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "TextDisabledForegroundFillKey");

		#endregion TextDisabledForegroundFillKey

		#region RibbonGroupCaptionTextFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the RibbonGroupCaptionTextFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey RibbonGroupCaptionTextFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "RibbonGroupCaptionTextFillKey");

		#endregion RibbonGroupCaptionTextFillKey

		#region HorizontalSeparatorNormalLightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the HorizontalSeparatorNormalLightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey HorizontalSeparatorNormalLightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "HorizontalSeparatorNormalLightFillKey");

		#endregion HorizontalSeparatorNormalLightFillKey

		#region HorizontalSeparatorNormalDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the HorizontalSeparatorNormalDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey HorizontalSeparatorNormalDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "HorizontalSeparatorNormalDarkFillKey");

		#endregion HorizontalSeparatorNormalDarkFillKey

		#region VerticalSeparatorNormalLightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the VerticalSeparatorNormalLightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey VerticalSeparatorNormalLightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "VerticalSeparatorNormalLightFillKey");

		#endregion VerticalSeparatorNormalLightFillKey

		#region VerticalSeparatorNormalDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the VerticalSeparatorNormalDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey VerticalSeparatorNormalDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "VerticalSeparatorNormalDarkFillKey");

		#endregion VerticalSeparatorNormalDarkFillKey

		#region VerticalSeparatorHoverLightFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the VerticalSeparatorHoverLightFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey VerticalSeparatorHoverLightFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "VerticalSeparatorHoverLightFillKey");

		#endregion VerticalSeparatorHoverLightFillKey

		#region VerticalSeparatorHoverDarkFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the VerticalSeparatorHoverDarkFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey VerticalSeparatorHoverDarkFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "VerticalSeparatorHoverDarkFillKey");

		#endregion VerticalSeparatorHoverDarkFillKey

		#region IsCheckedBoxBorderFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the IsCheckedBoxBorderFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey IsCheckedBoxBorderFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "IsCheckedBoxBorderFillKey");

		#endregion IsCheckedBoxBorderFillKey

		#region IsCheckedBoxCenterFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the IsCheckedBoxCenterFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey IsCheckedBoxCenterFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "IsCheckedBoxCenterFillKey");

		#endregion IsCheckedBoxCenterFillKey

		#region CheckmarkBackgroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the CheckmarkBackgroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey CheckmarkBackgroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "CheckmarkBackgroundFillKey");

		#endregion CheckmarkBackgroundFillKey

		#region CheckmarkForegroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the CheckmarkForegroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey CheckmarkForegroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "CheckmarkForegroundFillKey");

		#endregion CheckmarkForegroundFillKey

		#region GalleryToolPreviewPresenterDisabledArrowBackgroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolPreviewPresenterDisabledArrowBackgroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolPreviewPresenterDisabledArrowBackgroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolPreviewPresenterDisabledArrowBackgroundFillKey");

		#endregion GalleryToolPreviewPresenterDisabledArrowBackgroundFillKey

		#region GalleryToolPreviewPresenterDisabledArrowForegroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolPreviewPresenterDisabledArrowForegroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolPreviewPresenterDisabledArrowForegroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolPreviewPresenterDisabledArrowForegroundFillKey");

		#endregion GalleryToolPreviewPresenterDisabledArrowForegroundFillKey

		#region GalleryToolPreviewPresenterEnabledArrowBackgroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolPreviewPresenterEnabledArrowBackgroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolPreviewPresenterEnabledArrowBackgroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolPreviewPresenterEnabledArrowBackgroundFillKey");

		#endregion GalleryToolPreviewPresenterEnabledArrowBackgroundFillKey

		#region GalleryToolPreviewPresenterEnabledArrowForegroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolPreviewPresenterEnabledArrowForegroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolPreviewPresenterEnabledArrowForegroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolPreviewPresenterEnabledArrowForegroundFillKey");

		#endregion GalleryToolPreviewPresenterEnabledArrowForegroundFillKey

		#region GalleryToolPreviewPresenterDropDownButtonIconBackgroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolPreviewPresenterDropDownButtonIconBackgroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolPreviewPresenterDropDownButtonIconBackgroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolPreviewPresenterDropDownButtonIconBackgroundFillKey");

		#endregion GalleryToolPreviewPresenterDropDownButtonIconBackgroundFillKey

		#region GalleryToolPreviewPresenterDropDownButtonIconForegroundFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the GalleryToolPreviewPresenterDropDownButtonIconForegroundFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GalleryToolPreviewPresenterDropDownButtonIconForegroundFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GalleryToolPreviewPresenterDropDownButtonIconForegroundFillKey");

		#endregion GalleryToolPreviewPresenterDropDownButtonIconForegroundFillKey

		#region VistaQATTextFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the VistaQATTextFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey VistaQATTextFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "VistaQATTextFillKey");

		#endregion VistaQATTextFillKey

		#region GrayTextBrushKey

		/// <summary>
		/// The key that identifies a resource to be used as the GrayTextBrushKey.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey GrayTextBrushKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "GrayTextBrushKey");

		#endregion GrayTextBrushKey

		#region IsSegmentedNonHottrackDividerFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the IsSegmentedNonHottrackDividerFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey IsSegmentedNonHottrackDividerFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "IsSegmentedNonHottrackDividerFillKey");

		#endregion IsSegmentedNonHottrackDividerFillKey

		#region LargeSegmentedDisabledDarkSeparatorFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the LargeSegmentedDisabledDarkSeparatorFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey LargeSegmentedDisabledDarkSeparatorFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "LargeSegmentedDisabledDarkSeparatorFillKey");

		#endregion LargeSegmentedDisabledDarkSeparatorFillKey

		#region LargeSegmentedDisabledLightSeparatorFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the LargeSegmentedDisabledLightSeparatorFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey LargeSegmentedDisabledLightSeparatorFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "LargeSegmentedDisabledLightSeparatorFillKey");

		#endregion LargeSegmentedDisabledLightSeparatorFillKey

		#region LargeSegmentedHoverSeparatorFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the LargeSegmentedHoverSeparatorFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey LargeSegmentedHoverSeparatorFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "LargeSegmentedHoverSeparatorFillKey");

		#endregion LargeSegmentedHoverSeparatorFillKey

		#region LargeSegmentedCheckedSeparatorFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the LargeSegmentedCheckedSeparatorFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey LargeSegmentedCheckedSeparatorFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "LargeSegmentedCheckedSeparatorFillKey");

		#endregion LargeSegmentedCheckedSeparatorFillKey

		#region LargeSegmentedCheckedHottrackSeparatorFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the LargeSegmentedCheckedHottrackSeparatorFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey LargeSegmentedCheckedHottrackSeparatorFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "LargeSegmentedCheckedHottrackSeparatorFillKey");

		#endregion LargeSegmentedCheckedHottrackSeparatorFillKey

		#region MenuItemDropDownArrowFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the MenuItemDropDownArrowFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey MenuItemDropDownArrowFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "MenuItemDropDownArrowFillKey");

		#endregion MenuItemDropDownArrowFillKey

		#region MenuItemDropDownArrowHottrackFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the MenuItemDropDownArrowHottrackFill.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey MenuItemDropDownArrowHottrackFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "MenuItemDropDownArrowHottrackFillKey");

		#endregion MenuItemDropDownArrowHottrackFillKey


		#region ScenicRibbonCaptionGlassEffectFillKey

		/// <summary>
		/// The key that identifies a resource to be used as the ScenicRibbonCaptionGlassEffectFillKey.  Look here <see cref="RibbonBrushKeys"/> for 
		/// an explanation of how these keys are used. 
		/// </summary>
		public static readonly ResourceKey ScenicRibbonCaptionGlassEffectFillKey = new StaticPropertyResourceKey(typeof(RibbonBrushKeys), "ScenicRibbonCaptionGlassEffectFillKey");

		#endregion ScenicRibbonCaptionGlassEffectFillKey


		#endregion Generated Keys
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