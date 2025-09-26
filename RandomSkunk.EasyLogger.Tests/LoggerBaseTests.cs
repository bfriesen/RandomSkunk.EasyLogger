using FluentAssertions;
using Microsoft.Extensions.Logging;

namespace RandomSkunk.Logging.Tests;

public class LoggerBaseTests
{
    public class CategoryProperty
    {
        public class GivenNoCategoryInConstructor
        {
            [Fact]
            public void ReturnsTheConcreteLoggerTypeName()
            {
                LoggerBase logger = new ConcreteLoggerBase();
                logger.Category.Should().Be(typeof(ConcreteLoggerBase).ToString());
            }
        }

        public class GivenCategoryInConstructor
        {
            [Fact]
            public void ReturnsTheValuePassedToTheConstructor()
            {
                const string category = "MyCategory";
                LoggerBase logger = new ConcreteLoggerBase(category);
                logger.Category.Should().Be(category);
            }
        }
    }

    public class MinimumLogLevelProperty
    {
        [Fact]
        public void DefaultValueIsInformation()
        {
            LoggerBase logger = new ConcreteLoggerBase();

            logger.MinimumLogLevel.Should().Be(LogLevel.Information);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(7)]
        public void GivenInvalidValueThrowsException(int logLevel)
        {
            var act = () => new ConcreteLoggerBase { MinimumLogLevel = (LogLevel)logLevel };

            act.Should().Throw<ArgumentOutOfRangeException>();
        }
    }

    public class IncludeScopesProperty
    {
        [Fact]
        public void DefaultValueIsTrue()
        {
            LoggerBase logger = new ConcreteLoggerBase();

            logger.IncludeScopes.Should().BeTrue();
        }

        [Fact]
        public void ControlsScopeInclusion()
        {
            LoggerBase logger = new ConcreteLoggerBase
            {
                IncludeScopes = false
            };

            using (logger.BeginScope("Scope1"))
            {
                logger.CurrentScope.Should().BeEmpty();
            }

            logger.IncludeScopes = true;

            using (logger.BeginScope("Scope2"))
            {
                logger.CurrentScope.Should().ContainSingle().Which.Should().Be("Scope2");
            }
        }
    }

    public class CurrentScopeProperty
    {
        [Fact]
        public void DefaultValueIsEmpty()
        {
            LoggerBase logger = new ConcreteLoggerBase();

            logger.CurrentScope.Should().BeEmpty();
        }
    }

