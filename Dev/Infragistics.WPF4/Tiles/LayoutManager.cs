using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Diagnostics;
using System.Windows;
using System.Globalization;
using System.ComponentModel;
using System.Windows.Controls;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Tiles.Events;
using Infragistics.Shared;
using System.Collections;
using System.Windows.Media;
using Infragistics.Collections;

namespace Infragistics.Windows.Tiles
{
	/// <summary>
	/// Helper class for managing loading and saving a layout file.
	/// </summary>
	internal static class LayoutManager
	{
		#region Constants

		// tags
		private const string RootTag = "xamTilesControl";
		private const string ItemsTag = "items";
		private const string ItemTag = "item";

		// attribs
		private const string LayoutVersionAttrib = "layoutVersion";
		private const string AssemblyVersionAttrib = "assemblyVersion";
		private const string IsExplicitLayoutAttrib = "isExplicitLayout";
        private const string MinimizedAreaExtentXAttrib = "MinimizedAreaExtentX";
        private const string MinimizedAreaExtentYAttrib = "MinimizedAreaExtentY";
        private const string SynchronizedItemWidthAttrib = "SynchronizedItemWidth";
        private const string SynchronizedItemHeightAttrib = "SynchronizedItemHeight";

        // item attributes
		private const string SerializationIdAttrib = "serializationId";
        private const string IsExpandedWhenMinimizedAttrib = "isExpandedWhenMinimized";
		private const string IsMaximizedAttrib = "isMaximized";
		private const string IsClosedAttrib = "isClosed";
		private const string LogicalIndexAttrib = "logicalIndex";
		private const string MaximizedIndexAttrib = "maximizedIndex";
		private const string RowAttrib = "row";
		private const string RowSpanAttrib = "rowSpan";
		private const string RowWeightAttrib = "rowWeight";
		private const string ColumnAttrib = "column";
		private const string ColumnSpanAttrib = "columnSpan";
		private const string ColumnWeightAttrib = "columnWeight";
        private const string PreferredWidthOverrideAttrib = "PreferredWidthOverride";
        private const string PreferredHeightOverrideAttrib = "PreferredHeightOverride";

        private const int LAYOUT_VERSION = 1;

		#endregion //Constants

		#region Member Variables
		#endregion // Member Variables

		#region Constructor
		static LayoutManager()
		{
		} 
		#endregion //Constructor

		#region Public Methods

		#region LoadLayout

		public static void LoadLayout(XamTilesControl tilesControl, String layout)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				StreamWriter sw = new StreamWriter(ms);
				sw.Write(layout);
				sw.Flush();
				ms.Position = 0;

