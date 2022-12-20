namespace ProcessCommunication;

using System;
using System.IO.MemoryMappedFiles;

/// <summary>
/// Represents a communication channel.
/// </summary>
public class Channel : IDisposable
{
    /// <summary>
    /// Gets a shared guid from client to server.
    /// </summary>
    public static Guid ClientToServerGuid { get; } = new Guid("{03C9C797-C924-415E-A6F9-9112AE75E56F}");

    /// <summary>
    /// Gets the max channel capacity.
    /// </summary>
    public const int Capacity = 0x100000;

    /// <summary>
    /// Initializes a new instance of the <see cref="Channel"/> class.
    /// </summary>
    /// <param name="guid">The channel guid.</param>
    /// <param name="mode">The caller channel mode.</param>
    public Channel(Guid guid, Mode mode)
    {
        Guid = guid;
        Mode = mode;
    }

    /// <summary>
    /// Gets the channel guid.
    /// </summary>
    public Guid Guid { get; }

    /// <summary>
    /// Gets the caller channel mode.
    /// </summary>
    public Mode Mode { get; }

    /// <summary>
    /// Opens the chanel.
    /// </summary>
    public void Open()
    {
        if (IsOpen)
            throw new InvalidOperationException();

        try
        {
            int CapacityWithHeadTail = Capacity + (sizeof(int) * 2);
            string ChannelName = Guid.ToString("B");

            switch (Mode)
            {
                default:
                case Mode.Send:
                    File = MemoryMappedFile.OpenExisting(ChannelName, MemoryMappedFileRights.ReadWrite);
                    break;
                case Mode.Receive:
                    File = MemoryMappedFile.CreateNew(ChannelName, CapacityWithHeadTail, MemoryMappedFileAccess.ReadWrite);
                    break;
            }

            Accessor = File.CreateViewAccessor();
        }
        catch (Exception exception)
        {
            LastError = exception.Message;
            Close();
        }
    }

    /// <summary>
    /// Gets a value indicating whether the channel is open.
    /// </summary>
    public bool IsOpen { get => File is not null && Accessor is not null; }

    /// <summary>
    /// Gets the last error.
    /// </summary>
    public string LastError { get; private set; } = string.Empty;

    /// <summary>
    /// Closes the chanel.
    /// </summary>
    public void Close()
    {
        if (Accessor is not null)
        {
            using (MemoryMappedViewAccessor DisposedAccessor = Accessor)
            {
                Accessor = null;
            }
        }

        if (File is not null)
        {
            using (MemoryMappedFile DisposedFile = File)
            {
                File = null;
            }
        }
    }

    /// <summary>
    /// Reads data from the channel.
    /// </summary>
    public byte[]? Read()
    {
        if (Accessor is null)
            throw new InvalidOperationException();

        int EndOfBuffer = Capacity;
        Accessor.Read(EndOfBuffer, out int Head);
        Accessor.Read(EndOfBuffer + sizeof(int), out int Tail);

        byte[]? Result;

        if (Head > Tail)
        {
            int Length = Head - Tail;
            Result = new byte[Length];
            Accessor.ReadArray(Tail, Result, 0, Length);
        }
        else if (Head < Tail)
        {
            int Length = EndOfBuffer - Tail + Head;
            Result = new byte[Length];
            Accessor.ReadArray(Tail, Result, 0, EndOfBuffer - Tail);
            Accessor.ReadArray(0, Result, EndOfBuffer - Tail, Head);
        }
        else
            return null;

        // Copy head to tail.
        Accessor.Write(EndOfBuffer + sizeof(int), Head);

        return Result;
    }

    /// <summary>
    /// Gets the number of free bytes in the channel.
    /// </summary>
    public int GetFreeLength()
    {
        if (Accessor is null)
            throw new InvalidOperationException();

        int EndOfBuffer = Capacity;
        Accessor.Read(EndOfBuffer, out int Head);
        Accessor.Read(EndOfBuffer + sizeof(int), out int Tail);

        int Length;

        if (Tail > Head)
            Length = Tail - Head - 1;
        else
            Length = EndOfBuffer - Head + Tail - 1;

        return Length;
    }

    /// <summary>
    /// Writes data to the channel.
    /// </summary>
    public void Write(byte[] data)
    {
        if (Accessor is null)
            throw new InvalidOperationException();

        int EndOfBuffer = Capacity;
        Accessor.Read(EndOfBuffer, out int Head);
        Accessor.Read(EndOfBuffer + sizeof(int), out int Tail);

        int Length = data.Length;

        if (Tail > Head)
        {
            if (Length > Tail - Head - 1)
                throw new InvalidOperationException();

            Accessor.WriteArray(Head, data, 0, data.Length);
        }
        else
        {
            if (Length > EndOfBuffer - Tail + Head)
                throw new InvalidOperationException();

            int FirstCopyLength = Math.Min(EndOfBuffer - Head, Length);
            int SecondCopyLength = Length - FirstCopyLength;

            Accessor.WriteArray(Head, data, 0, FirstCopyLength);
            Accessor.WriteArray(Head + FirstCopyLength, data, FirstCopyLength, SecondCopyLength);
        }

        Head += Length;
        if (Head >= EndOfBuffer)
            Head -= EndOfBuffer;

        // Copy the new head.
        Accessor.Write(EndOfBuffer, Head);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Close();
    }

    private MemoryMappedFile? File;
    private MemoryMappedViewAccessor? Accessor;
}