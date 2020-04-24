namespace WallpaperChanger
{
    public class MonitorInfos
    {
        public int Width { get; set; }
        public int Heigth { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public void ResetCoordinates(int xOffset, int yOffset)
        {
            this.X += xOffset;
            this.Y += yOffset;
        }
    }
}

