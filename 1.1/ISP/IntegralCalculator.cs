using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace IntegralLib
{
  
    public class IntegralResultEventArgs : EventArgs
    {
        public double Result { get; set; }
        public long ElapsedTicks { get; set; }
    }

 
    public class IntegralProgressEventArgs : EventArgs
    {
        public int Percent { get; set; }
    }

    public class IntegralCalculator
    {
     
        public event EventHandler<IntegralResultEventArgs> Completed;
        public event EventHandler<IntegralProgressEventArgs> ProgressChanged;

    
        private static readonly object singleLock = new object();

      
        private static SemaphoreSlim semaphore = new SemaphoreSlim(1);

        public static void SetMaxParallel(int n)
        {
            if (n < 1) n = 1;
            semaphore = new SemaphoreSlim(n, n);
        }

   
        private double CalculateIntegralInternal(double a, double b, double h)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            double sum = 0.0;
            long steps = (long)((b - a) / h);
            double x = a;

            int lastPercent = -1;

            for (long i = 0; i < steps; i++)
            {
              
                sum += Math.Sin(x) * h;
                x += h;

              
                for (int j = 0; j < 100000; j++)
                {
                    double tmp = j * j; 
                }

           
                int percent = (int)(i * 100 / steps);
                if (percent != lastPercent)
                {
                    lastPercent = percent;
                    OnProgressChanged(percent);
                }
            }

          
            OnProgressChanged(100);

            sw.Stop();
            OnCompleted(sum, sw.ElapsedTicks);

            return sum;
        }

    
        public double CalculateIntegral(double a, double b, double h)
        {
            return CalculateIntegralInternal(a, b, h);
        }

        
        public double CalculateIntegralSingleThread(double a, double b, double h)
        {
            lock (singleLock)
            {
                return CalculateIntegralInternal(a, b, h);
            }
        }

  
        public double CalculateIntegralLimited(double a, double b, double h)
        {
            semaphore.Wait(); 
            try
            {
                return CalculateIntegralInternal(a, b, h);
            }
            finally
            {
                semaphore.Release();
            }
        }

        
        protected virtual void OnCompleted(double result, long ticks)
        {
            Completed?.Invoke(this, new IntegralResultEventArgs
            {
                Result = result,
                ElapsedTicks = ticks
            });
        }

      
        protected virtual void OnProgressChanged(int percent)
        {
            ProgressChanged?.Invoke(this, new IntegralProgressEventArgs
            {
                Percent = percent
            });
        }
    }
}
