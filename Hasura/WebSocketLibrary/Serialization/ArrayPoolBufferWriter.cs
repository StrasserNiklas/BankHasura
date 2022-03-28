using System;
using System.Buffers;

namespace WebSocketLibrary.Serialization
{
    public sealed class ArrayPoolBufferWriter : IBufferWriter<byte>, IDisposable
    {
        private const int MinimumBufferSize = 256;
        private byte[] rentedBuffer;
        private int written;

        public ArrayPoolBufferWriter()
        {
            this.rentedBuffer = ArrayPool<byte>.Shared.Rent(MinimumBufferSize);
            this.written = 0;
        }

        public ArrayPoolBufferWriter(int initialCapacity)
        {
            if (initialCapacity <= 0)
            {
                throw new ArgumentException(nameof(initialCapacity));
            }

            this.rentedBuffer = ArrayPool<byte>.Shared.Rent(initialCapacity);
            this.written = 0;
        }

        public ReadOnlyMemory<byte> OutputAsMemory
        {
            get
            {
                this.CheckIfDisposed();
                return this.rentedBuffer.AsMemory(0, this.written);
            }
        }

        public ArraySegment<byte> OutputAsArraySegment
        {
            get
            {
                this.CheckIfDisposed();
                return new ArraySegment<byte>(this.rentedBuffer, 0, this.written);
            }
        }

        public ReadOnlySpan<byte> OutputAsSpan
        {
            get
            {
                this.CheckIfDisposed();
                return this.rentedBuffer.AsSpan(0, this.written);
            }
        }

        public void Advance(int count)
        {
            this.CheckIfDisposed();

            if (count < 0)
            {
                throw new ArgumentException(nameof(count));
            }

            if (this.written > this.rentedBuffer.Length - count)
            {
                throw new InvalidOperationException("Advancing too far.");
            }

            this.written += count;
        }

        public Memory<byte> GetMemory(int sizeHint)
        {
            this.CheckIfDisposed();

            if (sizeHint < 0)
            {
                throw new ArgumentException(nameof(sizeHint));
            }

            this.CheckAndResizeBuffer(sizeHint);
            return this.rentedBuffer.AsMemory(this.written);
        }

        public Span<byte> GetSpan(int sizeHint)
        {
            this.CheckIfDisposed();

            if (sizeHint < 0)
            {
                throw new ArgumentException(nameof(sizeHint));
            }

            this.CheckAndResizeBuffer(sizeHint);
            return this.rentedBuffer.AsSpan(this.written);
        }

        // Returns the rented buffer back to the pool
        public void Dispose()
        {
            if (this.rentedBuffer == null)
            {
                return;
            }

            ArrayPool<byte>.Shared.Return(this.rentedBuffer);
            this.rentedBuffer = null;
            this.written = 0;
        }

        public void Reset()
        {
            this.CheckIfDisposed();
            this.rentedBuffer.AsSpan(0, this.written).Clear();
            this.written = 0;
        }

        public ArraySegment<byte> GetArraySegment(int sizeHint)
        {
            this.CheckIfDisposed();

            if (sizeHint < 0)
            {
                throw new ArgumentException(nameof(sizeHint));
            }

            this.CheckAndResizeBuffer(sizeHint);

            return new ArraySegment<byte>(this.rentedBuffer, this.written, this.rentedBuffer.Length - this.written);
        }

        private void CheckAndResizeBuffer(int sizeHint)
        {
            if (sizeHint == 0)
            {
                sizeHint = MinimumBufferSize;
            }

            var availableSpace = this.rentedBuffer.Length - this.written;

            if (sizeHint > availableSpace)
            {
                var growBy = sizeHint > this.rentedBuffer.Length ? sizeHint : this.rentedBuffer.Length;
                var newSize = checked(this.rentedBuffer.Length + growBy);
                var oldBuffer = this.rentedBuffer;

                this.rentedBuffer = ArrayPool<byte>.Shared.Rent(newSize);

                oldBuffer.AsSpan(0, this.written).CopyTo(this.rentedBuffer);
                ArrayPool<byte>.Shared.Return(oldBuffer);
            }
        }

        private void CheckIfDisposed()
        {
            if (this.rentedBuffer == null)
            {
                throw new ObjectDisposedException(nameof(ArrayPoolBufferWriter));
            }
        }
    }
}
