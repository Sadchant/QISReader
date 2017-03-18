using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace QisReaderBackground.BackgroundTasks
{
    class UpdateDataBackground : IBackgroundTask
    {
        BackgroundTaskDeferral _deferral; // wird benötigt, um async-Code innerhalb eines Background-Tasks auszuführen
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();

            Scraper scraper = new Scraper();

            _deferral.Complete();
        }
    }
}
