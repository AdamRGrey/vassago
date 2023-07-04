using System;
using System.ComponentModel;
using System.Reflection;

namespace vassago.Models;

public static class Enumerations
{
    public enum LewdnessFilterLevel
    {
        [Description("this is a christian minecraft server 🙏")]
        Strict,
        [Description("G-Rated")]
        G,
        [Description("polite company")]
        Moderate,
        [Description(";) ;) ;)")]
        unrestricted
    }
    public enum MeannessFilterLevel
    {
        [Description("good vibes only")]
        Strict,
        [Description("a bit cheeky")]
        Medium,
        [Description("387.44 million miles of printed circuits, etc")]
        Unrestricted
    }
    public enum WellknownPermissions
    {
        Master, //e.g., me. not that I think this would ever be released?
        TwitchSummon,
    }

    public static string GetDescription<T>(this T enumerationValue)
    where T : struct
    {
        Type type = enumerationValue.GetType();
        if (!type.IsEnum)
        {
            throw new ArgumentException("EnumerationValue must be of Enum type", "enumerationValue");
        }

        //Tries to find a DescriptionAttribute for a potential friendly name
        //for the enum
        MemberInfo[] memberInfo = type.GetMember(enumerationValue.ToString());
        if (memberInfo != null && memberInfo.Length > 0)
        {
            object[] attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attrs != null && attrs.Length > 0)
            {
                //Pull out the description value
                return ((DescriptionAttribute)attrs[0]).Description;
            }
        }
        //If we have no description attribute, just return the ToString of the enum
        return enumerationValue.ToString();
    }
}