using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using WallpaperChanger.Properties;

namespace WallpaperChanger
{
    public class MyApplicationContext : ApplicationContext
    {

        private NotifyIcon _trayIcon;

        public MyApplicationContext()
        {
            _trayIcon = new NotifyIcon
            {
                Icon = Resources.AppIcon,
                ContextMenu = new ContextMenu(new MenuItem[]
                {
                    new MenuItem("Exit", Exit)
                }),
                Visible = true,
                Text = "WallpaperChanger"
            };
            RunProgrammAsync();
        }

        private void StartApp()
        {
            var program = new WallpaperChanger();
            program.SetUp();
        }

        private async Task RunProgrammAsync()
        {
            await Task.Run(async () =>
            {
                StartApp();
            });
        }

        private void Exit(object sender, EventArgs e)
        {
            _trayIcon.Visible = false;
            var thisProcess = Process.GetCurrentProcess();
            thisProcess.Kill();
        }
    }
}
