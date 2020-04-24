using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace WallpaperChanger
{
    public class WallpaperChanger
    {
        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x02;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        List<string> WallpaperFolderPaths;
        List<int> CurrentIndeces = new List<int>();
        List<List<string>> WallpaperPaths = new List<List<string>>();
        List<MonitorInfos> MonitorInfos = new List<MonitorInfos>();
        Bitmap CustomWallpaper = new Bitmap(1,1);
        int CustomWidth = 0;
        int CustomHeigth = 0;
        int ChangeIntervall = 0;
        string CustomWallPaperFolderPath = string.Empty;
        const string ConifgPath = "config.txt";

        public void SetUp()
        {
            WallpaperFolderPaths = new List<string>();
            GetScreenDimension();
            ConfigSetup();
            HandleFolders();
          

            while (true)
            {
                Thread.Sleep(ChangeIntervall);
                OnTimer();
            }
        }

        private void ConfigSetup()
        {
            var configList = new List<string>();
            var uwWallpaperFolderPath = string.Empty;
            var portraitWallpaperFolderPath = string.Empty;
            try
            {
                using (StreamReader reader = new StreamReader(ConifgPath))
                {
                    var line = string.Empty;
                    while ((line = reader.ReadLine()) != null)
                    {
                        configList.Add(line);
                    }
                }

                configList = configList.Where(line => line != string.Empty).ToList();
                HandleConfig(configList);
            }
            catch
            {
                var thisProcess = Process.GetCurrentProcess();
                thisProcess.Kill();
            }
        }

        private void HandleConfig(List<string> configList)
        {
            var length = configList.Count;
            var i = 0;
            while(i < length - 2)
            {
                WallpaperFolderPaths.Add(configList[i]);
                i++;
            }
            CustomWallPaperFolderPath = Path.Combine(configList[i], "wallpaper.bmp");
            i++;
            if (configList[i].Contains('.'))
            {
                configList[i] = configList[i].Replace('.', ',');
            }
            bool canConvert = float.TryParse(configList[i], out float configIntervall);
            if (!canConvert)
            {
                configIntervall = 10;
            }
            if (configIntervall <= 0)
            {
                configIntervall = Math.Abs(configIntervall);
            }

            configIntervall *= 60;
            ChangeIntervall = (int)(configIntervall * 1000);
        }

        private void HandleFolders()
        {
            var i = 0;
            foreach(var folder in WallpaperFolderPaths)
            {
                if (!Directory.Exists(folder))
                {
                    var thisProcess = Process.GetCurrentProcess();
                    thisProcess.Kill();
                }
                WallpaperPaths.Add(new List<string>());
                CurrentIndeces.Add(0);
                var files = Directory.GetFiles(folder);
                foreach (var file in files)
                {
                    bool isValidImage;
                    try
                    {
                        var image = Image.FromFile(file);
                        isValidImage = true;
                    }
                    catch (OutOfMemoryException)
                    {
                        isValidImage = false;
                    }
                    if (isValidImage)
                    {
                        WallpaperPaths[i].Add(file);
                    }
                }
                WallpaperPaths[i] = Shuffle(WallpaperPaths[i]);
                i++;
            }
        }

        private List<string> Shuffle(List<string> list)
        {
            var seed = DateTime.Now.Millisecond;
            Random rng = new Random(seed);
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                string value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
         }

        private void OnTimer()
        {
            if (File.Exists(CustomWallPaperFolderPath))
            {
                File.Delete(CustomWallPaperFolderPath);
            }
            CreateWallpaper();
        }


        private void SetWallpaper(string path)
        {
            var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);

            key.SetValue(@"WallpaperStyle", "2"); //2 is tile
            key.SetValue(@"TileWallpaper", "2");

            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, path, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }

        private void CreateWallpaper()
        {
            var images = GatherImagesForNewWallpaper();
            CustomWallpaper = new Bitmap(CustomWidth, CustomHeigth);
            var g = Graphics.FromImage(CustomWallpaper);

            //set background color
            g.Clear(Color.Black);

            //go through each image and draw it on the final image
            var j = 0;
                foreach(var image in images)
                {
                    var x = MonitorInfos[j].X;
                    var y = MonitorInfos[j].Y;
                    var width = MonitorInfos[j].Width;
                    var height = MonitorInfos[j].Heigth;
                    if (image.Height != height || image.Width != width)
                {
                    ResizeImage(image, j);
                }
                    g.DrawImage(image, new Rectangle(x, y, width, height));
                    image.Dispose();
                    j++;
                }
            g.Dispose();
            CustomWallpaper.Save(CustomWallPaperFolderPath, ImageFormat.Bmp);

            CustomWallpaper.Dispose();
            SetWallpaper(CustomWallPaperFolderPath);
        }

        private List<Image> GatherImagesForNewWallpaper()
        {
            var images = new List<Image>();
            var i = 0;
            foreach (var folder in WallpaperPaths)
            {
                images.Add(Image.FromFile(folder[CurrentIndeces[i]]));
                if (CurrentIndeces[i] == folder.Count - 1)
                {
                    CurrentIndeces[i] = 0;
                }
                else
                {
                    CurrentIndeces[i]++;
                }
                i++;
            }

            return images;
        }

        private Image ResizeImage(Image toResize, int monitorIndex)
        {
            var width = MonitorInfos[monitorIndex].Width;
            var height = MonitorInfos[monitorIndex].Heigth;
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(toResize.HorizontalResolution, toResize.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
                    graphics.DrawImage(toResize, destRect, 0, 0, toResize.Width, toResize.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        private void GetScreenDimension()
        {
            var screens = Screen.AllScreens;
            CreateListOfMonitors(screens);
            GetCustomWidth(screens);
            GetCustomHeigth(screens);          
            MonitorInfos = MonitorInfos.OrderBy(m => m.X).ToList();
            SetNewCoordinates(MonitorInfos);
        }

        private void GetCustomHeigth(Screen[] displays)
        {
            displays = displays.OrderBy(s => s.Bounds.Top).ToArray();
            var lowestY = displays[0].Bounds.Top;
            displays = displays.OrderBy(s => s.Bounds.Bottom).ToArray();
            var highestY = displays[displays.Length - 1].Bounds.Bottom;
            CustomHeigth = highestY - lowestY;
        }

        private void GetCustomWidth(Screen[] displays)
        {
            displays = displays.OrderBy(s => s.Bounds.Left).ToArray();
            var lowestX = displays[0].Bounds.Left;
            displays = displays.OrderBy(s => s.Bounds.Right).ToArray();
            var highestX = displays[displays.Length - 1].Bounds.Right;
            CustomWidth = highestX - lowestX;
        }

        private void CreateListOfMonitors(Screen[] displays)
        {
            foreach(var display in displays)
            {
                var newMonitor = new MonitorInfos
                {
                    Width = display.Bounds.Width,
                    Heigth = display.Bounds.Height,
                    X = display.Bounds.X,
                    Y = display.Bounds.Y,
                };
                MonitorInfos.Add(newMonitor);
            }
        }

        private void SetNewCoordinates(List<MonitorInfos> monitors)
        {
            var xOffset = CalcualteXOffset(monitors); //maybe only handle negative changes
            var yOffset = CalcualteYOffset(monitors); ;

            foreach (var monitor in monitors)
            {
                monitor.ResetCoordinates(xOffset, yOffset);
            }
        }

        private int CalcualteXOffset(List<MonitorInfos> monitors)
        {
            return Math.Abs(monitors.First().X);
        }

        private int CalcualteYOffset(List<MonitorInfos> monitors)
        {
            int mostNegativeY = 0;
            foreach (var monitor in monitors)
            {
                if (monitor.Y < mostNegativeY)
                {
                    mostNegativeY = monitor.Y;
                }
            }
            return Math.Abs(mostNegativeY);
        }
    }
}

