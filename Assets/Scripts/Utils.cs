using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils : MonoBehaviour
{
    [ContextMenu("List Muscle")]
    void ListMuscle()
    {
        String line = "";
        for (var i = 0; i < HumanTrait.MuscleCount; i++)
        {
            line += $"{HumanTrait.MuscleName[i].Replace(" ", "").Replace("-", "")} = {i},\n";
        }

        Debug.Log(line);
    }
}
