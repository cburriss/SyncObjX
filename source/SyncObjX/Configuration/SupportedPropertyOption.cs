using System;
using System.Runtime.Serialization;

namespace SyncObjX.Configuration
{
    [DataContract]
    public class SupportedPropertyOption
    {
        private string _code;

        private string _textToDisplay;

        [DataMember]
        public string Code
        {
            get { return _code; }

            set 
            {
                if (String.IsNullOrWhiteSpace(value))
                    throw new Exception("Option code is missing or empty.");

                _code = value; 
            }
        }

        [DataMember]
        public string TextToDisplay
        {
            get { return _textToDisplay; }

            set 
            {
                if (String.IsNullOrWhiteSpace(value))
                    throw new Exception("Option text to display is missing or empty.");

                _textToDisplay = value; 
            }
        }

        public SupportedPropertyOption(string textToDisplay)
            : this(textToDisplay, textToDisplay) { }

        public SupportedPropertyOption(string code, string textToDisplay)
        {
            Code = code;

            TextToDisplay = textToDisplay;
        }

        public static T GetEnumValueForOption<T>(SupportedProperty property, string valueToMap)
        {
            var enumType = typeof(T);

            if (!enumType.IsEnum)
                throw new Exception(string.Format("T '{0}' is not of type enum.", enumType.Name));

            Object optionAsEnum;

            if (property.Options == null || property.Options.Count == 0)
                throw new Exception(string.Format("No options are defined for supported property '{0}'.", property.Name));

            if (typeof(T) != property.OptionsEnumType)
                throw new Exception(string.Format("Conversion type '{0}' does not match the supported property option enum type '{1}'.", enumType.Name, property.OptionsEnumType.Name));

            SupportedProperty.ThrowExceptionIfInvalid(property, valueToMap);

            try
            {
                optionAsEnum = Enum.Parse(property.OptionsEnumType, valueToMap == null ? null : valueToMap, true);

                return (T)optionAsEnum;
            }
            catch
            {
                throw new Exception(string.Format("Supported property '{0}' could not convert option '{1}' to enum type '{2}'.", property.Name, valueToMap == null ? "null" : valueToMap, property.OptionsEnumType.Name));
            }  
        }
    }
}
