using FluentAssertions;

namespace RandomSkunk.EasyLogging.Tests;

public class LogAttributesTests
{
    public class InternalConstructor
    {
        [Fact]
        public void SetsStateAndScopeProperties()
        {
            var state = new object();
            var fakeScope = new FakeScope();

            var attributes = new LogAttributes(state, fakeScope);

            attributes.State.Should().BeSameAs(state);
            attributes.Scope.Should().BeSameAs(fakeScope);
        }
    }

    public class PublicConstructor
    {
        [Fact]
        public void SetsStateAndScopeProperties()
        {
            var state = new object();
            var scope1 = "abc";
            var scope2 = "xyz";

            var attributes = new LogAttributes(state, scope1, scope2);

            attributes.State.Should().BeSameAs(state);
            attributes.Scope.Should().NotBeNull();
            attributes.Scope?.State.Should().BeSameAs(scope1);
            attributes.Scope?.ParentScope.Should().NotBeNull();
            attributes.Scope?.ParentScope?.State.Should().BeSameAs(scope2);
        }
    }

    public class IEnumerableInterface
    {
        [Fact]
        public void EnumeratesThroughStateOfTypeIEnumerableOfKeyValuePairOfStringToObject()
        {
            var state = new Dictionary<string, object>
            {
                { "foo", "abc" },
                { "bar", 123 }
            };

            var attributes = new LogAttributes(state).ToList();

            attributes.Should().HaveCount(2);
            attributes.Should().ContainKey("foo").WhoseValue.Should().Be("abc");
            attributes.Should().ContainKey("bar").WhoseValue.Should().Be(123);
        }

        [Fact]
        public void EnumeratesThroughStateThatOverridesToStringAndIsOfTypeIEnumerableOfKeyValuePairOfStringToObject()
        {
            var state = new DictionaryWithOverriddenToStringMethod("Hello world!")
            {
                { "foo", "abc" },
                { "bar", 123 }
            };

            var attributes = new LogAttributes(state).ToList();

            attributes.Should().HaveCount(3);
            attributes.Should().ContainKey("foo").WhoseValue.Should().Be("abc");
            attributes.Should().ContainKey("bar").WhoseValue.Should().Be(123);
            attributes.Should().ContainKey("State").WhoseValue.Should().Be("Hello world!");
        }

        [Fact]
        public void EnumeratesThroughStateOfTypeNonStringIEnumerable()
        {
            var state = new List<object> { "abc", 123 };

            var attributes = new LogAttributes(state).ToList();

            attributes.Should().HaveCount(2);
            attributes.Should().ContainKey("State[0]").WhoseValue.Should().Be("abc");
            attributes.Should().ContainKey("State[1]").WhoseValue.Should().Be(123);
        }

        [Fact]
        public void EnumeratesThroughStateThatOverridesToStringAndIsOfTypeNonStringIEnumerable()
        {
            var state = new ListWithOverriddenToStringMethod("Hello world!") { "abc", 123 };

            var attributes = new LogAttributes(state).ToList();

            attributes.Should().HaveCount(3);
            attributes.Should().ContainKey("State[0]").WhoseValue.Should().Be("abc");
            attributes.Should().ContainKey("State[1]").WhoseValue.Should().Be(123);
            attributes.Should().ContainKey("State").WhoseValue.Should().Be("Hello world!");
        }

        [Fact]
        public void EnumeratesThroughStateOfOtherType()
        {
            var state = new object();

            var attributes = new LogAttributes(state).ToList();

            attributes.Should().HaveCount(1);
            attributes.Should().ContainKey("State").WhoseValue.Should().BeSameAs(state);
        }

        [Fact]
        public void EnumeratesThroughScopeOfTypeIEnumerableOfKeyValuePairOfStringToObject()
        {
            var innerScope = new Dictionary<string, object>
            {
                { "foo", "abc" },
                { "bar", 123 }
            };

            var outerScope = new Dictionary<string, object>
            {
                { "baz", "xyz" },
                { "qux", 789 }
            };

            var attributes = new LogAttributes(null, innerScope, outerScope).ToList();

            attributes.Should().HaveCount(4);
            attributes.Should().ContainKey("foo").WhoseValue.Should().Be("abc");
            attributes.Should().ContainKey("bar").WhoseValue.Should().Be(123);
            attributes.Should().ContainKey("baz").WhoseValue.Should().Be("xyz");
            attributes.Should().ContainKey("qux").WhoseValue.Should().Be(789);
        }

