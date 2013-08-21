using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Dev2.Activities.Adorners
{
    /// <summary>
    /// Used as a wrapper for adorner options that only need to execute a command
    /// IE these adorners have no content
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <date>2013/07/24</date>
    public class CommandAdornerPresenter : AdornerPresenterBase
    {
        public static ICommand GetCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(CommandProperty);
        }

        public static void SetCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(CommandProperty, value);
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command", typeof(ICommand),
            typeof(CommandAdornerPresenter), new PropertyMetadata(null));

        public override ButtonBase Button
        {
            get { throw new System.NotImplementedException(); }
        }
    }
}
