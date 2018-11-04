using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class PlayerAgent : Agent
{
    public int player = -1;

    public bool ControlledByKeys = false;
    public bool VisionOnly = false;

    // Rods order:
    //   0: Goalie
    //   1: Defense
    //   2: 5-bar
    //   3: 3-bar
    public List<GameObject> rods = new List<GameObject>(4);
    public List<GameObject> players = new List<GameObject>(13);
    public float torque_factor = 3000.0f;
    public float force_factor = 20000.0f;
    public float last_distance_goal_reward = 0.0f;
    public float total_reward = 0.0f;

    public ForceMode mode = ForceMode.Force;

    private TableManager tm = null;
    private PlayerAgent opponent = null;
    private GameObject goal = null;
    private Bounds play_area;  // x: table_inner_length, y: table_inner_wall_height, z: table_inner_width

    // Use this for initialization
    void Start () {
        // Load the table manager
        tm = GetComponent<TableManager>();
        opponent = tm.player_agents[Mathf.Abs(player - 1)];
        goal = tm.goals[player];
        play_area = tm.play_area;
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
        if (VisionOnly)
        {
            // No observations, just the camera as input
        }
        else
        {
            // Add both player rod positions, angles, speeds, and ball position as features

            // Ball coordinates
            AddVectorObs( NormalizePositionToPlayArea(tm.ball.transform.position) );
            //Debug.Log(NormalizePositionToPlayArea(tm.ball.transform.position));

            // Ball velocity
            AddVectorObs( NormalizeVelocity(tm.ball.GetComponent<Rigidbody>().velocity * 10.0f ) );
            //Debug.Log( NormalizeVelocity(tm.ball.GetComponent<Rigidbody>().velocity * 10.0f));

            // Current player rods
            //  x -> direction of play
            //  y -> vertical
            //  z -> direction of rods
            foreach (GameObject rod in rods)
            {
                // Position
                AddVectorObs( NormalizePositionToPlayArea(rod.GetComponent<Rigidbody>().transform.position).z * 3.0f );
                //if( player == 0 && rod == rods[0])
                //    Debug.Log(NormalizePositionToPlayArea(rod.GetComponent<Rigidbody>().transform.position));
                AddVectorObs( NormalizeVelocity(rod.GetComponent<Rigidbody>().velocity).z * 5.0f );


                // Rod Angle
                float angle = Vector3.SignedAngle(rod.transform.up, new Vector3(0.0f, 1.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f));
                AddVectorObs( NormalizeAngle(angle) );
                //if (player == 0 && rod == rods[0])
                //    Debug.Log( NormalizeAngle(angle) );
                
                AddVectorObs( NormalizeAngularVelocity(rod.GetComponent<Rigidbody>().angularVelocity, 50.0f ).z );
                // TODO: Validate
            }

            // Opponent player rods
            foreach (GameObject rod in opponent.rods)
            {
                // Position
                AddVectorObs(NormalizePositionToPlayArea(rod.GetComponent<Rigidbody>().transform.position).z * 3.0f );
                AddVectorObs(NormalizeVelocity(rod.GetComponent<Rigidbody>().velocity).z * 5.0f );

                // Rod Angle
                AddVectorObs(NormalizeAngle(rod.GetComponent<Rigidbody>().rotation.eulerAngles).z);
                AddVectorObs(NormalizeAngularVelocity(rod.GetComponent<Rigidbody>().angularVelocity, 50.0f).z);
            }

            // Ball position with respect to each current player physical rod players
            foreach (GameObject player in players)
            {
                Vector3 RelPosition = tm.ball.transform.position - player.transform.position;
                Vector2 RelPosition2d = new Vector2(
                        RelPosition.x / play_area.size.x,
                        RelPosition.z / play_area.size.z
                    );
                
                // Add the player position to ball location vector
                AddVectorObs( NormalizeLogistic(RelPosition2d * 10.0f ) ); // 10x resolution scaling so it gets detailed when the ball is close to each player on each axis
            }

            // Total:
            // 3*3 + 4 * 4 + 4 * 4 = 41 float observations
        }
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

    private float NormalizeWidth(float width)
    {
        // x: table_inner_length, y: table_inner_wall_height, z: table_inner_width
        return Mathf.Clamp(width / play_area.size.z, 0f, 1f);
    }

    private float NormalizeHeight(float height)
    {
        // x: table_inner_length, y: table_inner_wall_height, z: table_inner_width
        return Mathf.Clamp(height / play_area.size.y, 0f, 1f);
    }

    private float NormalizeLength(float length)
    {
        // x: table_inner_length, y: table_inner_wall_height, z: table_inner_width
        return Mathf.Clamp(length / play_area.size.x, 0f, 1f);
    }

    private Vector3 NormalizePositionToPlayArea(Vector3 coord)
    {
        // x: table_inner_length, y: table_inner_wall_height, z: table_inner_width
        Vector3 RelPlayArea = coord - play_area.center;

        return new Vector3(
                Mathf.Clamp(RelPlayArea.x / play_area.size.x, -1f, 1f),  // Now normalized on range of -1.0 to 1.0 for play area range
                Mathf.Clamp(RelPlayArea.y / play_area.size.y, -1f, 1f),
                Mathf.Clamp(RelPlayArea.z / play_area.size.z, -1f, 1f)
            );
    }

    private Vector3 NormalizeAngle(Vector3 eulerAngles)
    {
        // x: table_inner_length, y: table_inner_wall_height, z: table_inner_width
        Vector3 Normalized = eulerAngles / 180.0f;

        return new Vector3(
                Mathf.Clamp(Normalized.x - 1.0f, -1f, 1f),  // Now normalized on range of -1.0 to 1.0 for play area range
                Mathf.Clamp(Normalized.y - 1.0f, -1f, 1f),
                Mathf.Clamp(Normalized.z - 1.0f, -1f, 1f)
            );
    }

    private float NormalizeAngle(float angle)
    {
        // x: table_inner_length, y: table_inner_wall_height, z: table_inner_width
        return Mathf.Clamp(angle / 180.0f, -1f, 1f);
    }

    private Vector3 NormalizeVelocity(Vector3 velocity)
    {
        // x: table_inner_length, y: table_inner_wall_height, z: table_inner_width

        // Normalize so +-1 is one length of the table
        Vector3 RelPlayAreaSize = new Vector3(
                velocity.x / play_area.size.x,
                velocity.y / play_area.size.y,
                velocity.z / play_area.size.z
            );

        // Map through a sigmoid function to limit it to -1f to 1f.
        return new Vector3(
                2.0f / (1.0f + Mathf.Exp(-RelPlayAreaSize.x)) - 1.0f,
                2.0f / (1.0f + Mathf.Exp(-RelPlayAreaSize.y)) - 1.0f,
                2.0f / (1.0f + Mathf.Exp(-RelPlayAreaSize.z)) - 1.0f
            );
    }

    private Vector3 NormalizeLogistic(Vector3 input)
    {
        // Map through a sigmoid function to limit it to -1f to 1f.
        return new Vector3(
                2.0f / (1.0f + Mathf.Exp(-input.x)) - 1.0f,
                2.0f / (1.0f + Mathf.Exp(-input.y)) - 1.0f,
                2.0f / (1.0f + Mathf.Exp(-input.z)) - 1.0f
            );
    }

    private Vector2 NormalizeLogistic(Vector2 input)
    {
        // Map through a sigmoid function to limit it to -1f to 1f.
        return new Vector2(
                2.0f / (1.0f + Mathf.Exp(-input.x)) - 1.0f,
                2.0f / (1.0f + Mathf.Exp(-input.y)) - 1.0f
            );
    }

    private Vector3 NormalizeAngularVelocity(Vector3 velocity_rads, float factor)
    {
        // x: table_inner_length, y: table_inner_wall_height, z: table_inner_width

        // Map through a sigmoid function to limit it to -1f to 1f.
        return new Vector3(
                2.0f / (1.0f + Mathf.Exp(-velocity_rads.x / factor)) - 1.0f,
                2.0f / (1.0f + Mathf.Exp(-velocity_rads.y / factor)) - 1.0f,
                2.0f / (1.0f + Mathf.Exp(-velocity_rads.z / factor)) - 1.0f
            );
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
        // Penalty is current ball distance penalty against the player or minimum of -0.05
        float penalty = 0.0f;
        penalty = last_distance_goal_reward - 0.05f;
        if (penalty > -0.05f) // Minimum 0.05 penalty for exceeding 10 seconds
            penalty = -0.05f;

        
        AddReward(penalty);
        total_reward += penalty;
    }

    public override void AgentReset()
    {
        GetComponent<TableManager>().ResetGame();
    }


}
