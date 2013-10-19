using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Phone.Controls;
using System.Threading;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Text;
using System.Windows.Controls.Primitives;

namespace GiantBombQuiz
{
    public partial class MainPage : PhoneApplicationPage
    {
        private const byte AmountOfCategories = 5; // The amount of categories.

        private int score;
        private int clicks;
        private int timerSpan;
        private int currentRound;

        private Thread loadNextRoundThread;
        private bool nextRoundLoaded;
        private bool loadNextRound;

        private Popup splash;
        private Random rand;

        private DispatcherTimer timer;

        private String boxMessage;
            
        private Question[] questions;
        private Question[] preLoadedQuestions;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MainPage()
        {
            // Initializes all the graphical components and shows the splash screen.
            InitializeComponent();
            ShowSplash();

            this.rand = new Random();

            // Initialize some of the variables.
            this.score = 0;
            this.clicks = 0;
            this.currentRound = 0;
            this.timerSpan = 100;
            this.boxMessage = string.Empty;
            this.loadNextRound = true;
            this.nextRoundLoaded = false;
            this.questions = new Question[AmountOfCategories];
            this.preLoadedQuestions = new Question[AmountOfCategories];

            // Creates the download manager used to download all the games for the first round.
            DownloadManager dlm = new DownloadManager();

            // Hooks up the DownloadsComplete method to the event and adds five different games to the queue.
            dlm.DownloadsComplete += DownloadsComplete;
            dlm.AddToQueue("http://www.giantbomb.com/api/game/3030-" + rand.Next(1000, 40000) + "/?api_key=eb6e06956ccffbfb9c2966194f3d618fe622ab51&format=json&field_list=genres,name,developers,characters,releases,publishers");
            dlm.AddToQueue("http://www.giantbomb.com/api/game/3030-" + rand.Next(1000, 40000) + "/?api_key=eb6e06956ccffbfb9c2966194f3d618fe622ab51&format=json&field_list=genres,name,developers,characters,releases,publishers");
            dlm.AddToQueue("http://www.giantbomb.com/api/game/3030-" + rand.Next(1000, 40000) + "/?api_key=eb6e06956ccffbfb9c2966194f3d618fe622ab51&format=json&field_list=genres,name,developers,characters,releases,publishers");
            dlm.AddToQueue("http://www.giantbomb.com/api/game/3030-" + rand.Next(1000, 40000) + "/?api_key=eb6e06956ccffbfb9c2966194f3d618fe622ab51&format=json&field_list=genres,name,developers,characters,releases,publishers");
            dlm.AddToQueue("http://www.giantbomb.com/api/game/3030-" + rand.Next(1000, 40000) + "/?api_key=eb6e06956ccffbfb9c2966194f3d618fe622ab51&format=json&field_list=genres,name,developers,characters,releases,publishers");
            dlm.StartDownload();

            // Creates a thread for downloading data in the background.
            this.loadNextRoundThread = new Thread(new ThreadStart(LoadNextRound));
            this.loadNextRoundThread.IsBackground = true;
            this.loadNextRoundThread.Start();
        }

        #region Load First

        /// <summary>
        /// Method called when the game has downloaded info about five unique games.
        /// </summary>
        /// <param name="downloads">The list of games.</param>
        private void DownloadsComplete(List<Download> downloads)
        {
            // Creates all the four questons.
            this.questions[0] = new Question(downloads[0].Game.Title, downloads[0].Game.Developer, downloads[1].Game.Developer, downloads[2].Game.Developer, downloads[3].Game.Developer);
            this.questions[1] = new Question(downloads[1].Game.Title, downloads[1].Game.ReleaseYear, downloads[4].Game.ReleaseYear, downloads[2].Game.ReleaseYear, downloads[3].Game.ReleaseYear);
            this.questions[2] = new Question(downloads[2].Game.Title, downloads[2].Game.Publisher, downloads[4].Game.Publisher, downloads[0].Game.Publisher, downloads[3].Game.Publisher);
            this.questions[3] = new Question(downloads[3].Game.Title, downloads[3].Game.Character, downloads[4].Game.Character, downloads[0].Game.Character, downloads[1].Game.Character);
            this.questions[4] = new Question(downloads[4].Game.Title, downloads[4].Game.Genre, downloads[2].Game.Genre, downloads[0].Game.Genre, downloads[1].Game.Genre);

            // Updates the gui.
            UpdateGUI();

            // Removes the splash screen, this must be done on the UI-thread.
            this.Dispatcher.BeginInvoke(() =>
                {
                    this.splash.IsOpen = false;
                }
            );

            // Starts the timer.
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1); // one second
            timer.Tick += timer_Tick;
            timer.Start();
        }

