using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsPaperReader
{
    internal class NewsPaperFactory
    {
        public static INewsPaper CreateNewsPaper(string type)
        {
            switch (type)
            {
                case "人民日报":
                    return new RenMinRiBao();
                default:
                    throw new ArgumentException("未知报纸类型","type");
            }
        }
    }
}
