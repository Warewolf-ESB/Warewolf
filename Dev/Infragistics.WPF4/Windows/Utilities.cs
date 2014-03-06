using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Data;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Diagnostics;
using System.Collections;
using System.Text;
using Infragistics.Windows.Helpers;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using System.Runtime.CompilerServices;
using System.Security;
using System.Windows.Input;
using Infragistics.Windows.Controls;
using System.Text.RegularExpressions;
using System.Windows.Interop;
using System.Threading;
using System.Windows.Markup;
using System.Windows.Threading;
using Infragistics.Shared;
using Infragistics.Collections;
using Infragistics.Windows.Licensing;

namespace Infragistics.Windows
{
	/// <summary>
	/// Exposes some statichelper methods
	/// </summary>
	public class Utilities : CoreUtilities
	{
		#region Member Variables

		// AS 3/1/10 TFS28705
		[ThreadStatic]
		private static Random _rnd;

		// JJD 4/10/08
		// Added map to cache cultures for each language
		[ThreadStatic()]
		private static Dictionary<XmlLanguage, CultureInfo> g_LanguageCultureMap;

		// AS 1/20/11 Optimization
		private static readonly Type UIElement3DType;

		// JM 03-24-11 TFS68833 
		[ThreadStatic()]
		private static bool _modelessKeyboardInteropEnabled;

		#endregion //Member Variables

		#region Constants

		private const double SMALLEST_DOUBLE = 2.2204460492503131E-13;
		private const string publicKeyTokenLiteral = "PublicKeyToken";

		#endregion //Constants	

		#region Constructor
		static Utilities()
		{
			// AS 1/20/11 Optimization
			UIElement3DType = Type.GetType("System.Windows.UIElement3D, " + typeof(UIElement).Assembly.FullName, false);
		}

		/// <summary>
		/// For internal use
		/// </summary>
		protected Utilities()
		{
		} 
		#endregion // Constructor
    
		#region Properties

		// AS 3/1/10 TFS28705
		#region Rnd
		private static Random Rnd
		{
			get
			{
				if (null == _rnd)
					_rnd = new Random();

				return _rnd;
			}
		}
		#endregion //Rnd

		#endregion //Properties

        #region AllowsAsyncOperations
        /// <summary>
        /// Returns a boolean indicating if the object may perform asynchronous operations.
        /// </summary>
        /// <param name="d">The object to evaluate</param>
        /// <returns>Return true unless the object is within a report.</returns>
        public static bool AllowsAsyncOperations(DependencyObject d)
        {



            return Infragistics.Windows.Reporting.ReportSection.GetIsInReport(d) == false;

        } 
        #endregion //AllowsAsyncOperations

        #region AnimateTranslateTransform

		/// <summary>
		/// Animates a translate transform
		/// </summary>
        public static void AnimateTranslateTransform(TranslateTransform translateTransform, double fromX, double toX, double fromY, double toY, int durationMilliseconds, bool autoReverse, EventHandler callbackHandler)
        {
            DoubleAnimation translateX = new DoubleAnimation(fromX, toX, TimeSpan.FromMilliseconds(durationMilliseconds), FillBehavior.HoldEnd);
            translateX.BeginTime = new Nullable<TimeSpan>(); // Clock.CurrentGlobalTime;
            translateX.AutoReverse = autoReverse;
            translateX.AccelerationRatio = .5;
            translateX.DecelerationRatio = .5;

            DoubleAnimation translateY = new DoubleAnimation(fromY, toY, TimeSpan.FromMilliseconds(durationMilliseconds), FillBehavior.HoldEnd);
            translateY.BeginTime = new Nullable<TimeSpan>(); // Clock.CurrentGlobalTime;
            translateY.AutoReverse = autoReverse;
            translateY.AccelerationRatio = .5;
            translateY.DecelerationRatio = .5;


            AnimationClock translateXClock = translateX.CreateClock();
            AnimationClock translateYClock = translateY.CreateClock();
            translateTransform.ApplyAnimationClock(TranslateTransform.XProperty, translateXClock);
            translateTransform.ApplyAnimationClock(TranslateTransform.YProperty, translateYClock);

            //translateTransform.PersistentAnimations[TranslateTransform.XProperty] = translateX;
            //translateTransform.PersistentAnimations[TranslateTransform.YProperty] = translateY;
            //Clock translateXClock = translateTransform.PersistentAnimations.GetClock(TranslateTransform.XProperty);
            //Clock translateYClock = translateTransform.PersistentAnimations.GetClock(TranslateTransform.YProperty);

            if (callbackHandler != null)
                translateXClock.CurrentStateInvalidated += callbackHandler;

            translateXClock.Controller.Begin();
            translateYClock.Controller.Begin();
        }

        #endregion //AnimateTranslateTransform

		#region CanExecuteCommand
		/// <summary>
		/// Returns true if the command of the specified <see cref="ICommandSource"/> can be executed.
		/// </summary>
		/// <param name="commandSource">The command source to evaluate.</param>
		/// <returns>Returns true if the command can be executed.</returns>
		/// <exception cref="ArgumentNullException">The <paramref name="commandSource"/> was null.</exception>
		public static bool CanExecuteCommand(ICommandSource commandSource)
		{
			if (commandSource == null)
				throw new ArgumentNullException("commandSource");

			bool canExecute = false;
			ICommand command = commandSource.Command;

			if (command != null)
			{
				RoutedCommand routedCmd = command as RoutedCommand;
				object parameter = commandSource.CommandParameter;

				if (routedCmd != null)
				{
					IInputElement target = commandSource.CommandTarget ?? commandSource as IInputElement;

					canExecute = routedCmd.CanExecute(parameter, target);
				}
				else
					canExecute = command.CanExecute(parameter);
			}

			return canExecute;
		} 
		#endregion //CanExecuteCommand

		#region CalculateTotalMarginAndPadding

		/// <summary>
		/// Calculates the total padding and margin values beetween a descendant element and its ancestor
		/// </summary>
		/// <param name="ancestor"></param>
		/// <param name="descendant"></param>
		/// <returns>Returns the aggregate of all margin and padding settings</returns>
		public static Thickness CalculateTotalMarginAndPadding(DependencyObject ancestor, DependencyObject descendant)
		{
			if (ancestor == null)
				throw new ArgumentNullException("ancestor");

			if (descendant == null)
				throw new ArgumentNullException("descendant");

			Thickness margin = new Thickness();
			Visual visual = ancestor as Visual;

			if ( null != visual &&
				!visual.IsAncestorOf(descendant))
				return margin;

			DependencyObject current = descendant;

			while (current != null)
			{
				DependencyProperty marginProperty = null;
				DependencyProperty paddingProperty = null;

				if (current is AnchoredBlock)
				{
					marginProperty = AnchoredBlock.MarginProperty;
					paddingProperty = AnchoredBlock.PaddingProperty;
				}
				else if (current is Block)
				{
					marginProperty = Block.MarginProperty;
					paddingProperty = Block.PaddingProperty;
				}
				else if (current is FrameworkElement)
					marginProperty = FrameworkElement.MarginProperty;
				else if (current is ListItem)
				{
					marginProperty = ListItem.MarginProperty;
					paddingProperty = ListItem.PaddingProperty;
				}
				else if (current is Border)
					paddingProperty = Border.PaddingProperty;
				else if (current is Control)
					paddingProperty = Control.PaddingProperty;
				else if (current is TableCell)
					paddingProperty = TableCell.PaddingProperty;
				else if (current is TextBlock)
					paddingProperty = TextBlock.PaddingProperty;

				if (marginProperty != null)
				{
					Thickness feMargin = (Thickness)(current.GetValue(marginProperty));
					margin.Top += feMargin.Top;
					margin.Left += feMargin.Left;
					margin.Bottom += feMargin.Bottom;
					margin.Right += feMargin.Right;
				}

				if (paddingProperty != null)
				{
					Thickness fePadding = (Thickness)(current.GetValue(paddingProperty));
					margin.Top += fePadding.Top;
					margin.Left += fePadding.Left;
					margin.Bottom += fePadding.Bottom;
					margin.Right += fePadding.Right;
				}

				// if we have reached the ancestor then break out
				if (current == ancestor)
					break;

				// walk up the parent chain
				// JJD 8/23/07
				// Call the safer GetParent method that will tolerate FrameworkContentElements
				//current = VisualTreeHelper.GetParent(current);
				current = Utilities.GetParent(current);
			}

			return margin;
		}

		#endregion //CalculateTotalMarginAndPadding	

        #region ConvertFromLogicalPixels

        /// <summary>
        /// Converts the logical pixel representation into screen pixels.
        /// </summary>
        /// <param name="logicalPixelValue">The logical pixel value to convert.</param>
        /// <returns>The screen pixel representation of the specified logical pixels.</returns>
        [MethodImpl(MethodImplOptions.NoInlining)] // AS 3/21/08 Preventing inlining since this could cause drawing/winforms dlls to be loaded.
		public static int ConvertFromLogicalPixels(double logicalPixelValue)
        {
			// AS 3/24/10 TFS27164
			//int pixelScreenWidth = System.Windows.Forms.SystemInformation.PrimaryMonitorSize.Width;
			//
			//double logicalPixelScreenWidth = SystemParameters.PrimaryScreenWidth;
			//
			//return (int)((logicalPixelValue * (double)pixelScreenWidth) / logicalPixelScreenWidth);
			return ConvertFromLogicalPixels(logicalPixelValue, null);
        }

        /// <summary>
        /// Converts the logical pixel representation into screen pixels.
        /// </summary>
        /// <param name="logicalPixelValue">The logical pixel value to convert.</param>
		/// <param name="relativeVisual">An element that is used to obtain the required DPI information or null to use the default dpi information.</param>
		/// <returns>The screen pixel representation of the specified logical pixels.</returns>
		public static int ConvertFromLogicalPixels(double logicalPixelValue, Visual relativeVisual)
		{
			Matrix matrix = GetDeviceMatrix(true, relativeVisual);
			return (int)(matrix.M22 * logicalPixelValue);
		}

		// AS 8/4/11 TFS83465/TFS83469
		internal static Rect ConvertFromLogicalPixels(Rect deviceRect, Visual relativeElement)
		{
			return new Rect(
				Utilities.ConvertFromLogicalPixels(deviceRect.X, relativeElement),
				Utilities.ConvertFromLogicalPixels(deviceRect.Y, relativeElement),
				Utilities.ConvertFromLogicalPixels(deviceRect.Width, relativeElement),
				Utilities.ConvertFromLogicalPixels(deviceRect.Height, relativeElement)
				);
		}
		#endregion //ConvertFromLogicalPixels

        #region ConvertToLogicalPixels

        /// <summary>
        /// Converts the specified pixel value into the logical pixel representation.
        /// </summary>
        /// <param name="pixelValue">The pixel value to convert.</param>
        /// <returns>The logical pixel representation of the specified screen pixels.</returns>
		[MethodImpl(MethodImplOptions.NoInlining)] // AS 3/21/08 Preventing inlining since this could cause drawing/winforms dlls to be loaded.
		public static double ConvertToLogicalPixels(int pixelValue)
        {
			// AS 3/24/10 TFS27164
			//int pixelScreenWidth = System.Windows.Forms.SystemInformation.PrimaryMonitorSize.Width;
			//
			//double logicalPixelScreenWidth = SystemParameters.PrimaryScreenWidth;
			//
			//return ((double)pixelValue * logicalPixelScreenWidth) / (double)pixelScreenWidth;
			return ConvertToLogicalPixels(pixelValue, null);
        }

		/// <summary>
		/// Converts the specified pixel value into the logical pixel representation.
		/// </summary>
		/// <param name="pixelValue">The pixel value to convert.</param>
		/// <param name="relativeVisual">An element that is used to obtain the required DPI information or null to use the default dpi information.</param>
		/// <returns>The logical pixel representation of the specified screen pixels.</returns>
		public static double ConvertToLogicalPixels(int pixelValue, Visual relativeVisual)
		{
			Matrix matrix = GetDeviceMatrix(false, relativeVisual);
			return matrix.M11 * pixelValue;
		}

        // AS 7/9/08 BR34723
        internal static Point ConvertToLogicalPixels(int pixelX, int pixelY, Visual relativeElement)
        {
			// AS 3/24/10 TFS27164
			//if (null != relativeElement && false == BrowserInteropHelper.IsBrowserHosted)
			//{
			//    HwndSource source = HwndSource.FromVisual(relativeElement) as HwndSource;
			//
			//    if (null != source)
			//        return source.CompositionTarget.TransformFromDevice.Transform(new Point(pixelX, pixelY));
			//}
			//
			//return new Point(ConvertToLogicalPixels(pixelX), ConvertToLogicalPixels(pixelY));
			Matrix matrix = GetDeviceMatrix(false, relativeElement);
			return matrix.Transform(new Point(pixelX, pixelY));
        }

		// AS 8/4/11 TFS83465/TFS83469
		internal static Rect ConvertToLogicalPixels(Rect deviceRect, Visual relativeElement)
		{
			return new Rect(
				Utilities.ConvertToLogicalPixels((int)deviceRect.Left, (int)deviceRect.Top, relativeElement),
				Utilities.ConvertToLogicalPixels((int)deviceRect.Right, (int)deviceRect.Bottom, relativeElement)
				);
		}
        #endregion //ConvertToLogicalPixels	
    
		#region CopyPropertyValues

		/// <summary>
		/// Copies all of the property settings from one object to another
		/// </summary>
		/// <param name="target">The FrameworkElementFactory to receive the values.</param>
		/// <param name="source">The source of the property values.</param>
		public static void CopyPropertyValues(FrameworkElementFactory target, DependencyObject source)
		{
			if (target == null)
				throw new ArgumentNullException("target");

			if (source == null)
				throw new ArgumentNullException("source");

			LocalValueEnumerator enumerator = source.GetLocalValueEnumerator();

			while (enumerator.MoveNext())
			{
				if (enumerator.Current.Value is BindingExpressionBase)
					target.SetBinding(enumerator.Current.Property, ((BindingExpressionBase)(enumerator.Current.Value)).ParentBindingBase);
				else if (enumerator.Current.Value is BindingBase)
					target.SetBinding(enumerator.Current.Property, (BindingBase)(enumerator.Current.Value));
				else
					target.SetValue(enumerator.Current.Property, enumerator.Current.Value);
			}
		}

		/// <summary>
		/// Copies all of the property settings from one object to another
		/// </summary>
		/// <param name="target">The DependencyObject to receive the values.</param>
		/// <param name="source">The source of the property values.</param>
		public static void CopyPropertyValues(DependencyObject target, DependencyObject source)
		{
			if (target == null)
				throw new ArgumentNullException("target");

			if (source == null)
				throw new ArgumentNullException("source");

			LocalValueEnumerator enumerator = source.GetLocalValueEnumerator();

			while (enumerator.MoveNext())
			{
				if (enumerator.Current.Value is BindingExpressionBase)
					BindingOperations.SetBinding(target, enumerator.Current.Property, ((BindingExpressionBase)(enumerator.Current.Value)).ParentBindingBase);
				else
				if (  enumerator.Current.Value is BindingBase )
				{
					FrameworkElement fe = target as FrameworkElement;

					if ( fe != null )
					{
						fe.SetBinding(enumerator.Current.Property, (BindingBase)(enumerator.Current.Value));
						continue;
					}

					FrameworkContentElement fce = target as FrameworkContentElement;

					if ( fce != null )
					{
						fce.SetBinding(enumerator.Current.Property, (BindingBase)(enumerator.Current.Value));
						continue;
					}

					Debug.Fail("Object can't be the target of a binding. Type: " + target.GetType().ToString());
				}
				else
					target.SetValue(enumerator.Current.Property, enumerator.Current.Value);
			}
		}

		#endregion //CopyPropertyValues	
    
		#region Create2by2Grid

		/// <summary>
		/// Create grid with 2 columns and 2 rows
		/// </summary>
		public static FrameworkElementFactory Create2by2Grid()
		{
			FrameworkElementFactory fefGrid = new FrameworkElementFactory(typeof(Grid));

			fefGrid.SetValue(Grid.ShowGridLinesProperty, KnownBoxes.FalseBox);

			#region Column definitions

			int i;
			FrameworkElementFactory fefColumn;

			for (i = 0; i < 2; i++)
			{
				fefColumn = new FrameworkElementFactory(typeof(ColumnDefinition));
				fefColumn.SetValue(ColumnDefinition.WidthProperty, i == 0 ? new GridLength(1.0, GridUnitType.Star): new GridLength(0, GridUnitType.Auto));
//				fefColumn.SetValue(ColumnDefinition.WidthProperty, new GridLength( ? GridUnitType.Star : GridUnitType.Auto));
				fefGrid.AppendChild(fefColumn);
			}

			#endregion Column definitions

			#region Row definitions

			FrameworkElementFactory fefRow;

			for (i = 0; i < 2; i++)
			{
				fefRow = new FrameworkElementFactory(typeof(RowDefinition));
                fefRow.SetValue(RowDefinition.HeightProperty, i == 0 ? new GridLength(1.0, GridUnitType.Star): new GridLength(0, GridUnitType.Auto));
//                fefRow.SetValue(RowDefinition.HeightProperty, new GridLength(0, i == 0 ? GridUnitType.Star : GridUnitType.Auto));
				fefGrid.AppendChild(fefRow);
			}

			#endregion Row definitions

			return fefGrid;
		}

