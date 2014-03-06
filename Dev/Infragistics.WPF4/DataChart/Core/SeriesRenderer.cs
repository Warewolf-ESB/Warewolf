using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Infragistics.Controls.Charts
{
    internal class SeriesRenderingArguments
    {
        public SeriesRenderingArguments(Series series, Rect viewport, Rect window, bool animate)
        {
            TransitionDuration = series.TransitionDuration;
            Container = series;
            Viewport = viewport;
            Window = window;
            Animate = animate;
        }
        public Rect Viewport { get; set; }
        public Rect Window { get; set; }



        public TimeSpan TransitionDuration { get; set; }

        public bool Animate { get; set; }
        public FrameworkElement Container { get; set; }
    }

    internal class SeriesRenderer<TFrame, TView>
    {
        internal Action<TFrame, TView> PrepareFrame { get; set; }
        internal Action<TFrame, TView> RenderFrame { get; set; }
        internal Action<TFrame> CalculateBuckets { get; set; }
        internal Action StartupAnimation { get; set; }
        internal Func<bool> AnimationActive { get; set; }

        public SeriesRenderer(Action<TFrame, TView> prepare, Action<TFrame, TView> render, Func<bool> animationActive, Action startupAnimation)
        {
            PrepareFrame = prepare;
            RenderFrame = render;
            CalculateBuckets = (f) => { };
            AnimationActive = animationActive;
            StartupAnimation = startupAnimation;
        }

        public SeriesRenderer(Action<TFrame, TView> prepare, Action<TFrame, TView> render, Func<bool> animationActive, Action startupAnimation, Action<TFrame> calculateBuckets)
        {
            PrepareFrame = prepare;
            RenderFrame = render;
            CalculateBuckets = calculateBuckets;
            AnimationActive = animationActive;
            StartupAnimation = startupAnimation;
        }

        public void Render( SeriesRenderingArguments arguments,
            ref TFrame previousFrame, ref TFrame currentFrame, ref TFrame transitionFrame, TView view)
        {
            int totalMilliseconds = 0;



            totalMilliseconds = (int)arguments.TransitionDuration.TotalMilliseconds;


            if (arguments.Animate && 
                totalMilliseconds > 0)
            {
                TFrame prevFrame = previousFrame;

                if (AnimationActive())
                {
                    previousFrame = transitionFrame;
                    transitionFrame = prevFrame;
                }
                else
                {
                    previousFrame = currentFrame;
                    currentFrame = prevFrame;
                }

                CalculateBuckets(currentFrame);
                PrepareFrame(currentFrame, view);

                StartupAnimation();
            }
            else
            {
                CalculateBuckets(currentFrame);
                PrepareFrame(currentFrame, view);
                RenderFrame(currentFrame, view);
            }
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