using System.Collections.ObjectModel;

namespace Dev2.Studio.Dock
{
	/// <summary>
	/// Collection of <see cref="ItemBinding"/> instances
	/// </summary>
	public class ItemBindingCollection : ObservableCollection<ItemBinding>
	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="ItemBindingCollection"/>
		/// </summary>
		public ItemBindingCollection()
		{
		}
		#endregion //Constructor
	}
}
