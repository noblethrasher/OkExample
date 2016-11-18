using System;
using System.IO;
using System.Threading;

namespace ArcReaction
{
    public sealed class FileRepresentation : Representation
    {
        static readonly char PATH_SEPARATOR = Path.DirectorySeparatorChar;
        
        readonly Stream stream;
        readonly string name;
        readonly ContentDisposition disposition;

        public enum ContentDisposition
        {
            inline,
            attachment
        }

        public const ContentDisposition Inline = ContentDisposition.inline;
        public const ContentDisposition Attachment = ContentDisposition.attachment;

        public FileRepresentation(string name, Stream strm, ContentDisposition disposition = ContentDisposition.attachment)
        {
            this.name = name;
            this.stream = strm;
            this.disposition = disposition;
        }

        public FileRepresentation(string path, ContentDisposition disposition = ContentDisposition.attachment)
        {
            var index = path.LastIndexOf(PATH_SEPARATOR);

            if (index >= 0)
                name = path.Substring(index + 1);
            else
                name = path;

            stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            this.disposition = disposition;
        }

        public override void ProcessRequest(System.Web.HttpContext context)
        {
            using(stream)
            {
                context.Response.AddHeader("Content-Disposition", disposition + ";filename=" + name);

                stream.CopyTo(context.Response.OutputStream);

                #pragma warning disable

                try
                {
                    var length = stream.Length;
                    context.Response.AddHeader("Content-Length", length.ToString());
                }
                catch (NotSupportedException ex)
                {

                }

                #pragma warning restore
            }
        }
    }
}