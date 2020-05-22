using Elementary.Properties.Comparers;
using System;
using Xunit;

namespace Elementary.Properties.Test.Comparers
{
    public class DynamicEqualityComparerTest
    {
        public class Data1
        {
            public int Integer1 { get; set; }

            public string String1 { get; set; }

            public Data2 Reference1 { set; get; }

            public int[] Collection1 { get; set; }
        }

        public class Data2
        {
            public int Integer2 { get; set; }

            public string String2 { get; set; }

            public Data3 Reference2 { set; get; }

            public int[] Collection2 { get; set; }
        }

        public class Data3
        {
            public int Integer3 { get; set; }

            public string String3 { get; set; }
        }

        [Fact]
        public void EqualityComparer_accepts_same_instance()
        {
            // ARRANGE

            var data = new Data1();
            var comparer = DynamicEqualityComparerFactory.Of<Data1>();

            // ACT

            var result = comparer.Equals(data, data);

            // ASSERT

            Assert.True(result);
        }

        [Fact]
        public void EqualityComparer_accepts_null()
        {
            // ARRANGE

            var comparer = DynamicEqualityComparerFactory.Of<Data1>();

            // ACT

            var result = comparer.Equals(null, null);

            // ASSERT

            Assert.True(result);
        }

        [Fact]
        public void EqualityComparer_accepts_same_values()
        {
            // ARRANGE

            var left = new Data1
            {
                Integer1 = 1,
                String1 = "test",
                Reference1 = new Data2
                {
                    Integer2 = 2,
                    String2 = "2"
                },
                Collection1 = new[] { 1 }
            };

            var right = new Data1
            {
                Integer1 = 1,
                String1 = "test",
                Reference1 = new Data2
                {
                    Integer2 = 22, // <-- different, but not included
                    String2 = "22"
                },
                Collection1 = new[] { 1 }
            };

            var comparer = DynamicEqualityComparerFactory.Of<Data1>();

            // ACT

            var result = comparer.Equals(left, right);

            // ASSERT

            Assert.True(result);
        }

        [Fact]
        public void EqualityComparer_rejects_different_values()
        {
            // ARRANGE

            var left = new Data1
            {
                Integer1 = 1,
                String1 = "a",
                Reference1 = new Data2(),
                Collection1 = new[] { 1 }
            };

            var right1 = new Data1
            {
                Integer1 = 2,
                String1 = "a",
                Reference1 = new Data2(),
                Collection1 = new[] { 1 }
            };

            var right2 = new Data1
            {
                Integer1 = 1,
                String1 = "b",
                Reference1 = new Data2(),
                Collection1 = new[] { 1 }
            };

            var comparer = DynamicEqualityComparerFactory.Of<Data1>();

            // ACT

            var result = (
                a: comparer.Equals(left, right1),
                b: comparer.Equals(left, right2)
            );

            // ASSERT

            Assert.Equal((false, false), result);
        }

        [Fact]
        public void EqualityComparer_rejects_null_instance()
        {
            // ARRANGE

            var left = new Data1
            {
                Integer1 = 1,
                String1 = "a",
                Reference1 = new Data2(),
                Collection1 = new[] { 1 }
            };

            var comparer = DynamicEqualityComparerFactory.Of<Data1>();

            // ACT

            var result = (
                a: comparer.Equals(left, null),
                b: comparer.Equals(null, left)
            );

            // ASSERT

            Assert.Equal((false, false), result);
        }

