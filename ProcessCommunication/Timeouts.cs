namespace ProcessCommunication;

using System;

/// <summary>
/// Provides constants for variuous timeouts.
/// </summary>
public static class Timeouts
{
    /// <summary>
    /// The timeout waiting for the verifier process to be started.
    /// </summary>
    public static readonly TimeSpan VerifierProcessLaunchTimeout = TimeSpan.FromSeconds(5);

    /// <summary>
    /// The timeout waiting for verification acknowledge.
    /// </summary>
    public static readonly TimeSpan VerificationAcknowledgeTimeout = TimeSpan.FromSeconds(5);

    /// <summary>
    /// The timeout waiting for new data to verify.
    /// </summary>
    public static readonly TimeSpan VerificationIdleTimeout = TimeSpan.FromSeconds(60);
}