        #endregion

        #region Thread Loading

        /// <summary>
        /// Laddar nästa spelrunda, denna funktion körs i en tråd som förbereder spelet för nästa omgång.
        /// På detta vis så slipper man vänta mellan omgångarna.
        /// </summary>
        private void LoadNextRound()
        {
            // Loop forever.
            while (true)
            {
                if (this.loadNextRound)
                {
                    // If the next round should be loaded then we creates a new DownloadManager and starts the download of five games.
                    DownloadManager dlm = new DownloadManager();

                    dlm.DownloadsComplete += PreDownloadsComplete;
                    dlm.AddToQueue("http://www.giantbomb.com/api/game/3030-" + rand.Next(1000, 40000) + "/?api_key=eb6e06956ccffbfb9c2966194f3d618fe622ab51&format=json&field_list=genres,name,developers,characters,releases,publishers");
                    dlm.AddToQueue("http://www.giantbomb.com/api/game/3030-" + rand.Next(1000, 40000) + "/?api_key=eb6e06956ccffbfb9c2966194f3d618fe622ab51&format=json&field_list=genres,name,developers,characters,releases,publishers");
                    dlm.AddToQueue("http://www.giantbomb.com/api/game/3030-" + rand.Next(1000, 40000) + "/?api_key=eb6e06956ccffbfb9c2966194f3d618fe622ab51&format=json&field_list=genres,name,developers,characters,releases,publishers");
                    dlm.AddToQueue("http://www.giantbomb.com/api/game/3030-" + rand.Next(1000, 40000) + "/?api_key=eb6e06956ccffbfb9c2966194f3d618fe622ab51&format=json&field_list=genres,name,developers,characters,releases,publishers");
                    dlm.AddToQueue("http://www.giantbomb.com/api/game/3030-" + rand.Next(1000, 40000) + "/?api_key=eb6e06956ccffbfb9c2966194f3d618fe622ab51&format=json&field_list=genres,name,developers,characters,releases,publishers");
                    dlm.StartDownload();

                    // We should not download the next round.
                    this.loadNextRound = false;
                }
                else
                    Thread.Sleep(100); // If the next round has already been loaded then sleep for 100 milliseconds.
            }
        }

        /// <summary>
        /// Method called when the preload-thread has downloaded info about five unique games.
        /// </summary>
        /// <param name="downloads">The list of downloads.</param>
        private void PreDownloadsComplete(List<Download> downloads)
        {
            // Creates all the preloaded questions.
            this.preLoadedQuestions[0] = new Question(downloads[0].Game.Title, downloads[0].Game.Developer, downloads[1].Game.Developer, downloads[2].Game.Developer, downloads[3].Game.Developer);
            this.preLoadedQuestions[1] = new Question(downloads[1].Game.Title, downloads[1].Game.ReleaseYear, downloads[4].Game.ReleaseYear, downloads[2].Game.ReleaseYear, downloads[3].Game.ReleaseYear);
            this.preLoadedQuestions[2] = new Question(downloads[2].Game.Title, downloads[2].Game.Publisher, downloads[4].Game.Publisher, downloads[0].Game.Publisher, downloads[3].Game.Publisher);
            this.preLoadedQuestions[3] = new Question(downloads[3].Game.Title, downloads[3].Game.Character, downloads[4].Game.Character, downloads[0].Game.Character, downloads[1].Game.Character);
            this.preLoadedQuestions[4] = new Question(downloads[4].Game.Title, downloads[4].Game.Genre, downloads[2].Game.Genre, downloads[0].Game.Genre, downloads[1].Game.Genre);

            this.nextRoundLoaded = true; // The next round has been loaded.
        }
        
        #endregion

