using System;
using System.Threading.Tasks;

namespace NewsPaperReader
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("开始测试华西都市报网页分析...");
            var testAnalyzer = new TestWebAnalyzer();
            await testAnalyzer.TestHuaxiMetroDaily();
            Console.WriteLine("\n测试完成，按任意键退出...");
            Console.ReadKey();
        }
    }
}