		#endregion Create2by2Grid

		#region CreateRoundedRectGeometry (Rect, Corners, double, double, double)
		/// <summary>
		/// Helper routine to create a geometry that describes a rounded rectangle.
		/// </summary>
		/// <param name="rect">The rectangle that defines the size and position of the rounded rectangle</param>
		/// <param name="roundedCorners">An enumeration indicating which corners will be rounded</param>
		/// <param name="radiusX">A double that indicates the horizontal radius of the corners</param>
		/// <param name="radiusY">A double that indicates the vertical radius of the corners</param>
		/// <param name="edgeThickness">The thickness of the edge for the rectangle. If the rectangle will have a border drawn, this would typically be the thickness of the pen.</param>
		/// <returns>A geometry that represents a rectangle with the specified rounded corners</returns>
		public static Geometry CreateRoundedRectGeometry(Rect rect, RoundedRectCorners roundedCorners, double radiusX, double radiusY, double edgeThickness)
		{
			return CreateRoundedRectGeometry(rect, roundedCorners, radiusX, radiusY, edgeThickness, RoundedRectSide.Left, RoundedRectSide.Left);
		}
		#endregion //CreateRoundedRectGeometry (Rect, Corners, double, double, double)

		#region CreateRoundedRectGeometry (Rect, Corners, double, double, double, Side, Side)
		/// <summary>
		/// Helper routine to create a geometry that describes a rounded rectangle. This overloads is used to return an open geometry where one or more sides are not included.
		/// </summary>
		/// <param name="rect">The rectangle that defines the size and position of the rounded rectangle</param>
		/// <param name="roundedCorners">An enumeration indicating which corners will be rounded</param>
		/// <param name="radiusX">A double that indicates the horizontal radius of the corners</param>
		/// <param name="radiusY">A double that indicates the vertical radius of the corners</param>
		/// <param name="edgeThickness">The thickness of the edge for the rectangle. If the rectangle will have a border drawn, this would typically be the thickness of the pen.</param>
		/// <param name="startSide">The first side of the rectangle to include</param>
		/// <param name="endSide">The last side of the rectangle to include. To close the rect, this value must be the same as the <paramref name="startSide"/>.</param>
		/// <returns>A geometry that represents a rectangle with the specified rounded corners</returns>
		public static Geometry CreateRoundedRectGeometry(Rect rect, RoundedRectCorners roundedCorners, double radiusX, double radiusY,
			double edgeThickness, RoundedRectSide startSide, RoundedRectSide endSide)
		{
			double halfEdgeThickness = edgeThickness / 2d;

			#region Adjust Rect
			if (startSide == endSide)
			{
				// create the geometry along the midline of the pen
				rect.Inflate(-halfEdgeThickness, -halfEdgeThickness);

				// make sure the radius is at most half the width
				radiusX = Math.Min(radiusX, rect.Width / 2d);

				// make sure the radius is at most half the height
				radiusY = Math.Min(radiusY, rect.Height / 2d);
			}
			else
			{
				const int SideCount = 4;
				int startSideIndex = (int)startSide;
				int endSideIndex = (int)endSide;

				if (endSideIndex <= startSideIndex)
					endSideIndex += SideCount;

				bool hasLeft = (int)RoundedRectSide.Left >= startSideIndex || ((int)RoundedRectSide.Left + SideCount) <= endSideIndex;
				bool hasTop = (int)RoundedRectSide.Top >= startSideIndex || ((int)RoundedRectSide.Top + SideCount) <= endSideIndex;
				bool hasRight = (int)RoundedRectSide.Right >= startSideIndex || ((int)RoundedRectSide.Right + SideCount) <= endSideIndex;
				bool hasBottom = (int)RoundedRectSide.Bottom >= startSideIndex || ((int)RoundedRectSide.Bottom + SideCount) <= endSideIndex;

				if (hasLeft)
				{
					rect.X += halfEdgeThickness;
					rect.Width = Math.Max(rect.Width - halfEdgeThickness, 0);
				}

				if (hasTop)
				{
					rect.Y += halfEdgeThickness;
					rect.Height = Math.Max(rect.Height - halfEdgeThickness, 0);
				}

				if (hasRight)
					rect.Width = Math.Max(rect.Width - halfEdgeThickness, 0);

				if (hasBottom)
					rect.Height = Math.Max(rect.Height - halfEdgeThickness, 0);

				// make sure the radius can be honored
				radiusX = Math.Min(radiusX, hasLeft & hasRight ? rect.Width / 2d : rect.Width);
				radiusY = Math.Min(radiusY, hasTop & hasBottom ? rect.Height / 2d : rect.Height);
			}
			#endregion //Adjust Rect

			if (rect.IsEmpty)
				return null;

			#region Setup

			StreamGeometry geometry = new StreamGeometry();

			// determine which corners we will be drawing as rounded
			bool isTopLeftRound = (roundedCorners & RoundedRectCorners.TopLeft) == RoundedRectCorners.TopLeft;
			bool isTopRightRound = (roundedCorners & RoundedRectCorners.TopRight) == RoundedRectCorners.TopRight;
			bool isBottomLeftRound = (roundedCorners & RoundedRectCorners.BottomLeft) == RoundedRectCorners.BottomLeft;
			bool isBottomRightRound = (roundedCorners & RoundedRectCorners.BottomRight) == RoundedRectCorners.BottomRight;

			Size radiusSize = new Size(radiusX, radiusY);

			#endregion //Setup

			#region Build Points

			// build all the points as if we were going to draw from the upper left all around
			const int PointCount = 8;
			Point[] points = new Point[PointCount];

			points[0] = isBottomLeftRound ? new Point(rect.Left, rect.Bottom - radiusY) : rect.BottomLeft; // left bottom
			points[1] = isTopLeftRound ? new Point(rect.Left, rect.Top + radiusY) : rect.TopLeft; // left top
			points[2] = isTopLeftRound ? new Point(rect.X + radiusX, rect.Top) : rect.TopLeft; // top left
			points[3] = isTopRightRound ? new Point(rect.Right - radiusX, rect.Top) : rect.TopRight; // top right
			points[4] = isTopRightRound ? new Point(rect.Right, rect.Top + radiusY) : rect.TopRight; // right top
			points[5] = isBottomRightRound ? new Point(rect.Right, rect.Bottom - radiusY) : rect.BottomRight; // right bottom
			points[6] = isBottomRightRound ? new Point(rect.Right - radiusX, rect.Bottom) : rect.BottomRight; // bottom right
			points[7] = isBottomLeftRound ? new Point(rect.Left + radiusX, rect.Bottom) : rect.BottomLeft; // bottom left

			#endregion //Build Points

			using (StreamGeometryContext context = geometry.Open())
			{
				// determine the starting point
				int startIndex = (int)startSide * 2;
				int endIndex = endSide == startSide ? PointCount : ((int)endSide * 2) + 1;

				if (endIndex < startIndex)
					endIndex += PointCount;

				// start in the top left
				Point previousPoint = points[startIndex];
				context.BeginFigure(previousPoint, true, false);

				for (int i = startIndex + 1; i <= endIndex; i++)
				{
					int index = i % PointCount;
					Point nextPoint = points[index];

					// when going from an even to an odd, we're doing a line to
					if (index % 2 == 1)
					{
						context.LineTo(nextPoint, true, false);
					}
					else if (nextPoint != previousPoint)
					{
						// we need an arc if the even point is not the same as the last point
						context.ArcTo(nextPoint, radiusSize, 0, false, SweepDirection.Clockwise, true, false);
					}

					previousPoint = nextPoint;
				}
			}

			return geometry;
		}
		#endregion //CreateRoundedRectGeometry (Rect, Corners, double, double, double, Side, Side)

        #region StringFromNonDefaultProperties

        /// <summary>
        /// Creates and returns a string with any non-default <see cref="DependencyProperty"/> values. 
        /// </summary>
        /// <param name="obj">The DependencyObject in question.</param>
        /// <returns>A string concatenated with any DependencyProperty values that are not set to their defaults</returns>
        public static string StringFromNonDefaultProperties(DependencyObject obj)
        {
            if (obj == null)
                return string.Empty;

            Type objectType = obj.GetType();

            SortedDictionary<string, object> nonDefaultProps = null;

            LocalValueEnumerator enumerator = obj.GetLocalValueEnumerator();

            while (enumerator.MoveNext())
            {
                LocalValueEntry entry = enumerator.Current;

                Type ownerType = entry.Property.OwnerType;

                // JJD 2/10/10 - TFS27344
                // Make sure the property was defined on the type of the object or one
                // of its base classes
                if (objectType == ownerType ||
                     ownerType.IsAssignableFrom(objectType))
                {
                    if (!Object.Equals(entry.Property.DefaultMetadata.DefaultValue, entry.Value))
                    {
                        if (nonDefaultProps == null)
                            nonDefaultProps = new SortedDictionary<string, object>();

                        nonDefaultProps.Add(entry.Property.Name, entry.Value);
                    }
                }
            }

            if (nonDefaultProps == null)
                return string.Empty;

            StringBuilder sb = null;

            foreach (KeyValuePair<string, object> keyValuePair in nonDefaultProps)
            {
                if (sb == null)
                    sb = new StringBuilder();
                else
                    sb.Append(", ");

                sb.Append(keyValuePair.ToString());
            }

            return sb.ToString();
        }

        #endregion //StringFromNonDefaultProperties

        #region DoesElementContainPoint



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        internal static bool DoesElementContainPoint(FrameworkElement elem, Point p)
        {
            return p.X >= 0 && p.Y >= 0 && p.X < elem.ActualWidth && p.Y < elem.ActualHeight;
        }

        #endregion // DoesElementContainPoint

        #region EnsureInElementBounds



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        internal static Point EnsureInElementBounds(Point point, FrameworkElement elem)
        {
            point.X = Utilities.EnsureInRange(point.X, 0, elem.ActualWidth);
            point.Y = Utilities.EnsureInRange(point.Y, 0, elem.ActualHeight);

            return point;
        }

        #endregion // EnsureInElementBounds

        #region EnsureInRange

        internal static double EnsureInRange(double val, double min, double max)
        {
            return Math.Min(Math.Max(val, min), max);
        }

        #endregion // EnsureInRange

		#region DoubleIsZero

		/// <summary>
		/// Returns true if the specified double value is zero or close to zero.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool DoubleIsZero(double value)
		{

			if ((Math.Abs(value) < Utilities.SMALLEST_DOUBLE))
				return true;

			return false;
		}

		#endregion //DoubleIsZero

		#region CreateBindingObject

		/// <summary>
		/// Create a Binding object
		/// </summary>
		public static Binding CreateBindingObjectLikeAlias(DependencyProperty dp)
		{
			Binding binding = new Binding();

			binding.Path = new PropertyPath(dp);
			binding.Mode = BindingMode.OneWay;
			binding.RelativeSource = RelativeSource.TemplatedParent;

			return binding;
		}

		/// <summary>
		/// Create a Binding object
		/// </summary>
		public static Binding CreateBindingObject(DependencyProperty dp, BindingMode bindingMode, RelativeSource relativeSource)
		{
			Binding binding = new Binding();

			binding.Path = new PropertyPath(dp);
			binding.Mode = bindingMode;
			binding.RelativeSource = relativeSource;

			return binding;
		}

		/// <summary>
		/// Create a Binding object
		/// </summary>
		public static Binding CreateBindingObject(DependencyProperty dp, BindingMode bindingMode, object source)
		{
			Binding binding = new Binding();

			binding.Path = new PropertyPath(dp);
			binding.Mode = bindingMode;
			binding.Source = source;

			return binding;
		}

		/// <summary>
		/// Create a Binding object
		/// </summary>
		public static Binding CreateBindingObject(DependencyProperty dp, BindingMode bindingMode, object source, IValueConverter converter)
		{
			Binding binding = new Binding();

			binding.Path = new PropertyPath(dp);
			binding.Mode = bindingMode;
			binding.Source = source;
			binding.Converter = converter;

			return binding;
		}

		/// <summary>
		/// Create a Binding object
		/// </summary>
		public static Binding CreateBindingObject(string path, BindingMode bindingMode, object source)
		{
 			Binding binding = new Binding();

            binding.Path = new PropertyPath(path);

			binding.Mode = bindingMode;
			binding.Source = source;

			return binding;
		}

		/// <summary>
		/// Create a Binding object
		/// </summary>
		public static Binding CreateBindingObject(string path, BindingMode bindingMode, object source, IValueConverter converter)
		{
			Binding binding = new Binding();

            binding.Path = new PropertyPath(path);
			binding.Mode = bindingMode;
			binding.Source = source;
			binding.Converter = converter;

			return binding;
		}

		#endregion CreateBindingObject

		#region ExecuteCommand
		/// <summary>
		/// Invokes the command of the specified <see cref="ICommandSource"/>.
		/// </summary>
		/// <param name="commandSource">The command source whose command is to be invoked.</param>
		/// <exception cref="ArgumentNullException">The <paramref name="commandSource"/> was null.</exception>
		public static void ExecuteCommand(ICommandSource commandSource)
		{
			if (commandSource == null)
				throw new ArgumentNullException("commandSource");

			if (CanExecuteCommand(commandSource))
			{
				ICommand command = commandSource.Command;

				RoutedCommand routedCmd = command as RoutedCommand;
				object parameter = commandSource.CommandParameter;

				if (routedCmd != null)
				{
					IInputElement target = commandSource.CommandTarget ?? commandSource as IInputElement;

					routedCmd.Execute(parameter, target);
				}
				else
					command.Execute(parameter);
			}
		}
		#endregion //ExecuteCommand

		#region FindLayoutTransform

		internal static Transform FindLayoutTransform(FrameworkElement element, Type transformType)
		{
			if (element.LayoutTransform == null)
				return null;


			if (element.LayoutTransform.GetType() == transformType)
				return element.LayoutTransform;


			if (element.LayoutTransform.GetType() == typeof(TransformGroup))
			{
				TransformGroup tg = element.LayoutTransform as TransformGroup;

				foreach (Transform t in tg.Children)
				{
					if (t.GetType() == transformType)
						return t;
				}
			}

			return null;
		}

		#endregion //FindLayoutTransform

		#region FindRenderTransform

		internal static Transform FindRenderTransform(UIElement element, Type transformType)
		{
			if (element.RenderTransform == null)
				return null;


			if (element.RenderTransform.GetType() == transformType)
				return element.RenderTransform;


			if (element.RenderTransform.GetType() == typeof(TransformGroup))
			{
				TransformGroup tg = element.RenderTransform as TransformGroup;

				foreach (Transform t in tg.Children)
				{
					if (t.GetType() == transformType)
						return t;
				}
			}

			return null;
		}

		#endregion //FindRenderTransform

		#region GetAncestorFromName

		/// <summary>
		/// Get an ancestor parent based on its type.
		/// </summary>
		/// <returns>An ancestor parent of the specified type or null if not found.</returns>
		public static FrameworkElement GetAncestorFromName(DependencyObject descendant, string name )
		{
			DependencyObject child = descendant;

            FrameworkElement fe;

			while(child != null)
			{
                fe = child as FrameworkElement;

                if (fe != null && fe.Name == name)
                    return fe;
                
				// JJD 8/23/07
				// Call the safer GetParent method that will tolerate FrameworkContentElements
                //child = VisualTreeHelper.GetParent(child) as DependencyObject;
                child = Utilities.GetParent(child);
			}

			return null;
        }

        #endregion GetAncestorFromName

        #region GetAncestorFromType

        /// <summary>
		/// Get an ancestor parent based on its type.
		/// </summary>
		/// <returns>An ancestor parent of the specified type or null if not found.</returns>
        public static DependencyObject GetAncestorFromType(DependencyObject descendant, Type type, bool allowSubclassOfType)
        {
            return GetAncestorFromType(descendant, type, allowSubclassOfType, null);
        }

