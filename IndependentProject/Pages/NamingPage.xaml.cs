using Id3;
using Id3.Id3v2;
using IndependentProject.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static IndependentProject.Models.Artist;
using static IndependentProject.Models.SearchTrackResponse;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace IndependentProject.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NamingPage : Page
    {
        private StorageFolder musicFolder;

        public NamingPage()
        {
            this.InitializeComponent();

        }


         
        


        //Button will check if path exists then navigate to LoadingPage
        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (musicFolder != null)    //if the BrowseButton was used
            {
                //Then continue updating
                UpdateSongFiles();
            }
            else
            {
                //Show a warning to use the browse button
                DialogTextBlock.Text = "Please use the browse button to choose a file";
                await WarningContentDialog.ShowAsync();
            }

        }

        private void UpdateSongFiles()
        {
            //code retrieved from https://github.com/jcoutch/id3-DotNetCore on 5/28/18
            //string[] musicFiles = Directory.GetFiles(folderFilePath);    //get all MP3s from the file

            System.Diagnostics.Debug.WriteLine("UpdateSongFiles: " + musicFolder.Path);

            this.Frame.Navigate(typeof(LoadingPage), musicFolder); //pass the musicFolder to LoadingPage

            /*  Left over from when filepath was passed to LoadingPage
            if (musicFiles != null)
            {
                System.Diagnostics.Debug.WriteLine("Music files not null, navigating to loading page");
                // Page page = new LoadingPage(musicFiles);
                //this.Frame.Navigate(typeof(Page));
                
                this.Frame.Navigate(typeof(LoadingPage), musicFolder);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("musicFiles was null on line 105 of LoadingPage.cs");
            }
            */
        }

        private void DialogOkayButton_Click(object sender, RoutedEventArgs e)
        {
            WarningContentDialog.Hide();
        }

        private async void BrowseButton_Click(object sender, RoutedEventArgs e)
        {

            FolderPicker fp = new FolderPicker();   //create a folder picker
            
            fp.SuggestedStartLocation = PickerLocationId.MusicLibrary;  //set the start location
            fp.FileTypeFilter.Add("*"); //means that this will allow all folders to be visible

            musicFolder = await fp.PickSingleFolderAsync(); //set the musicFolder to the selected folder

            if (musicFolder != null) //if a folder was chosen
            {
                FilePathTextBox.Text = musicFolder.Path;    //set the textbox to the correct path
            }

        }
    }

}