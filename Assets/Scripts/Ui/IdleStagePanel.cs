using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleStagePanel : MonoBehaviour
{
    public static IdleStagePanel Instance;
    void Awake() => 
        Instance = this;

    [SerializeField] List<Behaviour> displayBehaviours;

    void Start() => 
        setActive(false);

    public void setActive(bool state)
    {
        foreach (Behaviour behaviour in this.displayBehaviours)
        {
            
            behaviour.enabled = state;
        }
    }
}
