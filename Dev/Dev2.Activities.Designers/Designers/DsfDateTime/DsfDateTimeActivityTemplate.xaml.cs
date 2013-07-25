using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Dev2.Activities.Designers.DsfDateTime
{
    /// <summary>
    /// Interaction logic for DsfDateTimeActivityTemplate.xaml
    /// </summary>
    public partial class DsfDateTimeActivityTemplate : IActivityTemplate
    {
        public DsfDateTimeActivityTemplate()
        {
            InitializeComponent();
        }
    }

    public interface IActivityTemplate
    {
    }
}
