using System;
using System.Diagnostics;
using System.IO;

namespace SpecFlow.BrowserStack
{
    public class TestEnvironment
    {
        private static TestEnvironment _instance;

        private Process _iis;

        public static TestEnvironment Current
        {
            get
            {
                return _instance ?? (_instance = new TestEnvironment());
            }
        }

        public bool HideIIS { get; private set; }
        public string WebAppProjectName { get; private set; }
        public string WebAppUrl { get; private set; }

        public TestEnvironment()
        {
        }

        public void Setup(string webAppProjectName, string webAppUrl, bool hideIIS = true, bool startWebSiteNow = true)
        {
            this.HideIIS = hideIIS;
            this.WebAppProjectName = webAppProjectName;
            this.WebAppUrl = webAppUrl;

            if(startWebSiteNow)
            {
                this.StartWebsite();
            }
        }

        public void StartWebsite()
        {
            this.StopWebsite();

            if (string.IsNullOrEmpty(this.WebAppProjectName))
            {
                return;
            }

            Uri uri;
            if (!Uri.TryCreate(this.WebAppUrl, UriKind.RelativeOrAbsolute, out uri))
            {
                Debug.WriteLine(
                        "[Error] Failed to parse website url config value{0}{1}",
                        Environment.NewLine,
                        this.WebAppUrl);

                return;
            }

            var port = uri.Port;
            var path = GetApplicationPath(this.WebAppProjectName);
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

            var psi = new ProcessStartInfo
            {
                FileName = string.Format("{0}\\IIS Express\\iisexpress.exe", programFiles),
                Arguments = string.Format("/path:\"{0}\" /port:{1}", path, port),
                WindowStyle =
                                      this.HideIIS
                                              ? ProcessWindowStyle.Hidden
                                              : ProcessWindowStyle.Normal,
                ErrorDialog = true,
                CreateNoWindow = this.HideIIS,
                UseShellExecute = false
            };

            try
            {
                this._iis = new Process { StartInfo = psi };
                this._iis.Start();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[Error] Failed to start iisexpress.exe{0}{1}", System.Environment.NewLine, ex);
                this.StopWebsite();
            }
        }

        public void StopWebsite()
        {
            if (this._iis == null || this._iis.HasExited)
            {
                return;
            }

            try
            {
                if (this.HideIIS)
                {
                    // If we don't have a window, we have to 'kill' the process.
                    this._iis.Kill();
                }
                else
                {
                    this._iis.CloseMainWindow();
                    this._iis.Dispose();
                }

                if (this._iis.HasExited || this._iis.WaitForExit(5000))
                {
                    return;
                }

                throw new TimeoutException("iisexpress.exe is taking longer than expected to exit");
            }
            catch (InvalidOperationException ioe)
            {
                // If we no processs is associated => process already exited.
                if (!ioe.Message.Contains("No process is associated"))
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[Error] Failed to stop iisexpress.exe{0}{1}", System.Environment.NewLine, ex);
                try
                {
                    // Try to kill the process again if it hasn't exited yet.
                    this._iis.Kill();
                }
                catch
                {
                    Debug.WriteLine("iisexpress.exe already exited");
                }
            }
            finally
            {
                this._iis = null;
            }
        }

        private static string GetApplicationPath(string webAppName)
        {
            var webAppFolderPath = System.IO.Path.GetFullPath(webAppName);

            if (Directory.Exists(webAppFolderPath))
            {
                return webAppFolderPath;
            }
            else
            {
                var solutionFolder =
                    Path.GetDirectoryName(
                            Path.GetDirectoryName(Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)));

                return Path.Combine(solutionFolder, webAppName);
            }
        }
    }
}