		/// <summary>
		/// Get an ancestor parent based on its type.
		/// </summary>
		/// <returns>An ancestor parent of the specified type or null if not found.</returns>
		public static DependencyObject GetAncestorFromType(DependencyObject descendant, Type type, bool allowSubclassOfType, DependencyObject stopAtVisual)
		{
            return GetAncestorFromType(descendant, type, allowSubclassOfType, stopAtVisual, null);
		}
		/// <summary>
		/// Get an ancestor parent based on its type.
		/// </summary>
		/// <returns>An ancestor parent of the specified type or null if not found.</returns>
		public static DependencyObject GetAncestorFromType(DependencyObject descendant, 
															Type type, 
															bool allowSubclassOfType, 
															DependencyObject stopAtVisual,
															Type stopAtType)
		{
			DependencyObject child = descendant;

			DependencyObject parent;

			while(child != null)
			{
				// JJD 8/23/07
				// Call the safer GetParent method that will tolerate FrameworkContentElements
				//parent = VisualTreeHelper.GetParent(child) as DependencyObject;
				parent = Utilities.GetParent(child);

				if (parent != null)
				{
					if (parent == stopAtVisual)
						return null;

					if (stopAtType != null &&
						stopAtType == parent.GetType())
						return null;

					Type parentType = parent.GetType();

					if (parentType == type)
						return parent;

					// JJD 6/14/07
					// Use IsAssignableFrom instead of IsSubclassOf since that is 10x more efficient
					//if (allowSubclassOfType == true && parentType.IsSubclassOf(type))
					if (allowSubclassOfType == true && type.IsAssignableFrom(parentType))
						return parent;
				}

				child = parent;
			}

			return null;
		}

		#endregion GetAncestorFromType

        #region GetAncestorPropertyValue

        /// <summary>
		/// Returns the value of a property that has a non-null setting
		/// </summary>
		/// <returns>A non-null value setting from an ancestor or null if not found.</returns>
		public static object GetAncestorPropertyValue(DependencyObject descendant, DependencyProperty dp, DependencyObject stopAtVisual)
		{
			DependencyObject child = descendant;

			DependencyObject parent;

			while(child != null)
			{
				// JJD 8/23/07
				// Call the safer GetParent method that will tolerate FrameworkContentElements
				//parent = VisualTreeHelper.GetParent(child) as DependencyObject;
				parent = Utilities.GetParent(child);
				if (parent != null)
				{
					if (parent == stopAtVisual)
						return null;

					object value = parent.GetValue(dp);

					if (value != null)
						return value;
				}

				child = parent;
			}

			return null;
        }

        #endregion GetAncestorPropertyValue

		// JJD 10/27/10 - TFS37193 - added
		#region GetNonDefaultLanguage

		internal static XmlLanguage GetNonDefaultLanguage(DependencyObject element)
		{
			XmlLanguage language = element.GetValue(FrameworkElement.LanguageProperty) as XmlLanguage;

			if (language != null)
			{
				ValueSource vsource = DependencyPropertyHelper.GetValueSource(element, FrameworkElement.LanguageProperty);

				// If the language wasn't explicitly set then return null
				if (vsource.BaseValueSource == BaseValueSource.Default &&
					language.GetEquivalentCulture().IetfLanguageTag == "en-US")
					return null;
			}

			return language;
		}

		#endregion //GetNonDefaultLanguage	
    
        // JJD 4/15/08 - Added 
        #region GetNonNeutralCulture

        /// <summary>
        /// Returns a specific, i.e. non-neutral, culture that can be used to format strings.
        /// </summary>
        /// <param name="element">Either a FrameworkElement or a FrameworkContentElement</param>
        /// <returns>A specific culture that can be used to format strings, dates, etc.</returns>
        /// <exception cref="ArgumentException">If element is not a FrameworkElement or FrameworkContentElement.</exception>
        public static CultureInfo GetNonNeutralCulture(IFrameworkInputElement element)
        {

            XmlLanguage language = null;

			// JJD 10/27/10 - TFS37193 
			// Call GetNonDefaultLanguage which will retutn null if the language wasn't explicitly set
			// somewhere in the ancestor chain or if the default value wan't changed from 'en-US'
			if (element is FrameworkElement)
                //language = ((FrameworkElement)element).Language;
                language = GetNonDefaultLanguage(((FrameworkElement)element));
            else
                if (element is FrameworkContentElement)
                    //language = ((FrameworkContentElement)element).Language;
					language = GetNonDefaultLanguage(((FrameworkContentElement)element));
                else
                    if (element == null)
                        throw new ArgumentException(SR.GetString("LE_NonNeutralCultureRequiresElement"), "element");

            CultureInfo culture = null;

            if (language != null)
            {
                try
                {
                    culture = language.GetEquivalentCulture();
                }
                catch (InvalidOperationException)
                {
                }
            }

			// JJD 10/27/10 - TFS37193 
			// If we don't have a culture then fallback to the current culture
			if (culture == null)
			{
				// use the current culture as a fallback
				// SSP 7/9/08
				// Use the CurrentCulture instead of CurrentUICulture. That's what we've been using
				// in the past.
				// 
				//culture = Thread.CurrentThread.CurrentUICulture;
				culture = System.Globalization.CultureInfo.CurrentCulture;

				if (culture == null)
					culture = CultureInfo.InvariantCulture;
			}

            return GetNonNeutralCulture(culture);
        }


        /// <summary>
        /// Returns a specific, i.e. non-neutral, culture that can be used to format strings.
        /// </summary>
        /// <param name="culture">A CultureInfo object.</param>
        /// <returns>The passed in culture if it is non-neutral. Otherwise, a specific culture that is appropriate for the neutral culture. If a specific culture can not be found then the current Thread's CurrentUICulture will be returned.</returns>
        /// <exception cref="ArgumentNullException">If culture is null.</exception>
        public static CultureInfo GetNonNeutralCulture(CultureInfo culture)
        {
            if (culture == null)
                throw new ArgumentNullException("culture", SR.GetString("LE_NonNeutralCultureRequiresCulture"));

            // Since neutral cultures can not be used for formatting we need to find a 
            // spewcific culture that applies
            if (culture.IsNeutralCulture)
            {
                CultureInfo specificCulture = null;

                try
                {
                    CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);

                    foreach (CultureInfo cinfo in cultures)
                    {
                        // If the parent culture is the same as this culture 
                        // then use it
                        if (culture.Equals(cinfo.Parent))
                        {
                            specificCulture = cinfo;
                            break;
                        }
                    }

                    Debug.Assert(specificCulture != null);
                }
                catch (SecurityException)
                {
                }

				if ( specificCulture == null )
					// SSP 7/9/08
					// Use the CurrentCulture instead of CurrentUICulture. That's what we've been using
					// in the past.
					// 
					//specificCulture = Thread.CurrentThread.CurrentUICulture;
					specificCulture = System.Globalization.CultureInfo.CurrentCulture;

                // as a final fallback use the curretnUiCulture
                if (specificCulture == null || specificCulture.IsNeutralCulture)
                    specificCulture = CultureInfo.InvariantCulture;

                culture = specificCulture;
            }

            return culture;
        }

        #endregion //GetNonNeutralCulture	
     
        #region GetDescendantFromName

        /// <summary>
		/// Gets a descendant FrameworkElement based on its name.
		/// </summary>
		/// <returns>A descendant FrameworkElement with the specified name or null if not found.</returns>
		public static FrameworkElement GetDescendantFromName(DependencyObject parent, string name)
		{
			int count = VisualTreeHelper.GetChildrenCount(parent);
	
			if (count < 1)
				return null;

			FrameworkElement fe;

            for (int i = 0; i < count; i++)
			{
                fe = VisualTreeHelper.GetChild(parent, i) as FrameworkElement;
				if (fe != null)
				{
					if ( fe.Name == name)
						return fe;

					fe = GetDescendantFromName(fe, name);
					if (fe != null)
						return fe;
				}
			}

			return null;
		}

		#endregion GetDescendantFromName

		#region GetDescendantFromType

		/// <summary>
		/// Get a descendant parent based on its type.
		/// </summary>
		/// <param name="parent">DependencyObject whose descendants are to be searched.</param>
		/// <param name="type">Type of the descendant being sought</param>
		/// <param name="allowSubclassOfType">True if subclasses of <paramref name="type"/> may be considered.</param>
		/// <returns>A descendant parent of the specified type or null if not found.</returns>
		public static DependencyObject GetDescendantFromType(DependencyObject parent, Type type, bool allowSubclassOfType)
		{
			return GetDescendantFromType<DependencyObject>(parent, type, allowSubclassOfType, null, null);
		}

		/// <summary>
		/// Get a descendant parent based on its type.
		/// </summary>
		/// <param name="parent">DependencyObject whose descendants are to be searched.</param>
		/// <param name="allowSubclassOfType">True if subclasses of type may be considered.</param>
		/// <param name="callback">Delegate that should be invoked when a potential match is located. Return false from the callback to indicate that the object is not a match and that the search should be continued. Otherwise return true to indicate that the specified object should be returned from the method.</param>
		/// <returns>A descendant parent of the specified type or null if not found.</returns>
		public static T GetDescendantFromType<T>(DependencyObject parent, bool allowSubclassOfType, DependencyObjectSearchCallback<T> callback ) 
			where T : DependencyObject
		{
			return GetDescendantFromType<T>(parent, allowSubclassOfType, callback, null);
		}

		/// <summary>
		/// Get a descendant parent based on its type.
		/// </summary>
		/// <param name="parent">DependencyObject whose descendants are to be searched.</param>
		/// <param name="allowSubclassOfType">True if subclasses of type may be considered.</param>
		/// <param name="callback">Delegate that should be invoked when a potential match is located. Return false from the callback to indicate that the object is not a match and that the search should be continued. Otherwise return true to indicate that the specified object should be returned from the method.</param>
		/// <param name="typesToIgnore">Array of types identifying descendants that should not be searched.</param>
		/// <returns>A descendant parent of the specified type or null if not found.</returns>
		public static T GetDescendantFromType<T>(DependencyObject parent, bool allowSubclassOfType, DependencyObjectSearchCallback<T> callback, Type[] typesToIgnore ) 
			where T : DependencyObject
		{
			return GetDescendantFromType<T>(parent, typeof(T), allowSubclassOfType, callback, typesToIgnore);
		}



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		private static T GetDescendantFromType<T>(DependencyObject parent, Type type, bool allowSubclassOfType, DependencyObjectSearchCallback<T> callback, Type[] typesToIgnore) 
			where T : DependencyObject
		{
			int count = VisualTreeHelper.GetChildrenCount(parent);
	
			if (count < 1)
				return null;

			DependencyObject visual;

            for (int i = 0; i < count; i++)
			{
                T visualT = VisualTreeHelper.GetChild(parent, i) as T;
				if (visualT != null)
				{
					Type childType = visualT.GetType();

					// JJD 6/14/07
					// Use IsAssignableFrom instead of IsSubclassOf since that is 10x more efficient
					//if (childType == type ||
					//    (allowSubclassOfType == true && childType.IsSubclassOf(type)) )
					if (childType == type ||
						(allowSubclassOfType == true && type.IsAssignableFrom( childType )) )
					{
						if (callback != null && callback(visualT) == false)
							continue;

						return visualT;
					}
				}
			}


            for (int i = 0; i < count; i++)
			{
                visual = VisualTreeHelper.GetChild(parent, i);
				if (visual != null)
				{
					// if the type of object is in the list of descendants to ignore then do not delve into it
					if (typesToIgnore == null || false == ContainsType(typesToIgnore, visual.GetType()))
					{
						visual = GetDescendantFromType<T>(visual, type, allowSubclassOfType, callback, typesToIgnore);
						if (visual != null)
							return (T)visual;
					}
				}
			}

			return null;
		}

		private static bool ContainsType(Type[] types, Type typeToLocate)
		{
			for (int i = 0; i < types.Length; i++)
			{
				if (typeToLocate == types[i] || types[i].IsAssignableFrom(typeToLocate))
					return true;
			}

			return false;
		}

		#endregion GetDescendantFromName

		// AS 3/19/08 NA 2008 Vol 1 - XamDockManager
		#region GetElementToAdorn
		private static UIElement GetElementToAdorn(AdornerLayer layer, UIElement adornedElement)
		{
			if (null != layer)
			{
				DependencyObject parent = VisualTreeHelper.GetParent(layer);

				for (int i = 0, count = VisualTreeHelper.GetChildrenCount(parent); i < count; i++)
				{
					Visual child = VisualTreeHelper.GetChild(parent, i) as Visual;
					if (null != child && child.IsAncestorOf(adornedElement))
						return child as UIElement;
				}
			}

			return null;
		}
		#endregion //GetElementToAdorn

		#region GetFirstMnemonicChar

		/// <summary>
		/// Returns the first mnemonic character found in a string.
		/// </summary>
		/// <param name="text">A string that may contain one or more mnemonic characters.</param>
		/// <returns>The first mnemonic character found, converted to uppercase based on the current UI culture.</returns>
		/// <remarks>A mnemonic, or accelerator, character is defined as a character that has a single ampersand preceding it.</remarks>
		public static char GetFirstMnemonicChar(string text)
		{
			return GetFirstMnemonicChar(text, System.Globalization.CultureInfo.CurrentUICulture);
		}

		/// <summary>
		/// Returns the first mnemonic character found in a string.
		/// </summary>
		/// <param name="text">A string that may contain one or more mnemonic characters.</param>
		/// <param name="culture">The culture to use to convert the character to uppercase, null will return the character as is.</param>
		/// <returns>The first mnemonic character found, converted to uppercase based on the passed in culture.</returns>
		/// <remarks>A mnemonic, or accelerator, character is defined as a character that has a single ampersand preceding it.</remarks>
		public static char GetFirstMnemonicChar(string text, System.Globalization.CultureInfo culture)
		{
			if (text == null || text.Length < 1)
				return (char)0;

			int ampersandIndex = text.IndexOf('_');

			// If there is no ampersand then return 0
			if (ampersandIndex < 0)
				return (char)0;

			int currentIndex = ampersandIndex;

			// Loop over the string looking for the index of the first
			// '&' but ignore the index of the doubles '&&'.
			while (currentIndex <= text.Length - 2)
			{
				char nextchr = text[currentIndex + 1];
				if (char.IsLetterOrDigit(nextchr))
				{
					if (culture != null)
						return char.ToUpper(nextchr, culture);

					return nextchr;
				}

				// start at an index 2 higher than the last ampersand 
				// so that we strip by the next char.
				ampersandIndex = text.IndexOf('_', currentIndex + 2);

				if (ampersandIndex <= currentIndex)
					break;

				currentIndex = ampersandIndex;
			}

			return (char)0;
		}

		#endregion GetFirstMnemonicChar

		#region GetItemsPanelFromItem

		/// <summary>
		/// Returns the items host that contains the item inside an ItemsControl
		/// </summary>
		/// <param name="item"></param>
		/// <returns>The Items host or null.</returns>
		public static DependencyObject GetItemsHostFromItem(DependencyObject item)
		{
            DependencyObject child = item;

			DependencyObject parent;

			while(child != null)
			{
				// JJD 8/23/07
				// Call the safer GetParent method that will tolerate FrameworkContentElements
				//parent = VisualTreeHelper.GetParent(child) as DependencyObject;
				parent = Utilities.GetParent(child);

                // // if there is no parent parent then get the logical parent instead
                if ( parent == null )
                    parent = LogicalTreeHelper.GetParent(child);

                if (parent != null)
                {
					Panel panel = parent as Panel;

                    if (null != panel)
                    {
                        if (panel.IsItemsHost)
                            return parent;
                    }
                }

				if (parent is ItemsControl)
					return null;

				child = parent;
			}


			return null;
		}

		#endregion GetItemsPanelFromItem

		#region GetParent

		// JJD 8/23/07 added GetParent
		/// <summary>
		/// Returns the parent of a child element.
		/// </summary>
		/// <param name="child">The child element.</param>
		/// <returns>The parent element.</returns>
		/// <remarks>
		/// <para class="note"><b>Note:</b> if the child is derived from <see cref="Visual"/> or <see cref="Visual3D"/> this method returns its parent parent. However, if the child is derived from <see cref="FrameworkContentElement"/> this method returns its logical parent.</para></remarks>
		public static DependencyObject GetParent(DependencyObject child)
		{
			return GetParent( child, true );
		}

		// SSP 9/19/07
		// Added an overload that takes in includeLogicalParent parameter.
		// 
		/// <summary>
		/// Returns the parent of a child element.
		/// </summary>
		/// <param name="child">The child element.</param>
		/// <param name="includeLogicalParent">Whether to consider logical parent as well. 
		/// If the visual parent is null and this parameter is true then the logical parent 
		/// of the child will be returned.</param>
		/// <returns>The parent element.</returns>
		/// <remarks>
		/// <para class="note"><b>Note:</b> if the child derives from <see cref="Visual"/> or 
		/// <see cref="Visual3D"/> this method returns its visual parent. If the visual parent is null
		/// and includeLogicalParent parameter is true then it returns the logical parent of the child
		/// if any. If the child is derived from <see cref="FrameworkContentElement"/> this method returns 
		/// its logical parent unless includeLogicalParent parameter is false.</para>
		/// </remarks>
		public static DependencyObject GetParent( DependencyObject child, bool includeLogicalParent )
		{
			if (child == null)
				throw new ArgumentNullException("child");

			Visual visual = child as Visual;

			if (visual != null)
			{
				DependencyObject parent = VisualTreeHelper.GetParent(visual);

				if (parent != null)
					return parent;

				// SSP 9/19/07
				// Added an overload that takes in includeLogicalParent parameter.
				// Enclosed the existing code into the if block.
				// 
				if ( includeLogicalParent )
				{
					FrameworkElement fe = child as FrameworkElement;

					if ( fe != null )
						return fe.Parent;
				}

				return null;
			}

			// SSP 9/19/07
			// Added an overload that takes in includeLogicalParent parameter.
			// Enclosed the existing code into the if block.
			// 
			if ( includeLogicalParent )
			{
				FrameworkContentElement fce = child as FrameworkContentElement;

				// since FrameworkContentElements are not Visuals calling VisualTreeHelper.GetParent will
				// cause an exception so we just return the logical parent in this case
				if ( fce != null )
					return fce.Parent;
			}

			Visual3D visual3D = child as Visual3D;

			if (visual3D != null)
				return VisualTreeHelper.GetParent(visual3D);

			return null;

		}

