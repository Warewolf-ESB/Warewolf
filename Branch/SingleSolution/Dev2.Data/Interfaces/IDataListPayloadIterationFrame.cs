using Dev2.DataList.Contract.TO;

namespace Dev2.Data.Interfaces
{
    public interface IDataListPayloadIterationFrame<T>
    {

        /// <summary>
        /// Adds the specified exp.
        /// </summary>
        /// <param name="exp">The exp.</param>
        /// <param name="val">The val.</param>
        /// <returns></returns>
        bool Add(string exp, T val);

        /// <summary>
        /// Fetches the frame items.
        /// </summary>
        /// <returns></returns>
        DataListPayloadFrameTO<T> FetchNextFrameItem();


        /// <summary>
        /// Determines whether this instance has data.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance has data; otherwise, <c>false</c>.
        /// </returns>
        bool HasData();



    }
}
