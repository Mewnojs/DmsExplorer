using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DmsUtils;

public static class DmsNodeStringEditExtension
{
    public static bool SaveStringEdit(this DmsNode node, string text, string s_digits, bool isRawEdit = false) 
    {
        Type nodeType = node.GetType();
        if (nodeType == typeof(DmsDataNode) || isRawEdit)
        {
            string sanitized = Regex.Replace(text, "[^0-9a-fA-F]", "");
            if (sanitized.Length % 2 != 0) return false;    // invalid Length
            byte[] bytes = new byte[sanitized.Length / 2];
            for (int i = 0; i < bytes.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(sanitized.Substring(i, 2), 16);
            }
            node.RawData = bytes;
            return true;
        } 
        else if (node is DmsFloatNode floatnode)
        {
            floatnode.NumberData = double.Parse(text);
            return true;
        }
        else if (node is DmsAnsiStringNode stringnode)
        {
            stringnode.StringData = text;
            return true;
        }
        int digits;
        if (!int.TryParse(s_digits, out digits)) return false;
        if (node is DmsIntegerNode intnode)
        {
            BigInteger bigInteger = BigInteger.Parse(text);
            intnode.IntegerData = bigInteger;
            intnode.Length = digits;
            return true;
        }

        return false;
    }
}
