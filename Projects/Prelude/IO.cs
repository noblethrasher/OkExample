using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Prelude
{
    public sealed class Files : IEnumerable<FileInfo>
    {
        readonly DirectoryInfo dir;
        readonly string[] patterns;

        public Files(string path)
        {
            #if (FORWARDSLASH_DELIMITED_PATHS || WIN64 || WIN32)

            if (path.Contains("/"))                
                path = System.Web.HttpContext.Current.Server.MapPath(path);
                
            #else
                
                #if (!NOT_FORWARDSLASH_DELIMITED_PATHS)
                    #error Define symbols for either FORWARDSLASH_DELIMITED_PATHS, NOT_FORWARDSLASH_DELIMITED_PATHS, WIN32, or WIN64
                #endif

            #endif

            dir = new DirectoryInfo(path);
        }

        public Files(string path, string pattern) : this(path)
        {
            patterns = new[] { pattern };
        }

        public Files(string path, params string[] patterns)
            : this(path)
        {
            this.patterns = patterns;
        }

        public IEnumerator<FileInfo> GetEnumerator()
        {
            if (patterns == null)
                return dir.GetFiles().Select(f => f).GetEnumerator();
            else
            {
                return

                (from pattern in patterns
                 from file in dir.GetFiles(pattern)
                 select file).GetEnumerator();
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count
        {
            get
            {
                return (patterns == null ? dir.GetFiles() as IEnumerable<FileInfo> : from pattern in patterns from file in dir.GetFiles(pattern) select file).Count();
            }
        }
    }
}