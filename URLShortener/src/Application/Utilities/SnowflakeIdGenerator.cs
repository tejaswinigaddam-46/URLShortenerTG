using System;

namespace URLShortener.Application.Utilities
{
    public class SnowflakeIdGenerator
    {
        private static readonly long Epoch = 1704067200000L; // 2024-01-01 00:00:00 UTC
        private static readonly int MachineIdBits = 5;
        private static readonly int DatacenterIdBits = 5;
        private static readonly int SequenceBits = 12;

        private static readonly long MaxMachineId = -1L ^ (-1L << MachineIdBits);
        private static readonly long MaxDatacenterId = -1L ^ (-1L << DatacenterIdBits);

        private static readonly int MachineIdShift = SequenceBits;
        private static readonly int DatacenterIdShift = SequenceBits + MachineIdBits;
        private static readonly int TimestampLeftShift = SequenceBits + MachineIdBits + DatacenterIdBits;
        private static readonly long SequenceMask = -1L ^ (-1L << SequenceBits);

        private long _machineId;
        private long _datacenterId;
        private long _sequence = 0L;
        private long _lastTimestamp = -1L;

        private static readonly object Lock = new object();

        public SnowflakeIdGenerator(long machineId = 1, long datacenterId = 1)
        {
            if (machineId > MaxMachineId || machineId < 0)
                throw new ArgumentException($"machine Id can't be greater than {MaxMachineId} or less than 0");
            if (datacenterId > MaxDatacenterId || datacenterId < 0)
                throw new ArgumentException($"datacenter Id can't be greater than {MaxDatacenterId} or less than 0");

            _machineId = machineId;
            _datacenterId = datacenterId;
        }

        public long NextId()
        {
            lock (Lock)
            {
                long timestamp = TimeGen();

                if (timestamp < _lastTimestamp)
                {
                    throw new Exception($"Clock moved backwards. Refusing to generate id for {_lastTimestamp - timestamp} milliseconds");
                }

                if (_lastTimestamp == timestamp)
                {
                    // If it's the same millisecond, increment the sequence
                    _sequence = (_sequence + 1) & SequenceMask;
                    
                    // If sequence overflow (reaches 0 after mask), wait for next millisecond
                    if (_sequence == 0)
                    {
                        timestamp = TilNextMillis(_lastTimestamp);
                    }
                }
                else
                {
                    // New millisecond, reset sequence to 0
                    _sequence = 0L;
                }

                _lastTimestamp = timestamp;

                // Shift and combine components to create the 64-bit ID
                long id = ((timestamp - Epoch) << TimestampLeftShift) |
                          (_datacenterId << DatacenterIdShift) |
                          (_machineId << MachineIdShift) |
                          _sequence;

                // Ensure the ID is positive (bit 63 is 0)
                return id & long.MaxValue;
            }
        }

        private long TilNextMillis(long lastTimestamp)
        {
            long timestamp = TimeGen();
            while (timestamp <= lastTimestamp)
            {
                timestamp = TimeGen();
            }
            return timestamp;
        }

        private long TimeGen()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}
