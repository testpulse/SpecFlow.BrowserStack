﻿using System;
using System.Configuration;
using System.Diagnostics;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;
using TechTalk.SpecFlow;

namespace SpecFlow.BrowserStack
{
    [Binding]
    public class Setup
    {
        IWebDriver driver;

        [BeforeTestRun]
        public static void BeforeTestRun()
        {
            if (Process.GetProcessesByName("BrowserStackLocal").Length == 0)
            {
                new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "BrowserStackLocal.exe",
                        Arguments = ConfigurationManager.AppSettings["browserstack.key"] + " -forcelocal"
                    }
                }.Start();

                TestEnvironment.Current.Setup("TestWebApplication", "http://localhost:63064/");

                // Wait BrowserStack Local ready
                System.Threading.Thread.Sleep(15000);
            }
        } 

        [BeforeScenario]
        public void BeforeScenario()
        {
            
            var capabilities = new DesiredCapabilities();

            capabilities.SetCapability(CapabilityType.Version, ConfigurationManager.AppSettings["version"]);
            capabilities.SetCapability("os", ConfigurationManager.AppSettings["os"]);
            capabilities.SetCapability("os_version", ConfigurationManager.AppSettings["os_version"]);
            capabilities.SetCapability("browserName", ConfigurationManager.AppSettings["browser"]);
            
            capabilities.SetCapability("browserstack.user", ConfigurationManager.AppSettings["browserstack.user"]);
            capabilities.SetCapability("browserstack.key", ConfigurationManager.AppSettings["browserstack.key"]);
            capabilities.SetCapability("browserstack.local", true);

            //To enable visual logs browserstack.debug=true
            capabilities.SetCapability("browserstack.debug", true);

            capabilities.SetCapability("project", "BrowserStack Demo");
            capabilities.SetCapability("name", ScenarioContext.Current.ScenarioInfo.Title);

            driver = new RemoteWebDriver(new Uri(ConfigurationManager.AppSettings["browserstack.hub"]), capabilities);
            driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(1));
            ScenarioContext.Current["driver"] = driver;
        }

        [AfterScenario]
        public void AfterScenario()
        {
            driver.Dispose();
        }
    }
}
