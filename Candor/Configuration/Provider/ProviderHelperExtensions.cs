using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Xml;

namespace Candor.Configuration.Provider
{
	public static class ProviderHelperExtensions
	{

		/// <summary>
		/// A helper method to get a boolean value from a configuration
		/// setting value.
		/// </summary>
		/// <param name="config">The available configuation values.</param>
		/// <param name="name">The name of the setting to retrieve.</param>
		/// <param name="defaultValue">The default in case the named
		/// value does not exist.</param>
		/// <returns>If the setting is not specified, then the default
		/// is returned.  If the setting can not be cast to a boolean
		/// then an exception is thrown.</returns>
		public static Boolean GetBooleanValue( this NameValueCollection config,
			string name, Boolean defaultValue )
		{
			if (config == null)
				return defaultValue;
			string val = config[name];
			if (val == null)
				return defaultValue;
			val = val.Trim();
			if (val.Length == 0)
				return defaultValue;

			try
			{
				return Boolean.Parse(val);
			}
			catch (Exception)
			{
				throw new ArgumentOutOfRangeException(name, val,
					string.Format("'{0}' must be a boolean value.", name));
			}
		}
		/// <summary>
		/// A helper method to get a boolean value from a configuration
		/// setting value.
		/// </summary>
		/// <param name="config">The available configuation values.</param>
		/// <param name="name">The name of the setting to retrieve.</param>
		/// <param name="defaultValue">The default in case the named
		/// value does not exist.</param>
		/// <returns>If the setting is not specified, then the default
		/// is returned.  If the setting can not be cast to a boolean
		/// then an exception is thrown.</returns>
		public static Boolean GetBooleanValue( this XmlAttributeCollection config,
			string name, Boolean defaultValue )
		{
			if (config == null)
				return defaultValue;
			XmlAttribute attr = config[name];
			if (attr == null)
				return defaultValue;

			string val = attr.Value;
			if (val == null)
				return defaultValue;
			val = val.Trim();
			if (val.Length == 0)
				return defaultValue;

			try
			{
				return Boolean.Parse(val);
			}
			catch (Exception)
			{
				throw new ArgumentOutOfRangeException(name, val,
					string.Format("'{0}' must be a boolean value.", name));
			}
		}
		/// <summary>
		/// A helper method to get a Int64 integer value from a configuration
		/// setting value.
		/// </summary>
		/// <param name="config">The available configuation values.</param>
		/// <param name="name">The name of the setting to retrieve.</param>
		/// <param name="defaultValue">The default in case the named
		/// value does not exist.</param>
		/// <returns>If the setting is not specified, then the default
		/// is returned.  If the setting can not be cast to a Int64 integer
		/// then an exception is thrown.</returns>
		public static Int64 GetInt64Value( this NameValueCollection config,
			string name, Int64 defaultValue )
		{
			if (config == null)
				return defaultValue;
			string val = config[name];
			if (val == null)
				return defaultValue;
			val = val.Trim();
			if (val.Length == 0)
				return defaultValue;

			try
			{
				return Int64.Parse(val);
			}
			catch (Exception)
			{
				throw new ArgumentOutOfRangeException(name, val,
					string.Format("'{0}' must be an integer value.", name));
			}
		}
		/// <summary>
		/// A helper method to get a Int64 integer value from a configuration
		/// setting value.
		/// </summary>
		/// <param name="config">The available configuation values.</param>
		/// <param name="name">The name of the setting to retrieve.</param>
		/// <param name="defaultValue">The default in case the named
		/// value does not exist.</param>
		/// <returns>If the setting is not specified, then the default
		/// is returned.  If the setting can not be cast to a Int64 integer
		/// then an exception is thrown.</returns>
		public static Int64 GetInt64Value( this XmlAttributeCollection config,
			string name, Int64 defaultValue )
		{
			if (config == null)
				return defaultValue;
			XmlAttribute attr = config[name];
			if (attr == null)
				return defaultValue;

			string val = attr.Value;
			if (val == null)
				return defaultValue;
			val = val.Trim();
			if (val.Length == 0)
				return defaultValue;

			try
			{
				return Int64.Parse(val);
			}
			catch (Exception)
			{
				throw new ArgumentOutOfRangeException(name, val,
					string.Format("'{0}' must be an integer value.", name));
			}
		}
		/// <summary>
		/// A helper method to get an integer value from a configuration
		/// setting value.
		/// </summary>
		/// <param name="config">The available configuation values.</param>
		/// <param name="name">The name of the setting to retrieve.</param>
		/// <param name="defaultValue">The default in case the named
		/// value does not exist.</param>
		/// <returns>If the setting is not specified, then the default
		/// is returned.  If the setting can not be cast to an integer
		/// then an exception is thrown.</returns>
		public static Int32 GetInt32Value( this NameValueCollection config,
			string name, Int32 defaultValue )
		{
			if (config == null)
				return defaultValue;
			string val = config[name];
			if (val == null)
				return defaultValue;
			val = val.Trim();
			if (val.Length == 0)
				return defaultValue;

			try
			{
				return Int32.Parse(val);
			}
			catch (Exception)
			{
				throw new ArgumentOutOfRangeException(name, val,
					string.Format("'{0}' must be an integer value.", name));
			}
		}
		/// <summary>
		/// A helper method to get an integer value from a configuration
		/// setting value.
		/// </summary>
		/// <param name="config">The available configuation values.</param>
		/// <param name="name">The name of the setting to retrieve.</param>
		/// <param name="defaultValue">The default in case the named
		/// value does not exist.</param>
		/// <returns>If the setting is not specified, then the default
		/// is returned.  If the setting can not be cast to an integer
		/// then an exception is thrown.</returns>
		public static Int32 GetInt32Value( this XmlAttributeCollection config,
			string name, Int32 defaultValue )
		{
			if (config == null)
				return defaultValue;
			XmlAttribute attr = config[name];
			if (attr == null)
				return defaultValue;

			string val = attr.Value;
			if (val == null)
				return defaultValue;
			val = val.Trim();
			if (val.Length == 0)
				return defaultValue;

			try
			{
				return Int32.Parse(val);
			}
			catch (Exception)
			{
				throw new ArgumentOutOfRangeException(name, val,
					string.Format("'{0}' must be an integer value.", name));
			}
		}
		/// <summary>
		/// A helper method to get an double value from a configuration
		/// setting value.
		/// </summary>
		/// <param name="config">The available configuation values.</param>
		/// <param name="name">The name of the setting to retrieve.</param>
		/// <param name="defaultValue">The default in case the named
		/// value does not exist.</param>
		/// <returns>If the setting is not specified, then the default
		/// is returned.  If the setting can not be cast to a double
		/// then an exception is thrown.</returns>
		public static double GetDoubleValue( this NameValueCollection config,
			string name, Double defaultValue )
		{
			if (config == null)
				return defaultValue;
			string val = config[name];
			if (val == null)
				return defaultValue;
			val = val.Trim();
			if (val.Length == 0)
				return defaultValue;

			try
			{
				return Double.Parse(val);
			}
			catch (Exception)
			{
				throw new ArgumentOutOfRangeException(name, val,
					string.Format("'{0}' must be a double value.", name));
			}
		}
		/// <summary>
		/// A helper method to get a double value from a configuration
		/// setting value.
		/// </summary>
		/// <param name="config">The available configuation values.</param>
		/// <param name="name">The name of the setting to retrieve.</param>
		/// <param name="defaultValue">The default in case the named
		/// value does not exist.</param>
		/// <returns>If the setting is not specified, then the default
		/// is returned.  If the setting can not be cast to a double
		/// then an exception is thrown.</returns>
		public static double GetDoubleValue( this XmlAttributeCollection config,
			string name, Double defaultValue )
		{
			if (config == null)
				return defaultValue;
			XmlAttribute attr = config[name];
			if (attr == null)
				return defaultValue;

			string val = attr.Value;
			if (val == null)
				return defaultValue;
			val = val.Trim();
			if (val.Length == 0)
				return defaultValue;

			try
			{
				return Double.Parse(val);
			}
			catch (Exception)
			{
				throw new ArgumentOutOfRangeException(name, val,
					string.Format("'{0}' must be a double value.", name));
			}
		}
		/// <summary>
		/// A helper method to get a string value from a configuration
		/// setting value.
		/// </summary>
		/// <param name="config">The available configuation values.</param>
		/// <param name="name">The name of the setting to retrieve.</param>
		/// <param name="defaultValue">The default in case the named
		/// value does not exist.</param>
		/// <returns>If the setting is not specified, then the default
		/// is returned.</returns>
		public static string GetStringValue( this NameValueCollection config,
			string name, String defaultValue )
		{
			if (config == null)
				return defaultValue;
			string val = config[name];
			if (val == null)
				return defaultValue;
			val = val.Trim();
			if (val.Length == 0)
				return defaultValue;
			return val;
		}
		/// <summary>
		/// A helper method to get a string value from a configuration
		/// setting value.
		/// </summary>
		/// <param name="config">The available configuation values.</param>
		/// <param name="name">The name of the setting to retrieve.</param>
		/// <param name="defaultValue">The default in case the named
		/// value does not exist.</param>
		/// <returns>If the setting is not specified, then the default
		/// is returned.</returns>
		public static string GetStringValue( this XmlAttributeCollection config,
			string name, String defaultValue )
		{
			if (config == null)
				return defaultValue;
			XmlAttribute attr = config[name];
			if (attr == null)
				return defaultValue;

			string val = attr.Value;
			if (val == null)
				return defaultValue;
			val = val.Trim();
			if (val.Length == 0)
				return defaultValue;
			return val;
		}
	}
}