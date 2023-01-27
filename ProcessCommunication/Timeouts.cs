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
    /// The timeout waiting for channels to no longer be busy.
    /// </summary>
    public static readonly TimeSpan VerificationBusyTimeout = TimeSpan.FromSeconds(5);

    /// <summary>
    /// The timeout waiting for verification acknowledge. TODO: add an ack when there is no more incoming results, otherwise we always wait for the timeout even in case of success.
    /// </summary>
    public static readonly TimeSpan VerificationAcknowledgeTimeout = TimeSpan.FromSeconds(10);

    /// <summary>
    /// The timeout waiting for new data to verify.
    /// </summary>
    public static readonly TimeSpan VerificationIdleTimeout = TimeSpan.FromSeconds(60);
}
