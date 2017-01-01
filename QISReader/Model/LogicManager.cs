using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QISReader.Model
{
    public class LogicManager
    {
        internal Scraper Scraper { get; set; }
        internal NotenParser NotenParser { get; set; }
        internal FachManager FachManager { get; set; }

        public LogicManager()
        {
            Scraper = new Scraper();
            NotenParser = new NotenParser();
            FachManager = new FachManager();
        }
    }
}
