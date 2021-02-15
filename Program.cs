using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace AutoAbsenSKI
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand
            {
                new Option<bool>(new string[] { "--settings" , "-s" }, MessageResources.SettingsCommandDescription),
                new Option<bool>(new string[] { "--install", "-i" }, MessageResources.InstallSchedulerCommandDescription),
                new Option<bool>(new string[] { "--dry-run" , "-d" }, MessageResources.DryRunCommandDescription),
                new Option<bool>(new string[] { "--no-headless" , "-n" }, MessageResources.DryRunCommandDescription),
            };

            rootCommand.Description = MessageResources.RootCommandDescription;
            rootCommand.Handler = CommandHandler.Create<bool, bool, bool, bool>(async (settings, install, dryRun, noHeadless) =>
            {
                try
                {
                    var processor = new Processor();

                    Console.WriteLine(MessageResources.RootStatusLoading);
                    processor.Initialize();

                    if (settings)
                    {
                        processor.OpenSettings();
                        return 0;
                    }

                    await processor.DownloadChromium();

                    if (!processor.IsValidState())
                    {
                        Console.WriteLine(MessageResources.RootStatusInvalidState);
                        return -1;
                    }

                    if (install)
                    {
                        await processor.InstallTaskScheduler();
                        return 0;
                    }

                    Console.WriteLine("### Generate and Send Report ###");
                    var report = await processor.GenerateReport(!noHeadless);

                    if (!dryRun)
                    {
                        await processor.SendEmail(report);
                    }
                    
                    return 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(MessageResources.RootException);
                    Console.WriteLine(ex.ToString());
                    return -1;
                }
            });

            return await rootCommand.InvokeAsync(args);
        }
    }
}
