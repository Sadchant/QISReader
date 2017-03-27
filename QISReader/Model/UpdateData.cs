using QisReaderBackground;
using QisReaderClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;

namespace QISReader.Model
{
    public class UpdateData
    {
        public delegate void EventMethod();
        public event EventMethod StartDataUpdate;
        public event EventMethod DataUpdated;

        private const string BACKGROUNDTASKID = "UpdateDataBackgroundTask";

        public async Task UpdateTrigger()
        {
            RemoveTrigger(); //beim Updaten immer zunächst entfernen (da es ja entweder ausgeschaltet wurde oder ein neues UpdateInterval gesetzt -> trigger eh ersetzen)
            await InitTrigger(); // danach neuen Trigger setzen
        }

        public async Task InitTrigger()
        {            
            if ((bool)ApplicationData.Current.LocalSettings.Values[GlobalValues.SETTINGS_AUTOUPDATE]) // sollte das updaten auf true gesetzt sein, setze den Trigger mit aktuellen Update-Rate
            {
                TimeTrigger timeTrigger = new TimeTrigger((uint)ApplicationData.Current.LocalSettings.Values[GlobalValues.SETTINGS_UPDATERATE], false); // das false steht für: es soll nicht nur einmal wiederholt werden
                BackgroundTaskRegistration task = await BackgroundTaskManager.RegisterBackgroundTask(typeof(UpdateDataBackground).ToString(), BACKGROUNDTASKID, timeTrigger, null);
                AttachProgressAndCompletedHandlers(task);
            }

        }
        private void RemoveTrigger()
        {
            foreach (var t in BackgroundTaskRegistration.AllTasks)
            {
                BackgroundTaskRegistration x = (BackgroundTaskRegistration)t.Value;
                if (x.Name.Equals(BACKGROUNDTASKID))
                    x.Unregister(true);
            }
        }

        private void OnProgress(IBackgroundTaskRegistration task, BackgroundTaskProgressEventArgs args)
        {
            if (StartDataUpdate != null)
                StartDataUpdate();
        }

        private void OnCompleted(IBackgroundTaskRegistration task, BackgroundTaskCompletedEventArgs args)
        {
            if (DataUpdated != null)
                DataUpdated();
        }

        private void AttachProgressAndCompletedHandlers(IBackgroundTaskRegistration task)
        {
            task.Completed += new BackgroundTaskCompletedEventHandler(OnCompleted);
        }
    }
}
