using System;
using System.Linq;
using System.Collections.Generic;

namespace GiantBombQuiz
{
    /// <summary>
    /// A class containing the title of the game, the options and the correct answer.
    /// </summary>
    public class Question
    {
        /// <summary>
        /// The game title.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// The options.
        /// </summary>
        public string[] Options { get; private set; }

        /// <summary>
        /// The correct answer.
        /// </summary>
        public string CorrectAnswer { get; private set; }

        /// <summary>
        /// Creates a question.
        /// </summary>
        /// <param name="title">The title of the game.</param>
        /// <param name="correctAnswer">The correct answer.</param>
        /// <param name="options">The incorrect options.</param>
        public Question(string title, string correctAnswer, params string[] options)
        {
            this.Title = title;
            this.CorrectAnswer = correctAnswer;
            this.Options = new string[options.Length + 1];

            // Lägger till det korrekta svaret och alternativen till Options.
            this.Options[0] = correctAnswer;
            for (int i = 0; i < options.Length; i++)
                this.Options[1 + i] = options[i];

            Random rand = new Random();

            // Skapa en lista av KeyValuePair bestående av ett random nummer och ett av svaren.
            List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>();
            foreach (string s in this.Options)
                list.Add(new KeyValuePair<int, string>(rand.Next(), s));

            // Sortera listan efter random numret.
            var sorted = from item in list orderby item.Key select item;

            // Ändra this.Options till de blandade alternativen.
            int index = 0;
            foreach (KeyValuePair<int, string> pair in sorted)
            {
                this.Options[index] = pair.Value;
                index++;
            }
        }

        /// <summary>
        /// Checks if the text equals to the correct answer.
        /// </summary>
        /// <param name="text">The option the user selected.</param>
        /// <returns>Returns true if the user selected the right option, otherwise false.</returns>
        public bool IsCorrect(string text)
        {
            return this.CorrectAnswer == text;
        }
    }
}