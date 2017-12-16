using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlPromptDisplayHelpers : MonoBehaviour
{
    // Fields
    private static ControllerMapSpriteType m_controllerMapTypePC;

    // Methods
    public static string ConvertString(string inputString)
    {
        return inputString.Replace("[B_SL]", "‱").Replace("[B_CN]", "⁋").Replace("[B_BL]", "⁌").Replace("[B_BU]", "⁍").Replace("[B_LB]", "⁐").Replace("[B_RB]", "⁑").Replace("[B_LT]", "⁖").Replace("[B_RT]", "⁘").Replace("[D_ANY]", "⁛").Replace("[D_LEFT]", "⁜").Replace("[D_RIGHT]", "⁞").Replace("[D_UP]", "⁝").Replace("[D_DOWN]", "⁰").Replace("[D_LR]", "ⁱ").Replace("[D_UD]", "⁴").Replace("[S_L]", "⁵").Replace("[S_R]", "⁶").Replace("[M]", "꜒").Replace("[M_LC]", "꜓").Replace("[M_RC]", "꜔").Replace("[M_MC]", "꜕").Replace("[M_MS]", "꜖");
    }

    public static ControllerMapSpriteType GetMapSpriteType()
    {
        return m_controllerMapTypePC;
    }

    public static void SetLastControllerType(ControllerMapSpriteType cmt)
    {
        m_controllerMapTypePC = cmt;
    }

    // Nested Types
    public enum ControllerMapSpriteType
    {
        STEAM,
        PS4,
        XBOX,
        SWITCH
    }
}

