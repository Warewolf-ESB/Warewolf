using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Dev2.DataList.Contract
{
    public class ActivityDataParser : IActivityDataParser {

        #region Attributes
        private readonly IList<IDev2Definition> _scalars;
        //private readonly IRecordSetCollection _recordSets;
        //private readonly IDataValue _systemTags;

        private ActivityMappingTO _to;

        #endregion

        #region Ctor

        internal ActivityDataParser() {
            _scalars = new List<IDev2Definition>();
            _to = new ActivityMappingTO();
        }

        #endregion

        #region Properties
        public ActivityMappingTO ParsedData {
            get {
                return _to;
            }
        }
        #endregion

        #region Public Methods
        public void ParseDataStream(string xmlData, IList<IDev2Definition> defs) {
            // do the heavy data parsing here, only extract data required as per the output definition mapping
            // HOLD ON, WHERE IS THE HEAVY DATA PARSING?!!!!!
            // init the collection
            _to = new ActivityMappingTO();

            _to.Recordsets = DataListFactory.CreateRecordSetCollection(defs);
            _to.Scalars = DataListFactory.CreateScalarList(defs);
            //_to.SystemRegion = new SystemTagsTO(DataListFactory.CreateDataListCompiler().TransferSystemTags(xmlData));

        }

        #endregion
        
    }
}
