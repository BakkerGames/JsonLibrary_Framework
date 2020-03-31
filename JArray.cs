// Purpose: Provide a JSON Array class
// Author : Scott Bakker
// Created: 09/13/2019

// Notes  : The values in the list ARE ordered based on when they are added.
//          The values are NOT sorted, and there can be duplicates.
//        : The function ToString(JsonFormat.Indent) will return a string representation with
//          whitespace added. Two spaces per level are used for indenting, and CRLF between lines.
//        : The function ToString(JsonFormat.Tabs) will return a string representation with
//          tabs added. One tab per level is used for indenting, and CRLF between lines.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace JsonLibrary
{
    public class JArray : IEnumerable<object>
    {
        private List<object> _data;

        public JArray()
        {
            // Purpose: Create new JArray object
            // Author : Scott Bakker
            // Created: 09/13/2019
            _data = new List<object>();
        }

        public JArray(JArray ja)
        {
            // Purpose: Create new JArray object from an existing JArray
            // Author : Scott Bakker
            // Created: 09/13/2019
            _data = new List<object>();
            this.Append(ja);
        }

        public IEnumerator<object> GetEnumerator()
        {
            // Purpose: Provide IEnumerable access directly to _data
            // Author : Scott Bakker
            // Created: 09/13/2019
            return _data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            // Purpose: Provide IEnumerable access directly to _data
            // Author : Scott Bakker
            // Created: 09/13/2019
            return _data.GetEnumerator();
        }

        public void Add(object value)
        {
            // Purpose: Adds a new value to the end of the JArray list
            // Author : Scott Bakker
            // Created: 09/13/2019
            // Changes: 10/03/2019 Removed extra string processing, was wrong
            _data.Add(value);
        }

        public void Append(JArray ja)
        {
            // Purpose: Append all values in the sent JArray at the end of the JArray list
            // Author : Scott Bakker
            // Created: 09/13/2019
            if (ja != null && ja._data != null)
            {
                foreach (object obj in ja._data)
                {
                    _data.Add(obj);
                }
            }
        }

        public int Count()
        {
            // Purpose: Return the count of items in the JArray
            // Author : Scott Bakker
            // Created: 09/13/2019
            return _data.Count;
        }

        public object this[int index]
        {
            // Purpose: Give access to item values by index
            // Author : Scott Bakker
            // Created: 09/13/2019
            get
            {
                if (index < 0 || index >= _data.Count)
                {
                    throw new ArgumentOutOfRangeException();
                }
                return _data[index];
            }
            set
            {
                if (index < 0 || index >= _data.Count)
                {
                    throw new ArgumentOutOfRangeException();
                }
                _data[index] = value;
            }
        }

        public void RemoveAt(int index)
        {
            // Purpose: Remove the item at the specified index
            // Author : Scott Bakker
            // Created: 09/13/2019
            if (index < 0 || index >= _data.Count)
            {
                throw new ArgumentOutOfRangeException();
            }
            _data.RemoveAt(index);
        }

        public List<object> Items()
        {
            // Purpose: Get cloned list of all objects
            // Author : Scott Bakker
            // Created: 09/13/2019
            return new List<object>(_data);
        }

        public override string ToString()
        {
            // Purpose: Convert this JArray into a string with no formatting
            // Author : Scott Bakker
            // Created: 09/13/2019
            StringBuilder result = new StringBuilder();
            result.Append("[");
            bool addComma = false;
            foreach (object obj in _data)
            {
                if (addComma)
                {
                    result.Append(",");
                }
                else
                {
                    addComma = true;
                }
                result.Append(JsonRoutines.ValueToString(obj, JsonFormat.None));
            }
            result.Append("]");
            return result.ToString();
        }

        public string ToString(JsonFormat jf)
        {
            // Purpose: Convert this JArray into a string with formatting
            // Author : Scott Bakker
            // Created: 10/17/2019
            int indentLevel = 0;
            return ToString(ref indentLevel, jf);
        }

        public static JArray Parse(string value)
        {
            // Purpose: Convert a string into a JArray
            // Author : Scott Bakker
            // Created: 09/13/2019
            int pos = 0;
            return Parse(ref pos, value);
        }

        public static JArray Clone(JArray ja)
        {
            // Purpose: Clones a JArray
            // Author : Scott Bakker
            // Created: 09/20/2019
            JArray result = new JArray();
            if (ja != null && ja._data != null)
            {
                result._data = new List<object>(ja._data);
            }
            return result;
        }

        #region internal routines

        internal string ToString(ref int indentLevel, JsonFormat jf)
        {
            // Purpose: Convert this JArray into a string with formatting
            // Author : Scott Bakker
            // Created: 10/17/2019
            if (_data.Count == 0)
            {
                return "[]"; // avoid indent errors
            }
            StringBuilder result = new StringBuilder();
            result.Append("[");
            if (jf != JsonFormat.None)
            {
                indentLevel++;
                result.AppendLine();
            }
            bool addComma = false;
            foreach (object obj in _data)
            {
                if (addComma)
                {
                    result.Append(",");
                    if (jf != JsonFormat.None)
                    {
                        result.AppendLine();
                    }
                }
                else
                {
                    addComma = true;
                }
                if (indentLevel > 0)
                {
                    result.Append(JsonRoutines.IndentSpace(indentLevel, jf));
                }
                result.Append(JsonRoutines.ValueToString(obj, jf));
            }
            if (indentLevel > 0)
            {
                result.AppendLine();
                indentLevel--;
                if (indentLevel > 0)
                {
                    result.Append(JsonRoutines.IndentSpace(indentLevel, jf));
                }
            }
            result.Append("]");
            return result.ToString();
        }

        internal static JArray Parse(ref int pos, string value)
        {
            // Purpose: Convert a partial string into a JArray
            // Author : Scott Bakker
            // Created: 09/13/2019
            if (value == null || value.Length == 0)
            {
                return null;
            }
            JArray result = new JArray();
            JsonRoutines.SkipWhitespace(ref pos, value);
            if (value[pos] != '[')
            {
                throw new SystemException($"JSON Error: Unexpected token to start JArray: {value[pos]}");
            }
            pos++;
            do
            {
                JsonRoutines.SkipWhitespace(ref pos, value);
                // check for symbols
                if (value[pos] == ']')
                {
                    pos++;
                    break; // done building
                }
                if (value[pos] == ',')
                {
                    // this logic ignores extra commas, but is ok
                    pos++;
                    continue;
                }
                if (value[pos] == '{') // JObject
                {
                    JObject jo = JObject.Parse(ref pos, value);
                    result.Add(jo);
                }
                else if (value[pos] == '[') // JArray
                {
                    JArray ja = JArray.Parse(ref pos, value);
                    result.Add(ja);
                }
                else
                {
                    // Get value as a string, convert to object
                    string tempValue = JsonRoutines.GetToken(ref pos, value);
                    result.Add(JsonRoutines.JsonValueToObject(tempValue));
                }
            } while (true);
            return result;
        }

        #endregion

    }
}