        /// <summary>
        /// Shows the custom splash screen.
        /// </summary>
        private void ShowSplash()
        {
            this.splash = new Popup();
            this.splash.Child = new SplashScreenControl();
            this.splash.IsOpen = true;
        }

        /// <summary>
        /// Method called every second to reduce the timer and check if the timer reaches zero.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void timer_Tick(object sender, EventArgs e)
        {            
            // Reduce the timer by one.
            timerSpan -= 1;

            // Update the timer GUI if the timer is equal or more than zero.
            if (timerSpan >= 0)
            {
                timer1.Text = timerSpan.ToString();
                timer2.Text = timerSpan.ToString();
                timer3.Text = timerSpan.ToString();
                timer4.Text = timerSpan.ToString();
                timer5.Text = timerSpan.ToString();
            }

            // If the timer reaches zero then it's game over.
            if(timerSpan == 0)
            {
                // Show sthe messageBox and starts the next round when the user presses OK.
                if(MessageBox.Show("Game Over!! Press OK for new round", "Time up suckah!", MessageBoxButton.OK) == MessageBoxResult.OK)
                    NextRound();

                score = 0;
            }
        }

        /// <summary>
        /// Method called when a new round should start.
        /// </summary>
        private void NextRound()
        {
            #region endRound messages

            // If the user has answered all categories.
            if (clicks == 5)
            {
                // Increase the currentRound and creates a message depending on the score.
                currentRound++;

                switch (score)
                {
                    case 0: 
                        boxMessage = "Well, at least it's not a negative score! It's been " + currentRound + " rounds and you are still at zero. " +
                             "That's okay though. You're an awesome person for playing this game. Next round is coming up!";
                        break;

                    case 500:
                        boxMessage =
                            "Five hundred points is cool. Really cool dude. You are trying. I bet you're even trying really hard! The only path onwards is up! " +
                            "Unless you get questions wrong. Then it's down. This next round is big!";
                        break;

                    case 1000:
                        boxMessage = 
                            "Now we're getting somewhere. You got dat 1k son! Making bank! In video game points! You can totally use those to buy... friends?" +
                             " High scores are impressive right? I'm pretty sure arcades are awesome. NEW ROUND!";
                        break;

                    case 1500:
                        boxMessage = 
                            "This is 500 more than 1000. If you have a friend that got a thousand points, you are totally better than him. Not just better at games, " +
                             "you are actually a better person. Feel free to ridicule him at this point, cause you're cool.";;
                        break;

                    case 2000:
                        boxMessage =
                            "TWO LARGE you are rolling points now! Just keep going, it's only been " + currentRound + " rounds and you're already at 2 large. That's OG (Original Gamer) shit. " +
                            "GG RE? Let's go next.";
                            break;
                    case -500:
                        boxMessage =
                            "Look, we've had a set-back. This is fixable though. Keep fighting! Just start the next round!";
                        break;
                }

                if (score < 501)
                    boxMessage = "Look man... Mistakes were made. Let's not dwell on it. Just... just move on to the next round.";

                if (score > 2500 && score < 5000)
                    boxMessage = "I like you. You got dem points. You know what's up. You have " + score + " points and you know that is a rad amount of points because videogames.";

                if (score > 5000 && score > 1000)
                    boxMessage = "THIS SCORE IS BANANAS\nB A N A N A S! You must frequent the GiantBomb Wiki regularly! Hit the next round and get some more of dem POINTS!";
                if (score > 10000 && score < 20000)
                    boxMessage =
                        "You clearly don't need me to tell you words of encouragement. You are everything a gamer should aspire to me. " +
                        "You are really, really good at this quiz game. I award you all the points, and may God have mercy on your soul. Go to the next round, and continue to destroy this quiz.";

                if (score > 20000)
                    boxMessage =
                        "You are way too good at this game. Way better than I'll ever be. You have achieved demigod status simply by playing this quiz. Godspeed. " +
                        "Go to the next round if you want to get HIGH SCORES! \n P.S We don't really have a leaderboard though :}";

                // Show the message.
                MessageBox.Show(boxMessage, "Round "+currentRound+". Score: "+score.ToString(), MessageBoxButton.OK);
                clicks = 0;
            }

            #endregion

            // Resets the timer and stops it.
            timerSpan = 99;
            timer1.Text = "99";
            timer.Stop();

            // If next round is loaded then show the new questions.
            if (nextRoundLoaded)
            {
                // If the splash screen is open, close it.
                if (this.splash.IsOpen)
                    this.splash.IsOpen = false;

                // Copy the pre loaded questions to the current questions.
                this.questions = (Question[])this.preLoadedQuestions.Clone();
                this.loadNextRound = true; // The next round should be loaded.
                this.nextRoundLoaded = false; // The next round is not loaded.

                // Updates the GUI and starts the timer again.
                UpdateGUI();
                timer.Start();
            }
            else
            {
                // If the next round is not loaded then create a new Thread on the Threadpool
                ThreadPool.QueueUserWorkItem(delegate(object state)
                {
                    // Loop until we break.
                    while (true)
                    {
                        // Open the splash screen.
                        this.Dispatcher.BeginInvoke(() =>
                            {
                                this.splash.IsOpen = true;
                            }
                        );

                        if (this.nextRoundLoaded)
                            break;

                        // Sleep for 100 milliseconds, we don't want to stress the processor.
                        Thread.Sleep(100);
                    }

                    // When the next round is loaded.
                    this.Dispatcher.BeginInvoke(() =>
                        {
                            // Copy the preloaded questions.
                            this.questions = (Question[])this.preLoadedQuestions.Clone();
                            this.loadNextRound = true;
                            this.nextRoundLoaded = false;

                            UpdateGUI();
                            
                            // Close the splash screen.
                            this.splash.IsOpen = false;
                            timer.Start();
                        }
                    );
                });
            }
        }

