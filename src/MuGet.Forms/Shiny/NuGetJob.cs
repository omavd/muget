﻿using MuGet.Forms.Services;
using Shiny.Jobs;
using Shiny.Notifications;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MuGet.Forms
{
    public class NuGetJob : IJob
    {
        private readonly INuGetService _nuGetService;
        private readonly INotificationManager _notifications;

        public NuGetJob(INuGetService nuGetService, INotificationManager notifications)
        {
            _nuGetService = nuGetService;
            _notifications = notifications;
        }

        public async Task<bool> Run(JobInfo jobInfo, CancellationToken cancelToken)
        {
            try
            {
                await _notifications.Send(new Notification()
                {
                    Id = 66,
                    Title = "Running background fetch",
                    Message = DateTime.Now.ToString("g"),
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            var favouritePackages = _nuGetService.GetFavouritePackages();

            if (favouritePackages.Any())
            {
                var includePrerelease = _nuGetService.IncludePrerelease;
                foreach (var fp in favouritePackages)
                {
                    var catalogEntries = await _nuGetService.GetCatalogEntries(fp.PackageId, cancelToken);
                    var latest = catalogEntries.FirstOrDefault(e => includePrerelease || !e.PackVersion.IsPrerelease);

                    if (latest != null &&
                        latest.Published > fp.Published)
                    {
                        fp.Version = latest.Version;
                        fp.Published = latest.Published;

                        _nuGetService.UpsertFavouritePackage(fp);

                        try
                        {
                            await _notifications.Send(new Notification()
                            {
                                Id = fp.Id.GetHashCode(),
                                Title = $"{fp.PackageId} ({fp.Version})",
                                Message = $"by {latest.Authors}, published {fp.Published.ToShortDateString()}",
                            });
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(ex);
                        }
                    }                    
                }

                return true;
            }
                        
            return false;
        }
    }
}