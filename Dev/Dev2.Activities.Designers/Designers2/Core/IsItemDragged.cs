namespace Dev2.Activities.Designers2.Core
{
    public class IsItemDragged
    {

        #region Fields
        private static IsItemDragged _instance;
        bool _isDragged;

        #endregion

        #region Ctor

        public IsItemDragged()
        {
            IsDragged = false;
        }

        #endregion

        #region Properties

        public static IsItemDragged Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new IsItemDragged();
                }
                return _instance;
            }
        }

        public bool IsDragged
        {
            get
            {
                return _isDragged;
            }
            set
            {
                _isDragged = value;
            }
        }

        #endregion
    }
}
