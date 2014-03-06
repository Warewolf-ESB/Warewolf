
#region Using

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Windows;
using System.ComponentModel;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Data;
using System.Xml;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

#endregion Using

namespace Infragistics.Windows.Chart
{
    /// <summary>
    /// Series is used as a container for <see cref="DataPoints"/>. Has data binding 
    /// functionality and default appearance for all data points if appearance is not 
    /// specified per data point.
    /// </summary>
    /// <remarks>
    /// When a chart is deployed on the page, a default column chart appears with two series. 
    /// This default chart does not contain any data, but when the chart is placed into a visual 
    /// design surface, the chart automatically creates demo data to give it some appearance will 
    /// it is being worked with, or until you supply your data. As soon as you add actual data to 
    /// the control, the demo data disappears from the chart. You cannot change the sample data 
    /// because it does not exist in the Series collection. 
    /// </remarks>
    public class Series : ChartFrameworkContentElement, IWeakEventListener
    {
        #region Fields

        // Private Fields
        private object _chartParent;
        private int _realIndex = -1;
        private string[] _bindColumns;
        private string[] _bindProperties;
        private DataTable _oldDataTable = null;
        private XmlDocument _oldXmlDocument = null;
        private IEnumerable _oldIEnumerable = null;

        #endregion Fields

        #region Properties

        /// <summary>
        /// The parent object
        /// </summary>
        internal object ChartParent
        {
            get
            {
                return _chartParent;
            }
            set
            {
                _chartParent = value;
            }
        }

        /// <summary>
        /// The real index is use for Z position for 3D charts. Different Z 
        /// positions exist because of stacked chart types.
        /// </summary>
        internal int RealIndex
        {
            get
            {
                return _realIndex;
            }
            set
            {
                _realIndex = value;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Initializes a new instance of the Series class. 
        /// </summary>
        public Series()
        {
        }

        /// <summary>
        /// Sends notice when the specified property has been invalidated. 
        /// </summary>
        /// <param name="e">Event arguments that describe the property that changed, including the old and new values.</param>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == DataPoint.ToolTipProperty)
            {
                XamChart control = XamChart.GetControl(this);
                if (control != null && e.NewValue != e.OldValue)
                {
                    control.RefreshProperty();
                }
            }

            base.OnPropertyChanged(e);
        }

        /// <summary>
        /// Represents the callback that is invoked when the effective property value of a given dependency property changes.
        /// </summary>
        /// <param name="d">The DependencyObject on which the property is used.</param>
        /// <param name="e">Arguments that are issued by any event that tracks changes to the effective value of this property.</param>
        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            XamChart control = XamChart.GetControl(d);

            if (e.Property == Series.DataSourceProperty || e.Property == Series.DataMappingProperty)
            {
                if (control != null && control.DefaultChart != null)
                {
                    // fix for bug #8625
                    // when the OnPropertyChanged is called during refresh and we have not binded 
                    // the data souce (i.e. UpdateDataSource == false) we need to force the refresh
                    if (control.IsRefreshingMode && !control.DefaultChart.UpdateDataSource)
                    {
                        control.DefaultChart.UpdateDataSource = true;
                        control.Refresh();
                    }
                    else
                    {
                        control.DefaultChart.UpdateDataSource = true;
                        control.RefreshProperty();
                    }
                    return;
                }
            }

            if (control != null && e.NewValue != e.OldValue)
            {
                control.RefreshProperty();
            }
        }

        /// <summary>
        /// Returns active X axis for this series
        /// </summary>
        /// <returns>X axis</returns>
        internal Axis GetAxisX()
        {
            AxisCollection axes = GetIChart().Axes;

            if (!string.IsNullOrEmpty(AxisX) && axes.GetAxis(AxisType.SecondaryX) != null && !string.IsNullOrEmpty(axes.GetAxis(AxisType.SecondaryX).Name) && string.Compare(axes.GetAxis(AxisType.SecondaryX).Name, AxisX, true, CultureInfo.InvariantCulture) == 0)
            {
                return axes.GetAxis(AxisType.SecondaryX);
            }
            else
            {
                return axes.GetAxis(AxisType.PrimaryX);
            }
        }

        /// <summary>
        /// Gets chart parameter value using chart parameter type
        /// </summary>
        /// <param name="type">Chart parameter type</param>
        /// <returns>Chart parameter value from the parameter collection</returns>
        internal Brush GetParameterValueBrush(ChartParameterType type)
        {
            foreach (ChartParameter parameter in ChartParameters)
            {
                if (parameter.Type == type)
                {
                    return parameter.GetDefaultBrush() as Brush;
                }
            }

            ChartParameter extra = GetExtraParameter(type);
            if (extra != null)
            {
                return extra.GetDefaultBrush() as Brush;
            }

            ChartParameter newAttribute = new ChartParameter();
            newAttribute.Type = type;
            return newAttribute.GetDefaultBrush() as Brush;
        }

        /// <summary>
        /// Gets chart parameter value using chart parameter type
        /// </summary>
        /// <param name="type">Chart parameter type</param>
        /// <returns>Chart parameter value from the parameter collection</returns>
        internal Animation GetParameterValueAnimation(ChartParameterType type)
        {
            foreach (ChartParameter parameter in ChartParameters)
            {
                if (parameter.Type == type)
                {
                    return parameter.GetDefaultAnimation() as Animation;
                }
            }

            ChartParameter extra = GetExtraParameter(type);
            if (extra != null)
            {
                return extra.GetDefaultAnimation() as Animation;
            }

            return null;
        }

        /// <summary>
        /// Gets chart parameter value using chart parameter type
        /// </summary>
        /// <param name="type">Chart parameter type</param>
        /// <returns>Chart parameter value from the parameter collection</returns>
        internal int GetParameterValueInt(ChartParameterType type)
        {
            foreach (ChartParameter parameter in ChartParameters)
            {
                if (parameter.Type == type)
                {
                    return (int)parameter.GetDefaultInt();
                }
            }

            ChartParameter extra = GetExtraParameter(type);
            if (extra != null)
            {
                return (int)extra.GetDefaultInt();
            }

            ChartParameter newAttribute = new ChartParameter();
            newAttribute.Type = type;
            return (int)newAttribute.GetDefaultInt();
        }

