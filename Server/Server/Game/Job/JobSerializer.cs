﻿using System;
using System.Collections.Generic;

namespace Server.Game
{
    public class JobSerializer
    {
        JobTimer timer = new JobTimer();
        Queue<IJob> jobQueue = new Queue<IJob>();
        object lockObj = new object();
        bool flush = false;

        public IJob PushAfter(int tickAfter, Action action) { return PushAfter(tickAfter, new Job(action)); }
        public IJob PushAfter<T1>(int tickAfter, Action<T1> action, T1 t1) { return PushAfter(tickAfter, new Job<T1>(action, t1)); }
        public IJob PushAfter<T1, T2>(int tickAfter, Action<T1, T2> action, T1 t1, T2 t2) { return PushAfter(tickAfter, new Job<T1, T2>(action, t1, t2)); }
        public IJob PushAfter<T1, T2, T3>(int tickAfter, Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3) { return PushAfter(tickAfter, new Job<T1, T2, T3>(action, t1, t2, t3)); }

        public IJob PushAfter(int tickAfter, IJob job)
        {
            timer.Push(job, tickAfter);
            return job;
        }

        public void Push(Action action) { Push(new Job(action)); }
        public void Push<T1>(Action<T1> action, T1 t1) { Push(new Job<T1>(action, t1)); }
        public void Push<T1, T2>(Action<T1, T2> action, T1 t1, T2 t2) { Push(new Job<T1, T2>(action, t1, t2)); }
        public void Push<T1, T2, T3>(Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3) { Push(new Job<T1, T2, T3>(action, t1, t2, t3)); }

        public void Push(IJob job)
        {
            lock (lockObj)
            {
                jobQueue.Enqueue(job);
            }
        }

        public void Flush()
        {
            timer.Flush();

            while (true)
            {
                IJob job = Pop();
                if (job == null)
                    return;

                job.Execute();
            }
        }

        IJob Pop()
        {
            lock (lockObj)
            {
                if (jobQueue.Count == 0)
                {
                    flush = false;
                    return null;
                }
                return jobQueue.Dequeue();
            }
        }
    }
}
