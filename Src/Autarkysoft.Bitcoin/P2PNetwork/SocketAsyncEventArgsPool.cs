// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System.Collections.Generic;
using System.Net.Sockets;

namespace Autarkysoft.Bitcoin.P2PNetwork
{
    internal sealed class SocketAsyncEventArgsPool
    {
        // initializes the object pool to the specified size.
        // "capacity" = Maximum number of SocketAsyncEventArgs objects
        internal SocketAsyncEventArgsPool(int capacity)
        {
            pool = new Stack<SocketAsyncEventArgs>(capacity);
        }



        private readonly Stack<SocketAsyncEventArgs> pool;



        internal int Count => pool.Count;


        internal SocketAsyncEventArgs Pop()
        {
            lock (pool)
            {
                return pool.Pop();
            }
        }


        internal void Push(SocketAsyncEventArgs item)
        {
            lock (pool)
            {
                pool.Push(item);
            }
        }
    }
}
