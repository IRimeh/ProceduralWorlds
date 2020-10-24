using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deformer
{
    private DeformationSettings settings;
    private Noise noise;

    public Deformer(DeformationSettings _settings, int _noiseSeed)
    {
        noise = new Noise(_noiseSeed);
        settings = _settings;
    }

    public Vector3 GetDeformedPoint(Vector3 point)
    {
        Vector3 pos = point;
        Vector3 uvs = point;

        float terrain =     GetTerrainDeformation(uvs * settings.TerrainNoiseScale)     * (settings.TerrainEnabled ? 1 : 0);
        float mountains =   GetMountainDeformation(uvs * settings.MountainNoiseScale)   * (settings.MountainsEnabled ? 1 : 0);
        float ocean =       GetOceanDeformation(uvs * settings.OceanNoiseScale)         * (settings.OceansEnabled ? 1 : 0);

        float terrainHeight = (terrain * settings.TerrainNoiseStrength);
        float mountainHeight = (mountains * settings.MountainNoiseStrength);
        float oceanDepth = (ocean * settings.OceanDepth) * (1.0f - mountains);

        float height = terrainHeight + mountainHeight - oceanDepth;

        pos +=  point.normalized * height;
        return pos;
    }


    private float GetTerrainDeformation(Vector3 uvs)
    {
        float height = noise.Evaluate(uvs);
        for (int i = 1; i < settings.TerrainNoiseLayers; i++)
        {
            float temp = noise.Evaluate(uvs * Mathf.Pow(2.0f, i));
            switch (settings.TerrainBlendMode)
            {
                case BlendSetting.Add:
                    height += temp;
                    break;
                case BlendSetting.Multiply:
                    height *= temp;
                    break;
                case BlendSetting.Subtract:
                    height -= temp;
                    break;
            }
        }

        switch (settings.TerrainBlendMode)
        {
            case BlendSetting.Add:
                height = height / settings.TerrainNoiseLayers;
                break;
            case BlendSetting.Subtract:
                height = height * settings.TerrainNoiseLayers;
                break;
            default:
                break;
        }


        height *= Mathf.Max(noise.Evaluate(uvs * 0.1f));
        height = Mathf.Clamp01(height);
        height = Mathf.Pow(height, settings.TerrainNoisePower);
        return height;
    }

    private float GetMountainDeformation(Vector3 uvs)
    {
        float height = noise.Evaluate(uvs) * 0.5f;
        for (int i = 1; i < settings.MountainNoiseLayers; i++)
        {
            float temp = noise.Evaluate(uvs * i) * 0.5f;
            if (i % 2 == 0)
                height -= temp;
            else
                height += temp;
        }

        height = height * (noise.Evaluate(uvs * 1.0f) - noise.Evaluate(uvs * 0.5f));

        height = Mathf.Clamp01(height);
        height = Mathf.Pow(height, settings.MountainNoisePower);

        return height;
    }

    private float GetOceanDeformation(Vector3 uvs)
    {
        float height = noise.Evaluate(uvs);
        height += noise.Evaluate((uvs * 2.0f) + new Vector3(10.215f, 0.0f, 0.0f));
        height *= 0.5f;
        height = Mathf.Pow(height, settings.OceanNoisePower);

        height = Mathf.Clamp01(height - settings.OceanThreshold) * (1.0f / (1.0f - settings.OceanThreshold));

        return height;
    }

    public void UpdateSettings(DeformationSettings newDeformationSettings, int newSeed)
    {
        settings = newDeformationSettings;
        noise = new Noise(newSeed);
    }
}

