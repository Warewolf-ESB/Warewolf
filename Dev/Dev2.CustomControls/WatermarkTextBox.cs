using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Dev2.CustomControls
{
    class WatermarkTextBox : TextBox
    {
        public WatermarkTextBox()
        {
            this.DefaultStyleKey = typeof (WatermarkTextBox);
        }
    }
}
