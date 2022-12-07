using Microsoft.Playwright;
using Playwright.Example.Pages;
using System.Text.RegularExpressions;

namespace Playwright.Example.Tests
{
    [Parallelizable(ParallelScope.Self)]
    [TestFixture]
    public class WebAppTests : Hooks
    {
        [Category("TraceIt")]
        [Test]
        public async Task WhenContactFormButtonIsClicked_UserIsTakenToContactForm()
        {
            var _homePage = new HomePage(page);
            await _homePage.GotoAsync();
            await _homePage.OpenForm();
            await Expect(page).ToHaveURLAsync(new Regex(".*Home/Form"));
        }

        [Category("TraceIt")]
        [Test]
        public async Task WhenUserFillsInContactForm_ItCanBeSubmitted()
        {
            var _contactFormPage = new ContactFormPage(page);
            await _contactFormPage.GotoAsync();
            await _contactFormPage.FillInForm("Louis", "Andrews", "email@email.com", "1998-06-18");
            await _contactFormPage.SubmitForm();
            await Expect(page).ToHaveURLAsync(new Regex(".*Home/Success"));
        }

        [Category("TraceIt")]
        [Test]
        public async Task WhenUserInputsInvalidEmail_FormReturnsError()
        {
            var _contactFormPage = new ContactFormPage(page);
            await _contactFormPage.GotoAsync();
            Assert.False(await _contactFormPage.ValidationChecker("The Email address field is not a valid e-mail address."));
            await _contactFormPage.FillInForm("Louis", "Andrews", "incorrectemail.com", "1998-06-18");
            await _contactFormPage.SubmitForm();
            await Expect(page).ToHaveURLAsync(new Regex(".*Home/Form"));
            Assert.True(await _contactFormPage.ValidationChecker("The Email address field is not a valid e-mail address."));
        }

    }
}