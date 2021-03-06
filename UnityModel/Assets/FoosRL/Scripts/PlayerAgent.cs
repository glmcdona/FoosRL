﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using System.Linq;

public class PlayerAgent : Agent
{
    public int player = -1;

    public int DiscreteActionLevels = 3;
    public float TimeBetweenDecisionsAtInference = 0.01f;
    private float m_TimeSinceDecision = 0.0f;
    public Camera RenderCamera;

    public bool ControlledByKeys = false;
    public bool VisionOnly = false;
    public bool EndGameOnMaxGoals = true;
    public bool ApplyForceVerticalRods = true;
    public bool AgentControlPosition = true;

    //private float PlayerContactReward = 0.0f;
    private Vector3 BallLastDirection = new Vector3(0.0f,0.0f,0.0f);


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
    public int max_goals = 10;
    public int goals = 0;
    public int SelectedRod = 0;
    public int LastSelectedRod = 0;

    public ForceMode mode = ForceMode.Force;

    private TableManager tm = null;
    private PlayerAgent opponent = null;
    private GameObject goal = null;
    private Bounds play_area;  // x: table_inner_length, y: table_inner_wall_height, z: table_inner_width
    private Vector3 direction_of_play;

    // Use this for initialization
    void Start () {
        // Load the table manager
        tm = transform.parent.GetComponent<TableManager>();
        opponent = tm.PlayerAgents[Mathf.Abs(player - 1)];
        goal = tm.Goals[player];
        play_area = tm.PlayArea;
        if (player == 0)
            direction_of_play = new Vector3(1.0f, 0.0f, 0.0f);
        else
            direction_of_play = new Vector3(-1.0f, 0.0f, 0.0f);
    }
    
    
    public void SetCamera(Camera camera)
    {
        // Load this camera to this agent
        this.RenderCamera = camera;
    }

    public override void InitializeAgent()
    {
        // Nothing.
    }

    public void FixedUpdate()
    {
        WaitTimeInference();
    }

