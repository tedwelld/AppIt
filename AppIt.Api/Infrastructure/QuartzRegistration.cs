using AppIt.Api.Infrastructure.Jobs;
using Quartz;

namespace AppIt.Api.Infrastructure
{
    public static class QuartzRegistration
    {
        public static void AddAppItQuartz(this IServiceCollection services, IConfiguration config)
        {
            services.AddQuartz(q =>
            {
                q.UseSimpleTypeLoader();
                q.UseInMemoryStore();

                RegisterJob<EndOfDayJob>(q, "EndOfDayJob", "EndOfDayTrigger",
                    config.GetValue<bool>("Jobs:EnableEndOfDayJob"),
                    t => t.WithDailyTimeIntervalSchedule(s => s.OnEveryDay().StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(22, 30))));

                RegisterJob<FiscalJob>(q, "FiscalJob", "FiscalTrigger",
                    config.GetValue<bool>("Jobs:EnableFiscalJob"),
                    t => t.WithCronSchedule("0 */5 * * * ?"));

                RegisterJob<SimunyeJob>(q, "SimunyeJob", "SimunyeTrigger",
                    config.GetValue<bool>("Jobs:EnableSimunyeJob"),
                    t => t.WithCronSchedule("0 0/1 * * * ?"));

                RegisterJob<CurrencyExchangeRatesJob>(q, "CurrencyExchangeRatesJob", "CurrencyTrigger",
                    config.GetValue<bool>("Jobs:EnableCurrencySync"),
                    t => t.WithDailyTimeIntervalSchedule(s => s.OnEveryDay().StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(23, 0))));

                RegisterJob<SyncRoomsInventoryJob>(q, "SyncRoomsInventoryJob", "InventoryTrigger",
                    config.GetValue<bool>("Jobs:EnableInventorySync"),
                    t => t.WithCronSchedule("0 * 4-22 * * ?"));

                RegisterJob<Beds24ApiCallJob>(q, "Beds24ApiCallJob", "Beds24Trigger",
                    config.GetValue<bool>("Jobs:EnableBeds24"),
                    t => t.WithCronSchedule("0 */5 * * * ?"));

                RegisterJob<CacheRefreshJob>(q, "CacheRefreshJob", "CacheTrigger",
                    config.GetValue<bool>("Jobs:EnableCacheRefresh"),
                    t => t.WithSimpleSchedule(s => s.WithIntervalInMinutes(60).RepeatForever()));

                RegisterJob<HConnectSyncJob>(q, "HConnectSyncJob", "HConnectTrigger",
                    config.GetValue<bool>("Jobs:EnableHConnect"),
                    t => t.WithCronSchedule("0 */5 * * * ?"));
            });

            services.AddQuartzHostedService(o => o.WaitForJobsToComplete = true);
        }

        private static void RegisterJob<T>(
            IServiceCollectionQuartzConfigurator q,
            string jobKey,
            string triggerKey,
            bool enabled,
            Action<ITriggerConfigurator> schedule) where T : IJob
        {
            if (!enabled) return;
            q.AddJob<T>(opts => opts.WithIdentity(jobKey));
            q.AddTrigger(opts =>
            {
                opts.ForJob(jobKey).WithIdentity(triggerKey);
                schedule(opts);
            });
        }
    }
}
