using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Prelude
{
    using DamienG.Security.Cryptography;
    using little_endian = Prelude.LittleEndianBytes;
    using FileHeaders = System.IO.Stream;

    struct SeekableStream
    {
        readonly Stream strm;

        public SeekableStream(Stream strm)
        {
            if (!strm.CanSeek || !strm.CanRead)
                throw new ArgumentException("Stream must be seekable.");
            else
                this.strm = strm;
        }

        public void CopyTo(Stream strm)
        {
            this.strm.CopyTo(strm);
        }

        public long Length
        {
            get
            {
                return strm.Length;
            }
        }

        public static implicit operator bool(SeekableStream strm)
        {
            return strm.strm != null && strm.strm.CanSeek && strm.strm.CanRead;
        }

        public static implicit operator SeekableStream(Stream strm)
        {
            return new SeekableStream(strm);
        }

        public static implicit operator Stream(SeekableStream strm)
        {
            return strm.strm;
        }
    }

    abstract class LittleEndianBytes
    {
        public static LittleEndianBytes EmptyBytes = new Empty();

        protected abstract byte[] GetBytes();
        protected abstract Stream GetStream();
        public abstract long Length { get; }

        public static implicit operator byte[](LittleEndianBytes bytes)
        {
            return bytes.GetBytes();
        }

        public override string ToString()
        {
            return string.Join(" ", from b in GetBytes() select b.ToString("X2"));
        }

        public static implicit operator LittleEndianBytes(short n)
        {
            return new LittleEndian16(n);
        }

        public static implicit operator LittleEndianBytes(ushort n)
        {
            return new LittleEndian16(n);
        }

        public static implicit operator LittleEndianBytes(int n)
        {
            return new LittleEndian32(n);
        }

        public static implicit operator LittleEndianBytes(uint n)
        {
            return new LittleEndian32(n);
        }

        public static implicit operator LittleEndianBytes(long n)
        {
            return new LittleEndian64(n);
        }

        public static implicit operator LittleEndianBytes(ulong n)
        {
            return new LittleEndian64(n);
        }

        public static implicit operator Stream(LittleEndianBytes bytes)
        {
            return bytes.GetStream();
        }

        sealed class Empty : LittleEndianBytes
        {
            static readonly byte[] empty = new byte[0];
            static readonly MemoryStream ms = new MemoryStream();

            protected override byte[] GetBytes()
            {
                return empty;
            }

            public override long Length
            {
                get { return 0; }
            }

            protected override Stream GetStream()
            {
                return ms;
            }
        }

        public sealed class LittleEndian16 : LittleEndianBytes
        {
            readonly byte b1, b2;

            public LittleEndian16(short n)
            {
                b1 = (byte)((n >> 00) & 0xFF);
                b2 = (byte)((n >> 08) & 0xFF);
            }

            public LittleEndian16(ushort n)
            {
                b1 = (byte)((n >> 00) & 0xFF);
                b2 = (byte)((n >> 08) & 0xFF);
            }

            protected override byte[] GetBytes()
            {
                return new[] { b1, b2 };
            }

            public static implicit operator LittleEndian16(short n)
            {
                return new LittleEndian16(n);
            }

            public static implicit operator LittleEndian16(ushort n)
            {
                return new LittleEndian16(n);
            }

            public override long Length
            {
                get { return 2; }
            }

            protected override Stream GetStream()
            {
                return new Stream2(this);
            }

            sealed class Stream2 : SeekableReadonlyStream
            {
                long position = 0;
                LittleEndian16 bytes;

                public Stream2(LittleEndian16 bytes)
                {
                    this.bytes = bytes;
                }

                protected override byte this[long n]
                {
                    get
                    {
                        switch (n)
                        {
                            case 0:
                                return bytes.b1;

                            case 1:
                                return bytes.b2;

                            default:
                                throw new IndexOutOfRangeException();
                        }

                    }
                }

                public override long Length
                {
                    get { return bytes.Length; }
                }

                public override long Position
                {
                    get
                    {
                        return position;
                    }
                    set
                    {
                        if (value > 2 || value < 0)
                            throw new IndexOutOfRangeException();

                        position = value;
                    }
                }
            }
        }

        public sealed class LittleEndian32 : LittleEndianBytes
        {
            readonly byte b1, b2, b3, b4;

            public LittleEndian32(int n)
            {
                b1 = (byte)((n >> 00) & 0xFF);
                b2 = (byte)((n >> 08) & 0xFF);
                b3 = (byte)((n >> 16) & 0xFF);
                b4 = (byte)((n >> 24) & 0xFF);
            }

            public LittleEndian32(uint n)
            {
                b1 = (byte)((n >> 00) & 0xFF);
                b2 = (byte)((n >> 08) & 0xFF);
                b3 = (byte)((n >> 16) & 0xFF);
                b4 = (byte)((n >> 24) & 0xFF);
            }

            protected override byte[] GetBytes()
            {
                return new[] { b1, b2, b3, b4 };
            }

            public static implicit operator LittleEndian32(int n)
            {
                return new LittleEndian32(n);
            }

            public static implicit operator LittleEndian32(uint n)
            {
                return new LittleEndian32(n);
            }

            public override long Length
            {
                get { return 4; }
            }

            protected override Stream GetStream()
            {
                return new Stream4(this);
            }

            sealed class Stream4 : SeekableReadonlyStream
            {
                long position = 0;
                LittleEndian32 bytes;

                public Stream4(LittleEndian32 bytes)
                {
                    this.bytes = bytes;
                }

                protected override byte this[long n]
                {
                    get
                    {
                        switch (n)
                        {
                            case 0:
                                return bytes.b1;

                            case 1:
                                return bytes.b2;

                            case 2:
                                return bytes.b3;

                            case 3:
                                return bytes.b4;

                            default:
                                throw new IndexOutOfRangeException();
                        }

                    }
                }

                public override long Length
                {
                    get { return bytes.Length; }
                }

                public override long Position
                {
                    get
                    {
                        return position;
                    }
                    set
                    {
                        if (value > 4 || value < 0)
                            throw new IndexOutOfRangeException();

                        position = value;
                    }
                }
            }
        }

        public sealed class LittleEndian64 : LittleEndianBytes
        {
            readonly byte b1, b2, b3, b4, b5, b6, b7, b8;

            public LittleEndian64(long n)
            {
                b1 = (byte)((n >> 00) & 0xFF);
                b2 = (byte)((n >> 08) & 0xFF);
                b3 = (byte)((n >> 16) & 0xFF);
                b4 = (byte)((n >> 24) & 0xFF);
                b5 = (byte)((n >> 32) & 0xFF);
                b6 = (byte)((n >> 40) & 0xFF);
                b7 = (byte)((n >> 48) & 0xFF);
                b8 = (byte)((n >> 56) & 0xFF);
            }

            public LittleEndian64(ulong n)
            {
                b1 = (byte)((n >> 00) & 0xFF);
                b2 = (byte)((n >> 08) & 0xFF);
                b3 = (byte)((n >> 16) & 0xFF);
                b4 = (byte)((n >> 24) & 0xFF);
                b5 = (byte)((n >> 32) & 0xFF);
                b6 = (byte)((n >> 40) & 0xFF);
                b7 = (byte)((n >> 48) & 0xFF);
                b8 = (byte)((n >> 56) & 0xFF);
            }

            protected override byte[] GetBytes()
            {
                return new[] { b1, b2, b3, b4, b5, b6, b7, b8 };
            }

            public static implicit operator LittleEndian64(long n)
            {
                return new LittleEndian64(n);
            }

            public static implicit operator LittleEndian64(ulong n)
            {
                return new LittleEndian64(n);
            }

            public override long Length
            {
                get { return 8; }
            }

            protected override Stream GetStream()
            {
                return new Stream8(this);
            }

            sealed class Stream8 : SeekableReadonlyStream
            {
                long position = 0;
                LittleEndian64 bytes;

                public Stream8(LittleEndian64 bytes)
                {
                    this.bytes = bytes;
                }

                protected override byte this[long n]
                {
                    get
                    {
                        switch (n)
                        {
                            case 0:
                                return bytes.b1;

                            case 1:
                                return bytes.b2;

                            case 2:
                                return bytes.b3;

                            case 3:
                                return bytes.b4;

                            case 4:
                                return bytes.b5;

                            case 5:
                                return bytes.b6;

                            case 6:
                                return bytes.b7;

                            case 7:
                                return bytes.b8;

                            default:
                                throw new IndexOutOfRangeException();
                        }
                    }
                }

                public override long Length
                {
                    get { return bytes.Length; }
                }

                public override long Position
                {
                    get
                    {
                        return position;
                    }
                    set
                    {
                        if (value > 8 || value < 0)
                            throw new IndexOutOfRangeException();

                        position = value;
                    }
                }
            }
        }
    }

    public class ZipFile : Stream
    {
        internal static readonly little_endian
            CENTRAL_DIRECTORY_FILE_HEADER_SIGNATURE = (uint)0x02014b50,
            END_OF_CENTRAL_DIRECTORY_SIGNATURE = (uint)0x06054b50,
            ZIP64_END_OF_CENTRAL_DIRECTORY_SIGNATURE = (uint)0x06064b50,
            ZIP64_END_OF_CENTRAL_DIRECTORY_LOCATOR_SIGNATURE = (uint)0x07064b50,
            VERSION_MADE_BY = (ushort)10,
            VERSION_NEEDED_TO_EXTRACT = (ushort)50;


        readonly Dictionary<long, ZipEntry> offsets = new Dictionary<long, ZipEntry>();
        readonly IEnumerable<ZipEntry> entries;

        readonly Stream stream;

        public ZipFile(IEnumerable<ZipEntry> entries)
        {
            this.entries = entries;

            var offset = 0L;

            foreach (var entry in entries)
            {
                offsets.Add(offset, entry);
                offset += entry.Length;
            }

            var main_entries = new ConcatenatedStream(from entry in entries select (Stream)entry);

            var central_directory = new List<Stream>();

            foreach (var entry in offsets)
            {
                FileHeaders

                    _offset = (little_endian)(int)entry.Key,
                    total_records = (little_endian)entries.Count(),
                    made_by = (little_endian)(ushort)19,
                    version_needed = ZipEntry.VERSION_NEEDED,
                    general = ZipEntry.GENERAL,
                    method = ZipEntry.COMPRESSION_METHOD,
                    last_mod_time = entry.Value.last_mod_time,
                    last_mod_date = entry.Value.last_mod_date,
                    crc32 = entry.Value.crc32,
                    compressed_size = entry.Value.compressed_size,
                    uncompressed_size = entry.Value.uncompressed_size,
                    file_name_length = entry.Value.file_name_length,
                    file_name_bytes = new MemoryStream(entry.Value.file_name_bytes),
                    file_comment_length = (little_endian)(ushort)0,
                    disk_where_file_starts = (little_endian)(short)0,
                    internal_file_attributes = (little_endian)(short)0,
                    external_file_attributes = (little_endian)(int)0,
                    extra_field_length = ZipEntry.EXTRA_FIELD_LENGTH,
                    zip64_id = entry.Value.zip64_id,
                    zip64_len = entry.Value.zip64_len,
                    zip64_compressed = entry.Value.zip64_compressed,
                    zip64_uncompressed = entry.Value.zip64_uncompressed;

                ConcatenatedStream directory_entry =

                    new[]
                    {
                        CENTRAL_DIRECTORY_FILE_HEADER_SIGNATURE,
                        made_by,
                        version_needed,
                        general,
                        method,
                        last_mod_time,
                        last_mod_date,
                        crc32,
                        compressed_size,
                        uncompressed_size,
                        file_name_length,
                        extra_field_length,
                        file_comment_length,
                        disk_where_file_starts,
                        internal_file_attributes,
                        external_file_attributes,
                        _offset,
                        file_name_bytes,
                        zip64_id,
                        zip64_len,
                        zip64_compressed,
                        zip64_uncompressed
                    };

                central_directory.Add(directory_entry);
            }

            FileHeaders //ZIP64 END OF CENTRAL DIRECTORY

                zip_64_end_of_central_directory_signature = ZIP64_END_OF_CENTRAL_DIRECTORY_SIGNATURE,
                size_of_zip64_end_of_central_directory_record = (little_endian)(ulong)44,
                version_made_by = VERSION_MADE_BY,
                version_needed_to_extract = VERSION_NEEDED_TO_EXTRACT,
                number_of_this_disk_64 = (little_endian)(uint)0,
                number_of_the_disk_with_the_start_of_the_central_directory = (little_endian)(uint)0,
                total_number_of_entries_in_the_central_directory_on_this_disk = (little_endian)(ulong)offsets.Count,
                total_number_of_entries_in_the_central_director = (little_endian)(ulong)offsets.Count,
                size_of_the_central_directory = (little_endian)(ulong)central_directory.Sum(x => x.Length),
                offset_of_start_of_central_directory_with_respect_to_the_starting_disk_number = (little_endian)(ulong)main_entries.Length;

            ConcatenatedStream

                zip64_end_of_central_directory =

                new[]
                {
                    zip_64_end_of_central_directory_signature,
                    size_of_zip64_end_of_central_directory_record,
                    version_made_by,
                    version_needed_to_extract,
                    number_of_this_disk_64,
                    number_of_the_disk_with_the_start_of_the_central_directory,
                    total_number_of_entries_in_the_central_director,
                    total_number_of_entries_in_the_central_directory_on_this_disk,
                    size_of_zip64_end_of_central_directory_record,
                    offset_of_start_of_central_directory_with_respect_to_the_starting_disk_number
                };


            FileHeaders

                zip64_end_of_central_dir_locator_signature = ZIP64_END_OF_CENTRAL_DIRECTORY_LOCATOR_SIGNATURE,
                number_of_the_disk_with_the_start_of_the_zip64_end_of_central_directory = (little_endian)(uint)0,
                relative_offset_of_the_zip64_end_of_central_directory_record
                    = (little_endian)(ulong)((ulong)main_entries.Length + (ulong)(central_directory.Sum(x => x.Length))),
                total_number_of_disks = (little_endian)(uint)1;


            ConcatenatedStream

                zip64_end_of_central_directory_locator =

                new[]
                {
                    zip64_end_of_central_dir_locator_signature,
                    number_of_the_disk_with_the_start_of_the_zip64_end_of_central_directory,
                    relative_offset_of_the_zip64_end_of_central_directory_record,
                    total_number_of_disks
                };


            FileHeaders

                central_directory_length = (little_endian)(int)central_directory.Sum(x => x.Length),
                number_of_this_disk = (little_endian)(ushort)0,
                disk_where_central_directory_starts = (little_endian)(ushort)0,
                number_of_central_directory_records_on_this_disk = (little_endian)(ushort)central_directory.Count,
                total_number_of_records = (little_endian)(ushort)central_directory.Count,
                offset_start_of_central_directory = (little_endian)(int)main_entries.Length,
                comment_length = (little_endian)(ushort)0;


            ConcatenatedStream end_of_directory =

                new[]
                {
                    END_OF_CENTRAL_DIRECTORY_SIGNATURE,
                    number_of_this_disk,
                    disk_where_central_directory_starts,
                    number_of_central_directory_records_on_this_disk,
                    total_number_of_records,
                    central_directory_length,
                    offset_start_of_central_directory,
                    comment_length,
                };

            stream = main_entries + central_directory + zip64_end_of_central_directory + zip64_end_of_central_directory_locator + end_of_directory;
        }

        public override bool CanRead
        {
            get { return stream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return stream.CanSeek; }
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

        public override int Read(byte[] buffer, int offset, int count)
        {
            return stream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            stream.Write(buffer, offset, count);
        }
    }

    public class ZipEntry
    {
        static readonly Crc32 crc32_hash = new Crc32();

        internal static readonly little_endian
            SIGNATURE = 0x04034b50,
            VERSION_NEEDED = ZipFile.VERSION_NEEDED_TO_EXTRACT,
            GENERAL = (ushort)0,
            COMPRESSION_METHOD = (ushort)0,
            EXTRA_FIELD_LENGTH = (ushort)20;

        static readonly long BASE_LENGTH;

        readonly SeekableStream data;
        readonly string file_name;

        internal readonly byte[] file_name_bytes;

        internal readonly little_endian
            crc32 = 0,
            last_mod_time = (ushort)0,
            last_mod_date = (ushort)0,
            compressed_size = (uint)0xffffffff,
            uncompressed_size = (uint)0xffffffff,
            file_name_length,

            zip64_id = (ushort)1,
            zip64_len = (ushort)16,
            zip64_compressed,
            zip64_uncompressed;

        static ZipEntry()
        {
            BASE_LENGTH =
                SIGNATURE.Length +
                VERSION_NEEDED.Length +
                GENERAL.Length +
                COMPRESSION_METHOD.Length +
                EXTRA_FIELD_LENGTH.Length;
        }

        public ZipEntry(string file_name, Stream data)
        {
            this.file_name = file_name;
            this.file_name_bytes = Encoding.UTF8.GetBytes(file_name);
            this.file_name_length = (ushort)file_name_bytes.Length;
            this.data = data;

            var hash = crc32_hash.ComputeHash(data);

            this.crc32 =

                (uint)(hash[0] << 24) +
                (uint)(hash[1] << 16) +
                (uint)(hash[2] << 08) +
                (uint)(hash[3] << 00);

            data.Seek(0, SeekOrigin.Begin);

            //if (data.Length < 0xffffffff)
            //    compressed_size = uncompressed_size = (int)data.Length;


            zip64_compressed = (ulong)data.Length;
            zip64_uncompressed = (ulong)data.Length;
        }

        public string FileName
        {
            get
            {
                return file_name;
            }
        }

        public long Length
        {
            get
            {
                return

                    BASE_LENGTH +
                    compressed_size.Length +
                    uncompressed_size.Length +
                    last_mod_time.Length +
                    last_mod_date.Length +
                    crc32.Length +
                    file_name_length.Length +
                    file_name_bytes.Length +

                        zip64_id.Length +
                        zip64_len.Length +
                        zip64_compressed.Length +
                        zip64_uncompressed.Length +

                    data.Length;
            }
        }

        public static implicit operator Stream(ZipEntry entry)
        {
            FileHeaders

                signature = SIGNATURE,
                version_needed = VERSION_NEEDED,
                general = GENERAL,
                compression_method = COMPRESSION_METHOD,
                last_mod_time = entry.last_mod_time,
                last_mod_date = entry.last_mod_date,
                crc32 = entry.crc32,
                comressed_sz = entry.compressed_size,
                uncompressed_sz = entry.uncompressed_size,
                file_name_length = entry.file_name_length,
                extra_field_length = EXTRA_FIELD_LENGTH,
                file_name_bytes = new MemoryStream(entry.file_name_bytes),

                    zip64_id = entry.zip64_id,
                    zip64_len = entry.zip64_len,
                    zip64_compressed = entry.zip64_compressed,
                    zip64_uncompressed = entry.zip64_uncompressed,

                data = entry.data;

            ConcatenatedStream

                data_stream = new[]
                {
                    signature,
                    version_needed,
                    general,
                    compression_method,
                    last_mod_time,
                    last_mod_date,
                    crc32,
                    comressed_sz,
                    uncompressed_sz,
                    file_name_length,
                    extra_field_length,
                    file_name_bytes,
                    
                        zip64_id,
                        zip64_len,
                        zip64_compressed,
                        zip64_uncompressed,

                    data,                    
                };

            return data_stream;
        }
    }

    namespace DamienG.Security.Cryptography
    {
        /// <summary>
        /// Implements a 32-bit CRC hash algorithm compatible with Zip etc.
        /// </summary>
        /// <remarks>
        /// Crc32 should only be used for backward compatibility with older file formats
        /// and algorithms. It is not secure enough for new applications.
        /// If you need to call multiple times for the same data either use the HashAlgorithm
        /// interface or remember that the result of one Compute call needs to be ~ (XOR) before
        /// being passed in as the seed for the next Compute call.
        /// </remarks>
        public sealed class Crc32 : HashAlgorithm
        {
            public const UInt32 DefaultPolynomial = 0xedb88320u;
            public const UInt32 DefaultSeed = 0xffffffffu;

            private static UInt32[] defaultTable;

            private readonly UInt32 seed;
            private readonly UInt32[] table;
            private UInt32 hash;

            public Crc32()
                : this(DefaultPolynomial, DefaultSeed)
            {
            }

            public Crc32(UInt32 polynomial, UInt32 seed)
            {
                table = InitializeTable(polynomial);
                this.seed = hash = seed;
            }

            public override void Initialize()
            {
                hash = seed;
            }

            protected override void HashCore(byte[] buffer, int start, int length)
            {
                hash = CalculateHash(table, hash, buffer, start, length);
            }

            protected override byte[] HashFinal()
            {
                var hashBuffer = UInt32ToBigEndianBytes(~hash);
                HashValue = hashBuffer;
                return hashBuffer;
            }

            public override int HashSize { get { return 32; } }

            public static UInt32 Compute(byte[] buffer)
            {
                return Compute(DefaultSeed, buffer);
            }

            public static UInt32 Compute(UInt32 seed, byte[] buffer)
            {
                return Compute(DefaultPolynomial, seed, buffer);
            }

            public static UInt32 Compute(UInt32 polynomial, UInt32 seed, byte[] buffer)
            {
                return ~CalculateHash(InitializeTable(polynomial), seed, buffer, 0, buffer.Length);
            }

            private static UInt32[] InitializeTable(UInt32 polynomial)
            {
                if (polynomial == DefaultPolynomial && defaultTable != null)
                    return defaultTable;

                var createTable = new UInt32[256];
                for (var i = 0; i < 256; i++)
                {
                    var entry = (UInt32)i;
                    for (var j = 0; j < 8; j++)
                        if ((entry & 1) == 1)
                            entry = (entry >> 1) ^ polynomial;
                        else
                            entry = entry >> 1;
                    createTable[i] = entry;
                }

                if (polynomial == DefaultPolynomial)
                    defaultTable = createTable;

                return createTable;
            }

            private static UInt32 CalculateHash(UInt32[] table, UInt32 seed, IList<byte> buffer, int start, int size)
            {
                var crc = seed;
                for (var i = start; i < size - start; i++)
                    crc = (crc >> 8) ^ table[buffer[i] ^ crc & 0xff];
                return crc;
            }

            private static byte[] UInt32ToBigEndianBytes(UInt32 uint32)
            {
                var result = BitConverter.GetBytes(uint32);

                if (BitConverter.IsLittleEndian)
                    Array.Reverse(result);

                return result;
            }
        }
    }
}