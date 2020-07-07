// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Autarkysoft.Bitcoin.P2PNetwork
{
    internal sealed class SocketAsyncEventArgsPool : IDisposable
    {
        // initializes the object pool to the specified size.
        // "capacity" = Maximum number of SocketAsyncEventArgs objects
        internal SocketAsyncEventArgsPool(int capacity)
        {
            pool = new Stack<SocketAsyncEventArgs>(capacity);
        }


        private Stack<SocketAsyncEventArgs> pool;
        private readonly object lockObj = new object();


        internal int Count => pool.Count;

        internal SocketAsyncEventArgs Pop()
        {
            lock (lockObj)
            {
                return pool.Pop();
            }
        }


        internal void Push(SocketAsyncEventArgs item)
        {
            lock (lockObj)
            {
                pool.Push(item);
            }
        }


        private bool isDisposed = false;

        /// <summary>
        /// Releases the resources used by this class.
        /// </summary>
        /// <param name="disposing">
        /// True to release both managed and unmanaged resources; false to release only unmanaged resources.
        /// </param>
        void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    if (!(pool is null))
                    {
                        foreach (var item in pool)
                        {
                            item.Dispose();
                        }
                    }

                    pool = null;
                }

                isDisposed = true;
            }
        }

        /// <summary>
        /// Releases all resources used by the current instance of this class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
    }
}
