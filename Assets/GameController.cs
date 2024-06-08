using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShortcutManagement;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private static GameController _instance;
    public static GameController Instance{
        get{
            if(_instance == null){
                _instance = GameObject.FindObjectOfType<GameController>();
            }
            return _instance;
        }
    }
    private static PlayerController player;

    private void Awake() {
        DontDestroyOnLoad(gameObject);
        player = GameObject.FindObjectOfType<PlayerController>();
    }
    public static PlayerController GetPlayer(){
        return player;
    }
}
