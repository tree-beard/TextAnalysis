using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace TextAnalysis
{
    class TextAnalysis
    {
        private static readonly object dictionaryLocker = new object();
        private static Dictionary<string, int> tripletsFrequency;

        public static void CountTripletsFromFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                Console.WriteLine("Specified file is not found!");
                return;
            }

            tripletsFrequency = new Dictionary<string, int>();

            CancellationTokenSource cts = new CancellationTokenSource();
            ParallelOptions po = new ParallelOptions();
            po.CancellationToken = cts.Token;
            po.MaxDegreeOfParallelism = System.Environment.ProcessorCount;

            Console.WriteLine("Computing, please wait...");

            //Start key press cancellation task
            Task.Factory.StartNew(() =>
            {
                try
                {
                    Console.WriteLine("Press any key to cancel");
                    Console.ReadKey(true);
                    cts.Cancel();
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
            });

            //Compute lines of text in parallel threads
            try
            {
                Parallel.ForEach(File.ReadLines(fileName), po, line =>
                {
                    CountTriplets(line);
                    po.CancellationToken.ThrowIfCancellationRequested();
                });
            }
            catch (OperationCanceledException e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                cts.Dispose();
            }

            ShowMostFrequentTriplets(10);
        }

        private static void CountTriplets(string text)
        {
            var wordPattern = new Regex(@"\w+");

            foreach (Match match in wordPattern.Matches(text))
            {
                string word = match.Value.ToLower();

                for (int i = 0; i < word.Length - 2; i++)
                {
                    string triplet = word.Substring(i, 3);

                    lock (dictionaryLocker)
                    {
                        int currentCount = 0;
                        tripletsFrequency.TryGetValue(triplet, out currentCount);
                        currentCount++;
                        tripletsFrequency[triplet] = currentCount;
                    }
                }
            }
        }

        private static void ShowMostFrequentTriplets(int n)
        {
            if (tripletsFrequency.Count == 0)
            {
                Console.WriteLine("No triplets found.");
            }
            else
            {
                var tripletsFreqList = new List<KeyValuePair<string, int>>();
                foreach (KeyValuePair<string, int> pair in tripletsFrequency)
                {
                    tripletsFreqList.Add(pair);
                }

                tripletsFreqList.Sort((x, y) => y.Value.CompareTo(x.Value));

                if (n > tripletsFreqList.Count)
                {
                    n = tripletsFreqList.Count;
                    Console.WriteLine("Too big number, can show only {0} triplets for this file.", n);
                }

                Console.WriteLine("{0} most frequent triplets are: ", n);
                for (int i = 0; i < n; i++)
                {
                    Console.WriteLine("Triplet {0} appears {1} times", tripletsFreqList[i].Key, tripletsFreqList[i].Value);
                }
            }
        }
    }
}
