using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Gtk;
using Extentions;

namespace yuzu.u
{
    class MainWindow : Window
    {
        static HttpClient client = new();
        progr ProgressValue {get; set;}
        [Builder.ObjectAttribute] private ProgressBar? _progress = null;

        public MainWindow() : this(new Builder("MainWindow.glade")) { }

        private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            ProgressValue = new progr();
            ProgressValue.SetProgress += setProggress;
            builder.Autoconnect(this);
            Task.Run(async () => {
                _progress.Text = "Checking for new Version";
                (string?, string?) VersionUrl = await GetVersionUrl();
                if(VersionUrl.Equals(default))
                {
                    Close();
                    Application.Quit();
                }
                    
                string Ver = string.Empty;
                if(File.Exists(Environment.CurrentDirectory + "/ver"))
                Ver = File.ReadAllText(Environment.CurrentDirectory + "/ver");
                if(Ver == VersionUrl.Item1)
                {
                    Close();
                    Application.Quit();
                    return;
                }
                _progress.Text = "Downloading";
                if(File.Exists(Environment.CurrentDirectory + "/yuzu.AppImage"))
                    File.Delete(Environment.CurrentDirectory + "/yuzu.AppImage");
                using (var file = new FileStream(Environment.CurrentDirectory + "/yuzu.AppImage", System.IO.FileMode.CreateNew, FileAccess.Write)) {
                    await client.DownloadAsync(VersionUrl.Item2, file, ProgressValue, new System.Threading.CancellationToken());
                }
                $"chmod +x {Environment.CurrentDirectory}/yuzu.AppImage".RunBash();
                File.Delete(Environment.CurrentDirectory + "/ver");
                File.WriteAllText(Environment.CurrentDirectory + "/ver", VersionUrl.Item1);
                Close();
                Application.Quit();
            });
        }

        private void setProggress(float obj) =>  _progress.Fraction = obj;

        class progr : IProgress<float>
        {
            public Action<float> SetProgress;
            public void Report(float value) => SetProgress?.Invoke(value);
        }

        static async Task<(string?, string?)> GetVersionUrl()
        {
            var client = new Octokit.GitHubClient(new Octokit.ProductHeaderValue("Yuzu-Updater"));
            var releases = await client.Repository.Release.GetLatest("pineappleEA", "pineapple-src");
            var LinuxRelease = releases.Assets.FirstOrDefault(x => x.Name.Contains(".AppImage", StringComparison.InvariantCulture) );
            if(releases.Equals(default)) return default;
            return(releases?.TagName, LinuxRelease.BrowserDownloadUrl);
        }
    }
}
