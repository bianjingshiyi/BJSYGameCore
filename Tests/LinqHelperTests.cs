using System.Collections.Generic;
using NUnit.Framework;
namespace BJSYGameCore.Tests
{
    public class LinqHelperTests
    {
        [Test]
        public void sequenceEqualsTest()
        {
            List<int> list1 = new List<int>() { 1, 2, 3 };
            List<int> list2 = new List<int>() { 1, 2, 3 };
            List<int> list3 = new List<int>() { 1, 2, 3, 4 };
            List<int> list4 = new List<int>() { 1, 2, 4 };
            Assert.True(list1.SequenceEqual(list2, (x, y) => x - y));
            Assert.False(list1.SequenceEqual(list3, (x, y) => x - y));
            Assert.False(list1.SequenceEqual(list4, (x, y) => x - y));
        }
    }
}