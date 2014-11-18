using System;
using System.Collections.Generic;

namespace DotNetEx.Reactive.Annotations
{
	[AttributeUsage( AttributeTargets.Method, Inherited = false )]
	public sealed class OnPropertyChangedAttribute : Attribute
	{
		public OnPropertyChangedAttribute()
		{
		}


		public OnPropertyChangedAttribute( params String[] propertyNames )
		{
			if ( propertyNames != null && propertyNames.Length > 0 )
			{
				this.PropertyNames = new HashSet<String>( propertyNames );
			}
		}


		public HashSet<String> PropertyNames { get; private set; }
	}
}
