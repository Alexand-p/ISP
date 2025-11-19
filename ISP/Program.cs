using System;
using System.Threading;
using IntegralLib;

namespace IntegralConsole
{
    internal class Program
    {
       
        private static void Calc_ProgressChanged(object sender, IntegralProgressEventArgs e)
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            int totalBlocks = 20;
            int filled = e.Percent * totalBlocks / 100;
            if (filled > totalBlocks) filled = totalBlocks;

            string bar = "[" + new string('=', filled) + new string(' ', totalBlocks - filled) + "]";

            Console.WriteLine($"Поток {threadId}: {bar} {e.Percent}%");
        }

        
        private static void Calc_Completed(object sender, IntegralResultEventArgs e)
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            Console.WriteLine($"Поток {threadId}: Завершен с результатом: {e.Result}, время: {e.ElapsedTicks} ticks");
        }

        static void Main(string[] args)
        {
            double a = 0.0;
            double b = 1.0;
            double h = 0.00000001; 

            Console.WriteLine("=== Один поток в отдельном Thread ===");

            IntegralCalculator calc1 = new IntegralCalculator();
            calc1.ProgressChanged += Calc_ProgressChanged;
            calc1.Completed += Calc_Completed;

            Thread t1 = new Thread(() =>
            {
                calc1.CalculateIntegral(a, b, h);
            });

            t1.Start();
            t1.Join();

            Console.WriteLine();
            Console.WriteLine("Нажмите любую клавишу для запуска двух потоков...");
            Console.ReadKey();

          
            Console.WriteLine("=== Два потока с разными приоритетами ===");

            IntegralCalculator highCalc = new IntegralCalculator();
            IntegralCalculator lowCalc = new IntegralCalculator();

            highCalc.ProgressChanged += Calc_ProgressChanged;
            highCalc.Completed += Calc_Completed;

            lowCalc.ProgressChanged += Calc_ProgressChanged;
            lowCalc.Completed += Calc_Completed;

            Thread highThread = new Thread(() =>
            {
                highCalc.CalculateIntegral(a, b, h);
            });

            Thread lowThread = new Thread(() =>
            {
                lowCalc.CalculateIntegral(a, b, h);
            });

            highThread.Priority = ThreadPriority.Highest;
            lowThread.Priority = ThreadPriority.Lowest;

            highThread.Start();
            lowThread.Start();

            highThread.Join();
            lowThread.Join();

            Console.WriteLine();
            Console.WriteLine("Нажмите любую клавишу для проверки режима 'только один поток'...");
            Console.ReadKey();

            
            Console.WriteLine("=== Проверка: выполняется только один поток ===");

            IntegralCalculator singleCalc = new IntegralCalculator();

            Thread[] threads = new Thread[5];
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(() =>
                {
                    singleCalc.CalculateIntegralSingleThread(a, b, h);
                });
                threads[i].Start();
            }

            foreach (var t in threads)
            {
                t.Join();
            }

            Console.WriteLine();
            Console.WriteLine("Нажмите любую клавишу для проверки режима с ограничением потоков...");
            Console.ReadKey();

          
            Console.WriteLine("=== Проверка: одновременно не более 2 потоков ===");

            IntegralCalculator.SetMaxParallel(2);
            IntegralCalculator limitedCalc = new IntegralCalculator();

            Thread[] threads2 = new Thread[5];
            for (int i = 0; i < threads2.Length; i++)
            {
                threads2[i] = new Thread(() =>
                {
                    limitedCalc.CalculateIntegralLimited(a, b, h);
                });
                threads2[i].Start();
            }

            foreach (var t in threads2)
            {
                t.Join();
            }

            Console.WriteLine("Готово. Нажмите любую клавишу для выхода.");
            Console.ReadKey();
        }
    }
}
