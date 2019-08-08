using System;
using System.Diagnostics;


namespace csharp_gbwallpaper
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Process.Start("Data\\rePaper-Unity.exe", "-popupwindow -screen-width 640 -screen-height 360");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
