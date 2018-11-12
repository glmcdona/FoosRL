using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball
{
    public GameObject BallGameObject;
    public GameObject BallDropSource;

    public int LastPlayerWithBall = -1;
    public int LastRodWithBall = -1;
    public float TimeHeldByPlayer = 0.0f;
    private TableManager _tm;

    public Ball(GameObject ball, TableManager tm)
    {
        BallGameObject = ball;
        _tm = tm;
    }

    public void Update()
    {
        TimeHeldByPlayer += Time.deltaTime;

        if (TimeHeldByPlayer > 10.0f)
        {
            BallStuck();
        }
    }

    public void BallEnterRod(int player, int rod_index)
    {
        if (LastPlayerWithBall != player)
            TimeHeldByPlayer = 0.0f;

        LastPlayerWithBall = player;
        LastRodWithBall = rod_index;
    }

    public void BallExitRod(int player, int rod_index)
    {

    }

    public void BallStuck()
    {
        if (LastPlayerWithBall == -1)
        {
            // Reset giving it to random player, no penalty
            ResetBall();
        }
        else
        {
            // Small penalty
            _tm.PenaltyTimeExceeded(LastPlayerWithBall);

            // Give ball to opponent at the 5-bar
            ResetBall(Mathf.Abs(LastPlayerWithBall - 1));
        }
    }


    public void ResetBall(int? give_to_player = null)
    {
        // Reset time held counter
        TimeHeldByPlayer = 0.0f;

        // Ask the table to reset the ball
        _tm.ResetBall(BallGameObject, give_to_player);
    }

    
}


public class TableManager : MonoBehaviour {


    public Dictionary<GameObject, Ball> Balls = new Dictionary<GameObject, Ball>(5);
    public GameObject BallDropSource;
    public Bounds PlayArea; // x: table_inner_length, y: table_inner_wall_height, z: table_inner_width

    public GameObject[] Goals = new GameObject[] {null, null};
    public PlayerAgent[] PlayerAgents = new PlayerAgent[] { null, null };

    public int LastPlayerWithBall = -1;
    public int LastRodWithBall = -1;
    public float TimeHeldByPlayer = 0.0f;

    public bool DropRandomLocation = true;
    

    // Use this for initialization
    void Start () {
        ResetGame();
    }

    public void ResetGame()
    {
        // Reposition all balls
        foreach (Ball ball in Balls.Values)
        {
            ball.ResetBall();
        }
    }
	
	// Update is called once per frame
	void Update () {
        foreach( Ball ball in Balls.Values)
        {
            ball.Update();
        }
    }
    
    public void PenaltyTimeExceeded(int playerWhoHadBall)
    {
        // note: no reward for opponent, only the penalty
        PlayerAgents[playerWhoHadBall].PenaltyTimeExceeded();
    }

    // Reset this ball
    public void ResetBall(GameObject ball, int? give_to_player = null)
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
                ball.transform.position = BallDropSource.transform.position - gameObject.transform.right * 1.5f * ball.transform.localScale.x;
            else
                ball.transform.position = BallDropSource.transform.position + gameObject.transform.right * 1.5f * ball.transform.localScale.x;

            // Reset ball speed
            ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
            ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }
        else
        {
            // Drop the ball in a random position on the table
            float delta = 0.4f;
            ball.transform.position = new Vector3(
                    UnityEngine.Random.Range(PlayArea.min.x + delta, PlayArea.max.x - delta),
                    UnityEngine.Random.Range(PlayArea.min.y + delta, PlayArea.max.y - delta),
                    UnityEngine.Random.Range(PlayArea.min.z + delta, PlayArea.max.z - delta)
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
    }

    // Callback for when a goal is made
    public void BallEnterGoal(GameObject ball, int player)
    {
        Debug.Log("Goal!");

        // Count the goal rewards
        PlayerAgents[player].GoalAgainst();
        PlayerAgents[Mathf.Abs(player - 1)].Goal();

        // Reset the ball
        Balls[ball].ResetBall(Mathf.Abs(player - 1));
    }

    // Callback for tracking which rods have the ball
    public void BallEnterRod(GameObject ball, int player, int rod_index)
    {
        Balls[ball].BallEnterRod(player, rod_index);
    }

    public void BallExitRod(GameObject ball, int player, int rod_index)
    {
        Balls[ball].BallExitRod(player, rod_index);
    }

    // Ball exit play area
    public void BallExitPlay(GameObject ball)
    {
        Debug.Log("Ball exited play.");
        Balls[ball].ResetBall();
    }

    
}
