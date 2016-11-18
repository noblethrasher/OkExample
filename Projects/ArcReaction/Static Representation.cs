using System;
using System.Web;
using System.Web.UI;

namespace ArcReaction
{
    public abstract class StaticRepresentation<T> : ModelView<T>
    {
        protected string BaseDirectory
        {
            get
            {
                return this.AppRelativeVirtualPath;
            }
        }
    }
}