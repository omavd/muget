﻿using Microsoft.Extensions.DependencyInjection;
using MuGet.Forms.Services;
using MuGet.Forms.ViewModels;
using Shiny;
using Shiny.Jobs;

namespace MuGet.Forms
{
    public class ShinyStartup : Shiny.ShinyStartup
    {
        public readonly static JobInfo NuGetJobInfo = new JobInfo(typeof(NuGetJob), nameof(NuGetJob))
        {
            BatteryNotLow = true,
            DeviceCharging = false,
            RequiredInternetAccess = InternetAccess.Any,
            Repeat = true
        };

        public override void ConfigureServices(IServiceCollection services)
        {
            // register your shiny services here
            services.UseNotifications<NotificationDelegate>(false);

            services.AddSingleton<ILogger, Logger>();
            services.AddSingleton<ICacheProvider, InMemoryCache>();
            services.AddSingleton<INuGetService, NuGetService>();
            services.AddSingleton<IMuGetPackageService, MuGetPackageService>();

            services.AddSingleton<MainViewModel>();
            services.AddSingleton<HomeViewModel>();
            services.AddSingleton<PackageViewModel>();
            services.AddSingleton<SettingsViewModel>();
        }

    }
}
