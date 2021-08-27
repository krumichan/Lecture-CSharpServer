using System;
using System.Collections.Generic;
using System.Text;

namespace Lecture_CSharpServer
{
    class GameRoom
    {
        List<ClientSession> _sessions = new List<ClientSession>();
        object _lock = new object();

        public void Broadcast(ClientSession session, string chat)
        {
            // 아래 4줄은 Thread 들이 공유하는 Data가 아니다.
            S_Chat packet = new S_Chat();
            packet.playerId = session.SessionId;
            packet.chat = $"{session.SessionId} : {chat}";
            ArraySegment<byte> segment = packet.Write();

            lock (_lock)
            {
                foreach (ClientSession s in _sessions)
                {
                    s.Send(segment);
                }
            }
        }

        public void Enter(ClientSession session)
        {
            lock (_lock)
            {
                _sessions.Add(session);
                session.Room = this;
            }
        }

        public void Leave(ClientSession session)
        {
            lock (_lock)
            {
                _sessions.Remove(session);
            }
        }
    }
}
