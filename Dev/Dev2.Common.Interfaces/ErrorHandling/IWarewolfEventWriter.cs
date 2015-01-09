namespace Dev2.Common.Interfaces.ErrorHandling
{
    /// <summary>
    /// class that writes application events to something like the event log
    /// </summary>
    public interface IWarewolfEventWriter
    {
        /// <summary>
        /// pushes an event 
        /// </summary>
        /// <param name="evt"></param>
        void Push(IWarewolfApplicationEvent evt);
    }
}