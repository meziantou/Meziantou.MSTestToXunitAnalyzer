# Meziantou.MsTestToXunitAnalyzer

This package provides a Roslyn Analyzer to convert MSTest tests to xUnit tests.

Goals:
- Convert most MSTest attributes to xUnit equivalents
- Convert most MSTest Assert calls to xUnit Assert calls

This doesn't handle 100% cases as this would not be possible. However, it should handle 90% of the cases.