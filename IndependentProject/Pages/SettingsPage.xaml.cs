using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
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
    public sealed partial class SettingsPage : Page
    {
        public Page mainPage;

        public SettingsPage()
        {
            this.InitializeComponent();
        }

        private async void ClearHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            ///Access the Application Data storage folder
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;

            //try to get the samplefile
            StorageFile historyFile = await storageFolder.TryGetItemAsync("History.txt") as StorageFile;

            if (historyFile == null)
            {
                DialogTextBlock.Text = "There was no local history";
            }
            else
            {
                await historyFile.DeleteAsync();
                DialogTextBlock.Text = "History Cleared";
            }

            await HistoryDialog.ShowAsync();
        }

        private void DialogButton_Click(object sender, RoutedEventArgs e)
        {
            HistoryDialog.Hide();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            mainPage = e.Parameter as Page;

            if (mainPage.RequestedTheme == ElementTheme.Dark)
            {
                ThemeToggleSwitch.IsOn = true;
            }
        }

        private async void ThemeToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (ThemeToggleSwitch.IsOn)
            {
                mainPage.RequestedTheme = ElementTheme.Dark;
            }
            else
            {
                mainPage.RequestedTheme = ElementTheme.Light;
            }

            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile themeFile = await storageFolder.CreateFileAsync("Theme.txt", CreationCollisionOption.ReplaceExisting);

            string theme = mainPage.RequestedTheme.ToString();
            System.Diagnostics.Debug.WriteLine($"\tSaving {theme} to file");
            await FileIO.WriteTextAsync(themeFile, theme);
        }
        

        private async void CreateTestFilesButton_Click(object sender, RoutedEventArgs e)
        {
            //Create a new folderPicker
            FolderPicker folderPicker = new FolderPicker();
            //Have it start with the user's music library
            folderPicker.SuggestedStartLocation = PickerLocationId.MusicLibrary;
            //Show all folders
            folderPicker.FileTypeFilter.Add("*");
            //Wait for the user to pick a folder
            StorageFolder targetFolder = await folderPicker.PickSingleFolderAsync();

            //In this folder create some (empty) song files 
            if (targetFolder != null)
            {
                //Create new folder to hold files
                StorageFolder testFolder = await targetFolder.TryGetItemAsync("Test_MP3Metadata") as StorageFolder;

                //If the folder does not already exist
                if (testFolder == null)
                {
                    //Then create it
                    testFolder = await targetFolder.CreateFolderAsync("Test_MP3Metadata");

                    //Gets the test file from project files
                    //code retrieved from https://blogs.msdn.microsoft.com/metroapps/2012/07/15/access-your-application-assets-folder/ 6/5/18
                    string tf = @"Assets\MP3TestFile.mp3";
                    StorageFile testFile = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(tf);

                    //Repeatedly copy the file and change it's metadata

                    //A-Punk by Vampire Weekend
                    StorageFile firstFile = await testFile.CopyAsync(testFolder);
                    MusicProperties firstMP = await firstFile.Properties.GetMusicPropertiesAsync();
                    firstMP.Artist = "Vampire Weekend";
                    firstMP.Title = "A-Punk";
                    await firstMP.SavePropertiesAsync();
                    await firstFile.RenameAsync("first_Test.mp3");

                    //Don't Stop Believin' by Journey
                    StorageFile secondFile = await testFile.CopyAsync(testFolder);
                    MusicProperties secondMP = await secondFile.Properties.GetMusicPropertiesAsync();
                    secondMP.Artist = "Journey";
                    secondMP.Title = "Don't Stop Believin'";
                    await secondMP.SavePropertiesAsync();
                    await secondFile.RenameAsync("second_Test.mp3");

                    // 2 / 14 by The Band CAMINO
                    StorageFile thirdFile = await testFile.CopyAsync(testFolder);
                    MusicProperties thirdMP = await thirdFile.Properties.GetMusicPropertiesAsync();
                    thirdMP.Artist = "The Band CAMINO";
                    thirdMP.Title = "2 / 14";
                    await thirdMP.SavePropertiesAsync();
                    await thirdFile.RenameAsync("third_Test.mp3");

                    // Banana Pancakes by Jack Johnson
                    StorageFile fourthFile = await testFile.CopyAsync(testFolder);
                    MusicProperties fourthMP = await fourthFile.Properties.GetMusicPropertiesAsync();
                    fourthMP.Artist = "Jack Johnson";
                    fourthMP.Title = "Banana Pancakes";
                    await fourthMP.SavePropertiesAsync();
                    await fourthFile.RenameAsync("fourth_Test.mp3");

                    // Billie Jean by Michael Jackson
                    StorageFile fifthFile = await testFile.CopyAsync(testFolder);
                    MusicProperties fifthMP = await fifthFile.Properties.GetMusicPropertiesAsync();
                    fifthMP.Artist = "Michael Jackson";
                    fifthMP.Title = "Billie Jean";
                    await fifthMP.SavePropertiesAsync();
                    await fifthFile.RenameAsync("fifth_Test.mp3");

                    // MP3 File which shouldn't work
                    StorageFile sixthFile = await testFile.CopyAsync(testFolder);
                    MusicProperties sixthMP = await sixthFile.Properties.GetMusicPropertiesAsync();
                    sixthMP.Artist = "Not Real Artist";
                    sixthMP.Title = "Not Real Title";
                    await sixthMP.SavePropertiesAsync();
                    await sixthFile.RenameAsync("sixth_Test.mp3");
                }
            }
        }
    }
}
