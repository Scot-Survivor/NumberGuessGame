using System;
using System.Collections.Generic;

namespace NGGStandalone
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Number Guess.\nYou must guess a number between 1 & 10. You win if you get 5 numbers correct.");
            List<string> commandWords = new List<string> {"quit", "score"};
            Random rnd = new Random();
            int score = 0;
            int tries = 0;
            while (true)
            {
                var numberToGuess = rnd.Next(0, 10);
                Console.Write(
                    "Commands\n     Quit/quit: Quit the Program.\n     Score/score: Print your score.\nEnter command or your guess\n>");
                string userInput = Console.ReadLine();
                if (userInput != null && commandWords.Contains(userInput.ToLower()))
                {
                    userInput = userInput.ToLower();
                    switch (userInput)
                    {
                        case "quit":
                            return;
                        case "score":
                            Console.WriteLine($"Your score is: {score} You have tried: {tries} times.");
                            break;
                    }
                }
                else if (userInput != null)
                {
                    int userInputInt = Convert.ToInt32(userInput);
                    if (numberToGuess == userInputInt)
                    {
                        tries++;
                        Console.WriteLine($"That's correct! You have tried: {tries} times.");
                        score++;
                        if (score < 5) continue;
                        Console.WriteLine("You Win!");
                        return;
                    }
                    else
                    {
                        tries++;
                        Console.WriteLine($"Wrong Number. You have tried: {tries} times.");
                    }
                }
                else
                {
                    Console.WriteLine("Please enter something!");
                }
            }
        }
    }
}