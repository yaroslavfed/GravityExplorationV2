using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia.Media.Imaging;
using Client.Avalonia.Properties;
using Client.Core.Data;
using Client.Core.Enums;
using Client.Core.Services.ComputationalDomainService;
using Client.Core.Services.PlotService;
using Client.Core.Services.StratumService;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Client.Avalonia.Containers.PlotsContainer;

public class PlotsContainerViewModel : ViewModelBase
{
    private readonly IPlotService                _plotService;
    private readonly IComputationalDomainService _domainService;
    private readonly IStratumService             _stratumService;

    public PlotsContainerViewModel(
        IPlotService plotService,
        IComputationalDomainService domainService,
        IStratumService stratumService
    )
    {
        _plotService = plotService;
        _domainService = domainService;
        _stratumService = stratumService;

        SelectedProjection = EProjection.XY;
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
            .StratumsList
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
            .WhenAnyValue(vm => vm.Domain, vm => vm.Stratums, vm => vm.SelectedProjection)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(
                async void (args) =>
                {
                    IsLoading = true;
                    try
                    {
                        if (args is not
                        {
                            Item1:
                            {
                            },
                            Item2:
                            {
                            }
                        })
                            return;

                        if (args.Item1 == null)
                            return;

                        if (args.Item2 == null)
                            return;

                        var outputImage = await _plotService.GenerateChartAsync(args.Item1, args.Item2, args.Item3);
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
    public EProjection SelectedProjection { get; set; }

    [Reactive]
    public bool IsLoading { get; set; }
}