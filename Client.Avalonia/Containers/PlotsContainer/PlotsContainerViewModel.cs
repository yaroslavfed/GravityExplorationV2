using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Media.Imaging;
using Client.Avalonia.Properties;
using Client.Core.Data;
using Client.Core.Services;
using Client.Core.Services.ComputationalDomainService;
using Client.Core.Services.PlotService;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Client.Avalonia.Containers.PlotsContainer;

public class PlotsContainerViewModel : ViewModelBase
{
    private readonly IPlotService                _plotService;
    private readonly IComputationalDomainService _domainService;
    private readonly IHandlerService<Stratum>    _stratumService;

    public PlotsContainerViewModel(
        IPlotService plotService,
        IComputationalDomainService domainService,
        IHandlerService<Stratum> stratumService
    )
    {
        _plotService = plotService;
        _domainService = domainService;
        _stratumService = stratumService;
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
                    Domain = null;
                    Domain = domain;
                }
            )
            .DisposeWith(disposables);

        _stratumService
            .UpdatedData
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(
                list =>
                {
                    Stratums = null;
                    Stratums = list;
                }
            )
            .DisposeWith(disposables);

        this
            .WhenAnyValue(vm => vm.Domain, vm => vm.Stratums)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(
                async void (args) =>
                {
                    try
                    {
                        if (args is not { Item1: { }, Item2: { } })
                            return;

                        var outputImage = await _plotService.GenerateChartAsync(args.Item1, args.Item2);
                        ChartImage = new(outputImage);
                    } catch (Exception e)
                    {
                        // ReSharper disable once AsyncVoidMethod
                        throw new(e.Message);
                    }
                }
            )
            .DisposeWith(disposables);
    }

    [Reactive]
    public Bitmap? ChartImage { get; private set; }

    [Reactive]
    public IReadOnlyList<Stratum>? Stratums { get; set; }

    [Reactive]
    public Domain? Domain { get; set; }
}