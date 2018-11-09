﻿using Fluffy.IO.Recycling;

using System;

namespace Fluffy.IO.Buffer
{
    public class BufferObject<T> : IResettable, ICapacityInitiatable
    {
        private  bool _initiated;

        public T[] Value { get; private protected set; }
        public int High { get; internal set; }
        public int Low { get; private set; }
        public int Length => High - Low;
        public int RemainingCapacity => Value.Length - High;

        public BufferObject(int cacheSize)
        {
            Initiate(cacheSize);
        }
        public void Initiate(int capacity)
        {
            if (_initiated)
            {
                throw new AggregateException("Cannot initiate cache twice");
            }

            Value = new T[capacity];
            _initiated = true;
        }
        /// <summary>
        /// Warning: Only use if you know what you are doing
        /// </summary>
        private protected BufferObject()
        {
        }

        /// <summary>
        /// Writes bytes to an <see cref="Value"/> and returns the amount of written bytes
        /// </summary>
        /// <param name="sourceBuffer"></param>
        /// <param name="sourceOffset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public virtual int Write(T[] sourceBuffer, int sourceOffset, int count = -1)
        {
            if (count == -1)
            {
                count = sourceBuffer.Length;
            }

            if (!_initiated)
            {
                throw new AggregateException("Cache not initialized!");
            }

            var localHigh = High;
            var low1 = sourceBuffer.Length - sourceOffset;
            var low2 = Value.Length - localHigh;

            if (low1 < count)
            {
                count = low1;
            }

            if (low2 < count)
            {
                count = low2;
            }

            if (count > 0)
            {
                Array.Copy(sourceBuffer, sourceOffset, Value, localHigh, count);
            }

            High += count;

            return count;
        }

        public virtual int Read(T[] destBuffer, int destOffset, int count = -1)
        {
            if (!_initiated)
            {
                throw new AggregateException("Cache not initialized!");
            }

            if (count == -1)
            {
                count = destBuffer.Length - destOffset;
            }

            var cLen = Length;
            if (cLen < count)
            {
                count = cLen;
            }

            if (count > 0)
            {
                Array.Copy(Value, Low, destBuffer, destOffset, count);
                Low += count;
            }

            return count;
        }

        public virtual void Reset()
        {
            High = 0;
            Low = 0;
        }
    }
}