        /// <summary>
        /// Search for a ChartParameter in ExtraParameters property which keeps a reference to 
        /// additional ChartParameterCollection. This collection is used for chart 
        /// parameter styling purposes.
        /// </summary>
        /// <param name="type">Chart parameter type</param>
        /// <returns>A chart parameter from ExtraParameters</returns>
        private ChartParameter GetExtraParameter(ChartParameterType type)
        {
            if (ExtraParameters != null)
            {
                if (ExtraParameters is ChartParameterCollection)
                {
                    ChartParameterCollection extra = ExtraParameters as ChartParameterCollection;
                    foreach (ChartParameter parameter in extra)
                    {
                        if (parameter.Type == type)
                        {
                            return parameter;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets chart parameter value using chart parameter type
        /// </summary>
        /// <param name="type">Chart parameter type</param>
        /// <returns>Chart parameter value from the parameter collection</returns>
        internal bool GetParameterValueBool(ChartParameterType type)
        {
            foreach (ChartParameter parameter in ChartParameters)
            {
                if (parameter.Type == type)
                {
                    return (bool)parameter.GetDefaultBool();
                }
            }

            ChartParameter extra = GetExtraParameter(type);
            if (extra != null)
            {
                return (bool)extra.GetDefaultBool();
            }

            return false;
        }

        /// <summary>
        /// Gets chart parameter value using chart parameter type
        /// </summary>
        /// <param name="type">Chart parameter type</param>
        /// <returns>Chart parameter value from the parameter collection</returns>
        internal double GetParameterValueDouble(ChartParameterType type)
        {
            foreach (ChartParameter parameter in ChartParameters)
            {
                if (parameter.Type == type)
                {
                    return (double)parameter.GetDefaultDouble();
                }
            }

            ChartParameter extra = GetExtraParameter(type);
            if (extra != null)
            {
                return (double)extra.GetDefaultDouble();
            }

            ChartParameter newAttribute = new ChartParameter();
            newAttribute.Type = type;
            return (double)newAttribute.GetDefaultDouble();
        }
               

        /// <summary>
        /// Returns a chart parameter from series.
        /// </summary>
        /// <param name="type">Chart Parameter Type</param>
        /// <returns>A Chart parameter or null if an attribute doesn�t exist.</returns>
        internal ChartParameter GetAttribute(ChartParameterType type)
        {
            foreach (ChartParameter parameter in ChartParameters)
            {
                if (parameter.Type == type)
                {
                    return parameter;
                }
            }

            return null;
        }

        /// <summary>
        /// Create a random colors or colors from palette
        /// </summary>
        /// <param name="index">Index of color</param>
        /// <param name="colorFromPoint">True if color is different for every data point.</param>
        /// <returns>Brush with predefined color</returns>
        internal Brush GetPredefinedBrush(int index, bool colorFromPoint)
        {
            XamChart control = XamChart.GetControl(this);
            Color color = control.GetRandomColor(index);
           
            if (control.StartPaletteBrush != null && control.EndPaletteBrush != null)
            {
                int numOfColors = 0;

                if (colorFromPoint)
                {
                    if (DataPoints != null)
                    {
                        numOfColors = DataPoints.Count;
                    }
                }
                else
                {
                    SeriesCollection seriesCollection = ChartParent as SeriesCollection;
                    numOfColors = seriesCollection.Count;
                }

                if (control.StartPaletteBrush != null && control.EndPaletteBrush != null)
                {
                    if (control.StartPaletteBrush is SolidColorBrush && control.EndPaletteBrush is SolidColorBrush)
                    {
                        SolidColorBrush startBrush = control.StartPaletteBrush as SolidColorBrush;
                        SolidColorBrush endBrush = control.EndPaletteBrush as SolidColorBrush;
                        if (numOfColors - 1 == 0)
                        {
                            color = ColorPosition(startBrush.Color, endBrush.Color, 0);
                        }
                        else
                        {
                            color = ColorPosition(startBrush.Color, endBrush.Color, ((double)index) / ((double)(numOfColors - 1)));
                        }

                        return new SolidColorBrush(color);
                    }

                    if (control.StartPaletteBrush is LinearGradientBrush && control.EndPaletteBrush is LinearGradientBrush)
                    {
                        LinearGradientBrush startBrush = control.StartPaletteBrush as LinearGradientBrush;
                        LinearGradientBrush endBrush = control.EndPaletteBrush as LinearGradientBrush;

                        LinearGradientBrush newBrush = startBrush.CloneCurrentValue();

                        int numOfBrushes = Math.Min(startBrush.GradientStops.Count, endBrush.GradientStops.Count);
                        for (int brushIndx = 0; brushIndx < numOfBrushes; brushIndx++)
                        {
                            Color colorG;
                            if (numOfColors - 1 == 0)
                            {
                                colorG = ColorPosition(startBrush.GradientStops[brushIndx].Color, endBrush.GradientStops[brushIndx].Color, 0);
                            }
                            else
                            {
                                colorG = ColorPosition(startBrush.GradientStops[brushIndx].Color, endBrush.GradientStops[brushIndx].Color, ((double)index) / ((double)(numOfColors - 1)));
                            }

                            newBrush.GradientStops[brushIndx].Color = colorG;
                        }
                        
                        return newBrush;
                    }

                    if (control.StartPaletteBrush is RadialGradientBrush && control.EndPaletteBrush is RadialGradientBrush)
                    {
                        RadialGradientBrush startBrush = control.StartPaletteBrush as RadialGradientBrush;
                        RadialGradientBrush endBrush = control.EndPaletteBrush as RadialGradientBrush;
                        Color color1;
                        Color color2;
                        if (numOfColors - 1 == 0)
                        {
                            color1 = ColorPosition(startBrush.GradientStops[0].Color, endBrush.GradientStops[0].Color, 0);
                            color2 = ColorPosition(startBrush.GradientStops[1].Color, endBrush.GradientStops[1].Color, 0);
                        }
                        else
                        {
                            color1 = ColorPosition(startBrush.GradientStops[0].Color, endBrush.GradientStops[0].Color, ((double)index) / ((double)(numOfColors - 1)));
                            color2 = ColorPosition(startBrush.GradientStops[1].Color, endBrush.GradientStops[1].Color, ((double)index) / ((double)(numOfColors - 1)));
                        }

                        RadialGradientBrush newBrush = startBrush.CloneCurrentValue();
                        newBrush.GradientStops[0].Color = color1;
                        newBrush.GradientStops[1].Color = color2;
                        return newBrush;
                    }
                }
            }

            // Set fill with random color
            return new SolidColorBrush(color);
        }

        /// <summary>
        /// Check if chart parameter value is set
        /// </summary>
        /// <param name="type">Chart parameter type</param>
        /// <returns>True if parameter is set</returns>
        internal bool IsParameterSet(ChartParameterType type)
        {
            foreach (ChartParameter parameter in ChartParameters)
            {
                if (parameter.Type == type)
                {
                    return true;
                }
            }

            return false;
        }

        private Color ColorPosition(Color startColor, Color endColor, double position)
        {
            Color newColor = new Color();
            newColor.R = ColorPosition(startColor.R, endColor.R, position);
            newColor.G = ColorPosition(startColor.G, endColor.G, position);
            newColor.B = ColorPosition(startColor.B, endColor.B, position);
            newColor.A = ColorPosition(startColor.A, endColor.A, position);

            return newColor;
        }

        private byte ColorPosition( byte startColor, byte endColor, double position )
        {
            double start = startColor;
            double end = endColor;

            if( position < 0 && position > 1 )
            {
                // Palette color position can be value between 0 and 1
                throw new InvalidOperationException(ErrorString.Exc38);
            }

            return (byte)Math.Floor(start + (end - start + 0.5) * position);
        }

        /// <summary>
        /// Get the parent IChart for this data series.
        /// </summary>
        internal IChart GetIChart()
        {
            SeriesCollection seriesCollection = ChartParent as SeriesCollection;
            return seriesCollection.ChartParent as IChart;
        }

        /// <summary>
        /// Add child objects to logical tree
        /// </summary>
        internal void AddChildren()
        {
            foreach (DataPoint point in this.DataPoints)
            {
                AddChild(point);
                point.AddChildren();
            }
            AddChild(this.Marker);
            AddChild(this.Animation);
        }

        /// <summary>
        /// Gets an enumerator for this element's logical child elements.
        /// </summary>
        protected override IEnumerator LogicalChildren
        {
            get
            {
                ArrayList _list = new ArrayList();

                foreach (DataPoint point in this.DataPoints)
                {
                    _list.Add(point);
                }
                _list.Add(this.Marker);
                _list.Add(this.Animation);

                return (IEnumerator)_list.GetEnumerator();
            }
        }

        #endregion Methods

        #region Public Properties

        #region Data Binding

        /// <summary>
        /// Split DataMapping string and creates arrays with column and value names
        /// </summary>
        private void SplitDataMapping()
        {
            if (this.DataMapping == null)
            {
                _bindColumns = null;
                _bindProperties = null;
                return;
            }

            // Split Data Mapping using ; character
            string[] sections = this.DataMapping.Split(';');

            _bindColumns = new string[sections.Length];
            _bindProperties = new string[sections.Length];

            if (sections.Length == 1)
            {
                // Split section of Data Mapping using = character
                string[] elements = sections[0].Split('=');

                if (elements.Length == 1)
                {
                    elements = new string[2];
                    elements[0] = "Value";
                    elements[1] = sections[0];
                }
                else if (elements.Length != 2)
                {
                    // Bad format for DataMapping property.
                    throw new InvalidOperationException(ErrorString.Exc37);
                }

                _bindProperties[0] = elements[0].Trim();
                _bindColumns[0] = elements[1].Trim();

                return;
            }

            int index = 0;
            foreach (string section in sections)
            {
                // Split section of Data Mapping using = character
                string[] elements = section.Split('=');

                if (elements.Length == 1)
                {
                    elements = new string[2];
                    elements[0] = section;
                    elements[1] = "";
                }
                else if (elements.Length != 2)
                {
                    // Bad format for DataMapping property.
                    throw new InvalidOperationException(ErrorString.Exc37);
                }

                _bindProperties[index] = elements[0].Trim();
                _bindColumns[index] = elements[1].Trim();

                index++;
            }
        }

        /// <summary>
        /// This method binds various data source types to data points of this series.
        /// </summary>
        internal void DataBind()
        {
            SplitDataMapping();

            // Data Mapping not specified
            if (_bindProperties == null)
            {
                return;
            }

            object dataSource = this.DataSource;

            if (dataSource == null && this.DataContext != null)
            {
                dataSource = this.DataContext;
            }

            // Data Source not specified
            if (dataSource == null)
            {
                return;
            }

            this.DataPoints.Clear();

            // Data set source
            if (dataSource is DataSet)
            {
                DataBind(dataSource as DataSet);
            }

            // Data table source
            if (dataSource is DataTable)
            {
                DataBind(dataSource as DataTable);
                return;
            }

            // XML source
            if (dataSource is XmlNode)
            {
                DataBind(dataSource as XmlNode);
                return;
            }

            // IEnumerable source
            if (dataSource is IEnumerable)
            {
                DataBind(dataSource as IEnumerable);
                return;
            }
        }
        private void ClearEventHandlers(IEnumerable dataSource)
        {
            INotifyCollectionChanged oldCollection = this._oldIEnumerable as INotifyCollectionChanged;
            if (oldCollection != null)
            {
               
            }

            
        }
        private void ListChanged()
        {
            XamChart control = XamChart.GetControl(this);
            if (control != null && control.DefaultChart != null)
            {
                control.DefaultChart.UpdateDataSource = true;
                control.Refresh();
            }
        }
        private void ibl_ListChanged(object sender, ListChangedEventArgs e)
        {
            this.ListChanged();
        }

        /// <summary>
        /// Occurs when the collection changes, either by adding or removing an item. 
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">Information about the event.</param>
        private void bindingList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.ListChanged();
        }

        /// <summary>
        /// Data bind data from IEnumerable to data points.
        /// </summary>
        /// <param name="source">IEnumerable</param>
        private void DataBind(IEnumerable source)
        {
            this.SetEvents(source);

            if (CheckNestedCollections(source))
            {
                return;
            }

            IEnumerator enumerator = source.GetEnumerator();

            while(enumerator.MoveNext())
            {
                // Data bind to data point properties
                DataPoint point = new DataPoint();

                object current = enumerator.Current;

                if (current is double || current is int || current is string)
                {
                    point.SetBindValue("VALUE", current, this);
                }
                else
                {   
                    for (int columnIndx = 0; columnIndx < _bindColumns.Length; columnIndx++)
                    {
                        if (!String.IsNullOrEmpty(_bindColumns[columnIndx]))
                        {
                            point.SetBindValueObject(_bindProperties[columnIndx], _bindColumns[columnIndx], current, this);
                            point.DataContext = current;
                        }
                    }
                }

                // Add data point per data row
                DataPoints.Add(point);
            } 
        }

        /// <summary>
        /// Data bind data from nested IEnumerable to data points without adding new points.
        /// </summary>
        /// <param name="source">IEnumerable</param>
        /// <param name="propertyIndex">The index of a property name to bind</param>
        private void DataBindSet(IEnumerable source, int propertyIndex)
        {
            IEnumerator enumerator = source.GetEnumerator();

            int pointIndex = 0;
            while (enumerator.MoveNext())
            {
                object current = enumerator.Current;
                if (pointIndex < DataPoints.Count)
                {
                    DataPoints[pointIndex].SetBindValue(_bindProperties[propertyIndex], current, this);
                }
                pointIndex++;
            }
        }

        /// <summary>
        /// Data bind data from nested IEnumerable to data points without adding new points.
        /// </summary>
        /// <param name="source">IEnumerable</param>
        /// <param name="propertyIndex">The index of a property name to bind</param>
        private void DataBindAdd(IEnumerable source, int propertyIndex)
        {
            IEnumerator enumerator = source.GetEnumerator();

            while (enumerator.MoveNext())
            {
                // Data bind to data point properties
                DataPoint point = new DataPoint();

                object current = enumerator.Current;
                point.SetBindValue(_bindProperties[propertyIndex], current, this);

                // Add data point per data row
                DataPoints.Add(point);
            } 
        }

        private bool CheckNestedCollections(IEnumerable source)
        {
            IEnumerator enumerator = source.GetEnumerator();

            bool isEnumerable = false;
            int index = 0;
            while (enumerator.MoveNext())
            {
                // Data bind to data point properties
                object current = enumerator.Current;
                
                if (current is IEnumerable)
                {
                    isEnumerable = true;
                    if( index == 0 )
                    {
                        DataBindAdd((IEnumerable)current, index);
                    }
                    else
                    {
                        DataBindSet((IEnumerable)current, index);
                    }
                    index++;
                }
            }

            return isEnumerable;
        }

        /// <summary>
        /// Data bind data from data set to data points.
        /// </summary>
        /// <param name="source">Data set</param>
        private void DataBind(DataSet source)
        {
            if( source.Tables.Count > 0 )
            {
                DataBind(source.Tables[0]);
            }
        }

        /// <summary>
        /// Data bind data from XML data source to data points.
        /// </summary>
        /// <param name="source">Xml node</param>
        private void DataBind(XmlNode source)
        {
            if (source.OwnerDocument != null)
            {
                XmlDocument xmlDocument = source.OwnerDocument;
                SetEvents(xmlDocument);
            }

            XmlReader reader = new XmlNodeReader(source);
            DataSet dataSet = new DataSet();
            dataSet.Locale = CultureInformation.CultureToUse;
            dataSet.ReadXml(reader);
            DataBind(dataSet.Tables[0]);
        }
           
        /// <summary>
        /// Data bind data from data table to data points.
        /// </summary>
        /// <param name="source">Data table</param>
        private void DataBind(DataTable source)
        {
            SetEvents(source);

            // Data row loop
            foreach (DataRow row in source.Rows)
            {
                // Data bind to data point properties
                DataPoint point = new DataPoint();
                for (int columnIndx = 0; columnIndx < _bindColumns.Length; columnIndx++)
                {
                    if (!String.IsNullOrEmpty(_bindColumns[columnIndx]))
                    {
                        point.SetBindValue(_bindProperties[columnIndx], row[_bindColumns[columnIndx]], this);
                    }
                }
                // Add data point per data row
                DataPoints.Add(point);
            }
        }

        private void SetEvents(XmlDocument source)
        {
            SeriesCollection seriesList = GetIChart().Series;

            foreach (Series series in seriesList)
            {
                if (series == this)
                {
                    break;
                }

                if (series.DataSource == source || series.DataContext == source)
                {
                    return;
                }
            }

            // Remove event from old xml document
            if (_oldXmlDocument != null && _oldXmlDocument != source)
            {
                _oldXmlDocument.NodeChanged -= new XmlNodeChangedEventHandler(source_NodeChanged);
                _oldXmlDocument.NodeInserted -= new XmlNodeChangedEventHandler(source_NodeInserted);
                _oldXmlDocument.NodeRemoved -= new XmlNodeChangedEventHandler(source_NodeRemoved);
            }

            if (_oldXmlDocument == null || _oldXmlDocument != source)
            {
                // Set Xml Document changed events
                source.NodeChanged += new XmlNodeChangedEventHandler(source_NodeChanged);
                source.NodeInserted += new XmlNodeChangedEventHandler(source_NodeInserted);
                source.NodeRemoved += new XmlNodeChangedEventHandler(source_NodeRemoved);
            }

            _oldXmlDocument = source;
        }

        private void SetEvents(DataTable source)
        {
            SeriesCollection seriesList = GetIChart().Series;

            foreach (Series series in seriesList)
            {
                if (series == this)
                {
                    break;
                }

                if (series.DataSource == source || series.DataContext == source)
                {
                    return;
                }
            }

            // Remove event from old data table
            if (_oldDataTable != null && _oldDataTable != source)
            {
                _oldDataTable.RowChanged -= new DataRowChangeEventHandler(source_RowChanged);
                _oldDataTable.RowDeleted -= new DataRowChangeEventHandler(source_RowDeleted);
                _oldDataTable.ColumnChanged -= new DataColumnChangeEventHandler(source_ColumnChanged);
                _oldDataTable.TableCleared -= new DataTableClearEventHandler(source_TableCleared);
                _oldDataTable.TableNewRow -= new DataTableNewRowEventHandler(source_TableNewRow);
            }

            if (_oldDataTable == null || _oldDataTable != source)
            {
                // Set Data Table changed events
                source.RowChanged += new DataRowChangeEventHandler(source_RowChanged);
                source.RowDeleted += new DataRowChangeEventHandler(source_RowDeleted);
                source.ColumnChanged += new DataColumnChangeEventHandler(source_ColumnChanged);
                source.TableCleared += new DataTableClearEventHandler(source_TableCleared);
                source.TableNewRow += new DataTableNewRowEventHandler(source_TableNewRow);
            }

            _oldDataTable = source;
        }

        private void SetEvents(IEnumerable source)
        {
            SeriesCollection seriesList = GetIChart().Series;

            foreach (Series series in seriesList)
            {
                if (series == this)
                {
                    break;
                }

                if (series.DataSource == source || series.DataContext == source)
                {
                    return;
                }
            }

            INotifyCollectionChanged collection = source as INotifyCollectionChanged;
            INotifyCollectionChanged oldCollection = _oldIEnumerable as INotifyCollectionChanged;
            IBindingList bindingList = source as IBindingList;
            IBindingList oldBindingList = this._oldIEnumerable as IBindingList;

            
            if (_oldIEnumerable != null && _oldIEnumerable != source)
            {
                if (oldCollection != null)
                {
                    CollectionChangedEventManager.RemoveListener(oldCollection, this);
                }
                else if (oldBindingList != null)
                {
                    oldBindingList.ListChanged -= new ListChangedEventHandler(ibl_ListChanged);
                }
            }

            if (_oldIEnumerable == null || _oldIEnumerable != source)
            {
                if (collection != null)
                {
                    CollectionChangedEventManager.AddListener(collection, this);
                }
                else if (bindingList != null)
                {
                    bindingList.ListChanged += new ListChangedEventHandler(ibl_ListChanged);
                }
            }

            _oldIEnumerable = source;
        }

        void source_TableNewRow(object sender, DataTableNewRowEventArgs e)
        {
            OnDataTableRefreshed();
        }

        void source_TableCleared(object sender, DataTableClearEventArgs e)
        {
            OnDataTableRefreshed();
        }

        void source_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            OnDataTableRefreshed();
        }

        void source_RowDeleted(object sender, DataRowChangeEventArgs e)
        {
            OnDataTableRefreshed();
        }

        void source_RowChanged(object sender, DataRowChangeEventArgs e)
        {
            OnDataTableRefreshed();
        }

        void source_NodeRemoved(object sender, XmlNodeChangedEventArgs e)
        {
            OnDataTableRefreshed();
        }

        void source_NodeInserted(object sender, XmlNodeChangedEventArgs e)
        {
            OnDataTableRefreshed();
        }

        void source_NodeChanged(object sender, XmlNodeChangedEventArgs e)
        {
            OnDataTableRefreshed();
        }


        void OnDataTableRefreshed()
        {
            XamChart control = XamChart.GetControl(this);
            if (control != null && control.DefaultChart != null)
            {
                control.DefaultChart.UpdateDataSource = true;
                control.Refresh();
            }
        }

        #endregion Data Binding

        #region DataPoints

        private DataPointCollection _dataPoints = new DataPointCollection();

        /// <summary>
        /// Gets or sets data points for this series.
        /// </summary>
        //[Description("Gets or sets data points for this series.")]
        //[Category("Data")]
        public DataPointCollection DataPoints
        {
            get
            {
                if (_dataPoints.ChartParent == null)
                {
                    _dataPoints.ChartParent = this;
                }
                return _dataPoints;
            }
        }

        #endregion DataPoints

        #region ChartParameters

        private ChartParameterCollection _chartParameters = new ChartParameterCollection();

        /// <summary>
        /// Gets or sets chart parameters for this series.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// Chart parameters are used to decrese the number of public 
        /// properties used in series and data points.  Used for numerous 
        /// number of parameters which are different for every chart type.
        /// </p>
        /// <p class="body">
        /// If chart parameter is specified for a series it will apply on every 
        /// data point from the series. If a chart parameter is set for a data point, 
        /// the chart parameter from the series will be ignored.
        /// </p>
        /// </remarks>
        //[Description("Gets or sets chart parameters for this series.")]
        //[Category("Data")]
        public ChartParameterCollection ChartParameters
        {
            get
            {
                if (_chartParameters.ChartParent == null)
                {
                    _chartParameters.ChartParent = this;
                }
                return _chartParameters;
            }
        }

        #endregion ChartParameters

        #region ExtraParameters

        /// <summary>
        /// Identifies the <see cref="ExtraParameters"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ExtraParametersProperty = DependencyProperty.Register("ExtraParameters",
            typeof(object), typeof(Series), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets a reference to additional ChartParameterCollection. This collection is used for chart 
        /// parameter styling purposes.
        /// </summary>
        /// <seealso cref="ExtraParametersProperty"/>
        //[Description("Gets or sets a reference to additional ChartParameterCollection. This collection is used for chart parameter styling purposes.")]
        public object ExtraParameters
        {
            get
            {
                return (object)this.GetValue(Series.ExtraParametersProperty);
            }
            set
            {
                this.SetValue(Series.ExtraParametersProperty, value);
            }
        }

        #endregion ExtraParameters

        #region ChartType

        /// <summary>
        /// Identifies the <see cref="ChartType"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ChartTypeProperty = DependencyProperty.Register("ChartType",
            typeof(ChartType), typeof(Series), new FrameworkPropertyMetadata(ChartType.Column, new PropertyChangedCallback(OnPropertyChanged)));


        /// <summary>
        /// Gets or sets a chart type used for this data series. For 3D chart types, <see cref="Infragistics.Windows.Chart.XamChart.View3D"/> property of the <see cref="Infragistics.Windows.Chart.XamChart"/> control has to be set to �true�.
        /// </summary>
        /// <para class="body">Refer to the <a href="xamChart_Chart_Types.html">Chart Types</a> topic in the Developer's Guide for a explanation of the various chart types.</para>
        /// <seealso cref="ChartTypeProperty"/>
        //[Description("Gets or sets a chart type used for this data series. For 3D chart types, View3D property of the XamChart control has to be set to �true�.")]
        //[Category("Appearance")]
        public ChartType ChartType
        {
            get
            {
                return (ChartType)this.GetValue(Series.ChartTypeProperty);
            }
            set
            {
                this.SetValue(Series.ChartTypeProperty, value);
            }
        }

        #endregion ChartType

        #region DataPointColor

        /// <summary>
        /// Identifies the <see cref="DataPointColor"/> dependency property
        /// </summary>
        public static readonly DependencyProperty DataPointColorProperty = DependencyProperty.Register("DataPointColor",
            typeof(DataPointColor), typeof(Series), new FrameworkPropertyMetadata(DataPointColor.Auto, new PropertyChangedCallback(OnPropertyChanged)));


        /// <summary>
        /// Gets or sets a value that indicates a way on which data points get colors. By default colors for data points depend on chart type (for example pie chart have different colors for every data point and column chart have same color for all points).
        /// </summary>
        /// <seealso cref="DataPointColorProperty"/>
        //[Description("Gets or sets a value that indicates a way on which data points get colors. By default colors for data points depend on chart type (for example pie chart have different colors for every data point and column chart have same color for all points).")]
        //[Category("Appearance")]
        public DataPointColor DataPointColor
        {
            get
            {
                return (DataPointColor)this.GetValue(Series.DataPointColorProperty);
            }
            set
            {
                this.SetValue(Series.DataPointColorProperty, value);
            }
        }

        #endregion DataPointColor

        #region DataMapping

        /// <summary>
        /// Identifies the <see cref="DataMapping"/> dependency property
        /// </summary>
        public static readonly DependencyProperty DataMappingProperty = DependencyProperty.Register("DataMapping",
            typeof(string), typeof(Series), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets an expression string used to define data binding. In the simplest case, this is just a column name from a data table.
        /// </summary>
        /// <remarks>
        /// If just a column name is specified, the values from data table will be bound to 
        /// <see cref="Infragistics.Windows.Chart.DataPoint.Value"/> property of a <see cref="Infragistics.Windows.Chart.DataPoint"/>. To specify multiple binding it is necessary to divide 
        /// column and value names using ; character (For example: "X = myXColumn; Y = myYColumn"). 
        /// The Character = is used to split Property Value name from Column name 
        /// (For example: "Value = mySalePrice"). In the following text we have few examples of 
        /// data binding: 
        /// <ul>
        /// <li>If Just column name is used "XColumn" the value will be bind to the Value property of a data point. We can also use following syntax to bind to the value property:  "Value = myXColumn".</li>
        /// <li>This example shows how to bind data for the scatter chart: "ValueX = myXColumn; ValueY = myYColumn; ValueZ = myZColumn". X, Y and Z are <see cref="Infragistics.Windows.Chart.ChartParameter"/>.</li>
        /// <li>The next example shows how to bind data for the stock chart: "High = myHiColumnName; Low = myLowColumnName; Open = myOpenColumnName; Close = myCloseColumnName". High, Low, Open and Close are <see cref="Infragistics.Windows.Chart.ChartParameter"/>.</li>
        /// <li>At the end we have example of binding data to the bubble chart: "ValueX = myXColumn; ValueY = myYColumn; ValueZ = myZColumn; Radius = myRColumn", where myXColumn, myYColumn, myZColumn and myRColumn are column names from your data base.</li>
        /// </ul>
        /// Contrary to the <see cref="Infragistics.Windows.Chart.DataPoint.Value"/> property of the <see cref="Infragistics.Windows.Chart.DataPoint"/>, ValueX, ValueY, ValueZ, Radius, High, Low, Open and Close values are 
        /// <see cref="Infragistics.Windows.Chart.ChartParameter"/> and can be found in the <see cref="Infragistics.Windows.Chart.DataPoint.ChartParameters"/> collection (data point�s property). Data Binding 
        /// process creates data points and chart parameters inside data points (if necessary for scatter, bubble, stock, etc.) and fill 
        /// collection of data points from <see cref="Infragistics.Windows.Chart.Series"/>.
        /// </remarks>
        /// <para class="body">Refer to the <a href="xamChart_Data_Mapping.html">Data Mapping</a> topic in the Developer's Guide for a explanation of the various chart types data binding.</para>
        /// <seealso cref="Infragistics.Windows.Chart.XamChart.DataSourceRefresh"/>
        /// <seealso cref="DataMappingProperty"/>
        //[Description("Gets or sets an expression string used to define data binding. In the simplest case, this is just a column name from a data table.")]
        //[Category("Data")]
        public string DataMapping
        {
            get
            {
                return (string)this.GetValue(Series.DataMappingProperty);
            }
            set
            {
                this.SetValue(Series.DataMappingProperty, value);
            }
        }

        #endregion DataMapping

        #region DataSource

        /// <summary>
        /// Identifies the <see cref="DataSource"/> dependency property
        /// </summary>
        public static readonly DependencyProperty DataSourceProperty = DependencyProperty.Register("DataSource",
            typeof(object), typeof(Series), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets a data source used to bind data to this series. Data source could be DataSet, DataTable, Collection, XML Data source, etc.
        /// </summary>
        /// <remarks>
        /// A <see cref="Infragistics.Windows.Chart.DataPoint"/> is created for every record, and the <see cref="Infragistics.Windows.Chart.DataPoint.Value"/> property of 
        /// the data point will be filled with a value from the specified data source. The series will be filled with created data points. 
        /// If <see cref="DataMapping"/> property is not specified, only the <see cref="Infragistics.Windows.Chart.DataPoint.Value"/> property from data point will be set. To fill <see cref="Infragistics.Windows.Chart.DataPoint.Label"/> or 
        /// <see cref="Infragistics.Windows.Chart.DataPoint.ChartParameters"/> (Chart parameters keep values for Stock or Scatter chart types), 
        /// the <see cref="DataMapping"/> property from the series has to be used. 
        /// </remarks>
        /// <seealso cref="DataMapping"/>
        /// <seealso cref="Infragistics.Windows.Chart.XamChart.DataSourceRefresh"/>
        /// <seealso cref="DataSourceProperty"/>
        //[Description("Gets or sets a data source used to bind data to this series. Data source could be DataSet, DataTable, etc.")]
        //[Category("Data")]
        public object DataSource
        {
            get
            {
                return (object)this.GetValue(Series.DataSourceProperty);
            }
            set
            {
                this.SetValue(Series.DataSourceProperty, value);
            }
        }

        #endregion DataSource

        #region Fill

        /// <summary>
        /// Identifies the <see cref="Fill"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FillProperty = DependencyProperty.Register("Fill",
            typeof(Brush), typeof(Series), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnFillPropertyChanged)));

        /// <summary>
        /// Gets or sets the Brush that specifies how to paint the interior of the shape. 
        /// </summary>
        /// <seealso cref="FillProperty"/>
        //[Description("Gets or sets the Brush that specifies how to paint the interior of the shape. ")]
        //[Category("Brushes")]
        public Brush Fill
        {
            get
            {
                return (Brush)this.GetValue(Series.FillProperty);
            }
            set
            {
                this.SetValue(Series.FillProperty, value);
            }
        }

        #endregion Fill

        #region Stroke

        /// <summary>
        /// Identifies the <see cref="Stroke"/> dependency property
        /// </summary>
        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register("Stroke",
            typeof(Brush), typeof(Series), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnStrokePropertyChanged)));

