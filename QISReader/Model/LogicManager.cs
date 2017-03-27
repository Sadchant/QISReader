using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QISReader.Model
{
    public class LogicManager
    {
        // internal weil public geht nicht...
        internal ReadQis ReadQis { get; set; }
        internal UpdateData UpdateData { get; set; }

        public LogicManager()
        {
            ReadQis = new ReadQis();
            UpdateData = new UpdateData();
        }

        public async Task InitLogic()
        {
            await ReadQis.InitReadQis();
        }
    }
}
