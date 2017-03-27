using QISReader.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace QISReader
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VerteilungsPage4 : Page
    {
        private VerteilungsViewModel viewModel;
        public VerteilungsViewModel ViewModel { get; set; }

        private float gridHeight = 300; // aus dem Grid im Kopf zusammengerechnet: Row 2 + 3 + 4
        private float anzahlBeschriftungAbstandnachOben = 7; // Abstand zwischen Anzahl Beschriftung und oberen Balkenende
        private float anzahlBeschriftungHeight = 15; // Höhe der Anzahlbeschriftung (bei Fontsize 15 ist die Schrift in etwa 11 hoch)

        private static int firstGridHeight = 40;
        public GridLength FirstGridHeight = new GridLength(firstGridHeight);

        public VerteilungsPage4()
        {
            this.InitializeComponent();
            DataContextChanged += (s, e) =>
            {
                ViewModel = DataContext as VerteilungsViewModel;
            };
            viewModel = (VerteilungsViewModel)DataContext; // ja das muss so!
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            bool has3rows = false;
            VerteilungsPageNavigationsArgs navigationArgs = e.Parameter as VerteilungsPageNavigationsArgs;
            viewModel.InsertValues(navigationArgs.Verteilung, navigationArgs.Durchschnitt, navigationArgs.EigeneNote);
            viewModel.Init(has3rows, navigationArgs.MaxNotenSpiegelBeschriftungsNumber, gridHeight, anzahlBeschriftungAbstandnachOben, anzahlBeschriftungHeight, firstGridHeight, Resources);
        }
    }
}
