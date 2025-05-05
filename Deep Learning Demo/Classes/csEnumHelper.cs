using DevExpress.XtraEditors.Mask;
using DevExpress.XtraEditors.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

public static class csEnumHelper<EnumType>
    where EnumType : struct, Enum // This constraint requires C# 7.3 or later.
{
    public static IList<EnumType> GetValues(Enum value)
    {
        var enumValues = new List<EnumType>();

        foreach (FieldInfo fi in value.GetType().GetFields(BindingFlags.Static | BindingFlags.Public))
        {
            enumValues.Add((EnumType)Enum.Parse(value.GetType(), fi.Name, false));
        }
        return enumValues;
    }

    public static EnumType Parse(string value)
    {
        return (EnumType)Enum.Parse(typeof(EnumType), value, true);
    }

    public static IList<string> GetNames(Enum value)
    {
        return value.GetType().GetFields(BindingFlags.Static | BindingFlags.Public).Select(fi => fi.Name).ToList();
    }

    public static IList<string> GetDisplayValues(Enum value)
    {
        return GetNames(value).Select(obj => GetDisplayValue(Parse(obj))).ToList();
    }

    private static string lookupResource(Type resourceManagerProvider, string resourceKey)
    {
        var resourceKeyProperty = resourceManagerProvider.GetProperty(resourceKey,
            BindingFlags.Static | BindingFlags.Public, null, typeof(string),
            new Type[0], null);
        if (resourceKeyProperty != null)
        {
            return (string)resourceKeyProperty.GetMethod.Invoke(null, null);
        }

        return resourceKey; // Fallback with the key name
    }

    public static string GetDisplayValue(EnumType value)
    {
        try
        {
            var fieldInfo = value.GetType().GetField(value.ToString());

            var descriptionAttributes = fieldInfo.GetCustomAttributes(
                typeof(DisplayAttribute), false) as DisplayAttribute[];

            if (descriptionAttributes[0].ResourceType != null)
                return lookupResource(descriptionAttributes[0].ResourceType, descriptionAttributes[0].Name);

            if (descriptionAttributes == null) return string.Empty;
            return (descriptionAttributes.Length > 0) ? descriptionAttributes[0].Name : value.ToString();
        }
        catch (Exception ex)
        {
            Trace.WriteLine("EnumHelper.GetDisplayValue:\r\n" + ex.Message);
            return string.Empty;
        }
    }

    public static object GetDefaultValue(EnumType value)
    {
        try
        {
            var fieldInfo = value.GetType().GetField(value.ToString());

            var attributes = fieldInfo.GetCustomAttributes(
                typeof(DefaultValueAttribute), false) as DefaultValueAttribute[];

            if (attributes == null) return string.Empty;
            return (attributes.Length > 0) ? attributes[0].Value : value.ToString();
        }
        catch (Exception ex)
        {
            Trace.WriteLine("EnumHelper.GetDefaultValue:\r\n" + ex.Message);
            return string.Empty;
        }
    }

    public static string GetDisplayDescription(EnumType value)
    {
        try
        {
            var fieldInfo = value.GetType().GetField(value.ToString());

            var descriptionAttributes = fieldInfo.GetCustomAttributes(
                typeof(DisplayAttribute), false) as DisplayAttribute[];

            if (descriptionAttributes[0].ResourceType != null)
                return lookupResource(descriptionAttributes[0].ResourceType, descriptionAttributes[0].Name);

            if (descriptionAttributes == null) return string.Empty;
            return (descriptionAttributes.Length > 0) ? descriptionAttributes[0].Description : value.ToString();
        }
        catch (Exception ex)
        {
            Trace.WriteLine("EnumHelper.GetDisplayValue:\r\n" + ex.Message);
            return string.Empty;
        }
    }



    public static string GetDescription(EnumType value)
    {
        try
        {
            var fieldInfo = value.GetType().GetField(value.ToString());
            if (fieldInfo == null) return null;

            var descriptionAttributes = fieldInfo.GetCustomAttributes(
                typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            if (descriptionAttributes == null) return string.Empty;
            return (descriptionAttributes.Length > 0) ? descriptionAttributes[0].Description : value.ToString();
        }
        catch (Exception ex)
        {
            Trace.WriteLine("EnumHelper.GetDescription:\r\n" + ex.Message);
            return string.Empty;
        }
    }

    public static string GetXmlEnum(EnumType value)
    {
        try
        {
            var fieldInfo = value.GetType().GetField(value.ToString());
            if (fieldInfo == null) return null;

            var xmlAttributes = fieldInfo.GetCustomAttributes(
                typeof(XmlEnumAttribute), false) as XmlEnumAttribute[];

            if (xmlAttributes == null) return string.Empty;
            return (xmlAttributes.Length > 0) ? xmlAttributes[0].Name : value.ToString();
        }
        catch (Exception ex)
        {
            Trace.WriteLine("EnumHelper.GetXmlEnum:\r\n" + ex.Message);
            return string.Empty;
        }
    }

    /// <summary>
    /// Get description from custom attribute
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string GetCustomDescription(EnumType value)
    {
        try
        {
            var fieldInfo = value.GetType().GetField(value.ToString());

            var customValueAttributes = fieldInfo.GetCustomAttributes(
                typeof(CustomEnumValueAttribute), false) as CustomEnumValueAttribute[];

            if (customValueAttributes == null) return string.Empty;
            return (customValueAttributes.Length > 0) ? customValueAttributes[0].Description : value.ToString();
        }
        catch (Exception ex)
        {
            Trace.WriteLine("EnumHelper.GetDescription:\r\n" + ex.Message);
            return string.Empty;
        }
    }

    /// <summary>
    /// Get value from custom attribute
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string GetCustomValue(EnumType value)
    {
        try
        {
            var fieldInfo = value.GetType().GetField(value.ToString());

            var customValueAttributes = fieldInfo.GetCustomAttributes(
                typeof(CustomEnumValueAttribute), false) as CustomEnumValueAttribute[];

            if (customValueAttributes == null || customValueAttributes.Length == 0) return string.Empty;
            return customValueAttributes[0].Value;
        }
        catch (Exception ex)
        {
            Trace.WriteLine("EnumHelper.GetDescription:\r\n" + ex.Message);
            return string.Empty;
        }
    }


    public static List<string> GetDescriptions()
    {
        var descriptions = new List<string>();

        try
        {
            var allFields = typeof(EnumType).GetFields();

            foreach (var field in allFields)
            {//Ignore special field
                if (field.IsSpecialName) continue;
                var description = (DescriptionAttribute)field.GetCustomAttribute(typeof(DescriptionAttribute));
                if (description == null) continue;
                descriptions.Add(description.Description);
            }

            return descriptions;

        }
        catch (Exception ex)
        {
            Trace.WriteLine("EnumHelper.GetDescriptions:\r\n" + ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Get description from custom attribute
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static List<string> GetCustomDescriptions()
    {
        var descriptions = new List<string>();

        try
        {
            var allFields = typeof(EnumType).GetFields();

            foreach (var field in allFields)
            {//Ignore special field
                if (field.IsSpecialName) continue;
                var description = (CustomEnumValueAttribute)field.GetCustomAttribute(typeof(CustomEnumValueAttribute));
                if (description == null) continue;
                descriptions.Add(description.Description);
            }

            return descriptions;

        }
        catch (Exception ex)
        {
            Trace.WriteLine("EnumHelper.GetDescriptions:\r\n" + ex.Message);
            return null;
        }
    }
}


/// <summary>
/// Used to store multiple values for a property
/// </summary>
public class CustomEnumValueAttribute : Attribute
{
    /// <summary>
    /// Used for display only
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Actual value
    /// </summary>
    public string Value { get; set; }



    public CustomEnumValueAttribute(string _value, string _description = null)
    {
        Value = _value;
        Description = _description;
    }



}