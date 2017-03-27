using QisReaderClassLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace QISReader.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoadDetailsPage : Page
    {
        public LoadDetailsPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            bool idNotFound = (bool)e.Parameter;
            if (idNotFound)
            {
                Statustext.Text = "Fehler beim Laden des Notenspiegels";
                ProgressBar.Visibility = Visibility.Collapsed;
                Prozenttext.Visibility = Visibility.Collapsed;
            }
            else
            {
                Statustext.Text = "Lade alle NotenSpiegel";
                ProgressBar.Maximum = 100;
                App.LogicManager.ReadQis.NotenDetailsProgressEvent += UpdateProgress;
            }
        }

        private async void UpdateProgress(int prozent)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ProgressBar.Value = prozent - GlobalValues.NOTENSPIEGELPROGRESSSTART; // der NotenSpiegelProgressstart geht ja von 200 bis 300
                Prozenttext.Text = (prozent - GlobalValues.NOTENSPIEGELPROGRESSSTART).ToString();
            });
        }
    }
}
