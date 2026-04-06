using UnityEngine;

public class LightRotator : MonoBehaviour
{
    public Light[] lights;

    public float speedMultiplier = 1.0f;
    public float circleSize = 15.0f;

    private float[] offsets;
    private Quaternion[] initialRotations;

    void Start()
    {
        offsets = new float[lights.Length];
        initialRotations = new Quaternion[lights.Length];

        for (int i = 0; i < lights.Length; i++)
        {
            offsets[i] = Random.Range(0f, 100f);
            initialRotations[i] = lights[i].transform.rotation;
        }
    }

    void Update()
    {
        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] == null) continue;

            float time = (Time.time + offsets[i]) * speedMultiplier;
            
            float xAngle = Mathf.Sin(time) * circleSize;
            float yAngle = Mathf.Cos(time) * circleSize;

            lights[i].transform.rotation = initialRotations[i] * Quaternion.Euler(xAngle, yAngle, 0);
        }
    }
}