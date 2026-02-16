using UnityEngine;

public class DinoStand : MonoBehaviour
{
    [Header("Settings")]
    public bool flag = false; 
    public Transform anchorPoint; 
    //public GameObject completeEffect;
    
    
    [Header("Detection")]
    public Vector3 boxSize = new Vector3(4, 3, 4);
    
    void Update()
    {
        if (flag) return;
        
        Collider[] hits = Physics.OverlapBox(transform.position, boxSize * 0.5f, transform.rotation);
        foreach (var hit in hits)
        {
            Dinosaur dino = hit.GetComponent<Dinosaur>();

            if (dino != null && dino.currentState == Dinosaur.DinoState.Stunned)
            {
                SecureDino(dino);
                break;
            }
        }
    }
    
    void SecureDino(Dinosaur dino)
    {
        flag = true; 
        dino.SecureDino(anchorPoint != null ? anchorPoint : transform);
        //if (completeEffect) Instantiate(completeEffect, transform.position, Quaternion.identity);
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = flag ? Color.green : Color.magenta;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, boxSize);
    }
}