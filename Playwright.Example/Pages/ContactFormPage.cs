using System.Threading.Tasks;
using Microsoft.Playwright;

namespace Playwright.Example.Pages
{
    public class ContactFormPage
    {
        public static string webAppUrl;
        private readonly IPage _page;
        private readonly ILocator _firstNameTextbox;
        private readonly ILocator _lastNameTextbox;
        private readonly ILocator _emailTextbox;
        private readonly ILocator _birthDateSelector;
        private readonly ILocator _sendbtn;
        public ContactFormPage(IPage page) 
        {
            webAppUrl = TestContext.Parameters["WebAppUrl"]
                ?? throw new Exception("WebAppUrl is not configured.");
            _page = page;
            _firstNameTextbox = page.Locator("text=First name");
            _lastNameTextbox = page.Locator("text=Last name");
            _emailTextbox = page.Locator("text=Email Address");
            _birthDateSelector = page.Locator("text=Birth date");
            _sendbtn = page.Locator("text=Send");
        }

        public async Task GotoAsync()
        {
            await _page.GotoAsync($"{webAppUrl}Home/Form");
        }

        public async Task FillInForm(string firstName, string lastName, string email, string birthDate)
        {
            await _firstNameTextbox.FillAsync(firstName);
            await _lastNameTextbox.FillAsync(lastName);
            await _emailTextbox.FillAsync(email);
            await _birthDateSelector.FillAsync(birthDate);
        }

        public async Task SubmitForm()
        {
            await _sendbtn.ClickAsync();
        }

        public Task<bool> ValidationChecker(string errorMsg)
        {
            ILocator emailValidationLocator
                = _page.Locator($"text={errorMsg}");
            return emailValidationLocator.IsVisibleAsync();
        }
    }
}
