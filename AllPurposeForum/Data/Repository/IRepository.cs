namespace AllPurposeForum.Data.Repository;

public interface IRepository<T, in TId> where TId : IEquatable<TId> where T : class
{
    Task<T?> GetByIdAsync(TId id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> CreateAsync(T obj);
    Task ReplaceAsync(T obj);
    Task UpsertAsync(T obj);
    Task DeleteAsync(T obj);
}