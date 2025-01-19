using Microsoft.CodeAnalysis;

namespace Meziantou.MsTestToXunitAnalyzer;

internal sealed class WellKnownTypes(Compilation compilation)
{
    public INamedTypeSymbol IDisposableSymbol { get; } = compilation.GetTypeByMetadataName("System.IDisposable");

    public INamedTypeSymbol MSTestAssertSymbol { get; } = compilation.GetTypeByMetadataName("Microsoft.VisualStudio.TestTools.UnitTesting.Assert");
    public INamedTypeSymbol MSTestCollectionAssertSymbol { get; } = compilation.GetTypeByMetadataName("Microsoft.VisualStudio.TestTools.UnitTesting.CollectionAssert");
    public INamedTypeSymbol MSTestStringAssertSymbol { get; } = compilation.GetTypeByMetadataName("Microsoft.VisualStudio.TestTools.UnitTesting.StringAssert");

    public INamedTypeSymbol MSTestTestClassAttributeSymbol { get; } = compilation.GetTypeByMetadataName("Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute");
    public INamedTypeSymbol MSTestTestMethodAttributeSymbol { get; } = compilation.GetTypeByMetadataName("Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute");
    public INamedTypeSymbol MSTestDataTestMethodAttributeSymbol { get; } = compilation.GetTypeByMetadataName("Microsoft.VisualStudio.TestTools.UnitTesting.DataTestMethodAttribute");
    public INamedTypeSymbol MSTestDataRowAttributeSymbol { get; } = compilation.GetTypeByMetadataName("Microsoft.VisualStudio.TestTools.UnitTesting.DataRowAttribute");
    public INamedTypeSymbol MSTestDynamicDataAttributeSymbol { get; } = compilation.GetTypeByMetadataName("Microsoft.VisualStudio.TestTools.UnitTesting.DynamicDataAttribute");

    public INamedTypeSymbol MSTestAssemblyInitializeAttributeSymbol { get; } = compilation.GetTypeByMetadataName("Microsoft.VisualStudio.TestTools.UnitTesting.AssemblyInitializeAttribute");
    public INamedTypeSymbol MSTestAssemblyCleanupAttributeSymbol { get; } = compilation.GetTypeByMetadataName("Microsoft.VisualStudio.TestTools.UnitTesting.AssemblyCleanupAttribute");
    public INamedTypeSymbol MSTestClassInitializeAttributeSymbol { get; } = compilation.GetTypeByMetadataName("Microsoft.VisualStudio.TestTools.UnitTesting.ClassInitializeAttribute");
    public INamedTypeSymbol MSTestClassCleanupAttributeSymbol { get; } = compilation.GetTypeByMetadataName("Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanupAttribute");
    public INamedTypeSymbol MSTestTestInitializeAttributeSymbol { get; } = compilation.GetTypeByMetadataName("Microsoft.VisualStudio.TestTools.UnitTesting.TestInitializeAttribute");
    public INamedTypeSymbol MSTestTestCleanupAttributeSymbol { get; } = compilation.GetTypeByMetadataName("Microsoft.VisualStudio.TestTools.UnitTesting.TestCleanupAttribute");
    public INamedTypeSymbol MSTestClassCleanupExecutionAttributeSymbol { get; } = compilation.GetTypeByMetadataName("Microsoft.VisualStudio.TestTools.UnitTesting.ClassCleanupExecutionAttribute");
    public INamedTypeSymbol MSTestParallelizeAttributeSymbol { get; } = compilation.GetTypeByMetadataName("Microsoft.VisualStudio.TestTools.UnitTesting.ParallelizeAttribute");

    public INamedTypeSymbol MSTestUnitTestAssertExceptionSymbol { get; } = compilation.GetTypeByMetadataName("Microsoft.VisualStudio.TestTools.UnitTesting.UnitTestAssertException");
    public INamedTypeSymbol MSTestITestDataSourceSymbol { get; } = compilation.GetTypeByMetadataName("Microsoft.VisualStudio.TestTools.UnitTesting.ITestDataSource");

    public INamedTypeSymbol XunitAssertSymbol { get; } = compilation.GetTypeByMetadataName("Xunit.Assert");
    public INamedTypeSymbol XunitFactAttributeSymbol { get; } = compilation.GetTypeByMetadataName("Xunit.FactAttribute");
    public INamedTypeSymbol XunitTheoryAttributeSymbol { get; } = compilation.GetTypeByMetadataName("Xunit.TheoryAttribute");
    public INamedTypeSymbol XunitInlineDataAttributeSymbol { get; } = compilation.GetTypeByMetadataName("Xunit.InlineDataAttribute");
    public INamedTypeSymbol XunitMemberDataAttributeSymbol { get; } = compilation.GetTypeByMetadataName("Xunit.MemberDataAttribute");
}