    public class IsEnabledMethod
    {
        [Theory]
        [InlineData(LogLevel.Trace, (LogLevel)(-1), false)]
        [InlineData(LogLevel.Trace, LogLevel.Trace, true)]
        [InlineData(LogLevel.Trace, LogLevel.Debug, true)]
        [InlineData(LogLevel.Trace, LogLevel.Information, true)]
        [InlineData(LogLevel.Trace, LogLevel.Warning, true)]
        [InlineData(LogLevel.Trace, LogLevel.Error, true)]
        [InlineData(LogLevel.Trace, LogLevel.Critical, true)]
        [InlineData(LogLevel.Trace, LogLevel.None, false)]
        [InlineData(LogLevel.Trace, (LogLevel)7, false)]
        [InlineData(LogLevel.Debug, (LogLevel)(-1), false)]
        [InlineData(LogLevel.Debug, LogLevel.Trace, false)]
        [InlineData(LogLevel.Debug, LogLevel.Debug, true)]
        [InlineData(LogLevel.Debug, LogLevel.Information, true)]
        [InlineData(LogLevel.Debug, LogLevel.Warning, true)]
        [InlineData(LogLevel.Debug, LogLevel.Error, true)]
        [InlineData(LogLevel.Debug, LogLevel.Critical, true)]
        [InlineData(LogLevel.Debug, LogLevel.None, false)]
        [InlineData(LogLevel.Debug, (LogLevel)7, false)]
        [InlineData(LogLevel.Information, (LogLevel)(-1), false)]
        [InlineData(LogLevel.Information, LogLevel.Trace, false)]
        [InlineData(LogLevel.Information, LogLevel.Debug, false)]
        [InlineData(LogLevel.Information, LogLevel.Information, true)]
        [InlineData(LogLevel.Information, LogLevel.Warning, true)]
        [InlineData(LogLevel.Information, LogLevel.Error, true)]
        [InlineData(LogLevel.Information, LogLevel.Critical, true)]
        [InlineData(LogLevel.Information, LogLevel.None, false)]
        [InlineData(LogLevel.Information, (LogLevel)7, false)]
        [InlineData(LogLevel.Warning, (LogLevel)(-1), false)]
        [InlineData(LogLevel.Warning, LogLevel.Trace, false)]
        [InlineData(LogLevel.Warning, LogLevel.Debug, false)]
        [InlineData(LogLevel.Warning, LogLevel.Information, false)]
        [InlineData(LogLevel.Warning, LogLevel.Warning, true)]
        [InlineData(LogLevel.Warning, LogLevel.Error, true)]
        [InlineData(LogLevel.Warning, LogLevel.Critical, true)]
        [InlineData(LogLevel.Warning, LogLevel.None, false)]
        [InlineData(LogLevel.Warning, (LogLevel)7, false)]
        [InlineData(LogLevel.Error, (LogLevel)(-1), false)]
        [InlineData(LogLevel.Error, LogLevel.Trace, false)]
        [InlineData(LogLevel.Error, LogLevel.Debug, false)]
        [InlineData(LogLevel.Error, LogLevel.Information, false)]
        [InlineData(LogLevel.Error, LogLevel.Warning, false)]
        [InlineData(LogLevel.Error, LogLevel.Error, true)]
        [InlineData(LogLevel.Error, LogLevel.Critical, true)]
        [InlineData(LogLevel.Error, LogLevel.None, false)]
        [InlineData(LogLevel.Error, (LogLevel)7, false)]
        [InlineData(LogLevel.Critical, (LogLevel)(-1), false)]
        [InlineData(LogLevel.Critical, LogLevel.Trace, false)]
        [InlineData(LogLevel.Critical, LogLevel.Debug, false)]
        [InlineData(LogLevel.Critical, LogLevel.Information, false)]
        [InlineData(LogLevel.Critical, LogLevel.Warning, false)]
        [InlineData(LogLevel.Critical, LogLevel.Error, false)]
        [InlineData(LogLevel.Critical, LogLevel.Critical, true)]
        [InlineData(LogLevel.Critical, LogLevel.None, false)]
        [InlineData(LogLevel.Critical, (LogLevel)7, false)]
        [InlineData(LogLevel.None, (LogLevel)(-1), false)]
        [InlineData(LogLevel.None, LogLevel.Trace, false)]
        [InlineData(LogLevel.None, LogLevel.Debug, false)]
        [InlineData(LogLevel.None, LogLevel.Information, false)]
        [InlineData(LogLevel.None, LogLevel.Warning, false)]
        [InlineData(LogLevel.None, LogLevel.Error, false)]
        [InlineData(LogLevel.None, LogLevel.Critical, false)]
        [InlineData(LogLevel.None, LogLevel.None, false)]
        [InlineData(LogLevel.None, (LogLevel)7, false)]
        public void ReturnsTheCorrectValue(LogLevel loggerLogLevel, LogLevel logEventLogLevel, bool expectedIsEnabled)
        {
            LoggerBase logger = new ConcreteLoggerBase { MinimumLogLevel = loggerLogLevel };

            var actualIsEnabled = logger.IsEnabled(logEventLogLevel);

            actualIsEnabled.Should().Be(expectedIsEnabled);
        }
    }

    public class BeginScopeMethod
    {
        [Fact]
        public void GivenNullStateObjectThrowsException()
        {
            LoggerBase logger = new ConcreteLoggerBase();

            logger.Invoking(x => x.BeginScope<object>(null!))
                .Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void GivenIncludeScopesIsFalseReturnsNull()
        {
            LoggerBase logger = new ConcreteLoggerBase { IncludeScopes = false };

            var scope = logger.BeginScope(123);

            scope.Should().BeNull();
            logger.CurrentScope.Should().BeEmpty();
        }

        [Fact]
        public void ReturnsScopeObjectAndSetsItToCurrentScope()
        {
            LoggerBase logger = new ConcreteLoggerBase();

            // Begin the first, outer scope.
            using (var scope1 = logger.BeginScope(123))
            {
                Assert.NotNull(scope1);
                logger.CurrentScope.Should().HaveCount(1);
                logger.CurrentScope.First().Should().Be(123);

                // Begin the second, inner scope.
                using (var scope2 = logger.BeginScope(456))
                {
                    Assert.NotNull(scope2);
                    logger.CurrentScope.Should().HaveCount(2);
                    logger.CurrentScope.First().Should().Be(456);
                    logger.CurrentScope.Skip(1).First().Should().Be(123);
                } // Dispose the inner scope.

                logger.CurrentScope.Should().HaveCount(1);
                logger.CurrentScope.First().Should().Be(123);
            } // Dispose the outer scope.

            logger.CurrentScope.Should().BeEmpty();
        }

        [Fact]
        public void HandlesOutOfOrderDisposalOfScopeObjects()
        {
            LoggerBase logger = new ConcreteLoggerBase();

            // Begin the first, outer scope.
            var scope1 = logger.BeginScope(123)!;

            // Begin the second, inner scope.
            var scope2 = logger.BeginScope(456)!;

            // Since this is the outer scope, disposing it should leave the logger without a scope.
            scope1.Dispose();

            logger.CurrentScope.Should().BeEmpty();

            // This should do nothing since the logger no longer has a scope.
            scope2.Dispose();

            logger.CurrentScope.Should().BeEmpty();
        }
    }
}
