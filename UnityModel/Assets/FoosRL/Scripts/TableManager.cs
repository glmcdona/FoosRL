using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableManager : MonoBehaviour {

    
    public GameObject ball;
    public GameObject ball_drop_source;
    public Bounds play_area; // x: table_inner_length, y: table_inner_wall_height, z: table_inner_width

    public GameObject[] goals = new GameObject[] {null, null};
    public PlayerAgent[] player_agents = new PlayerAgent[] { null, null };

    public int LastPlayerWithBall = -1;
    public int LastRodWithBall = -1;
    public float TimeHeldByPlayer = 0.0f;

    public bool DropRandomLocation = true;

    // Agents
    public PlayerAgent[] agents;
    
    private bool[,] _player_rod_hasball = new bool[2, 4] { { false, false, false, false }, { false, false, false, false } };

    // Use this for initialization
    void Start () {
        ResetGame();
    }
	
	// Update is called once per frame
	void Update () {
        TimeHeldByPlayer += Time.deltaTime;

        if(TimeHeldByPlayer > 10.0f)
        {
            BallStuck();
        }
    }


    public void BallStuck()
    {
        Debug.Log("Ball is stuck.");
        if(LastPlayerWithBall == -1)
        {
            // Reset giving it to random player, no penalty
            ResetGame();
        }
        else
        {
            // Small penalty
            player_agents[LastPlayerWithBall].PenaltyTimeExceeded();
            // note: no reward for opponent, only the penalty

            // Give ball to opponent at the 5-bar
            ResetGame(Mathf.Abs(LastPlayerWithBall - 1));
        }
    }

    // Callback for when a goal is made
    public void BallEnterGoal(int player)
    {
        Debug.Log("Goal!");

        // Count the goal rewards
        player_agents[player].GoalAgainst();
        player_agents[Mathf.Abs(player - 1)].Goal();

        // Reset, give the ball to the opponent
        ResetGame(Mathf.Abs(player - 1));
    }

    // Callback for tracking which rods have the ball
    public void BallEnterRod(int player, int rod_index)
    {
        if (LastPlayerWithBall != player)
            TimeHeldByPlayer = 0.0f;

        LastPlayerWithBall = player;
        LastRodWithBall = rod_index;
        _player_rod_hasball[player, rod_index] = true;
    }

    public void BallExitRod(int player, int rod_index)
    {
        _player_rod_hasball[player, rod_index] = false;
    }

    // Ball exit play area
    public void BallExitPlay()
    {
        Debug.Log("Ball exited play.");
        ResetGame();
    }

    // Reset
    public void ResetGame( int? give_to_player = null )
    {
        if (!DropRandomLocation)
        {
            if (give_to_player == null)
            {
                // Give the ball to a random player
                give_to_player = UnityEngine.Random.value > 0.5f ? 1 : 0;
            }
            Debug.Log("Giving to player " + give_to_player.ToString());

            // Give the ball to the specified player
            if (give_to_player == 0)
                ball.transform.position = ball_drop_source.transform.position - gameObject.transform.right * 1.5f * ball.transform.localScale.x;
            else
                ball.transform.position = ball_drop_source.transform.position + gameObject.transform.right * 1.5f * ball.transform.localScale.x;

            // Reset ball speed
            ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
            ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }
        else
        {
            // Drop the ball in a random position on the table
            float delta = 0.4f;
            ball.transform.position = new Vector3(
                    UnityEngine.Random.Range(play_area.min.x + delta, play_area.max.x - delta),
                    UnityEngine.Random.Range(play_area.min.y + delta, play_area.max.y - delta),
                    UnityEngine.Random.Range(play_area.min.z + delta, play_area.max.z - delta)
                );

            // Assign random velocity
            ball.GetComponent<Rigidbody>().velocity = new Vector3(
                    UnityEngine.Random.Range(-3f, 3f),
                    UnityEngine.Random.Range(-3f, 3f),
                    UnityEngine.Random.Range(-3f, 3f)
                );
            ball.GetComponent<Rigidbody>().angularVelocity = new Vector3(
                    UnityEngine.Random.Range(-3f, 3f),
                    UnityEngine.Random.Range(-3f, 3f),
                    UnityEngine.Random.Range(-3f, 3f)
                );
        }
        
        // Reset time held counter
        TimeHeldByPlayer = 0.0f;
    }
}
