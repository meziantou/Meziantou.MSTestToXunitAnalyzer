using System.Threading.Tasks;
using Meziantou.MsTestToXunitAnalyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace Meziantou.MsTestToXunitAnalyzerTests;

public class AttributesTests
{
    private static CSharpCodeFixTest<MSTestAnalyzer, AttributesCodeFixer, DefaultVerifier> CreateContext()
    {
        var context = new CSharpCodeFixTest<MSTestAnalyzer, AttributesCodeFixer, DefaultVerifier>()
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
    public async Task TestClassAttribute()
    {
        var context = CreateContext();
        context.TestCode = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;
            using Xunit;

            [{|MSTestXunit200:TestClass|}]
            public class Sample
            {
            }
            """;

        context.FixedCode = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;
            using Xunit;
            
            public class Sample
            {
            }
            """;

        await context.RunAsync();
    }
    
    [Fact]
    public async Task TestClassAttribute_Multi()
    {
        var context = CreateContext();
        context.TestCode = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;
            using Xunit;

            class SampleAttribute : System.Attribute {}

            [{|MSTestXunit200:TestClass|}, Sample]
            public class Sample
            {
            }
            """;

        context.FixedCode = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;
            using Xunit;
            
            class SampleAttribute : System.Attribute {}
            
            [Sample]
            public class Sample
            {
            }
            """;

        await context.RunAsync();
    }

    [Fact]
    public async Task TestMethodAttribute()
    {
        var context = CreateContext();
        context.TestCode = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;
            using Xunit;

            public class Sample
            {
                [{|MSTestXunit201:TestMethod|}]
                public void Test() { }
            }
            """;

        context.FixedCode = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;
            using Xunit;
            
            public class Sample
            {
                [Fact]
                public void Test() { }
            }
            """;

        await context.RunAsync();
    }

    [Fact]
    public async Task DataTestMethodAttribute()
    {
        var context = CreateContext();
        context.TestCode = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;
            using Xunit;

            public class Sample
            {
                [{|MSTestXunit201:DataTestMethod|}]
                [{|MSTestXunit202:DataRow("test")|}]
                [{|MSTestXunit202:DynamicData("test")|}]
                [{|MSTestXunit202:DynamicData("test", DynamicDataDisplayNameDeclaringType = typeof(object))|}]
                public void Test(string a) { }
            }
            """;

        context.FixedCode = """
            using Microsoft.VisualStudio.TestTools.UnitTesting;
            using Xunit;
            
            public class Sample
            {
                [Theory()]
                [InlineData("test")]
                [MemberData("test")]
                [MemberData("test", MemberType = typeof(object))]
                public void Test(string a) { }
            }
            """;

        await context.RunAsync();
    }
}
