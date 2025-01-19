using System.Threading.Tasks;
using Meziantou.MsTestToXunitAnalyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace Meziantou.MsTestToXunitAnalyzerTests;

public class AssertTests
{
    private static CSharpCodeFixTest<MSTestAnalyzer, AssertCodeFixer, DefaultVerifier> CreateContext()
    {
        var context = new CSharpCodeFixTest<MSTestAnalyzer, AssertCodeFixer, DefaultVerifier>()
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80
                        .AddPackages([new PackageIdentity("MSTest.TestFramework", "3.7.1")])
                        .AddPackages([new PackageIdentity("xunit.v3.assert", "1.0.1")]),
            TestState =
            {
                OutputKind = OutputKind.ConsoleApplication,
            },
        };

        return context;
    }

    [Theory]
    [InlineData("Assert.IsTrue(false)", "Assert.True(false)")]
    [InlineData("Assert.IsFalse(false)", "Assert.False(false)")]
    [InlineData("Assert.IsNull(false)", "Assert.Null(false)")]
    [InlineData("Assert.IsNotNull(false)", "Assert.NotNull(false)")]
    [InlineData("Assert.AreEqual(false, true)", "Assert.Equal(false, true)")]
    [InlineData("Assert.AreNotEqual(false, true)", "Assert.NotEqual(false, true)")]
    [InlineData("Assert.IsInstanceOfType(1, typeof(int))", "Assert.IsType(typeof(int), 1)")]
    [InlineData("Assert.IsInstanceOfType<int>(1)", "Assert.IsType<int>(1)")]
    [InlineData("Assert.IsNotInstanceOfType(1, typeof(int))", "Assert.IsNotType(typeof(int), 1)")]
    [InlineData("Assert.IsNotInstanceOfType<int>(1)", "Assert.IsNotType<int>(1)")]
    [InlineData("Assert.ThrowsException<System.Exception>(() => { })", "Assert.Throws<System.Exception>(() => { })")]
    [InlineData("Assert.ThrowsExceptionAsync<System.Exception>(async () => { })", "Assert.ThrowsAsync<System.Exception>(async () => { })")]
    [InlineData("StringAssert.EndsWith(\"value\", \"v\")", "Assert.EndsWith(\"v\", \"value\")")]
    [InlineData("StringAssert.EndsWith(\"value\", \"v\", System.StringComparison.Ordinal)", "Assert.EndsWith(\"v\", \"value\", System.StringComparison.Ordinal)")]
    [InlineData("StringAssert.StartsWith(\"value\", \"v\")", "Assert.StartsWith(\"v\", \"value\")")]
    [InlineData("StringAssert.StartsWith(\"value\", \"v\", System.StringComparison.Ordinal)", "Assert.StartsWith(\"v\", \"value\", System.StringComparison.Ordinal)")]
    [InlineData("StringAssert.Contains(\"value\", \"v\")", "Assert.Contains(\"v\", \"value\")")]
    [InlineData("StringAssert.Contains(\"value\", \"v\", System.StringComparison.Ordinal)", "Assert.Contains(\"v\", \"value\", System.StringComparison.Ordinal)")]
    [InlineData("CollectionAssert.AreEqual(new[] { 1 }, new[] { 1, 2 })", "Assert.Equal(new[] { 1 }, new[] { 1, 2 })")]
    [InlineData("CollectionAssert.AreEqual(new[] { 1 }, new[] { 1, 2 }, (Comparer)null)", "Assert.Equal(new[] { 1 }, new[] { 1, 2 }, (Comparer)null)")]
    [InlineData("CollectionAssert.AreNotEqual(new[] { 1 }, new[] { 1, 2 })", "Assert.NotEqual(new[] { 1 }, new[] { 1, 2 })")]
    [InlineData("CollectionAssert.AreNotEqual(new[] { 1 }, new[] { 1, 2 }, (Comparer)null)", "Assert.NotEqual(new[] { 1 }, new[] { 1, 2 }, (Comparer)null)")]
    //[InlineData("CollectionAssert.IsSubsetOf(new[] { 1 }, new[] { 1, 2 })", "Assert.Contains(new[] { 1 }, new[] { 1, 2 })")]
    //[InlineData("CollectionAssert.IsNotSubsetOf(new[] { 1 }, new[] { 1, 2 })", "Assert.DoesNotContain(new[] { 1 }, new[] { 1, 2 })")]
    [InlineData("CollectionAssert.Contains(new[] { 1, 2 }, 1)", "Assert.Contains(1, new[] { 1, 2 })")]
    [InlineData("CollectionAssert.DoesNotContain(new[] { 1, 2 }, 1)", "Assert.DoesNotContain(1, new[] { 1, 2 })")]
    public async Task Assert(string source, string expected)
    {
        var context = CreateContext();
        context.TestCode = $$"""
            using MSAssert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
            using MSCollectionAssert = Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert;
            using MSStringAssert = Microsoft.VisualStudio.TestTools.UnitTesting.StringAssert;
            using Assert = Xunit.Assert;

            {|MSTestXunit100:MS{{source}}|};
            
            class Comparer : System.Collections.Generic.IComparer<int>, System.Collections.Generic.IEqualityComparer<int>, System.Collections.IComparer
            {
                public int Compare(int x, int y) => 0;
                public int Compare(object? x, object? y) => 0;
                public bool Equals(int x, int y) => true;
                public int GetHashCode(int obj) => 0;
            }
            """;

        context.FixedCode = $$"""
            using MSAssert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
            using MSCollectionAssert = Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert;
            using MSStringAssert = Microsoft.VisualStudio.TestTools.UnitTesting.StringAssert;
            using Assert = Xunit.Assert;

            {{expected}};

            class Comparer : System.Collections.Generic.IComparer<int>, System.Collections.Generic.IEqualityComparer<int>, System.Collections.IComparer
            {
                public int Compare(int x, int y) => 0;
                public int Compare(object? x, object? y) => 0;
                public bool Equals(int x, int y) => true;
                public int GetHashCode(int obj) => 0;
            }
            """;

        await context.RunAsync();
    }
}
