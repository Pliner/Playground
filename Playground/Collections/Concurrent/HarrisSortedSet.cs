using System.Collections;
using System.Collections.Generic;
using Collections.Concurrent.Utils;

namespace Collections.Concurrent
{
    public class HarrisSortedSet<TElement> : IReadOnlyCollection<TElement>
    {
        private readonly Node head;
        private readonly Node tail;
        private readonly AtomicInt count;

        public HarrisSortedSet() : this(Comparer<TElement>.Default)
        {
        }

        public HarrisSortedSet(IComparer<TElement> comparer)
        {
            Comparer = comparer;
            tail = new Node(default, new AtomicMarkableReference<Node, Status>(null, Status.Alive));
            head = new Node(default, new AtomicMarkableReference<Node, Status>(tail, Status.Alive));
            count = new AtomicInt();
        }

        public IComparer<TElement> Comparer { get; }

        public IEnumerator<TElement> GetEnumerator()
        {
            var (current, _) = head.Next.MarkedReference;
            while (current != tail)
            {
                var (successor, currentStatus) = current.Next.MarkedReference;
                if (currentStatus != Status.Dead)
                    yield return current.Element;

                current = successor;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => count;

        public bool Contains(TElement element)
        {
            var (_, right, found) = Search(element);
            var (_, rightStatus) = right.Next.MarkedReference;
            return found && rightStatus == Status.Alive;
        }

        public bool Add(TElement element)
        {
            var inserting = new Node(element, new AtomicMarkableReference<Node, Status>(null, Status.Alive));

            while (true)
            {
                var (left, right, found) = Search(element);
                if (found) return false;

                inserting.Next.MarkedReference = (right, Status.Alive);
                
                var (_, leftStatus) = left.Next.MarkedReference;
                if (leftStatus == Status.Alive && left.Next.CompareAndSet((right, Status.Alive), (inserting, Status.Alive)))
                {
                    count.Increment();
                    return true;
                }
            }
        }

        public bool Delete(TElement element)
        {
            while (true)
            {
                var (_, right, found) = Search(element);
                if (!found) return false;

                var (rightNext, rightStatus) = right.Next.MarkedReference;
                if (rightStatus == Status.Dead) continue;

                if (right.Next.CompareAndSet((rightNext, Status.Alive), (rightNext, Status.Dead)))
                {
                    count.Decrement();
                    return true;
                }
            }
        }

        private (Node, Node, bool) Search(TElement element)
        {
            while (true)
            {
                var predecessor = head;
                var (current, _) = predecessor.Next.MarkedReference;

                while (true)
                {
                    var (successor, currentStatus) = current.Next.MarkedReference;
                    if (currentStatus == Status.Dead && !predecessor.Next.CompareAndSet((current, Status.Alive), (successor, Status.Alive)))
                        break;

                    if (current == tail) return (predecessor, current, false);

                    var compareResult = Comparer.Compare(current.Element, element);
                    if (compareResult < 0)
                    {
                        predecessor = current;
                        current = successor;
                        continue;
                    }

                    return (predecessor, current, compareResult == 0);
                }
            }
        }

        private enum Status
        {
            Alive,
            Dead
        }

        private class Node
        {
            public Node(TElement element, AtomicMarkableReference<Node, Status> next)
            {
                Element = element;
                Next = next;
            }

            public TElement Element { get; }
            public AtomicMarkableReference<Node, Status> Next { get; }
        }
    }
}