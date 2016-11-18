using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prelude
{
    public struct Default<T>
    {
        public T Value
        {
            get
            {
                return default(T);
            }
        }
    }
}
