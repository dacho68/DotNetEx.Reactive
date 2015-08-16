using System;
using System.Collections.Generic;
using System.Reflection;

namespace DotNetEx.Reactive
{
	[AttributeUsage( AttributeTargets.Property, AllowMultiple = false )]
	public sealed class ReferencesAttribute : Attribute
	{
		internal static IReadOnlyCollection<String> Get( Type targetType, String propertyName )
		{
			Check.NotNull( targetType, "targetType" );
			Check.NotNull( propertyName, "propertyName" );

			Dictionary<String, List<String>> map;
			List<String> references;

			lock ( s_types )
			{
				if ( !s_types.TryGetValue( targetType, out map ) )
				{
					map = new Dictionary<String, List<String>>();

					foreach ( var property in targetType.GetProperties( BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic ) )
					{
						var attribute = property.GetCustomAttribute<ReferencesAttribute>();

						if ( attribute != null )
						{
							foreach ( var name in attribute.PropertyNames )
							{
								List<String> dependencies = null;

								if ( !map.TryGetValue( name, out dependencies ) )
								{
									dependencies = new List<String>();

									map.Add( name, dependencies );
								}

								dependencies.Add( property.Name );
							}
						}
					}

					s_types[ targetType ] = map;
				}
			}

			if ( map.TryGetValue( propertyName, out references ) )
			{
				return references;
			}

			return EMPTY;
		}


		public ReferencesAttribute( params String[] propertyNames )
		{
			this.PropertyNames = new HashSet<String>( propertyNames );
		}


		public HashSet<String> PropertyNames { get; private set; }


		private static readonly Dictionary<Type, Dictionary<String, List<String>>> s_types = new Dictionary<Type, Dictionary<String, List<String>>>();
		private static readonly String[] EMPTY = new String[ 0 ];
	}
}