        [Fact]
        public void EnumeratesThroughScopeThatOverridesToStringAndIsOfTypeIEnumerableOfKeyValuePairOfStringToObject()
        {
            var innerScope = new DictionaryWithOverriddenToStringMethod("Hello world!")
            {
                { "foo", "abc" },
                { "bar", 123 }
            };

            var outerScope = new DictionaryWithOverriddenToStringMethod("Good-bye world!")
            {
                { "baz", "xyz" },
                { "qux", 789 }
            };

            var attributes = new LogAttributes(null, innerScope, outerScope).ToList();

            attributes.Should().HaveCount(6);
            attributes.Should().ContainKey("foo").WhoseValue.Should().Be("abc");
            attributes.Should().ContainKey("bar").WhoseValue.Should().Be(123);
            attributes.Should().ContainKey("baz").WhoseValue.Should().Be("xyz");
            attributes.Should().ContainKey("qux").WhoseValue.Should().Be(789);
            attributes.Should().ContainKey("Scope").WhoseValue.Should().Be("Hello world!");
            attributes.Should().ContainKey("Scope.ParentScope").WhoseValue.Should().Be("Good-bye world!");
        }

        [Fact]
        public void EnumeratesThroughScopeOfTypeNonStringIEnumerable()
        {
            var innerScope = new List<object> { "abc", 123 };
            var outerScope = new List<object> { "xyz", 789 };

            var attributes = new LogAttributes(null, innerScope, outerScope).ToList();

            attributes.Should().HaveCount(4);
            attributes.Should().ContainKey("Scope[0]").WhoseValue.Should().Be("abc");
            attributes.Should().ContainKey("Scope[1]").WhoseValue.Should().Be(123);
            attributes.Should().ContainKey("Scope.ParentScope[0]").WhoseValue.Should().Be("xyz");
            attributes.Should().ContainKey("Scope.ParentScope[1]").WhoseValue.Should().Be(789);
        }

        [Fact]
        public void EnumeratesThroughScopeThatOverridesToStringAndIsOfTypeNonStringIEnumerable()
        {
            var innerScope = new ListWithOverriddenToStringMethod("Hello world!") { "abc", 123 };
            var outerScope = new ListWithOverriddenToStringMethod("Good-bye world!") { "xyz", 789 };

            var attributes = new LogAttributes(null, innerScope, outerScope).ToList();

            attributes.Should().HaveCount(6);
            attributes.Should().ContainKey("Scope[0]").WhoseValue.Should().Be("abc");
            attributes.Should().ContainKey("Scope[1]").WhoseValue.Should().Be(123);
            attributes.Should().ContainKey("Scope.ParentScope[0]").WhoseValue.Should().Be("xyz");
            attributes.Should().ContainKey("Scope.ParentScope[1]").WhoseValue.Should().Be(789);
            attributes.Should().ContainKey("Scope").WhoseValue.Should().Be("Hello world!");
            attributes.Should().ContainKey("Scope.ParentScope").WhoseValue.Should().Be("Good-bye world!");
        }

        [Fact]
        public void EnumeratesThroughScopeOfOtherType()
        {
            var innerScope = new object();
            var outerScope = new object();

            var attributes = new LogAttributes(null, innerScope, outerScope).ToList();

            attributes.Should().HaveCount(2);
            attributes.Should().ContainKey("Scope").WhoseValue.Should().BeSameAs(innerScope);
            attributes.Should().ContainKey("Scope.ParentScope").WhoseValue.Should().BeSameAs(outerScope);
        }

        [Fact]
        public void EnumeratesThroughDefaultLogAttributes()
        {
            var attributes = default(LogAttributes).ToList();

            attributes.Should().NotBeNull();
            attributes.Should().BeEmpty();
        }
    }

    public class ToStringMethod
    {
        [Fact]
        public void WorksGivenStateAndNestedScope()
        {
            var state = new Dictionary<string, object>
            {
                { "foo", "abc" }
            };

            var innerScope = new Dictionary<string, object>
            {
                { "bar", 123 }
            };

            var outerScope = new Dictionary<string, object>
            {
                { "baz", true }
            };

            var attributes = new LogAttributes(state, innerScope, outerScope);

            attributes.ToString().Should().Be("""
                {
                 State = { [foo] = abc },
                 Scope = { [bar] = 123 },
                 ParentScope = { [baz] = True } 
                }
                """);
        }

        [Fact]
        public void WorksGivenStateAndNestedScopeAllWithToStringOverride()
        {
            var state = new DictionaryWithOverriddenToStringMethod("Hello world!")
            {
                { "foo", "abc" }
            };

            var innerScope = new DictionaryWithOverriddenToStringMethod("Good-bye world!")
            {
                { "bar", 123 }
            };

            var outerScope = new DictionaryWithOverriddenToStringMethod("Greetings world!")
            {
                { "baz", true }
            };

            var attributes = new LogAttributes(state, innerScope, outerScope);

            attributes.ToString().Should().Be("""
                {
                 State = Hello world! { [foo] = abc },
                 Scope = Good-bye world! { [bar] = 123 },
                 ParentScope = Greetings world! { [baz] = True } 
                }
                """);
        }

