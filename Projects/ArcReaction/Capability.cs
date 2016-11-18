using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Security.Principal;

namespace ArcReaction
{
    public abstract class ArcReactionUser : IPrincipal, IIdentity, IEnumerable<Capability>
    {
        readonly int? id;
        readonly bool is_authenticated;
        readonly string name, authentication_type, session;
        readonly List<Capability> capabilities = new List<Capability>();

        protected ArcReactionUser(string name, bool authenticated, string authentication_type, string session, int? id)
        {
            is_authenticated = authenticated;
            this.name = name;

            this.authentication_type = authentication_type;

            this.session = session;

            this.id = id;
        }
        
        protected void AddCapability(Capability cap)            
        {
            if(cap !=null)
            {
                if (cap.User == this)
                    capabilities.Add(cap);
                else
                    throw new InvalidOperationException("Cannot add a capability to user unless the capability references the user");
            }
        }

        public virtual IIdentity Identity
        {
            get
            {
                return this;
            }
        }

        public virtual bool IsInRole(string role)
        {
            throw new NotImplementedException();
        }

        public string AuthenticationType
        {
            get { return authentication_type; }
        }

        public bool IsAuthenticated
        {
            get { return is_authenticated; }
        }

        public string Name
        {
            get { return name; }
        }

        public IEnumerable<Capability> GetCapabilities()
        {
            return from c in capabilities select c;
        }

        public T GetCapability<T>() where T : Capability
        {
            if (capabilities != null)
                foreach (var cap in capabilities)
                    if (cap is T)
                        return cap as T;

            return null;
        }

        public static implicit operator bool(ArcReactionUser user)
        {
            return user != null && user.is_authenticated;
        }

        
        IEnumerator<Capability> IEnumerable<Capability>.GetEnumerator()
        {
            return capabilities.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<Capability>).GetEnumerator();
        }
    }

    public abstract class Capability
    {
        readonly ArcReactionUser user;

        public Capability(ArcReactionUser user)
        {
            if (user == null)
                throw new ArgumentException("User cannot be null");

            this.user = user;
        }

        public ArcReactionUser User => user;

        public static implicit operator ArcReactionUser(Capability cap) => cap.user;

        //public abstract void Log();
    }

    public abstract class Capability<T> : Capability
        where T : Capability
    {
        public Capability(ArcReactionUser user) : base(user) { }

        public static implicit operator Capability<T>(ArcReactionUser user) => user.GetCapability<Capability<T>>();
    }
    
}