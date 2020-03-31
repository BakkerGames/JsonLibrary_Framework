// Purpose: Provide a set of routines to support JSON Object and JSON Array classes
// Author : Scott Bakker
// Created: 09/13/2019

using System;
using System.Collections;
using System.Text;

namespace JsonLibrary
{
    public enum JsonFormat
    {
        None,
        Indent,
        Tabs
    }

    public static class JsonRoutines
    {

        #region constants

        private const string _dateFormat = "yyyy-MM-dd";
        private const string _timeFormat = "HH:mm:ss";
        private const string _timeMilliFormat = "HH:mm:ss.fff";
        private const string _dateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        private const string _dateTimeMilliFormat = "yyyy-MM-dd HH:mm:ss.fff";
        private const string _dateTimeOffsetFormat = "yyyy-MM-dd HH:mm:sszzz";
        private const string _dateTimeOffsetMilliFormat = "yyyy-MM-dd HH:mm:ss.fffffffzzz";

        private const int _indentSpaceSize = 2;

        #endregion

        #region internal routines

        internal static string IndentSpace(int indentLevel, JsonFormat jf)
        {
            // Purpose: Return a string with the proper number of spaces or tabs
            // Author : Scott Bakker
            // Created: 09/13/2019
            if (indentLevel <= 0 || jf == JsonFormat.None)
            {
                return "";
            }
            if (jf == JsonFormat.Tabs)
            {
                return new string('\t', indentLevel);
            }
            return new string(' ', indentLevel * _indentSpaceSize);
        }

        internal static string ValueToString(object value, JsonFormat jf)
        {
            // Purpose: Return a value in proper JSON string format
            // Author : Scott Bakker
            // Created: 09/13/2019
            int indentLevel = -1; // don't indent
            return ValueToString(value, ref indentLevel, jf);
        }

