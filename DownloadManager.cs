using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;

namespace GiantBombQuiz
{
    /// <summary>
    /// A class containing data about a single download.
    /// </summary>
    public class Download
    {
        /// <summary>
        /// The URL download.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The WebClient used to download the data.
        /// </summary>
        public WebClient WebClient { get; set; }

        /// <summary>
        /// The Data.
        /// </summary>
        public Game Game { get; set; }

        public Download()
        {
            this.Game = new Game();
            this.WebClient = new WebClient();
        }
    }

    /// <summary>
    /// DownloadManager manages all the downloads and when all downloads are finished an event will be fired.
    /// </summary>
    public class DownloadManager
    {
        public delegate void Downloads(List<Download> downloads);
        public event Downloads DownloadsComplete;

        private Random rand;
        private List<Download> queue;
        private int downloadsCompleted;

        /// <summary>
        /// Constructor.
        /// </summary>
        public DownloadManager()
        {
            this.rand = new Random();
            this.downloadsCompleted = 0;
            this.queue = new List<Download>();
        }

        /// <summary>
        /// Adds a new download to the queue.
        /// </summary>
        /// <param name="url"></param>
        public void AddToQueue(string url)
        {
            this.queue.Add(new Download());
            this.queue[this.queue.Count - 1].WebClient.DownloadStringCompleted += halfDownloadCompleted;
            this.queue[this.queue.Count - 1].Url = url;
        }

        /// <summary>
        /// Starts the download.
        /// </summary>
        public void StartDownload()
        {
            for (int i = 0; i < this.queue.Count; i++)
                this.queue[i].WebClient.DownloadStringAsync(new Uri(this.queue[i].Url), i);
        }

        /// <summary>
        /// Method called when the first half of the download is complete.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The download string completed event arguments.</param>
        private void halfDownloadCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            // Get the index from the userState.
            int index = (int)e.UserState;

            try
            {
                // Create a rootObject and the serializer.
                RootObject rootObject = new RootObject();
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(rootObject.GetType());
                using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(e.Result)))
                    rootObject = serializer.ReadObject(ms) as RootObject;

                // If any value is null or if we get a status_code of 1, then try again with a new game.
                if (rootObject.status_code != 1 || rootObject.results.characters == null || rootObject.results.developers == null || rootObject.results.genres == null || rootObject.results.publishers == null || rootObject.results.releases == null)
                {
                    this.queue[index].WebClient.DownloadStringAsync(new Uri("http://www.giantbomb.com/api/game/3030-" + this.rand.Next(1000, 40000) + "/?api_key=eb6e06956ccffbfb9c2966194f3d618fe622ab51&format=json&field_list=genres,name,developers,characters,releases,publishers"), index);
                    return;
                }

                // Loop through the queue and check if the values are now unique, if they're not then try again with a new game.
                for (int i = 0; i < this.queue.Count; i++)
                {
                    if (this.queue[i].Game.Character == rootObject.results.characters[0].name ||
                        this.queue[i].Game.Developer == rootObject.results.developers[0].name ||
                        this.queue[i].Game.Genre == rootObject.results.genres[0].name ||
                        this.queue[i].Game.Publisher == rootObject.results.publishers[0].name ||
                        this.queue[i].Game.Title == rootObject.results.name)
                    {
                        this.queue[index].WebClient.DownloadStringAsync(new Uri("http://www.giantbomb.com/api/game/3030-" + this.rand.Next(1000, 40000) + "/?api_key=eb6e06956ccffbfb9c2966194f3d618fe622ab51&format=json&field_list=genres,name,developers,characters,releases,publishers"), index);
                        return;
                    }
                }

                // Add the new values to the right download object.
                this.queue[index].Game.Title = rootObject.results.name;
                this.queue[index].Game.Character = rootObject.results.characters[0].name;
                this.queue[index].Game.Developer = rootObject.results.developers[0].name;
                this.queue[index].Game.Genre = rootObject.results.genres[0].name;
                this.queue[index].Game.Publisher = rootObject.results.publishers[0].name;

                // Start the next half of the download.
                this.queue[index].WebClient.DownloadStringCompleted += downloadCompleted;
                this.queue[index].WebClient.DownloadStringCompleted -= halfDownloadCompleted;
                this.queue[index].WebClient.DownloadStringAsync(new Uri("http://www.giantbomb.com/api/release/3050-" + rootObject.results.releases[0].id + "/?api_key=eb6e06956ccffbfb9c2966194f3d618fe622ab51&format=json&field_list=release_date,name"), index);
            }
            catch (Exception)
            {
                // If something went wrong then try again with another game.
                this.queue[index].WebClient.DownloadStringAsync(new Uri("http://www.giantbomb.com/api/game/3030-" + this.rand.Next(1000, 40000) + "/?api_key=eb6e06956ccffbfb9c2966194f3d618fe622ab51&format=json&field_list=genres,name,developers,characters,releases,publishers"), index);
                return;
            }
        }

        /// <summary>
        /// Method called when the download is complete.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void downloadCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            // Get the index from the userState.
            int index = (int)e.UserState;

            try
            {
                // Creates the release date object, the serializer and reads the json object.
                ReleaseDate releaseDate = new ReleaseDate();
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(releaseDate.GetType());
                using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(e.Result)))
                    releaseDate = serializer.ReadObject(ms) as ReleaseDate;

                // Parse the release date.
                DateTime dateTime = DateTime.Parse(releaseDate.results.release_date);

                // Loop through the queue and check if the release date is unique, if it's not then try again with a new game.
                for (int i = 0; i < this.queue.Count; i++)
                {
                    if (this.queue[i].Game.ReleaseYear == dateTime.Year.ToString())
                    {
                        // Reset the downloaded data and start a new download.
                        this.queue[index].Game = new Game();
                        this.queue[index].WebClient.DownloadStringCompleted -= downloadCompleted;
                        this.queue[index].WebClient.DownloadStringCompleted += halfDownloadCompleted;
                        this.queue[index].WebClient.DownloadStringAsync(new Uri("http://www.giantbomb.com/api/game/3030-" + this.rand.Next(1000, 40000) + "/?api_key=eb6e06956ccffbfb9c2966194f3d618fe622ab51&format=json&field_list=genres,name,developers,characters,releases,publishers"), index);
                        return;
                    }
                }

                // Set the values to the right download.
                this.queue[index].Game.ReleaseYear = dateTime.Year.ToString();
                this.queue[index].WebClient.DownloadStringCompleted -= downloadCompleted;

                // Increase the downloadsComplete with one and check if we've reached the end of the queue.
                // If we've then fire the DownloadsComplete event.
                this.downloadsCompleted++;
                if (this.downloadsCompleted == this.queue.Count)
                    if (this.DownloadsComplete != null)
                        this.DownloadsComplete(this.queue);
            }
            catch (Exception)
            {
                // If something went wrong then try with a new game.
                try
                {
                    this.queue[index].WebClient.DownloadStringCompleted -= downloadCompleted;
                }
                catch (Exception) { }

                this.queue[index].WebClient.DownloadStringCompleted += halfDownloadCompleted;
                this.queue[index].WebClient.DownloadStringAsync(new Uri("http://www.giantbomb.com/api/game/3030-" + this.rand.Next(1000, 40000) + "/?api_key=eb6e06956ccffbfb9c2966194f3d618fe622ab51&format=json&field_list=genres,name,developers,characters,releases,publishers"), index);
                return;
            }
        }
    }
}