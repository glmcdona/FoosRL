using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableManager : MonoBehaviour {

    
    public GameObject ball;
    public GameObject ball_drop_source;
    public GameObject goal1;
    public GameObject goal2;

    public PlayerAgent player1_manager;
    public PlayerAgent player2_manager;

    public int LastPlayerWithBall = -1;
    public int LastRodWithBall = -1;
    public float TimeHeldByPlayer = 0.0f;

    // Agents
    public PlayerAgent[] agents;

    // Total player reward
    public float[] player_rewards = new float[] { 0.0f, 0.0f };

    // Total reward from scores made
    public float[] _player_rewards_fromscores = new float[] { 0.0f, 0.0f };

    // Delta reward based on current ball state
    public float[] _player_rewards_currentball = new float[] { 0.0f, 0.0f };

    


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
            _player_rewards_fromscores[LastPlayerWithBall] -= 0.05f;
            _player_rewards_fromscores[Mathf.Abs(LastPlayerWithBall - 1)] += 0.05f;

            // Give ball to opponent at the 5-bar
            ResetGame(Mathf.Abs(LastPlayerWithBall - 1));
        }
    }

    // Callback for when a goal is made
    public void BallEnterGoal(int player)
    {
        Debug.Log("Goal!");

        // Count the rewards
        _player_rewards_fromscores[player] += 1.0f;
        _player_rewards_fromscores[Mathf.Abs(player - 1)] -= 1.0f;

        // Reset, give the ball to the opponent
        ResetGame(Mathf.Abs(player - 1));
    }

    private void UpdateRewards()
    {
        // Update the reward from the current ball position
        if( LastPlayerWithBall == -1 )
        {
            _player_rewards_currentball[0] = 0.0f;
            _player_rewards_currentball[1] = 0.0f;
        }
        else
        {
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
            float denominator = 2.0f * Vector3.Distance(ball_drop_source.transform.position , goal1.transform.position);
            float distance_goal1 = Vector3.Distance(ball.transform.position, goal1.transform.position);
            float distance_goal2 = Vector3.Distance(ball.transform.position, goal2.transform.position);
            _player_rewards_currentball[0] = proximal_reward - proximal_reward * distance_goal2 / denominator;
            _player_rewards_currentball[1] = proximal_reward - proximal_reward * distance_goal1 / denominator;


            //float reward = (LastPlayerWithBall == 0 ? 1.0f : -1.0f) * rewards[LastRodWithBall];
            //_player_rewards_currentball[0] = reward;
            //_player_rewards_currentball[1] = -reward;
        }

        // Update total reward
        player_rewards[0] = _player_rewards_currentball[0] + _player_rewards_fromscores[0];
        player_rewards[1] = _player_rewards_currentball[1] + _player_rewards_fromscores[1];
    }

    // Callback for tracking which rods have the ball
    public void BallEnterRod(int player, int rod_index)
    {
        if (LastPlayerWithBall != player)
            TimeHeldByPlayer = 0.0f;

        LastPlayerWithBall = player;
        LastRodWithBall = rod_index;
        _player_rod_hasball[player, rod_index] = true;
        UpdateRewards();
    }

    public void BallExitRod(int player, int rod_index)
    {
        _player_rod_hasball[player, rod_index] = false;
        UpdateRewards();
    }

    // Ball exit play area
    public void BallExitPlay()
    {
        Debug.Log("Ball exited play.");
        ResetGame();
    }

    public void Reset()
    {
        // Clear reward and reset ball
        _player_rewards_currentball[0] = 0.0f;
        _player_rewards_currentball[1] = 0.0f;
        _player_rewards_fromscores[0] = 0.0f;
        _player_rewards_fromscores[1] = 0.0f;
        ResetGame();
    }

    // Reset
    public void ResetGame( int? give_to_player = null )
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

        // Reset time held counter
        TimeHeldByPlayer = 0.0f;

        UpdateRewards();
    }
}
