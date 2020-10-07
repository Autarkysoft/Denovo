// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using Autarkysoft.Bitcoin.Cryptography.Hashing;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Autarkysoft.Bitcoin.Blockchain.Blocks
{
    /// <summary>
    /// A block miner that finds and sets correct <see cref="IBlock"/> properties so that the resulting block header hash
    /// is lower than the defined <see cref="IBlock.NBits"/> (ie. the target).
    /// </summary>
    public class Miner
    {
        /// <summary>
        /// Goes through all possible nonces to find the appropriate hash by changing <see cref="IBlock.Nonce"/> and
        /// <see cref="IBlock.BlockTime"/>. Return value indicates success.
        /// <para/>
        /// </summary>
        /// <param name="block">Block to mine (it has to be instantiated with all of its properties set to correct values)</param>
        /// <param name="token">
        /// Can be used to cancel the process 
        /// (eg. if the same block was found by someone else or block transactions have to change based on new ones in mempool)
        /// </param>
        /// <param name="maxDegreeOfParallelism">
        /// [Default value = 0] Sets the <see cref="ParallelOptions.MaxDegreeOfParallelism"/> only if the value is bigger than 0,
        /// otherwise the maximum will be used.
        /// <para/>0 -> max
        /// <para/>1 -> 1 core
        /// <para/>2 -> 2 cores
        /// </param>
        /// <returns>False if mining was canceled or failed to find anything; otherwise true.</returns>
        public async Task<bool> Mine(IBlock block, CancellationToken token, int maxDegreeOfParallelism = 0)
        {
            var options = new ParallelOptions()
            {
                CancellationToken = token
            };
            if (maxDegreeOfParallelism > 0)
            {
                options.MaxDegreeOfParallelism = maxDegreeOfParallelism;
            }

            try
            {
                // The for loop is using different time offsets in seconds to be added to the block time on each thread.
                // Each thread is compting uint.max (or ~4.3 billion) hashes and could take minutes to complete.
                await Task.Run(() => Parallel.For(0, 120, options, (i, state) => Mine(block, i, state, options)));
            }
            catch (OperationCanceledException)
            {
                return false;
            }

            return true;
        }

        private unsafe bool Mine(IBlock block, int timeOffset, ParallelLoopState state, ParallelOptions options)
        {
            /*** Target ***/
            uint[] targetArr = block.NBits.ToUInt32Array();

            // Compute SHA256(SHA256(80_byte_header))
            // Convert to integer in little endian order
            // Finish if hash <= target

            // Double SHA256 of 80 bytes is:
            //    Compress 64 bytes              -> block1, hashState1
            //    Compress 16 bytes + pad + len  -> block2, hashState2
            //    Compress 32 bytes + pad + len  -> block3, hashState3

            using Sha256 sha = new Sha256();

            uint* buffer = stackalloc uint[64 + 64 + 64 + 64 + 8 + 8]; // 1088 bytes total
            uint* wPt = buffer;
            uint* blkPt1 = buffer + 64;
            uint* blkPt2 = blkPt1 + 64;
            uint* blkPt3 = blkPt2 + 64;
            uint* hPt1 = blkPt3 + 64;
            uint* hPt3 = hPt1 + 8;

            fixed (uint* tarPt = &targetArr[0])
            fixed (byte* prvBlkH = &block.PreviousBlockHeaderHash[0], mrkl = &block.MerkleRootHash[0])
            {
                /*** First block (64 bytes) ***/
                // 4 byte block version
                blkPt1[0] = ((uint)block.Version).SwapEndian();

                // 32 byte previous block hash
                blkPt1[1] = (uint)(prvBlkH[0] << 24 | prvBlkH[1] << 16 | prvBlkH[2] << 8 | prvBlkH[3]);
                blkPt1[2] = (uint)(prvBlkH[4] << 24 | prvBlkH[5] << 16 | prvBlkH[6] << 8 | prvBlkH[7]);
                blkPt1[3] = (uint)(prvBlkH[8] << 24 | prvBlkH[9] << 16 | prvBlkH[10] << 8 | prvBlkH[11]);
                blkPt1[4] = (uint)(prvBlkH[12] << 24 | prvBlkH[13] << 16 | prvBlkH[14] << 8 | prvBlkH[15]);
                blkPt1[5] = (uint)(prvBlkH[16] << 24 | prvBlkH[17] << 16 | prvBlkH[18] << 8 | prvBlkH[19]);
                blkPt1[6] = (uint)(prvBlkH[20] << 24 | prvBlkH[21] << 16 | prvBlkH[22] << 8 | prvBlkH[23]);
                blkPt1[7] = (uint)(prvBlkH[24] << 24 | prvBlkH[25] << 16 | prvBlkH[26] << 8 | prvBlkH[27]);
                blkPt1[8] = (uint)(prvBlkH[28] << 24 | prvBlkH[29] << 16 | prvBlkH[30] << 8 | prvBlkH[31]);

                // 28/32 byte MerkleRoot
                blkPt1[9] = (uint)(mrkl[0] << 24 | mrkl[1] << 16 | mrkl[2] << 8 | mrkl[3]);
                blkPt1[10] = (uint)(mrkl[4] << 24 | mrkl[5] << 16 | mrkl[6] << 8 | mrkl[7]);
                blkPt1[11] = (uint)(mrkl[8] << 24 | mrkl[9] << 16 | mrkl[10] << 8 | mrkl[11]);
                blkPt1[12] = (uint)(mrkl[12] << 24 | mrkl[13] << 16 | mrkl[14] << 8 | mrkl[15]);
                blkPt1[13] = (uint)(mrkl[16] << 24 | mrkl[17] << 16 | mrkl[18] << 8 | mrkl[19]);
                blkPt1[14] = (uint)(mrkl[20] << 24 | mrkl[21] << 16 | mrkl[22] << 8 | mrkl[23]);
                blkPt1[15] = (uint)(mrkl[24] << 24 | mrkl[25] << 16 | mrkl[26] << 8 | mrkl[27]);


                // Compress first block (the result should be reused for all nonces)
                sha.Init(hPt1);
                for (int i = 16; i < 64; i++)
                {
                    blkPt1[i] = SSIG1(blkPt1[i - 2]) + blkPt1[i - 7] + SSIG0(blkPt1[i - 15]) + blkPt1[i - 16];
                }
                sha.CompressBlock_WithWSet(hPt1, blkPt1);

                /*** Second block (16 bytes) ***/
                // (final 4 bytes) 32/32 byte MerkleRoot
                blkPt2[0] = (uint)(mrkl[28] << 24 | mrkl[29] << 16 | mrkl[30] << 8 | mrkl[31]);


                // 4 byte BlockTime (index at 1)
                // will be incremented inside the block time loop
                // BlockTime property should not change here since the same instance is being accessed from different threads
                blkPt2[1] = (block.BlockTime + (uint)timeOffset).SwapEndian();

                // 4 byte NBits
                blkPt2[2] = ((uint)block.NBits).SwapEndian();

                // 4 byte Nonce (index at 3)
                // will be set and incremented inside the nonce loop

                // append length and padding:
                blkPt2[4] = 0b10000000_00000000_00000000_00000000U;
                // from 5 to 14 are zeros and are already set
                blkPt2[15] = 640; // 80*8=640

                // Second block values set up to here won't change in the loop

                /*** Third block, second hash (32 bytes) ***/
                // Set values that don't change
                blkPt3[8] = 0b10000000_00000000_00000000_00000000U;
                // From 9 to 14 are zero, already set and won't change
                blkPt3[15] = 256; // 32*8=256

                // Nonce loop
                for (ulong nonce = block.Nonce.SwapEndian(); nonce <= uint.MaxValue; nonce++)
                {
                    if (state.IsStopped || options.CancellationToken.IsCancellationRequested)
                    {
                        return false;
                    }

                    blkPt2[3] = (uint)nonce;

                    blkPt2[16] = SSIG0(blkPt2[1]) + blkPt2[0];
                    blkPt2[17] = 17825792 + SSIG0(blkPt2[2]) + blkPt2[1];
                    blkPt2[18] = SSIG1(blkPt2[16]) + SSIG0(blkPt2[3]) + blkPt2[2];
                    blkPt2[19] = SSIG1(blkPt2[17]) + 285220864 + blkPt2[3];
                    blkPt2[20] = SSIG1(blkPt2[18]) + 2147483648;
                    blkPt2[21] = SSIG1(blkPt2[19]);
                    blkPt2[22] = SSIG1(blkPt2[20]) + 640;
                    blkPt2[23] = SSIG1(blkPt2[21]) + blkPt2[16];
                    blkPt2[24] = SSIG1(blkPt2[22]) + blkPt2[17];
                    blkPt2[25] = SSIG1(blkPt2[23]) + blkPt2[18];
                    blkPt2[26] = SSIG1(blkPt2[24]) + blkPt2[19];
                    blkPt2[27] = SSIG1(blkPt2[25]) + blkPt2[20];
                    blkPt2[28] = SSIG1(blkPt2[26]) + blkPt2[21];
                    blkPt2[29] = SSIG1(blkPt2[27]) + blkPt2[22];
                    blkPt2[30] = SSIG1(blkPt2[28]) + blkPt2[23] + 10485845;
                    blkPt2[31] = SSIG1(blkPt2[29]) + blkPt2[24] + SSIG0(blkPt2[16]) + 640;
                    blkPt2[32] = SSIG1(blkPt2[30]) + blkPt2[25] + SSIG0(blkPt2[17]) + blkPt2[16];
                    blkPt2[33] = SSIG1(blkPt2[31]) + blkPt2[26] + SSIG0(blkPt2[18]) + blkPt2[17];
                    blkPt2[34] = SSIG1(blkPt2[32]) + blkPt2[27] + SSIG0(blkPt2[19]) + blkPt2[18];
                    blkPt2[35] = SSIG1(blkPt2[33]) + blkPt2[28] + SSIG0(blkPt2[20]) + blkPt2[19];
                    blkPt2[36] = SSIG1(blkPt2[34]) + blkPt2[29] + SSIG0(blkPt2[21]) + blkPt2[20];
                    blkPt2[37] = SSIG1(blkPt2[35]) + blkPt2[30] + SSIG0(blkPt2[22]) + blkPt2[21];
                    blkPt2[38] = SSIG1(blkPt2[36]) + blkPt2[31] + SSIG0(blkPt2[23]) + blkPt2[22];
                    blkPt2[39] = SSIG1(blkPt2[37]) + blkPt2[32] + SSIG0(blkPt2[24]) + blkPt2[23];
                    blkPt2[40] = SSIG1(blkPt2[38]) + blkPt2[33] + SSIG0(blkPt2[25]) + blkPt2[24];
                    blkPt2[41] = SSIG1(blkPt2[39]) + blkPt2[34] + SSIG0(blkPt2[26]) + blkPt2[25];
                    blkPt2[42] = SSIG1(blkPt2[40]) + blkPt2[35] + SSIG0(blkPt2[27]) + blkPt2[26];
                    blkPt2[43] = SSIG1(blkPt2[41]) + blkPt2[36] + SSIG0(blkPt2[28]) + blkPt2[27];
                    blkPt2[44] = SSIG1(blkPt2[42]) + blkPt2[37] + SSIG0(blkPt2[29]) + blkPt2[28];
                    blkPt2[45] = SSIG1(blkPt2[43]) + blkPt2[38] + SSIG0(blkPt2[30]) + blkPt2[29];
                    blkPt2[46] = SSIG1(blkPt2[44]) + blkPt2[39] + SSIG0(blkPt2[31]) + blkPt2[30];
                    blkPt2[47] = SSIG1(blkPt2[45]) + blkPt2[40] + SSIG0(blkPt2[32]) + blkPt2[31];
                    blkPt2[48] = SSIG1(blkPt2[46]) + blkPt2[41] + SSIG0(blkPt2[33]) + blkPt2[32];
                    blkPt2[49] = SSIG1(blkPt2[47]) + blkPt2[42] + SSIG0(blkPt2[34]) + blkPt2[33];
                    blkPt2[50] = SSIG1(blkPt2[48]) + blkPt2[43] + SSIG0(blkPt2[35]) + blkPt2[34];
                    blkPt2[51] = SSIG1(blkPt2[49]) + blkPt2[44] + SSIG0(blkPt2[36]) + blkPt2[35];
                    blkPt2[52] = SSIG1(blkPt2[50]) + blkPt2[45] + SSIG0(blkPt2[37]) + blkPt2[36];
                    blkPt2[53] = SSIG1(blkPt2[51]) + blkPt2[46] + SSIG0(blkPt2[38]) + blkPt2[37];
                    blkPt2[54] = SSIG1(blkPt2[52]) + blkPt2[47] + SSIG0(blkPt2[39]) + blkPt2[38];
                    blkPt2[55] = SSIG1(blkPt2[53]) + blkPt2[48] + SSIG0(blkPt2[40]) + blkPt2[39];
                    blkPt2[56] = SSIG1(blkPt2[54]) + blkPt2[49] + SSIG0(blkPt2[41]) + blkPt2[40];
                    blkPt2[57] = SSIG1(blkPt2[55]) + blkPt2[50] + SSIG0(blkPt2[42]) + blkPt2[41];
                    blkPt2[58] = SSIG1(blkPt2[56]) + blkPt2[51] + SSIG0(blkPt2[43]) + blkPt2[42];
                    blkPt2[59] = SSIG1(blkPt2[57]) + blkPt2[52] + SSIG0(blkPt2[44]) + blkPt2[43];
                    blkPt2[60] = SSIG1(blkPt2[58]) + blkPt2[53] + SSIG0(blkPt2[45]) + blkPt2[44];
                    blkPt2[61] = SSIG1(blkPt2[59]) + blkPt2[54] + SSIG0(blkPt2[46]) + blkPt2[45];
                    blkPt2[62] = SSIG1(blkPt2[60]) + blkPt2[55] + SSIG0(blkPt2[47]) + blkPt2[46];
                    blkPt2[63] = SSIG1(blkPt2[61]) + blkPt2[56] + SSIG0(blkPt2[48]) + blkPt2[47];

                    // HashState is the first block's hashState but it should remain the same for all nonces.
                    // After compressing this block hashState should be used as the next "block".
                    // So copy first block's hashState in third block
                    Buffer.MemoryCopy(hPt1, blkPt3, 32, 32);

                    // Compress second block
                    sha.CompressBlock_WithWSet(blkPt3, blkPt2);

                    // Compress third block
                    sha.Init(hPt3);

                    blkPt3[16] = SSIG0(blkPt3[1]) + blkPt3[0];
                    blkPt3[17] = 10485760 + SSIG0(blkPt3[2]) + blkPt3[1];
                    blkPt3[18] = SSIG1(blkPt3[16]) + SSIG0(blkPt3[3]) + blkPt3[2];
                    blkPt3[19] = SSIG1(blkPt3[17]) + SSIG0(blkPt3[4]) + blkPt3[3];
                    blkPt3[20] = SSIG1(blkPt3[18]) + SSIG0(blkPt3[5]) + blkPt3[4];
                    blkPt3[21] = SSIG1(blkPt3[19]) + SSIG0(blkPt3[6]) + blkPt3[5];
                    blkPt3[22] = SSIG1(blkPt3[20]) + 256 + SSIG0(blkPt3[7]) + blkPt3[6];
                    blkPt3[23] = SSIG1(blkPt3[21]) + blkPt3[16] + 285220864 + blkPt3[7];
                    blkPt3[24] = SSIG1(blkPt3[22]) + blkPt3[17] + 2147483648;
                    blkPt3[25] = SSIG1(blkPt3[23]) + blkPt3[18];
                    blkPt3[26] = SSIG1(blkPt3[24]) + blkPt3[19];
                    blkPt3[27] = SSIG1(blkPt3[25]) + blkPt3[20];
                    blkPt3[28] = SSIG1(blkPt3[26]) + blkPt3[21];
                    blkPt3[29] = SSIG1(blkPt3[27]) + blkPt3[22];
                    blkPt3[30] = SSIG1(blkPt3[28]) + blkPt3[23] + 4194338;
                    blkPt3[31] = SSIG1(blkPt3[29]) + blkPt3[24] + SSIG0(blkPt3[16]) + 256;
                    blkPt3[32] = SSIG1(blkPt3[30]) + blkPt3[25] + SSIG0(blkPt3[17]) + blkPt3[16];
                    blkPt3[33] = SSIG1(blkPt3[31]) + blkPt3[26] + SSIG0(blkPt3[18]) + blkPt3[17];
                    blkPt3[34] = SSIG1(blkPt3[32]) + blkPt3[27] + SSIG0(blkPt3[19]) + blkPt3[18];
                    blkPt3[35] = SSIG1(blkPt3[33]) + blkPt3[28] + SSIG0(blkPt3[20]) + blkPt3[19];
                    blkPt3[36] = SSIG1(blkPt3[34]) + blkPt3[29] + SSIG0(blkPt3[21]) + blkPt3[20];
                    blkPt3[37] = SSIG1(blkPt3[35]) + blkPt3[30] + SSIG0(blkPt3[22]) + blkPt3[21];
                    blkPt3[38] = SSIG1(blkPt3[36]) + blkPt3[31] + SSIG0(blkPt3[23]) + blkPt3[22];
                    blkPt3[39] = SSIG1(blkPt3[37]) + blkPt3[32] + SSIG0(blkPt3[24]) + blkPt3[23];
                    blkPt3[40] = SSIG1(blkPt3[38]) + blkPt3[33] + SSIG0(blkPt3[25]) + blkPt3[24];
                    blkPt3[41] = SSIG1(blkPt3[39]) + blkPt3[34] + SSIG0(blkPt3[26]) + blkPt3[25];
                    blkPt3[42] = SSIG1(blkPt3[40]) + blkPt3[35] + SSIG0(blkPt3[27]) + blkPt3[26];
                    blkPt3[43] = SSIG1(blkPt3[41]) + blkPt3[36] + SSIG0(blkPt3[28]) + blkPt3[27];
                    blkPt3[44] = SSIG1(blkPt3[42]) + blkPt3[37] + SSIG0(blkPt3[29]) + blkPt3[28];
                    blkPt3[45] = SSIG1(blkPt3[43]) + blkPt3[38] + SSIG0(blkPt3[30]) + blkPt3[29];
                    blkPt3[46] = SSIG1(blkPt3[44]) + blkPt3[39] + SSIG0(blkPt3[31]) + blkPt3[30];
                    blkPt3[47] = SSIG1(blkPt3[45]) + blkPt3[40] + SSIG0(blkPt3[32]) + blkPt3[31];
                    blkPt3[48] = SSIG1(blkPt3[46]) + blkPt3[41] + SSIG0(blkPt3[33]) + blkPt3[32];
                    blkPt3[49] = SSIG1(blkPt3[47]) + blkPt3[42] + SSIG0(blkPt3[34]) + blkPt3[33];
                    blkPt3[50] = SSIG1(blkPt3[48]) + blkPt3[43] + SSIG0(blkPt3[35]) + blkPt3[34];
                    blkPt3[51] = SSIG1(blkPt3[49]) + blkPt3[44] + SSIG0(blkPt3[36]) + blkPt3[35];
                    blkPt3[52] = SSIG1(blkPt3[50]) + blkPt3[45] + SSIG0(blkPt3[37]) + blkPt3[36];
                    blkPt3[53] = SSIG1(blkPt3[51]) + blkPt3[46] + SSIG0(blkPt3[38]) + blkPt3[37];
                    blkPt3[54] = SSIG1(blkPt3[52]) + blkPt3[47] + SSIG0(blkPt3[39]) + blkPt3[38];
                    blkPt3[55] = SSIG1(blkPt3[53]) + blkPt3[48] + SSIG0(blkPt3[40]) + blkPt3[39];
                    blkPt3[56] = SSIG1(blkPt3[54]) + blkPt3[49] + SSIG0(blkPt3[41]) + blkPt3[40];
                    blkPt3[57] = SSIG1(blkPt3[55]) + blkPt3[50] + SSIG0(blkPt3[42]) + blkPt3[41];
                    blkPt3[58] = SSIG1(blkPt3[56]) + blkPt3[51] + SSIG0(blkPt3[43]) + blkPt3[42];
                    blkPt3[59] = SSIG1(blkPt3[57]) + blkPt3[52] + SSIG0(blkPt3[44]) + blkPt3[43];
                    blkPt3[60] = SSIG1(blkPt3[58]) + blkPt3[53] + SSIG0(blkPt3[45]) + blkPt3[44];
                    blkPt3[61] = SSIG1(blkPt3[59]) + blkPt3[54] + SSIG0(blkPt3[46]) + blkPt3[45];
                    blkPt3[62] = SSIG1(blkPt3[60]) + blkPt3[55] + SSIG0(blkPt3[47]) + blkPt3[46];
                    blkPt3[63] = SSIG1(blkPt3[61]) + blkPt3[56] + SSIG0(blkPt3[48]) + blkPt3[47];

                    sha.CompressBlock_WithWSet(hPt3, blkPt3);

                    // Check to see if the hash result is smaller than target
                    if (CompareTarget(hPt3, tarPt))
                    {
                        block.Nonce = ((uint)nonce).SwapEndian();
                        // Block time should also be set here since it is different for each thread
                        block.BlockTime += (uint)timeOffset;
                        state.Stop();
                        return true;
                    }
                }

                return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint SSIG0(uint x) => (x >> 7 | x << 25) ^ (x >> 18 | x << 14) ^ (x >> 3);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint SSIG1(uint x) => (x >> 17 | x << 15) ^ (x >> 19 | x << 13) ^ (x >> 10);

        private unsafe bool CompareTarget(uint* hash, uint* target)
        {
            for (int i = 0, j = 7; i < 8; i++, j--)
            {
                if (target[i] == 0 && hash[j] != 0)
                {
                    return false;
                }
                else if (target[i] == 0 && hash[j] == 0)
                {
                    continue;
                }
                else
                {
                    uint h = hash[j].SwapEndian();
                    if (h > target[i])
                    {
                        return false;
                    }
                    else if (h < target[i])
                    {
                        return true;
                    }
                    else if (i + 1 < 8 && target[i + 1] != 0) // && h == target[i]
                    {
                        h = hash[i - 1].SwapEndian();
                        if (h > target[i + 1])
                        {
                            return false;
                        }
                        else if (h < target[i + 1])
                        {
                            return true;
                        }
                        // Target will never have more than 2 set values in it (the rest are zero)
                    }
                    // else is skipped
                }
            }

            return false;
        }
    }
}
