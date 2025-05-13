using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Client.Avalonia.ViewModels;
using Client.Core.Data;
using Client.Core.Enums;
using Client.Core.Services.ComputationalDomainService;
using Client.Core.Services.PlotService;
using Client.Core.Services.SensorsService;
using Client.Core.Services.StratumService;
using Common.Data;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Client.Avalonia.Containers.PlotsContainer;

public class PlotsContainerViewModel : ViewModelBase
{
    private readonly IMeshPlotHelper             _meshPlotHelper;
    private readonly IComputationalDomainService _domainService;
    private readonly IStratumService             _stratumService;
    private readonly ISensorsService             _sensorsService;

    public PlotsContainerViewModel(
        IMeshPlotHelper meshPlotHelper,
        IComputationalDomainService domainService,
        IStratumService stratumService,
        ISensorsService sensorsService
    )
    {
        _meshPlotHelper = meshPlotHelper;
        _domainService = domainService;
        _stratumService = stratumService;
        _sensorsService = sensorsService;

        SelectedProjection = EProjection.XY;
        IsSensorsGridTurnedOn = false;
    }

    protected override async Task OnActivation(CompositeDisposable disposables)
    {
        await base.OnActivation(disposables);

        _domainService
            .Domain
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(domain =>
                {
                    Domain = null;
                    Domain = domain;
                }
            )
            .DisposeWith(disposables);

        _stratumService
            .StratumsList
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(list =>
                {
                    Stratums = null;
                    Stratums = list;
                }
            )
            .DisposeWith(disposables);

        _sensorsService
            .SensorsGrid
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(sensorsGrid =>
                {
                    SensorsGrid = null;
                    SensorsGrid = sensorsGrid;
                }
            )
            .DisposeWith(disposables);

        this
            .WhenAnyValue(
                vm => vm.Domain,
                vm => vm.Stratums,
                vm => vm.SensorsGrid,
                vm => vm.IsSensorsGridTurnedOn,
                vm => vm.SelectedProjection
            )
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(async void (args) =>
                {
                    IsDirty = true;
                    IsLoading = true;
                    try
                    {
                        if (args is not
                        {
                            Item1:
                            { },
                            Item2:
                            { },
                            Item3:
                            { }
                        })
                            return;

                        if (args.Item3 == null)
                            return;

                        var outputImage = await _meshPlotHelper.GenerateChartAsync(args.Item3, args.Item4, args.Item5);
                        ChartImage = new(outputImage);
                    } catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка: {ex.Message}");
                    } finally
                    {
                        IsLoading = false;
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

    [Reactive]
    public SensorsGrid? SensorsGrid { get; set; }

    [Reactive]
    public bool IsSensorsGridTurnedOn { get; set; }

    [Reactive]
    public EProjection SelectedProjection { get; set; }

    [Reactive]
    public bool IsLoading { get; set; }

    [Reactive]
    public bool IsDirty { get; set; }
}