        /// <summary>
        /// Method called when the GUI should be updated.
        /// </summary>
        private void UpdateGUI()
        {
            // Changes all the question states to true.
            questionState(true, 1);
            questionState(true, 2);
            questionState(true, 3);
            questionState(true, 4);
            questionState(true, 5);

            // Updates all the questions and their options.
            Question1.Text = "Which developer is responsible for";
            Q1_Title.Text = this.questions[0].Title;
            Q1_Option1.Content = this.questions[0].Options[0];
            Q1_Option2.Content = this.questions[0].Options[1];
            Q1_Option3.Content = this.questions[0].Options[2];
            Q1_Option4.Content = this.questions[0].Options[3];

            Question2.Text = "What year was this game released";
            Q2_Title.Text = this.questions[1].Title;
            Q2_Option1.Content = this.questions[1].Options[0];
            Q2_Option2.Content = this.questions[1].Options[1];
            Q2_Option3.Content = this.questions[1].Options[2];
            Q2_Option4.Content = this.questions[1].Options[3];

            Question3.Text = "Which company published";
            Q3_Title.Text = this.questions[2].Title;
            Q3_Option1.Content = this.questions[2].Options[0];
            Q3_Option2.Content = this.questions[2].Options[1];
            Q3_Option3.Content = this.questions[2].Options[2];
            Q3_Option4.Content = this.questions[2].Options[3];

            Question4.Text = "Who is the leading character in";
            Q4_Title.Text = this.questions[3].Title;
            Q4_Option1.Content = this.questions[3].Options[0];
            Q4_Option2.Content = this.questions[3].Options[1];
            Q4_Option3.Content = this.questions[3].Options[2];
            Q4_Option4.Content = this.questions[3].Options[3];

            Question5.Text = "To which genre does the game below belong";
            Q5_Title.Text = this.questions[4].Title;
            Q5_Option1.Content = this.questions[4].Options[0];
            Q5_Option2.Content = this.questions[4].Options[1];
            Q5_Option3.Content = this.questions[4].Options[2];
            Q5_Option4.Content = this.questions[4].Options[3];
        }

        #region Questions

