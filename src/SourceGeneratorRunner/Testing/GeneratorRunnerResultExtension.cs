namespace SourceGeneratorRunner.Testing;

public static class RunnerResultExtension
{
    public static bool Verify(this RunnerResult result, Func<RunnerResult, bool> verifier)
    {
        return verifier(result);
    }

    public static void Verify(this RunnerResult result, Action<RunnerResult> verifier)
    {
        verifier(result);
    }
}