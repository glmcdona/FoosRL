using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class PlayerAgent : Agent
{
    public int player = -1;

    public bool ControlledByKeys = false;

    // Rods order:
    //   0: Goalie
    //   1: Defense
    //   2: 5-bar
    //   3: 3-bar
    public List<GameObject> rods = new List<GameObject>(4);
    public float torque_factor = 5000.0f;
    public float force_factor = 30000.0f;
    public ForceMode mode = ForceMode.Force;

    private TableManager tm = null;

    // Use this for initialization
    void Start () {
        // Load the table manager
        tm = GetComponent<TableManager>();

        
    }

    public void SetCamera(Camera camera)
    {
        // Load this camera to this agent
        this.agentParameters.agentCameras.Clear();
        this.agentParameters.agentCameras.Add(camera);
    }

    // Update is called once per frame
    void Update () {
        // Update rods
        if (ControlledByKeys)
        {
            if (Input.GetKey(KeyCode.W))
            {
                foreach (GameObject rod in rods)
                {
                    rod.GetComponent<Rigidbody>().AddTorque(new Vector3(0.0f, 0.0f, torque_factor), mode);
                }
            }
            if (Input.GetKey(KeyCode.S))
            {
                foreach (GameObject rod in rods)
                {
                    rod.GetComponent<Rigidbody>().AddTorque(new Vector3(0.0f, 0.0f, -torque_factor), mode);
                }
            }
        }
    }

    public override void InitializeAgent()
    {
        // Nothing.
    }

    public override void CollectObservations()
    {
        // No observations, just the camera as input
        
        /*
        AddVectorObs(gameObject.transform.rotation.z);
        AddVectorObs(gameObject.transform.rotation.x);
        AddVectorObs(ball.transform.position - gameObject.transform.position);
        AddVectorObs(ballRb.velocity);
        */
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        // Take the actions. Action format is continuous:
        // 0-3: Linear rods 0 to 3
        // 4-7: Torque rods 0 to 3
        if (brain.brainParameters.vectorActionSpaceType == SpaceType.continuous)
        {
            for(int rod = 0; rod < 4; rod ++)
            {
                // Apply the agent action forces for this rod
                var linear = force_factor * Mathf.Clamp(vectorAction[rod], -1f, 1f);
                var torque = torque_factor * Mathf.Clamp(vectorAction[rod+4], -1f, 1f);
                rods[rod].GetComponent<Rigidbody>().AddRelativeTorque(new Vector3(0.0f, 0.0f, torque), mode);
                rods[rod].GetComponent<Rigidbody>().AddRelativeForce(new Vector3(0.0f, 0.0f, linear), mode);
            }
        }

        // Set the reward
        SetReward(tm.player_rewards[player]);
    }

    public override void AgentReset()
    {
        GetComponent<TableManager>().Reset();
    }


}
