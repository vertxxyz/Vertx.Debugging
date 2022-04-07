#if !UNITY_2022_1_OR_NEWER
using System.Collections.Generic;
using System;

namespace UnityEngine.Pool
{
	internal struct PoolableList<T> : IDisposable
	{
		private readonly List<T> _toReturn;
		private readonly Action<List<T>> _release;
		internal PoolableList(List<T> value, Action<List<T>> release)
		{
			_toReturn = value;
			_release = release;
		}

		void IDisposable.Dispose() => _release(_toReturn);
	}

	/// <summary>
	/// A simplified fallback for the default list pool.
	/// </summary>
	internal class ListPool<T>
	{
		private static readonly Stack<List<T>> _stack = new Stack<List<T>>();
		private static readonly Action<List<T>> _release = Release; 
		public static void Release(List<T> list) => _stack.Push(list);
		public static List<T> Get() => _stack.Pop();
		public static PoolableList<T> Get(out List<T> value) => new PoolableList<T>(value = Get(), _release);
	}
}
#endif