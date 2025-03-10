namespace Client.Core.Services;

public interface IWriteable<in TData>
{
    Task Add(TData data);

    Task Update(TData data);

    Task Delete(Guid data);
}