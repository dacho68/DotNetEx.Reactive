using System;

namespace DotNetEx.Internal
{
	internal static class ReflectionTraits
	{
		public static class Assignable<TTo, TFrom>
		{
			public static readonly Boolean Value = typeof( TTo ).IsAssignableFrom( typeof( TFrom ) );
		}
	}
}
