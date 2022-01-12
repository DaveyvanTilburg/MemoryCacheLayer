namespace MemoryCacheLayer.Domain.Repository
{
    public interface IRepositoryItem<out T> where T : struct, ICloneItem
    {
        int Id();
        T Clone();
        bool Equals(ICloneItem other);
    }
}