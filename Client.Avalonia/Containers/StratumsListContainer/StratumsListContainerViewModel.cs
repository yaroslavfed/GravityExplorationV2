using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using Client.Avalonia.Properties;
using Client.Core.Data;
using Client.Core.Data.Entities;
using Client.Core.Holders;
using System.Reactive.Concurrency;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Client.Avalonia.Containers.StratumsListContainer;

public class StratumsListContainerViewModel : ViewModelBase
{
    private readonly IHolderService<Stratum>                 _holderService;
    private readonly BehaviorSubject<IReadOnlyList<Stratum>> _stratumsList = new([]);

    public StratumsListContainerViewModel(IHolderService<Stratum> holderService)
    {
        _holderService = holderService;

        _holderService.DataList.ObserveOn(RxApp.MainThreadScheduler).ToPropertyEx(this, vm => vm.StratumsList);

        AddCommand = ReactiveCommand.CreateFromTask(AddStratum);
    }

    protected override void OnActivation(CompositeDisposable disposables)
    {
        base.OnActivation(disposables);
    }

    [ObservableAsProperty]
    public IReadOnlyList<Stratum> StratumsList { get; set; }

    public ReactiveCommand<Unit, Unit> AddCommand { get; }

    private async Task AddStratum()
    {
        var stratum = new Stratum
        {
            Id = Guid.NewGuid(),
            Dimensions = new() { Center = new() { X = 0, Y = 0, Z = 0 }, Bounds = new() { X = 1, Y = 1, Z = 1 } }
        };

        await _holderService.Add(stratum);
    }
}