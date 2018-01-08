using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using SyncObjX.Exceptions;

namespace SyncObjX.Configuration
{
    [DataContract]
    public class SupportedProperty
    {
        private string _name;

        private string _description;

        private SupportedPropertyType _fieldType;

        private int? _maxValue;

        private int? _minValue;

        private List<SupportedPropertyOption> _options;

        private Type _optionsEnumType;

        private bool _isRequired;

        private object _defaultValue;

        [DataMember]
        public string Name
        {
            get { return _name; }

            set
            {
                if (String.IsNullOrWhiteSpace(value))
                    throw new Exception("Supported property name is missing or empty.");
                else
                    _name = value;
            }
        }

        [DataMember]
        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        [DataMember]
        public SupportedPropertyType FieldType
        {
            get { return _fieldType; }
            set { _fieldType = value; }
        }

        [DataMember]
        public int? MaxValue
        {
            get { return _maxValue; }
            set { _maxValue = value; }
        }

        [DataMember]
        public int? MinValue
        {
            get { return _minValue; }
            set { _minValue = value; }
        }

        [DataMember]
        public List<SupportedPropertyOption> Options
        {
            get { return _options; }

            set
            {
                ThrowExceptionIfOptionCodesAreInvalid();

                _options = value; 
            }
        }

        /// <summary>
        /// Optional enumeration type that maps to the option code.
        /// </summary>
        //[DataMember]
        // TODO: Type won't deserialize
        public Type OptionsEnumType
        {
            get { return _optionsEnumType; }

            set
            {
                if (!value.IsEnum)
                    throw new Exception(string.Format("'{0}' is not of type enum.", value.Name));

                ThrowExceptionIfOptionCodesAreInvalid();

                _optionsEnumType = value;
            }
        }

        [DataMember]
        public bool IsRequired
        {
            get { return _isRequired; }
            set { _isRequired = value; }
        }

        [DataMember]
        public object DefaultValue
        {
            get { return _defaultValue; }
            set { _defaultValue = value; }
        }

        public SupportedProperty(string name, SupportedPropertyType fieldType, bool isRequired)
        {
            Name = name;

            FieldType = fieldType;

            IsRequired = isRequired;
        }

        private void ThrowExceptionIfOptionCodesAreInvalid()
        {
            if (Options != null && OptionsEnumType != null)
            {
                foreach (var option in Options)
                {
                    var enumIsValid = Enum.IsDefined(OptionsEnumType, Enum.Parse(OptionsEnumType, option.Code));

                    if (!enumIsValid)
                        throw new Exception(string.Format("Supported property option code '{0}' is not valid for enum of type '{1}'.", option.Code, OptionsEnumType.Name));
                }
            }
        }

        public static void ThrowExceptionIfInvalid(SupportedProperty property, object value)
        {
            if (property.IsRequired && value == null)
                throw new Exception(string.Format("Supported property '{0}' is required.", property.Name));

            switch(property.FieldType)
            {
                case SupportedPropertyType.Integer:

                    int propValueAsInt;
                    if (!int.TryParse(value.ToString(), out propValueAsInt))
                        throw new Exception(string.Format("Supported property '{0}' with value '{1}' could not be converted to an integer.", property.Name, value));

                    if (property.MinValue.HasValue && propValueAsInt < property.MinValue)
                        throw new Exception(string.Format("Supported property '{0}' has a minimum value of {1}.", property.Name, property.MinValue.Value));

                    if (property.MaxValue.HasValue && propValueAsInt > property.MaxValue)
                        throw new Exception(string.Format("Supported property '{0}' has a maximum value of {1}.", property.Name, property.MaxValue.Value));

                    break;

                case SupportedPropertyType.Text:
                    // do nothing
                    break;

                case SupportedPropertyType.Options:

                    if (property.OptionsEnumType == null)
                    {
                        if (property.Options.Where(d => (d == null ? null : d.ToString()) == (value == null ? null : value.ToString())).Count() == 0)
                            throw new Exception(string.Format("Supported property '{0}' does not support option '{1}'.", property.Name, value == null ? "null" : value));
                    }
                    else
                    {
                        if (value == null)
                            throw new Exception("Option values can not be null.");

                        var enumIsValid = Enum.IsDefined(property.OptionsEnumType, Enum.Parse(property.OptionsEnumType, value.ToString()));

                        if (!enumIsValid)
                            throw new Exception(string.Format("Supported property option value '{0}' is not valid for enum of type '{1}'.", value.ToString(), property.OptionsEnumType.Name));
                    }

                    break;

                case SupportedPropertyType.Boolean:

                    bool propValueAsBool;
                    if (!bool.TryParse(value.ToString(), out propValueAsBool))
                        throw new Exception(string.Format("Property '{0}' with value '{1}' could not be converted to a Boolean.", property.Name, value));

                    break;

                default:
                    throw new EnumValueNotImplementedException<SupportedPropertyType>(property.FieldType);
            }
        }
    }
}
