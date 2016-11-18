using System;

namespace ArcReaction
{
    public abstract class ArcException : Exception
    {
        public abstract bool CanHandle { get; }
        public abstract AppState AppState { get; }

        public static implicit operator AppState(ArcException ex)
        {
            return ex.AppState;
        }
    }
}