    void WaitTimeInference()
    {
        //if (RenderCamera != null)
        //{
        //    RenderCamera.Render();
        //}

        if (Academy.Instance.IsCommunicatorOn)
        {
            RequestDecision();
        }
        else
        {
            if (m_TimeSinceDecision >= TimeBetweenDecisionsAtInference)
            {
                m_TimeSinceDecision = 0f;
                RequestDecision();
            }
            else
            {
                m_TimeSinceDecision += Time.fixedDeltaTime;
            }
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //if (VisionOnly)
        //{
            /*
            // No observations, just the camera as input

            // Basic feature set (32+6=38 floats)

            // Ball coordinates
            AddVectorObs(NormalizePositionToPlayArea(tm.BallsGo[0].transform.position));

            // Ball velocity
            AddVectorObs(NormalizeVelocity(tm.BallsGo[0].GetComponent<Rigidbody>().velocity * 10.0f));
            
            //Debug.Log( NormalizeVelocity(tm.BallsGo[0].GetComponent<Rigidbody>().velocity * 10.0f));

            // Current player rods
            //  x -> direction of play
            //  y -> vertical
            //  z -> direction of rods
            foreach (GameObject rod in rods)
            {
                // Position
                AddVectorObs(NormalizePositionToPlayArea(rod.GetComponent<Rigidbody>().transform.position).z * 3.0f);
                //if( player == 0 && rod == rods[0])
                //    Debug.Log(NormalizePositionToPlayArea(rod.GetComponent<Rigidbody>().transform.position));
                AddVectorObs(NormalizeVelocity(rod.GetComponent<Rigidbody>().velocity).z * 5.0f);


                // Rod Angle
                float angle = Vector3.SignedAngle(rod.transform.up, new Vector3(0.0f, 1.0f, 0.0f), rod.transform.forward);
                AddVectorObs(NormalizeAngle(angle));
                //if (player == 0 && rod == rods[0])
                //    Debug.Log( NormalizeAngle(angle) );

                AddVectorObs(NormalizeAngularVelocity(rod.GetComponent<Rigidbody>().angularVelocity, 5.0f).z);
                //if (player == 0 && rod == rods[0])
                //    Debug.Log(NormalizeAngularVelocity(rod.GetComponent<Rigidbody>().angularVelocity, 5.0f).z);
            }

            // Opponent player rods
            foreach (GameObject rod in opponent.rods)
            {
                // Position
                AddVectorObs(NormalizePositionToPlayArea(rod.GetComponent<Rigidbody>().transform.position).z * 3.0f);
                AddVectorObs(NormalizeVelocity(rod.GetComponent<Rigidbody>().velocity).z * 5.0f);

                // Rod Angle
                float angle = Vector3.SignedAngle(rod.transform.up, new Vector3(0.0f, 1.0f, 0.0f), rod.transform.forward);
                AddVectorObs(NormalizeAngle(angle));
                AddVectorObs(NormalizeAngularVelocity(rod.GetComponent<Rigidbody>().angularVelocity, 5.0f).z);
            }

            // Nearest ball position with respect to each current player physical rod players
            foreach (GameObject player in players)
            {
                Vector2 RelPosition2d_min = Vector2.positiveInfinity;

                foreach (GameObject ball in tm.Balls.Keys)
                {
                    Vector3 RelPosition = ball.transform.position - player.transform.position;
                    Vector2 RelPosition2d = new Vector2(
                            RelPosition.x / play_area.size.x,
                            RelPosition.z / play_area.size.z
                        );

                    if (RelPosition2d.magnitude < RelPosition2d_min.magnitude)
                        RelPosition2d_min = RelPosition2d;
                }

                // Add the player position to ball location vector
                AddVectorObs(NormalizeLogistic(RelPosition2d_min * 10.0f)); // 10x resolution scaling so it gets detailed when the ball is close to each player on each axis
            }
            */
        //}
        //else
        //{
            // Features:
            //  2d map of where the ball(s) are
            //  2d map of where your player are
            //  2d map of where opponent players are
            //  float[8] position of each rod
            //  float[8] angular velocity of each rod
            //  float[8] velocity of each rod

            // To form the 2d maps, this outputs an array of Vector2 for each object. The python code is expected to build this into the map.
            // Build the 2d map of where the ball is

        //}
        /*
        else
        {
            // Add both player rod positions, angles, speeds, and ball position as features

            // Ball coordinates
            //AddVectorObs(NormalizePositionToPlayArea(tm.ball.transform.position));
            //Debug.Log(NormalizePositionToPlayArea(tm.ball.transform.position));

            // Ball velocity
            //AddVectorObs(NormalizeVelocity(tm.ball.GetComponent<Rigidbody>().velocity * 10.0f));
            //Debug.Log( NormalizeVelocity(tm.ball.GetComponent<Rigidbody>().velocity * 10.0f));

            // Current player rods
            //  x -> direction of play
            //  y -> vertical
            //  z -> direction of rods
            foreach (GameObject rod in rods)
            {
                // Position
                AddVectorObs(NormalizePositionToPlayArea(rod.GetComponent<Rigidbody>().transform.position).z * 3.0f);
                //if( player == 0 && rod == rods[0])
                //    Debug.Log(NormalizePositionToPlayArea(rod.GetComponent<Rigidbody>().transform.position));
                AddVectorObs(NormalizeVelocity(rod.GetComponent<Rigidbody>().velocity).z * 5.0f);


                // Rod Angle
                float angle = Vector3.SignedAngle(rod.transform.up, new Vector3(0.0f, 1.0f, 0.0f), rod.transform.forward);
                AddVectorObs(NormalizeAngle(angle));
                //if (player == 0 && rod == rods[0])
                //    Debug.Log( NormalizeAngle(angle) );

                AddVectorObs(NormalizeAngularVelocity(rod.GetComponent<Rigidbody>().angularVelocity, 5.0f).z);
                //if (player == 0 && rod == rods[0])
                //    Debug.Log(NormalizeAngularVelocity(rod.GetComponent<Rigidbody>().angularVelocity, 5.0f).z);
            }

            // Opponent player rods
            foreach (GameObject rod in opponent.rods)
            {
                // Position
                AddVectorObs(NormalizePositionToPlayArea(rod.GetComponent<Rigidbody>().transform.position).z * 3.0f);
                AddVectorObs(NormalizeVelocity(rod.GetComponent<Rigidbody>().velocity).z * 5.0f);

                // Rod Angle
                float angle = Vector3.SignedAngle(rod.transform.up, new Vector3(0.0f, 1.0f, 0.0f), rod.transform.forward);
                AddVectorObs(NormalizeAngle(angle));
                AddVectorObs(NormalizeAngularVelocity(rod.GetComponent<Rigidbody>().angularVelocity, 5.0f).z);
            }

            // Nearest ball position with respect to each current player physical rod players
            foreach (GameObject player in players)
            {
                Vector2 RelPosition2d_min = Vector2.positiveInfinity;

                foreach(GameObject ball in tm.Balls.Keys)
                {
                    Vector3 RelPosition = ball.transform.position - player.transform.position;
                    Vector2 RelPosition2d = new Vector2(
                            RelPosition.x / play_area.size.x,
                            RelPosition.z / play_area.size.z
                        );

                    if (RelPosition2d.magnitude < RelPosition2d_min.magnitude)
                        RelPosition2d_min = RelPosition2d;
                }

                // Add the player position to ball location vector
                AddVectorObs(NormalizeLogistic(RelPosition2d_min * 10.0f)); // 10x resolution scaling so it gets detailed when the ball is close to each player on each axis
            }

            // Total:
            // 3*3 + 4 * 4 + 4 * 4 = 41 float observations
        }
        */
    }


    // Random heuristic
    private float timeInState = 10.0f;
    private float[] state = new float[8] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, };
    public override float[] Heuristic()
    {
        // Random actions
        // 0-3: Linear rods 0 to 3
        // 4-7: Torque rods 0 to 3
        timeInState += Time.deltaTime;
        if (timeInState > 0.3)
        {
            state = new float[8] {
                (Random.value - 0.5f) * 2.0f,
                (Random.value - 0.5f) * 2.0f,
                (Random.value - 0.5f) * 2.0f,
                (Random.value - 0.5f) * 2.0f,
                (Random.value - 0.5f) * 2.0f,
                (Random.value - 0.5f) * 2.0f,
                (Random.value - 0.5f) * 2.0f,
                (Random.value - 0.5f) * 2.0f
            };
            timeInState = 0.0f;
        }
        return state;
    }

