using UnityEngine;

// Contains environment settings for different dimensions
public class Environment : MonoBehaviour
{
    public static Environment Instance;

    public EnvironmentVariables pixelDimensionVaribles;
    public EnvironmentVariables vectorDimensionVaribles;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
