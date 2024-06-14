using System;
using System.Collections;
using System.Collections.Generic;
using TNRD;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Data", menuName ="ScriptableObjects/Artifact", order =1)]
public class Artifact : ScriptableObject
{
    public string fullName;
    public string description;
    public Sprite image;
    public bool stackable;
    public Effect[] effects;
    [Serializable]
    public class Effect{
        public SerializableInterface<IArtifactEffect> effect;
        public float amt;
    }
}
