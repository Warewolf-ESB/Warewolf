namespace Dev2.ViewModels.Deploy
{
    public class DeployDialogTO
    {
        #region Properties

        public string SourceName { get; private set; }
        public string DestinationName { get; private set; }
        public int RowNumber { get; private set; }

        #endregion

        #region Ctor

        public DeployDialogTO(int rowNumber, string sourceName, string destinationName)
        {
            RowNumber = rowNumber;
            SourceName = sourceName;
            DestinationName = destinationName;
        }

        #endregion
    }
}