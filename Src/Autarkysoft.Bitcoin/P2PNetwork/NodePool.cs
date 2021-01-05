// Autarkysoft.Bitcoin
// Copyright (c) 2020 Autarkysoft
// Distributed under the MIT software license, see the accompanying
// file LICENCE or http://www.opensource.org/licenses/mit-license.php.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace Autarkysoft.Bitcoin.P2PNetwork
{
    /// <summary>
    /// Thread safe observable collection of <see cref="Node"/>s.
    /// </summary>
    public class NodePool : IList<Node>, INotifyCollectionChanged, INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of <see cref="NodePool"/> with default capacity of 10 and
        /// current SynchronizationContext.
        /// </summary>
        public NodePool() : this(10)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="NodePool"/> with the specified initial capacity and
        /// current SynchronizationContext.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="capacity">Initial capacity to use</param>
        public NodePool(int capacity) : this(capacity, SynchronizationContext.Current)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="NodePool"/> using the given parameters.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <param name="capacity"></param>
        /// <param name="c"></param>
        public NodePool(int capacity, SynchronizationContext c)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be a positive number.");
            if (c is null)
                throw new ArgumentNullException(nameof(c));

            items = new Node[capacity];
            context = c;
        }


        private Node[] items;
        private int size, version;
        private readonly SynchronizationContext context;
        private readonly object lockObj = new object();
        private SemaphoreSlim monitor = new SemaphoreSlim(1, 1);

        /// <inheritdoc/>
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Occurs when one or more items are removed.
        /// </summary>
        public event EventHandler ItemRemovedEvent;


        private void CheckIndex(int index)
        {
            if (index < 0 || index >= items.Length)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        private void Resize()
        {
            if (size == items.Length)
            {
                Node[] temp = new Node[items.Length + 10];
                Array.Copy(items, 0, temp, 0, items.Length);
                items = temp;
            }
        }

        private void OnNotifyCollectionReset()
        {
            monitor.Wait();
            context.Send(state =>
            {
                ItemRemovedEvent?.Invoke(this, null);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }, null);
            monitor.Release();
        }

        private void OnNotifyItemAdded(Node item, int index)
        {
            monitor.Wait();
            context.Send(state =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
            }, null);
            monitor.Release();
        }

        private void OnNotifyItemRemoved(Node item, int index)
        {
            monitor.Wait();
            context.Send(state =>
            {
                ItemRemovedEvent?.Invoke(this, null);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
            }, null);
            monitor.Release();
        }

        private void OnNotifyItemReplaced(Node newItem, Node oldItem, int index)
        {
            monitor.Wait();
            context.Send(state =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem, index));
            }, null);
            monitor.Release();
        }


        /// <inheritdoc/>
        public Node this[int index]
        {
            get
            {
                lock (lockObj)
                {
                    CheckIndex(index);
                    return items[index];
                }
            }
            set
            {
                lock (lockObj)
                {
                    CheckIndex(index);
                    Node oldVal = items[index];
                    items[index] = value;
                    OnNotifyItemReplaced(value, oldVal, index);
                }
            }
        }

        /// <inheritdoc/>
        public int Count => size;

        /// <inheritdoc/>
        public bool IsReadOnly => false;


        /// <inheritdoc/>
        public void Add(Node item)
        {
            lock (lockObj)
            {
                Resize();
                items[size] = item;
                version++;
                item.NodeStatus.DisconnectEvent += NodeStatus_DisconnectEvent;
                Interlocked.Increment(ref size);
                OnNotifyItemAdded(item, size);
            }
        }

        private void NodeStatus_DisconnectEvent(object sender, EventArgs e)
        {
            if (sender is INodeStatus status)
            {
                int index = -1;
                lock (lockObj)
                {
                    for (int i = 0; i < size; i++)
                    {
                        if (ReferenceEquals(items[i].NodeStatus, status))
                        {
                            index = i;
                            break;
                        }
                    }
                }
                if (index != -1)
                {
                    RemoveAt(index);
                }
            }
        }

        /// <inheritdoc/>
        public void Clear()
        {
            lock (lockObj)
            {
                foreach (var item in items)
                {
                    item.Dispose();
                }
                Array.Clear(items, 0, items.Length);
                version++;
                OnNotifyCollectionReset();
            }
        }

        /// <inheritdoc/>
        public bool Contains(Node item)
        {
            lock (lockObj)
            {
                return items.Any(x => x.NodeStatus.IP.Equals(item.NodeStatus.IP));
            }
        }

        /// <inheritdoc/>
        public void CopyTo(Node[] array, int arrayIndex)
        {
            lock (lockObj)
            {
                Array.Copy(items, 0, array, arrayIndex, size);
            }
        }

        /// <inheritdoc/>
        public int IndexOf(Node item)
        {
            lock (lockObj)
            {
                // TODO: this will probably fail because Node doesn't override Equals!
                return Array.IndexOf(items, item);
            }
        }

        /// <inheritdoc/>
        public void Insert(int index, Node item)
        {
            lock (lockObj)
            {
                if (index < 0 || index > items.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                Resize();
                if (index < items.Length)
                {
                    Array.Copy(items, index, items, index + 1, size - index);
                }

                items[index] = item;
                version++;
                OnNotifyItemAdded(item, index);
            }
        }

        /// <inheritdoc/>
        public bool Remove(Node item)
        {
            lock (lockObj)
            {
                int index = IndexOf(item);
                if (index < 0)
                {
                    return false;
                }

                Node removedVal = items[index];
                Interlocked.Decrement(ref size);
                if (index < size)
                {
                    Array.Copy(items, index + 1, items, index, size - index);
                }
                items[size] = null;
                version++;
                OnNotifyItemRemoved(removedVal, index);
                removedVal.Dispose();
                removedVal = null;
                return true;
            }
        }

        /// <inheritdoc/>
        public void RemoveAt(int index)
        {
            lock (lockObj)
            {
                CheckIndex(index);

                Node removedVal = items[index];
                Interlocked.Decrement(ref size);
                if (index < size)
                {
                    Array.Copy(items, index + 1, items, index, size - index);
                }
                items[size] = null;
                version++;
                OnNotifyItemRemoved(removedVal, index);
                removedVal.Dispose();
                removedVal = null;
            }
        }

        /// <inheritdoc/>
        public IEnumerator<Node> GetEnumerator() => new Enumerator<Node>(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc/>
        public void Dispose()
        {
            lock (lockObj)
            {
                if (!(items is null))
                {
                    foreach (var item in items)
                    {
                        item.Dispose();
                    }
                    items = null;
                }
            }
            if (!(monitor is null))
            {
                monitor.Dispose();
                monitor = null;
            }
        }


#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public struct Enumerator<T> : IEnumerator<T>, IEnumerator where T : Node
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            internal Enumerator(NodePool pool)
            {
                list = pool;
                index = 0;
                version = pool.version;
                Current = default;
            }


            private readonly NodePool list;
            private int index;
            private readonly int version;

            /// <inheritdoc/>
            public bool MoveNext()
            {
                var localList = list;

                if (version == localList.version && ((uint)index < (uint)localList.size))
                {
                    Current = (T)localList.items[index];
                    index++;
                    return true;
                }
                return MoveNextRare();
            }

            private bool MoveNextRare()
            {
                if (version != list.version)
                {
                    throw new InvalidOperationException();
                }

                index = list.size + 1;
                Current = default;
                return false;
            }

            /// <inheritdoc/>
            public T Current { get; private set; }

            object IEnumerator.Current
            {
                get
                {
                    if (index == 0 || index == list.size + 1)
                    {
                        throw new InvalidOperationException();
                    }
                    return Current;
                }
            }

            void IEnumerator.Reset()
            {
                if (version != list.version)
                {
                    throw new InvalidOperationException();
                }

                index = 0;
                Current = default;
            }

            /// <inheritdoc/>
            public void Dispose()
            {
            }
        }
    }
}
