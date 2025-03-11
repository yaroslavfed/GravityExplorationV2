using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Client.Avalonia.Properties;
using Client.Core.Holders;
using Avalonia.ReactiveUI;
using Client.Avalonia.Data;
using ReactiveUI;

namespace Client.Avalonia.Containers.StratumsListContainer;

public class StratumsListContainerViewModel : ViewModelBase
{
    private readonly IHandlerService<StratumDto> _handlerService;

    public StratumsListContainerViewModel(IHandlerService<StratumDto> handlerService)
    {
        _handlerService = handlerService;

        StratumsList = _handlerService.UpdatedData;

        CreateStratumCommand = ReactiveCommand.CreateFromTask(
            CreateStratumAsync,
            outputScheduler: AvaloniaScheduler.Instance
        );

        UpdateStratumCommand = ReactiveCommand.CreateFromTask<StratumDto>(
            UpdateStratumAsync,
            outputScheduler: AvaloniaScheduler.Instance
        );

        RemoveStratumCommand = ReactiveCommand.CreateFromTask<Guid>(
            RemoveStratumAsync,
            outputScheduler: AvaloniaScheduler.Instance
        );
    }

    public IObservable<IReadOnlyList<StratumDto>> StratumsList { get; set; }

    public ReactiveCommand<Unit, Unit> CreateStratumCommand { get; }

    public ReactiveCommand<StratumDto, Unit> UpdateStratumCommand { get; }

    public ReactiveCommand<Guid, Unit> RemoveStratumCommand { get; }

    private async Task CreateStratumAsync()
    {
        var stratum = new StratumDto
        {
            Id = Guid.NewGuid(),
            Dimensions = new() { Center = new() { X = 0, Y = 0, Z = 0 }, Bounds = new() { X = 1, Y = 1, Z = 1 } }
        };

        await _handlerService.AddAsync(stratum);
    }

    private async Task UpdateStratumAsync(StratumDto stratum)
    {
        await _handlerService.UpdateAsync(stratum);
    }

    private async Task RemoveStratumAsync(Guid id)
    {
        await _handlerService.RemoveAsync(id);
    }
}