    public override void AgentAction(float[] vectorAction)
    {
        // Take the actions. Action format is continuous:
        // 0-3: Linear rods 0 to 3
        // 4-7: Torque rods 0 to 3
        for (int rod = 0; rod < 4; rod++)
        {
            // Apply the agent action forces for this rod
            var linear = force_factor * Mathf.Clamp(vectorAction[rod], -1f, 1f);
            var torque = torque_factor * Mathf.Clamp(vectorAction[rod + 4], -1f, 1f);

            rods[rod].GetComponent<Rigidbody>().AddRelativeTorque(new Vector3(0.0f, 0.0f, torque), mode);
            rods[rod].GetComponent<Rigidbody>().AddRelativeForce(new Vector3(0.0f, 0.0f, linear), mode);
        }

        //// 0-3: Linear rods 0 to 3
        ////      0: push hardest. 1 push moderate, 2 push slowly, 3 nothing, 4 push slowly, 5 push moderate, 6, push hard
        //// 4-7: Torque rods 0 to 3
        ////      0: nothing. 1 cw, DiscreteActionLevels+1 ccw
        //for (int rod = 0; rod < 4; rod++)
        //{
        //    // Apply the agent action forces for this rod
        //    var linear = force_factor * (float)(vectorAction[rod] - (this.DiscreteActionLevels)) / (float)this.DiscreteActionLevels;
        //    var torque = force_factor * (float)(vectorAction[rod + 4] - (this.DiscreteActionLevels)) / (float)this.DiscreteActionLevels;
        //    rods[rod].GetComponent<Rigidbody>().AddRelativeTorque(new Vector3(0.0f, 0.0f, torque), mode);
        //    rods[rod].GetComponent<Rigidbody>().AddRelativeForce(new Vector3(0.0f, 0.0f, linear), mode);
        //}

        // Second attempt at descrete control. Can only control one rod at a time!
        // Rod selection + discrete movement controls

        /*
        // Apply the agent action forces for this rod
        var selected_rod = Mathf.FloorToInt((Mathf.Clamp(vectorAction[0], -1, 1)/2.0f + 0.5f)*4.0f); // 0 to 3 to select which rod to control
        if (selected_rod > 3)
            selected_rod = 3;
        if (selected_rod < 0)
            selected_rod = 0;
        //var linear = force_factor * (float)(vectorAction[1] - (this.DiscreteActionLevels*2.0f-1f)/2.0f) / (float)(this.DiscreteActionLevels * 2.0f - 1f) / 2.0f;
        //var torque = force_factor * (float)(vectorAction[2] - (this.DiscreteActionLevels * 2.0f - 1f) / 2.0f) / (float)(this.DiscreteActionLevels * 2.0f - 1f) / 2.0f;
        var linear = force_factor * Mathf.Clamp(vectorAction[1], -1f, 1f);
        var torque = torque_factor * Mathf.Clamp(vectorAction[2], -1f, 1f);
        rods[selected_rod].GetComponent<Rigidbody>().AddRelativeTorque(new Vector3(0.0f, 0.0f, torque), mode);
        rods[selected_rod].GetComponent<Rigidbody>().AddRelativeForce(new Vector3(0.0f, 0.0f, linear), mode);
        
        if (selected_rod != LastSelectedRod)
        {
            // Highlight the newly selected rod
            rods[selected_rod].transform.Find("Rod").GetComponent<Renderer>().material = tm.MaterialRodSelected;
            rods[selected_rod].transform.Find("Rod").GetComponent<Renderer>().material.color = Color.blue;

            // Unhiglight the old rod
            rods[LastSelectedRod].transform.Find("Rod").GetComponent<Renderer>().material = tm.MaterialRodNormal;
            rods[LastSelectedRod].transform.Find("Rod").GetComponent<Renderer>().material.color = Color.grey;

            // Apply a penalty for a rod switch
            //AddReward(-1f / 20000.0f * 20.0f); // 20x the timestep penalty
            //total_reward += -1f / 20000.0f * 20.0f;
        }
        LastSelectedRod = SelectedRod;
        SelectedRod = selected_rod;

        if (this.ApplyForceVerticalRods)
        {
            // Apply force to each rod to make them vertical
            foreach (GameObject rod in rods)
            {
                // Rod Angle
                float angle = Vector3.SignedAngle(rod.transform.up, new Vector3(0.0f, 1.0f, 0.0f), rod.transform.forward);
                rod.GetComponent<Rigidbody>().AddRelativeTorque(
                    new Vector3(0.0f, 0.0f, (angle / 700.0f - rod.GetComponent<Rigidbody>().angularVelocity.z / 10000.0f) * force_factor)
                    , mode);
            }
        }
        */

        // Tiny penalty for time passing
        AddReward((-1f / 20000.0f) / 5.0f);
        total_reward += (-1f / 20000.0f) / 5.0f;
    }

    public void Goal()
    {
        // 1.0 for scoring
        AddReward(0.2f);
        total_reward += 0.2f;
        last_distance_goal_reward = 0.0f;
        goals++;
        if (goals >= max_goals && EndGameOnMaxGoals)
        {
            goals = 0;
            Done();
        }
    }

    public void GoalAgainst()
    {
        // -1.0 for being scored against
        AddReward(-0.2f);
        total_reward += -0.2f;
        last_distance_goal_reward = 0.0f;
        goals++;
        if (goals >= max_goals && EndGameOnMaxGoals)
        {
            goals = 0;
            Done();
        }
    }

    public void PenaltyTimeExceeded()
    {
        // Penalty is current ball distance penalty against the player or minimum of -0.05
        float penalty = 0.0f;
        penalty = last_distance_goal_reward - 0.05f;
        if (penalty > -0.05f) // Minimum 0.05 penalty for exceeding 10 seconds
            penalty = -0.05f;


        //AddReward(penalty);
        //opponent.AddReward(-penalty);
        //total_reward += penalty;
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


    public override void AgentReset()
    {
        goals = 0;
        transform.parent.GetComponent<TableManager>().ResetGame();
        BallLastDirection = Vector3.zero;
    }


}
