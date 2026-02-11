using System;
using System.Windows;

namespace NewsPaperReader
{
    class TestApp
    {
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Starting application...");
                var app = new App();
                app.InitializeComponent();
                app.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}