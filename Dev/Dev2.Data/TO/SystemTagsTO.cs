using Dev2.Data.Interfaces;

namespace Dev2.DataList.Contract
{
    public class SystemTagsTO : IDataListInjectionContents {

        private readonly string _payload;

        public SystemTagsTO(string payload) {
            _payload = payload;
        }

        public bool IsSystemRegion {
            get {
                return true;
            }
        }

        public string ToInjectableState(){
            return _payload;
        }

    }
}
