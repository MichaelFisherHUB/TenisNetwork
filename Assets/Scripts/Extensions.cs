using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Extensions
{
    public static class Extensions
    {
        public static string ColorTag(this string str, ColorStringTag color)
        {
            string tag = color.ToString().ToLower();
            return string.Format("<color={0}>{1}</color>", tag, str);
        }
    }
}
