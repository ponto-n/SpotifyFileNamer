using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.Net.Http.Headers;
using IndependentProject.Models;
using System.Net.Http;
using Newtonsoft.Json;
using static IndependentProject.Models.Artist;
using IndependentProject.Pages;
using static IndependentProject.Models.SearchTrackResponse;

namespace IndependentProject
{
    class WebRetriever
    {

        

        public async Task<AccessToken> GetToken()
        {
        //Retrieved from https://gist.github.com/lqdev/5e82a5c856fcf0818e0b5e002deb0c28 on 5/22/18
            string clientId = "eab9238ad6e24e68be78022d60b0ca55";
            string clientSecret = "bc36da6c5d474166aa961d0845718c50";
            string credentials = String.Format("{0}:{1}", clientId, clientSecret);

            using (var client = new HttpClient())
            {
                //Define Headers
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials)));

                //Prepare Request Body
                List<KeyValuePair<string, string>> requestData = new List<KeyValuePair<string, string>>();
                requestData.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));

                FormUrlEncodedContent requestBody = new FormUrlEncodedContent(requestData);

                //Request Token
                var request = await client.PostAsync("https://accounts.spotify.com/api/token", requestBody);
                var response = await request.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<AccessToken>(response);
            }
        }

        public async Task<ArtistRootObject> GetArtist(string artistCode, AccessToken accessToken)
        {
            using (var client = new HttpClient())
            {
                //Define Headers
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(accessToken.token_type, accessToken.access_token);
                
                //Request Artist
                var response = await client.GetStringAsync("https://api.spotify.com/v1/artists/" + artistCode);
                
                return JsonConvert.DeserializeObject<ArtistRootObject>(response);
            }
        }

        public async Task<SearchTrackRootObject> SearchTrack(string trackTitle, string artistName)
        {
            System.Diagnostics.Debug.WriteLine($"WebRetriever.SearchTrack called with: {trackTitle} and {artistName}");

            string searchQuery = GenerateSearchQuery(trackTitle, artistName);

            System.Diagnostics.Debug.WriteLine("Searching with query " + searchQuery);

            using (var client = new HttpClient())
            {
                
                //Define Headers
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //Get an access token
                AccessToken accessToken = await this.GetToken();

                //use the access token to authorize the header
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(accessToken.token_type, accessToken.access_token);

                //request search results
                var response = await client.GetStringAsync("https://api.spotify.com/v1/search?q=" + searchQuery + "&type=track&market=US&limit=3&offset=0");

                //return the deserialized object
                return JsonConvert.DeserializeObject<SearchTrackRootObject>(response);
            }
        }

        private string GenerateSearchQuery(string trackTitle, string artistName)   //replaces the spaces with "%20" and returns the string
        {
            //code retrieved from http://csharp.net-informations.com/string/csharp-string-split.htm on 5/29/18

            string combined = trackTitle + " " + artistName;    //combine the track and artist into one string

            combined = combined.Replace("%", "%25");

            combined = combined.Replace(" ", "%20");

            combined = combined.Replace("/", "%2F");

            return combined;
        }



    }
}