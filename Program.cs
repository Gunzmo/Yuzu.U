using System;
using System.Diagnostics;
using Gtk;

namespace yuzu.u
{
    class Program
    {
        static Process? Yuzu;
        [STAThread]
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
            Application.Init();
            var app = new Application("org.yuzu.updater", GLib.ApplicationFlags.NonUnique);
            app.Register(GLib.Cancellable.Current);
            var win = new MainWindow();
            app.AddWindow(win);
            win.Show();
            Application.Run();
            Yuzu = new();
            Yuzu.StartInfo.FileName = $"{Environment.CurrentDirectory}/yuzu.AppImage";
            if(args.Length > 0)
                foreach (var argument in args)
                    Yuzu.StartInfo.ArgumentList.Add(argument);
            Yuzu.Start();
            Yuzu.WaitForExit();
        }

        private static void CurrentDomain_ProcessExit(object? sender, EventArgs e) => Yuzu?.Kill();
    }
}