        /// <summary>
        /// Gets or sets the Brush that specifies how to paint the Shape outline.
        /// </summary>
        /// <seealso cref="StrokeProperty"/>
        //[Description("Gets or sets the Brush that specifies how to paint the Shape outline.")]
        //[Category("Brushes")]
        public Brush Stroke
        {
            get
            {
                return (Brush)this.GetValue(Series.StrokeProperty);
            }
            set
            {
                this.SetValue(Series.StrokeProperty, value);
            }
        }

        #endregion Stroke

        #region StrokeThickness

        /// <summary>
        /// Identifies the <see cref="StrokeThickness"/> dependency property
        /// </summary>
        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register("StrokeThickness",
            typeof(double), typeof(Series), new FrameworkPropertyMetadata((double)1, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the width of the Shape outline. 
        /// </summary>
        /// <seealso cref="StrokeThicknessProperty"/>
        //[Description("Gets or sets the width of the Shape outline.")]
        //[Category("Appearance")]
        public double StrokeThickness
        {
            get
            {
                return (double)this.GetValue(Series.StrokeThicknessProperty);
            }
            set
            {
                this.SetValue(Series.StrokeThicknessProperty, value);
            }
        }

        #endregion StrokeThickness	

        #region Animation

        /// <summary>
        /// Identifies the <see cref="Animation"/> dependency property
        /// </summary>
        public static readonly DependencyProperty AnimationProperty = DependencyProperty.Register("Animation",
            typeof(Animation), typeof(Series), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the animation for Series. This animation is different for every chart type.
        /// </summary>
        /// <remarks>
        /// This animation is used only when one data point has to be animated synchronously with another 
        /// data points. Data Points could be animated using animation property from data points, but 
        /// the most common use of animation is from series, when data points are animated together. This 
        /// animation is only used to create growing effect, but data point animation could be also created 
        /// using brush property and WPF animation.
        /// </remarks>
        /// <seealso cref="AnimationProperty"/>
        //[Description("Gets or sets the animation for Series. This animation is different for every chart type.")]
        //[Category("Appearance")]
        public Animation Animation
        {
            get
            {
                Animation obj = (Animation)this.GetValue(Series.AnimationProperty);
                if (obj != null)
                {
                    obj.ChartParent = this;
                }

                return obj;
            }
            set
            {
                this.SetValue(Series.AnimationProperty, value);
            }
        }

        #endregion Animation

        #region Label

        /// <summary>
        /// Identifies the <see cref="Label"/> dependency property
        /// </summary>
        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register("Label",
            typeof(string), typeof(Series), new FrameworkPropertyMetadata(String.Empty, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets series label. Series labels stores text values for the legend. In 3D they are used for category Z labels also.
        /// </summary>
        /// <seealso cref="LabelProperty"/>
        /// <remarks>
        /// <p class="body">To change appearance of the legend items the font properties from the Legend has to be used. Label property of a Series is used only to store a text value.</p>
        /// <p class="body">Important! Some chart types like pie or doughnut use data point labels for legend items.</p>
        /// </remarks>
        //[Description("Gets or sets data point label. Data point labels stores text values for category axis labels.")]
        //[Category("Data")]
        public string Label
        {
            get
            {
                return (string)this.GetValue(Series.LabelProperty);
            }
            set
            {
                this.SetValue(Series.LabelProperty, value);
            }
        }

        #endregion Label

        #region Marker

        /// <summary>
        /// Identifies the <see cref="Marker"/> dependency property
        /// </summary>
        public static readonly DependencyProperty MarkerProperty = DependencyProperty.Register("Marker",
            typeof(Marker), typeof(Series), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the marker. Marker is a colored shape which shows exact value of 
        /// a Data Point. Marker has corresponding marker label. Used in combination with 
        /// different chart types. 
        /// </summary>
        /// <remarks>
        /// <p class="body">Markers can be defined for series or data points. If Marker is 
        /// not defined for DataPoint, the marker from parent series is used.</p>
        /// <p class="body">Some chart types don�t use marker shapes or marker labels. 
        /// Chart types without Axis don�t have marker shapes (pie or doughnut charts). 
        /// 3D Charts don�t have marker shapes, they have marker labels only.</p>
        /// </remarks>
        /// <seealso cref="MarkerProperty"/>
        //[Description("Gets or sets the marker. Marker is a colored shape which shows exact value of a Data Point. Marker has corresponding marker label. Used in combination with different chart types.")]
        public Marker Marker
        {
            get
            {
                Marker obj = (Marker)this.GetValue(Series.MarkerProperty);
                if (obj != null)
                {
                    obj.ChartParent = this;
                }

                return obj;
            }
            set
            {
                this.SetValue(Series.MarkerProperty, value);
            }
        }

        #endregion Marker

        #region AxisX

        /// <summary>
        /// Identifies the <see cref="AxisX"/> dependency property
        /// </summary>
        public static readonly DependencyProperty AxisXProperty = DependencyProperty.Register("AxisX",
            typeof(string), typeof(Series), new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the X Axis Name. Used to make connection between Series and Axes for Secondary axes.
        /// </summary>
        /// <remarks>
        /// Every series needs X and Y axes for 2D charts and X, Y and Z axes for 3D charts.  Every chart scene 
        /// can have 5 axes 3 Primary and 2 secondary (3D charts cannot have secondary axes). For example, series can use only one X axis, and it could 
        /// be primary or secondary.  To specify which axis we want to use we have to set the same text for AxisX 
        /// property of the Series and the <see cref="Infragistics.Windows.Chart.Axis.Name"/> property of an axis. If those properties are empty the primary axes are used.
        /// </remarks>
        /// <seealso cref="AxisXProperty"/>
        //[Description("Gets or sets the X Axis Name. Used to make connection between Series and Axes for Secondary axes.")]
        public string AxisX
        {
            get
            {
                return (string)this.GetValue(Series.AxisXProperty);
            }
            set
            {
                this.SetValue(Series.AxisXProperty, value);
            }
        }

        #endregion AxisX

        #region AxisY

        /// <summary>
        /// Identifies the <see cref="AxisY"/> dependency property
        /// </summary>
        public static readonly DependencyProperty AxisYProperty = DependencyProperty.Register("AxisY",
            typeof(string), typeof(Series), new FrameworkPropertyMetadata(string.Empty, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets the Y Axis Name. Used to make connection between Series and Axes for Secondary axes.
        /// </summary>
        /// <remarks>
        /// Every series needs X and Y axes for 2D charts and X, Y and Z axes for 3D charts.  Every chart scene 
        /// can have 5 axes 3 Primary and 2 secondary (3D charts cannot have secondary axes). For example, series can use only one Y axis, and it could 
        /// be primary or secondary.  To specify which axis we want to use we have to set the same text for AxisY 
        /// property of the Series and the <see cref="Infragistics.Windows.Chart.Axis.Name"/> property of an axis. If those properties are empty the primary axes are used.
        /// </remarks>
        /// <seealso cref="AxisYProperty"/>
        //[Description("Gets or sets the Y Axis Name. Used to make connection between Series and Axes for Secondary axes.")]
        public string AxisY
        {
            get
            {
                return (string)this.GetValue(Series.AxisYProperty);
            }
            set
            {
                this.SetValue(Series.AxisYProperty, value);
            }
        }

        #endregion AxisY

        #region UseDataTemplate

        /// <summary>
        /// Identifies the <see cref="UseDataTemplate"/> dependency property
        /// </summary>
        public static readonly DependencyProperty UseDataTemplateProperty = DependencyProperty.Register("UseDataTemplate",
            typeof(bool), typeof(Series), new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnPropertyChanged)));

        /// <summary>
        /// Gets or sets a value that indicates whether data points use a data template.
        /// </summary>
        /// <seealso cref="UseDataTemplateProperty"/>
        //[Description("Gets or sets a value that indicates whether data points use a data template.")]
        //[Category("Behavior")]
        public bool UseDataTemplate
        {
            get
            {
                return (bool)this.GetValue(Series.UseDataTemplateProperty);
            }
            set
            {
                this.SetValue(Series.UseDataTemplateProperty, value);
            }
        }

        #endregion UseDataTemplate

        #region ToolTip

        /// <summary>
        /// Gets or sets the tool-tip object that is displayed for this <see cref="Infragistics.Windows.Chart.Series"/>. 
        /// </summary>
        //[Description("Gets or sets the tool-tip object that is displayed for this Series")]
        //[Category("Data")]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public new Object ToolTip
        {
            get
            {
                return base.ToolTip;
            }
            set
            {
                base.ToolTip = value;
            }
        }

        #endregion ToolTip

        #endregion Public Properties

        internal void UpdateActualDataPointsFill(int seriesIndex)
        {
            for (int pointIndex = 0; pointIndex < this.DataPoints.Count; pointIndex++)
            {
                this.DataPoints[pointIndex].UpdateActualFill(seriesIndex, pointIndex);
            }
        }

        internal void UpdateActualDataPointsStrokes(int seriesIndex)
        {
            for (int pointIndex = 0; pointIndex < this.DataPoints.Count; pointIndex++)
            {
                this.DataPoints[pointIndex].UpdateActualStroke(seriesIndex, pointIndex);
            }
        }

        private static void OnFillPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Series series = d as Series;

            XamChart control = XamChart.GetControl(d);

            if (series == null || control == null)
            {
                return;
            }

            // if it is performance rendering - recreate the whole chart
            bool isPerformance = control.IsPerformance();

            if (isPerformance)
            {
                OnPropertyChanged(d, e);

                return;
            }

            if (control.Legend != null && control.Legend.LegendPane != null)
            {
                control.Legend.LegendPane.Draw();
            }

            int seriesIndex = control.Series.IndexOf(series);

            series.UpdateActualDataPointsFill(seriesIndex);
        }

        private static void OnStrokePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Series series = d as Series;

            XamChart control = XamChart.GetControl(d);

            if (series == null || control == null)
            {
                return;
            }

            // if it is performance rendering - recreate the whole chart
            bool isPerformance = control.IsPerformance();

            if (isPerformance)
            {
                OnPropertyChanged(d, e);

                return;
            }

            if (control.Legend != null && control.Legend.LegendPane != null)
            {
                control.Legend.LegendPane.Draw();
            }

            int seriesIndex = control.Series.IndexOf(series);

            series.UpdateActualDataPointsStrokes(seriesIndex);
        }

        #region IWeakEventListener Members

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            this.bindingList_CollectionChanged(sender, e as NotifyCollectionChangedEventArgs);
            return true;
        }

        #endregion
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