using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class HeuristicDecision : MonoBehaviour, Decision
{
    private float timeInState = 10.0f;
    private float[] state = new float[8] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, };

    public float[] Decide(
        List<float> vectorObs,
        List<Texture2D> visualObs,
        float reward,
        bool done,
        List<float> memory)
    {
        Debug.Log("Test!");
        // Take the actions. Action format is continuous:
        // 0-3: Linear rods 0 to 3
        // 4-7: Torque rods 0 to 3
        timeInState += Time.deltaTime;
        if (timeInState > 0.5)
        {
            state = new float[8] {
                (Random.value - 0.5f) / 3.0f,
                (Random.value - 0.5f) / 3.0f,
                (Random.value - 0.5f) / 3.0f,
                (Random.value - 0.5f) / 3.0f,
                (Random.value - 0.5f) / 3.0f,
                (Random.value - 0.5f) / 3.0f,
                (Random.value - 0.5f) / 3.0f,
                (Random.value - 0.5f) / 3.0f
            };
            timeInState = 0.0f;
        }
        return state;
    }

    public List<float> MakeMemory(
        List<float> vectorObs,
        List<Texture2D> visualObs,
        float reward,
        bool done,
        List<float> memory)
    {
        return new List<float>();
    }
}
