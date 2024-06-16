using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ArtifactSelector : MonoBehaviour
{
    public GameObject[] panels;
    public Artifact[] artifacts;
    private Artifact[] selection;
    private bool cardSelected = false;
    void Start(){
        selection = new Artifact[panels.Length];
        List<int> options = new List<int>();
        for (int i = 0; i < artifacts.Length; i++){
            options.Add(i);
        }
        for (int i = 0; i < panels.Length; i++){
            int choice = Random.Range(0, options.Count);
            selection[i] = artifacts[options[choice]];
            options.RemoveAt(choice);
            while(!selection[i].stackable && CheckStacks(selection[i])){
                choice = Random.Range(0, options.Count);
                selection[i] = artifacts[options[choice]];
                options.RemoveAt(choice);
            }
            panels[i].transform.GetChild(0).GetComponent<Image>().sprite = selection[i].image;
            panels[i].transform.GetChild(1).GetComponent<TMP_Text>().text = selection[i].fullName;
            panels[i].transform.GetChild(2).GetComponent<TMP_Text>().text = selection[i].description;
        }
    }
    public void SelectCard(int i){
        if(!cardSelected){
            cardSelected = true;
            GameController.Instance.artifacts.Add(selection[i]);
            GameController.Instance.ProceedStage();
        }
    }
    private bool CheckStacks(Artifact artifact){
        foreach (Artifact a in GameController.Instance.artifacts){
            if(a.name.Equals(artifact.name)){
                return true;
            }
        }
        return false;
    }
}
