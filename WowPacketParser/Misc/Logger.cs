using System;
using System.Collections.Generic;
using System.Globalization;
using WowPacketParser.Enums;

namespace WowPacketParser.Misc
{
    public static class Logger
    {
        private static readonly Dictionary<string, List<long>> enumLogs =
            new Dictionary<string, List<long>>();

        public static void CheckForMissingValues<T>(long rawValue)
        {
            if (!Settings.LogErrors || !typeof(T).IsEnum || !Attribute.IsDefined(typeof(T), typeof(FlagsAttribute)))
                return;

            var key = typeof(T).ToString().Replace("WowPacketParser.Enums.", "");

            // Remove all know values
            foreach (T value in Enum.GetValues(typeof(T)))
                rawValue = rawValue & ~Convert.ToInt64(value, CultureInfo.InvariantCulture);

            if (rawValue == 0)
                return;

            long temp = 1;
            while (temp < rawValue)
            {
                if ((rawValue & temp) == temp)
                    AddEnumErrorLog(key, temp);
                temp <<= 2;
            }
        }

        private static void AddEnumErrorLog(string key, long rawValue)
        {
            var list = enumLogs.ContainsKey(key) ? enumLogs[key] : new List<long>();

            if (list.Contains(rawValue))
                return;

            list.Add(rawValue);
            enumLogs.Add(key, list);
        }

        public static void WriteErrors()
        {
            if (!Settings.LogErrors)
                return;

            Console.WriteLine();
            foreach (var pair in enumLogs)
            {
                pair.Value.Sort();

                var errors = "";
                foreach (var error in pair.Value)
                {
                    if (errors.Length > 0)
                        errors += ", ";

                    var str = "";
                    UnknownFlags enumFlag;
                    if (Enum.TryParse(error.ToString(CultureInfo.InvariantCulture), out enumFlag))
                        str = enumFlag.ToString();

                    errors += str;
                }

                Console.WriteLine("{0} has undefined flags: {1}", pair.Key, errors);
            }
        }
    }
}
