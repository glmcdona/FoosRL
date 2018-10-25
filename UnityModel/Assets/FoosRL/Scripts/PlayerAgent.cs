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
    public float last_distance_goal_reward = 0.0f;
    public float total_reward = 0.0f;

    public ForceMode mode = ForceMode.Force;

    private TableManager tm = null;
    private PlayerAgent opponent = null;
    private GameObject goal = null;

    // Use this for initialization
    void Start () {
        // Load the table manager
        tm = GetComponent<TableManager>();
        opponent = tm.player_agents[Mathf.Abs(player - 1)];
        goal = tm.goals[player];
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

        // Add both player rod positions, angles, speeds, and ball position as features

        // Ball position with respect to the center of the table
        float denominator = 2.0f * Vector3.Distance(tm.ball_drop_source.transform.position, goal.transform.position);
        AddVectorObs( (tm.ball_drop_source.transform.position - tm.ball.transform.position) / denominator);

        // Ball position with respect to my goal
        AddVectorObs( (goal.transform.position - tm.ball.transform.position) / denominator);

        // Ball velocity
        AddVectorObs( tm.ball.GetComponent<Rigidbody>().velocity / denominator);

        // Current player rods
        //  x -> direction of play
        //  y -> vertical
        //  z -> direction of rods
        foreach (GameObject rod in rods)
        {
            // Position
            AddVectorObs( (rod.GetComponent<Rigidbody>().position.z - tm.ball_drop_source.transform.position.z) / denominator );
            AddVectorObs( rod.GetComponent<Rigidbody>().velocity.z / denominator );

            // Rod Angle
            AddVectorObs( rod.GetComponent<Rigidbody>().rotation.eulerAngles.z / 180.0f );
            AddVectorObs( rod.GetComponent<Rigidbody>().angularVelocity.z / 50.0f );
        }

        // Opponent player rods
        foreach (GameObject rod in opponent.rods)
        {
            // Position
            AddVectorObs( (rod.GetComponent<Rigidbody>().position.z - tm.ball_drop_source.transform.position.z) / denominator );
            AddVectorObs( rod.GetComponent<Rigidbody>().velocity.z / denominator );

            // Rod Angle
            AddVectorObs( rod.GetComponent<Rigidbody>().rotation.eulerAngles.z / 180.0f );
            AddVectorObs( rod.GetComponent<Rigidbody>().angularVelocity.z / 50.0f);
        }
        
        // Total:
        // 3*3 + 4 * 4 + 4 * 4 = 41 float observations
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

        // Calculate the ball position reward

        // Give a reward depending on who has the ball
        /*
        float[] rewards = new float[]
        {
            0.10f, // At goalie (10% from here)
            0.10f, // At defence (10% from here)
            0.25f, // At 5-bar (25% from here)
            0.40f, // At 3-bar offense (40% chance of scoring from here)
        };
        */

        /*
        // Simpler reward structure, send ball towards opponents net!
        float[] rewards = new float[]
        {
            -0.10f, // At goalie (10% from here)
            -0.10f, // At defence (10% from here)
            0.00f, // At 5-bar (25% from here)
            0.10f, // At 3-bar offense (40% chance of scoring from here)
        };
        */

        //float reward = (LastPlayerWithBall == 0 ? 1.0f : -1.0f) * rewards[LastRodWithBall];
        //_player_rewards_currentball[0] = reward;
        //_player_rewards_currentball[1] = -reward;

        // Even simpler reward! Reward is distance to goal.
        float proximal_reward = 0.3f;
        float denominator = 2.0f * Vector3.Distance(tm.ball_drop_source.transform.position, opponent.goal.transform.position);
        float distance_goal_opponent = Vector3.Distance(tm.ball.transform.position, opponent.goal.transform.position);
        float distance_goal_mine = Vector3.Distance(tm.ball.transform.position, goal.transform.position);
        float distance_goal_reward = proximal_reward - proximal_reward * distance_goal_opponent / denominator
                        - (proximal_reward - proximal_reward * distance_goal_mine / denominator);

        // Add the reward delta
        AddReward(distance_goal_reward - last_distance_goal_reward);
        total_reward += distance_goal_reward - last_distance_goal_reward;

        // Update last ball position reward
        last_distance_goal_reward = distance_goal_reward;

        // Tiny penalty for time passing
        AddReward(-0.1f / agentParameters.maxStep);
    }

    public void Goal()
    {
        // 1.0 for scoring
        AddReward(1.0f);
        total_reward += 1.0f;
        Done();
    }

    public void GoalAgainst()
    {
        // -1.0 for being scored against
        AddReward(-1.0f);
        total_reward += -1.0f;
        Done();
    }

    public void PenaltyTimeExceeded()
    {
        // 0.05 penalty for exceeding 10 seconds
        AddReward(-0.05f);
        total_reward += 0.05f;
    }

    public override void AgentReset()
    {
        GetComponent<TableManager>().ResetGame();
    }


}
