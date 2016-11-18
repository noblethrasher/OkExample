using System;
using System.Web;

namespace ArcReaction
{
    public sealed class AuthorizationRequired : AppState
    {
        public override Representation GetRepresentation(HttpContextEx context)
        {
            return new AdHocRepresentation(ctx =>
            {
                ctx.Response.Write("Authorization Required.");
                ctx.Response.StatusCode = 401;
            });    
        }
    }
}