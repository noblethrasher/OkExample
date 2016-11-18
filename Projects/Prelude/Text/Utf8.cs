using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prelude
{
    public static class TextUtils
    {
        public static void Write(this Stream s, utf8 enc)
        {
            byte[] buffer = enc;

            s.Write(buffer, 0, buffer.Length);
        }
    }


    public abstract class utf8
    {
        public abstract byte[] GetBytes();
        public abstract string GetString();
        public abstract Stream GetStream();

        public abstract override string ToString();

        public int Length
        {
            get
            {
                return GetBytes().Length;
            }
        }

        public long LongLength
        {
            get
            {
                return GetBytes().LongLength;
            }
        }

        public static implicit operator utf8(string s)
        {
            return new utf8_encoding(s);
        }

        public static implicit operator utf8(Stream s)
        {
            return new utf8_decoding(s);
        }

        public static implicit operator utf8(byte[] bytes)
        {
            return new utf8_decoding(bytes);
        }

        public static implicit operator string(utf8 enc)
        {
            return enc.ToString();
        }

        public static implicit operator byte[](utf8 enc)
        {
            return enc.GetBytes();
        }

        public static implicit operator Octets(utf8 enc)
        {
            return enc.GetBytes();
        }

        sealed class utf8_encoding : utf8
        {
            readonly byte[] bytes;

            public utf8_encoding(string s)
            {
                bytes = Encoding.UTF8.GetBytes(s);
            }

            public override byte[] GetBytes()
            {
                return bytes;
            }

            public override string GetString()
            {
                return Encoding.UTF8.GetString(bytes);
            }

            public override string ToString()
            {
                return "<<UTF8 Encoding>>";
            }

            public override Stream GetStream()
            {
                return new MemoryStream(bytes);
            }
        }

        sealed class utf8_decoding : utf8
        {
            readonly string s;
            readonly byte[] buffer;

            public utf8_decoding(byte[] buffer)
            {
                s = Encoding.UTF8.GetString(buffer);
                this.buffer = buffer;
            }

            public utf8_decoding(Stream s)
            {
                long? position = null;

                if (s.CanSeek)
                {
                    position = s.Position;
                    s.Position = 0;
                }

                using (var sr = new StreamReader(s, Encoding.UTF8))
                    this.s = sr.ReadToEnd();
            }

            public override byte[] GetBytes()
            {
                return Encoding.UTF8.GetBytes(s);
            }

            public override string GetString()
            {
                return s;
            }

            public override string ToString()
            {
                return s;
            }

            public override Stream GetStream()
            {
                return new MemoryStream(buffer != null ? buffer : Encoding.UTF8.GetBytes(s));
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

        sealed class UTF8_Stream : SeekableReadonlyStream
        {
            readonly Stream stream;
            static readonly byte[] @byte = new byte[0];

            public UTF8_Stream(Stream s)
            {
                stream = s;
            }

            protected override byte this[long n]
            {
                get
                {
                    var old_position = Position;
                    byte b;
                    Position = n;

                    lock (@byte)
                    {
                        stream.Read(@byte, 0, 1);
                        b = @byte[0];
                    }

                    Position = old_position;

                    return b;
                }
            }

            public override long Length
            {
                get { return stream.Length; }
            }

            public override long Position
            {
                get
                {
                    return stream.Position;
                }
                set
                {
                    stream.Position = value;
                }
            }
        }
    }
}
