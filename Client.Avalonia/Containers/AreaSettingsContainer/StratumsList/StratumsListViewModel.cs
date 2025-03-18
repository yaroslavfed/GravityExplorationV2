using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.ReactiveUI;
using Client.Avalonia.Properties;
using Client.Core.Data;
using Client.Core.Services;
using ReactiveUI;

namespace Client.Avalonia.Containers.AreaSettingsContainer.StratumsList;

public class StratumsListViewModel : ViewModelBase
{
    private readonly IHandlerService<Stratum>      _handlerService;
    private readonly ObservableCollection<Stratum> _stratumsList = [];

    public StratumsListViewModel(IHandlerService<Stratum> handlerService)
    {
        _handlerService = handlerService;

        CreateStratumCommand = ReactiveCommand.CreateFromTask(
            CreateStratumAsync,
            outputScheduler: AvaloniaScheduler.Instance
        );
        UpdateStratumCommand = ReactiveCommand.CreateFromTask<Stratum>(
            UpdateStratumAsync,
            outputScheduler: AvaloniaScheduler.Instance
        );
        RemoveStratumCommand = ReactiveCommand.CreateFromTask<Guid>(
            RemoveStratumAsync,
            outputScheduler: AvaloniaScheduler.Instance
        );

        StratumsList = new(_stratumsList);
    }

    protected override void OnActivation(CompositeDisposable disposables)
    {
        base.OnActivation(disposables);
        _handlerService
            .UpdatedData
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(UpdateCollection)
            .DisposeWith(disposables);
    }

    public ReadOnlyObservableCollection<Stratum> StratumsList { get; }

    public ReactiveCommand<Unit, Unit> CreateStratumCommand { get; }

    public ReactiveCommand<Stratum, Unit> UpdateStratumCommand { get; }

    public ReactiveCommand<Guid, Unit> RemoveStratumCommand { get; }

    private async Task CreateStratumAsync()
    {
        await _handlerService.AddAsync(new() { Id = Guid.NewGuid() });
    }

    private async Task UpdateStratumAsync(Stratum stratum)
    {
        await _handlerService.UpdateAsync(stratum);
    }

    private async Task RemoveStratumAsync(Guid id)
    {
        await _handlerService.RemoveAsync(id);
    }

    private void UpdateCollection(IReadOnlyList<Stratum>? source)
    {
        if (source == null)
            return;

        var stratumsList = _stratumsList.ToList();

        foreach (var oldItem in stratumsList)
        {
            if (stratumsList.Contains(oldItem))
                _stratumsList.Remove(oldItem);
        }


        foreach (var newItem in source)
            _stratumsList.Add(newItem);
    }
}