using System;

namespace NGGStandalone
{
    class Program
    {
        static void Main(string[] args)
        {
            Random rnd = new Random();
            int num = rnd.Next(0, 100);
            Console.Write("Please enter a number.\n>");
            int userNumber = Convert.ToInt32(Console.ReadLine());
            if (num == userNumber)
            {
                Console.WriteLine("That's correct!");
            }
            else
            {
                Console.WriteLine("Wrong Number.");}
            
        }
    }
}