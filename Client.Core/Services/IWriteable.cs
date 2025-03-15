namespace Client.Core.Services;

public interface IWriteable<in TData>
{
    Task AddAsync(TData data);

    Task RemoveAsync(Guid data);
}