using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CheckPointManager : MonoBehaviour
{
    [SerializeField] private List<CheckPoint> checkpoints;
    
    [SerializeField] private List<DinoStand> dinoStands;

    public bool AreAllTasksComplete => 
        checkpoints.All(cp => cp.flag == true) && 
        dinoStands.All(ds => ds.flag == true);

    public int TotalTasks => checkpoints.Count + dinoStands.Count;

    public int CompletedTasks => 
        checkpoints.Count(cp => cp.flag == true) + 
        dinoStands.Count(ds => ds.flag == true);

    void Awake()
    {
        checkpoints = FindObjectsOfType<CheckPoint>().ToList();  
        dinoStands = FindObjectsOfType<DinoStand>().ToList();
        
        // Debug.Log($"Checkpoints: {checkpoints.Count}, DinoStands: {dinoStands.Count}");
    }
}