        /// <summary>
        /// Sets the status of the question and makes it visible/invisible
        /// </summary>
        /// <param name="state">Should the question be clickable.</param>
        /// <param name="category">Which category the questions belong to.</param>
        private void questionState(bool state, int category)
        {
            // If category is set to 1 then change all the options for the first question.
            if(category == 1)
            {
                if (state == false)
                {
                    Q1_Option1.IsEnabled = false;
                    Q1_Option2.IsEnabled = false;
                    Q1_Option3.IsEnabled = false;
                    Q1_Option4.IsEnabled = false;

                    Q1_Option1.BorderThickness = new Thickness(0, 0, 0, 0);
                    Q1_Option2.BorderThickness = new Thickness(0, 0, 0, 0);
                    Q1_Option3.BorderThickness = new Thickness(0, 0, 0, 0);
                    Q1_Option4.BorderThickness = new Thickness(0, 0, 0, 0);
                }

                else
                {
                    Q1_Option1.IsEnabled = true;
                    Q1_Option2.IsEnabled = true;
                    Q1_Option3.IsEnabled = true;
                    Q1_Option4.IsEnabled = true;

                    Q1_Option1.BorderThickness = new Thickness(3, 3, 3, 3);
                    Q1_Option2.BorderThickness = new Thickness(3, 3, 3, 3);
                    Q1_Option3.BorderThickness = new Thickness(3, 3, 3, 3);
                    Q1_Option4.BorderThickness = new Thickness(3, 3, 3, 3);
                }
            }

            // If category is set to 2 then change all the options for the first question.
            if (category == 2)
            {
                if (state == false)
                {
                    Q2_Option1.IsEnabled = false;
                    Q2_Option2.IsEnabled = false;
                    Q2_Option3.IsEnabled = false;
                    Q2_Option4.IsEnabled = false;

                    Q2_Option1.BorderThickness = new Thickness(0, 0, 0, 0);
                    Q2_Option2.BorderThickness = new Thickness(0, 0, 0, 0);
                    Q2_Option3.BorderThickness = new Thickness(0, 0, 0, 0);
                    Q2_Option4.BorderThickness = new Thickness(0, 0, 0, 0);
                }

                else
                {
                    Q2_Option1.IsEnabled = true;
                    Q2_Option2.IsEnabled = true;
                    Q2_Option3.IsEnabled = true;
                    Q2_Option4.IsEnabled = true;

                    Q2_Option1.BorderThickness = new Thickness(3, 3, 3, 3);
                    Q2_Option2.BorderThickness = new Thickness(3, 3, 3, 3);
                    Q2_Option3.BorderThickness = new Thickness(3, 3, 3, 3);
                    Q2_Option4.BorderThickness = new Thickness(3, 3, 3, 3);
                }
            }

            // If category is set to 3 then change all the options for the first question.
            if (category == 3)
            {
                if (state == false)
                {
                    Q3_Option1.IsEnabled = false;
                    Q3_Option2.IsEnabled = false;
                    Q3_Option3.IsEnabled = false;
                    Q3_Option4.IsEnabled = false;

                    Q3_Option1.BorderThickness = new Thickness(0, 0, 0, 0);
                    Q3_Option2.BorderThickness = new Thickness(0, 0, 0, 0);
                    Q3_Option3.BorderThickness = new Thickness(0, 0, 0, 0);
                    Q3_Option4.BorderThickness = new Thickness(0, 0, 0, 0);
                }

                else
                {
                    Q3_Option1.IsEnabled = true;
                    Q3_Option2.IsEnabled = true;
                    Q3_Option3.IsEnabled = true;
                    Q3_Option4.IsEnabled = true;

                    Q3_Option1.BorderThickness = new Thickness(3, 3, 3, 3);
                    Q3_Option2.BorderThickness = new Thickness(3, 3, 3, 3);
                    Q3_Option3.BorderThickness = new Thickness(3, 3, 3, 3);
                    Q3_Option4.BorderThickness = new Thickness(3, 3, 3, 3);
                }
            }

            // If category is set to 4 then change all the options for the first question.
            if (category == 4)
            {
                if (state == false)
                {
                    Q4_Option1.IsEnabled = false;
                    Q4_Option2.IsEnabled = false;
                    Q4_Option3.IsEnabled = false;
                    Q4_Option4.IsEnabled = false;

                    Q4_Option1.BorderThickness = new Thickness(0, 0, 0, 0);
                    Q4_Option2.BorderThickness = new Thickness(0, 0, 0, 0);
                    Q4_Option3.BorderThickness = new Thickness(0, 0, 0, 0);
                    Q4_Option4.BorderThickness = new Thickness(0, 0, 0, 0);
                }

                else
                {
                    Q4_Option1.IsEnabled = true;
                    Q4_Option2.IsEnabled = true;
                    Q4_Option3.IsEnabled = true;
                    Q4_Option4.IsEnabled = true;

                    Q4_Option1.BorderThickness = new Thickness(3, 3, 3, 3);
                    Q4_Option2.BorderThickness = new Thickness(3, 3, 3, 3);
                    Q4_Option3.BorderThickness = new Thickness(3, 3, 3, 3);
                    Q4_Option4.BorderThickness = new Thickness(3, 3, 3, 3);
                }
            }

            // If category is set to 5 then change all the options for the first question.
            if (category == 5)
            {
                if (state == false)
                {
                    Q5_Option1.IsEnabled = false;
                    Q5_Option2.IsEnabled = false;
                    Q5_Option3.IsEnabled = false;
                    Q5_Option4.IsEnabled = false;

                    Q5_Option1.BorderThickness = new Thickness(0, 0, 0, 0);
                    Q5_Option2.BorderThickness = new Thickness(0, 0, 0, 0);
                    Q5_Option3.BorderThickness = new Thickness(0, 0, 0, 0);
                    Q5_Option4.BorderThickness = new Thickness(0, 0, 0, 0);
                }

                else
                {
                    Q5_Option1.IsEnabled = true;
                    Q5_Option2.IsEnabled = true;
                    Q5_Option3.IsEnabled = true;
                    Q5_Option4.IsEnabled = true;

                    Q5_Option1.BorderThickness = new Thickness(3, 3, 3, 3);
                    Q5_Option2.BorderThickness = new Thickness(3, 3, 3, 3);
                    Q5_Option3.BorderThickness = new Thickness(3, 3, 3, 3);
                    Q5_Option4.BorderThickness = new Thickness(3, 3, 3, 3);
                }
            }

        }

