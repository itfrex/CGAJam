using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaletteCycler : MonoBehaviour
{
    
    int colorIndex;
    public Color[] rainbow;
    private float timer;
    public float shiftTime;
    public void Update(){
        timer += Time.deltaTime;
        if(timer > shiftTime){
            colorIndex = (colorIndex+1)%rainbow.Length;
            timer -= shiftTime;
            GameController.Instance.SwapColor("_Magic",rainbow[colorIndex]);
        }
    }
}
