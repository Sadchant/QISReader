using QISReader.ViewModel;
using QisReaderClassLibrary;
using System;
using System.Collections.Generic;
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

namespace QISReader.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BestandenPage : Page
    {
        private BestandenViewModel viewModel;
        public BestandenViewModel ViewModel { get; set; }

        private int gridWidth = 550;
        public GridLength GridWidth { get; } = new GridLength(550);

        public BestandenPage()
        {
            this.InitializeComponent();
            DataContextChanged += (s, e) =>
            {
                ViewModel = DataContext as BestandenViewModel;
            };
            viewModel = (BestandenViewModel)DataContext; // ja das muss so!
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            NotenDetails notenDetails = e.Parameter as NotenDetails;
            viewModel.Init(gridWidth, notenDetails.Verteilung);
        }
    }
}
