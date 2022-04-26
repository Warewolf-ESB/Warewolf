namespace Dev2.DataList.Contract.Binary_Objects
{
    public class ResumableExecutionContainerFactory : IResumableExecutionContainerFactory
    {
        public IResumableExecutionContainer New(Guid startActivityId, ServiceAction sa, DsfDataObject dataObject)
        {
            return new ResumableExecutionContainer(startActivityId, sa, dataObject);
        }

        public IResumableExecutionContainer New(Guid startActivityId, ServiceAction sa, DsfDataObject dataObject,
            IWorkspace workspace)
        {
            return new ResumableExecutionContainer(startActivityId, sa, dataObject, workspace);
        }
    }
}