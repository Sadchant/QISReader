using QISReader.Model;
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
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace QISReader.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DatenPage : Page
    {
        private DatenViewModel viewModel;
        public DatenViewModel ViewModel { get; set; }

        public DatenPage()
        {
            this.InitializeComponent();
            DataContextChanged += (s, e) =>
            {
                ViewModel = DataContext as DatenViewModel;
            };
            viewModel = (DatenViewModel)DataContext; // ja das muss so!
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            NotenDetails notenDetails = e.Parameter as NotenDetails;
            viewModel.Init(notenDetails.DatenBeschriftung, notenDetails.DatenInhalt, notenDetails.Überschrift);
        }
    }
}
