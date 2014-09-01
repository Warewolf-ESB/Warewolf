namespace Dev2.Common.Interfaces.DataList.Contract
{
    /// <summary>
    /// List of operations a user can perform on system tags
    /// </summary>
    public enum enTranslationDepth
    {
       /* Take the shape */
       Shape,
       /* Take the data from the right, avoid overwriting existing data if present */
       Data, 
       /* Take the data from the right overwriting it all */
       Data_With_Blank_OverWrite
    }
}
