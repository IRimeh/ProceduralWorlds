using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ColorController
{
    private ColorSettings settings;
    private Texture2D colorGradientTexture;

    public ColorController(ColorSettings _settings, int _textureResolution = 64)
    {
        settings = _settings;
        colorGradientTexture = new Texture2D(_textureResolution, 1);
        GenerateGradientTexture();
    }

    private void GenerateGradientTexture()
    {
        for (int i = 0; i < colorGradientTexture.width; i++)
        {
            colorGradientTexture.SetPixel(i, 0, settings.colorGradient.Evaluate((1.0f / colorGradientTexture.width) * i));
        }
        colorGradientTexture.Apply();
    }

    public void ApplyColorSettings(Material material, float minHeight, float maxHeight)
    {
        GenerateGradientTexture();
        material.SetTexture("_ColorGradient", colorGradientTexture);
        material.SetFloat("_MinHeight", minHeight);
        material.SetFloat("_MaxHeight", maxHeight);
    }

    public void UpdateSettings(ColorSettings newSettings)
    {
        settings = newSettings;
    }
}
