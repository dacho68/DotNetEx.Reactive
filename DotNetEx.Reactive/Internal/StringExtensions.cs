using System;

namespace DotNetEx.Internal
{
	[Flags]
	internal enum StringOptions
	{
		Letters		= 1 << 0,
		Numbers		= 1 << 1,
		Underscores	= 1 << 2,
		Dots		= 1 << 3,

		LettersAndNumbers = Letters | Numbers
	}


	internal static class StringExtensions
	{
		public static String Truncate( this String target, Int32 count, String suffix = null )
		{
			return ( target == null || ( target.Length <= count ) ) ? target : ( target.Substring( 0, count ) + suffix );
		}


		public static String ToString( this String source, StringOptions options )
		{
			if ( source == null )
			{
				return source;
			}

			return new String( Array.FindAll<Char>( source.ToCharArray(), x =>
			{
				Boolean valid = false;

				if ( !valid && ( ( options & StringOptions.Letters ) == StringOptions.Letters ) )
				{
					valid = Char.IsLetter( x );
				}

				if ( !valid && ( ( options & StringOptions.Numbers ) == StringOptions.Numbers ) )
				{
					valid = Char.IsNumber( x );
				}

				if ( !valid && ( ( options & StringOptions.Underscores ) == StringOptions.Underscores ) )
				{
					valid = x == '_';
				}

				if ( !valid && ( ( options & StringOptions.Dots ) == StringOptions.Dots ) )
				{
					valid = x == '.';
				}

				return valid;
			} ) );
		}
	}
}