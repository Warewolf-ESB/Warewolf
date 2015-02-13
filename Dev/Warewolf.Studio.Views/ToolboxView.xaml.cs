using System.Windows.Controls;
using Dev2.Common.Interfaces;

namespace Warewolf.Studio.Views
{
	/// <summary>
	/// Interaction logic for ToolboxView.xaml
	/// </summary>
	public partial class ToolboxView : UserControl, IToolboxView
	{
		public ToolboxView()
		{
			InitializeComponent();
		}

	    #region Implementation of IWarewolfView

	    public void Blur()
	    {
	    }

	    public void UnBlur()
	    {
	    }

	    #endregion
	}
}