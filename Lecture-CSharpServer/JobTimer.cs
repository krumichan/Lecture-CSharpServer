using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using ServerCore;

namespace Lecture_CSharpServer
{
    struct JobTimerElement : IComparable<JobTimerElement>
    {
        public int executeTick; // 실행시간.
        public Action action;

        public int CompareTo(JobTimerElement other)
        {
            return other.executeTick - executeTick;
        }
    }

    class JobTimer
    {
        PriorityQueue<JobTimerElement> _pq = new PriorityQueue<JobTimerElement>();
        object _lock = new object();

        public static JobTimer Instance { get; } = new JobTimer();

        public void Push(Action action, int tickAfter = 0)
        {
            JobTimerElement job;
            job.executeTick = System.Environment.TickCount + tickAfter;
            job.action = action;

            lock (_lock)
            {
                _pq.Push(job);
            }
        }

        public void Flush()
        {
            while (true)
            {
                int now = System.Environment.TickCount;

                JobTimerElement job;

                lock (_lock)
                {
                    if (_pq.Count == 0)
                    {
                        break;
                    }

                    // 가장 앞 Element의 시간이 가장 빠른 시간.
                    // 따라서 가장 앞 Element 시간이 미래라면 바로 종료.
                    job = _pq.Peek();
                    if (job.executeTick > now)
                    {
                        break;
                    }

                    _pq.Pop();
                }

                job.action.Invoke();
            }
        }
    }
}
