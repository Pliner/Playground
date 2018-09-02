using System.Threading;

namespace Collections.Concurrent.Utils
{
    internal class AtomicMarkableReference<TReference, TMark>
    {
        private volatile ReferenceWithMark markedReference;

        public AtomicMarkableReference(TReference reference, TMark mark)
        {
            markedReference = new ReferenceWithMark(reference, mark);
        }

        public (TReference Reference, TMark Mark) MarkedReference
        {
            get
            {
                var localState = markedReference;
                return (localState.Reference, localState.Mark);
            }
            set
            {
                var (newReference, newMark) = value;
                markedReference = new ReferenceWithMark(newReference, newMark);
            }
        }

        public bool CompareAndSet((TReference Reference, TMark Mark) source, (TReference Reference, TMark Mark) destination)
        {
            var localState = markedReference;

            return ReferenceEquals(localState.Reference, source.Reference)
                   && localState.Mark.Equals(source.Mark)
                   && Interlocked.CompareExchange(ref markedReference, new ReferenceWithMark(destination.Reference, destination.Mark), localState) == localState;
        }

        private class ReferenceWithMark
        {
            public readonly TMark Mark;
            public readonly TReference Reference;

            public ReferenceWithMark(TReference reference, TMark mark)
            {
                Reference = reference;
                Mark = mark;
            }
        }
    }
}