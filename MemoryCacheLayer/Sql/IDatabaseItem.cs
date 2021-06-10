namespace MemoryCacheLayer.Sql
{
    public interface IDatabaseItem<T> where T : IDatabaseItem<T>
    {
        int Id();
        bool Equals(T other);
        T Clone();
    }
}