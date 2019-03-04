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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace IndependentProject.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HistoryPage : Page
    {
        public HistoryPage()
        {
            this.InitializeComponent();
        }

        public HistoryPage(string addedSongs)   //this will only be called by the finish button on the NamingPage
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {

            HistoryTextBlock.Text = await GetHistory();
            
        }

        private async Task<string> GetHistory()
        {

            //Access the Application Data storage folder
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;

            //try to get the samplefile
            StorageFile historyFile = await storageFolder.TryGetItemAsync("History.txt") as StorageFile;
            
            //if it doesn't exist
            if (historyFile == null)
            {
                return "There is no history to show";
            }

            //Getting the text
            string history = await FileIO.ReadTextAsync(historyFile);
            return history;
        }
    }
}
