using UnityEngine;

public class Rotate : MonoBehaviour
{
    [SerializeField]
    protected Transform center;
    [SerializeField]
    protected float radius;
    [SerializeField]
    protected float radiusSpeed;
    public void Update()
    {
        transform.RotateAround(center.position, Vector3.up, radiusSpeed * Time.deltaTime);
        var desiredPosition = (transform.position - center.position).normalized * radius + center.position;
        transform.position = Vector3.MoveTowards(transform.position, desiredPosition, Time.deltaTime * radiusSpeed);
    }

}
