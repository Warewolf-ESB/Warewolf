namespace Technical_Assesment.Value_Objects
{
    public interface ImportBuilder<T>
    {
        T FromImportTokens(string[] parts);

        int TokenCnt();
    }
}
