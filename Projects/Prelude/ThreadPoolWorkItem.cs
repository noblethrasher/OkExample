using System.Threading;

namespace Prelude
{
    public struct QueuedWorkItem
    {
        readonly WaitCallback cb;

        public QueuedWorkItem(WaitCallback cb)
        {
            ThreadPool.QueueUserWorkItem(this.cb = cb);
        }
    }
}