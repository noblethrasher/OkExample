using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prelude
{
    public struct ThreadComa
    {
        public ThreadComa(int timeout)
        {
            System.Threading.Thread.Sleep(timeout);
        }
    }
}
