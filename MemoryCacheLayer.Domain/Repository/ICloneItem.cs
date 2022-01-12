namespace MemoryCacheLayer.Domain.Repository
{
    public interface ICloneItem
    {
        int Id();
        string Hash();
    }
}