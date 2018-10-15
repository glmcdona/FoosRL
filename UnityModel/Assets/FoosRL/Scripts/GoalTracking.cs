using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalTracking : MonoBehaviour {

    public int player = -1;
    public TableManager tableManager = null;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.CompareTag("Ball"))
        {
            if (tableManager != null)
                tableManager.BallEnterGoal(player);
        }
    }
}