        /// <summary>
        /// Updates the score-GUI.
        /// </summary>
        private void currentScoreUpdate()
        {
            currentScoreQ1.Text = score.ToString();
            currentScoreQ2.Text = score.ToString();
            currentScoreQ3.Text = score.ToString();
            currentScoreQ4.Text = score.ToString();
            currentScoreQ5.Text = score.ToString();
        }

        #endregion 

        #region Kategori Events

        /// <summary>
        /// Method called when the user clicks on one of the options for the first question.
        /// </summary>
        /// <param name="sender">The button the user clicked.</param>
        /// <param name="e">The Routed Event arguments.</param>
        private void category1_Click(object sender, RoutedEventArgs e)
        {
            // Sets the question state to false for the first category.
            questionState(false, 1);

            // Check if the player clicked the right answer.
            if (this.questions[0].IsCorrect(((Button)sender).Content.ToString()))
            {
                Question1.Text = "Correct! The answer is ";
                Q1_Title.Text = this.questions[0].CorrectAnswer;

                // Adds score and updates the screen.
                score += 1500;
                currentScoreUpdate();
            }
            else
            {
                // If the player clicked the wrong answer.
                Question1.Text = "Incorrect =( The correct answer is :";
                Q1_Title.Text = this.questions[0].CorrectAnswer;
                // Reduce the score and update the screen.
                score -= 1500;
                currentScoreUpdate();
            }

            clicks++;

            // If the player has clicked on all the five categories then start the next round.
            if(clicks == 5)
                NextRound();
        }

        /// <summary>
        /// Method called when the user clicks on one of the options for the second question.
        /// </summary>
        /// <param name="sender">The button the user clicked.</param>
        /// <param name="e">The Routed Event arguments.</param>
        private void category2_Click(object sender, RoutedEventArgs e)
        {
            // Sets the question state to false for the second category.
            questionState(false, 2);

            // Check if the player clicked the right answer.
            if (this.questions[1].IsCorrect(((Button)sender).Content.ToString()))
            {
                Question2.Text = "Correct! The answer is ";
                Q2_Title.Text = this.questions[1].CorrectAnswer;

                // Adds score and updates the screen.
                score += 1500;
                currentScoreUpdate();
            }
            else
            {
                // If the player clicked the wrong answer.
                Question2.Text = "Incorrect =( The correct answer is :";
                Q2_Title.Text = this.questions[1].CorrectAnswer;
                // Reduce the score and update the screen.
                score -= 1500;
                currentScoreUpdate();
            }

            clicks++;
            // If the player has clicked on all the five categories then start the next round.
            if (clicks == 5)
                NextRound();
        }

