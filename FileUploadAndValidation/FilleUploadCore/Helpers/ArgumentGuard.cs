using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FilleUploadCore.Helpers
{
    public static class ArgumentGuard
    {
        public static void NotNull<T>(T argument, string argumentName)
        {
            if (argument == null)
            {
                throw new ArgumentNullException($"'{argumentName}' cannot be null");
            }
        }

        public static void NotDefault<T>(T value, string argumentName)
            where T : struct
        {
            if (value.Equals(default(T)))
            {
                throw new ArgumentNullException($"'{argumentName}' needs to be set.");
            }
        }

        public static void NotNullOrEmpty<T>(IEnumerable<T> argument, string argumentName)
        {
            if (argument == null || !argument.Any())
            {
                throw new ArgumentException($"'{argumentName}' cannot be null or empty");
            }
        }

        public static void NotNullOrWhiteSpace(string argument, string argumentName)
        {
            if (string.IsNullOrWhiteSpace(argument))
            {
                throw new ArgumentException($"'{argumentName}' cannot be null or empty");
            }
        }

        public static void NotEmpty(Guid argument, string argumentName)
        {
            if (argument == Guid.Empty)
            {
                throw new ArgumentException($"'{argumentName}' cannot be an empty Guid");
            }
        }

    }
}
