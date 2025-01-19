using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace Meziantou.MsTestToXunitAnalyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MSTestAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor ExceptionRule = new(
       "MSTestXunit000",
       title: "Replace MSTest exceptions with equivalent",
       messageFormat: "Use xUnit exceptions",
       description: "",
       category: "Design",
       defaultSeverity: DiagnosticSeverity.Warning,
       isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor AssertRule = new(
       "MSTestXunit100",
       title: "Replace MSTest assertions with xUnit assertions",
       messageFormat: "Use xUnit equivalent",
       description: "",
       category: "Design",
       defaultSeverity: DiagnosticSeverity.Warning,
       isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor TestClassAttributeRule = new(
       "MSTestXunit200",
       title: "Replace MSTest attributes with xUnit equivalent",
       messageFormat: "Use xUnit attributes",
       description: "",
       category: "Design",
       defaultSeverity: DiagnosticSeverity.Warning,
       isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor TestMethodAttributeRule = new(
       "MSTestXunit201",
       title: "Replace MSTest attributes with xUnit equivalent",
       messageFormat: "Use xUnit attributes",
       description: "",
       category: "Design",
       defaultSeverity: DiagnosticSeverity.Warning,
       isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor DataRowAttributeRule = new(
       "MSTestXunit202",
       title: "Replace MSTest attributes with xUnit equivalent",
       messageFormat: "Use xUnit attributes",
       description: "",
       category: "Design",
       defaultSeverity: DiagnosticSeverity.Warning,
       isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor AssemblyInitializeAttributeRule = new(
       "MSTestXunit203",
       title: "Replace MSTest attributes with xUnit equivalent",
       messageFormat: "Use xUnit attributes",
       description: "",
       category: "Design",
       defaultSeverity: DiagnosticSeverity.Warning,
       isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor AssemblyCleanupAttributeRule = new(
       "MSTestXunit204",
       title: "Replace MSTest attributes with xUnit equivalent",
       messageFormat: "Use xUnit attributes",
       description: "",
       category: "Design",
       defaultSeverity: DiagnosticSeverity.Warning,
       isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor ClassInitializeAttributeRule = new(
       "MSTestXunit205",
       title: "Replace MSTest attributes with xUnit equivalent",
       messageFormat: "Use xUnit equivalent",
       description: "",
       category: "Design",
       defaultSeverity: DiagnosticSeverity.Warning,
       isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor ClassCleanupAttributeRule = new(
       "MSTestXunit206",
       title: "Replace MSTest attributes with xUnit attributes",
       messageFormat: "Use xUnit equivalent",
       description: "",
       category: "Design",
       defaultSeverity: DiagnosticSeverity.Warning,
       isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor TestInitializeAttributeRule = new(
       "MSTestXunit207",
       title: "Replace MSTest attributes with xUnit equivalent",
       messageFormat: "Use xUnit equivalent",
       description: "",
       category: "Design",
       defaultSeverity: DiagnosticSeverity.Warning,
       isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor TestCleanupAttributeRule = new(
       "MSTestXunit208",
       title: "Replace MSTest attributes with xUnit equivalent",
       messageFormat: "Use xUnit equivalent",
       description: "",
       category: "Design",
       defaultSeverity: DiagnosticSeverity.Warning,
       isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor ParallelizeAttributeRule = new(
       "MSTestXunit209",
       title: "Replace MSTest attributes with xUnit equivalent",
       messageFormat: "Use xUnit equivalent",
       description: "",
       category: "Design",
       defaultSeverity: DiagnosticSeverity.Warning,
       isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
        TestClassAttributeRule,
        TestMethodAttributeRule,
        DataRowAttributeRule,
        AssertRule,
        ExceptionRule,
        AssemblyInitializeAttributeRule,
        AssemblyCleanupAttributeRule,
        ClassInitializeAttributeRule,
        ClassCleanupAttributeRule,
        TestInitializeAttributeRule,
        TestCleanupAttributeRule,
        ParallelizeAttributeRule);

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        context.RegisterCompilationStartAction(context =>
        {
            var wellKnownTypes = new WellKnownTypes(context.Compilation);

            context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
            context.RegisterOperationAction(AnalyzeThrow, OperationKind.Throw);
            context.RegisterOperationAction(AnalyzeAttribute, OperationKind.Attribute);

            void AnalyzeAttribute(OperationAnalysisContext context)
            {
                var attribute = (IAttributeOperation)context.Operation;
                var attributeType = attribute.Operation.Type;
                if (attributeType is null)
                    return;

                if (attributeType.IsOrImplements(wellKnownTypes.MSTestITestDataSourceSymbol))
                {
                    context.ReportDiagnostic(Diagnostic.Create(DataRowAttributeRule, attribute.Syntax.GetLocation()));
                }

                ReportAttribute(wellKnownTypes.MSTestParallelizeAttributeSymbol, ParallelizeAttributeRule);
                ReportAttribute(wellKnownTypes.MSTestTestClassAttributeSymbol, TestClassAttributeRule);
                ReportAttribute(wellKnownTypes.MSTestTestMethodAttributeSymbol, TestMethodAttributeRule);
                ReportAttribute(wellKnownTypes.MSTestAssemblyInitializeAttributeSymbol, AssemblyInitializeAttributeRule);
                ReportAttribute(wellKnownTypes.MSTestAssemblyCleanupAttributeSymbol, AssemblyCleanupAttributeRule);
                ReportAttribute(wellKnownTypes.MSTestClassInitializeAttributeSymbol, ClassInitializeAttributeRule);
                ReportAttribute(wellKnownTypes.MSTestClassCleanupAttributeSymbol, ClassCleanupAttributeRule);
                ReportAttribute(wellKnownTypes.MSTestTestInitializeAttributeSymbol, TestInitializeAttributeRule);
                ReportAttribute(wellKnownTypes.MSTestTestCleanupAttributeSymbol, TestCleanupAttributeRule);

                void ReportAttribute(INamedTypeSymbol type, DiagnosticDescriptor descriptor)
                {
                    if (attributeType.IsOrInheritsFrom(type))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(descriptor, attribute.Syntax.GetLocation()));
                    }
                }
            }

            void AnalyzeInvocation(OperationAnalysisContext context)
            {
                var op = (IInvocationOperation)context.Operation;
                if (IsMsTestAssertClass(op.TargetMethod.ContainingType))
                {
                    context.ReportDiagnostic(Diagnostic.Create(AssertRule, op.Syntax.GetLocation()));
                }
            }

            void AnalyzeThrow(OperationAnalysisContext context)
            {
                var op = (IThrowOperation)context.Operation;
                if (op.Exception is not null && op.Exception.RemoveImplicitConversion().Type.IsOrInheritsFrom(wellKnownTypes.MSTestUnitTestAssertExceptionSymbol))
                {
                    context.ReportDiagnostic(Diagnostic.Create(ExceptionRule, op.Syntax.GetLocation()));
                }
            }

            bool IsMsTestAssertClass(ITypeSymbol typeSymbol)
            {
                if (typeSymbol is null)
                    return false;

                return typeSymbol.Equals(wellKnownTypes.MSTestAssertSymbol, SymbolEqualityComparer.Default)
                    || typeSymbol.Equals(wellKnownTypes.MSTestStringAssertSymbol, SymbolEqualityComparer.Default)
                    || typeSymbol.Equals(wellKnownTypes.MSTestCollectionAssertSymbol, SymbolEqualityComparer.Default);
            }
        });
    }
}
