using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Candor.WindowsAzure.Storage.Table
{
    public class TableEntityProxy<T> : TableEntity
        where T : class, new()
    {
        public TableEntityProxy()
        {
            Entity = new T();
        }
        public TableEntityProxy(T entity)
        {
            Entity = entity;
        }

        public T Entity { get; set; }

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
                    return;
            }
        }
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
                properties.Add(columnName, propertyFromObject);
        }

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

            return null;
        }
    }
}
