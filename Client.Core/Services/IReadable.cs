namespace Client.Core.Services;

public interface IReadable<TData>
{
    Task<TData> Get(Guid id);

    Task<IReadOnlyList<TData>> GetAll();
}