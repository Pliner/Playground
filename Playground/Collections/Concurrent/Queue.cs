using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Collections.Concurrent.Utils;

namespace Collections.Concurrent
{
    public class Queue<TElement> : IEnumerable<TElement>
    {
        private readonly AtomicInt count;
        private volatile Node head;
        private volatile Node tail;

        public Queue()
        {
            count = new AtomicInt();
            tail = new Node(default, null);
            head = tail;
        }

        public int Count => count;

        public IEnumerator<TElement> GetEnumerator()
        {
            for (var current = head.Next; current != null; current = current.Next)
                yield return current.Element;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Enqueue(TElement element)
        {
            var newTail = new Node(element, null);

            while (true)
            {
                var localTail = tail;
                var localTailNext = localTail.Next;
                if (localTailNext != null)
                {
                    CompareAndSetTail(localTail, localTailNext);
                    continue;
                }

                if (localTail.CompareAndSetNext(null, newTail))
                {
                    CompareAndSetTail(localTail, newTail);
                    count.Increment();
                    return;
                }
            }
        }

        public bool TryHead(out TElement element)
        {
            var localHead = head;
            var localHeadNext = localHead.Next;
            if (localHeadNext == null)
            {
                element = default;
                return false;
            }

            element = localHeadNext.Element;
            return true;
        }

        public bool TryTail(out TElement element)
        {
            var localHead = head;
            var localTail = tail;
            if (localHead == localTail)
            {
                element = default;
                return false;
            }

            element = localTail.Element;
            return true;
        }

        public bool TryDequeue(out TElement element)
        {
            while (true)
            {
                var localHead = head;
                var localHeadNext = localHead.Next;

                if (localHeadNext == null)
                {
                    element = default;
                    return false;
                }

                var localTail = tail;
                if (localHead == localTail)
                {
                    CompareAndSetTail(localTail, localHeadNext);
                    continue;
                }

                if (CompareAndSetHead(localHead, localHeadNext))
                {
                    count.Decrement();

                    element = localHeadNext.Element;
                    return true;
                }
            }
        }

        private bool CompareAndSetTail(Node source, Node destination)
        {
            return Interlocked.CompareExchange(ref tail, destination, source) == source;
        }

        private bool CompareAndSetHead(Node source, Node destination)
        {
            return Interlocked.CompareExchange(ref head, destination, source) == source;
        }

        private class Node
        {
            private volatile Node next;

            public Node(TElement element, Node next)
            {
                Element = element;
                this.next = next;
            }

            public TElement Element { get; }

            public Node Next => next;

            public bool CompareAndSetNext(Node source, Node destination)
            {
                var local = next;
                return local == source && Interlocked.CompareExchange(ref next, destination, local) == local;
            }
        }
    }
}