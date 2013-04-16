using System;
using System.Collections.Generic;
using System.Activities;


namespace Unlimited.Applications.BusinessDesignStudio.Activities {

    public class DsfRemoteActivity : DsfActivityAbstract<bool> {
        public string ServiceAddress { get; set; }

        protected override bool CanInduceIdle {
            get {
                return true;
            }
        }

        public DsfRemoteActivity() : base() {}

        //IDSFDataObject dataObject;


        protected override void CacheMetadata(NativeActivityMetadata metadata) {
            base.CacheMetadata(metadata);
            //metadata.AddDelegate(_delegate);
        }

        protected override void OnExecute(NativeActivityContext context) {
            throw new NotImplementedException("Nothing here...");
        }


        //private void Resumed(NativeActivityContext context, Bookmark bookmark, object value) {
        //    XElementEnumerator enumerator = new XElementEnumerator();

        //    IDSFDataObject dto = context.GetExtension<IDSFDataObject>();

        //    IDataListBinder binder = context.GetExtension<IDataListBinder>();

        //    var dataList = AmbientDataList.Get(context);

        //    dataList.Add(value.ToString());

        //    var dataObject = binder.DataListToUnlimitedObject(dataList);

        //    var keyValuePairs = from p in new XElementEnumerator().EnumerateXElementTree(dataObject.xmlData)
        //                        group p by p.Key into g
        //                        select new KeyValuePair<string, string>(g.Key, (from t2 in g select t2.Value).Last());


        //    var newDataList = new UnlimitedObject("xData");
        //    keyValuePairs.ToList().ForEach(c => newDataList.GetElement(c.Key).SetValue(c.Value));


        //    AmbientDataList.Set(context, new List<string>() { newDataList.XmlString });
        //}       

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            throw new NotImplementedException();
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            throw new NotImplementedException();
        }
    }
}
