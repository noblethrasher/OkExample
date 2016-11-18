using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Prelude
{
    public sealed class ConcatenatedStream : Stream
    {
        long length;

        readonly Queue<Stream> processed_streams = new Queue<Stream>();
        readonly Queue<Stream> pending_streams;

        private ConcatenatedStream()
        {
            pending_streams = new Queue<Stream>();
        }

        public ConcatenatedStream(byte[] bytes) : this(new [] {new MemoryStream(bytes)})
        {
 
        }

        public ConcatenatedStream(IEnumerable<Stream> streams)
        {
            pending_streams = new Queue<Stream>(streams);
            length = (from stream in streams select stream.Length).Sum();
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
            return;
        }

        public override long Length
        {
            get { return length; }
        }

        public override long Position
        {
            get
            {
                var total = processed_streams.Sum(x => x.Length);

                if (pending_streams.Count > 0)
                    total += pending_streams.Peek().Position;

                return total;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = 0;

            while (pending_streams.Count > 0 && read < count)
            {
                var head = pending_streams.Peek();

                if (head.Position >= head.Length)
                    processed_streams.Enqueue(pending_streams.Dequeue());
                else
                    read += head.Read(buffer, offset + read, count - read);
            }

            return read;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var stream in processed_streams)
                    stream.Dispose();

                foreach (var stream in pending_streams)
                    stream.Dispose();

                GC.SuppressFinalize(this);
            }
        }

        public static implicit operator ConcatenatedStream(uint n)
        {
            var buffer = new[]
            {
                (byte)((n >> 24) & 0xFF),
                (byte)((n >> 16) & 0xFF),
                (byte)((n >> 08) & 0xFF),
                (byte)((n >> 00) & 0xFF),
            };

            return new ConcatenatedStream(buffer);
        }

        public static implicit operator ConcatenatedStream(int n)
        {
            var buffer = new[]
            {
                (byte)((n >> 24) & 0xFF),
                (byte)((n >> 16) & 0xFF),
                (byte)((n >> 08) & 0xFF),
                (byte)((n >> 00) & 0xFF),
            };

            return new ConcatenatedStream(buffer);
        }

        public static implicit operator ConcatenatedStream(short n)
        {
            var buffer = new[]
            {
                (byte)((n >> 08) & 0xFF),
                (byte)((n >> 00) & 0xFF),
            };

            return new ConcatenatedStream(buffer);
        }

        public static implicit operator ConcatenatedStream(ushort n)
        {
            var buffer = new[]
            {
                (byte)((n >> 08) & 0xFF),
                (byte)((n >> 00) & 0xFF),
            };

            return new ConcatenatedStream(buffer);
        }

        public static implicit operator ConcatenatedStream(long n)
        {
            var buffer = new[]
            {
                (byte)((n >> 56) & 0xFF),
                (byte)((n >> 48) & 0xFF),
                (byte)((n >> 40) & 0xFF),
                (byte)((n >> 32) & 0xFF),
                
                (byte)((n >> 24) & 0xFF),
                (byte)((n >> 16) & 0xFF),
                (byte)((n >> 08) & 0xFF),
                (byte)((n >> 00) & 0xFF),
            };

            return new ConcatenatedStream(buffer);
        }

        public static implicit operator ConcatenatedStream(ulong n)
        {
            var buffer = new[]
            {
                (byte)((n >> 56) & 0xFF),
                (byte)((n >> 48) & 0xFF),
                (byte)((n >> 40) & 0xFF),
                (byte)((n >> 32) & 0xFF),
                
                (byte)((n >> 24) & 0xFF),
                (byte)((n >> 16) & 0xFF),
                (byte)((n >> 08) & 0xFF),
                (byte)((n >> 00) & 0xFF),
            };

            return new ConcatenatedStream(buffer);
        }

        public static implicit operator ConcatenatedStream(byte[] bytes)
        {
            return new ConcatenatedStream(bytes);
        }

        public static implicit operator ConcatenatedStream(Stream[] strms)
        {
            return new ConcatenatedStream(strms);
        }

        public static ConcatenatedStream operator +(ConcatenatedStream cs, IEnumerable<Stream> xs)
        {
            var cstream = new ConcatenatedStream();

            cstream.pending_streams.Enqueue(cs);
            cstream.length += cs.length;

            foreach (var x in xs)
            {
                cstream.pending_streams.Enqueue(x);
                cstream.length += x.Length;
            }

            return cstream;
        }

        static ConcatenatedStream ConcatenateStream(Stream l, Stream r)
        {
            var cstream = new ConcatenatedStream();

            cstream.pending_streams.Enqueue(l);
            cstream.pending_streams.Enqueue(r);

            cstream.length = l.Length + r.Length;

            return cstream;
        }

        public static ConcatenatedStream operator +(ConcatenatedStream l, ConcatenatedStream r)
        {
            return ConcatenateStream(l, r);
        }

        public static ConcatenatedStream operator +(ConcatenatedStream cs, Stream str)
        {
            return ConcatenateStream(cs, str);
        }
    }

    abstract class SeekableReadonlyStream : Stream
    {
        public sealed override bool CanRead
        {
            get { return true; }
        }

        public sealed override bool CanSeek
        {
            get { return true; }
        }

        public sealed override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
            return;
        }

        protected abstract byte this[long n] { get; }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var curr = Position;

            for (var i = 0; i < Length && i < count; i++)
                if (Position >= Length)
                    break;
                else
                    buffer[offset + i] = this[Position++];

            return (int)(Position - curr);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    {
                        Position = offset;
                        break;
                    }

                case SeekOrigin.Current:
                    {
                        Position = Position + offset;
                        break;
                    }

                case SeekOrigin.End:
                    {
                        Position = Length + offset;
                        break;
                    }
            }

            return Position;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}