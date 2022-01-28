namespace MemoryCacheLayer.Domain.Repository
{
    public interface IRepositoryItem
    {
        int Id();
        IRepositoryItem CloneWithId(int value);
    }
}