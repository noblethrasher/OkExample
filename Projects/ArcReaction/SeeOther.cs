using System.Web;
namespace ArcReaction
{
    public sealed class SeeOther : AppState, IHttpHandler
    {
        readonly string location;

        public SeeOther(string location)
        {
            this.location = location;
        }

        public override AppState Accept(Message path)
        {
            return this;
        }
        
        public override Representation GetRepresentation(HttpContextEx context)
        {
            return Representation.Create(this);
        }

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.AddHeader("Location", location);
            context.Response.StatusCode = 303;
        }

        public static implicit operator Representation(SeeOther app)
        {
            return Representation.Create(app);
        }
    }
}