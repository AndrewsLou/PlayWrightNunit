using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playwright.Example.Pages
{
    public class HomePage
    {
        public static string webAppUrl;
        private readonly IPage _page;
        private readonly ILocator _openFormbtn;
        private readonly ILocator _homeTitle;
        public HomePage(IPage page)
        {
            webAppUrl = TestContext.Parameters["WebAppUrl"]
                ?? throw new Exception("WebAppUrl is not configured.");
            _page = page;
            _openFormbtn = page.Locator("text=Open Contact Form");
            _homeTitle = page.Locator("text= Web app for testing with Playwright");
        }

        public async Task GotoAsync()
        {
            await _page.GotoAsync(webAppUrl);
        }

        public async Task OpenForm()
        {
            await _openFormbtn.ClickAsync();
        }
    }
}
