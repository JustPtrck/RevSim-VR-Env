using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Score Object", menuName = "RevSim/ScoreObject")]
public class ScoreObject : ScriptableObject 
{
    [SerializeField] private int _score = 1;
    [SerializeField] private bool _oneTimeUse = true;
    
    public void Interact(){
        
        if (_oneTimeUse) Destroy(this);
    }
}