        /// <summary>
        /// Funktion som körs när användaren trycker på en av knapparna på andra kategorin.
        /// </summary>
        /// <param name="sender">Vilken knapp tryckte du på.</param>
        /// <param name="e">Routed Event argumenten.</param>
        private void category3_Click(object sender, RoutedEventArgs e)
        {
            // Sets the question state to false for the third category.
            questionState(false, 3);

            // Check if the player clicked the right answer.
            if (this.questions[2].IsCorrect(((Button)sender).Content.ToString()))
            {
                Question3.Text = "Correct! The answer is ";
                Q3_Title.Text = this.questions[2].CorrectAnswer;
                // Adds score and updates the screen.
                score += 1000;
                currentScoreUpdate();
            }
            else
            {
                // If the player clicked the wrong answer.
                Question3.Text = "Incorrect =( The correct answer is :";
                Q3_Title.Text = this.questions[2].CorrectAnswer;

                // Reduce the score and update the screen.
                score -= 1000;
                currentScoreUpdate();
            }

            clicks++;
            // If the player has clicked on all the five categories then start the next round.
            if (clicks == 5)
                NextRound();
        }

        /// <summary>
        /// Funktion som körs när användaren trycker på en av knapparna på andra kategorin.
        /// </summary>
        /// <param name="sender">Vilken knapp tryckte du på.</param>
        /// <param name="e">Routed Event argumenten.</param>
        private void category4_Click(object sender, RoutedEventArgs e)
        {
            // Sets the question state to false for the fourth category.
            questionState(false, 4);

            // Check if the player clicked the right answer.
            if (this.questions[3].IsCorrect(((Button)sender).Content.ToString()))
            {
                Question4.Text = "Correct! The answer is ";
                Q4_Title.Text = this.questions[3].CorrectAnswer;
                // Adds score and updates the screen.
                score += 500;
                currentScoreUpdate();
            }
            else
            {
                // If the player clicked the wrong answer.
                Question4.Text = "Incorrect =( The correct answer is :";
                Q4_Title.Text = this.questions[3].CorrectAnswer;
                // Reduce the score and update the screen.
                score -= 500;
                currentScoreUpdate();
            }

            clicks++;
            // If the player has clicked on all the five categories then start the next round.
            if (clicks == 5)
                NextRound();
        }

        /// <summary>
        /// Funktion som körs när användaren trycker på en av knapparna på andra kategorin.
        /// </summary>
        /// <param name="sender">Vilken knapp tryckte du på.</param>
        /// <param name="e">Routed Event argumenten.</param>
        private void category5_Click(object sender, RoutedEventArgs e)
        {
            // Sets the question state to false for the fifth category.
            questionState(false, 5);

            // Check if the player clicked the right answer.
            if (this.questions[4].IsCorrect(((Button)sender).Content.ToString()))
            {
                Question5.Text = "Correct! The answer is ";
                Q5_Title.Text = this.questions[4].CorrectAnswer;
                // Adds score and updates the screen.
                score += 500;
                currentScoreUpdate();
            }
            else
            {
                // If the player clicked the wrong answer.
                Question5.Text = "Incorrect =( The correct answer is :";
                Q5_Title.Text = this.questions[4].CorrectAnswer;
                // Reduce the score and update the screen.
                score -= 500;
                currentScoreUpdate();
            }

            clicks++;
            // If the player has clicked on all the five categories then start the next round.
            if (clicks == 5)
                NextRound();
        }

        #endregion

        /// <summary>
        /// Method called when the user clicks on the reset button.
        /// </summary>
        /// <param name="sender">The button the user clicked on.</param>
        /// <param name="e">The routed event arguments.</param>
        private void restart_Click(object sender, RoutedEventArgs e)
        {
            // Sets the score and clicks to zero.
            score = 0;
            clicks = 0;
            NextRound(); // Start the next round.
            currentScoreUpdate(); // Updates the timer gui.
        }
    }
}