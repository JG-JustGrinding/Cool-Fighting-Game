using UnityEngine;
using TMPro;

public class FakeTimer : MonoBehaviour
{
    public TextMeshProUGUI text;
    public float startTime;
    private float time;

    void Start()
    {
        time = startTime;
    }

    void Update()
    {
        time -= Time.deltaTime;
        if (time < 0)
        {
            time = 0;
        }
        int seconds = Mathf.FloorToInt(time);
        text.text = string.Format("{0:0}", seconds);
    }
}
