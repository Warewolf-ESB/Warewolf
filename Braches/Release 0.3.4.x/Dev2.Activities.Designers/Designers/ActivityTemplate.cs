using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Dev2.Activities.Designers
{
    public abstract class ActivityTemplate : UserControl
    {
        public bool HideHelpContent { get; set; }
    }
}
