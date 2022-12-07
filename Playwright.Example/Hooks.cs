using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Playwright.Example
{
    public class Hooks : PageTest 
    {
        public IPlaywright? PlaywrightContext { get; private set; }
        public IBrowser? browser { get; private set; }
        public IPage? page { get; private set; }

        [SetUp]
        public async Task Init()
        {
            browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });

            page = await browser.NewPageAsync();
            var properties = TestContext.CurrentContext.Test.Properties;

            if (properties.ContainsKey("Category") &&
                (string)properties.Get("Category")! == "TraceIt")
            {
                await Context.Tracing.StartAsync(
                    new TracingStartOptions
                    {
                        Screenshots = true,
                        Snapshots = true,
                        Sources = true,
                        Name = TestContext.CurrentContext.Test.FullName
                    });
            }

            var startInfo = new ProcessStartInfo()
            {
                FileName = "powershell.exe",
                Arguments = @";.\RunWebApp.ps1",
                UseShellExecute = false
            };
            Process.Start(startInfo);
        }

        [TearDown]
        public async Task TearDownAsync() 
        {
            PlaywrightContext?.Dispose();
            var properties = TestContext.CurrentContext.Test.Properties;
            var testOutcome = TestContext.CurrentContext.Result.Outcome.ToString();

            if (properties.ContainsKey("Category") &&
                (string)properties.Get("Category")! == "TraceIt" &&
                testOutcome.ToLower().Contains("failed"))
            {
                await Context.Tracing.StopAsync(
                    new TracingStopOptions
                    {
                        Path = TestContext.CurrentContext.Test.FullName + ".zip"
                    });
            }
        }

        public class BrowserCollection : Hooks
        {
            // This class has no code, and is never created. Its purpose is simply
            // to be the place to apply [CollectionDefinition] and all the
            // ICollectionFixture<> interfaces.
        }
    }
}