        internal static string ValueToString(object value, ref int indentLevel, JsonFormat jf)
        {
            // Purpose: Return a value in proper JSON string format
            // Author : Scott Bakker
            // Created: 09/13/2019

            if (value == null)
            {
                return "null";
            }

            Type t = value.GetType();

            // Check for generic list types
            if (t.IsGenericType)
            {
                StringBuilder result = new StringBuilder();
                result.Append("[");
                if (jf != JsonFormat.None)
                {
                    indentLevel++;
                }
                bool addComma = false;
                foreach (object obj in (IEnumerable)value)
                {
                    if (addComma)
                    {
                        result.Append(",");
                    }
                    else
                    {
                        addComma = true;
                    }
                    if (jf != JsonFormat.None)
                    {
                        result.AppendLine();
                        result.Append(IndentSpace(indentLevel, jf));
                    }
                    result.Append(ValueToString(obj, jf));
                }
                if (indentLevel > 0)
                {
                    indentLevel--;
                    result.AppendLine();
                    result.Append(IndentSpace(indentLevel, jf));
                }
                result.Append("]");
                return result.ToString();
            }

            // Check for byte array, return as hex string "0x00..."
            if (t.IsArray && t == typeof(byte[]))
            {
                StringBuilder result = new StringBuilder();
                result.Append("0x");
                foreach (byte b in (byte[])value)
                {
                    result.Append(b.ToString("x2", null));
                }
                return result.ToString();
            }

            // Check for array, return in JArray format
            if (t.IsArray)
            {
                StringBuilder result = new StringBuilder();
                result.Append("[");
                if (jf != JsonFormat.None)
                {
                    indentLevel++;
                }
                bool addComma = false;
                for (int i = 0; i < ((Array)value).Length; i++)
                {
                    if (addComma)
                    {
                        result.Append(",");
                    }
                    else
                    {
                        addComma = true;
                    }
                    if (jf != JsonFormat.None)
                    {
                        result.AppendLine();
                        result.Append(IndentSpace(indentLevel, jf));
                    }
                    object obj = ((Array)value).GetValue(i);
                    result.Append(ValueToString(obj, jf));
                }
                if (indentLevel > 0)
                {
                    indentLevel--;
                    result.AppendLine();
                    result.Append(IndentSpace(indentLevel, jf));
                }
                result.Append("]");
                return result.ToString();
            }

            // Check for individual types
            if (t == typeof(string))
            {
                StringBuilder result = new StringBuilder();
                result.Append("\"");
                foreach (char c in (string)value)
                {
                    result.Append(ToJsonChar(c));
                }
                result.Append("\"");
                return result.ToString();
            }
            if (t == typeof(char))
            {
                StringBuilder result = new StringBuilder();
                result.Append("\"");
                result.Append(ToJsonChar((char)value));
                result.Append("\"");
                return result.ToString();
            }
            if (t == typeof(Guid))
            {
                return $"\"{value}\"";
            }
            if (t == typeof(bool))
            {
                if ((bool)value)
                {
                    return "true";
                }
                else
                {
                    return "false";
                }
            }
            if (t == typeof(DateTime))
            {
                DateTime d = (DateTime)value;
                if (d.Hour + d.Minute + d.Second + d.Millisecond == 0)
                {
                    return $"\"{d.ToString(_dateFormat, null)}\"";
                }
                if (d.Year + d.Month + d.Day == 0)
                {
                    if (d.Millisecond == 0)
                    {
                        return $"\"{d.ToString(_timeFormat, null)}\"";
                    }
                    else
                    {
                        return $"\"{d.ToString(_timeMilliFormat, null)}\"";
                    }
                }
                if (d.Millisecond == 0)
                {
                    return $"\"{d.ToString(_dateTimeFormat, null)}\"";
                }
                else
                {
                    return $"\"{d.ToString(_dateTimeMilliFormat, null)}\"";
                }
            }
            if (t == typeof(DateTimeOffset))
            {
                DateTimeOffset d = (DateTimeOffset)value;
                if (d.Millisecond == 0)
                {
                    return $"\"{d.ToString(_dateTimeOffsetFormat, null)}\"";
                }
                else
                {
                    return $"\"{d.ToString(_dateTimeOffsetMilliFormat, null)}\"";
                }
            }
            if (t == typeof(JObject))
            {
                return ((JObject)value).ToString(ref indentLevel, jf);
            }
            if (t == typeof(JArray))
            {
                return ((JArray)value).ToString(ref indentLevel, jf);
            }
            if (t == typeof(byte) ||
                t == typeof(sbyte) ||
                t == typeof(short) ||
                t == typeof(int) ||
                t == typeof(long) ||
                t == typeof(ushort) ||
                t == typeof(uint) ||
                t == typeof(ulong) ||
                t == typeof(float) ||
                t == typeof(double) ||
                t == typeof(decimal))
            {
                // Let ToString do all the work
                return value.ToString();
            }

            throw new SystemException($"JSON Error: Unknown object type: {t}");
        }

        internal static string FromJsonString(string value)
        {
            // Purpose: Convert a string with escaped characters into control codes
            // Author : Scott Bakker
            // Created: 09/17/2019
            if (value == null)
            {
                return null;
            }
            if (!value.Contains("\\"))
            {
                return value;
            }
            StringBuilder result = new StringBuilder();
            bool lastBackslash = false;
            int unicodeCharCount = 0;
            string unicodeValue = "";
            foreach (char c in value)
            {
                if (unicodeCharCount > 0)
                {
                    unicodeValue += c;
                    unicodeCharCount--;
                    if (unicodeCharCount == 0)
                    {
                        result.Append(Convert.ToChar(Convert.ToUInt16(unicodeValue, 16)));
                        unicodeValue = "";
                    }
                }
                else if (lastBackslash)
                {
                    switch (c)
                    {
                        case '\"':
                            result.Append('\"');
                            break;
                        case '\\':
                            result.Append('\\');
                            break;
                        case '/':
                            result.Append('/');
                            break;
                        case 'r':
                            result.Append('\r');
                            break;
                        case 'n':
                            result.Append('\n');
                            break;
                        case 't':
                            result.Append('\t');
                            break;
                        case 'b':
                            result.Append('\b');
                            break;
                        case 'f':
                            result.Append('\f');
                            break;
                        case 'u':
                            unicodeCharCount = 4;
                            unicodeValue = "";
                            break;
                        default:
                            throw new SystemException($"JSON Error: Unexpected escaped char: {c}");
                    }
                    lastBackslash = false;
                }
                else if (c == '\\')
                {
                    lastBackslash = true;
                }
                else
                {
                    result.Append(c);
                }
            }
            return result.ToString();
        }

