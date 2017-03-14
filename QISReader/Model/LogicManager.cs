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
        internal Scraper Scraper { get; set; }
        internal NotenParser NotenParser { get; set; }
        internal FachManager FachManager { get; set; }
        internal LoginDataSaver LoginDataSaver { get; set; }
        internal NotenDataSaver NotenDataSaver { get; set; }

        public LogicManager()
        {
            InitLogic();
        }

        public void InitLogic()
        {
            Scraper = new Scraper();
            NotenParser = new NotenParser();
            FachManager = new FachManager();
            LoginDataSaver = new LoginDataSaver();
            NotenDataSaver = new NotenDataSaver();
        }
    }
}
