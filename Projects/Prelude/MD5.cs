using System;
using System.Security.Cryptography;

namespace Prelude
{
    public struct MD5Digest
    {
        static readonly MD5 md5 = MD5.Create();

        readonly byte[] digest;

        public MD5Digest(byte[] bytes)
        {
            digest = md5.ComputeHash(bytes);
        }

        public override string ToString() => Convert.ToBase64String(digest);

        public static implicit operator string (MD5Digest digest) => digest.ToString();
    }
}