        internal static string GetToken(ref int pos, string value)
        {
            // Purpose: Get a single token from string value for parsing
            // Author : Scott Bakker
            // Created: 09/13/2019
            // Notes  : Does not do escaped character expansion here, just passes exact value.
            //        : Properly handles \" within strings properly this way, but nothing else.
            if (value == null)
            {
                return null;
            }
            char c;
            // Ignore whitespece before token
            SkipWhitespace(ref pos, value);
            // Get first char, check for special symbols
            c = value[pos];
            pos++;
            // Stop if one-character JSON symbol found
            if (IsJsonSymbol(c))
            {
                return c.ToString();
            }
            // Have to build token char by char
            StringBuilder result = new StringBuilder();
            bool inQuote = false;
            bool lastBackslash = false;
            do
            {
                // Check for whitespace or symbols to end token
                if (!inQuote)
                {
                    if (IsWhitespace(c))
                    {
                        break;
                    }
                    if (IsJsonSymbol(c))
                    {
                        pos--; // move back one char so symbol can be read next time
                        break;
                    }
                    // Any comments end the token
                    if (c == '/' && pos < value.Length)
                    {
                        if (value[pos] == '*' || value[pos] == '/')
                        {
                            pos--; // move back one char so comment can be read next time
                            break;
                        }
                    }
                    if (c != '\"' && !IsJsonValueChar(c))
                    {
                        throw new SystemException($"JSON Error: Unexpected character: {c}");
                    }
                }
                // Check for escaped chars
                if (inQuote && lastBackslash)
                {
                    // Add backslash and character, no expansion here
                    result.Append('\\');
                    result.Append(c);
                    lastBackslash = false;
                }
                else if (inQuote && c == '\\')
                {
                    // Remember backslash for next loop, but don't add it to result
                    lastBackslash = true;
                }
                else if (c == '\"')
                {
                    // Check for quotes around a string
                    if (inQuote)
                    {
                        result.Append('\"'); // add ending quote
                        break; // Token is done
                    }
                    if (result.Length > 0)
                    {
                        // Quote in the middle of a token?
                        throw new SystemException("JSON Error: Unexpected quote char");
                    }
                    result.Append('\"'); // add beginning quote
                    inQuote = true;
                }
                else
                {
                    // Add this char
                    result.Append(c);
                }
                // Get char for next loop
                c = value[pos];
                pos++;
            }
            while (pos <= value.Length);
            return result.ToString();
        }

        internal static object JsonValueToObject(string value)
        {
            // Purpose: Convert a string representation of a value to an actual object
            // Author : Scott Bakker
            // Created: 09/13/2019
            if (value == null || value.Length == 0)
            {
                return null;
            }
            try
            {
                if (value.StartsWith("\"", StringComparison.Ordinal) && value.EndsWith("\"", StringComparison.Ordinal))
                {
                    value = value.Substring(1, value.Length - 2); // remove quotes
                    if (DateTime.TryParse(value, out DateTime tempDate))
                    {
                        return tempDate;
                    }
                    // Parse all escaped sequences to chars
                    return FromJsonString(value);
                }
                if (value == "null")
                {
                    return null;
                }
                if (value == "true")
                {
                    return true;
                }
                if (value == "false")
                {
                    return false;
                }
                // must be numeric
                if (value.Contains("e") || value.Contains("E"))
                {
                    return double.Parse(value);
                }
                if (value.Contains("."))
                {
                    return decimal.Parse(value);
                }
                if (long.Parse(value) > int.MaxValue || long.Parse(value) < int.MinValue)
                {
                    return long.Parse(value);
                }
                return int.Parse(value);
            }
            catch (Exception ex)
            {
                throw new SystemException($"JSON Error: Value not recognized: {value}\r\n{ex.Message}");
            }
        }

