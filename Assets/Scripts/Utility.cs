using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class Utility
{
    public static void SetAlpha(this Graphic graphic, float alpha)
    {
        if (alpha > 1f)
            alpha = (alpha - 0f) / (255f - 0f);
        alpha = Mathf.Clamp(alpha, 0f, 1f);

        Color c = graphic.color;
        graphic.color = new Color(c.r, c.g, c.b, alpha);
    }
}
