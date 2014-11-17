using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using DotNetEx.Reactive.Internal;

namespace DotNetEx.Reactive
{
	[AttributeUsage( AttributeTargets.Property, AllowMultiple = false )]
	public sealed class DependsOnAttribute : Attribute
	{
		public static IReadOnlyDictionary<String, HashSet<String>> GetDependencies( Type targetType )
		{
			Check.NotNull( targetType, "targetType" );

			Dictionary<String, HashSet<String>> result;

			if ( !s_types.TryGetValue( targetType, out result ) )
			{
				result = new Dictionary<String, HashSet<String>>();

				foreach ( var property in targetType.GetProperties( BindingFlags.Public | BindingFlags.Instance ) )
				{
					var attribute = property.GetCustomAttribute<DependsOnAttribute>();

					if ( attribute != null )
					{
						foreach ( var propertyName in attribute.PropertyNames )
						{
							HashSet<String> dependencies = null;

							if ( !result.TryGetValue( propertyName, out dependencies ) )
							{
								dependencies = new HashSet<String>();

								result.Add( propertyName, dependencies );
							}

							dependencies.Add( property.Name );	
						}
					}
				}

				s_types[ targetType ] = result;
			}

			return result;
		}


		public DependsOnAttribute( params String[] propertyNames )
		{
			this.PropertyNames = propertyNames;
		}


		public String[] PropertyNames { get; private set; }


		private static readonly ConcurrentDictionary<Type, Dictionary<String, HashSet<String>>> s_types = new ConcurrentDictionary<Type, Dictionary<String, HashSet<String>>>();
	}
}
