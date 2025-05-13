using System.ComponentModel;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using ReactiveUI;

namespace Client.Avalonia.ViewModels;

public class ViewModelBase : ReactiveObject, IActivatableViewModel
{

    #region LifeCycle

    protected ViewModelBase()
    {
        Activator = new ViewModelActivator();
        this.WhenActivated(disposables =>
            {
                OnActivation(disposables);
                SetupPropertyChangedHandler();
                Disposable.Create(OnDeactivation).DisposeWith(disposables);
            }
        );
    }

    protected virtual Task OnActivation(CompositeDisposable disposables)
    {
        return Task.CompletedTask;
    }

    protected virtual void OnDeactivation()
    {
        DisposePropertyChangedHandler();
    }

    #endregion

    #region Properties

    public ViewModelActivator Activator { get; }

    #endregion

    #region Methods

    private void SetupPropertyChangedHandler()
    {
        PropertyChanged += OnPropertyChanged;
    }

    private void DisposePropertyChangedHandler()
    {
        PropertyChanged -= OnPropertyChanged;
    }

    protected virtual void OnPropertyChanged(object? sender, PropertyChangedEventArgs e) { }

    #endregion

}