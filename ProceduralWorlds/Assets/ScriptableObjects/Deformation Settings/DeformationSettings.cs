using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "Deformation Settings", menuName = "ScriptableObjects/Deformation Settings", order = 1)]
public class DeformationSettings : ScriptableObject
{
    [Header("General Terrain")]
    public bool TerrainEnabled = true;
    [Range(1, 5)]
    public int TerrainNoiseLayers;
    [Range(0.01f, 0.25f)]
    public float TerrainNoiseScale;
    [Range(0.5f, 10.0f)]
    public float TerrainNoiseStrength;
    [Range(0.5f, 4.0f)]
    public float TerrainNoisePower;
    public BlendSetting TerrainBlendMode = BlendSetting.Add;


    [Header("Mountain Variables")]
    public bool MountainsEnabled = true;
    [Range(1, 5)]
    public int MountainNoiseLayers;
    [Range(0.02f, 0.04f)] 
    public float MountainNoiseScale;
    [Range(0.1f, 100.0f)]
    public float MountainNoiseStrength;
    [Range(0.01f, 4.0f)]
    public float MountainNoisePower;

    [Header("Ocean Variables")]
    public bool OceansEnabled = true;
    [Range(0.0f, 0.999f)]
    public float OceanThreshold = 0.2f;
    [Range(0.01f, 0.05f)]
    public float OceanNoiseScale;
    [Range(0.0f, 50.0f)]
    public float OceanDepth = 1.0f;
    [Range(0.01f, 4.0f)]
    public float OceanNoisePower;

    private void OnValidate()
    {
        if(World.world && World.world.autoUpdate)
            World.world.GenerateWorld(this);
    }

    public void Randomize()
    {
        TerrainNoiseLayers = Random.Range(4, 5);
        TerrainNoiseScale = Mathf.Max(Random.Range(0.2f, 0.35f), 0.125f / TerrainNoiseLayers);
        TerrainNoiseStrength = Mathf.Max(Mathf.Pow(Random.Range(0.0f, 1.0f), 2), 0.5f) * 10.0f;
        TerrainNoisePower = Random.Range(0.8f, 1.25f);
        TerrainBlendMode = BlendSetting.Multiply;

        MountainNoiseLayers = 5;
        MountainNoiseScale = Random.Range(0.02f, 0.04f);
        MountainNoiseStrength = Mathf.Max(Random.Range(7.5f, 20.0f), TerrainNoiseStrength);
        MountainNoisePower = Random.Range(0.75f, 1.25f);

        OceanThreshold = Random.Range(0.3f, 0.6f);
        OceanNoiseScale = Random.Range(0.005f, 0.015f);
        OceanDepth = Random.Range(5.0f, 30.0f);
        OceanNoisePower = Random.Range(0.8f, 2.0f);
    }
}


public enum BlendSetting
{
    Add,
    Multiply,
    Subtract
}
