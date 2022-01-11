namespace MemoryCacheLayer.Sql
{
    public interface IDatabaseItem<out T> where T : struct, ICloneItem
    {
        int Id();
        bool Equals(ICloneItem other);
        T Clone();
    }
}