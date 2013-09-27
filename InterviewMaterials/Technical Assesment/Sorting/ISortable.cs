namespace Technical_Assesment.Sorting
{
    public interface ISortable<T>
    {
        int CompareTo(T left, T right);
    }
}
