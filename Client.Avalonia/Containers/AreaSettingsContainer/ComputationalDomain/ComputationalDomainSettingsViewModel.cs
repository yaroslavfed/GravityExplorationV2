using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.ReactiveUI;
using Client.Avalonia.Properties;
using Client.Core.Data;
using Client.Core.Services.ComputationalDomainService;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Client.Avalonia.Containers.AreaSettingsContainer.ComputationalDomain;

public class ComputationalDomainSettingsViewModel : ViewModelBase
{
    private readonly IComputationalDomainService _domainService;

    public ComputationalDomainSettingsViewModel(IComputationalDomainService domainService)
    {
        _domainService = domainService;
        SaveDomainCommand = ReactiveCommand.CreateFromTask(
            SaveComputationalDomain,
            outputScheduler: AvaloniaScheduler.Instance
        );
    }

    protected override void OnActivation(CompositeDisposable disposables)
    {
        base.OnActivation(disposables);
        _domainService
            .Domain
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(
                domain =>
                {
                    ComputationalDomain = domain;
                    Debug.WriteLine(domain);
                }
            )
            .DisposeWith(disposables);
    }

    public string StartCoordinateLabel { get; set; } = "Нижняя граница сетки:";

    public string EndCoordinateLabel { get; set; } = "Верхняя граница сетки:";

    public string SplittingParamsLabel { get; set; } = "Кол-во ячеек:";

    public string MultyParamsLabel { get; set; } = "Коэффициент разрядки:";

    [Reactive]
    public Domain? ComputationalDomain { get; private set; }

    public ReactiveCommand<Unit, Unit> SaveDomainCommand { get; }

    private Task SaveComputationalDomain()
    {
        _domainService.UpdateAsync(ComputationalDomain!);
        return Task.CompletedTask;
    }
}