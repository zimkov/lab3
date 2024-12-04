using System.Collections.Concurrent;

namespace lab3
{
    internal class Program
    {
        static void Main(string[] args)
        {
            BlockingCollection<int>[] producers =
            [
                new BlockingCollection<int>(boundedCapacity: 10),
                new BlockingCollection<int>(boundedCapacity: 10),
                new BlockingCollection<int>(boundedCapacity: 10)
            ];

            Task.Factory.StartNew(() =>
            {
                for (var i = 1; i <= 10; i++)
                {
                    producers[0].Add(i);
                    Thread.Sleep(100);
                }

                producers[0].CompleteAdding();
            });

            Task.Factory.StartNew(() =>
            {
                for (var i = 11; i <= 20; i++)
                {
                    producers[1].Add(i);
                    Thread.Sleep(150);
                }

                producers[1].CompleteAdding();
            });

            Task.Factory.StartNew(() =>
            {
                for (var i = 21; i <= 30; i++)
                {
                    producers[2].Add(i);
                    Thread.Sleep(250);
                }

                producers[2].CompleteAdding();
            });

            List<int> list1 = new List<int>();
            List<int> list2 = new List<int>();

            Task.Factory.StartNew(() =>
            {
                while (!producers.All(producer => producer.IsCompleted))
                {
                    BlockingCollection<int>.TryTakeFromAny(
                        producers,
                        out var item,
                        TimeSpan.FromSeconds(1));
                    if (item != default)
                    {
                        list1.Add(item);
                        Console.Write($"Читатель 1: {item} \n");
                    }
                }
            });

            Task.Factory.StartNew(() =>
            {
                while (!producers.All(producer => producer.IsCompleted))
                {
                    BlockingCollection<int>.TryTakeFromAny(
                        producers,
                        out var item2,
                        TimeSpan.FromSeconds(1));
                    if (item2 != default)
                    {
                        list2.Add(item2);
                        Console.Write($"Читатель 2: {item2} \n");
                    }
                }
            });

            

            Console.ReadLine();

            Console.Write("Читатель 1: ");
            foreach ( var item in list1 ) Console.Write(item + " ");

            Console.Write("\nЧитатель 2: ");
            foreach (var item in list2) Console.Write(item + " ");

        }
    }
}
