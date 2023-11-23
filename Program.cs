using System;
using System.Reflection;

[AttributeUsage(AttributeTargets.Field)]
public class CustomNameAttribute : Attribute
{
    public string CustomName { get; }

    public CustomNameAttribute(string customName)
    {
        CustomName = customName;
    }
}

public class CustomSerialization
{
    public static string ObjectToString(object obj)
    {
        Type type = obj.GetType();
        var fields = type.GetFields();

        string result = "";

        foreach (var field in fields)
        {
            object value = field.GetValue(obj);
            string fieldName = GetCustomName(field) ?? field.Name;

            result += $"{fieldName}:{value}, ";
        }

        return result.TrimEnd(' ', ',');
    }

    public static void StringToObject(object obj, string data)
    {
        Type type = obj.GetType();
        var fields = type.GetFields();

        string[] keyValuePairs = data.Split(',');

        foreach (var pair in keyValuePairs)
        {
            string[] parts = pair.Split(':');
            string propertyName = parts[0].Trim();
            string propertyValue = parts[1].Trim();

            FieldInfo field = GetFieldByName(type, propertyName);
            if (field != null)
            {
                object parsedValue = Convert.ChangeType(propertyValue, field.FieldType);
                field.SetValue(obj, parsedValue);
            }
        }
    }

    private static FieldInfo GetFieldByName(Type type, string propertyName)
    {
        var fields = type.GetFields();

        foreach (var field in fields)
        {
            string fieldName = GetCustomName(field) ?? field.Name;

            if (fieldName == propertyName)
            {
                return field;
            }
        }

        return null;
    }

    private static string GetCustomName(FieldInfo field)
    {
        var customNameAttribute = (CustomNameAttribute)Attribute.GetCustomAttribute(field, typeof(CustomNameAttribute));

        return customNameAttribute?.CustomName;
    }
}

public class MyClass
{
    [CustomName("CustomFieldName")]
    public int I = 0;
}

class Program
{
    static void Main()
    {
        MyClass obj = new MyClass();

        string serializedData = CustomSerialization.ObjectToString(obj);
        Console.WriteLine(serializedData);

        CustomSerialization.StringToObject(obj, "CustomFieldName:42");
        Console.WriteLine(obj.I);
    }
}
