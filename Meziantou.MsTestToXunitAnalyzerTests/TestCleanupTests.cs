using System.Threading.Tasks;
using Meziantou.MsTestToXunitAnalyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace Meziantou.MsTestToXunitAnalyzerTests;

public class TestCleanupTests
{
    private static CSharpCodeFixTest<MSTestAnalyzer, TestCleanupCodeFixer, DefaultVerifier> CreateContext()
    {
        var context = new CSharpCodeFixTest<MSTestAnalyzer, TestCleanupCodeFixer, DefaultVerifier>()
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net80
                        .AddPackages([new PackageIdentity("MSTest.TestFramework", "3.7.1")])
                        .AddPackages([new PackageIdentity("xunit.v3.assert", "1.0.1")])
                        .AddPackages([new PackageIdentity("xunit.v3.extensibility.core", "1.0.1")]),
            TestState =
            {
                OutputKind = OutputKind.DynamicallyLinkedLibrary,
            },
        };

        return context;
    }

    [Fact]
    public async Task TestCleanupAttribute()
    {
        var context = CreateContext();
        context.TestCode = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;

            public class Test
            {
                // test
                [{|MSTestXunit208:TestCleanup|}]
                public static void A() { }
            }
            """;

        context.FixedCode = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;
            
            public class Test : System.IDisposable
            {
                // test
                public void Dispose()
                {
                }
            }
            """;

        await context.RunAsync();
    }
}