				LoadLayout(tilesControl, ms);
			}
		}

		public static void LoadLayout(XamTilesControl tilesControl, Stream stream)
		{
			if (tilesControl.IsLoadingLayout)
				throw new InvalidOperationException(XamTilesControl.GetString("LE_LoadLayoutInProgress"));

            ObservableCollectionExtended<object> maximizedItems;

            TilesPanel panel = tilesControl.Panel;

            if (panel != null )
                maximizedItems = panel.MaximizedItemsInternal;
            else
                maximizedItems = tilesControl.MaximizedItemsInternal;

            List<ItemInfo> existingMaximizedItems   = new List<ItemInfo>();

            foreach (object item in maximizedItems)
            {
                ItemInfo info = tilesControl.GetItemInfo(item);

                
                // Make sure an ItemInfo object was returned
                if (info != null)
                    existingMaximizedItems.Add(info);
            }

            List<ItemInfo> newMaximizedItems        = new List<ItemInfo>();
 
            // call BeginUpdate so we don't respond to notifications during the load process
//            maximizedItems.BeginUpdate();

			tilesControl.IsLoadingLayout = true;

            try
			{
				// note, there seems to be a bug in the xmltextreader where
				// an exception regarding not finding the root element
				// is thrown because the data is not loaded. i came across
				// this when loading in info from a filestream. loading from 
				// a manifest resource stream worked fine though - presumably 
				// because the bits were already loaded. to get around this, 
				// we need to use a bufferedstream but since we don't know the 
				// source of the stream, we will always use it
				using (BufferedStream bufferedStream = new BufferedStream(stream))
				{
					XmlDocument document = new XmlDocument();
					document.Load(bufferedStream);

					#region Prepare

					XmlNode rootNode = document.SelectSingleNode(RootTag);

					if (rootNode == null)
						throw new InvalidOperationException(XamTilesControl.GetString("LE_LoadLayoutInvalidRootElement", RootTag));

					// load the version
					Version assemblyVersion = new Version(rootNode.Attributes[AssemblyVersionAttrib].Value);
					int layoutVersion = 1;
                    
                    int.TryParse(rootNode.Attributes[LayoutVersionAttrib].Value, out layoutVersion);

                    bool isExplicitLayout = ReadAttribute(rootNode, IsExplicitLayoutAttrib, false);

                    tilesControl.MinimizedAreaExplicitExtentX = ReadAttribute(rootNode, MinimizedAreaExtentXAttrib, 0d);
                    tilesControl.MinimizedAreaExplicitExtentY = ReadAttribute(rootNode, MinimizedAreaExtentYAttrib, 0d);

                    double synchronizedItemWidth = ReadAttribute(rootNode, SynchronizedItemWidthAttrib, 0d);
                    double synchronizedItemHeight = ReadAttribute(rootNode, SynchronizedItemHeightAttrib, 0d);

                    Size? synchronizedItemSize = null;
                    if (synchronizedItemWidth > 0 ||
                         synchronizedItemHeight > 0)
                        synchronizedItemSize = new Size(synchronizedItemWidth, synchronizedItemHeight);

                    tilesControl.Manager.SynchronizedItemSize = synchronizedItemSize;

					#endregion //Prepare

					#region Items

					// process items
					XmlNode itemsNode = rootNode.SelectSingleNode(ItemsTag);

                    foreach (XmlNode itemNode in itemsNode.SelectNodes(ItemTag))
                    {
                        // prepare the item
                        string serializationId = ReadAttribute(itemNode, SerializationIdAttrib, null);

                        LoadingItemMappingEventArgs args = new LoadingItemMappingEventArgs(tilesControl, serializationId);

                        tilesControl.RaiseLoadingItemMapping(args);
                        object item = args.Item;

                        if (item == null)
                            continue;

                        ItemInfo info = tilesControl.GetItemInfo(item);

                        if (info == null)
                            continue;

                        ItemSerializationInfo serializationInfo = new ItemSerializationInfo();

                        info.IsClosed                   = ReadAttribute(itemNode, IsClosedAttrib, false);
                        info.IsExpandedWhenMinimized    = ReadNullableBoolAttribute(itemNode, IsExpandedWhenMinimizedAttrib);

                        
                        
                        
                        
                        bool wasSerializedAsMaximized   = ReadAttribute(itemNode, IsMaximizedAttrib, false);

                        
                        
                        
                        if (wasSerializedAsMaximized)
                        {
                            // add it to the new list of maximized items
                            newMaximizedItems.Add(info);

                            
#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)


                            // if it is in the existing list then remove it
                            int oldIndex = existingMaximizedItems.IndexOf(info);

                            if (oldIndex >= 0)
                                existingMaximizedItems.RemoveAt(oldIndex);
                        }

                        serializationInfo.Item              = item;
                        serializationInfo.SerializationId   = serializationId;
                        serializationInfo.MaximizedIndex    = ReadAttribute(itemNode, MaximizedIndexAttrib, -1);
                        serializationInfo.LogicalIndex      = ReadAttribute(itemNode, LogicalIndexAttrib, -1);

                        if (isExplicitLayout)
                        {
                            Tile tile = tilesControl.TileFromItem(item);
                            DependencyObject dpo = item as DependencyObject;

                            ReadRowColumnAttribute(itemNode, serializationInfo, TilesPanel.ColumnProperty,          ColumnAttrib,       tile, dpo);
                            ReadRowColumnAttribute(itemNode, serializationInfo, TilesPanel.ColumnSpanProperty,      ColumnSpanAttrib,   tile, dpo);
                            ReadRowColumnAttribute(itemNode, serializationInfo, TilesPanel.ColumnWeightProperty,    ColumnWeightAttrib, tile, dpo);
                            ReadRowColumnAttribute(itemNode, serializationInfo, TilesPanel.RowProperty,             RowAttrib,          tile, dpo);
                            ReadRowColumnAttribute(itemNode, serializationInfo, TilesPanel.RowSpanProperty,         RowSpanAttrib,      tile, dpo);
                            ReadRowColumnAttribute(itemNode, serializationInfo, TilesPanel.RowWeightProperty,       RowWeightAttrib,    tile, dpo);
                        }

                        info.SerializationInfo = serializationInfo;

                        double preferredWidthOverride = ReadAttribute(itemNode, PreferredWidthOverrideAttrib, 0d);
                        double preferredHeightOverride = ReadAttribute(itemNode, PreferredHeightOverrideAttrib, 0d);

                        Size? sizeOverride = null;
                        if (preferredWidthOverride > 0 ||
                             preferredHeightOverride > 0)
                            sizeOverride = new Size(preferredWidthOverride, preferredHeightOverride);

                        info.SizeOverride = sizeOverride;
                    }
				}

				#endregion //Items

                // un-maximize any existing maximized guys that are left
                foreach (ItemInfo info in existingMaximizedItems)
                {
                    info.IsMaximized = false;

                    if (panel != null)
                    {
                        Tile tile = tilesControl.TileFromItem(info.Item);

                        if (tile != null)
                            tile.SynchStateFromInfo(info);
                        
                        panel.BumpLayoutVersion();
                    }
                }

                maximizedItems.BeginUpdate();

                // clear the maximized items collection
                maximizedItems.Clear();

                // sort the new maximized items into the proper order
                if ( newMaximizedItems.Count > 1 )
                    newMaximizedItems.Sort(new MaximizedItemInfoComparer());

                
                // Keep a count so we don't exceed the MaximizedTileLimit
                int limit = tilesControl.MaximizedTileLimit;
                int countOfNewMaximizedItems = 0;

                // add in the new maximized items which will ensure that
                // they are in the proper order
                foreach (ItemInfo info in newMaximizedItems)
                {
					// JJD 4/22/11 - TFS58637
					// If the item is closed then just set the IsMaximized flag and return
					if (info.IsClosed)
					{
                        info.IsMaximized = true;
						continue;
					}

                    
                    // See if we have reached the MaximizedTileLimit
                    if (countOfNewMaximizedItems == limit)
                    {
                        // If the item was maximized then set its IsMaxmized
                        // property to false andfall thru to sync up the 
                        // tiles state below.
                        // Otherwise, just continue
                        if (info.IsMaximized)
                            info.IsMaximized = false;
                        else
                            continue;
                    }
                    else
                    {
                        // add the item to the collection
                        maximizedItems.Add(info.Item);
                        
                        
                        // set its IsMaxmized state to true
                        info.IsMaximized = true;

                        
                        // Bump the count 
						countOfNewMaximizedItems++;
                    }
                    
                    
                    // Synchronize the Tile's state from the info object
                    if (panel != null)
                    {
                        Tile tile = tilesControl.TileFromItem(info.Item);

                        if (tile != null)
                            tile.SynchStateFromInfo(info);
                    }

                }

			}
			finally
			{
				tilesControl.IsLoadingLayout = false;

                // call EndUpdate on the maximized items collection so a notification is sent out
                maximizedItems.EndUpdate();
               
                tilesControl.Manager.SortItems(new ItemInfoComparer());

                if (panel != null)
                {
                    panel.SetValue(TilesPanel.IsInMaximizedModePropertyKey, KnownBoxes.FromValue(maximizedItems.Count > 0));

                    Debug.Assert(panel.IsInMaximizedMode == tilesControl.IsInMaximizedMode, "Maximized mode out of sync");
                    Debug.Assert(panel.MaximizedItems.Count == tilesControl.MaximizedItems.Count, "Maximized item count out of sync");

                    
                    // Bump the version to sync up all tile states
                    panel.BumpLayoutVersion();

                    panel.InvalidateMeasure();
                 }
    
             }
		}
		#endregion //LoadLayout

		#region SaveLayout

		public static string SaveLayout(XamTilesControl tilesControl)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				SaveLayout(tilesControl, ms);

				ms.Position = 0;
				StreamReader sr = new StreamReader(ms);
				return sr.ReadToEnd();
			}
		}

		public static void SaveLayout(XamTilesControl tilesControl, Stream stream)
		{
			XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8);
			writer.Formatting = Formatting.Indented;

			writer.WriteStartDocument();
			writer.WriteStartElement(RootTag); // <xamTilesControl>
			writer.WriteAttributeString(AssemblyVersionAttrib, AssemblyVersion.Version);
			WriteAttribute(writer, LayoutVersionAttrib, LAYOUT_VERSION);

            bool isExplicitLayout = false;
            NormalModeSettings settings = tilesControl.NormalModeSettings;

            if (settings != null && settings.TileLayoutOrder == TileLayoutOrder.UseExplicitRowColumnOnTile)
            {
                isExplicitLayout = true;
                WriteAttribute(writer, IsExplicitLayoutAttrib, true);
            }

            double minimizedAreaExtentX = tilesControl.MinimizedAreaExplicitExtentX;

            if (minimizedAreaExtentX > 0)
                WriteAttribute(writer, MinimizedAreaExtentXAttrib, minimizedAreaExtentX);

            double minimizedAreaExtentY = tilesControl.MinimizedAreaExplicitExtentY;

            if (minimizedAreaExtentY > 0)
                WriteAttribute(writer, MinimizedAreaExtentYAttrib, minimizedAreaExtentY);

            Size? synchronizedItemSize = tilesControl.Manager.SynchronizedItemSize;

            if (synchronizedItemSize.HasValue)
            {
                if (synchronizedItemSize.Value.Width > 0)
                    WriteAttribute(writer, SynchronizedItemWidthAttrib, synchronizedItemSize.Value.Width);

                if (synchronizedItemSize.Value.Height > 0)
                    WriteAttribute(writer, SynchronizedItemHeightAttrib, synchronizedItemSize.Value.Height);
            }

			#region Items

			// first build a list of the items
			writer.WriteStartElement(ItemsTag); // <items>

			foreach (object item in tilesControl.Items)
			{
                ItemInfo info = tilesControl.GetItemInfo(item);

                Debug.Assert(info != null, "ItemInfo not found for item");
                if (info == null)
                    continue;

                Tile tile = tilesControl.TileFromItem(item);

				// skip tiles that shouldn't be serialized
				if (tile != null && tile.SaveInLayout == false)
					continue;

                SavingItemMappingEventArgs args = new SavingItemMappingEventArgs( tile, tilesControl, item);

                tilesControl.RaiseSavingItemMapping(args);

                string serializationId = args.SerializationId;

                if (string.IsNullOrEmpty(serializationId))
                    continue;

				writer.WriteStartElement(ItemTag); // <item>

                writer.WriteAttributeString(SerializationIdAttrib, serializationId);

                if ( info.IsClosed )
                    WriteAttribute(writer, IsClosedAttrib, true);

                if ( info.IsExpandedWhenMinimized.HasValue )
                    WriteAttribute(writer, IsExpandedWhenMinimizedAttrib, info.IsExpandedWhenMinimized.Value);

				// JJD 4/22/11 - TFS58637
				// If the item is closed it won't be in the MaximizedItems collection
				//if (info.IsMaximized)
                if (info.IsMaximized && !info.IsClosed)
                {
                    WriteAttribute(writer, IsMaximizedAttrib, true);

                    int index = tilesControl.MaximizedItems.IndexOf(item);

                    Debug.Assert(index >= 0, "item not found in maximized items collection");

                    if ( index >= 0 )
                        WriteAttribute(writer, MaximizedIndexAttrib, index);
                }

                WriteAttribute(writer, LogicalIndexAttrib, info.LogicalIndex);

                if (isExplicitLayout)
                {
                    DependencyObject dpo = item as DependencyObject;
                    WriteRowColumnAttribueIfNonDefault(writer, TilesPanel.ColumnProperty,       ColumnAttrib,       tile, dpo);
                    WriteRowColumnAttribueIfNonDefault(writer, TilesPanel.ColumnSpanProperty,   ColumnSpanAttrib,   tile, dpo);
                    WriteRowColumnAttribueIfNonDefault(writer, TilesPanel.ColumnWeightProperty, ColumnWeightAttrib, tile, dpo);
                    WriteRowColumnAttribueIfNonDefault(writer, TilesPanel.RowProperty,          RowAttrib,          tile, dpo);
                    WriteRowColumnAttribueIfNonDefault(writer, TilesPanel.RowSpanProperty,      RowSpanAttrib,      tile, dpo);
                    WriteRowColumnAttribueIfNonDefault(writer, TilesPanel.RowWeightProperty,    RowWeightAttrib,    tile, dpo);
                }

                Size? sizeOverride = info.SizeOverride;

                if (sizeOverride.HasValue)
                {
                    if (sizeOverride.Value.Width > 0)
                        WriteAttribute(writer, PreferredWidthOverrideAttrib, sizeOverride.Value.Width);

                    if (sizeOverride.Value.Height > 0)
                        WriteAttribute(writer, PreferredHeightOverrideAttrib, sizeOverride.Value.Height);
                }

				writer.WriteEndElement(); // </item>
			}
			writer.WriteEndElement(); // <items> 

			#endregion //Items

			writer.WriteEndElement(); // </xamTilesControl>
			writer.WriteEndDocument();
			writer.Flush();
		}

		#endregion //SaveLayout

		#endregion //Public Methods

		#region Private Methods

		#region GetItems
		private static object[] GetItems(System.Collections.IList list)
		{
			object[] items = null;
			items = new object[list.Count];
			list.CopyTo(items, 0);
			return items;
		}
		#endregion //GetItems

		#region ParseEnum (string)


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal static T ParseEnum<T>(string value)
		{
			Debug.Assert(typeof(T).IsEnum);

			// AS 3/17/06 Case Insensitive
			// I decided not to use IgnoreCase here since I think we always
			// want this to be case sensitive.
			//
			return (T)Enum.Parse(typeof(T), value, false);
		}
		#endregion //ParseEnum (string)

		#region ReadAttribute
		private static string ReadAttribute(XmlNode node, string name, string defaultValue)
		{
			XmlAttribute attrib = node.Attributes[name];

			if (attrib != null)
				return attrib.Value;

			return defaultValue;
		}

		private static bool ReadAttribute(XmlNode node, string name, bool defaultValue)
		{
			string attrib = ReadAttribute(node, name, null);

			return attrib != null ? XmlConvert.ToBoolean(attrib) : defaultValue;
		}

		private static bool? ReadNullableBoolAttribute(XmlNode node, string name)
		{
			string attrib = ReadAttribute(node, name, null);

            if (attrib != null)
                return XmlConvert.ToBoolean(attrib);

            return null;
		}

		private static int ReadAttribute(XmlNode node, string name, int defaultValue)
		{
			string attrib = ReadAttribute(node, name, null);

			return attrib != null ? XmlConvert.ToInt32(attrib) : defaultValue;
		}

		private static float ReadAttribute(XmlNode node, string name, float defaultValue)
		{
			string attrib = ReadAttribute(node, name, null);

			return attrib != null ? XmlConvert.ToSingle(attrib) : defaultValue;
		}

		private static double ReadAttribute(XmlNode node, string name, double defaultValue)
		{
			string attrib = ReadAttribute(node, name, null);

			return attrib != null ? XmlConvert.ToDouble(attrib) : defaultValue;
		}
		#endregion //ReadAttribute

        #region ReadRowColumnAttribute

        private static void ReadRowColumnAttribute(XmlNode node, ItemSerializationInfo serInfo, DependencyProperty dp, string attributeName, Tile tile, DependencyObject item)
        {
            object defaultValue = dp.DefaultMetadata.DefaultValue;

            Type type = defaultValue.GetType();

            object value = null;
            if (type == typeof(int))
            {
                value = ReadAttribute(node, attributeName, (int)defaultValue);
            }
            else
            if (type == typeof(float))
            {
                value = ReadAttribute(node, attributeName, (float)defaultValue);
            }

            Debug.Assert(value != null, "Value should not be null");
            
            if (value == null)
                return;

            if (tile != null)
                tile.SetValue(dp, value);

            if (item != null)
                item.SetValue(dp, value);
            else
            {
                if (serInfo.RowColumnSettings == null)
                    serInfo.RowColumnSettings = new Dictionary<DependencyProperty, object>();

                serInfo.RowColumnSettings.Add(dp, value);
            }
        }

        #endregion //ReadRowColumnAttribute

        #region WriteAttribute
        
        private static void WriteAttribute(XmlWriter writer, string name, float value)
        {
            writer.WriteAttributeString(name, XmlConvert.ToString(value));
        }

        private static void WriteAttribute(XmlWriter writer, string name, double value)
        {
            writer.WriteAttributeString(name, XmlConvert.ToString(value));
        }

		private static void WriteAttribute(XmlWriter writer, string name, int value)
		{
			writer.WriteAttributeString(name, XmlConvert.ToString(value));
		}

		private static void WriteAttribute(XmlWriter writer, string name, bool value)
		{
			writer.WriteAttributeString(name, XmlConvert.ToString(value));
		}

		#endregion //WriteAttribute

        #region WriteRowColumnAttribueIfNonDefault

        private static void WriteRowColumnAttribueIfNonDefault(XmlTextWriter writer, DependencyProperty dp, string attributeName, Tile tile, DependencyObject item)
        {
            object value = null;

            if (tile != null)
                value = tile.GetValue(dp);
            else if (item != null)
                value = item.GetValue(dp);

            if (value == null)
                return;

            object defalutValue = dp.DefaultMetadata.DefaultValue;

            if (Object.Equals(value, defalutValue))
                return;

            Type type = value.GetType();

            if (type == typeof(int))
            {
                WriteAttribute(writer, attributeName, (int)value);
            }
            else
            if (type == typeof(float))
            {
                WriteAttribute(writer, attributeName, (float)value);
            }
            else
            {
                Debug.Fail("Invalid RowCol property type");
            }
        }

        #endregion //WriteRowColumnAttribueIfNonDefault	
    
		#endregion //Private Methods

        #region ItemSerializationInfo class
        internal class ItemSerializationInfo
		{
			internal object Item;
			internal string SerializationId;
            internal int MaximizedIndex;
            internal int LogicalIndex;
            internal Dictionary<DependencyProperty, object> RowColumnSettings;
        }
        #endregion //ItemSerializationInfo class

        #region ItemInfoComparer class

        private class ItemInfoComparer : IComparer<ItemInfoBase>
        {
            #region IComparer<ItemInfoBase> Members

            public int Compare(ItemInfoBase x, ItemInfoBase y)
            {
                if (x == null)
                    return -1;

                if (y == null)
                    return 1;

                ItemInfo infoX = x as ItemInfo;
                ItemInfo infoY = y as ItemInfo;

                int logicalIndexX = -1;
                int logicalIndexY = -1;
                if (infoX != null && infoX.SerializationInfo != null)
                    logicalIndexX = infoX.SerializationInfo.LogicalIndex;
                else
                    logicalIndexX = x.LogicalIndex;

                if (infoY != null && infoY.SerializationInfo != null)
                    logicalIndexY = infoY.SerializationInfo.LogicalIndex;
                else
                    logicalIndexY = y.LogicalIndex;

                if (logicalIndexX < logicalIndexY)
                    return -1;

                if (logicalIndexX > logicalIndexY)
                    return 1;

                if (x.Index < y.Index)
                    return -1;

                if (x.Index > y.Index)
                    return 1;

                return 0;
            }

            #endregion
        }

        #endregion //ItemInfoComparer class	
    
        #region MaximizedItemInfoComparer class

        private class MaximizedItemInfoComparer : IComparer<ItemInfo>
        {
            #region IComparer<ItemInfo> Members

            public int Compare(ItemInfo x, ItemInfo y)
            {
                if (x == null)
                    return -1;

                if (y == null)
                    return 1;

                int indexX = -1;
                int indexY = -1;
                if (x.SerializationInfo != null)
                    indexX = x.SerializationInfo.MaximizedIndex;
                if (y.SerializationInfo != null)
                    indexY = y.SerializationInfo.MaximizedIndex;

                if (indexX < indexY)
                    return -1;

                if (indexX > indexY)
                    return 1;

                return 0;
            }

            #endregion
        }

        #endregion //MaximizedItemInfoComparer class	
    
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