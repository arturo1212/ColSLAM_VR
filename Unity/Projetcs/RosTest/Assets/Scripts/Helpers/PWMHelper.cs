using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PWMHelper {

    public static int Remap(this float value, float from1, float to1, float from2, float to2)
    {
        if (value == 0) return (int)value;
        return (int)((value - from1) / (to1 - from1) * (to2 - from2) + from2);
    }

}
