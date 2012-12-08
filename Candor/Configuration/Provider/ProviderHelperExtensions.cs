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
		public static bool GetBooleanValue( this NameValueCollection config,
			string name, bool defaultValue )
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
				return bool.Parse(val);
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
		public static bool GetBooleanValue( this XmlAttributeCollection config,
			string name, bool defaultValue )
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
				return bool.Parse(val);
			}
			catch (Exception)
			{
				throw new ArgumentOutOfRangeException(name, val,
					string.Format("'{0}' must be a boolean value.", name));
			}
		}
		/// <summary>
		/// A helper method to get a long integer value from a configuration
		/// setting value.
		/// </summary>
		/// <param name="config">The available configuation values.</param>
		/// <param name="name">The name of the setting to retrieve.</param>
		/// <param name="defaultValue">The default in case the named
		/// value does not exist.</param>
		/// <returns>If the setting is not specified, then the default
		/// is returned.  If the setting can not be cast to a long integer
		/// then an exception is thrown.</returns>
		public static long GetLongValue( this NameValueCollection config,
			string name, long defaultValue )
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
				return long.Parse(val);
			}
			catch (Exception)
			{
				throw new ArgumentOutOfRangeException(name, val,
					string.Format("'{0}' must be an integer value.", name));
			}
		}
		/// <summary>
		/// A helper method to get a long integer value from a configuration
		/// setting value.
		/// </summary>
		/// <param name="config">The available configuation values.</param>
		/// <param name="name">The name of the setting to retrieve.</param>
		/// <param name="defaultValue">The default in case the named
		/// value does not exist.</param>
		/// <returns>If the setting is not specified, then the default
		/// is returned.  If the setting can not be cast to a long integer
		/// then an exception is thrown.</returns>
		public static long GetLongValue( this XmlAttributeCollection config,
			string name, long defaultValue )
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
				return long.Parse(val);
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
		public static int GetIntValue( this NameValueCollection config,
			string name, int defaultValue )
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
				return int.Parse(val);
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
		public static int GetIntValue( this XmlAttributeCollection config,
			string name, int defaultValue )
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
				return int.Parse(val);
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
			string name, double defaultValue )
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
				return double.Parse(val);
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
			string name, double defaultValue )
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
				return double.Parse(val);
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
			string name, string defaultValue )
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
			string name, string defaultValue )
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