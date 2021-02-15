using PuppeteerSharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace AutoAbsenSKI
{
    public class Processor
    {
        private static readonly string SettingsPath = Path.Combine(AppContext.BaseDirectory, "settings.json");
        public JsonSettings _settings;

        public bool IsValidState() 
        {
            return _settings.IsValidState();
        }

        public async Task Initialize()
        {
            _settings = JsonSettings.Load(SettingsPath);
            if (!File.Exists(_settings.ChromiumPath))
            {
                Console.WriteLine();
                await DownloadChromium();
            }
        }

        public void OpenSettings()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = SettingsPath,
                UseShellExecute = true,
                Verb = "open"
            });
        }

        public async Task DownloadChromium()
        {
            Console.WriteLine(MessageResources.DownloadChromiumStart);

            var fetcher = new BrowserFetcher();
            var result = await fetcher.DownloadAsync(BrowserFetcher.DefaultRevision);
            _settings.ChromiumPath = result.ExecutablePath;
            JsonSettings.Save(_settings, SettingsPath);
        }

        public async Task<byte[]> GenerateReport()
        {
            Console.WriteLine(MessageResources.GenerateReportLaunching);
            var opt = new LaunchOptions
            {
                Headless = true,
                Args = new[] { "--no-sandbox" },
                DefaultViewport = new ViewPortOptions
                {
                    Width = _settings.Viewport.Width,
                    Height = _settings.Viewport.Height
                },
                ExecutablePath = _settings.ChromiumPath
            };

            using var browser = await Puppeteer.LaunchAsync(opt);
            using var page = await browser.NewPageAsync();

            Console.WriteLine(MessageResources.GenerateReportLoggingIn);
            await page.GoToAsync("https://telkomdds.atlassian.net", WaitUntilNavigation.Networkidle0);

            await page.TypeAsync("#username", _settings.AtlassianAccount.Email);
            await page.ClickAsync("#login-submit");
            await page.WaitForTimeoutAsync(3 * 1000);
            await page.TypeAsync("#password", _settings.AtlassianAccount.Password);
            await page.ClickAsync("#login-submit");
            await page.WaitForTimeoutAsync(5 * 1000);

            Console.WriteLine(MessageResources.GenerateReportScreenshot);
            await page.GoToAsync("https://telkomdds.atlassian.net/issues/?filter=11241");
            await page.WaitForTimeoutAsync(3 * 1000);
            var img = await page.ScreenshotDataAsync();


            Console.WriteLine(MessageResources.GenerateReportLoggingOut);
            await page.GoToAsync("https://telkomdds.atlassian.net/logout", 5 * 1000, new[] { WaitUntilNavigation.Networkidle0 });

            await page.CloseAsync();
            await browser.CloseAsync();

            await SaveReport(img);

            return img;
        }

        public async Task SendEmail(byte[] attachment)
        {
            Console.WriteLine(MessageResources.SendEmailComposing);
            var sc = new SmtpClient
            {
                Host = _settings.EmailAccount.Host,
                Port = _settings.EmailAccount.Port,
                EnableSsl = _settings.EmailAccount.Ssl,
                Credentials = new NetworkCredential(_settings.EmailAccount.Email, _settings.EmailAccount.Password)
            };

            (string lastPeriod, string currentPeriod) = GetDatePeriod();
            var mail = new MailMessage
            {
                From = new MailAddress(_settings.EmailAccount.Email),
                Subject = $"Absen SKI - {_settings.EmployeeName}",
                Body = $"Absen SKI atas nama {_settings.EmployeeName} periode {lastPeriod} s.d. {currentPeriod}"
            };

            using var ms = new MemoryStream(attachment);
            var attach = new Attachment(ms, "absen.jpg", MediaTypeNames.Image.Jpeg);
            mail.Attachments.Add(attach);

            foreach (var recipient in _settings.Recipients)
            {
                mail.To.Add(new MailAddress(recipient));
            }

            Console.WriteLine(MessageResources.SendEmailSending);
            await sc.SendMailAsync(mail);

            Console.WriteLine(MessageResources.GenerateReportLoggingIn, DateTime.Now);
        }

        public async Task InstallTaskScheduler()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.Error.WriteLine(MessageResources.InstallSchedulerNotCompatible);
                return;
            }

            Console.WriteLine(MessageResources.InstallSchedulerInstalling);
            var errorStreamDelegate = new DataReceivedEventHandler((sender, e) => Console.Error.WriteLine(e.Data));
            var executablePath = Path.Combine(AppContext.BaseDirectory, "autoabsenski.exe");
            
            using var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "schtasks.exe",
                    Arguments = $"/create /tn AutoAbsenSKI /tr \"{executablePath}\" /sc MONTHLY /d 15 /st 10:00 /f",
                    UseShellExecute = false,
                    RedirectStandardError = true
                }
            };

            p.ErrorDataReceived += errorStreamDelegate;
            p.Start();
            p.BeginErrorReadLine();
            await p.WaitForExitAsync();
            p.ErrorDataReceived -= errorStreamDelegate;

            Console.WriteLine(p.ExitCode != 0 ? MessageResources.InstallSchedulerError : MessageResources.InstallSchedulerSuccess);
        }

        // ----- Helper Methods

        private static (string lastPeriod, string currentPeriod) GetDatePeriod()
        {
            var currentDate = DateTime.Now;
            var lastMonthDate = currentDate.AddMonths(-1);

            string MonthToString(int month) => month switch
            {
                1 => "Januari",
                2 => "Februari",
                3 => "Maret",
                4 => "April",
                5 => "Mei",
                6 => "Juni",
                7 => "Juli",
                8 => "Agustus",
                9 => "September",
                10 => "Oktober",
                11 => "November",
                12 => "Desember",
                _ => ""
            };

            return ($"{MonthToString(lastMonthDate.Month)} {lastMonthDate.Year}", $"{MonthToString(currentDate.Month)} {currentDate.Year}");
        }

        private static async Task SaveReport(byte[] data)
        {
            (string lastPeriod, string currentPeriod) = GetDatePeriod();
            var filename = $"report-{lastPeriod}-{currentPeriod}";
            await File.WriteAllBytesAsync(Path.Combine(AppContext.BaseDirectory, filename), data);
        }
    }
}
