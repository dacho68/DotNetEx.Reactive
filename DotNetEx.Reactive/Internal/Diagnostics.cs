using System;
using System.Collections.Generic;

namespace DotNetEx.Reactive
{
	internal static class Check
	{
		/// <summary>
		/// Checks the provided arguments is not null.
		/// </summary>
		/// <param name="arg">The argument.</param>
		/// <param name="argName">Name of the argument.</param>
		/// <exception cref="System.ArgumentNullException">When the provided argument is null.</exception>
		public static void NotNull<T>( T arg, String argName ) where T : class
		{
			if ( Object.ReferenceEquals( arg, null ) )
			{
				throw new ArgumentNullException( argName );
			}
		}


		/// <summary>
		/// Checks the provided arguments is not null, empty or contains only white spaces.
		/// </summary>
		/// <param name="arg">The argument.</param>
		/// <param name="argName">Name of the argument.</param>
		/// <exception cref="System.ArgumentException">When the argument is null, empty or contains only white space.</exception>
		public static void NotEmpty( String arg, String argName )
		{
			if ( String.IsNullOrWhiteSpace( arg ) )
			{
				throw new ArgumentException( argName + " cannot be an empty string" );
			}
		}


		/// <summary>
		/// Checks the provided argument is not the default value. Performs null reference check for reference types
		/// and empty value for value types.
		/// </summary>
		/// <param name="arg">The argument.</param>
		/// <param name="argName">Name of the argument.</param>
		/// <exception cref="System.ArgumentException">When the argument is null for reference types and when the argument is default( T ) for value types.</exception>
		public static void NotDefault<T>( T arg, String argName )
		{
			if ( Object.Equals( arg, default( T ) ) )
			{
				if ( typeof( T ).IsValueType )
				{
					throw new ArgumentException( argName + " cannot be default or emtpy." );
				}

				throw new ArgumentException( argName + " cannot be null." );
			}
		}


		/// <summary>
		/// Checks the argument is in the specified range.
		/// </summary>
		/// <param name="arg">The argument.</param>
		/// <param name="argName">Name of the argument.</param>
		/// <param name="minValue">The minimum inclusive value.</param>
		/// <param name="maxValue">The maximum inclusive value.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">When the argument is out of range.</exception>
		public static void OutOfRange<T>( T arg, String argName, T minValue, T maxValue ) where T : IComparable<T>
		{
			var comparer = Comparer<T>.Default;

			if ( comparer.Compare( arg, minValue ) == -1 )
			{
				throw new ArgumentOutOfRangeException( argName + " cannot be less than " + minValue );
			}

			if ( comparer.Compare( arg, maxValue ) == 1 )
			{
				throw new ArgumentOutOfRangeException( argName + " cannot be greater than " + maxValue );
			}
		}


		/// <summary>
		/// Checks the provided arguments for equality.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <exception cref="System.ArgumentException">When the two arguments are not equal.</exception>
		public static void Equal<T>( T arg1, T arg2, String errorMessage ) where T : struct, IComparable<T>
		{
			if ( arg1.CompareTo( arg2 ) != 0 )
			{
				throw new ArgumentException( errorMessage );
			}
		}


		/// <summary>
		/// Checks the provided arguments point to the same object reference.
		/// </summary>
		/// <exception cref="System.ArgumentException">When the two arguments are not reference to the same object.</exception>
		public static void ReferenceEqual<T>( T arg1, T arg2, String errorMessage ) where T : class
		{
			if ( !Object.ReferenceEquals( arg1, arg2 ) )
			{
				throw new ArgumentException( errorMessage );
			}
		}


		/// <summary>
		/// Checks the specified condition and throws InvalidOperationException with the provided message when fails.
		/// </summary>
		/// <exception cref="System.NotImplementedException">When the condition is false.</exception>
		public static void Invariant( Boolean condition, String errorMessage )
		{
			if ( !condition )
			{
				throw new InvalidOperationException( errorMessage );
			}
		}
	}
}
