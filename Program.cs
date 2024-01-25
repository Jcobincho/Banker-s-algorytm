using System;
using System.Threading;
using System.Linq;

class BankerAlgorithm
{
    static int numResources = 3;
    static int[] availableResources = { 10, 5, 7 };
    static object resourceLock = new object();

    static int[][] maxDemand = new int[][]
    {
        new int[] {7, 5, 3},
        new int[] {3, 2, 2},
        new int[] {9, 0, 2},
        new int[] {2, 2, 2},
        new int[] {4, 3, 3}
    };

    static int[][] allocations = new int[][]
    {
        new int[] {0, 1, 0},
        new int[] {2, 0, 0},
        new int[] {3, 0, 2},
        new int[] {2, 1, 1},
        new int[] {0, 0, 2}
    };

    static int[][] remainingNeeds = new int[][]
    {
        new int[] {7, 4, 3},
        new int[] {1, 2, 2},
        new int[] {6, 0, 0},
        new int[] {0, 1, 1},
        new int[] {4, 3, 1}
    };

    static void Main()
    {
        Console.Write("Podaj liczbę wątków do wykonania: ");
        int numThreads = int.Parse(Console.ReadLine());

        Thread[] threads = new Thread[numThreads];
        for (int i = 0; i < numThreads; i++)
        {
            threads[i] = new Thread(ThreadFunction);
            threads[i].Start(i);
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }
    }

    static void ThreadFunction(object threadIdObj)
    {
        if (threadIdObj == null)
        {
            throw new ArgumentNullException(nameof(threadIdObj));
        }

        int threadId = (int)threadIdObj;

        while (true)
        {
            Console.WriteLine($"Wątek {threadId} oczekuje na naciśnięcie Enter...");
            Console.ReadLine();
            
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"Wątek {threadId} rozpoczyna przetwarzanie...");
            Console.ResetColor();
            Thread.Sleep(TimeSpan.FromSeconds(new Random().NextDouble() * 3));

            int[] request = new int[numResources];
            for (int i = 0; i < numResources; i++)
            {
                request[i] = new Random().Next(0, maxDemand[threadId][i] + 1);
            }

            BankerAlgorithmFunction(threadId, request);
        }
    }

    static void BankerAlgorithmFunction(int threadId, int[] request)
    {
        lock (resourceLock)
        {
            for (int i = 0; i < numResources; i++)
            {
                if (request[i] > remainingNeeds[threadId][i])
                {
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Żądanie wątku {threadId} przekracza pozostałe potrzeby. Prośba odrzucona.");
                    Console.ResetColor();
                    return;
                }
            }

            for (int i = 0; i < numResources; i++)
            {
                if (request[i] > availableResources[i])
                {
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Żądanie wątku {threadId} przekracza dostępne zasoby. Prośba odrzucona.");
                    Console.ResetColor();
                    return;
                }
            }

            for (int i = 0; i < numResources; i++)
            {
                availableResources[i] -= request[i];
                allocations[threadId][i] += request[i];
                remainingNeeds[threadId][i] -= request[i];
            }
        }

        if (IsSafeState())
        {
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Żądanie wątku {threadId} zostało przyjęte. Przydzielone zasoby: [{string.Join(", ", request)}]");
            Console.ResetColor();
        }
        else
        {
            lock (resourceLock)
            {
                for (int i = 0; i < numResources; i++)
                {
                    availableResources[i] += request[i];
                    allocations[threadId][i] -= request[i];
                    remainingNeeds[threadId][i] += request[i];
                }
            }

            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Żądanie wątku {threadId} odrzucone w celu utrzymania bezpiecznego stanu.");
            Console.ResetColor();
        }
    }

    static bool IsSafeState()
    {
        int[] work = availableResources.ToArray();
        bool[] finish = new bool[allocations.Length];

        while (true)
        {
            bool found = false;
            for (int i = 0; i < allocations.Length; i++)
            {
                if (!finish[i])
                {
                    if (remainingNeeds[i].All(need => need <= work[Array.IndexOf(remainingNeeds[i], need)]))
                    {
                        for (int j = 0; j < numResources; j++)
                        {
                            work[j] += allocations[i][j];
                        }

                        finish[i] = true;
                        found = true;
                    }
                }
            }

            if (!found)
            {
                break;
            }
        }
        
        return finish.All(f => f);
    }
}
