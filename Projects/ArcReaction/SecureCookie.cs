using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;

namespace ArcReaction
{
    public static class SecureCookieModule
    {
        public static SecureCookie Decrypt(this HttpCookie cookie)
        {
            if (cookie == null)
                return null;

            return new SecureCookie(cookie).Decrypt();
        }

        public static void Add(this HttpCookieCollection collection, string name)
        {
            collection.Add(new HttpCookie(name));
        }

        public static void Add(this HttpCookieCollection collection, string name, string value)
        {
            collection.Add(new HttpCookie(name, value));
        }
    }
    
    public sealed class SecureCookie
    {
        string value;
        string path;
        readonly string domain;
        readonly string name;

        DateTime expires;
        
        readonly bool http_only;
        readonly bool https_only;

        public SecureCookie(HttpCookie cookie)
        {
            value = cookie.Value;
            expires = cookie.Expires;
            http_only = cookie.HttpOnly;
            name = cookie.Name;
            path = cookie.Path;
            domain = cookie.Domain;
            https_only = cookie.Secure;
        }

        public SecureCookie(string name, string value)
        {
            this.value = value;
            this.name = name;
        }

        public SecureCookie(string name, string value, DateTime expires, string path, bool http_only = true)
        {
            this.value = value;
            this.name = name;
            this.expires = expires;
            this.http_only = http_only;
            this.path = path;
        }

        public SecureCookie Decrypt()
        {
            if (IsEncrypted)
                this.value = Encoding.Unicode.GetString(MachineKey.Unprotect(Convert.FromBase64String(value.Substring(11)), "AUTH"));
            
            return this;
        }

        public SecureCookie Encrypt()
        {
            if (value != null && !IsEncrypted)
                this.value = "encrypted__" + Convert.ToBase64String(MachineKey.Protect(Encoding.Unicode.GetBytes(value), "AUTH"));
            
            return this;
        }

        public string Path
        {
            get
            {
                return path;
            }
            set
            {
                path = value;
            }
        }

        public DateTime Expires
        {
            get
            {
                return expires;
            }
            set
            {
                expires = value;
            }
        }

        public string Value
        {
            get
            {
                return value;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        bool IsEncrypted
        {
            get
            {
                return value != null && value.StartsWith("encrypted__");
            }
        }

        public static implicit operator SecureCookie(HttpCookie cookie)
        {
            return new SecureCookie(cookie);
        }

        public static implicit operator HttpCookie(SecureCookie secure)
        {
            if (!secure.IsEncrypted)
                secure = secure.Encrypt();

            var cookie = new HttpCookie(secure.name, secure.value) { Domain = secure.domain, HttpOnly = secure.http_only, Secure = secure.https_only };

            if (secure.path != null)
                cookie.Path = secure.path;

            if (secure.expires != default(DateTime))
                cookie.Expires = secure.expires;

            return cookie;
        }
    }
}
