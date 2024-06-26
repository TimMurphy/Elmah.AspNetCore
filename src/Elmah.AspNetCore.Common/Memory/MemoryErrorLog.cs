using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable MemberCanBePrivate.Global
namespace Elmah.AspNetCore.Memory;

/// <summary>
///     An <see cref="ErrorLog" /> implementation that uses memory as its
///     backing store.
/// </summary>
/// <remarks>
///     All <see cref="MemoryErrorLog" /> instances will share the same memory
///     store that is bound to the application (not an instance of this class).
/// </remarks>
public sealed class MemoryErrorLog : ErrorLog
{
    //
    // The collection that provides the actual storage for this log
    // implementation and a lock to guarantee concurrency correctness.
    //
    private readonly EntryCollection _entries;
    private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

    /// <summary>
    ///     The maximum number of errors that will ever be allowed to be stored
    ///     in memory.
    /// </summary>
    private const int MaximumSize = 500;

    /// <summary>
    ///     The maximum number of errors that will be held in memory by default
    ///     if no size is specified.
    /// </summary>
    private const int DefaultSize = 15;

    public MemoryErrorLog(IOptions<MemoryErrorLogOptions> options) : this(options.Value.Size) { }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemoryErrorLog" /> class
    ///     with a default size for maximum recordable entries.
    /// </summary>

    // ReSharper disable once UnusedMember.Global
    public MemoryErrorLog() : this(DefaultSize)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MemoryErrorLog" /> class
    ///     with a specific size for maximum recordable entries.
    /// </summary>
    // ReSharper disable once MemberCanBePrivate.Global
    public MemoryErrorLog(int size)
    {
        if (size < 0 || size > MaximumSize)
        {
            throw new ArgumentOutOfRangeException(nameof(size), size, $"Size must be between 0 and {MaximumSize}.");
        }

        _entries = new EntryCollection(size);
    }

    /// <summary>
    ///     Gets the name of this error log implementation.
    /// </summary>
    public override string Name => "In-Memory Error Log";

    public override Task LogAsync(Error error, CancellationToken cancellationToken)
    {
        if (error == null)
        {
            throw new ArgumentNullException(nameof(error));
        }

        //
        // Make a copy of the error to log since the source is mutable.
        // Assign a new GUID and create an entry for the error.
        //
        error = error.Clone();
        error.ApplicationName = ApplicationName;
        var entry = new ErrorLogEntry(this, error);

        _lock.EnterWriteLock();

        try
        {
            _entries.Add(entry);
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Returns the specified error from application memory, or null
    ///     if it does not exist.
    /// </summary>
    public override Task<ErrorLogEntry?> GetErrorAsync(Guid id, CancellationToken cancellationToken)
    {
        _lock.EnterReadLock();

        ErrorLogEntry? entry;

        try
        {
            if (_entries == null)
            {
                return Task.FromResult((ErrorLogEntry?)null);
            }

            entry = _entries[id];
        }
        finally
        {
            _lock.ExitReadLock();
        }

        if (entry == null)
        {
            return Task.FromResult((ErrorLogEntry?)null);
        }

        //
        // Return a copy that the caller can party on.
        //
        var error = entry.Error.Clone();
        return Task.FromResult<ErrorLogEntry?>(new ErrorLogEntry(this, error));
    }

    /// <summary>
    ///     Returns a page of errors from the application memory in
    ///     descending order of logged time.
    /// </summary>
    public override Task<int> GetErrorsAsync(ErrorLogFilterCollection filters, int errorIndex, int pageSize,
        ICollection<ErrorLogEntry> entries, CancellationToken cancellationToken)
    {
        if (errorIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(errorIndex), errorIndex, null);
        }

        if (pageSize < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, null);
        }

        //
        // To minimize the time for which we hold the lock, we'll first
        // grab just references to the entries we need to return. Later,
        // we'll make copies and return those to the caller. Since Error 
        // is mutable, we don't want to return direct references to our 
        // internal versions since someone could change their state.
        //
        ErrorLogEntry[]? selectedEntries = null;
        int totalCount;

        _lock.EnterReadLock();

        try
        {
            if (_entries == null)
            {
                return Task.FromResult(0);
            }

            var sourceEntries = (KeyedCollection<Guid, ErrorLogEntry>)_entries;
            totalCount = _entries.Count;
            
            if (filters.Count > 0)
            {
                var items = _entries.Where(filters.IsMatch);
                sourceEntries = new EntryCollection(items.ToList());
                totalCount = sourceEntries.Count;
            }

            var startIndex = errorIndex;
            var endIndex = Math.Min(startIndex + pageSize, totalCount);
            var count = Math.Max(0, endIndex - startIndex);

            if (count > 0)
            {
                selectedEntries = new ErrorLogEntry[count];

                var sourceIndex = endIndex;
                var targetIndex = 0;

                while (sourceIndex > startIndex)
                {
                    selectedEntries[targetIndex++] = sourceEntries[--sourceIndex];
                }
            }
        }
        finally
        {
            _lock.ExitReadLock();
        }

        if (entries != null && selectedEntries != null)
        {
            //
            // Return copies of fetched entries. If the Error class would 
            // be immutable then this step wouldn't be necessary.
            //
            foreach (var entry in selectedEntries)
            {
                var error = entry.Error.Clone();
                entries.Add(new ErrorLogEntry(this, error));
            }
        }

        return Task.FromResult(totalCount);
    }

    private sealed class EntryCollection : KeyedCollection<Guid, ErrorLogEntry>
    {
        private readonly int _size;

        public EntryCollection(ICollection<ErrorLogEntry> items) : this(items.Count)
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }

        public EntryCollection(int size)
        {
            _size = size;
        }

        protected override Guid GetKeyForItem(ErrorLogEntry item)
        {
            return item.Id;
        }

        protected override void InsertItem(int index, ErrorLogEntry item)
        {
            if (Count == _size)
            {
                RemoveAt(0);
                index--;
            }

            base.InsertItem(index, item);
        }
    }
}