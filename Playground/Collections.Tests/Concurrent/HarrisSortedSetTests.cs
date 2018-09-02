using System.Collections.Concurrent;
using System.Linq;
using Collections.Concurrent;
using Xunit;

namespace Collections.Tests.Concurrent
{
    public class HarrisSortedSetTests
    {
        [Theory]
        [InlineData(1)]
        [InlineData(64)]
        [InlineData(128)]
        public void SingleThread_Increase(int count)
        {
            var elements = Enumerable.Range(0, count).ToArray();
            
            var set = new HarrisSortedSet<int>();
            foreach (var element in elements) 
                Assert.True(set.Add(element));
            
            Assert.True(set.Count == count);

            Assert.True(set.SequenceEqual(elements));
            foreach (var element in elements)
                Assert.True(set.Contains(element));
            
            foreach (var element in elements)
                Assert.True(set.Delete(element));

            foreach (var element in elements)
                Assert.False(set.Contains(element));
            
            Assert.True(set.Count == 0);
        }
    }
}