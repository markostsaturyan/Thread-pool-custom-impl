using System;

namespace Threads
{
    class Program
    {
        static void Main(string[] args)
        {
            for (int i = 1; i <= 10; i++)
            {
                ThreadPool.QueueUserWorkItem(PrinterFibonacci, i);
            }

            Console.Read();
        }

        static void PrinterFibonacci(object o)
        {
            int i;

            if (o is int)
            {
                i = (int)o;
            }
            else
            {
                throw new Exception("Invalid parametr.");
            }

            Console.WriteLine("Fibonacci sequence Index։ " + i + " Value: " + Helper(i));
        }

        private static int Helper(int v)
        {
            if (v == 1)
            {
                return 0;
            }
            else
            {
                if (v == 2)
                {
                    return 1;
                }
                else
                {
                    return Helper(v - 1) + Helper(v - 2);
                }
            }
        }
    }
}
