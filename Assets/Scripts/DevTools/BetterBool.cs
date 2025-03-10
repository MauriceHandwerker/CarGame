using System;
using UnityEngine;

public class BetterBool
{

    public static bool StringToBool(string value)
    {
        if (value == "false") return false;
        if (value == "False") return false;

        if (value == "true") return true;
        if (value == "True") return true;

        return false;
    }

    public static string BoolToString(bool value)
    {
        if (value) return "true";

        return "false";
    }

}
