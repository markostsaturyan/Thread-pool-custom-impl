using System;
using System.Collections.Generic;
using System.Threading;

namespace Threads
{
    public static class ThreadPool
    {
        private static Queue<Action> WaitingTasksQueue = new Queue<Action>();

        private static List<Thread> ThreadsList = new List<Thread>();
        private static List<Action> WaitCallbacksList = new List<Action>();

        private static object threadSleeper = new object();
        private static object lockObject = new object();

        private static int maxCountOfThreads = 10;
        private static int currentCountOfThreads = 0;

        public static Boolean QueueUserWorkItem(WaitCallback callBack)
        {
            lock (lockObject)
            {
                WaitingTasksQueue.Enqueue(() => callBack(null));
            }

            lock (threadSleeper)
            {
                Monitor.Pulse(threadSleeper);
            }

            NewTaskRuner();

            return true;
        }

        public static Boolean QueueUserWorkItem(WaitCallback callBack, Object state)
        {
            lock (lockObject)
            {
                WaitingTasksQueue.Enqueue(() => callBack(state));
            }

            lock (threadSleeper)
            {
                Monitor.Pulse(threadSleeper);
            }

            NewTaskRuner();

            return true;
        }

        private static void NewTaskRuner()
        {
            if (ThreadsList.Count == 0)
            {
                lock (lockObject)
                {
                    WaitCallbacksList.Add(WaitingTasksQueue.Dequeue());
                }

                ThreadsList.Add(new Thread(() =>
                {
                    while (true)
                    {
                        WaitCallbacksList[0]();

                        if (WaitingTasksQueue.Count != 0)
                        {
                            lock (lockObject)
                            {
                                WaitCallbacksList[0] = WaitingTasksQueue.Dequeue();
                            }
                        }
                        else
                        {
                            WaitCallbacksList[0] = () => { lock (threadSleeper) { Monitor.Wait(threadSleeper); } };
                        }
                    }
                }
                ));

                if (ThreadsList[0].ThreadState != ThreadState.Running)
                {
                    ThreadsList[0].Start();
                    currentCountOfThreads++;
                }
                
            }

            if (WaitingTasksQueue.Count / ThreadsList.Count > 2 && currentCountOfThreads <= maxCountOfThreads)
            {
                while (WaitingTasksQueue.Count / ThreadsList.Count > 2 && currentCountOfThreads <= maxCountOfThreads)
                {
                    int i = currentCountOfThreads;

                    lock (lockObject)
                    {
                        WaitCallbacksList.Add(WaitingTasksQueue.Dequeue());
                    }

                    ThreadsList.Add(new Thread(() =>
                    {

                        while (true)
                        {
                            WaitCallbacksList[i]();

                            if (WaitingTasksQueue.Count != 0)
                            {
                                lock (lockObject)
                                {
                                    WaitCallbacksList[i] = WaitingTasksQueue.Dequeue();
                                }
                            }
                            else
                            {
                                WaitCallbacksList[i] = () => { lock (threadSleeper) { Monitor.Wait(threadSleeper); } };
                            }

                        }
                    }

                    ));

                    if (ThreadsList[i].ThreadState != ThreadState.Running)
                    {
                        ThreadsList[i].Start();
                        currentCountOfThreads++;
                    }
                }
            }
        }
    }
}
