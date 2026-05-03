using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace AutoServiceBooking.Web.Extensions
{
    public static class EnumExtensions
    {
        private static readonly ConcurrentDictionary<Enum, string> DisplayNames = new ConcurrentDictionary<Enum, string>();

        public static string GetDisplayName(this Enum value)
        {
            return DisplayNames.GetOrAdd(value, enumValue =>
            {
                MemberInfo? member = enumValue.GetType().GetMember(enumValue.ToString()).FirstOrDefault();
                DisplayAttribute? display = member?.GetCustomAttribute<DisplayAttribute>();

                return display?.Name ?? enumValue.ToString();
            });
        }
    }
}
