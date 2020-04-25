# WallpaperChanger
This application lets you have a slideshow of pictures from different folders as your Windows wallpaper. Windows only allows you to select a single folder for a slideshow. It lets you define a seperate folder for every monitor you have. You should save pictures with the correct resoultion in those folders, while there is a rescaling method in there, I can not guarantee that the rescaled pictures will look good. To use this program just unpack the [zip](https://github.com/GuentherThomas/WallpaperChanger/releases/tag/v1.0) and fill the file named "config.txt" you fill up the rows with the full paths of the folders you want to use (open the folders in the file explorer and copy the path by clicking on the shown path). They should be written top to down in order from your left most monitor to your right most monitor (it is important how windows recognizes them, it always checks the leftmost pixels). Each path should have it's own line without any whitespaces. The next line should be the folder where you want to save the newly created wallpaper. The last line is the change interval you want in minutes (you can use decimal values with either ',' or '.'). The program has a TrayIcon from which you can close it any time.

Example for filling the config: Imagine you have 3 monitors, one 1080p on the left, 1440p in the middle and 2160p on the right

D:\Pics\1080p
D:\Pics\1440p
D:\Pics\2160p
D:\Pics\CustomWallpaper
10
