using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using System.Text.RegularExpressions;

namespace Playwright.Example
{
    public class Hooks : PageTest 
    {
        public static string webAppUrl;

        public IPlaywright? PlaywrightContext { get; private set; }
        public IBrowser? Browser { get; private set; }

        [SetUp]
        public async Task Init()
        {
            webAppUrl = TestContext.Parameters["WebAppUrl"]
                ?? throw new Exception("WebAppUrl is not configured.");
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
