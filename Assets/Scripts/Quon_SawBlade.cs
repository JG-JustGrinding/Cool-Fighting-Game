using System.Collections;
using UnityEngine;

public class Quon_SawBlade : MonoBehaviour
{
    public float speed = 2f;
    public int framesLastFor = 300;
    public int destroyFramesLastFor = 10;
    public int destroyFrames = 10;
    public int smoothRotateAfterFrames = 20; // Frames after which to start smooth rotation to zero
    public float smoothRotateDuration = 0.5f; // Duration over which to smooth rotate to zero
    public float rotationAngle = 90f;
    bool destroying;
    bool smoothingRotation;

    void Start()
    {
        StartCoroutine(DestroyAfterFrames());
        StartCoroutine(SmoothRotation());
    }

    void Update()
    {
        Vector3 rotatedForward = Quaternion.Euler(0, rotationAngle, 0) * transform.forward;

        transform.Translate((!destroying ? speed : speed * .4f) * Time.deltaTime * rotatedForward);

        if (smoothingRotation)
        {
            Quaternion targetRotation = Quaternion.Euler(0, 0, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime / smoothRotateDuration);
        }
    }

    IEnumerator DestroyAfterFrames()
    {
        yield return new WaitForSeconds(framesLastFor / 60f);

        destroying = true;
        GetComponent<Collider2D>().enabled = false;
        GetComponent<Animator>().SetTrigger("destroying");

        yield return new WaitForSeconds(destroyFramesLastFor / 60f);

        Destroy(gameObject);
    }

    IEnumerator SmoothRotation()
    {
        yield return new WaitForSeconds(smoothRotateAfterFrames / 60f);

        smoothingRotation = true;
    }
}
