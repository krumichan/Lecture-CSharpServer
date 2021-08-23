using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCore
{
    public class ReceiveBuffer
    {
        ArraySegment<byte> _buffer;

        int _readPosition;
        int _writePosition;

        public ReceiveBuffer(int bufferSize)
        {
            _buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
        }

        public int DataSize { get { return _writePosition - _readPosition; } }
        public int FreeSize { get { return _buffer.Count - _writePosition; } }

        public ArraySegment<byte> ReadSegment // DataSegment
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPosition, DataSize); }
        }

        public ArraySegment<byte> WriteSegment // ReceiveSegment
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePosition, FreeSize); }
        }

        public void Clean()
        {
            int dataSize = DataSize;
            // ReadPosition == WritePosition
            if (dataSize == 0)
            {
                _readPosition = _writePosition = 0;
            }
            // ReadPosition != WritePosition
            else
            {
                Array.Copy(_buffer.Array, _buffer.Offset + _readPosition, _buffer.Array, _buffer.Offset, dataSize);
                _readPosition = 0;
                _writePosition = dataSize;
            }
        }

        public bool OnRead(int numberOfBytes)
        {
            if (numberOfBytes > DataSize)
            {
                return false;
            }

            _readPosition += numberOfBytes;
            return true;
        }

        public bool OnWrite(int numberOfBytes)
        {
            if (numberOfBytes > FreeSize)
            {
                return false;
            }

            _writePosition += numberOfBytes;
            return true;
        }
    }
}
