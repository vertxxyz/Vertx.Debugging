#if !UNITY_2021_1_OR_NEWER
using System;
using System.Collections.Generic;

namespace Vertx.Debugging.Internal
{
	// These classes are a direct copy of UnityEngine.Pool...
	// Only to be used as a fallback when these classes aren't present.

	internal interface IObjectPool<T> where T : class
	{
		int CountInactive { get; }

		T Get();
		PooledObject<T> Get(out T v);
		void Release(T element);
		void Clear();
	}
	
	/// <summary>
	/// A Pooled object wraps a reference to an instance that will be returned to the pool when the Pooled object is disposed.
	/// The purpose is to automate the return of references so that they do not need to be returned manually.
	/// A PooledObject can be used like so:
	/// <code>
	/// MyClass myInstance;
	/// using(myPool.Get(out myInstance)) // When leaving the scope myInstance will be returned to the pool.
	/// {
	///     // Do something with myInstance
	/// }
	/// </code>
	/// </summary>
	internal struct PooledObject<T> : IDisposable where T : class
	{
		readonly T m_ToReturn;
		readonly IObjectPool<T> m_Pool;

		internal PooledObject(T value, IObjectPool<T> pool)
		{
			m_ToReturn = value;
			m_Pool = pool;
		}

		void IDisposable.Dispose() => m_Pool.Release(m_ToReturn);
	}
	
	/// <summary>
    /// Generic object pool implementation.
    /// </summary>
    /// <typeparam name="T">Type of the object pool.</typeparam>
    internal class ObjectPool<T> : IDisposable, IObjectPool<T> where T : class
    {
        internal readonly Stack<T> m_Stack;
        readonly Func<T> m_CreateFunc;
        readonly Action<T> m_ActionOnGet;
        readonly Action<T> m_ActionOnRelease;
        readonly Action<T> m_ActionOnDestroy;
        readonly int m_MaxSize; // Used to prevent catastrophic memory retention.
        internal bool m_CollectionCheck;

        /// <summary>
        /// The total number of active and inactive objects.
        /// </summary>
        public int CountAll { get; private set; }

        /// <summary>
        /// Number of objects that have been created by the pool but are currently in use and have not yet been returned.
        /// </summary>
        public int CountActive { get { return CountAll - CountInactive; } }

        /// <summary>
        /// Number of objects that are currently available in the pool.
        /// </summary>
        public int CountInactive { get { return m_Stack.Count; } }

        /// <summary>
        /// Creates a new ObjectPool.
        /// </summary>
        /// <param name="createFunc">Use to create a new instance when the pool is empty. In most cases this will just be <code>() => new T()</code></param>
        /// <param name="actionOnGet">Called when the instance is being taken from the pool.</param>
        /// <param name="actionOnRelease">Called when the instance is being returned to the pool. This could be used to clean up or disable the instance.</param>
        /// <param name="actionOnDestroy">Called when the element can not be returned to the pool due to it being equal to the maxSize.</param>
        /// <param name="collectionCheck">Collection checks are performed when an instance is returned back to the pool. An exception will be thrown if the instance is already in the pool. Collection checks are only performed in the Editor.</param>
        /// <param name="defaultCapacity">The default capacity the stack will be created with.</param>
        /// <param name="maxSize">The maximum size of the pool. When the pool reaches the max size then any further instances returned to the pool will be ignored and can be garbage collected. This can be used to prevent the pool growing to a very large size.</param>
        public ObjectPool(Func<T> createFunc, Action<T> actionOnGet = null, Action<T> actionOnRelease = null, Action<T> actionOnDestroy = null, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 10000)
        {
            if (createFunc == null)
                throw new ArgumentNullException(nameof(createFunc));

            if (maxSize <= 0)
                throw new ArgumentException("Max Size must be greater than 0", nameof(maxSize));

            m_Stack = new Stack<T>(defaultCapacity);
            m_CreateFunc = createFunc;
            m_MaxSize = maxSize;
            m_ActionOnGet = actionOnGet;
            m_ActionOnRelease = actionOnRelease;
            m_ActionOnDestroy = actionOnDestroy;
            m_CollectionCheck = collectionCheck;
        }

        /// <summary>
        /// Get an object from the pool.
        /// </summary>
        /// <returns>A new object from the pool.</returns>
        public T Get()
        {
            T element;
            if (m_Stack.Count == 0)
            {
                element = m_CreateFunc();
                CountAll++;
            }
            else
            {
                element = m_Stack.Pop();
            }
            m_ActionOnGet?.Invoke(element);
            return element;
        }

        /// <summary>
        /// Get a new <see cref="PooledObject"/> which can be used to return the instance back to the pool when the PooledObject is disposed.
        /// </summary>
        /// <param name="v">Output new typed object.</param>
        /// <returns>New PooledObject</returns>
        public PooledObject<T> Get(out T v) => new PooledObject<T>(v = Get(), this);

        /// <summary>
        /// Release an object to the pool.
        /// </summary>
        /// <param name="element">Object to release.</param>
        public void Release(T element)
        {
            if (m_CollectionCheck && m_Stack.Count > 0)
            {
                if (m_Stack.Contains(element))
                    throw new InvalidOperationException("Trying to release an object that has already been released to the pool.");
            }

            m_ActionOnRelease?.Invoke(element);

            if (CountInactive < m_MaxSize)
            {
                m_Stack.Push(element);
            }
            else
            {
                m_ActionOnDestroy?.Invoke(element);
            }
        }

        /// <summary>
        /// Releases all pooled objects so they can be garbage collected.
        /// </summary>
        public void Clear()
        {
            if (m_ActionOnDestroy != null)
            {
                foreach (var item in m_Stack)
                {
                    m_ActionOnDestroy(item);
                }
            }

            m_Stack.Clear();
            CountAll = 0;
        }

        public void Dispose()
        {
            // Ensure we do a clear so the destroy action can be called.
            Clear();
        }
    }
	
	internal class CollectionPool<TCollection, TItem> where TCollection : class, ICollection<TItem>, new()
	{
		internal static readonly ObjectPool<TCollection> s_Pool = new ObjectPool<TCollection>(() => new TCollection(), null, l => l.Clear());

		/// <summary>
		/// Get a new instance from the Pool.
		/// </summary>
		/// <returns></returns>
		public static TCollection Get() => s_Pool.Get();

		/// <summary>
		/// Get a new instance and a PooledObject. The PooledObject will automatically return the instance when it is Disposed.
		/// </summary>
		/// <param name="value">Output new instance.</param>
		/// <returns>A new PooledObject.</returns>
		public static PooledObject<TCollection> Get(out TCollection value) => s_Pool.Get(out value);

		/// <summary>
		/// Release an object to the pool.
		/// </summary>
		/// <param name="toRelease">instance to release.</param>
		public static void Release(TCollection toRelease) => s_Pool.Release(toRelease);
	}

	internal class ListPool<T> : CollectionPool<List<T>, T> {}
	internal class HashSetPool<T> : CollectionPool<HashSet<T>, T> {}
	internal class DictionaryPool<TKey, TValue> : CollectionPool<Dictionary<TKey, TValue>, KeyValuePair<TKey, TValue>> {}
}
#endif