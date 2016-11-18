using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace Prelude
{    
    public struct Base64Encoding
    {
        readonly byte[] bytes;

        public Base64Encoding(byte[] bytes)
        {
            this.bytes = bytes;
        }

        public static implicit operator string(Base64Encoding bytes) => Convert.ToBase64String(bytes.bytes);
    }

    //public sealed class ParsedUTF8Stream : IEnumerable<utf8>
    //{
    //    const int DEFAULT_BUFFER_LENGTH = 4000;

    //    readonly Stream stream;
    //    readonly List<byte[]> xs = new List<byte[]>();

    //    int total = 0;

    //    public ParsedUTF8Stream(Stream s)
    //    {
    //        stream = s;
    //    }

    //    public IEnumerator<utf8> GetEnumerator()
    //    {
    //        var read = 0;

    //        xs.Clear();

    //        do
    //        {
    //            if (total == 0)
    //                xs.Add(new byte[DEFAULT_BUFFER_LENGTH]);

    //            var buffer = xs[xs.Count - 1];

    //            read = stream.Read(buffer, total, DEFAULT_BUFFER_LENGTH - total);

    //            for (var i = 0; i < read; i++)
    //            {
    //                if(buffer[i] == 0)
    //                {
    //                    var temp = xs[xs.Count - 1] = new byte[i + 1];
                        
    //                    Array.Copy(buffer, temp, temp.Length);

    //                    yield return (utf8)(from x in xs from b in x select b).ToArray();
                        
    //                    temp = new byte[DEFAULT_BUFFER_LENGTH];

    //                    Array.Copy(buffer, i + 1, temp, 0, read - (i + 1));

    //                    xs.Clear();
    //                    xs.Add(temp);

    //                    total = read - i;

    //                    break;

    //                }
    //                else
    //                {
    //                    total = (total + read) % DEFAULT_BUFFER_LENGTH;
    //                }
    //            }

    //        } while (read != 0);
    //    }

    //    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    //    {
    //        return GetEnumerator();
    //    }
    //}
    
    
    //public struct SuperStream
    //{
    //    [ThreadStatic] static readonly byte[] buffer4 = new byte[4];

    //    readonly Stream stream;

    //    public SuperStream(Stream s)
    //    {
    //        this.stream = s;
    //    }

    //    public static implicit operator SuperStream (Stream s)
    //    {
    //        return new SuperStream(s);
    //    }

    //    public static explicit operator int (SuperStream s)
    //    {
    //        var n = s.stream.Read(buffer4, 0, 4);

    //        if (n < 4)
    //            throw new InvalidCastException("Stream has an insufficient number of bytes");

    //        return

    //            ((buffer4[0] << 24)) +
    //            ((buffer4[1] << 16)) +
    //            ((buffer4[2] << 08)) +
    //            ((buffer4[3] << 00));
    //    }

    //    public static explicit operator utf8 (SuperStream s)
    //    {   
    //        s.stream.Length
            
    //        unsafe
    //        {
    //            var read = -1;
    //            var total = 0;
    //            var xs = new List<Tuple<int[], byte[]>>() { Tuple.Create(new[] { 0 }, new byte[4000]) };
    //            var buffer = (byte[])null;

    //            do
    //            {
    //                if (xs[xs.Count - 1].Item1[0] % 4000 == 0)
    //                    xs.Add(Tuple.Create(new[] { 0 }, new byte[4000]));

    //                read = s.Read(buffer, 0, 4000 - total);



    //                xs[xs.Count - 1].Item1[0] += (read + total) % 4000;
    //            } 
    //            while (read != 0);

                
    //        }
    //    }

        
    //    public  bool CanRead
    //    {
    //        get { throw new NotImplementedException(); }
    //    }

    //    public  bool CanSeek
    //    {
    //        get { throw new NotImplementedException(); }
    //    }

    //    public  bool CanWrite
    //    {
    //        get { throw new NotImplementedException(); }
    //    }

    //    public  void Flush()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public  long Length
    //    {
    //        get { throw new NotImplementedException(); }
    //    }

    //    public  long Position
    //    {
    //        get
    //        {
    //            throw new NotImplementedException();
    //        }
    //        set
    //        {
    //            throw new NotImplementedException();
    //        }
    //    }

    //    public  int Read(byte[] buffer, int offset, int count)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public  long Seek(long offset, SeekOrigin origin)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public  void SetLength(long value)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public  void Write(byte[] buffer, int offset, int count)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
    
    
    public abstract class Octets : IEnumerable<byte>
    {   
        sealed class OctetList : Octets
        {
            readonly LinkedList<Octets> list;
            readonly LinkedListNode<Octets> head;
            readonly int total_octets;

            public Octets Head
            {
                get
                {
                    return head.Value;
                }
            }

            public OctetList(Octets octs) 
                : this(octs, new LinkedList<Octets>()) { }

            public OctetList(Octets head, LinkedList<Octets> rest)
            {
                total_octets = rest.Sum(x => x.Length) + head.Length;
                
                (list = rest).AddFirst(this.head = new LinkedListNode<Octets>(head));
            }

            public override Octets Combine(Octets y)
            {
                return new OctetList(y, list);
            }

            public override byte[] GetBytes()
            {
                var current = head;
                var xs = new List<byte[]>();

                do
                {
                    xs.Add(current.Value.GetBytes());
                    current = current.Next;
                }
                while (current != null);

                return (from bytes in xs from b in bytes select b).ToArray();
            }

            public override int Length
            {
                get 
                {
                    var total = 0;

                    foreach (var item in list)
                        total += item.Length;

                    return total;
                }
            }
        }

        public virtual Octets Combine(Octets x)
        {
            var xs = new LinkedList<Octets>();
            xs.AddFirst(this);

            return new OctetList(x, xs);
        }

        sealed class DefaultOctetList : Octets
        {
            readonly byte[] bytes;

            public DefaultOctetList(byte[] bytes)
            {
                this.bytes = bytes;
            }

            public override byte[] GetBytes()
            {
                return bytes;
            }

            public override int Length
            {
                get { return bytes.Length; }
            }
        }
        
        abstract class Int16Octets : Octets
        {
            public sealed override int Length
            {
                get { return 2; }
            }
            
            internal sealed class Signed : Int16Octets
            {
                readonly short n;

                public Signed(short n)
                {
                    this.n = n;
                }

                public override byte[] GetBytes()
                {
                    return new[]
                    {
                        (byte)((n >> 08) & 0xFF),
                        (byte)((n >> 00) & 0xFF)
                    };
                }
            }

            internal sealed class Unsigned : Int16Octets
            {
                readonly ushort n;

                public Unsigned(ushort n)
                {
                    this.n = n;
                }

                public override byte[] GetBytes()
                {
                    return new[]
                {
                    (byte)((n >> 08) & 0xFF),
                    (byte)((n >> 00) & 0xFF)
                };
                }
            }
        }

        abstract class Int32Octets : Octets
        {
            public sealed override int Length
            {
                get { return 4; }
            }

            internal sealed class Signed : Int32Octets
            {
                readonly int n;

                public Signed(int n)
                {
                    this.n = n;
                }

                public override byte[] GetBytes()
                {
                    return new[]
                    {
                        (byte)((n >> 24) & 0xFF),
                        (byte)((n >> 16) & 0xFF),
                        (byte)((n >> 08) & 0xFF),
                        (byte)((n >> 00) & 0xFF)
                    };
                }
            }

            internal sealed class Unsigned : Int32Octets
            {
                readonly uint n;

                public Unsigned(uint n)
                {
                    this.n = n;
                }

                public override byte[] GetBytes()
                {
                    return new[]
                {
                    (byte)((n >> 24) & 0xFF),
                    (byte)((n >> 16) & 0xFF),
                    (byte)((n >> 08) & 0xFF),
                    (byte)((n >> 00) & 0xFF)
                };
                }
            }
        }

        public abstract byte[] GetBytes();

        public abstract int Length { get; }

        public void CopyTo(VectorSegment<byte> vs)
        {
            var bytes = this.GetBytes();
            
            for (var i = 0; i < vs.Length; i++)
                vs[i] = bytes[i];
        }
        
        public void CopyTo(byte[] array)
        {
            GetBytes().CopyTo(array, 0);
        }

        public void CopyTo(byte[] array, int n)
        {
            GetBytes().CopyTo(array, n);
        }

        public void CopyTo(byte[] array, long n)
        {
            GetBytes().CopyTo(array, n);
        }

        public static Octets operator *(Octets octet1, Octets octet2)
        {
            return octet1.Combine(octet2);
        }

        public static implicit operator byte[](Octets octets)
        {
            return octets.GetBytes();
        }

        public static implicit operator Octets(byte[] bytes)
        {
            return new DefaultOctetList(bytes);
        }

        public static implicit operator Octets(int n)
        {
            return new Int32Octets.Signed(n);
        }

        public static implicit operator Octets(uint n)
        {
            return new Int32Octets.Unsigned(n);
        }

        public static implicit operator Octets(short n)
        {
            return new Int16Octets.Signed(n);
        }

        public static implicit operator Octets(ushort n)
        {
            return new Int16Octets.Unsigned(n);
        }

        public static Octets Create(int n)
        {
            return new Int32Octets.Signed(n);
        }

        public IEnumerator<byte> GetEnumerator()
        {
            return (GetBytes() as IEnumerable<byte>).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    
    
    public static class Int32
    {
        public static byte[] GetOctets(this int n)
        {
            return new[]
            {
                (byte)((n >> 24) & 0xFF),
                (byte)((n >> 16) & 0xFF),
                (byte)((n >> 08) & 0xFF),
                (byte)((n >> 00) & 0xFF)
            };
        }

        /// <summary>
        /// Converts a byte array (with length greater than or equal to 4) to a System.Int32 object, assuming a big-endian encoding
        /// </summary>
        /// <param name="octets"></param>
        /// <exception cref="System.ArgumentException"></exception>
        /// <returns></returns>        
        public static int GetInt32(this byte[] octets)
        {
            if (octets.Length < 4)
                throw new ArgumentException("Byte array must have length greater than or equal to 4.");
            
            return

                ((octets[0] << 24) & 0xFF) +
                ((octets[1] << 16) & 0xFF) +
                ((octets[2] << 08) & 0xFF) +
                ((octets[3] << 00) & 0xFF);
        }
    }
}
