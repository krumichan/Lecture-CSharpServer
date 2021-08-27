using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lecture_CSharpServer
{
    // JobQueue를 사용하는 Class는 별도의 lock을 설정하지 않아도 된다.
    // JobQueue에서 lock을 수행하고 있어서,
    // 단일 Thread 실행을 보장하기 때문이다.
    class GameRoom : IJobQueue
    {
        List<ClientSession> _sessions = new List<ClientSession>();
        JobQueue _jobQueue = new JobQueue();

        List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();

        public void Push(Action job)
        {
            _jobQueue.Push(job);
        }

        public void Flush()
        {
            foreach (ClientSession s in _sessions)
            {
                s.Send(_pendingList);
            }

            Console.WriteLine($"Flushed {_pendingList.Count} items");
            _pendingList.Clear();
        }

        public void Broadcast(ClientSession session, string chat)
        {
            // 아래 4줄은 Thread 들이 공유하는 Data가 아니다.
            S_Chat packet = new S_Chat();
            packet.playerId = session.SessionId;
            packet.chat = $"{session.SessionId} : {chat}";
            ArraySegment<byte> segment = packet.Write();

            _pendingList.Add(segment);

            // O(N^2)
            /*foreach (ClientSession s in _sessions)
            {
                s.Send(segment);
            }*/
        }

        public void Enter(ClientSession session)
        {
            _sessions.Add(session);
            session.Room = this;
        }

        public void Leave(ClientSession session)
        {
            _sessions.Remove(session);
        }
    }
}
