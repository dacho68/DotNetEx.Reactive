using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotNetEx.Reactive.Internal;

namespace DotNetEx.Reactive
{
	[AttributeUsage( AttributeTargets.Property, AllowMultiple = false )]
	public sealed class ReferencesAttribute : Attribute
	{
		internal static IEnumerable<String> Get( Type targetType, String propertyName )
		{
			Check.NotNull( targetType, "targetType" );
			Check.NotNull( propertyName, "propertyName" );

			Dictionary<String, HashSet<String>> map;
			HashSet<String> references;

			if ( !s_types.TryGetValue( targetType, out map ) )
			{
				map = new Dictionary<String, HashSet<String>>();

				foreach ( var property in targetType.GetProperties( BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic ) )
				{
					var attribute = property.GetCustomAttribute<ReferencesAttribute>();

					if ( attribute != null )
					{
						foreach ( var name in attribute.PropertyNames )
						{
							HashSet<String> dependencies = null;

							if ( !map.TryGetValue( name, out dependencies ) )
							{
								dependencies = new HashSet<String>();

								map.Add( name, dependencies );
							}

							dependencies.Add( property.Name );
						}
					}
				}

				s_types[ targetType ] = map;
			}

			if ( map.TryGetValue( propertyName, out references ) )
			{
				return references;
			}

			return Enumerable.Empty<String>();
		}


		public ReferencesAttribute( params String[] propertyNames )
		{
			this.PropertyNames = propertyNames;
		}


		public String[] PropertyNames { get; private set; }


		private static readonly ConcurrentDictionary<Type, Dictionary<String, HashSet<String>>> s_types = new ConcurrentDictionary<Type, Dictionary<String, HashSet<String>>>();
	}
}
