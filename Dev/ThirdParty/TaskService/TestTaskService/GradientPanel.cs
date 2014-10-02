
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TestTaskService
{
	internal class GradientPanel : Panel
	{
		public GradientPanel()
		{
			this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ContainerControl | ControlStyles.ResizeRedraw, true);
		}

		// Methods
		protected override void OnPaint(PaintEventArgs e)
		{
			Brush brush = new LinearGradientBrush(base.Bounds, SystemColors.Control, SystemColors.ControlDark, LinearGradientMode.Vertical);
			e.Graphics.FillRectangle(brush, base.Bounds);
			e.Graphics.DrawRectangle(SystemPens.WindowFrame, new Rectangle(base.Bounds.X, base.Bounds.Y, base.Width - 1, base.Height - 1));
		}
	}
}
