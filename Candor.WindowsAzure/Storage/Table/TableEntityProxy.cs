using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Candor.WindowsAzure.Storage.Table
{
    /// <summary>
    /// A generic implementatin of TableEntity that can be used
    /// to persist and retrieve your normal business layer class.
    /// It removes the need to create your own data layer proxy classses.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TableEntityProxy<T> : TableEntity
        where T : class, new()
    {
        /// <summary>
        /// The default constructor.
        /// </summary>
        public TableEntityProxy()
        {
            Entity = new T();
        }
        /// <summary>
        /// Creates a new instance with a specific instance of the model class.
        /// </summary>
        /// <param name="entity"></param>
        public TableEntityProxy(T entity)
        {
            Entity = entity;
        }

        /// <summary>
        /// The entity wrapped by this proxy, retrieved from or persisted to storage.
        /// </summary>
        public T Entity { get; set; }

        /// <summary>
        /// Deserializes this <see cref="T:Microsoft.WindowsAzure.Storage.Table.TableEntity"/> instance using the specified <see cref="T:System.Collections.Generic.Dictionary`2"/> of property names to <see cref="T:Microsoft.WindowsAzure.Storage.Table.EntityProperty"/> data typed values. 
        /// </summary>
        /// <param name="properties">The map of string property names to <see cref="T:Microsoft.WindowsAzure.Storage.Table.EntityProperty"/> data values to deserialize and store in this table entity instance.</param><param name="operationContext">An <see cref="T:Microsoft.WindowsAzure.Storage.OperationContext"/> object used to track the execution of the operation.</param>
        public override void ReadEntity(IDictionary<string, EntityProperty> properties,
            OperationContext operationContext)
        {
            if (Entity == null)
                Entity = new T();
            var dte = Entity as DynamicTableEntity;
            if (dte != null)
            {
                dte.Properties = properties;
                return;
            }
            foreach (var propertyInfo in typeof(T).GetProperties())
            {
                ReadEntityProperty(properties, operationContext, Entity, propertyInfo, null);
            }
        }
        /// <summary>
        /// During deserialization of the Entity, this reads one of the properties with support for
        /// member expressions.
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="operationContext"></param>
        /// <param name="propertyObject"></param>
        /// <param name="propertyInfo"></param>
        /// <param name="expressionChain"></param>
        /// <exception cref="InvalidCastException"></exception>
        protected void ReadEntityProperty(IDictionary<string, EntityProperty> properties,
            OperationContext operationContext, object propertyObject,
            PropertyInfo propertyInfo, String expressionChain)
        {
            if ((propertyInfo.GetSetMethod() == null)
                || !propertyInfo.GetSetMethod().IsPublic
                || (propertyInfo.GetGetMethod() == null)
                || !propertyInfo.GetGetMethod().IsPublic)
                return;
            var indexParams = propertyInfo.GetIndexParameters();
            if (indexParams.Length > 0)
                return;

            var memberExpression = String.IsNullOrEmpty(expressionChain)
                                       ? propertyInfo.Name
                                       : string.Format("{0}.{1}", expressionChain, propertyInfo.Name);
            var columnName = memberExpression.Replace(".", "");
            var propertyValue = propertyInfo.GetValue(propertyObject, null);
            //TODO: allow override of this convention via mappings in a static mappings dictionary class?

            if (propertyInfo.PropertyType.IsClass && propertyInfo.PropertyType != typeof(String))
            {
                if (propertyValue == null)
                {
                    var constructor = propertyInfo.PropertyType.GetConstructor(Type.EmptyTypes);
                    if (constructor == null)
                        return;
                    propertyValue = Activator.CreateInstance(propertyInfo.PropertyType, null, null);
                    propertyInfo.SetValue(propertyObject, propertyValue, null);
                }
                if (!properties.Keys.Any(p => p.StartsWith(columnName, StringComparison.InvariantCultureIgnoreCase)))
                    return; //don't recurse if none of the nested properties have values in the record.
                foreach (var subPropertyInfo in propertyInfo.PropertyType.GetProperties())
                {
                    ReadEntityProperty(properties, operationContext, propertyValue, subPropertyInfo, memberExpression);
                }
                return;
            }

            if (!properties.ContainsKey(columnName))
                return;

            var entityProperty = properties[columnName];
            switch (entityProperty.PropertyType)
            {
                case EdmType.String: //includes if exists: 'PartitionKey', 'RowKey', 'ETag'
                    if (propertyInfo.PropertyType == typeof(string))
                        propertyInfo.SetValue(propertyObject, entityProperty.StringValue, null);
                    else
                    {
                        var typeConverter = TypeDescriptor.GetConverter(propertyInfo.PropertyType);
                        if (typeConverter.CanConvertFrom(typeof(string)))
                            propertyInfo.SetValue(propertyObject, typeConverter.ConvertFrom(entityProperty.StringValue));
                    }
                    return;
                case EdmType.Binary:
                    if (propertyInfo.PropertyType == typeof(byte[]))
                        propertyInfo.SetValue(propertyObject, entityProperty.BinaryValue, null);
                    return;
                case EdmType.Boolean:
                    if (propertyInfo.PropertyType == typeof(bool))
                        propertyInfo.SetValue(propertyObject, entityProperty.BooleanValue.HasValue ? entityProperty.BooleanValue.Value : default(bool), null);
                    else if (propertyInfo.PropertyType == typeof(bool?))
                        propertyInfo.SetValue(propertyObject, entityProperty.BooleanValue, null);
                    return;
                case EdmType.DateTime: //Includes if exists: 'Timestamp'
                    if (propertyInfo.PropertyType == typeof(DateTime))
                        propertyInfo.SetValue(propertyObject, entityProperty.DateTimeOffsetValue.HasValue ? entityProperty.DateTimeOffsetValue.Value.UtcDateTime : new DateTime(), null);
                    else if (propertyInfo.PropertyType == typeof(DateTime?))
                        propertyInfo.SetValue(propertyObject, (entityProperty.DateTimeOffsetValue.HasValue ? new DateTime?(entityProperty.DateTimeOffsetValue.Value.UtcDateTime) : new DateTime?()), null);
                    else if (propertyInfo.PropertyType == typeof(DateTimeOffset))
                        propertyInfo.SetValue(propertyObject, entityProperty.DateTimeOffsetValue.HasValue ? entityProperty.DateTimeOffsetValue.Value : new DateTimeOffset(), null);
                    else if (propertyInfo.PropertyType == typeof(DateTimeOffset?))
                        propertyInfo.SetValue(propertyObject, entityProperty.DateTimeOffsetValue, null);
                    return;
                case EdmType.Double:
                    if (propertyInfo.PropertyType == typeof(double))
                        propertyInfo.SetValue(propertyObject, entityProperty.DoubleValue.HasValue ? entityProperty.DoubleValue.Value : default(double), null);
                    else if (propertyInfo.PropertyType == typeof(double?))
                        propertyInfo.SetValue(propertyObject, entityProperty.DoubleValue, null);
                    return;
                case EdmType.Guid:
                    if (propertyInfo.PropertyType == typeof(Guid))
                        propertyInfo.SetValue(propertyObject, entityProperty.GuidValue.HasValue ? entityProperty.GuidValue.Value : Guid.Empty, null);
                    else if (propertyInfo.PropertyType == typeof(Guid?))
                        propertyInfo.SetValue(propertyObject, entityProperty.GuidValue, null);
                    return;
                case EdmType.Int32:
                    if (propertyInfo.PropertyType == typeof(Int32))
                        propertyInfo.SetValue(propertyObject, entityProperty.Int32Value.HasValue ? entityProperty.Int32Value.Value : default(Int32), null);
                    else if (propertyInfo.PropertyType == typeof(Int32?))
                        propertyInfo.SetValue(propertyObject, entityProperty.Int32Value, null);
                    return;
                case EdmType.Int64:
                    if (propertyInfo.PropertyType == typeof(Int64))
                        propertyInfo.SetValue(propertyObject, entityProperty.Int64Value.HasValue ? entityProperty.Int64Value.Value : default(Int64), null);
                    else if (propertyInfo.PropertyType == typeof(Int64?))
                        propertyInfo.SetValue(propertyObject, entityProperty.Int64Value, null);
                    return;
                default:
                    throw new InvalidCastException(String.Format("Cannot cast Azure table value into property '{0}'.", propertyInfo.Name));
            }
        }

        /// <summary>
        /// Serializes the <see cref="T:System.Collections.Generic.Dictionary`2"/> of property names mapped to <see cref="T:Microsoft.WindowsAzure.Storage.Table.EntityProperty"/> data values from this <see cref="T:Microsoft.WindowsAzure.Storage.Table.TableEntity"/> instance.
        /// </summary>
        /// <param name="operationContext">An <see cref="T:Microsoft.WindowsAzure.Storage.OperationContext"/> object used to track the execution of the operation.</param>
        /// <returns>
        /// A map of property names to <see cref="T:Microsoft.WindowsAzure.Storage.Table.EntityProperty"/> data typed values created by serializing this table entity instance.
        /// </returns>
        public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            var dte = Entity as DynamicTableEntity;
            if (dte != null)
            {
                return new Dictionary<string, EntityProperty>(dte.Properties);
            }

            var properties = new Dictionary<string, EntityProperty>();
            foreach (var propertyInfo in typeof(T).GetProperties())
            {
                WriteEntityProperty(properties, operationContext, Entity, propertyInfo, null);
            }
            return properties;
        }
        /// <summary>
        /// During serialization of the Entity, this gets one of the properties and adds it to the properties collection.
        /// This will support deserializing an entire object graph below the specified property.
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="operationContext"></param>
        /// <param name="propertyObject"></param>
        /// <param name="propertyInfo"></param>
        /// <param name="expressionChain"></param>
        /// <exception cref="InvalidOperationException"></exception>
        protected void WriteEntityProperty(IDictionary<string, EntityProperty> properties,
            OperationContext operationContext, object propertyObject,
            PropertyInfo propertyInfo, String expressionChain)
        {
            if ((propertyInfo.GetSetMethod() == null)
                || !propertyInfo.GetSetMethod().IsPublic
                || (propertyInfo.GetGetMethod() == null)
                || !propertyInfo.GetGetMethod().IsPublic)
                return;
            var indexParams = propertyInfo.GetIndexParameters();
            if (indexParams.Length > 0)
                return;

            if (propertyInfo.Name == TableConstants.PartitionKey)
            {
                PartitionKey = (String)propertyInfo.GetValue(Entity, null);
                return;
            }
            if (propertyInfo.Name == TableConstants.RowKey)
            {
                RowKey = (String)propertyInfo.GetValue(Entity, null);
                return;
            }
            //if (propertyInfo.Name == TableConstants.Timestamp)
            //{   //Not used by caller in making REST call to Azure
            //    Timestamp = (DateTimeOffset)propertyInfo.GetValue(Entity, null);
            //    return;
            //}
            if (propertyInfo.Name == TableConstants.ETag)
            {
                ETag = (String)propertyInfo.GetValue(Entity, null);
                return;
            }

            var memberExpression = String.IsNullOrEmpty(expressionChain)
                                       ? propertyInfo.Name
                                       : string.Format("{0}.{1}", expressionChain, propertyInfo.Name);
            var columnName = memberExpression.Replace(".", "");
            var propertyValue = propertyInfo.GetValue(propertyObject, null);

            if (propertyInfo.PropertyType.IsClass && propertyInfo.PropertyType != typeof(String))
            {   //TODO: update to work if type is an interface also, and only write member values that are part of interface?
                if (propertyValue == null)
                    return;
                foreach (var subPropertyInfo in propertyInfo.PropertyType.GetProperties())
                {
                    WriteEntityProperty(properties, operationContext, propertyValue, subPropertyInfo,
                                        memberExpression);
                }
                return;
            }

            var propertyFromObject = CreateEntityPropertyFromObject(propertyInfo.PropertyType, propertyValue);
            if (propertyFromObject != null)
            {
                if (properties.ContainsKey(columnName))
                {
                    throw new InvalidOperationException(
                        String.Format(
                            "Property '{1}' of type '{2}' serializes to the same column name '{0}' as another property.  Change one of your property names, or set one of the conflicting property values to null to prevent it from serializing.",
                            columnName, memberExpression, typeof(T).FullName));
                }
                properties.Add(columnName, propertyFromObject);
            }
        }
        /// <summary>
        /// During serialization this will take a single value and create an EntityProperty.
        /// This supports more types than the base TableEntity class supports.
        /// This also supports any type that has a TypeDescriptor that can convert to and from a string.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static EntityProperty CreateEntityPropertyFromObject(Type type, object value)
        {
            if (type == typeof(string))
                return new EntityProperty((string)value);
            if (type == typeof(byte[]))
                return new EntityProperty((byte[])value);
            if (type == typeof(bool))
                return new EntityProperty((bool)value);
            if (type == typeof(bool?))
                return new EntityProperty((bool?)value);
            if (type == typeof (DateTime))
                return ((DateTime) value) < TableConstants.MinSupportedDateTime
                           ? new EntityProperty((DateTime?) null)
                           : new EntityProperty((DateTime) value);
            if (type == typeof(DateTime?))
                return new EntityProperty((DateTime?)value);
            if (type == typeof(DateTimeOffset))
                return new EntityProperty((DateTimeOffset)value);
            if (type == typeof(DateTimeOffset?))
                return new EntityProperty((DateTimeOffset?)value);
            if (type == typeof(double))
                return new EntityProperty((double)value);
            if (type == typeof(double?))
                return new EntityProperty((double?)value);
            if (type == typeof(Guid))
                return new EntityProperty((Guid)value);
            if (type == typeof(Guid?))
                return new EntityProperty((Guid?)value);
            if (type == typeof(int))
                return new EntityProperty((int)value);
            if (type == typeof(int?))
                return new EntityProperty((int?)value);
            if (type == typeof(long))
                return new EntityProperty((long)value);
            if (type == typeof(long?))
                return new EntityProperty((long?)value);

            var typeConverter = TypeDescriptor.GetConverter(type);
            if (typeConverter.CanConvertTo(typeof(string)) && typeConverter.CanConvertFrom(typeof(string)))
                return new EntityProperty((string)typeConverter.ConvertTo(value, typeof(string)));

            throw new InvalidCastException(String.Format("Cannot cast property '{0}' into an EntityProperty to be persisted into Azure. Implement a TypeConverter for conversion to and from a String.", type.FullName));
        }
    }
}