        [Fact]
        public void EqualityComparer_accepts_same_values_in_nested_classes()
        {
            // ARRANGE

            var left = new Data1
            {
                Integer1 = 1,
                String1 = "test",
                Reference1 = new Data2
                {
                    String2 = "string2",
                    Reference2 = new Data3()
                    {
                        Integer3 = 3
                    }
                },
                Collection1 = new[] { 1 }
            };

            var right = new Data1
            {
                Integer1 = 1,
                String1 = "test",
                Reference1 = new Data2
                {
                    String2 = "string2",
                    Reference2 = new Data3()
                    {
                        Integer3 = 33 // <-- different but not included
                    }
                },
                Collection1 = new[] { 1 }
            };

            var comparer = DynamicEqualityComparerFactory.Of<Data1>(configure: c => c.IncludeNested(d => d.Reference1));

            // ACT

            var result = comparer.Equals(left, right);

            // ASSERT

            Assert.True(result);
        }

        [Fact]
        public void EqualityComparer_accepts_both_nested_classes_null()
        {
            // ARRANGE

            var left = new Data1
            {
                Integer1 = 1,
                String1 = "test",
                Reference1 = null,
                Collection1 = new[] { 1 }
            };

            var right = new Data1
            {
                Integer1 = 1,
                String1 = "test",
                Reference1 = null,
                Collection1 = new[] { 1 }
            };

            var comparer = DynamicEqualityComparerFactory.Of<Data1>(configure: c => c.IncludeNested(d => d.Reference1));

            // ACT

            var result = comparer.Equals(left, right);

            // ASSERT

            Assert.True(result);
        }

        [Fact]
        public void EqualityComparer_accepts_both_nested_classes_same()
        {
            // ARRANGE

            var data2 = new Data2
            {
                Integer2 = 2,
                String2 = "2",
                Reference2 = new Data3 { Integer3 = 3 }
            };

            var left = new Data1
            {
                Integer1 = 1,
                String1 = "test",
                Reference1 = data2,
                Collection1 = new[] { 1 }
            };

            var right = new Data1
            {
                Integer1 = 1,
                String1 = "test",
                Reference1 = data2,
                Collection1 = new[] { 1 }
            };

            var comparer = DynamicEqualityComparerFactory.Of<Data1>(configure: c => c.IncludeNested(d => d.Reference1));

            // ACT

            var result = comparer.Equals(left, right);

            // ASSERT

            Assert.True(result);
        }

        [Fact]
        public void EqualityComparer_rejects_different_null_reference_to_nested_class()
        {
            // ARRANGE

            var left = new Data1
            {
                Integer1 = 1,
                String1 = "test",
                Reference1 = new Data2(),
                Collection1 = new[] { 1 }
            };

            var right = new Data1
            {
                Integer1 = 1,
                String1 = "test",
                Reference1 = null, // <-- different
                Collection1 = new[] { 1 }
            };

            var comparer = DynamicEqualityComparerFactory.Of<Data1>(configure: c => c.IncludeNested(d => d.Reference1));

            // ACT

            var result = comparer.Equals(left, right);

            // ASSERT

            Assert.False(result);
        }

        [Fact]
        public void EqualityComparer_rejects_different_value_in_nested_class()
        {
            // ARRANGE

            var left = new Data1
            {
                Integer1 = 1,
                String1 = "1",
                Reference1 = new Data2
                {
                    Integer2 = 2,
                    String2 = "2"
                },
                Collection1 = new[] { 1 }
            };

            var right1 = new Data1
            {
                Integer1 = 1,
                String1 = "1",
                Reference1 = new Data2
                {
                    Integer2 = 2,
                    String2 = "2-different" // <-- different
                },
                Collection1 = new[] { 2 }
            };

            var right2 = new Data1
            {
                Integer1 = 1,
                String1 = "1",
                Reference1 = new Data2
                {
                    Integer2 = 22, // <-- different
                    String2 = "2"
                },
                Collection1 = new[] { 2 }
            };

            var comparer = DynamicEqualityComparerFactory.Of<Data1>(configure: c => c.IncludeNested(d => d.Reference1));

            // ACT

            var result = (
                a: comparer.Equals(left, right1),
                b: comparer.Equals(left, right1)
            );

            // ASSERT

            Assert.Equal((false, false), result);
        }

