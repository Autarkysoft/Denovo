// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System.Collections.Generic;
using System.Net.Sockets;

namespace Autarkysoft.Bitcoin.P2PNetwork
{
    internal class BufferManager
    {
        public BufferManager(int totalBytes, int bytesPerSaea)
        {
            bufferBlock = new byte[totalBytes];
            currentIndex = 0;
            bufferBytesPerSaea = bytesPerSaea;
            freeIndexPool = new Stack<int>();
        }



        private readonly byte[] bufferBlock;
        private readonly Stack<int> freeIndexPool;
        private readonly int bufferBytesPerSaea;
        private int currentIndex;



        internal bool SetBuffer(SocketAsyncEventArgs args)
        {
            if (freeIndexPool.Count > 0)
            {
                args.SetBuffer(bufferBlock, freeIndexPool.Pop(), bufferBytesPerSaea);
            }
            else
            {
                if ((bufferBlock.Length - bufferBytesPerSaea) < currentIndex)
                {
                    return false;
                }
                args.SetBuffer(bufferBlock, currentIndex, bufferBytesPerSaea);
                currentIndex += bufferBytesPerSaea;
            }
            return true;
        }


        /// <summary>
        /// This is only used when destroying the SAEA object. Normally the socket is closed and SAEA is 
        /// put back in the pool.
        /// </summary>
        /// <param name="args"></param>
        internal void FreeBuffer(SocketAsyncEventArgs args)
        {
            freeIndexPool.Push(args.Offset);
            args.SetBuffer(null, 0, 0);
        }
    }
}
