namespace MemoryCacheLayer.Domain.Repository
{
    public interface IRepositoryItem
    {
        int Id();
        IRepositoryItem Clone();
        string Hash();
        bool Equals(IRepositoryItem other);
    }
}