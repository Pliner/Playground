using System.Threading;

namespace Collections.Concurrent.Utils
{
    internal class AtomicInt
    {
        private volatile int value;

        public AtomicInt(int value = 0)
        {
            this.value = value;
        }

        public int Increment()
        {
            return Interlocked.Increment(ref value);
        }
        
        public int Decrement()
        {
            return Interlocked.Decrement(ref value);
        }

        public int Value => value;
        
        public static implicit operator int(AtomicInt atomicInt) => atomicInt.value;
    }
}