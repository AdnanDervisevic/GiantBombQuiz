using System.Collections.Generic;

namespace GiantBombQuiz
{
    /// <summary>
    /// Class containing data about a game.
    /// </summary>
    public class Game
    {
        public string Title { get; set; }
        public string Character { get; set; }
        public string Developer { get; set; }
        public string Genre { get; set; }
        public string Publisher { get; set; }
        public string ReleaseYear { get; set; }
    }

    /// <summary>
    /// Class containing data about the Character object.
    /// </summary>
    public class Character
    {
        public string api_detail_url { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public string site_detail_url { get; set; }
    }

    /// <summary>
    /// Class containing data about the developer object.
    /// </summary>
    public class Developer
    {
        public string api_detail_url { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public string site_detail_url { get; set; }
    }

    /// <summary>
    /// Class containing data about the Genre object.
    /// </summary>
    public class Genre
    {
        public string api_detail_url { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public string site_detail_url { get; set; }
    }

    /// <summary>
    /// Class containing data about the Publisher object.
    /// </summary>
    public class Publisher
    {
        public string api_detail_url { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public string site_detail_url { get; set; }
    }

    /// <summary>
    /// Class containing data about the Release object.
    /// </summary>
    public class Release
    {
        public string api_detail_url { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public string site_detail_url { get; set; }
    }

    /// <summary>
    /// Class containing data about the Results object.
    /// </summary>
    public class Results
    {
        public string name { get; set; }
        public List<Character> characters { get; set; }
        public List<Developer> developers { get; set; }
        public List<Genre> genres { get; set; }
        public List<Publisher> publishers { get; set; }
        public List<Release> releases { get; set; }
    }

    /// <summary>
    /// Class containing data about the RootObject object.
    /// </summary>
    public class RootObject
    {
        public string error { get; set; }
        public int limit { get; set; }
        public int offset { get; set; }
        public int number_of_page_results { get; set; }
        public int number_of_total_results { get; set; }
        public int status_code { get; set; }
        public Results results { get; set; }
        public string version { get; set; }
    }

    /// <summary>
    /// Class containing data about the RelDate object.
    /// </summary>
    public class RelDate
    {
        public string name { get; set; }
        public string release_date { get; set; }
    }

    /// <summary>
    /// Class containing data about the ReleaseDate object.
    /// </summary>
    public class ReleaseDate
    {
        public string error { get; set; }
        public int limit { get; set; }
        public int offset { get; set; }
        public int number_of_page_results { get; set; }
        public int number_of_total_results { get; set; }
        public int status_code { get; set; }
        public RelDate results { get; set; }
        public string version { get; set; }
    }
}