﻿using MuGet.Forms.Localisation;
using MuGet.Forms.Models;
using MuGet.Forms.Services;
using MvvmHelpers;
using MvvmHelpers.Commands;
using Plugin.StoreReview;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace MuGet.Forms.ViewModels
{
    public enum SettingItem
    {
        RateApp,
        SendFeedback,
        ViewGitHub,
        NuGet,
    }

    public class SettingsViewModel : BaseViewModel
    {
        private const string AndroidAppId = "com.dgatto.muget";
        private const string AppleAppId = "1489637407";
        private const string FeedbackEmail = "outtaapps@gmail.com";
        private const string GitHubRepo = "https://github.com/dmariogatto/muget";
        private const string NuGetUrl = "https://nuget.org/";

        private readonly IMuGetPackageService _muGetPackageService;

        public SettingsViewModel(
            IMuGetPackageService muGetPackageService,
            INuGetService nuGetService,
            ILogger logger) : base(nuGetService, logger)
        {
            Title = Resources.Settings;

            _muGetPackageService = muGetPackageService;
            MuGetPackages = new ObservableRangeCollection<MuGetPackage>(_muGetPackageService.GetPackages());

            SettingsItemTappedCommand = new AsyncCommand<SettingItem>(SettingsItemTappedAsync);
            PackageTappedCommand = new AsyncCommand<MuGetPackage>(async (p) =>
            {
                if (!string.IsNullOrEmpty(p?.PackageId))
                    await Launcher.TryOpenAsync($"muget://package/{p.PackageId}/");
            });
            ResetNotificationsCommand = new Command(ResetNotifications);
            RunJobsCommand = new AsyncCommand(RunJobsAsync);
        }

        public bool IncludePrerelease
        {
            get => NuGetService.IncludePrerelease;
            set
            {
                NuGetService.IncludePrerelease = value;
                OnPropertyChanged(nameof(IncludePrerelease));
            }
        }

        public bool NewReleaseNotifications
        {
            get => NuGetService.NewReleaseNotifications;
            set
            {
                NuGetService.NewReleaseNotifications = value;

                if (value)
                {
                    Shiny.ShinyHost.Resolve<Shiny.Jobs.IJobManager>().Schedule(ShinyStartup.NuGetJobInfo);
                }
                else
                {
                    Shiny.ShinyHost.Resolve<Shiny.Jobs.IJobManager>().Cancel(ShinyStartup.NuGetJobInfo.Identifier);
                }

                OnPropertyChanged(nameof(NewReleaseNotifications));
            }
        }

        public ObservableRangeCollection<MuGetPackage> MuGetPackages { get; private set; }

        public string Version => VersionTracking.CurrentVersion;
        public string Build => VersionTracking.CurrentBuild;

        public AsyncCommand<SettingItem> SettingsItemTappedCommand { get; private set; }
        public AsyncCommand<MuGetPackage> PackageTappedCommand { get; private set; }
        public Command ResetNotificationsCommand { get; private set; }
        public AsyncCommand RunJobsCommand { get; private set; }

        private async Task SettingsItemTappedAsync(SettingItem item)
        {
            switch (item)
            {
                case SettingItem.RateApp:
                    var id = DeviceInfo.Platform == DevicePlatform.iOS ? AppleAppId : AndroidAppId;
                    CrossStoreReview.Current.OpenStoreReviewPage(id);
                    break;
                case SettingItem.SendFeedback:
                    await SendFeedbackAsync();
                    break;
                case SettingItem.ViewGitHub:
                    await Browser.OpenAsync(GitHubRepo);
                    break;
                case SettingItem.NuGet:
                    await Browser.OpenAsync(NuGetUrl);
                    break;
            }
        }

        private async Task SendFeedbackAsync()
        {
            var message = new EmailMessage
            {
                Subject = string.Format("MuGet Feedback ({0})", DeviceInfo.Platform),
                Body = string.Empty,
                To = new List<string>(1) { FeedbackEmail },
            };

            const string gmailScheme = "googlegmail://";
            const string gmail = "Gmail";
            const string appleMail = "Mail";

            if (DeviceInfo.Platform == DevicePlatform.iOS &&
                await Launcher.CanOpenAsync(gmailScheme))
            {
                var option = await DisplayAction(Resources.SendFeedback, Resources.Cancel, null, gmail, appleMail);

                if (!string.IsNullOrEmpty(option))
                {
                    switch (option)
                    {
                        case gmail:
                            await Launcher.TryOpenAsync($"{gmailScheme}co?subject={message.Subject}&body={message.Body}&to={message.To.First()}");
                            break;
                        case appleMail:
                            await Email.ComposeAsync(message);
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                await Email.ComposeAsync(message);
            }
        }

        private void ResetNotifications()
        {
            var favourites = NuGetService.GetFavouritePackages();
            foreach (var f in favourites)
            {
                f.Version = string.Empty;
                f.Published = DateTime.MinValue;
                NuGetService.UpsertFavouritePackage(f);
            }
        }

        private async Task RunJobsAsync()
        {
            try
            {
                var results = await Shiny.ShinyHost.Resolve<Shiny.Jobs.IJobManager>().RunAll();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }
}
