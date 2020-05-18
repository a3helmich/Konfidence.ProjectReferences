using System;
using JetBrains.Annotations;

namespace ToolModules.ExtensionMethods
{
    [UsedImplicitly]
    public static class BaseExtender
    {
        [ContractAnnotation("assignedObject:null => false")]
        public static bool IsAssigned(this object assignedObject)
        {
            if (assignedObject is string assignedString)
            {
                if (string.IsNullOrWhiteSpace(assignedString))
                {
                    return false;
                }

                return true;
            }

            if (assignedObject == null)
            {
                return false;
            }

            return true;
        }

        [UsedImplicitly]
        public static bool IsAssigned(this DateTime assignedTime)
        {
            if (assignedTime > DateTime.MinValue)
            {
                return true;
            }

            return false;
        }

        [UsedImplicitly]
        public static DateTime StartOfDayTime(this DateTime dateTime)
        {
            var afterMidnight = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, DateTimeKind.Utc);

            return afterMidnight;
        }

        [UsedImplicitly]
        public static DateTime EndOfDayTime(this DateTime dateTime)
        {
            var midnight = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 23, 59, 59, DateTimeKind.Utc);

            return midnight;
        }

        [UsedImplicitly]
        [ContractAnnotation("line:null => true")]
        public static bool IsEof(this string line)
        {
            return line == null;
        }

        [UsedImplicitly]
        [ContractAnnotation("assignedGuid:null => false")]
        public static bool IsGuid(this string assignedGuid)
        {
            return Guid.TryParse(assignedGuid, out var _);
        }

        [ContractAnnotation("assignedGuid:null => false")]
        public static bool IsAssigned(this Guid assignedGuid)
        {
            return !Guid.Empty.Equals(assignedGuid);
        }
    }
}