        [Fact]
        public void EqualityComparer_calculates_hashcode()
        {
            // ARRANGE

            var data1 = new Data1
            {
                Integer1 = 1,
                String1 = "test",
                Reference1 = new Data2(),
                Collection1 = new[] { 1 }
            };

            var data2 = new Data1
            {
                Integer1 = 1,
                String1 = "test",
                Reference1 = new Data2 // <-- different but not included
                {
                    Integer2 = 2
                },
                Collection1 = new[] { 1 }
            };

            var data3 = new Data1
            {
                Integer1 = 1,
                String1 = "test",
                Reference1 = null, // <-- different but not included
                Collection1 = new[] { 1 }
            };

            var comparer = DynamicEqualityComparerFactory.Of<Data1>();

            // ACT

            var result = (
                a: comparer.GetHashCode(data1),
                b: comparer.GetHashCode(data2),
                c: comparer.GetHashCode(data3)
            );

            // ASSERT

            var hash = HashCode.Combine(data1.Integer1, data1.String1);

            Assert.Equal((hash, hash, hash), result);
        }

        [Fact]
        public void EqualityComparer_calculates_hashcode_from_null()
        {
            // ARRANGE

            var comparer = DynamicEqualityComparerFactory.Of<Data1>();

            // ACT

            var result = comparer.GetHashCode(null);

            // ASSERT

            Assert.Equal(0, result);
        }

        [Fact]
        public void EqualityComparer_calculates_hashcode_with_included_null_reference()
        {
            // ARRANGE

            var data = new Data1
            {
                Integer1 = 1,
                String1 = "test",
                Reference1 = null,
                Collection1 = new[] { 1 }
            };

            var comparer = DynamicEqualityComparerFactory.Of<Data1>(configure: c => c.IncludeNested(d => d.Reference1));

            // ACT

            var result = comparer.GetHashCode(data);

            // ASSERT

            Assert.Equal(HashCode.Combine(data.Integer1, data.String1), result);
        }

        [Fact]
        public void EqualityComparer_calculates_hashcode_with_included_nested_class()
        {
            // ARRANGE

            var data1 = new Data1
            {
                Integer1 = 1,
                String1 = "test",
                Reference1 = new Data2
                {
                    Integer2 = 2,
                    Reference2 = new Data3()
                },
                Collection1 = new[] { 1 }
            };

            var data2 = new Data1
            {
                Integer1 = 1,
                String1 = "test",
                Reference1 = new Data2
                {
                    Integer2 = 2,
                    Reference2 = null // <-- different, but not included
                },
                Collection1 = new[] { 1 }
            };

            var comparer = DynamicEqualityComparerFactory.Of<Data1>(configure: c => c.IncludeNested(d => d.Reference1));

            // ACT

            var result = (
                a: comparer.GetHashCode(data1),
                b: comparer.GetHashCode(data2)
            );

            // ASSERT

            var hash = HashCode.Combine(data1.Integer1, data1.String1, data1.Reference1.Integer2, data1.Reference1.String2);

            Assert.Equal((hash, hash), result);
        }

        [Fact]
        public void EqualityComparer_calculates_hashcode_with_included_nested_class_different()
        {
            // ARRANGE

            var data1 = new Data1
            {
                Integer1 = 1,
                String1 = "test",
                Reference1 = new Data2
                {
                    Integer2 = 2
                },
                Collection1 = new[] { 1 }
            };

            var data2 = new Data1
            {
                Integer1 = 1,
                String1 = "test",
                Reference1 = new Data2
                {
                    Integer2 = 1
                },
                Collection1 = new[] { 1 }
            };

            var comparer = DynamicEqualityComparerFactory.Of<Data1>(configure: c => c.IncludeNested(d => d.Reference1));

            // ACT

            var result = (
                a: comparer.GetHashCode(data1),
                b: comparer.GetHashCode(data2)
            );

            // ASSERT

            Assert.NotEqual(result.a, result.b);
        }
    }
}