// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Blockchain.Scripts;
using System;
using System.Diagnostics;

namespace Autarkysoft.Bitcoin
{
    /// <summary>
    /// A size counter, to be used to calculate size of <see cref="IDeserializable"/> objects without actually serializing them.
    /// </summary>
    public class SizeCounter
    {
        /// <summary>
        /// Initializes a new instance of <see cref="SizeCounter"/> starting at 0 size.
        /// </summary>
        public SizeCounter()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="SizeCounter"/> starting with the given size.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="initialSize">The initial size</param>
        public SizeCounter(int initialSize)
        {
            if (initialSize < 0)
                throw new ArgumentOutOfRangeException(nameof(initialSize), "Initial size can not be negative.");

            Size = initialSize;
        }


        /// <summary>
        /// Returns the total size that is counted so far.
        /// </summary>
        public int Size { get; private set; }

        /// <summary>
        /// Add the given size to this counter
        /// </summary>
        /// <param name="additionalSize">Size to add (has to be positive, won't be checked)</param>
        public void Add(int additionalSize) => Size += additionalSize;

        /// <summary>
        /// Adds the given size with the size of its preceeding <see cref="StackInt"/> length.
        /// <para/>Useful for objects such as PushDataOp that write data to stream using <see cref="StackInt"/>
        /// ie: <see cref="StackInt"/> size + data
        /// </summary>
        /// <param name="additionalSize">Size to add (has to be positive)</param>
        public void AddWithStackIntLength(int additionalSize)
        {
            if (additionalSize < 0)
            {
                return;
            }
            if (additionalSize < (int)OP.PushData1) // StackInt is 1 Byte long: size + data
            {
                Size += additionalSize + 1;
            }
            else if (additionalSize <= byte.MaxValue) // StackInt is 2 Bytes long: OP_PushData1 + (byte)size + data
            {
                Size += additionalSize + 2;
            }
            else if (additionalSize <= ushort.MaxValue) // StackInt is 3 Bytes long: OP_PushData2 + (ushort)size + data
            {
                Size += additionalSize + 3;
            }
            else // additionalSize <= uint.MaxValue -> StackInt is 3 Bytes long: OP_PushData2 + (uint)size + data
            {
                Size += additionalSize + 5;
            }
        }

        /// <summary>
        /// Adds the given size with the size of its preceeding <see cref="CompactInt"/> length.
        /// <para/>Useful for objects such as scripts that write data to stream using <see cref="CompactInt"/>
        /// ie: <see cref="CompactInt"/> size + data
        /// </summary>
        /// <param name="additionalSize">Size to add (has to be positive, won't be checked)</param>
        public void AddWithCompactIntLength(int additionalSize)
        {
            Debug.Assert(additionalSize >= 0);
            if (additionalSize <= 252) // CompactInt is 1 Byte long
            {
                Size += additionalSize + 1;
            }
            else if (additionalSize <= 0xffff) // CompactInt is 1 + 2 Bytes long
            {
                Size += additionalSize + 3;
            }
            else // additionalSize <= 0xffffffff -> CompactInt is 1 + 4 Bytes long
            {
                Size += additionalSize + 5;
            }
            // else additionalSize <= 0xffffffffffffffff will never happen since additionalSize is of type Int32
        }

        /// <summary>
        /// Adds the given item count by computing length of its corresponding <see cref="CompactInt"/>.
        /// <para/>Useful for objects such as witness that write data to stream using <see cref="CompactInt"/>
        /// ie: <see cref="CompactInt"/> count + data
        /// </summary>
        /// <param name="count">Item count (has to be positive, won't be checked)</param>
        public void AddCompactIntCount(int count)
        {
            Debug.Assert(count >= 0);
            if (count <= 252) // CompactInt is 1 Byte long
            {
                Size++;
            }
            else if (count <= 0xffff) // CompactInt is 1 + 2 Bytes long
            {
                Size += 3;
            }
            else // additionalSize <= 0xffffffff -> CompactInt is 1 + 4 Bytes long
            {
                Size += 5;
            }
            // else additionalSize <= 0xffffffffffffffff will never happen since additionalSize is of type Int32
        }

        /// <summary>
        /// Add 1 (size of <see cref="byte"/>) to this counter
        /// </summary>
        public void AddByte() => Size++;
        /// <summary>
        /// Add 2 (size of <see cref="short"/>) to this counter
        /// </summary>
        public void AddInt16() => Size += sizeof(short);
        /// <summary>
        /// Add 4 (size of <see cref="int"/>) to this counter
        /// </summary>
        public void AddInt32() => Size += sizeof(int);
        /// <summary>
        /// Add 8 (size of <see cref="long"/>) to this counter
        /// </summary>
        public void AddInt64() => Size += sizeof(long);
        /// <summary>
        /// Add 2 (size of <see cref="ushort"/>) to this counter
        /// </summary>
        public void AddUInt16() => Size += sizeof(ushort);
        /// <summary>
        /// Add 4 (size of <see cref="uint"/>) to this counter
        /// </summary>
        public void AddUInt32() => Size += sizeof(uint);
        /// <summary>
        /// Add 8 (size of <see cref="ulong"/>) to this counter
        /// </summary>
        public void AddUInt64() => Size += sizeof(ulong);
        /// <summary>
        /// Add 20 (size of 160-bit hash) to this counter
        /// </summary>
        public void AddHash160() => Size += 20;
        /// <summary>
        /// Add 32 (size of 256-bit hash) to this counter
        /// </summary>
        public void AddHash256() => Size += 32;
    }
}
