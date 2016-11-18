using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prelude
{

    /// <summary>
    /// Respsesents that an instance of a class that respresents application state is invalid upon completion of of a constructor. This instances of this class are exceptionss that should only thrown from constructors (ctors).
    /// </summary>
    public sealed class InvalidConstructionException : Exception
    {
        public InvalidConstructionException()
        {

        }

        public InvalidConstructionException(string message) : base(message) { }
    }
}