		#endregion //GetParent	
    
		#region GetPropertyValueFromStyle

		/// <summary>
		/// Returns a property value from a style
		/// </summary>
		/// <param name="style">The source style</param>
		/// <param name="property">The property to look for.</param>
		/// <param name="walkUpBasedOnChain">True to search BasedOn styles.</param>
		/// <param name="returnBindingAsBinding">True to possibly return a binding object.</param>
		/// <returns>The value that was set or null.</returns>
		public static object GetPropertyValueFromStyle(Style style, 
														DependencyProperty property, 
														bool walkUpBasedOnChain,
														bool returnBindingAsBinding)
		{
			object value = GetPropertyValueFromStyleHelper(style, property, walkUpBasedOnChain);

			if (returnBindingAsBinding == false)
			{
				BindingExpressionBase expression = value as BindingExpressionBase;

				if (expression != null)
					return expression.ParentBindingBase.ProvideValue(null);
				
				BindingBase binding = value as BindingBase;

				if (binding != null)
					return binding.ProvideValue(null);
			}

			return value;
		}

		private static object GetPropertyValueFromStyleHelper(Style style, DependencyProperty property, bool walkUpBasedOnChain)
		{
			if (style == null)
				throw new ArgumentNullException("style");

			if (property == null)
				throw new ArgumentNullException("property");

			// look for a setter for the passed in property and return its value
			foreach (SetterBase setterBase in style.Setters)
			{
				Setter setter = setterBase as Setter;

				if (setter != null &&
					 setter.Property == property)
					return setter.Value;
			}

			// AS 5/3/07 BR22498
			object value = null;

			// if the style is based on another style then call this method
			// recursively to, in effect, walk up the 'BasedOn' chain
			// AS 5/3/07 BR22498
			// There appears to be a bug in the Vista 64 clr jitter that when you return
			// directly from this if block that it causes an invalidprogramexception.
			//
			//if (walkUpBasedOnChain == true && style.BasedOn != null)
			//	return GetPropertyValueFromStyleHelper(style.BasedOn, property, walkUpBasedOnChain);
			if (walkUpBasedOnChain)
			{
				Style basedOnStyle = style.BasedOn;

				if (null != basedOnStyle)
					value = GetPropertyValueFromStyleHelper(basedOnStyle, property, true);
			}

			// AS 5/3/07 BR22498
			//return null;
			return value;
		}

		#endregion //GetPropertyValueFromStyle
    
		#region GetPublicKeyToken

		/// <summary>
		/// Returns the public key token of a type's assembly.
		/// </summary>
		/// <param name="type">The <see cref="System.Type"/> whose assembly is checked.</param>
		/// <returns>The public key token value. Either "null" or a 16 byte string.</returns>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static string GetPublicKeyToken(System.Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			// get the assembly for the passed in object
			//
			System.Reflection.Assembly assembly = System.Reflection.Assembly.GetAssembly(type);

