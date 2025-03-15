namespace Client.Core.Services;

public interface IEditable<in TData>
{
    Task UpdateAsync(TData data);
}