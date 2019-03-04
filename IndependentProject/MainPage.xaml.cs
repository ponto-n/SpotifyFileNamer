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
using System.Threading;
using IndependentProject.Pages;
using Windows.Storage;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace IndependentProject
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            System.Diagnostics.Debug.WriteLine(IconsListBox.SelectedIndex);

            IconsListBox.SelectedItem = HomeListBoxItem;
            System.Diagnostics.Debug.WriteLine(IconsListBox.SelectedIndex);
            
            InnerFrame.Navigate(typeof(HomePage), this);

            CheckSavedTheme();
        }

        private async void CheckSavedTheme()
        {
            System.Diagnostics.Debug.WriteLine("Checking Saved Theme from MainPage");
            //Access the Application Data storage folder
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;

            //Check theme file 
            StorageFile themeFile = await storageFolder.TryGetItemAsync("Theme.txt") as StorageFile;

            //Check if it exists
            if (themeFile != null)
            {
                System.Diagnostics.Debug.WriteLine("\tFile exists");

                string text = await FileIO.ReadTextAsync(themeFile);
                System.Diagnostics.Debug.WriteLine("File containts "+ text);

                if (text.Equals("Dark"))
                {
                    RequestedTheme = ElementTheme.Dark;
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("\tNo file found");
            }


        }

        private void IconsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HomeListBoxItem.IsSelected)
            {
                InnerFrame.Navigate(typeof(HomePage));
            }
            else if(NamingListBoxItem.IsSelected)
            {
                InnerFrame.Navigate(typeof(NamingPage));
            }
            else if (HistoryListBoxItem.IsSelected)
            {
                InnerFrame.Navigate(typeof(HistoryPage));
            }
            else if (NamingListBoxItem.IsSelected)
            {
                InnerFrame.Navigate(typeof(NamingPage));
            }
            else if (SettingsListBoxItem.IsSelected)
            {
                InnerFrame.Navigate(typeof(SettingsPage), this);
            }
        }

        private void HamburgerMenuButton_Click(object sender, RoutedEventArgs e)
        {
            MenuSplitView.IsPaneOpen = true;
        }

        private void InnerFrame_Navigated(object sender, NavigationEventArgs e) //Changes the TitleTextBlock to the apropriate name
        {
            string alpha = InnerFrame.SourcePageType.Name;
            string name = alpha.Substring(0, alpha.Length - 4);
            PageTitleTextBlock.Text = name;

            if (name.Equals("Naming"))
            {
                IconsListBox.SelectedIndex = 1;    //Set the naming page to be selected
            }
            else if (name.Equals("Loading"))
            {
                IconsListBox.SelectedIndex = -1;    //Have nothing selected on the IconsListBox
            }
            else if (name.Equals("History"))
            {
                IconsListBox.SelectedIndex = 2;    //Set the history page to be selected
            }
        }

        public void changeSelected()
        {
            IconsListBox.SelectedItem = NamingListBoxItem;
        }
    }
}
