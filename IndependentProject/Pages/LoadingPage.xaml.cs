using Id3;
using Id3.Frames;
using IndependentProject.Models_View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static IndependentProject.Models.SearchTrackResponse;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace IndependentProject.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoadingPage : Page
    {
        private Boolean shouldContinue = true;   //this will be checked before each song

        public LoadingPage()
        {
            this.InitializeComponent();
            System.Diagnostics.Debug.WriteLine("LoadingPage initialized sucessfully with parameters");

            FinishButton.IsEnabled = false;
        }

        private async void UpdateHistory()
        {
            //code for this method retrieved from https://docs.microsoft.com/en-us/windows/uwp/files/quickstart-reading-and-writing-files on 6/1/18
            System.Diagnostics.Debug.WriteLine("UpdatingHistory...");

            //Access the Application Data storage folder
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;

            //Try to get the samplefile
            StorageFile historyFile = await storageFolder.TryGetItemAsync("History.txt") as StorageFile;


            //If it doesn't exist
            if (historyFile == null)
            {
                System.Diagnostics.Debug.WriteLine("History file nonexistent, creating History.txt");

                //then create it 
                historyFile = await storageFolder.CreateFileAsync("History.txt");
            }

            //Write the songs which were just updated to the file
            DateTime dateAdded = DateTime.Now;
            string header = dateAdded.ToString();
            string recentlyAddedSongs = CompletedSongsTextBlock.Text;

            string existingText = await FileIO.ReadTextAsync(historyFile);

            await FileIO.WriteTextAsync(historyFile, existingText + "\n" + header + recentlyAddedSongs + "\n");

        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("LoadingPage OnNavigatedTo Started");

            //code retrieved from https://stackoverflow.com/questions/18041112/passing-values-between-xaml-pages on 6/1/18
            StorageFolder musicFolder = e.Parameter as StorageFolder;

            IReadOnlyList<StorageFile> fileList = await musicFolder.GetFilesAsync();   //get the files from the folder

            //Set the completion bar max
            CompletionProgressBar.Maximum = fileList.Count;

            UpdateSongsAsync(fileList);

        }

        private async void UpdateSongsAsync(IReadOnlyList<StorageFile> fileList)
        {
            System.Diagnostics.Debug.WriteLine("UpdateSongs called");

            //Make list to store files with multiple results
            List<List<SpotifySongInfo>> multipleReturnedInfos = new List<List<SpotifySongInfo>>();

            //For each music file in the selected folder
            foreach (StorageFile musicFile in fileList)
            {
                System.Diagnostics.Debug.WriteLine("Begin using " + musicFile);

                //If the cancel button hasn't been hit
                if (shouldContinue)
                {
                    
                    //Search spotify for the track using the file
                    List<SpotifySongInfo> tracks = await SearchSpotify(musicFile);

                    //If the search found nothing
                    if (tracks == null || tracks.Count < 1)
                    {
                        //Then notify the user that spotify found nothing
                        CompletedSongsTextBlock.Text += "\nSpotify search returned nothing on " + GetFileName(musicFile);
                        CompletionProgressBar.Value++;
                    }
                    //If one SpotifySongInfo was returned
                    else if (tracks.Count == 1)
                    {
                        //Set the metadata using the one and only Spotify Song Info
                        SetMetadataAsync(tracks[0]);

                        CompletedSongsTextBlock.Text += "\n" + tracks[0].trackTitle + " by " + tracks[0].artistName + " on " + tracks[0].album;   //add the track to the text box
                    }
                    //More than one SpotifySongInfo was returned
                    else
                    {
                        //Notify the user
                        CompletedSongsTextBlock.Text += "\n" + tracks[0].trackTitle + " by " + tracks[0].artistName + " exists in multiple albums";
                        //Add the tracks to the list
                        multipleReturnedInfos.Add(tracks);
                    }
                }
                //shouldContinue was set to false
                else
                {
                    //Stop updating songs
                    System.Diagnostics.Debug.WriteLine("shouldContinue was false so the program stopped");
                    CompletedSongsTextBlock.Text += "\nCancel button clicked";
                    break;
                }
            }   //end of the songs forloop

            //deal with the songs which returned multiple results
            System.Diagnostics.Debug.WriteLine("Start dealing with multiple returns");

            //inform the user
            if (shouldContinue && multipleReturnedInfos.Count > 0)
            {
                await InformUserContentDialog.ShowAsync();

                foreach (List<SpotifySongInfo> spotifySongInfos in multipleReturnedInfos)
                {
                    SelectTrackGridView.ItemsSource = spotifySongInfos;
                    await SelectTrackContentDialog.ShowAsync();
                }
            }

           

            //when the songs are finished
            System.Diagnostics.Debug.WriteLine("SONGS FINISHED");

            //Change which buttons are enabled
            FinishButton.IsEnabled = true;
            CancelButton.IsEnabled = false;

            //Update the history text file 
            UpdateHistory();
        }
        
        private async void SetMetadataAsync(SpotifySongInfo spotifySongInfo)
        {
            StorageFile musicFile = spotifySongInfo.musicFile;

            MusicProperties musicProperties = await musicFile.Properties.GetMusicPropertiesAsync();

            musicProperties.Title = spotifySongInfo.trackTitle;
            musicProperties.Album = spotifySongInfo.album;
            musicProperties.Artist = spotifySongInfo.artistName;
            //code retrieved from https://social.msdn.microsoft.com/Forums/vstudio/en-US/3e1fdca3-1d6d-4a3d-ab17-8cb5f1215545/how-do-i-convert-int-to-uint-in-c?forum=csharpgeneral on 6/7/18
            musicProperties.TrackNumber = Convert.ToUInt32(spotifySongInfo.trackNumber);

            musicProperties.Year = Convert.ToUInt32(spotifySongInfo.year);



            //Set these properties to the actual file
            await musicProperties.SavePropertiesAsync();

            //rename the file 
            //make sure the file names aren't the same
            string newName = GenerateFileName(spotifySongInfo.trackTitle, spotifySongInfo.artistName);
            string oldName = GetFileName(musicFile);
            //if they are different
            if (!newName.Equals(oldName))   
            {
                //rename the file
                await musicFile.RenameAsync(newName);   
            }
            //Otherwise don't change the file name

            //Increase the number of songs updated
            CompletionProgressBar.Value++; 
        }

        private string GenerateFileName(string trackTitle, string artistName)
        {
            //put the strings together 
            string deltaName = trackTitle + " - " + artistName + ".mp3";

            //replace not-allowed characters
            //array of characters not allowed in file names
            char[] notAllowedCharacters = { '/', '%', '&', '*', '$', '!' };

            foreach (char nAC in notAllowedCharacters)
            {
                //remove any of those characters from the filename and replace with _
                deltaName = deltaName.Replace(nAC, '_');
            }

            //return the file name string
            return deltaName;
        }

        private string GetFileName(StorageFile file)
        {
            int nameStarts = file.Path.LastIndexOf('\\') + 1;
            string name = file.Path.Substring(nameStarts);

            return name;
        }

        private async Task<List<SpotifySongInfo>> SearchSpotify(StorageFile musicFile)
        {
            //Get the music properties from the file
            MusicProperties musicProperties = await musicFile.Properties.GetMusicPropertiesAsync();
            //Get the title and artist from the file
            string trackTitle = musicProperties.Title;
            string artistName = musicProperties.Artist;
            System.Diagnostics.Debug.WriteLine($"File info:\n\tTitle: {trackTitle}\n\tArtist: {artistName}\n\tAlbumArtist: {musicProperties.AlbumArtist}");

            System.Diagnostics.Debug.WriteLine($"Searching spotify using Title: {trackTitle} Artist: {artistName}");

            //Create a WebRetriever object to search the web
            WebRetriever webRetriever = new WebRetriever();

            //Use the WebRetriever and file info to search for track
            SearchTrackRootObject searchResults = await webRetriever.SearchTrack(trackTitle, artistName);

            List<SpotifySongInfo> ssInfoList = new List<SpotifySongInfo>();

            if (searchResults.tracks.total != 0)    //if the search returned some response
            {
                foreach (var track in searchResults.tracks.items)   //for every track the search returned
                {
                    //Make sure the track and artist are correct

                    //If they are correct
                    if (trackTitle.Equals(track.name) && artistName.Equals(track.artists[0].name))
                    {
                        //Create a new SpotifySongObject
                        SpotifySongInfo ssInfo = new SpotifySongInfo();

                        //Set it's fields using the internet info
                        ssInfo.trackTitle = track.name;
                        ssInfo.artistName = track.artists[0].name;
                        ssInfo.album = track.album.name;
                        //if the album is longer than 30 characters
                        if (ssInfo.album.Length > 30)
                        {
                            //then make a short album name
                            ssInfo.shortAlbum = track.album.name.Substring(0, 27) + "...";
                        }
                        else
                        {
                            //Otherwise leave it alone
                            ssInfo.shortAlbum = track.album.name;
                        }
                        ssInfo.trackNumber = track.track_number;
                        ssInfo.imageUrl = track.album.images[0].url;
                        ssInfo.musicFile = musicFile;
                        int year  = Int32.Parse(track.album.release_date.Substring(0,4));
                        ssInfo.year = year;


                        //add the instance to the lsit
                        ssInfoList.Add(ssInfo);
                    }
                    //Else skip this track and continue looking at others
                }
            }
            //Return the list
            return ssInfoList;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("CancleButton Clicked");
            shouldContinue = false;
        }

        private void FinishButton_Click(object sender, RoutedEventArgs e)
        {
            
            this.Frame.Navigate(typeof(HistoryPage));
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            //Get the item which was clicked
            SpotifySongInfo selectedSSI = e.ClickedItem as SpotifySongInfo;

            //Set the metadata for the file using that SpotifySongInfo
            SetMetadataAsync(selectedSSI);

            //Close the ContentDialog
            SelectTrackContentDialog.Hide();
        }

        private void DialogButton_Click(object sender, RoutedEventArgs e)
        {
            InformUserContentDialog.Hide();
        }
    }

}
