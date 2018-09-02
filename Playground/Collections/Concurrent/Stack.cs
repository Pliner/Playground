using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Collections.Concurrent.Utils;

namespace Collections.Concurrent
{
    public class Stack<TElement> : IEnumerable<TElement>
    {
        private volatile Node head;
        private readonly AtomicInt count;

        public Stack()
        {
            count = new AtomicInt();
        }

        public void Push(TElement element)
        {
            while (true)
            {
                var currentHead = head;
                var newHead = new Node(element, currentHead);
                if (Interlocked.CompareExchange(ref head, newHead, currentHead) == currentHead)
                {
                    count.Increment();
                    return;
                }
            }
        }

        public bool TryPop(out TElement element)
        {
            while (true)
            {
                var currentHead = head;
                if (currentHead == null)
                {
                    element = default;
                    return false;
                }

                var newHead = currentHead.Next;
                if (Interlocked.CompareExchange(ref head, newHead, currentHead) == currentHead)
                {
                    count.Decrement();
                    element = currentHead.Element;
                    return true;
                }
            }
        }

        public bool TryPeek(out TElement element)
        {
            var currentHead = head;
            if (currentHead == null)
            {
                element = default;
                return false;
            }
            element = currentHead.Element;
            return true;
        }

        public int Count => count;
        
        public IEnumerator<TElement> GetEnumerator()
        {
            for (var current = head; current != null; current = current.Next)
                yield return current.Element;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class Node
        {
            public Node(TElement element, Node next)
            {
                Element = element;
                Next = next;
            }

            public TElement Element { get; }
            public Node Next { get; }
        }
    }
}