        [Fact]
        public void WorksGivenStateOfTypeIEnumerableOfKeyValuePairOfStringToObject()
        {
            var state = new Dictionary<string, object>
            {
                { "foo", "abc" },
                { "bar", 123 }
            };

            var attributes = new LogAttributes(state);

            attributes.ToString().Should().Be("""
                {
                 State = { [foo] = abc, [bar] = 123 } 
                }
                """);
        }

        [Fact]
        public void WorksGivenStateThatOverridesToStringAndIsOfTypeIEnumerableOfKeyValuePairOfStringToObject()
        {
            var state = new DictionaryWithOverriddenToStringMethod("Hello world!")
            {
                { "foo", "abc" },
                { "bar", 123 }
            };

            var attributes = new LogAttributes(state);

            attributes.ToString().Should().Be("""
                {
                 State = Hello world! { [foo] = abc, [bar] = 123 } 
                }
                """);
        }

        [Fact]
        public void WorksGivenStateOfTypeNonStringIEnumerable()
        {
            var state = new List<object> { "abc", 123 };

            var attributes = new LogAttributes(state);

            attributes.ToString().Should().Be("""
                {
                 State = [ abc, 123 ] 
                }
                """);
        }

        [Fact]
        public void WorksGivenStateThatOverridesToStringAndIsOfTypeNonStringIEnumerable()
        {
            var state = new ListWithOverriddenToStringMethod("Hello world!") { "abc", 123 };

            var attributes = new LogAttributes(state);

            attributes.ToString().Should().Be("""
                {
                 State = Hello world! [ abc, 123 ] 
                }
                """);
        }

        [Fact]
        public void WorksGivenStateOfOtherType()
        {
            var state = 123.45M;

            var attributes = new LogAttributes(state);

            attributes.ToString().Should().Be("""
                {
                 State = 123.45 
                }
                """);
        }

        [Fact]
        public void WorksGivenScopeOfTypeIEnumerableOfKeyValuePairOfStringToObject()
        {
            var scope = new Dictionary<string, object>
            {
                { "foo", "abc" },
                { "bar", 123 }
            };

            var attributes = new LogAttributes(null, scope);

            attributes.ToString().Should().Be("""
                {
                 Scope = { [foo] = abc, [bar] = 123 } 
                }
                """);
        }

        [Fact]
        public void WorksGivenScopeThatOverridesToStringAndIsOfTypeIEnumerableOfKeyValuePairOfStringToObject()
        {
            var scope = new DictionaryWithOverriddenToStringMethod("Hello world!")
            {
                { "foo", "abc" },
                { "bar", 123 }
            };

            var attributes = new LogAttributes(null, scope);

            attributes.ToString().Should().Be("""
                {
                 Scope = Hello world! { [foo] = abc, [bar] = 123 } 
                }
                """);
        }

        [Fact]
        public void WorksGivenScopeOfTypeNonStringIEnumerable()
        {
            var scope = new List<object> { "abc", 123 };

            var attributes = new LogAttributes(null, scope);

            attributes.ToString().Should().Be("""
                {
                 Scope = [ abc, 123 ] 
                }
                """);
        }

        [Fact]
        public void WorksGivenScopeThatOverridesToStringAndIsOfTypeNonStringIEnumerable()
        {
            var scope = new ListWithOverriddenToStringMethod("Hello world!") { "abc", 123 };

            var attributes = new LogAttributes(null, scope);

            attributes.ToString().Should().Be("""
                {
                 Scope = Hello world! [ abc, 123 ] 
                }
                """);
        }

        [Fact]
        public void WorksGivenScopeOfOtherType()
        {
            var innerScope = 123.45M;

            var attributes = new LogAttributes(null, innerScope);

            attributes.ToString().Should().Be("""
                {
                 Scope = 123.45 
                }
                """);
        }

        [Fact]
        public void WorksGivenNeitherStateNorScope()
        {
            var attributes = new LogAttributes(null, (ILoggerScope?)null);

            var str = attributes.ToString();

            str.Should().Be("{ }");
        }

        [Fact]
        public void WorksGivenDefaultLogAttributes()
        {
            var attributes = default(LogAttributes);

            var str = attributes.ToString();

            str.Should().Be("{ }");
        }
    }

    private class FakeScope : ILoggerScope
    {
        public object State => throw new NotImplementedException();

        public ILoggerScope? ParentScope => throw new NotImplementedException();
    }
}
