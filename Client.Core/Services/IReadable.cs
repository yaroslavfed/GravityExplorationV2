namespace Client.Core.Services;

public interface IReadable<TData>
{
    Task<TData> GetAsync(Guid id);

    Task<IReadOnlyList<TData>> GetAllAsync();
}