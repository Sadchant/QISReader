using QisReaderClassLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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
    public sealed partial class EinstellungenPage : Page
    {
        public delegate void EventMethod();
        public static event EventMethod LogoutEvent;

        public EinstellungenPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Frame.BackStack.LastOrDefault() kann null sein wenn die app suspended wird
            if (Frame.BackStack.LastOrDefault() == null || Frame.BackStack.LastOrDefault().SourcePageType.Equals(typeof(LoginPage)))
            {
                Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = Windows.UI.Core.AppViewBackButtonVisibility.Collapsed;
                return;
            }
            if (Frame.CanGoBack)
            {
                // wird auf Mobile/im Tabletmode nicht beachtet
                Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = Windows.UI.Core.AppViewBackButtonVisibility.Visible;
            }
            else
            {
                Windows.UI.Core.SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = Windows.UI.Core.AppViewBackButtonVisibility.Collapsed;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LoginDataSaver loginDataSaver = new LoginDataSaver();
            loginDataSaver.Logout();
            LogoutEvent();
        }

        private async void AutoUpdateSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ApplicationData.Current.LocalSettings.Values[GlobalValues.SETTINGS_AUTOUPDATE] = AutoUpdateSwitch.IsOn;
            await App.LogicManager.UpdateData.UpdateTrigger();
        }

        private async void UpdateRateComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (UpdateRateComboBox.SelectedIndex)
            {
                case 0:
                    ApplicationData.Current.LocalSettings.Values[GlobalValues.SETTINGS_UPDATERATE] = GlobalValues.UPDATERATE_ALLE_30_MINUTEN;
                    break;
                case 1:
                    ApplicationData.Current.LocalSettings.Values[GlobalValues.SETTINGS_UPDATERATE] = GlobalValues.UPDATERATE_EINMAL_PRO_STUNDE;
                    break;
                case 2:
                    ApplicationData.Current.LocalSettings.Values[GlobalValues.SETTINGS_UPDATERATE] = GlobalValues.UPDATERATE_ALLE_2_STUNDEN;
                    break;
                case 3:
                    ApplicationData.Current.LocalSettings.Values[GlobalValues.SETTINGS_UPDATERATE] = GlobalValues.UPDATERATE_ALLE_6_STUNDENN;
                    break;
                case 4:
                    ApplicationData.Current.LocalSettings.Values[GlobalValues.SETTINGS_UPDATERATE] = GlobalValues.UPDATERATE_EINMAL_PRO_TAG;
                    break;               
            }
            await App.LogicManager.UpdateData.UpdateTrigger();
        }

       
    }
}
