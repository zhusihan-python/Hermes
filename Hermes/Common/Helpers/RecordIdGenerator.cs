using System;
using System.Threading;

namespace Hermes.Common.Helpers;

public class RecordIdGenerator
{
    private static long _lastTimestamp = -1;
    private static long _sequence = 0;
    private static readonly object _lock = new object();

    public static string GenerateUniqueTaskIdRecordId()
    {
        lock (_lock)
        {
            long currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            if (currentTimestamp == _lastTimestamp)
            {
                // Same millisecond, increment sequence. Max 999 sequence numbers.
                _sequence = (_sequence + 1) % 1000;
                if (_sequence == 0) // Sequence overflowed within the millisecond
                {
                    // Wait for the next millisecond to ensure unique IDs
                    currentTimestamp = GetNextMillisecond(currentTimestamp);
                }
            }
            else
            {
                _sequence = 0;
            }

            _lastTimestamp = currentTimestamp;

            return $"{currentTimestamp}{_sequence:D3}";
        }
    }

    private static long GetNextMillisecond(long currentTimestamp)
    {
        long newTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        SpinWait spinWait = new SpinWait();
        while (newTimestamp <= currentTimestamp)
        {
            spinWait.SpinOnce();
            newTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
        return newTimestamp;
    }
}
