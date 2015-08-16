using System;

namespace DotNetEx.Reactive
{
	internal static class ReflectionTraits
	{
		public static class Assignable<TTo, TFrom>
		{
			public static readonly Boolean Value = typeof( TTo ).IsAssignableFrom( typeof( TFrom ) );
		}
	}
}
