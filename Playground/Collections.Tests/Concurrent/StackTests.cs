using System.Collections.Concurrent;
using System.Linq;
using Collections.Concurrent;
using Xunit;

namespace Collections.Tests.Concurrent
{
    public class StackTests
    {
        [Theory]
        [InlineData(2)]
        [InlineData(64)]
        [InlineData(128)]
        public void SingleThread(int count)
        {
            var elements = Enumerable.Range(0, count).ToList();
            var stack = new Stack<int>();
            foreach (var element in elements)
            {
                stack.Push(element);
                Assert.True(stack.TryPeek(out var peek));
                Assert.Equal(element, peek);
            }
            
            Assert.True(stack.Count == count);

            while (stack.TryPop(out _))
            {
            }
                        
            Assert.True(stack.Count == 0);
        }
    }
}