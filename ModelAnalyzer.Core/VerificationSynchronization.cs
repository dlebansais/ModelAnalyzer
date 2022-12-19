namespace ModelAnalyzer;

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Provides synchronization features to slow down verification during tests.
/// </summary>
public static class VerificationSynchronization
{
    /// <summary>
    /// Waits for the verifier to finish processing data.
    /// </summary>
    public static async Task SynchronizeWithVerifierAsync()
    {
        await Task.Run(SynchronizeWithVerifier);
    }

    private static void SynchronizeWithVerifier()
    {
        SynchronizationMutex.WaitOne();

        // Wait at least two seconds between synchronization checks.
        // If previous uses of ClassModelManager didn't send a request, VerificationRequestCount will be 0 and we don't wait more.
        // Otherwise, odds are the request has been sent and we'll wait.
        // It doesn't matter if we don't synchronize well, all we want is to avoid an avalanche of requests sent together.
        while (SynchronizationWatch.Elapsed < TimeSpan.FromSeconds(2))
            Thread.Sleep(100);

        while (VerificationRequestCount > 0 && VerifierCallWatch.Elapsed < TimeSpan.FromSeconds(5))
            Thread.Sleep(100);

        Interlocked.Exchange(ref VerificationRequestCount, 0);
        VerifierCallWatch.Stop();

        SynchronizationWatch.Restart();
        SynchronizationMutex.ReleaseMutex();
    }

    /// <summary>
    /// Notifies synchronization that a request has been sent.
    /// </summary>
    public static void NotifyRequestSent()
    {
        Interlocked.Increment(ref VerificationRequestCount);
        VerifierCallWatch.Restart();
    }

    /// <summary>
    /// Notifies synchronization that a request ack has been received.
    /// </summary>
    public static void NotifyAcknowledgeReceived()
    {
        if (Interlocked.Decrement(ref VerificationRequestCount) == 0)
            VerifierCallWatch.Stop();
    }

    private static Mutex SynchronizationMutex = new();
    private static Stopwatch SynchronizationWatch = Stopwatch.StartNew();
    private static Stopwatch VerifierCallWatch = new();
    private static int VerificationRequestCount;
}
