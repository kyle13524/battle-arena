using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityEffect
{
    public float duration;
    public float rate;
    public Type type;
    public float value;

    public enum Type
    {
        DOT,
        HOT,
        SLOW,
        SPEED,
        DMG_DEC,
        DMG_INC,
        HP_DEC,
        HP_INC
    }
}
