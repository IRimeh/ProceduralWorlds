using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ColorSettings00", menuName = "ScriptableObjects/Color Settings", order = 2)]
public class ColorSettings : ScriptableObject
{
    public Gradient colorGradient;

    private void OnValidate()
    {
        if(World.world && World.world.autoUpdate)
            World.world.GenerateColors();
    }
}
