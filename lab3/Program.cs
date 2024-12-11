using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;

namespace lab3
{
    internal class Program
    {
        private static Queue<int> _buffer = new Queue<int>();
        private static int _bufferSize = 5; // Максимальный размер буфера
        private static Mutex _mutex = new Mutex();
        private static Random _random = new Random();
        static void Main(string[] args)
        {
            // Создаем канал
            var channel = Channel.CreateUnbounded<int>();

            // Запускаем продюсер и консюмер в отдельных потоках
            Thread producerThread = new Thread(() => Producer(channel.Writer));
            Thread consumerThread = new Thread(() => Consumer(channel.Reader));

            producerThread.Start();
            consumerThread.Start();

            // Ждем завершения потоков
            producerThread.Join();
            consumerThread.Join();
        }

        static void Producer(ChannelWriter<int> writer)
        {
            for (int i = 0; i < 10; i++)
            {
                writer.TryWrite(i);
                Console.WriteLine($"Producer produced: {i}");
                Thread.Sleep(new Random().Next(100, 500)); // Имитация времени производства
            }

            writer.Complete(); // Завершает добавление в канал
        }

        static void Consumer(ChannelReader<int> reader)
        {
            while (reader.TryRead(out var item))
            {
                Console.WriteLine($"Consumer consumed: {item}");
                Thread.Sleep(new Random().Next(100, 500)); // Имитация времени потребления
            }

            // Проверяем на наличие оставшихся элементов после завершения записи
            while (reader.WaitToReadAsync().Result)
            {
                while (reader.TryRead(out var item))
                {
                    Console.WriteLine($"Consumer consumed: {item}");
                }
            }
        }



        //static void mutex()
        //{
        //    Thread producerThread = new Thread(Producer);
        //    Thread consumerThread = new Thread(Consumer);

        //    producerThread.Start();
        //    consumerThread.Start();

        //    producerThread.Join();
        //    consumerThread.Join();
        //}

        //static void Producer()
        //{
        //    for (int i = 0; i < 10; i++)
        //    {
        //        _mutex.WaitOne(); // Захватываем мьютекс

        //        // Проверяем, есть ли место в буфере
        //        while (_buffer.Count >= _bufferSize)
        //        {
        //            _mutex.ReleaseMutex(); // Освобождаем мьютекс
        //            Thread.Sleep(10000); // Ждем, если буфер полон
        //            _mutex.WaitOne(); // Пытаемся снова захватить мьютекс
        //        }

        //        // Добавляем элемент в буфер
        //        _buffer.Enqueue(i);
        //        //Console.WriteLine($"Producer produced: {i}");

        //        _mutex.ReleaseMutex(); // Освобождаем мьютекс
        //        Thread.Sleep(_random.Next(100, 500)); // Имитация времени производства
        //    }
        //}

        //static void Consumer()
        //{
        //    for (int i = 0; i < 10; i++)
        //    {
        //        _mutex.WaitOne(); // Захватываем мьютекс

        //        // Проверяем, есть ли элементы в буфере
        //        while (_buffer.Count == 0)
        //        {
        //            _mutex.ReleaseMutex(); // Освобождаем мьютекс
        //            //Thread.Sleep(100); // Ждем, если буфер пуст
        //            _mutex.WaitOne(); // Пытаемся снова захватить мьютекс
        //        }

        //        // Извлекаем элемент из буфера
        //        int item = _buffer.Dequeue();
        //        Console.WriteLine($"Consumer consumed: {item}");

        //        _mutex.ReleaseMutex(); // Освобождаем мьютекс
        //        Thread.Sleep(_random.Next(100, 500)); // Имитация времени потребления
        //    }

        //}


        public static void Go()
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
            foreach (var item in list1) Console.Write(item + " ");

            Console.Write("\nЧитатель 2: ");
            foreach (var item in list2) Console.Write(item + " ");
        }

    }
}
