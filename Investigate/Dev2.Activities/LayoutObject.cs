using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2;
using Unlimited.Framework;

namespace Unlimited.Applications.BusinessDesignStudio.Activities {
    public class LayoutObject {
        public string WebPartServiceName { get; set; }
        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }
        public int RowSpan { get; set; }
        public int ColumnSpan { get; set; }
        public UnlimitedObject XmlConfiguration { get; set; }
        public string InputData { get; set; }
    }
}
