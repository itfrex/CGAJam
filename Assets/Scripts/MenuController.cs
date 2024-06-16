using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    public void StartGame(){
        GameController.Instance.StartGame();
    }
    public void ChangeContrast(){
        GameController.Instance.SwitchContrast();
    }
    public void ChangeVolume(){
        GameController.Instance.ChangeVolume();
    }
}
