using System;
using System.Globalization;
using System.Threading;

namespace LHJ.Tools
{
    internal static class ErrorUtilities
    {
        internal static void VerifyThrowArgument(bool condition, string formattedMessage)
        {
            if (condition)
                return;
            ErrorUtilities.ThrowArgumentException((Exception)null, formattedMessage);
        }

        internal static void VerifyThrowArgument(bool condition, string formattedMessage, object arg0)
        {
            if (condition)
                return;
            ErrorUtilities.ThrowArgumentException((Exception)null, formattedMessage, arg0);
        }

        internal static void VerifyThrowArgument(bool condition, string formattedMessage, object arg0, object arg1)
        {
            if (condition)
                return;
            ErrorUtilities.ThrowArgumentException((Exception)null, formattedMessage, arg0, arg1);
        }

        private static Exception ThrowArgumentException(Exception innerException, string unformattedMessage, params object[] args)
        {
            throw new ArgumentException(string.Format((IFormatProvider)CultureInfo.CurrentCulture, unformattedMessage, args), innerException);
        }

        internal static void VerifyThrowArgumentNull(object parameter, string parameterName)
        {
            if (parameter != null)
                return;
            ErrorUtilities.ThrowArgumentNull(parameterName);
        }

        internal static void ThrowArgumentNull(string parameterName)
        {
            ErrorUtilities.ThrowArgumentNull(parameterName, Properties.Resources.ParameterCannotBeNull);
        }

        internal static void ThrowArgumentNull(string parameterName, string message)
        {
            throw new ArgumentNullException(parameterName, string.Format((IFormatProvider)CultureInfo.CurrentCulture, message, new object[1]
            {
        (object) parameterName
            }));
        }

        internal static string VerifyThrowArgumentNullOrWhitespace(string parameter, string parameterName)
        {
            if (parameter == null)
                ErrorUtilities.ThrowArgumentNull(parameterName);
            if (parameter.Trim().Length == 0)
                throw new ArgumentException(string.Format((IFormatProvider)CultureInfo.CurrentCulture, Properties.Resources.ParameterCannotBeEmptyOrWhitespace, new object[1]
                {
          (object) parameterName
                }));
            return parameter;
        }

        internal static Exception ThrowArgument(string formattedMessage)
        {
            throw ErrorUtilities.ThrowArgumentException((Exception)null, formattedMessage);
        }

        internal static Exception ThrowArgument(string unformattedMessage, object arg0)
        {
            throw ErrorUtilities.ThrowArgumentException((Exception)null, unformattedMessage, arg0);
        }

        internal static Exception ThrowArgument(string unformattedMessage, object arg0, object arg1)
        {
            throw ErrorUtilities.ThrowArgumentException((Exception)null, unformattedMessage, arg0, arg1);
        }

        internal static Exception ThrowArgument(string unformattedMessage, params object[] args)
        {
            throw ErrorUtilities.ThrowArgumentException((Exception)null, unformattedMessage, args);
        }

        internal static bool IsCriticalException(Exception e)
        {
            return e is StackOverflowException || e is OutOfMemoryException || (e is ThreadAbortException || e is AccessViolationException);
        }
    }
}
