using NUnit.Framework;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace SpecFlow.BrowserStack
{
    [Binding]
    class LoginSteps
    {
        readonly IWebDriver _driver;

        public LoginSteps()
        {
            _driver = (IWebDriver)ScenarioContext.Current["driver"];
        }

        [When(@"I go to the Login page")]
        public void WhenIGoToTheLoginPage()
        {
            _driver.Navigate().GoToUrl("http://localhost:63064/Account/Login");
        }

        [Then(@"I should see a Log in button")]
        public void ThenIShouldSeeALogInButton()
        {
            Assert.That(_driver.FindElement(By.Id("btnLoginIn")).Text, Is.EqualTo("Log in"));
        }

    }
}
