using System.Threading.Tasks;
using Meziantou.MsTestToXunitAnalyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace Meziantou.MsTestToXunitAnalyzerTests;

public class AttributesTestsWithoutFixer
{
    private static CSharpAnalyzerTest<MSTestAnalyzer, DefaultVerifier> CreateContext()
    {
        var context = new CSharpAnalyzerTest<MSTestAnalyzer, DefaultVerifier>()
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
    public async Task AssemblyParallelizeAttribute()
    {
        var context = CreateContext();
        context.TestCode = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;

            [assembly: {|MSTestXunit209:Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)|}]
            """;

        await context.RunAsync();
    }

    [Fact]
    public async Task AssemblyInitializeAttribute()
    {
        var context = CreateContext();
        context.TestCode = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;

            public class Test
            {
                [{|MSTestXunit203:AssemblyInitialize|}]
                public static void A() { }
            }
            """;

        await context.RunAsync();
    }

    [Fact]
    public async Task AssemblyCleanupAttribute()
    {
        var context = CreateContext();
        context.TestCode = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;

            public class Test
            {
                [{|MSTestXunit204:AssemblyCleanup|}]
                public static void A() { }
            }
            """;

        await context.RunAsync();
    }

    [Fact]
    public async Task ClassInitializeAttribute()
    {
        var context = CreateContext();
        context.TestCode = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;

            public class Test
            {
                [{|MSTestXunit205:ClassInitialize|}]
                public static void A() { }
            }
            """;

        await context.RunAsync();
    }

    [Fact]
    public async Task ClassCleanupAttribute()
    {
        var context = CreateContext();
        context.TestCode = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;

            public class Test
            {
                [{|MSTestXunit206:ClassCleanup|}]
                public static void A() { }
            }
            """;

        await context.RunAsync();
    }

    [Fact]
    public async Task TestCleanupAttribute()
    {
        var context = CreateContext();
        context.TestCode = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;

            public class Test
            {
                [{|MSTestXunit208:TestCleanup|}]
                public static void A() { }
            }
            """;

        await context.RunAsync();
    }
}
