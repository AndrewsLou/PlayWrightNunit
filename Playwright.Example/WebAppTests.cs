using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using System.Text.RegularExpressions;

namespace Playwright.Example
{
    public class WebAppTests : Hooks
    {
        [Category("TraceIt")]
        [Test]
        public async Task WhenContactFormButtonIsClicked_UserIsTakenToContactForm()
        {
            await Page.GotoAsync(webAppUrl);
            var formButton = Page.Locator("text=Open Contact Form");
            await formButton.ClickAsync();
            await Expect(Page).ToHaveURLAsync(new Regex(".*Home/Form"));
        }

        [Category("TraceIt")]
        [Test]
        public async Task WhenUserFillsInContactForm_ItCanBeSubmitted()
        {
            await Page.GotoAsync($"{webAppUrl}Home/Form");
            await Page.Locator("text=First name").FillAsync("Gino");
            await Page.Locator("text=Last name").FillAsync("Da Campo");
            await Page.Locator("text=Email Address").FillAsync("test@gmail.com");
            await Page.Locator("text=Birth date").FillAsync("1994-04-21");
            await Page.Locator("text=Send").ClickAsync();
            await Expect(Page).ToHaveURLAsync(new Regex(".*Home/Success"));
        }

        [Category("TraceIt")]
        [Test]
        public async Task WhenUserInputsInvalidEmail_FormReturnsError()
        {
            await Page.GotoAsync($"{webAppUrl}Home/Form");

            ILocator emailValidationLocator 
                = Page.Locator("text=The Email address field is not a valid e-mail address.");
            await Expect(emailValidationLocator).Not.ToBeVisibleAsync();

            await Page.Locator("text=Email address").FillAsync("correctemail@email.com");
            await Page.Locator("text=Send").ClickAsync();

            await Expect(Page).ToHaveURLAsync(new Regex(".*Home/Form"));
            await Expect(emailValidationLocator).ToBeVisibleAsync();
        }

    }
}