using QisReaderBackground;
using QisReaderClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace QISReader.Model
{
    class ReadQis
    {
        public delegate void EventMethod();
        public event EventMethod StartAnmeldungEvent;
        public event EventMethod WrongLoginEvent;
        public event EventMethod KeineVerbindungEvent;
        public event EventMethod LoginFehlerEvent;

        public event EventMethod StartNotenNavigationEvent;
        public event EventMethod NotenNavigationsFehlerEvent;

        public event EventMethod StartNotenVerarbeitungEvent;
        public event EventMethod NotenVerarbeitungFehlerEvent;

        public event EventMethod NotenVerarbeitungFertigEvent;

        public event EventMethod BackgroundTaskFertigEvent;

        public delegate void NotenDetailsProgressEventMethod(int progress);
        public event NotenDetailsProgressEventMethod NotenDetailsProgressEvent;

        private ApplicationTrigger trigger;

        public async Task InitReadQis()
        {
            trigger = new ApplicationTrigger();
            BackgroundTaskRegistration task = await BackgroundTaskManager.RegisterBackgroundTask(typeof(ReadQisBackground).ToString(), "ReadQisBackgroundTaskManager", trigger, null);
            AttachProgressAndCompletedHandlers(task);
        }

        public async Task StartReadQis()
        {
            await trigger.RequestAsync();
        }


        private async void OnCompleted(IBackgroundTaskRegistration task, BackgroundTaskCompletedEventArgs args)
        {
            if (BackgroundTaskFertigEvent != null)
                BackgroundTaskFertigEvent();
        }

        // wird aufgerufen, wenn der zu task gehörige HintergrundProzess den progress ändert
        private void OnProgress(IBackgroundTaskRegistration task, BackgroundTaskProgressEventArgs args)
        {
            if (args.Progress >= 200) // wenn es über 200 ist, ist es NotenDetails-Fortschrittsprozentzahl
            {
                if (NotenDetailsProgressEvent != null)
                    NotenDetailsProgressEvent((int)args.Progress);                
                return;
            }
            // Ansonsten sind es Zahlen, die eine Botschaft bedeuten, es werden Events gefeuert, an die sich Gui-Klassen anmelden können
            switch (args.Progress)
            {
                case GlobalValues.STARTANMELDUNG:
                    StartAnmeldungEvent();
                    break;
                case GlobalValues.STARTNOTENNAVIGATION:
                    StartNotenNavigationEvent();
                    break;
                case GlobalValues.STARTNOTENVERARBEITUNG:
                    StartNotenVerarbeitungEvent();
                    break;
                case GlobalValues.NOTENVERARBEITUNGFERTIG:
                    NotenVerarbeitungFertigEvent();
                    break;

                //Fehler
                case GlobalValues.WRONGLOGIN:
                    WrongLoginEvent();
                    break;
                case GlobalValues.KEINEVERBINDUNG:
                    KeineVerbindungEvent();
                    break;
                case GlobalValues.LOGINFEHLER:
                    LoginFehlerEvent();
                    break;
                case GlobalValues.NOTENNAVIGATIONSFEHLER:
                    NotenNavigationsFehlerEvent();
                    break;
                case GlobalValues.NOTENVERARBEITUNGFEHLER:
                    NotenVerarbeitungFehlerEvent();
                    break;
            }
        }

        private void AttachProgressAndCompletedHandlers(IBackgroundTaskRegistration task)
        {
            task.Progress += new BackgroundTaskProgressEventHandler(OnProgress);
            task.Completed += new BackgroundTaskCompletedEventHandler(OnCompleted);
        }
    }
}
