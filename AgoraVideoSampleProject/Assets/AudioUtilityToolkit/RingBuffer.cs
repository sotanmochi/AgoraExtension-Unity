
using System;
using System.Runtime.InteropServices;

namespace AudioUtilityToolkit
{
    public sealed class RingBuffer<T> where T : struct
    {
        public int Count => _count;
        public int Capacity => _buffer.Length;
        public int FreeCount => Capacity - Count;

        public int Head => _head;
        public int Tail
        {
            get
            {
                int tail = _head + _count;
                if (tail >= _buffer.Length) tail -= _buffer.Length;
                return tail;
            }
        }

        private readonly T[] _buffer;
        private readonly int _bufferMask;
        private int _head;
        private int _count;

        public RingBuffer(int capacity)
        {
            int power = (int)Math.Ceiling(Math.Log(capacity, 2)); // x = log2(capacity)
            int bufferSize = (int)Math.Pow(2, power); // y = 2^x
            _buffer = new T[bufferSize]; // Buffer size should be a power of two.
            _bufferMask = bufferSize - 1;
        }

        public void Clear()
        {
            _head = _count = 0;
        }

        public void Enqueue(ReadOnlySpan<T> data)
        {
            if (FreeCount == 0)
            {
                return;
            }

            if (data.Length > FreeCount)
            {
                data = data.Slice(0, FreeCount);
            }

            var tail = (_head + _count) & _bufferMask;

            var backBuffer = new Span<T>(_buffer, tail, Capacity - tail);
            var frontBuffer = new Span<T>(_buffer, 0, _head);

            if (_head <= tail && backBuffer.Length < data.Length)
            {
                data.Slice(0, backBuffer.Length).CopyTo(backBuffer);
                data.Slice(backBuffer.Length).CopyTo(frontBuffer);
            }
            else
            {
                data.CopyTo(backBuffer);
            }

            _count += data.Length;
        }

        public void EnqueueDefault(int length)
        {
            if (FreeCount == 0)
            {
                return;
            }

            if (length > FreeCount)
            {
                length = FreeCount;
            }

            var tail = (_head + _count) & _bufferMask;

            var backBuffer = new Span<T>(_buffer, tail, Capacity - tail);
            var frontBuffer = new Span<T>(_buffer, 0, _head);

            if (_head <= tail && backBuffer.Length < length)
            {
                backBuffer.Fill(default);
                frontBuffer.Slice(0, length - backBuffer.Length).Fill(default);
            }
            else
            {
                backBuffer.Slice(0, length).Fill(default);
            }

            _count += length;
        }

        public void Enqueue(ReadOnlySpan<byte> data)
        {
            Enqueue(MemoryMarshal.Cast<byte, T>(data));
        }

        public void Dequeue(Span<T> dest)
        {
            if (Count == 0)
            {
                return;
            }

            if (dest.Length > Count)
            {
                dest = dest.Slice(0, Count);
            }

            var tail = (_head + _count) & _bufferMask;

            if (tail <= _head && Capacity - _head < dest.Length)
            {
                var part = Capacity - _head;
                new Span<T>(_buffer, _head, part).CopyTo(dest);
                new Span<T>(_buffer, 0, dest.Length - part).CopyTo(dest.Slice(part));
            }
            else
            {
                new Span<T>(_buffer, _head, dest.Length).CopyTo(dest);
            }

            _head = (_head + dest.Length) & _bufferMask;
            _count -= dest.Length;
        }
    }
}