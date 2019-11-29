﻿using MvvmHelpers;
using MvvmHelpers.Commands;
using MuGet.Forms.Localisation;
using MuGet.Forms.Models;
using MuGet.Forms.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms.StateSquid;

namespace MuGet.Forms.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private const int DefaultPageSize = 20;

        private readonly SemaphoreSlim _searchSemaphore = new SemaphoreSlim(1, 1);
        private CancellationTokenSource _searchCancellation;

        public MainViewModel()
        {
            Title = Resources.Search;

            Packages = new ObservableRangeCollection<PackageMetadata>();
            SearchCommand = new AsyncCommand<CancellationToken>((ct) => Search(SearchText, 0, ct));
            RemainingItemsThresholdReachedCommand = new AsyncCommand(() =>
                _searchCancellation != null
                    ? Search(SearchText, Packages.Count, _searchCancellation.Token)
                    : Task.FromResult(0));
            PackageTappedCommand = new AsyncCommand<PackageMetadata>(PackageTapped);
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value?.Trim());

                _searchCancellation?.Cancel();
                _searchCancellation?.Dispose();
                _searchCancellation = null;

                _searchCancellation = new CancellationTokenSource();
                SearchCommand.ExecuteAsync(_searchCancellation.Token);
            }
        }

        private int _remainingItemsThreshold = -1;
        public int RemainingItemsThreshold
        {
            get => _remainingItemsThreshold;
            set => SetProperty(ref _remainingItemsThreshold, value);
        }

        public ObservableRangeCollection<PackageMetadata> Packages { get; private set; }

        private int _totalHits;
        public int TotalHits
        {
            get => _totalHits;
            set => SetProperty(ref _totalHits, value);
        }

        public AsyncCommand<CancellationToken> SearchCommand { get; private set; }
        public AsyncCommand RemainingItemsThresholdReachedCommand { get; private set; }
        public AsyncCommand<PackageMetadata> PackageTappedCommand { get; private set; }

        private async Task Search(string searchText, int skip, CancellationToken cancellationToken)
        {
            RemainingItemsThreshold = -1;

            await _searchSemaphore.WaitAsync();

            if (SearchText == searchText && !cancellationToken.IsCancellationRequested)
            {
                IsBusy = true;

                try
                {
                    var packages = default(IList<PackageMetadata>);
                    var totalHits = 0;

                    if (!string.IsNullOrWhiteSpace(searchText))
                    {
                        var searchResult = await NuGetService.Search(searchText, skip, DefaultPageSize, cancellationToken);
                        packages = searchResult.Item2;
                        totalHits = searchResult.Item1;
                    }
                    else
                    {
                        packages = new List<PackageMetadata>(0);
                    }

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        if (skip > 0)
                        {
                            // Stop iOS exploding with an NSInternalInconsistencyException
                            foreach (var p in packages)
                            {
                                Packages.Add(p);
                            }
                        }
                        else
                        {
                            RemainingItemsThreshold = 5;

                            Packages.Clear();
                            if (packages?.Any() == true)
                                Packages.ReplaceRange(packages);
                        }

                        TotalHits = totalHits;
                        RemainingItemsThreshold = packages.Count == 0 ? -1 : 5;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
                finally
                {
                    IsBusy = false;
                }
            }

            _searchSemaphore.Release();
        }

        private async Task PackageTapped(PackageMetadata package)
        {
            if (IsBusy)
                return;

            IsBusy = true;

            if (package != null)
            {
                try
                {
                    NuGetService.AddRecentPackage(new RecentPackage()
                    {
                        PackageId = package.Id,
                        IndexUrl = package.IndexUrl,
                        SourceUrl = string.Empty,
                        SortOrder = 0,
                    });
                    await Shell.GoToAsync($"{PackagePage.RouteName}?{PackagePage.PackageIdUrlQueryProperty}={WebUtility.UrlEncode(package.Id)}&{PackagePage.VersionQueryProperty}={WebUtility.UrlEncode(package.Version)}");
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }

            IsBusy = false;
        }
    }
}