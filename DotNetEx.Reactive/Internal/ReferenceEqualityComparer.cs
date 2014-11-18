using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DotNetEx.Reactive
{
	internal sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T>
	{
		public static readonly ReferenceEqualityComparer<T> Instance = new ReferenceEqualityComparer<T>();


		private ReferenceEqualityComparer()
		{
		}


		public Boolean Equals( T x, T y )
		{
			return Object.ReferenceEquals( x, y );
		}


		public Int32 GetHashCode( T obj )
		{
			return RuntimeHelpers.GetHashCode( obj );
		}
	}
}