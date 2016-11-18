using System.Collections.Generic;
using System.Web;
using System.Linq;

namespace ArcReaction
{
    public sealed class Message
    {
        public string Text { get; internal set; }
        internal readonly HttpContextEx context;
        public readonly string[] paths;

        internal int count = 0;

        public Message(HttpContextEx context)
        {
            this.context = context;
            paths = context.Request.Url.Segments;
        }

        public Message(string text, HttpContextEx context) : this(context)
        {
            Text = text;
        }

        public HttpContextEx Context
        {
            get
            {
                return context;
            }
        }

        public IReadOnlyList<string> Rest
        {
            get
            {
                return paths.Skip(count + 1).ToList();
            }
        }

        public override string ToString()
        {
            return Text;
        }

        public static implicit operator string(Message msg)
        {
            return msg.Text;
        }
    }
    
    
    public abstract class AppState
    {
        public AppState Next(Message path)
        {
            path.count++;

            return Accept(path);
        }
        
        public virtual AppState Accept(Message path)
        {
            return null;
        }

        public abstract Representation GetRepresentation(HttpContextEx context);        
    }
}