        internal static void SkipWhitespace(ref int pos, string value)
        {
            // Purpose: Skip over any whitespace characters or any recognized comments
            // Author : Scott Bakker
            // Created: 09/23/2019
            // Notes  : Comments consist of /*...*/ or // to eol (aka line comment)
            //        : An unterminated comment is not an error, it is just all skipped
            if (value == null)
            {
                return;
            }
            bool inComment = false;
            bool inLineComment = false;
            while (pos < value.Length)
            {
                if (inComment)
                {
                    if (value[pos] == '/' && value[pos - 1] == '*') // found ending "*/"
                    {
                        inComment = false;
                    }
                    pos++;
                    continue;
                }
                if (inLineComment)
                {
                    if (value[pos] == '\r' || value[pos] == '\n') // found end of line
                    {
                        inLineComment = false;
                    }
                    pos++;
                    continue;
                }
                if (value[pos] == '/' && pos + 1 < value.Length && value[pos + 1] == '*')
                {
                    inComment = true;
                    pos += 3; // must be sure to skip enough so "/*/" pattern doesn't work but "/**/" does
                    continue;
                }
                if (value[pos] == '/' && pos + 1 < value.Length && value[pos + 1] == '/')
                {
                    inLineComment = true;
                    pos += 2; // skip over "//"
                    continue;
                }
                if (IsWhitespace(value[pos]))
                {
                    pos++;
                    continue;
                }
                break;
            }
        }

        #endregion

        #region private routines

        private static string ToJsonChar(char c)
        {
            // Purpose: Return a character in proper JSON format
            // Author : Scott Bakker
            // Created: 09/13/2019
            if (c == '\\') return "\\\\";
            if (c == '\"') return "\\\"";
            if (c == '\r') return "\\r";
            if (c == '\n') return "\\n";
            if (c == '\t') return "\\t";
            if (c == '\b') return "\\b";
            if (c == '\f') return "\\f";
            if (c < 32 || c >= 127)
            {
                return "\\u" + ((int)c).ToString("x4", null); // always lowercase
            }
            return c.ToString();
        }

        private static bool IsWhitespace(char c)
        {
            // Purpose: Check for recognized whitespace characters
            // Author : Scott Bakker
            // Created: 09/13/2019
            if (c == ' ') return true;
            if (c == '\r') return true;
            if (c == '\n') return true;
            if (c == '\t') return true;
            if (c == '\b') return true;
            if (c == '\f') return true;
            return false;
        }

        private static bool IsJsonSymbol(char c)
        {
            // Purpose: Check for recognized JSON symbol chars which are tokens by themselves
            // Author : Scott Bakker
            // Created: 09/13/2019
            if (c == '{') return true;
            if (c == '}') return true;
            if (c == '[') return true;
            if (c == ']') return true;
            if (c == ':') return true;
            if (c == ',') return true;
            return false;
        }

        private static bool IsJsonValueChar(char c)
        {
            // Purpose: Check for any valid characters in a non-string value
            // Author : Scott Bakker
            // Created: 09/23/2019
            switch (c)
            {
                case 'n': // null
                case 'u':
                case 'l':
                case 't': // true
                case 'r':
                case 'e':
                case 'f': // false
                case 'a':
                case 's':
                case '0': // numeric
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case '-':
                case '+':
                case '.':
                case 'E': // also 'e' checked for above
                    return true;
            }
            return false;
        }

        #endregion
    }
}
