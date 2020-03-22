using System;
using System.Diagnostics;

namespace TextAnalysis
{
    class Program
    {
        static void Main(string[] args)
        {
            string filePath;
            if (args.Length == 0)
            {
                Console.Write("Enter path to a text file: ");
                filePath = Console.ReadLine();
            }
            else
            {
                filePath = args[0];
            }

            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();
            TextAnalysis.CountTripletsFromFile(filePath);
            stopwatch.Stop();

            Console.WriteLine("Execution time is {0} ms.", stopwatch.ElapsedMilliseconds);
        }
    }
}