			// get the assembly for the passed in object
			//
			return GetPublicKeyToken(assembly);
		}

		/// <summary>
		/// Returns the public key token of an assembly.
		/// </summary>
		/// <param name="assemblyToCheck">The assembly to check.</param>
		/// <returns>The public key token value. Either "null" or a 16 byte string.</returns>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static string GetPublicKeyToken(System.Reflection.Assembly assemblyToCheck)
		{
			if (assemblyToCheck == null)
				throw new ArgumentNullException("assemblyToCheck");

			// split the fullname between '=' characters
			//
			string[] split = assemblyToCheck.FullName.Split(new char[] { '=' });
			string substr;

			// start at the last split and work backwards (since the PublicKeyToken is 
			// currently last) to find which string contains the publickeytoken
			//
			for (int i = split.Length - 1; i > 0; i--)
			{
				// if the preceding split string is shorted than the 
				// PublicKeyToken literal then continue
				//
				if (split[i - 1].Length < publicKeyTokenLiteral.Length)
					continue;

				// get the right most part of the preceding split string
				// so we can compare it to the  PublicKeyToken literal
				//
				substr = split[i - 1].Substring(split[i - 1].Length - publicKeyTokenLiteral.Length);

				// check for a match on the literal
				//
				if (substr.Equals(publicKeyTokenLiteral))
				{
					// since the public key token is only 16 characters long
					// we need to truncate it if it is longer.

					// If it is less than 17 characters in length
					// return the entire string
					//
					if (split[i].Length < 17)
						return split[i];

					// Otherwise return the leftmost 16 bytes
					//
					return split[i].Substring(0, 16);
				}
			}

			Debug.Fail("Public key token not found in DisposableObject.GetPublicKeyToken");

			// The PublicKeyToken literal was not found so return null
			//
			return string.Empty;
		}

		#endregion //GetPublicKeyToken	

		// AS 3/19/08 NA 2008 Vol 1 - XamDockManager
		#region GetRootAdornerLayer
		/// <summary>
		/// Helper method for getting the root adorner layer.
		/// </summary>
		/// <param name="adornedElement"></param>
		/// <param name="elementToAdorn">An out parameter that is set to the element that the layer being returned adorns</param>
		/// <returns></returns>
		internal static AdornerLayer GetRootAdornerLayer(UIElement adornedElement, out UIElement elementToAdorn)
		{
			AdornerLayer layer = AdornerLayer.GetAdornerLayer(adornedElement);

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
				}

				// use the previous one
				layer = previous;
			}

			elementToAdorn = GetElementToAdorn(layer, adornedElement);

			return layer;
		}
		#endregion //GetRootAdornerLayer

        // AS 1/15/09 NA 2009 Vol 1 - Fixed Fields
        #region GetTemplateChild
        /// <summary>
        /// Searches an element for an templated child of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of child to return.</typeparam>
        /// <param name="templatedParent">The element whose template children are to be searched.</param>
        /// <returns>An element of the specified type or null.</returns>
        public static T GetTemplateChild<T>(FrameworkElement templatedParent)
			where T : DependencyObject 
        {
            return GetTemplateChild<T>(templatedParent, templatedParent, null);
        }
        /// <summary>
        /// Searches an element for an templated child of the specified type.
        /// </summary>
        /// <param name="templatedParent">The element whose template children are to be searched.</param>
        /// <typeparam name="T">The type of child to return.</typeparam>
        /// <param name="callback">A delegate that is called when an element of the appropriate type is encountered to determine if that is the element that should be returned or if the search should continue.</param>
        /// <returns>An element of the specified type or null.</returns>
        public static T GetTemplateChild<T>(FrameworkElement templatedParent, DependencyObjectSearchCallback<T> callback)
			where T : DependencyObject
        {
            return GetTemplateChild(templatedParent, templatedParent,callback);
        }

		private static T GetTemplateChild<T>(FrameworkElement templatedParent, DependencyObject parent, DependencyObjectSearchCallback<T> callback)
			where T : DependencyObject
		{
			HashSet processedChildren = new HashSet();
			return GetTemplateChild<T>(templatedParent, parent, callback, processedChildren);
		}

		// AS 11/11/09
		// Since we're going to enumerate the logical as well as the visual tree we should try to 
		// avoid recursing into the same object multiple times so we'll keep a list of the children
		// we've seen.
		//
		private static T GetTemplateChild<T>(FrameworkElement templatedParent, DependencyObject parent, DependencyObjectSearchCallback<T> callback, HashSet processedChildren)
			where T : DependencyObject
        {
			if (null == parent)
				return null;

			// AS 11/11/09
			// We need to account for FrameworkContentElements as well.
			//
			//FrameworkElement fe = null;
			DependencyObject actualTemplatedParent = null;

			// AS 11/11/09
			// Added loop to enumerate the logical children because the template 
			// child may have its own control template and while we want to skip
			// those children we still want to chck its logical children since they 
			// could be template children of the specified templatedParent.
			//
			T targetChild;
			foreach (object logicalChild in LogicalTreeHelper.GetChildren(parent))
			{
				DependencyObject child = logicalChild as DependencyObject;

				if (GetTemplatedParent(child, out actualTemplatedParent) && actualTemplatedParent != templatedParent)
					continue;

				if (GetTemplateChild(templatedParent, callback, child, processedChildren, out targetChild))
					return targetChild;
			}

			// do not try to get visual children of a non-visual object
			if (!IsVisual(parent))
				return null;

            for (int i = 0, count = VisualTreeHelper.GetChildrenCount(parent); i < count; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

				// AS 11/11/09
				// To keep this consistent we'll use the same helper method that I added for 
				// checking the templated parent of logical children. At least right now only 
				// FrameworkElement would be here.
				//
				//fe = child as FrameworkElement;
				//
                //if (null != fe && fe.TemplatedParent != templatedParent)
                //    return null;
				if (GetTemplatedParent(child, out actualTemplatedParent) && actualTemplatedParent != templatedParent)
					return null;

				// AS 11/11/09
				// Moved into a helper method since we also need to iterate the
				// logical children.
				//
				//if (null != child)
				//{
				//    if (child is T)
				//    {
				//        if (null == callback || callback((T)child))
				//            return (T)child;
				//    }
				//
				//    child = GetTemplateChild(templatedParent, child, callback);
				//
				//    if (null != child)
				//        return (T)child;
				//}
				if (GetTemplateChild(templatedParent, callback, child, processedChildren, out targetChild))
					return targetChild;
			}

            return null;
        }

		// AS 11/11/09
		// Moved the callback/type check from the private GetTemplateChild into a helper
		// method since we need to do the same logic when checking the logical children.
		//
		private static bool GetTemplateChild<T>(FrameworkElement templatedParent, DependencyObjectSearchCallback<T> callback, DependencyObject child, HashSet processedChildren, out T targetChild)
			where T : DependencyObject
		{
			if (null != child && !processedChildren.Exists(child))
			{
				targetChild = child as T;

				if (targetChild != null)
				{
					if (null == callback || callback(targetChild))
						return true;
				}

				processedChildren.Add(child);

				targetChild = GetTemplateChild(templatedParent, child, callback, processedChildren);

				if (null != targetChild)
					return true;
			}

			targetChild = null;
			return false;
		}
		#endregion //GetTemplateChild

		// AS 11/11/09
		#region GetTemplatedParent
		// AS 11/3/11 TFS88222
		internal static DependencyObject GetTemplatedParent(DependencyObject reference)
		{
			DependencyObject tp;
			GetTemplatedParent(reference, out tp);
			return tp;
		}

		private static bool GetTemplatedParent(DependencyObject reference, out DependencyObject templatedParent)
		{
			FrameworkElement fe = reference as FrameworkElement;

			if (null != fe)
			{
				templatedParent = fe.TemplatedParent;
				return true;
			}

			FrameworkContentElement fce = reference as FrameworkContentElement;

			if (null != fce)
			{
				templatedParent = fce.TemplatedParent;
				return true;
			}

			templatedParent = null;
			return false;
		}
		#endregion //GetTemplatedParent

        // JJD 12/17/08 TFS10903 - added 
        #region HasLoadedAncestor

        /// <summary>
        /// Determines if any ancestor element is loaded.
        /// </summary>
        /// <param name="d">The starting element</param>
        /// <returns>True if an ancestor FrameworkElement is loaded, otherwise false.</returns>
        public static bool HasLoadedAncestor(DependencyObject d)
        {
            DependencyObject root = d;
            DependencyObject previous = null;

            while (root != null)
            {
                FrameworkElement fe = root as FrameworkElement;

                if (null != fe && fe.IsLoaded)
                    return true;

                previous = root;
                root = LogicalTreeHelper.GetParent(root);

                if (null == root && (previous is Visual || previous is System.Windows.Media.Media3D.Visual3D))
                    root = VisualTreeHelper.GetParent(previous);
            }

            return false;
        }

        #endregion //HasLoadedAncestor	
        
		#region HasSamePublicKey

		/// <summary>
		/// Checks if the test object is from an assembly that is
		/// signed with the same public key as this assembly
		/// </summary>
		/// <returns>True if same</returns>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static bool HasSamePublicKey(System.Type type)
		{
			if (type == null)
				return false;

			// JJD 10/25/01
			// Get the public key tokens for the passed in type as well
			// as this type
			//
			string publicKeyToken = GetPublicKeyToken(type);

			// AS 5/18/10
			// Accept the development SL key file.
			//
			if (string.Equals(AssemblyVersion.PublicKeyToken, publicKeyToken))
				return true;

			string thisPublicKeyToken = GetPublicKeyToken(typeof(Utilities));

			// return true if they are equal
			//
			return publicKeyToken.Equals(thisPublicKeyToken);
		}

		#endregion //HasSamePublicKey	

		// AS 3/19/08 NA 2008 Vol 1 - XamDockManager
		#region InvalidateMeasure


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal static void InvalidateMeasure(UIElement descendant, UIElement ancestor)
		{
			while (true)
			{
				UIElement parent = VisualTreeHelper.GetParent(descendant) as UIElement;

				if (parent == null || parent == ancestor)
					break;

				parent.InvalidateMeasure();
				descendant = parent;
			}
		}
		#endregion //InvalidateMeasure

		#region IsDescendantOf
		/// <summary>
		/// Indicates if the specified <paramref name="ancestor"/> is the visual parent of <paramref name="descendant"/> even if the descendant is within a popup.
		/// </summary>
		/// <param name="ancestor">The element to consider as the potential ancestor of <paramref name="descendant"/></param>
		/// <param name="descendant">The element being evaluated to determine if it is a child of <paramref name="ancestor"/></param>
		/// <returns>True if the <paramref name="descendant"/> is in the tree of the <paramref name="ancestor"/>.</returns>
        public static bool IsDescendantOf(DependencyObject ancestor, DependencyObject descendant)
        {
            // AS 2/5/09 TFS11796
            return IsDescendantOf(ancestor, descendant, false);
        }

        // AS 2/5/09 TFS11796
        // Added new overload which allowed us to prefer the logical tree when walking up. 
        // Previously this method would only walk up the visual tree using the logical tree
        // only when there was no logical parent.
        //
        /// <summary>
		/// Indicates if the specified <paramref name="ancestor"/> is an ancestor of <paramref name="descendant"/> even if the descendant is within a popup.
		/// </summary>
		/// <param name="ancestor">The element to consider as the potential ancestor of <paramref name="descendant"/></param>
		/// <param name="descendant">The element being evaluated to determine if it is a child of <paramref name="ancestor"/></param>
        /// <param name="preferLogicalTree">A boolean indicating whether to prefer the logical tree when walking up the ancestor chain.</param>
		/// <returns>True if the <paramref name="descendant"/> is in the tree of the <paramref name="ancestor"/>.</returns>
		public static bool IsDescendantOf(DependencyObject ancestor, DependencyObject descendant, bool preferLogicalTree)
		{
			return IsDescendantOf(ancestor, descendant, preferLogicalTree, false);
		}

		// AS 2/17/12 TFS100637
		/// <summary>
		/// Indicates if the specified <paramref name="ancestor"/> is an ancestor of <paramref name="descendant"/> even if the descendant is within a popup.
		/// </summary>
		/// <param name="ancestor">The element to consider as the potential ancestor of <paramref name="descendant"/></param>
		/// <param name="descendant">The element being evaluated to determine if it is a child of <paramref name="ancestor"/></param>
        /// <param name="preferLogicalTree">A boolean indicating whether to prefer the logical tree when walking up the ancestor chain.</param>
		/// <param name="usePopupTarget">True to use the PlacementTarget of the popup if possible</param>
		/// <returns>True if the <paramref name="descendant"/> is in the tree of the <paramref name="ancestor"/>.</returns>
		internal static bool IsDescendantOf(DependencyObject ancestor, DependencyObject descendant, bool preferLogicalTree, bool usePopupTarget)
		{
			if (ancestor == null)
				throw new ArgumentNullException("ancestor");

			while (descendant != null)
			{
				// AS 2/17/12 TFS100637
				if (usePopupTarget && descendant is Popup && ((Popup)descendant).PlacementTarget != null)
					descendant = ((Popup)descendant).PlacementTarget;
				else
                // AS 2/5/09 TFS11796
				//descendant = GetParent(descendant);
                if (!preferLogicalTree)
                    descendant = GetParent(descendant);
                else
                    descendant = LogicalTreeHelper.GetParent(descendant) ?? GetParent(descendant, false);

				if (ancestor == descendant)
					return true;
			}

			return false;
		} 
		#endregion //IsDescendantOf

		// AS 11/11/09
		#region IsVisual
		private static bool IsVisual(object reference)
		{
			return reference is Visual || reference is Visual3D;
		}
		#endregion //IsVisual

        #region LoadCursor
        /// <summary>
        /// Loads a cursor from the specified resource
        /// </summary>
        /// <param name="typeInAssembly">The type whose assembly contains the resource</param>
        /// <param name="cursorPath">The path to the cursor in the assembly</param>
        /// <returns>A cursor for the specified resource</returns>
        public static Cursor LoadCursor(Type typeInAssembly, string cursorPath)
        {
            ThrowIfNull(typeInAssembly, "typeInAssembly");
            ThrowIfNullOrEmpty(cursorPath, "cursorPath");

            Uri uri = BuildEmbeddedResourceUri(typeInAssembly.Assembly, cursorPath);

            try
            {
                return LoadCursor(uri);
            }
			catch (System.IO.IOException) // AS 8/6/12 TFS117486
			{
				return null;
			}
			catch (UnauthorizedAccessException) // AS 8/6/12 TFS117486
			{
				return null;
			}
            catch (SecurityException)
            {
                return null;
            }
        }

        internal static Cursor LoadCursor(Uri uri)
        {
            System.Windows.Resources.StreamResourceInfo resourceInfo = Application.GetResourceStream(uri);

            Debug.Assert(null != resourceInfo);

            return new Cursor(resourceInfo.Stream);
        }
        #endregion // LoadCursor

		#region RemoveAll

		/// <summary>
		/// Removes all occurrences of itemToRemove from list.
		/// </summary>
		/// <param name="list"></param>
		/// <param name="itemToRemove"></param>
		public static void RemoveAll( ArrayList list, object itemToRemove )
		{
			int delta = 0;
			int count = list.Count;

			for ( int i = 0; i < count; i++ )
			{
				if ( itemToRemove == list[i] )
					delta++;
				else if ( 0 != delta )
					list[i - delta] = list[i];
			}

			list.RemoveRange( count - delta, delta );
		}

		#endregion // RemoveAll

		// AS 5/11/12 TFS104724
		#region SetFocusedElement
		internal static void SetFocusedElement(DependencyObject focusScope, UIElement newFocusedElement)
		{
			// AS 10/8/08 TFS8629
			// I found this while debugging TFS8629. Basically setting the focused
			// element may change the keyboard focus as well. We'll prevent this by
			// handling the PreviewLostKeyboardFocus event
			KeyboardFocusChangedEventHandler previewHandler = delegate(object s, KeyboardFocusChangedEventArgs se)
			{
				if (se.NewFocus == newFocusedElement)
					se.Handled = true;
			};

			// AS 2/9/09 TFS13375
			// When focus was shifted into an hwnd host then in all likelihood the FocusedElement 
			// is null so they be able to ask the "old" focused element so handling the 
			// PreviewLostKeyboardFocus is not enough - we need to handle the PreviewGotKeyboardFocus 
			// and cancel that. Otherwise the WPF framework will shift keyboard focus into 
			// the WPF window and take it from the HwndHost's child window.
			//
			newFocusedElement.AddHandler(Keyboard.PreviewGotKeyboardFocusEvent, previewHandler);

			newFocusedElement.AddHandler(Keyboard.PreviewLostKeyboardFocusEvent, previewHandler);

			FocusManager.SetFocusedElement(focusScope, newFocusedElement);


			newFocusedElement.RemoveHandler(Keyboard.PreviewLostKeyboardFocusEvent, previewHandler);

			// AS 2/9/09 TFS13375
			newFocusedElement.RemoveHandler(Keyboard.PreviewGotKeyboardFocusEvent, previewHandler);
		}
		#endregion //SetFocusedElement

		#region SetPropertyFromStyle

		/// <summary>
		/// Extracts a property value from a Style and sets it on a DependencyObject
		/// </summary>
		/// <param name="target">The target to receive the property setting</param>
		/// <param name="targetProperty">The property to set</param>
		/// <param name="source">The style that contains the setting</param>
		/// <param name="sourceProperty">The property to extract</param>
		/// <param name="walkUpBasedOnChain">True to search up the 'BasedOn' style chain.</param>
		/// <returns>True if a value was found and set.</returns>
		public static bool SetPropertyFromStyle(DependencyObject target, 
												DependencyProperty targetProperty, 
												Style source, 
												DependencyProperty sourceProperty, 
												bool walkUpBasedOnChain)
		{
			if (target == null)
				throw new ArgumentNullException("target");

			if (sourceProperty == null)
				throw new ArgumentNullException("sourceProperty");
			
			if (targetProperty == null)
				throw new ArgumentNullException("targetProperty");

			if (!targetProperty.PropertyType.IsAssignableFrom(sourceProperty.PropertyType ))
				throw new NotSupportedException( SR.GetString( "LE_NotSupportedException_10" ) );

			object value = GetPropertyValueFromStyle(source, sourceProperty, walkUpBasedOnChain, true);

			BindingExpressionBase expression = value as BindingExpressionBase;

			if (expression != null)
			{
				BindingOperations.SetBinding(target, targetProperty, expression.ParentBindingBase);
				return true;
			}

			BindingBase binding = value as BindingBase;

			if (binding != null)
			{
				if (target is FrameworkElement)
				{
					((FrameworkElement)target).SetBinding(targetProperty, binding);
					return true;
				}

				if (target is FrameworkContentElement)
				{
					((FrameworkContentElement)target).SetBinding(targetProperty, binding);
					return true;
				}

				// can't set a binding
				return false;
			}

			if (value == null)
				return false;

			bool isAssignable = targetProperty.PropertyType.IsAssignableFrom(value.GetType());

			Debug.Assert(isAssignable);

			if ( isAssignable )
				target.SetValue(targetProperty, value);

			return isAssignable;
		}

		#endregion //SetPropertyFromStyle	
    
        #region SortMerge

        /// <summary>
        /// Sorts the passed in array list based on the passed in comparer using a modified merge-sort
        /// algorithm. 
        /// </summary>
        /// <param name="arrayList">The list to be sorted.</param>
        /// <param name="comparer">The comparer (must not be null).</param>
        public static void SortMerge(ArrayList arrayList, IComparer comparer)
        {
            if (arrayList == null)
                throw new ArgumentNullException("arrayList");

            if (comparer == null)
                throw new ArgumentNullException("comparer");

            // get the items as an array
            object[] array = arrayList.ToArray();

            // sort the array
            SortMerge(array, null, comparer);

            // clear the array list
            arrayList.Clear();

            // Add the sorted items back into the array list
            arrayList.AddRange(array);
        }

        #endregion // SortMerge

		#region StripMnemonics

		/// <summary>
		/// Strips mnemonics from a string.
		/// </summary>
		/// <param name="text">A string that may contain one or more mnemonic characters.</param>
		/// <param name="escapeRemainingMnemonics">True if any subsequent mnemonics should be escaped (if not already escaped by a preceeding '_'). This should be true if this will be assigned to an AccessText element (or a ContentPresenter whose RecognizesAccessKey is true) so that no character will be rendered as a mnemonic. This should be false to provide the behavior where the text appears as it would in an AccessText when the underline under the leading mnemonic is not rendered.</param>
		/// <returns>A new string with those mnemonics stripped out or the passed in string if it didn't contain any.</returns>
		/// <remarks>A mnemonic, or accelerator, character is defined as a character that has a single underscore ('_') preceding it.</remarks>
		public static string StripMnemonics(string text, bool escapeRemainingMnemonics)
		{
			if (text == null || text.Length < 2)
				return text;

			const char MnemonicPrefixChar = '_';
			int ampersandIndex = text.IndexOf(MnemonicPrefixChar);

			// If there is no ampersand then use the string as is
			if (ampersandIndex < 0)
				return text;

			StringBuilder sb = new StringBuilder();

			// use whatever is before the first underscore
			if (ampersandIndex > 0)
				sb.Append(text.Substring(0, ampersandIndex));

			int lastMnemonicIndex = ampersandIndex;
			bool foundFirstMnemonic = false;

			
#region Infragistics Source Cleanup (Region)








































#endregion // Infragistics Source Cleanup (Region)

			// basically we have 2 situations. we either have text with mnemonics
			// which we want to show in an access text as not having mnemonics or 
			// we have text with mnemonics that we want to make sure does not show
			// unnecessary underlines when shown in a textblock. 
			// for the former, we want to:
			// (a) remove the underscore before the 1st mnemonic
			// (b) keep 2 consecutive _ as is (i.e. __)
			// (c) replace any _ + character combination after the first mnemonic
			//		with __ + character so it renders with an underscore in front.
			// for the latter, we want to:
			// (a) remove the underscore before the 1st mnemonic
			// (b) treat 2 consecutive _ as a single _ (i.e. __ becomes _)
			// (c) leave any _ + character combination after the first mnemonic
			//		as is so it renders with an underscore in front.
			for (int i = ampersandIndex, len = text.Length - 1; i < len; i++)
			{
				char current = text[i];
				// AS 6/14/11 TFS73058
				//char next = text[i + 1];
				char next;

				if (current == MnemonicPrefixChar)
				{
					// AS 6/14/11 TFS73058
					// We only need the next character if this is a mnemonic.
					//
					next = text[i + 1];

					// if the next character is a mnemonic...
					if (next == MnemonicPrefixChar)
					{
						// if this will be shown in an access text then we want
						// to keep 2 consecutive underscores
						if (escapeRemainingMnemonics)
							sb.Append(MnemonicPrefixChar);

						// in either case we'll want an underscore so 
						// let that character get added
					}
					else
					{
						// the next char is a letter, etc.
						if (foundFirstMnemonic)
						{
							// if we're found the first mnemonic then for escaping
							// replace with double _ so it shows the underline in front
							// as the access text would if there were a leading mnemonic
							if (escapeRemainingMnemonics)
								sb.Append(MnemonicPrefixChar);

							sb.Append(MnemonicPrefixChar);
						}
						else
						{
							// this is the first mnemonic. in either case we want to pull
							// out the leading mnemonic and include just the character itself
							foundFirstMnemonic = true;
						}
					}

					// finally add the next character - whether its an underscore, letter, etc.
					sb.Append(next);
					i++;
				}
				else
				{
					// this is just a letter so add it
					sb.Append(current);
				}

				// if this is the last character, then just include the next one
				// as well. it doesn't matter if its a letter, underscore, etc.
				if (i == len - 1)
				{
					// AS 6/14/11 TFS73058
					// Get the next character to process.
					//
					next = text[i + 1];

					sb.Append(next);
				}
			}

			return sb.ToString();
		}

		#endregion StripMnemonics

		#region TransformPointToAncestorCoordinates

		/// <summary>
		/// Transforms a point to its ancestor's coordinates
		/// </summary>
		public static Point TransformPointToAncestorCoordinates(Point pointToTransform, Visual descendantVisual, Visual ancestorVisual)
		{
            MatrixTransform mt = descendantVisual.TransformToAncestor(ancestorVisual) as MatrixTransform;
			Matrix			matrix	= mt.Matrix;

			return matrix.Transform(pointToTransform);
		}

		#endregion TransformPointToAncestorCoordinates

		#region TransformPointToDescendantCoordinates

		/// <summary>
		/// Transforms a point to its descendant's coordinates
		/// </summary>
		public static Point TransformPointToDescendantCoordinates(Point pointToTransform, Visual descendantVisual, Visual ancestorVisual)
		{
            MatrixTransform mt = ancestorVisual.TransformToDescendant(descendantVisual) as MatrixTransform;
			Matrix matrix = mt.Matrix;

			return matrix.Transform(pointToTransform);
		}

		#endregion TransformPointToDescendantCoordinates

		#region DependencyObjectSearchCallback

		/// <summary>
		/// Delegate used to search for a specific <see cref="DependencyObject"/> when using the <see cref="Infragistics.Windows.Utilities.GetDescendantFromType&lt;T&gt;(DependencyObject, bool, DependencyObjectSearchCallback&lt;T&gt;)"/>
		/// </summary>
		/// <param name="dependencyObject">The dependency object being evaluated</param>
		/// <returns>Return true if the search should be stopped and object should be returned; otherwise return false to continue the search</returns>
		public delegate bool DependencyObjectSearchCallback<T>(T dependencyObject) 
			where T : DependencyObject; 

		#endregion //DependencyObjectSearchCallback

		#region GetDefaultValue

		/// <summary>
		/// Gets the default value for the specified property for the specified object.
		/// </summary>
		/// <param name="dependencyProperty"></param>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static object GetDefaultValue( DependencyProperty dependencyProperty, DependencyObject obj )
		{
			PropertyMetadata data = dependencyProperty.GetMetadata( obj );

			Debug.Assert( null != data );

			return null != data ? data.DefaultValue : dependencyProperty.DefaultMetadata.DefaultValue;
		}

		#endregion // GetDefaultValue

		#region ShouldSerialize

		/// <summary>
		/// A helper method for figuring out whether a property needs to be serialized.
		/// </summary>
		/// <param name="dependencyProperty"></param>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static bool ShouldSerialize( DependencyProperty dependencyProperty, DependencyObject obj )
		{
			object defVal = GetDefaultValue( dependencyProperty, obj );
			object currVal = obj.GetValue( dependencyProperty );

			return ! object.Equals( defVal, currVal );
		}

		#endregion // ShouldSerialize

		#region CreateResourceSetDictionary
		/// <summary>
		/// Helper method for creating a resource dictionary containing the specified sources.
		/// </summary>
		/// <param name="assembly">Assembly that contains the embedded resource.</param>
		/// <param name="resourcePath">Path to the embedded resource dictionary within the specified <paramref name="assembly"/>.</param>
		/// <returns></returns>
		public static ResourceDictionary CreateResourceSetDictionary(Assembly assembly, string resourcePath)
		{
			if (assembly == null)
				throw new ArgumentNullException("assembly");

			if (resourcePath == null)
				throw new ArgumentNullException("resourcePath");

			ResourceDictionary dictionary = new ResourceDictionary();

			string[] paths = resourcePath.Split(';');

			// AS 6/18/08
			// Found this while working on BR33926. Really we shouldn't be 
			// manipulating the merged dictionaries if we set the source. 
			// If we have only 1 dictionary, then we'll just set its source
			// otherwise we'll create a dictionary for each source and put
			// all of them into a single dictionary.
			//
			//Uri uri = Utilities.BuildEmbeddedResourceUri(assembly, paths[0]);
			//dictionary.Source = uri;
			//
			//for (int i = 1; i < paths.Length; i++)
			//{
			if (paths.Length == 1)
			{
				Uri uri = Utilities.BuildEmbeddedResourceUri(assembly, paths[0]);
				dictionary.Source = uri;
			}
			else
			{
				for (int i = 0; i < paths.Length; i++)
				{
					// skip any empty paths
					if (paths[i].Length == 0)
						continue;

					// load the next dictionary
					ResourceDictionary addlDictionary = new ResourceDictionary();
					Uri uri = Utilities.BuildEmbeddedResourceUri(assembly, paths[i]);
					addlDictionary.Source = uri;

					// add it to the merged dictionaries of the previous
					dictionary.MergedDictionaries.Add(addlDictionary);
				}
			}

			return dictionary;
		} 
		#endregion //CreateResourceSetDictionary

		#region CaretBlinkTime

		/// <summary>
		/// Returns the caret blink time in milliseconds.
		/// </summary>
		[ EditorBrowsable( EditorBrowsableState.Never ) ]
		public static int CaretBlinkTime
		{
			get
			{
				return NativeWindowMethods.CaretBlinkTime;
			}
		}

		#endregion // CaretBlinkTime

        // JJD 11/06/07 - Added methods to help support XBAP semi-trust applications
        #region PointFromScreenSafe

        /// <summary>
        /// Translates a point in screen coordinates into coordinates relative to an element
        /// </summary>
        /// <param name="element">The specified element</param>
        /// <param name="point">The point in screen coordinates.</param>
        /// <returns>The point translated into coordinates relative to the element.</returns>
        /// <exception cref="ArgumentNullException">If element is null</exception>
        /// <remarks>
        /// <para class="note"><b>Note:</b> In an XBAP application we don't have access rights to call PointFromScreen. In this situation we walk up the visual tree to the top level element and map the point from it.</para>
        /// </remarks>
        public static Point PointFromScreenSafe(Visual element, Point point)
        {
            return PointFromScreenSafe(element, point, null);
        }

        /// <summary>
        /// Translates a point in screen coordinates into coordinates relative to an element
        /// </summary>
        /// <param name="element">The specified element</param>
        /// <param name="point">The point in screen coordinates.</param>
        /// <param name="refElement">An optional second element to reference.</param>
        /// <returns>The point translated into coordinates relative to the element.</returns>
        /// <exception cref="ArgumentNullException">If element is null</exception>
        /// <remarks>
        /// <para class="note"><b>Note:</b> In an XBAP application we don't have access rights to call PointFromScreen. In this situation we look at the refElement, if not null we get the common ancestor element and map the point from it. If refElement is null then we walk up the visual tree to the top level element and map the point from it.</para>
        /// </remarks>
        public static Point PointFromScreenSafe(Visual element, Point point, Visual refElement)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            // when we aren't in a browser just return the point in screen coordinates
			if (!BrowserInteropHelper.IsBrowserHosted)
			{
				// AS 9/11/09 TFS21329
				// This can generate an InvalidOperationException if the element is out of the 
				// visual tree. Unfortunately there is no "empty" point although we could create 
				// a point with infinity but the caller may not know to check this. For now we've
				// decided to return a negative point so it seems to be outside the calling element.
				//
				try
				{
					return element.PointFromScreen(point);
				}
				catch (InvalidOperationException)
				{
					return new Point(-1, -1);
				}
			}

            Visual commonAncestor = GetCommonAncestor(element, refElement);

            if (commonAncestor == null || commonAncestor == element)
                return point;

            FrameworkElement fe = element as FrameworkElement;
            FrameworkElement feAncestor = commonAncestor as FrameworkElement;

			if (fe != null && feAncestor != null)
			{
				// AS 12/3/07 BR28846
				// This is reversed. Translate relative to the ancestor.
				//
				//return fe.TranslatePoint(point, feAncestor);
				return feAncestor.TranslatePoint(point, fe);
			}

            GeneralTransform transform = element.TransformToAncestor(commonAncestor);

            Debug.Assert(transform != null);

            if (transform != null)
                transform.Transform(point);

            return point;
        }

		// AS 11/27/07 BR28724
		//private static Visual GetCommonAncestor(Visual element, Visual refElement)
        internal static Visual GetCommonAncestor(Visual element, Visual refElement)
        {

            Visual commonAncestor = null;

            if (refElement != null)
                commonAncestor = refElement.FindCommonVisualAncestor(element) as Visual;

            if (commonAncestor == null)
            {
                Visual ancestor = VisualTreeHelper.GetParent(element) as Visual;
				while (ancestor != null)
				{
					commonAncestor = ancestor;
					// AS 12/5/07 BR28814
					// We were using the PointToScreenSafe and PointFromScreenSafe utility methods but with 
					// elements from different root visuals. We need to get the ultimate root visual so we 
					// need to get the logical parent if there isn't a visual one.
					//
					//ancestor = VisualTreeHelper.GetParent(ancestor) as Visual;
                    // AS 3/30/09 TFS16355 - WinForms Interop
                    // We don't want to get the containing popup (i.e. logical parent)
                    // if it doesn't have a visual parent. If we did return it then any 
                    // translations of point result in 0,0.
                    //
					//ancestor = VisualTreeHelper.GetParent(ancestor) as Visual ?? LogicalTreeHelper.GetParent(ancestor) as Visual;
                    Visual parent = VisualTreeHelper.GetParent(ancestor) as Visual;

                    if (null == parent)
                    {
                        parent = LogicalTreeHelper.GetParent(ancestor) as Visual;

                        if (parent is Popup && VisualTreeHelper.GetParent(parent) == null)
						{
							// AS 11/2/10 TFS49402/TFS49912/TFS51985
							// If this is a toolwindow's popup then we can continue up the owner chain.
							//
                            //parent = null;
							ToolWindowHostPopup host = ToolWindowHostPopup.GetHost(parent as Popup);

							if ( null != host && host.Child is ToolWindow )
								parent = ((ToolWindow)host.Child).Owner;
							else
	                            parent = null;
						}
                    }

                    ancestor = parent;
				}
            }

            return commonAncestor;
        }

        #endregion //PointFromScreenSafe

        // JJD 11/06/07 - Added methods to help support XBAP semi-trust applications
        #region PointToScreenSafe

        /// <summary>
        /// Translates a point relative to an element into screen coordinates
        /// </summary>
        /// <param name="element">The specified element</param>
        /// <param name="point">The point in element coordinates.</param>
        /// <returns>The point translated into screen coordinates.</returns>
        /// <exception cref="ArgumentNullException">If element is null</exception>
        /// <remarks>
        /// <para class="note"><b>Note:</b> In an XBAP application we don't have access rights to call PointToScreen. In this situation we walk up the visual tree to the top level element and map the point to it.</para>
        /// </remarks>
        public static Point PointToScreenSafe(Visual element, Point point)
        {
            return PointToScreenSafe(element, point, null);
        }

        /// <summary>
        /// Translates a point relative to an element into screen coordinates
        /// </summary>
        /// <param name="element">The specified element</param>
        /// <param name="point">The point in element coordinates.</param>
        /// <param name="refElement">An optional second element to reference.</param>
        /// <returns>The point translated into screen coordinates.</returns>
        /// <exception cref="ArgumentNullException">If element is null</exception>
        /// <remarks>
        /// <para class="note"><b>Note:</b> In an XBAP application we don't have access rights to call PointToScreen. In this situation we look at the refElement, if not null we get the common ancestor element and map the point to it. If refElement is null then we walk up the visual tree to the top level element and map the point to it.</para>
        /// </remarks>
        public static Point PointToScreenSafe(Visual element, Point point, Visual refElement)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            // when we aren't in a browser just return the point in screen coordinates
            if (!BrowserInteropHelper.IsBrowserHosted)
                return element.PointToScreen(point);

            Visual commonAncestor = GetCommonAncestor(element, refElement);

            if (commonAncestor == null || commonAncestor == element)
                return point;

            FrameworkElement fe = element as FrameworkElement;
            FrameworkElement feAncestor = commonAncestor as FrameworkElement;

            if (fe != null && feAncestor != null)
                return fe.TranslatePoint(point, feAncestor);

            GeneralTransform transform = element.TransformToAncestor(commonAncestor);

            Debug.Assert(transform != null);

            if (transform != null)
                transform.Transform(point);

            return point;
        }

        #endregion //PointToScreenSafe

        // JJD 11/06/07 - Added methods to help support XBAP semi-trust applications
        #region GetWorkArea

        /// <summary>
        /// Returns the work area of the monitor relative to the top/left of the passed in element
        /// </summary>
        /// <param name="element">The specified element</param>
        /// <returns>The rect of the monitor or top level elment.</returns>
        /// <exception cref="ArgumentNullException">If element is null</exception>
        /// <remarks>
        /// <para class="note"><b>Note:</b> In an XBAP application we don't have access rights to get monitor information. In this situation we walk up the visual tree to the top level element and use its size.</para>
        /// </remarks>
        public static Rect GetWorkArea(Visual element)
        {
            return GetWorkArea(element, null);
        }

        /// <summary>
        /// Returns the work area of the monitor relative to the top/left of the passed in element
        /// </summary>
        /// <param name="element">The specified element</param>
        /// <param name="refElement">An optional second element to reference.</param>
        /// <returns>The rect of the monitor or top level elment.</returns>
        /// <exception cref="ArgumentNullException">If element is null</exception>
        /// <remarks>
        /// <para class="note"><b>Note:</b> In an XBAP application we don't have access rights to get monitor information. In this situation we look at the refElement, if not null we get the common ancestor element and use its size to construct the rect. If refElement is null then we walk up the visual tree to the top level element and use its size.</para>
        /// </remarks>
        public static Rect GetWorkArea(Visual element, Visual refElement)
        {
            return GetWorkArea(element, new Point(), refElement);
        }

        /// <summary>
        /// Returns the work area of the monitor relative to the top/left of the passed in element
        /// </summary>
        /// <param name="element">The specified element</param>
        /// <param name="point">The point in element coordinates.</param>
        /// <param name="refElement">An optional second element to reference.</param>
        /// <returns>The rect of the monitor or top level elment.</returns>
        /// <exception cref="ArgumentNullException">If element is null</exception>
        /// <remarks>
        /// <para class="note"><b>Note:</b> In an XBAP application we don't have access rights to get monitor information. In this situation we look at the refElement, if not null we get the common ancestor element and use its size to construct the rect. If refElement is null then we walk up the visual tree to the top level element and use its size.</para>
        /// </remarks>
        public static Rect GetWorkArea(Visual element, Point point, Visual refElement)
        {
            if (element == null)
                throw new ArgumentNullException("element");

            // when we aren't in a browser just return the point in screen coordinates
            if (!BrowserInteropHelper.IsBrowserHosted)
                return NativeWindowMethods.GetWorkArea(PointToScreenSafe(element, point, refElement));

            FrameworkElement fe = element as FrameworkElement;

            Debug.Assert(fe != null);

            Visual commonAncestor = GetCommonAncestor(element, refElement);

            if (commonAncestor == null || commonAncestor == element)
            {
                if ( fe != null )
                    return new Rect(fe.RenderSize);

                return new Rect();
            }

            // AS 7/24/08 
            // The ActualWidth/Height (and the RenderSize) of a Window include
            // the non-client area. We want to exclude that so I'm getting its
            // first child (this assumption may not be valid ergo the assert)
            // and using its RenderSize.
            //
            Window w = commonAncestor as Window;

            if (null != w)
            {
                Debug.Assert(VisualTreeHelper.GetChildrenCount(w) == 1);
                FrameworkElement rootChild = VisualTreeHelper.GetChildrenCount(w) > 0 ? VisualTreeHelper.GetChild(w, 0) as FrameworkElement : null;

                if (null != rootChild)
                    return new Rect(rootChild.RenderSize);
            }

            FrameworkElement feAncestor = commonAncestor as FrameworkElement;

            Debug.Assert(feAncestor != null);

            if (feAncestor != null)
                return new Rect(feAncestor.RenderSize);

            return new Rect();
        }

        #endregion //GetWorkArea

		// AS 12/7/07 RightToLeft
		#region RectFromPoints
		/// <summary>
		/// Helper method for returning a normalized <see cref="Rect"/> based on two points.
		/// </summary>
		/// <param name="point1">A point that represents a corner of the rect</param>
		/// <param name="point2">A point that represents the corner of the rect opposite that of <paramref name="point1"/></param>
		/// <returns>
		/// A normalized rect based on the specified points. For example, the <see cref="Rect.X"/> will be 
        /// smaller of the <see cref="Point.X"/> of the <paramref name="point1"/> and <paramref name="point2"/>.
		/// </returns>
        // AS 1/29/09 TFS13199
        // I need this same routine in the DataPresenter.
        //
		//internal static Rect RectFromPoints(Point point1, Point point2)
		public static Rect RectFromPoints(Point point1, Point point2)
		{
			double x = Math.Min(point1.X, point2.X);
			double y = Math.Min(point1.Y, point2.Y);
			double width = Math.Abs(point2.X - point1.X);
			double height = Math.Abs(point2.Y - point1.Y);

			return new Rect(x, y, width, height);
		} 
		#endregion //RectFromPoints

		// AS 5/7/08
		#region VerifyCanBeModified
		internal static void VerifyCanBeModified(ResourceDictionary dictionary)
		{
			if (dictionary.IsReadOnly)
                throw new InvalidOperationException(SR.GetString("LE_ResourceDictionaryReadOnly"));
		} 
		#endregion //VerifyCanBeModified

		// AS 5/7/08
		#region ThrowIfXXX
		/// <summary>
		/// Helper method to throw an exception if the specified enum is not valid for the enum type.
		/// </summary>
		/// <param name="parameter">The enum to evaluate. Note, this should not be a flagged enum.</param>
		/// <param name="parameterName">The name of the parameter. This is used in the exception if the parameter is not a valid enum member.</param>
		internal static void ThrowIfInvalidEnum(Enum parameter, string parameterName)
		{
			Type enumType = parameter.GetType();
			Debug.Assert(enumType.GetCustomAttributes(typeof(FlagsAttribute), false).Length == 0, "This should not be used with flagged enums");

			if (false == Enum.IsDefined(enumType, parameter))
			{
				throw new InvalidEnumArgumentException(parameterName, Convert.ToInt32(parameter), enumType);
			}
		}

		/// <summary>
		/// Helper method to throw an exception if the specified parameter is null.
		/// </summary>
		/// <param name="parameter">The parameter to evaluate</param>
		/// <param name="parameterName">The name of the parameter. This is used in the exception if the parameter is null</param>
		internal static void ThrowIfNull(object parameter, string parameterName)
		{
			if (null == parameter)
				throw new ArgumentNullException(parameterName);
		}
		/// <summary>
		/// Helper method to throw an exception if the specified parameter is null.
		/// </summary>
		/// <param name="parameter">The parameter to evaluate</param>
		/// <param name="parameterName">The name of the parameter. This is used in the exception if the parameter is null</param>
		internal static void ThrowIfNullOrEmpty(string parameter, string parameterName)
		{
			if (string.IsNullOrEmpty(parameter))
				throw new ArgumentNullException(parameterName);
		}
		#endregion //ThrowIfXXX

		#region IsNumericType

		// SSP 2/15/08
		// Moved from Editors Utils.
		// 
		/// <summary>
		/// Determines if a given System.Type is a numeric type.
		/// </summary>
		/// <param name="type">The System.Type to test.</param>
		/// <returns>True if the type is a numeric type.</returns>
		public static bool IsNumericType( System.Type type )
		{
			if ( type.IsPrimitive || type == typeof( decimal ) )
			{
				if ( type != typeof( bool ) && type != typeof( char ) )
					return true;
				else
					return false;
			}
			return false;
		}

		#endregion // IsNumericType

        // AS 10/1/08 TFS5939/BR32114
        #region SnapElementToDevicePixels

        /// <summary>
        /// SnapElementToDevicePixels Attached Dependency Property
        /// </summary>
        /// <remarks>
        /// <p class="body">The SnapsElementToDevicePixels is meant to be used on elements such as an Image that 
        /// does not currently support SnapsToDevicePixels.</p>
        /// <p class="note"><b>Note:</b> The RenderTransform of the element is used so you should not set the RenderTransform 
        /// on the same element on which you are setting SnapElementToDevicePixels to true. You may set the RenderTransform on 
        /// an ancestor element. Also, you should not set this property on an element that is an ancestor of one or more 
        /// elements that have this property set to true. In such a case the results are undefined since the child element's 
        /// RenderTransform could be calculated before that of the ancestor element. Since this property should be set on 
        /// discrete elements such as an Image, this scenario should not be required.</p>
        /// </remarks>
        public static readonly DependencyProperty SnapElementToDevicePixelsProperty =
            DependencyProperty.RegisterAttached("SnapElementToDevicePixels", typeof(bool), typeof(Utilities),
                new FrameworkPropertyMetadata(KnownBoxes.FalseBox,
					// AS 8/26/09 TFS21361
					// Since there is now another property that is the real property indicating if the 
					// functionality is enabled, we don't need this option on this version.
					//
                    //FrameworkPropertyMetadataOptions.AffectsRender,
                    new PropertyChangedCallback(OnSnapElementToDevicePixelsChanged)));

        /// <summary>
        /// Gets the SnapElementToDevicePixels property.  This dependency property 
        /// indicates how the element should be adjusted so that it renders along a device pixel boundary.
        /// </summary>
        /// <seealso cref="SnapElementToDevicePixelsProperty"/>
        public static bool GetSnapElementToDevicePixels(DependencyObject d)
        {
            return (bool)d.GetValue(SnapElementToDevicePixelsProperty);
        }

        /// <summary>
        /// Sets the SnapElementToDevicePixels property.  This dependency property 
        /// indicates how the element should be adjusted so that it renders along a device pixel boundary.
        /// </summary>
        /// <seealso cref="SnapElementToDevicePixelsProperty"/>
        public static void SetSnapElementToDevicePixels(DependencyObject d, bool value)
        {
            d.SetValue(SnapElementToDevicePixelsProperty, value);
        }

        /// <summary>
        /// Handles changes to the SnapElementToDevicePixels property.
        /// </summary>
        private static void OnSnapElementToDevicePixelsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement fe = d as FrameworkElement;

            if (null != fe)
            {
				// AS 8/26/09 TFS21361
				// When you hook/unhook the loaded/unloaded events, the WPF framework notifies 
				// the ancestor element so it can track whether there is a descendant with a 
				// loaded/unloaded handler since that has some bearing on whether the ancestor 
				// considers itself loaded. To get around this we will asynchronously set a 
				// different attached property that will actually perform the load/unload hook up.
				//
				if (e.Property == SnapElementToDevicePixelsProperty)
				{
					fe.Dispatcher.BeginInvoke(DispatcherPriority.Send, new DispatcherOperationCallback(OnSyncSnapElementActual), d);
					return;
				}

                RoutedEventHandler loadHandler = new RoutedEventHandler(OnSnappedElementLoadChanged);

                if (true.Equals(e.NewValue))
                {
                    if (true == fe.IsLoaded)
                    {
                        // if its loaded we want to know when its unloaded to remove
                        // it from the processing list
                        fe.Unloaded += loadHandler;
                        SnapInfo.Add(fe);
                    }
                    else
                    {
                        // wait for the element to be loaded...
                        fe.Loaded += loadHandler;
                    }
                }
                else
                {
                    // disconnect snapped element
                    fe.Loaded -= loadHandler;
                    fe.Unloaded -= loadHandler;
                    SnapInfo.Remove(fe);
                }
            }
        }

		// AS 8/26/09 TFS21361
		// Define a 2nd DP that we can use as the actual SnapElement property and we will
		// asynchronously keep this in sync with the SnapElementToDevicePixels property.
		//
		private static readonly DependencyProperty SnapElementToDevicePixelsActualProperty =
			DependencyProperty.RegisterAttached("SnapElementToDevicePixelsActual", typeof(bool), typeof(Utilities),
				new FrameworkPropertyMetadata(KnownBoxes.FalseBox,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    new PropertyChangedCallback(OnSnapElementToDevicePixelsChanged)));

		// AS 8/26/09 TFS21361
		private static object OnSyncSnapElementActual(object param)
		{
			DependencyObject d = param as DependencyObject;
			Debug.Assert(d != null);
			d.SetValue(SnapElementToDevicePixelsActualProperty, d.GetValue(SnapElementToDevicePixelsProperty));
			return null;
		}

        #region OnSnappedElementLoadChanged
        private static void OnSnappedElementLoadChanged(object sender, RoutedEventArgs e)
        {
            bool isLoaded = e.RoutedEvent == FrameworkElement.LoadedEvent;
            FrameworkElement fe = sender as FrameworkElement;

            Debug.Assert(null != fe);
            RoutedEventHandler loadHandler = new RoutedEventHandler(OnSnappedElementLoadChanged);

            if (isLoaded)
            {
                fe.Loaded -= loadHandler;
                fe.Unloaded += loadHandler;
                SnapInfo.Add(fe);
            }
            else
            {
                fe.Unloaded -= loadHandler;
                fe.Loaded += loadHandler;
                SnapInfo.Remove(fe);
            }
        }
        #endregion //OnSnappedElementLoadChanged

        #endregion //SnapElementToDevicePixels

        // AS 10/1/08 TFS5939/BR32114
        #region SnapInfo class
        private class SnapInfo
        {
            #region Member Variables

            private WeakReference _rootVisual;
            private FrameworkElement _rootLoadElement;
            private WeakList<FrameworkElement> _loadedElements = new WeakList<FrameworkElement>();
            private bool _compactPending;

            #endregion //Member Variables

            #region Constructor
            private SnapInfo(FrameworkElement rootLoadedElement)
            {
                if (null == rootLoadedElement)
                    throw new ArgumentNullException();

                this._rootLoadElement = rootLoadedElement;
            }
            #endregion //Constructor

            #region Properties

            #region Info

            /// <summary>
            /// Info Read-Only Dependency Property
            /// </summary>
            private static readonly DependencyPropertyKey InfoPropertyKey
                = DependencyProperty.RegisterAttachedReadOnly("Info", typeof(SnapInfo), typeof(SnapInfo),
                    new FrameworkPropertyMetadata((SnapInfo)null));

            internal static readonly DependencyProperty InfoProperty
                = InfoPropertyKey.DependencyProperty;

            private static SnapInfo GetInfo(DependencyObject d)
            {
                return (SnapInfo)d.GetValue(InfoProperty);
            }

            private static void SetInfo(DependencyObject d, SnapInfo value)
            {
                d.SetValue(InfoPropertyKey, value);
            }

            #endregion //Info

            #region Owner

            /// <summary>
            /// Owner Read-Only Dependency Property
            /// </summary>
            private static readonly DependencyPropertyKey OwnerPropertyKey
                = DependencyProperty.RegisterAttachedReadOnly("Owner", typeof(FrameworkElement), typeof(SnapInfo),
                    new FrameworkPropertyMetadata((FrameworkElement)null));

            internal static readonly DependencyProperty OwnerProperty
                = OwnerPropertyKey.DependencyProperty;

            private static FrameworkElement GetOwner(DependencyObject d)
            {
                return (FrameworkElement)d.GetValue(OwnerProperty);
            }

            private static void SetOwner(DependencyObject d, FrameworkElement value)
            {
                d.SetValue(OwnerPropertyKey, value);
            }

            #endregion //Owner

            #endregion //Properties

            #region Methods

            #region Private

            #region AddDescendant
            private void AddDescendant(FrameworkElement snapToElement)
            {
                this.VerifyCompacted();
                this._loadedElements.Add(snapToElement);

                if (this._loadedElements.Count == 1)
                {
                    DependencyObject rootLoad;
                    DependencyObject rootVisual = GetRootVisual(this._rootLoadElement, out rootLoad);
                    Debug.Assert(null != rootVisual);

                    if (null != rootVisual)
                    {
                        this._rootVisual = new WeakReference(rootVisual);
                        this._rootLoadElement.LayoutUpdated += new EventHandler(OnRootLayoutUpdated);
                    }
                    else
                        this._rootVisual = null;
                }

                snapToElement.RenderTransform = null;
            }
            #endregion //AddDescendant

            #region CompactList
            private void CompactList(object param)
            {
                this.VerifyCompacted();
            }
            #endregion //CompactList

			// AS 10/22/09 TFS23498
			#region GetDeviceMatrixes
			
#region Infragistics Source Cleanup (Region)
























































#endregion // Infragistics Source Cleanup (Region)

			private void GetDeviceMatrixes(Visual rootVisual, out Matrix toDeviceMatrix, out Matrix fromDeviceMatrix)
			{
				toDeviceMatrix = Utilities.GetDeviceMatrix(true, rootVisual);
				fromDeviceMatrix = Utilities.GetDeviceMatrix(false, rootVisual);
			}
			#endregion //GetDeviceMatrixes

			#region GetRootVisual
            private static DependencyObject GetRootVisual(DependencyObject descendant, out DependencyObject ancestorForLoaded)
            {
                DependencyObject ancestor = descendant;
                ancestorForLoaded = null;

                while (null != descendant)
                {
                    if (descendant is Page || descendant is Frame)
                        ancestorForLoaded = descendant;

                    ancestor = descendant;
                    descendant = VisualTreeHelper.GetParent(descendant);
                }

                if (null == ancestorForLoaded)
                    ancestorForLoaded = ancestor;

                return ancestor;
            }
            #endregion //GetRootVisual

            #region OnRootLayoutUpdated
            private void OnRootLayoutUpdated(object sender, EventArgs e)
            {
                Debug.Assert(null != this._rootVisual);

                if (null != this._rootVisual)
                {
                    Visual rootVisual = Infragistics.Windows.Utilities.GetWeakReferenceTargetSafe(this._rootVisual) as Visual;

                    if (null != rootVisual)
                    {
                        this.VerifyCompacted();

						// AS 10/22/09 TFS23498
						Matrix toDeviceTransform, fromDeviceTransform;
						this.GetDeviceMatrixes(rootVisual, out toDeviceTransform, out fromDeviceTransform);

						for (int i = 0, count = this._loadedElements.Count; i < count; i++)
                        {
                            FrameworkElement descendant = this._loadedElements[i];

                            if (null != descendant && rootVisual.IsAncestorOf(descendant))
                            {
                                Point offset = new Point();

                                try
                                {
									GeneralTransform gt = descendant.TransformToAncestor(rootVisual);

									// AS 1/6/10 TFS25834
									if (gt == null)
										continue;

                                    offset = gt.Transform(new Point());
                                }
                                catch (InvalidOperationException)
                                {
                                    // skip the element - its a weakreference so it should be
                                    // released if for some reason its not explicitly removed
                                    continue;
                                }

								// AS 10/22/09 TFS23498
								// We need to work with the Transform applied to the element.
								//
								//TranslateTransform tt = descendant.RenderTransform as TranslateTransform;
								//
								//// if there was a transform adjust the point to take that into account
								//if (null != tt)
								//{
								//    offset.X -= tt.X;
								//    offset.Y -= tt.Y;
								//}

                                #region Test Code for variations on calculating offset
                                
#region Infragistics Source Cleanup (Region)






























#endregion // Infragistics Source Cleanup (Region)

                                #endregion //Test Code for variations on calculating offset

								// AS 10/22/09 TFS23498
								// Make sure we're looking at device coordinates to see if we're on pixel boundaries
								//
								offset = toDeviceTransform.Transform(offset);

								double deltaX = Math.Round(offset.X) - offset.X;
								double deltaY = Math.Round(offset.Y) - offset.Y;

								// AS 10/22/09 TFS23498 [Start]
								bool changeX = !Utilities.AreClose(deltaX, 0);
								bool changeY = !Utilities.AreClose(deltaY, 0);

								// If the object is already on the device pixels then leave it alone.
								if (!changeX && !changeY)
									continue;

								Point originalOffset = offset;

								TranslateTransform tt = descendant.RenderTransform as TranslateTransform;
								Transform vt = VisualTreeHelper.GetTransform(descendant);

								// if its already got a render transform then we need to remove it and get the real offset
								if (null != tt && null != vt)
								{
									Point initialOffset = offset;

									try
									{
										offset = new Point();
										offset = vt.Inverse.Transform(offset);
										// AS 1/6/10 TFS25834
										//offset = descendant.TransformToAncestor(rootVisual).Transform(offset);
										GeneralTransform r2dTransform = descendant.TransformToAncestor(rootVisual);

										if (r2dTransform == null)
											continue;

										offset = r2dTransform.Transform(offset);
									}
									catch (InvalidOperationException)
									{
										// skip the element - its a weakreference so it should be
										// released if for some reason its not explicitly removed
										continue;
									}

									// Make sure we're looking at device coordinates.
									offset = toDeviceTransform.Transform(offset);
								}

								// round the offset so its on a device pixel boundary
								offset.X = Math.Round(offset.X);
								offset.Y = Math.Round(offset.Y);

								// transform the offset from device coordinates to that of the root visual
								offset = fromDeviceTransform.Transform(offset);

								// now transform them back to the descendants coordinate system
								// AS 1/6/10 TFS25834
								// There seems to be a bug in the wpf framework where while you may 
								// get a transform when you call TransformToAncestor, the call to 
								// TransformToDescendant may return null. Since we need the transform 
								// to do the mapping back to the descendants coordinates all we can 
								// do is skip this element. Note: using TransformToVisual doesn't help 
								// as in this case it just returns an identity transform.
								//
								//offset = rootVisual.TransformToDescendant(descendant).Transform(offset);
								GeneralTransform d2rTransform = rootVisual.TransformToDescendant(descendant);

								if (d2rTransform == null)
									continue;

								offset = d2rTransform.Transform(offset);

								// readjust for the inverted transform
								if (null != tt && null != vt)
								{
									offset = vt.Transform(offset);
								}

								deltaX = offset.X;
								deltaY = offset.Y;

								if (null != tt)
								{
									changeX = !Utilities.AreClose(deltaX, tt.X);
									changeY = !Utilities.AreClose(deltaY, tt.Y);
								}
								// AS 10/22/09 TFS23498 [End]

                                if (deltaX != 0 || deltaY != 0)
                                {
									// JM 05-19-10 TFS32481 
									//if (null == tt)
									if (null == tt || tt.IsSealed)
									{
                                        tt = new TranslateTransform(deltaX, deltaY);
										descendant.RenderTransform = tt;
									}
                                    else
                                    {
										// AS 10/22/09 TFS23498
										// Only set the X/Y if there was a change.
										//
										if (changeX)
	                                        tt.X = deltaX;

										if (changeY)
											tt.Y = deltaY;
                                    }
								}
                                else if (tt != null)
                                {
                                    // remove the transform
                                    descendant.RenderTransform = null;
                                }
                            }
                        }
                    }
                }
            }

            #endregion //OnRootLayoutUpdated

            #region RemoveDescendant
            private void RemoveDescendant(FrameworkElement snapToElement)
            {
                int index = this._loadedElements.IndexOf(snapToElement);

                // just empty the slot and we will lazily compact the list
                if (index >= 0)
                    this._loadedElements[index] = null;

                this._compactPending = true;
                this._rootLoadElement.Dispatcher.BeginInvoke(DispatcherPriority.Input, new SendOrPostCallback(CompactList), this._loadedElements);

                snapToElement.ClearValue(UIElement.RenderTransformProperty);
            }
            #endregion //RemoveDescendant

            #region VerifyCompacted
            private void VerifyCompacted()
            {
                if (this._compactPending)
                {
                    this._compactPending = false;

                    int oldCount = this._loadedElements.Count;
                    this._loadedElements.Compact();

                    if (oldCount > 0 && this._loadedElements.Count == 0)
                    {
                        this._rootLoadElement.LayoutUpdated -= new EventHandler(OnRootLayoutUpdated);
                    }
                }
            }
            #endregion //VerifyCompacted

            #endregion //Private

            #region Internal

            #region Add
            internal static void Add(FrameworkElement fe)
            {
                DependencyObject ancestorForLoad;
                DependencyObject rootVisual = SnapInfo.GetRootVisual(fe, out ancestorForLoad);

                Debug.Assert(ancestorForLoad is FrameworkElement);
                FrameworkElement feAncestorForLoad = ancestorForLoad as FrameworkElement;

                if (null != feAncestorForLoad)
                {
                    SnapInfo info = SnapInfo.GetInfo(ancestorForLoad);

                    if (null == info)
                    {
                        info = new SnapInfo(feAncestorForLoad);
                        SetInfo(ancestorForLoad, info);
                    }

                    info.AddDescendant(fe);
                    SetOwner(fe, feAncestorForLoad);
                }
            }
            #endregion //Add

            #region Remove
            internal static void Remove(FrameworkElement fe)
            {
                FrameworkElement owner = GetOwner(fe);

                if (null != owner)
                {
                    SnapInfo si = GetInfo(owner);
                    Debug.Assert(null != si);

                    fe.ClearValue(OwnerPropertyKey);
                    si.RemoveDescendant(fe);
                }
            }
            #endregion //Remove

            #endregion //Internal

            #endregion //Methods
        }
        #endregion //SnapInfo class

        // AS 10/23/08 TFS9546
        #region ShowMessageBox
        /// <summary>
        /// Helper method to display a messagebox using the Window of the specified element if available.
        /// </summary>
        /// <param name="element">The element whose Window should be used to show the MessageBox</param>
        /// <param name="messageBoxText">The text to display</param>
        /// <param name="caption">The title bar caption</param>
        /// <param name="button">Indicates which button(s) to display</param>
        /// <param name="icon">The icon to display</param>
        /// <returns>A MessageBoxResult that indicates which button was clicked to close the messagebox.</returns>
        public static MessageBoxResult ShowMessageBox(DependencyObject element, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            Window w = element != null ? Window.GetWindow(element) : null;

            if (null == w)
                return MessageBox.Show(messageBoxText, caption, button, icon);
            else
                return MessageBox.Show(w, messageBoxText, caption, button, icon);
        } 
        #endregion //ShowMessageBox

        // AS 12/11/08 TFS11518
        #region GetLineCount
        /// <summary>
        /// Returns the number of lines in the specified string.
        /// </summary>
        /// <param name="text">String whose line count is to be calculated</param>
        /// <returns>The number of lines based on the presence of "\r\n", "\r" and "\n" sequences within the specified text</returns>
        public static int GetLineCount(string text)
        {
            int count = 0;

            if (false == string.IsNullOrEmpty(text))
            {
                string[] arr = text.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                count = arr.Length;
            }

            return count;
        }
        #endregion //GetLineCount

        // AS 3/30/09 TFS16355 - WinForms Interop
        #region IsBrowserWindow
        internal static bool IsBrowserWindow(Window window)
        {
            if (BrowserInteropHelper.IsBrowserHosted)
            {
                if (window is NavigationWindow)
                {
                    if (window.GetType().Name == "RootBrowserWindow")
                        return true;
                }
            }

            return false;
        } 
        #endregion //IsBrowserWindow

        // JJD 2/23/10 - Tiles - added
        #region IsContainedBy

        internal static bool IsContainedBy(Rect containerRect, Rect rectToTest, bool checkWidth)
        {
            if (checkWidth)
            {
                return (containerRect.Left <= rectToTest.Left &&
                        containerRect.Right >= rectToTest.Right);
            }
            else
            {
                return (containerRect.Top <= rectToTest.Top &&
                        containerRect.Bottom >= rectToTest.Bottom);
            }
        }

        #endregion //IsContainedBy	
    
        // JJD 4/23/09 - TFS17037
        #region ValidateTargetTypeOfStyle

        /// <summary>
        /// Verifies that the TargetType of the style is either null or is set to the targetType, an ancestor type or a descendant type.
        /// </summary>
        /// <param name="style">The style in question (must not be null)</param>
        /// <param name="targetType">The expected target type (must not be null)</param>
        /// <param name="propertyName">The name of the property that is being set (must not be null). This is used for formatting an exception</param>
        /// <exception cref="ArgumentNullException">If either style, targtType or propertyName arguments are null.</exception>
        /// <exception cref="ArgumentException">If the TargetType of the style is incompatible with the passed in targetType.</exception>
        public static void ValidateTargetTypeOfStyle(Style style, Type targetType, string propertyName)
        {
            if (style == null)
                throw new ArgumentNullException("style");

            if (targetType == null)
                throw new ArgumentNullException("targetType");

            if (propertyName == null)
                throw new ArgumentNullException("propertyName");

            Type styleTargetType = style.TargetType;

            // the style is valid if it doesn't have a targettype or
            // its targettype is an ancestor or descendant of the
            // passed in type
            if (styleTargetType == null ||
                styleTargetType == targetType ||
                styleTargetType.IsAssignableFrom(targetType) ||
                targetType.IsAssignableFrom(styleTargetType))
                return;

            // if we are here then the targettype on the style is invalid
            throw new ArgumentException(SR.GetString("LE_TargetTypeInvalid", new object[] { propertyName, targetType.Name, styleTargetType.Name }), propertyName);

        }

        #endregion //ValidateTargetTypeOfStyle

		// AS 3/24/10 TFS27164
		// Refactored the existing to/from logical implementations so we only have one and we try to use 
		// the information on the hwndsource first. This specific issue happens because the SystemParameters 
		// information is cached and the information we were using was not updated by the time we asked 
		// for it in the ribbon.
		//
		#region GetDeviceMatrix
		private static bool _canGetHwndSource = true;

		internal static Matrix GetDeviceMatrix(bool toDevice, Visual relativeVisual)
		{
			if (_canGetHwndSource && null != relativeVisual)
			{
				try
				{
					return GetDeviceMatrixImpl(toDevice, relativeVisual);
				}
				catch (SecurityException)
				{
					_canGetHwndSource = false;
				}
			}

			return GetDeviceMatrixImpl(toDevice);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static Matrix GetDeviceMatrixImpl(bool toDevice, Visual relativeElement)
		{
			HwndSource hs = HwndSource.FromVisual(relativeElement) as HwndSource;

			if (null != hs)
			{
				if (toDevice)
					return hs.CompositionTarget.TransformToDevice;
				else
					return hs.CompositionTarget.TransformFromDevice;
			}

			return GetDeviceMatrixImpl(toDevice);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static Matrix GetDeviceMatrixImpl(bool toDevice)
		{
			Size pixelSize = NativeWindowMethods.PrimaryMonitorSize;
			double logicalX = SystemParameters.PrimaryScreenWidth;
			double logicalY = SystemParameters.PrimaryScreenHeight;

			if (toDevice)
				return new Matrix(pixelSize.Width / logicalX, 0, 0, pixelSize.Height / logicalY, 0, 0);
			else
				return new Matrix(logicalX / pixelSize.Width, 0, 0, logicalY / pixelSize.Height, 0, 0);
		} 
		#endregion //GetDeviceMatrix

		// AS 3/1/10 TFS28705
		#region RandomizeList
		internal static void RandomizeList<T>(IList<T> list)
		{
            int count = list.Count;

            if (count < 2)
                return;

			Random random = Rnd;

			for (int i = count - 1; i >= 0; i--)
			{
				int r = random.Next(0, i);
				T tmpObj = list[i];
				list[i] = list[r];
				list[r] = tmpObj;
			}
		}
		#endregion //RandomizeList

        // JJD 1/27/10 - TFS26749 - added
        #region SystemDoubleClickTime

        /// <summary>
        /// Returns the maximum number of milliseconds allowed between mouse button downs to generate a double click message.
        /// </summary>
        public static int SystemDoubleClickTime
        {
            get { return NativeWindowMethods.DoubleClickTime; }
        }

        #endregion //SystemDoubleClickTime	

        // JJD 1/27/10 - TFS26749 - added
        #region SystemDoubleClickSize

        /// <summary>
        /// Returns the maximum distance the cursor is allowed to travel between mouse button downs to generate a double click message.
        /// </summary>
        public static Size SystemDoubleClickSize
        {
            get { return NativeWindowMethods.DoubleClickSize; }
        }

        #endregion //SystemDoubleClickSize	

        // JJD 1/27/10 - TFS26749 - added
        #region SystemDragSize

        /// <summary>
        /// Returns the maximum distance the cursor is allowed to traval while the mouse button is down before initiating a drag operation.
        /// </summary>
        public static Size SystemDragSize
        {
            get { return NativeWindowMethods.DragSize; }
        }

        #endregion //SystemDrageSize	

		#region ValidateLicense
		internal static UltraLicense ValidateLicense(Type type, object instance)
		{
			try
			{
				return LicenseManager.Validate(type, instance) as UltraLicense;
			}
			catch (System.IO.FileNotFoundException)
			{
				return null;
			}
		}
		#endregion //ValidateLicense

		#region Combine
		internal static T[] Combine<T>(params IEnumerable<T>[] lists)
		{
			List<T> list = new List<T>();

			if (null != lists)
			{
				foreach (IList<T> item in lists)
				{
					if (null != item)
						list.AddRange(item);
				}
			}

			return list.ToArray();
		} 
		#endregion // Combine

        #region IMeetsCriteria Interface

        // JJD 12/14/09 - added to support tile dragging
        internal interface IMeetsCriteria
        {
            bool MeetsCriteria(object item);
        }

        #endregion // IMeetsCriteria Interface

        #region MethodDelegate

        // Optimization - only have 1 parameterless void delegate class defined.
        //
        internal delegate void MethodDelegate();

        #endregion //MethodDelegate

		// MD 8/12/10 - TFS26592
		// Moved this code from ValueEditor.LanguageCultureInfo so it could be used in other places.
		#region GetLanguageCultureInfo

		/// <summary>
		/// Gets the CultureInfo associated with the Language of the specified element.
		/// </summary>
		/// <param name="element">The element of which to get the associated CultureInfo.</param>
		/// <returns>The CultureInfo associated with the specified element.</returns>
		public static CultureInfo GetLanguageCultureInfo(FrameworkElement element)
		{
			// JJD 10/27/10 - TFS37193 
			// Call GetNonDefaultLanguage which will retutn null if the language wasn't explicitly set
			// somewhere in the ancestor chain or if the default value wan't changed from 'en-US'
			//XmlLanguage xmlLanguage = element.Language;
			XmlLanguage xmlLanguage = GetNonDefaultLanguage( element );
			if (null != xmlLanguage)
			{
				CultureInfo cultureInfo = null;
				bool wasLanguageMapped = false;

				// JJD 4/10/08
				// Added map to cache cultureinfo for each language
				if (g_LanguageCultureMap == null)
					g_LanguageCultureMap = new Dictionary<XmlLanguage, CultureInfo>();
				else
				{
					if (g_LanguageCultureMap.TryGetValue(xmlLanguage, out cultureInfo))
						wasLanguageMapped = true;
				}

				if (!wasLanguageMapped)
				{
					// JJD 4/15/08
					// Call the new GetNonNeutralCulture method instead which will return a 
					// culture that is non-neutral and can be used in formatting operations
					// without throwing an exception.
					//cultureInfo = xmlLanguage.GetEquivalentCulture();
					cultureInfo = Utilities.GetNonNeutralCulture(element);

					// map the lamguage regardless of whether GetEquivalentCulture succeeded
					g_LanguageCultureMap.Add(xmlLanguage, cultureInfo);
				}

				if (cultureInfo != null)
					return cultureInfo;
			}

			// use the current thread's ui culture as a fallback
			// SSP 7/9/08 BR34636
			// Use the CurrentCulture instead of CurrentUICulture. That's what we've been using
			// in the past.
			// 
			//return Thread.CurrentThread.CurrentUICulture;
			// JJD 10/27/10 - TFS37193 
			// Make sure we have a valid culture
			//return System.Globalization.CultureInfo.CurrentCulture;
			// If we don't have a culture then fallback to the current culture
			CultureInfo culture = System.Globalization.CultureInfo.CurrentCulture;

			if (culture == null)
				culture = CultureInfo.InvariantCulture;

			return GetNonNeutralCulture(culture);
		}

		#endregion // GetLanguageCultureInfo

		// AS 10/19/10 TFS42668
		#region SetFocusedElement
		internal static void SetFocusedElement(DependencyObject focusScope, IInputElement focusedElement)
		{
			// AS 10/8/08 TFS8629
			// I found this while debugging TFS8629. Basically setting the focused
			// element may change the keyboard focus as well. We'll prevent this by
			// handling the PreviewLostKeyboardFocus event
			KeyboardFocusChangedEventHandler previewHandler = delegate(object s, KeyboardFocusChangedEventArgs se)
			{
				if (se.NewFocus == focusedElement)
					se.Handled = true;
			};

			Keyboard.AddPreviewGotKeyboardFocusHandler(focusScope, previewHandler);
			Keyboard.AddPreviewLostKeyboardFocusHandler(focusScope, previewHandler);

			try
			{
				FocusManager.SetFocusedElement(focusScope, focusedElement);
			}
			finally
			{
				Keyboard.RemovePreviewGotKeyboardFocusHandler(focusScope, previewHandler);
				Keyboard.RemovePreviewLostKeyboardFocusHandler(focusScope, previewHandler);
			}
		}
		#endregion //SetFocusedElement

		// AS 1/20/11 Optimization
		#region IsUIElementOrUIElement3D
		internal static bool IsUIElementOrUIElement3D(DependencyObject d)
		{
			if (d is UIElement)
				return true;

			if (null != d && null != UIElement3DType && UIElement3DType.IsAssignableFrom(d.GetType()))
				return true;

			return false;
		}
		#endregion //IsUIElementOrUIElement3D

		// AS 8/4/11 TFS83465/TFS83469
		// The Rect.Intersects with assumes that the right/bottom is inclusive so 
		// adjacent rects are considered as intersecting.
		// e.g. the following would return true
		// new Rect(0,0,1,1).IntersectsWith(new Rect(1,0,1,1));
		//
		#region Intersect
		internal static Rect Intersect(Rect rect1, Rect rect2)
		{
			Rect intersection = Rect.Intersect(rect1, rect2);

			if (intersection.IsEmpty)
				return Rect.Empty;

			if (intersection.Y == rect1.Bottom || intersection.Y == rect2.Bottom)
				return Rect.Empty;

			if (intersection.X == rect1.Right || intersection.X == rect2.Right)
				return Rect.Empty;

			return intersection;
		}
		#endregion //Intersect

		// AS 8/4/11 TFS83465/TFS83469
		#region CalculateOnScreenLocation
		internal static Point? CalculateOnScreenLocation(Rect logicalScreenRect, Visual relativeElement, bool fullyInView)
		{
			Rect workingArea;
			Rect screenRect = ConvertFromLogicalPixels(logicalScreenRect, relativeElement);

			if (BrowserInteropHelper.IsBrowserHosted)
			{
				workingArea = GetWorkArea(relativeElement);
			}
			else
			{
				workingArea = ConvertToLogicalPixels(NativeWindowMethods.GetWorkArea(screenRect), relativeElement);
			}

			// if at least part of it is in view in the working area then we don't need to do anything
			Rect intersection = Utilities.Intersect(workingArea, logicalScreenRect);

			if (fullyInView && intersection == logicalScreenRect)
				return null;
			else if (!fullyInView && !intersection.IsEmpty)
				return null;

			// if we're not hosted in the browser then make sure we're not in another screen
			Rect intersectingScreenRects;

			if (BrowserInteropHelper.IsBrowserHosted)
			{
				// just a single screen...
				intersectingScreenRects = workingArea;
			}
			else
			{
				// we may need the virtual screen (at least the portion with which the window intersects)
				intersectingScreenRects = NativeWindowMethods.GetWorkArea(logicalScreenRect.TopLeft);
				intersectingScreenRects.Union(NativeWindowMethods.GetWorkArea(logicalScreenRect.TopRight));
				intersectingScreenRects.Union(NativeWindowMethods.GetWorkArea(logicalScreenRect.BottomLeft));
				intersectingScreenRects.Union(NativeWindowMethods.GetWorkArea(logicalScreenRect.BottomRight));

				// it is possible that the screen with the most doesn't have any of it in the working 
				// area but another portion is in a working area so we'll also bail if that is the case
				if (!fullyInView && !Utilities.Intersect(screenRect, intersectingScreenRects).IsEmpty)
					return null;
				
				// convert this to logical coordinates
				intersectingScreenRects = Utilities.ConvertToLogicalPixels(intersectingScreenRects, relativeElement);
			}

			var location = logicalScreenRect.Location;

			if (logicalScreenRect.Right < workingArea.Left || (fullyInView && logicalScreenRect.Width < workingArea.Width && logicalScreenRect.Left < workingArea.Left))
				location.X = workingArea.Left;
			else if (logicalScreenRect.Left >= workingArea.Right || (fullyInView && logicalScreenRect.Width < workingArea.Width && logicalScreenRect.Right > workingArea.Right))
				location.X = (workingArea.Right - 1) - logicalScreenRect.Width;
			else if (fullyInView && intersection.Width != logicalScreenRect.Width)
			{
				// we want the window to be fully in view but it's only partially in view. we'll use the 
				// unioned screen rects in the case that the window is wider than the screen
				if (logicalScreenRect.Left < intersectingScreenRects.Left)
					location.X = intersectingScreenRects.Left;
				else if (logicalScreenRect.Right > intersectingScreenRects.Right)
					location.X = (intersectingScreenRects.Right - 1) - logicalScreenRect.Width;
			}

			if (logicalScreenRect.Bottom < workingArea.Top || (fullyInView && logicalScreenRect.Height < workingArea.Height && logicalScreenRect.Top < workingArea.Top))
				location.Y = workingArea.Top;
			else if (logicalScreenRect.Top >= workingArea.Bottom || (fullyInView && logicalScreenRect.Height < workingArea.Height && logicalScreenRect.Bottom > workingArea.Bottom))
				location.Y = (workingArea.Bottom - 1) - logicalScreenRect.Height;
			else if (fullyInView && intersection.Height != logicalScreenRect.Height)
			{
				// we want the window to be fully in view but it's only partially in view. we'll use the 
				// unioned screen rects in the case that the window is wider than the screen
				if (logicalScreenRect.Top < intersectingScreenRects.Top)
					location.Y = intersectingScreenRects.Top;
				else if (logicalScreenRect.Bottom > intersectingScreenRects.Bottom)
					location.Y = (intersectingScreenRects.Bottom - 1) - logicalScreenRect.Height;
			}

			return location;
		} 
		#endregion //CalculateOnScreenLocation

		// JM 03-24-11 TFS68833 Added.
		#region EnableModelessKeyboardInterop
		/// <summary>
		/// This method should be called if your application is a WindowsForms application that will display an Infragistics WPF control inside an ElementHost control.
		/// </summary>
		/// <remarks>
		/// This method should be called to ensure proper keyboard processing for controls contained in modeless windows that may be displayed
		/// by Infragistics WPF controls.
		/// </remarks>
		public static void EnableModelessKeyboardInterop()
		{
			Utilities._modelessKeyboardInteropEnabled = true;
		}
		#endregion //EnableModelessKeyboardInterop

		// JM 03-24-11 TFS68833 Added.
		#region EnableModelessKeyboardInteropForWindow
		internal static void EnableModelessKeyboardInteropForWindow(Window window)
		{
			if (false == Utilities._modelessKeyboardInteropEnabled)
				return;

			try
			{
				EnableModelessKeyboardInteropForWindowHelper(window);
			}
			catch { }

		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static void EnableModelessKeyboardInteropForWindowHelper(Window window)
		{
			// Construct the full assembly name of the WindowsFormsIntegration assembly for the .NET version we are currently running on.
			// Do this by getting the Full Name of an assembly that contains a referenced type and extracting the portion of that full 
			// name that contains the version, culture and publickeytoken, and prepending it with 'WindowsFormsIntegration'.
			string	knownAssemblyFullName				= typeof(DependencyObject).Assembly.FullName;
			int		startOfVersionPart					= knownAssemblyFullName.IndexOf("Version=");
			string	windowsFormsIntegrationAssemblyName	= "WindowsFormsIntegration, " + knownAssemblyFullName.Substring(startOfVersionPart);

			// Walk over the loaded assemblies and see if the WindowsFormsIntegration assembly is loaded.  If so, call the 
			// EnableModelessKeyboardInterop static method on the ElementHost class. 
			// We are accepting the fact that even though the WindowsFormsIntegration assembly is loaded, the window may not
			// actually be inside of an ElementHost control, but there is no surefire way to determine if it is so.
			var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (System.Reflection.Assembly assembly in loadedAssemblies)
			{
				if (assembly.FullName == windowsFormsIntegrationAssemblyName)
				{
					Type elementHostType = assembly.GetType("System.Windows.Forms.Integration.ElementHost");
					if (null != elementHostType)
					{
						System.Reflection.MethodInfo method = elementHostType.GetMethod("EnableModelessKeyboardInterop");
						if (null != method)
							method.Invoke(null, new object[] { window });
					}

					break;
				}
			}
		}
		#endregion //EnableModelessKeyboardInteropForWindow
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