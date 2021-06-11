namespace MemoryCacheLayer.Sql
{
    public interface IDatabaseItem<T> where T : class, IDatabaseItem<T>, new()
    {
        int Id();
        bool Equals(T other);
        T Clone();
    }
}