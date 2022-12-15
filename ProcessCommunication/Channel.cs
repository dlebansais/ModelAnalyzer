namespace ProcessCommunication;

using System;
using System.IO.MemoryMappedFiles;

/// <summary>
/// Represents a communication channel.
/// </summary>
public class Channel
{
    /// <summary>
    /// Gets a shared guid.
    /// </summary>
    public static Guid SharedGuid { get; } = new Guid("{03C9C797-C924-415E-A6F9-9112AE75E56F}");

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
    public bool Open()
    {
        try
        {
            int CapacityWithHeadTail = Capacity + sizeof(int) * 2;
            string ChannelName = Guid.ToString("B");

            if (Mode == Mode.Server)
                File = MemoryMappedFile.CreateNew(ChannelName, CapacityWithHeadTail, MemoryMappedFileAccess.ReadWrite);
            else
                File = MemoryMappedFile.OpenExisting(ChannelName, MemoryMappedFileRights.ReadWrite);

            Accessor = File.CreateViewAccessor();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Closes the chanel.
    /// </summary>
    public void Close()
    {
        if (File is null || Accessor is null)
            return;

        using (MemoryMappedViewAccessor DisposedAccessor = Accessor)
        {
            Accessor = null;
        }

        using (MemoryMappedFile DisposedFile = File)
        {
            File = null;
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

            Accessor.WriteArray(Tail, data, 0, FirstCopyLength);
            Accessor.WriteArray(Tail + FirstCopyLength, data, FirstCopyLength, SecondCopyLength);
        }

        Head += Length;
        if (Head >= EndOfBuffer)
            Head -= EndOfBuffer;

        // Copy the new head.
        Accessor.Write(EndOfBuffer, Head);
    }

    private MemoryMappedFile? File;
    private MemoryMappedViewAccessor? Accessor;
}