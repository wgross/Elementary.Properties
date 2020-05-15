using Elementary.Properties.Comparers;
using System;
using Xunit;

namespace Elementary.Properties.Test.Comparers
{
    public class DynamicEqualityCompaterTest
    {
        public class Data
        {
            public int Integer { get; set; }

            public string String { get; set; }

            public Data Reference { set; get; }

            public int[] Collection { get; set; }
        }

        [Fact]
        public void EqualityComparer_accepts_same_instance()
        {
            // ARRANGE

            var data = new Data();
            var comparer = DynamicEqualityComparerFactory.Of<Data>();

            // ACT

            var result = comparer.Equals(data, data);

            // ASSERT

            Assert.True(result);
        }

        [Fact]
        public void EqualityComparer_accepts_null()
        {
            // ARRANGE

            var comparer = DynamicEqualityComparerFactory.Of<Data>();

            // ACT

            var result = comparer.Equals(null, null);

            // ASSERT

            Assert.True(result);
        }

        [Fact]
        public void EqualityComparer_accepts_same_values()
        {
            // ARRANGE

            var left = new Data
            {
                Integer = 1,
                String = "test",
                Reference = new Data(),
                Collection = new[] { 1 }
            };

            var right = new Data
            {
                Integer = 1,
                String = "test",
                Reference = new Data(),
                Collection = new[] { 1 }
            };

            var comparer = DynamicEqualityComparerFactory.Of<Data>();

            // ACT

            var result = comparer.Equals(left, right);

            // ASSERT

            Assert.True(result);
        }

        [Fact]
        public void EqualityComparer_rejects_different_values()
        {
            // ARRANGE

            var left = new Data
            {
                Integer = 1,
                String = "a",
                Reference = new Data(),
                Collection = new[] { 1 }
            };

            var right1 = new Data
            {
                Integer = 2,
                String = "a",
                Reference = new Data(),
                Collection = new[] { 1 }
            };

            var right2 = new Data
            {
                Integer = 1,
                String = "b",
                Reference = new Data(),
                Collection = new[] { 1 }
            };

            var comparer = DynamicEqualityComparerFactory.Of<Data>();

            // ACT

            var result = (
                a: comparer.Equals(left, right1),
                b: comparer.Equals(left, right2)
            );

            // ASSERT

            Assert.Equal((false, false), result);
        }

        [Fact]
        public void EqualityComparer_calculates_hashcode()
        {
            // ARRANGE

            var data = new Data
            {
                Integer = 1,
                String = "test",
                Reference = new Data(),
                Collection = new[] { 1 }
            };

            var comparer = DynamicEqualityComparerFactory.Of<Data>();

            // ACT

            var result = comparer.GetHashCode(data);

            // ASSERT

            Assert.Equal(HashCode.Combine(data.Integer, data.String), result);
        }
    }
}