using UnityEngine;

public static class RigidbodyUtil
{
    public static void Reset(Rigidbody rb)
    {
        rb.position = Vector3.zero;
        rb.rotation = Quaternion.identity;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public static void SetRandomForce2D(Rigidbody rb, float maxForce)
    {
        Vector3 randomForce = maxForce * Random.insideUnitCircle;
        rb.velocity = Vector3.zero;
        rb.AddForce(randomForce);

        Debug.Log("force: " + randomForce);
    }

    public static void SetRandomTorque(Rigidbody rb, float maxTorque)
    {
        Vector3 randomTorque = maxTorque * Random.insideUnitSphere;
        rb.angularVelocity = Vector3.zero;
        rb.AddTorque(randomTorque);
    }

    internal static void SetRandomForceAndTorque(Rigidbody rb, Transform trans)
    {
        rb.AddForce(CreateRandomSpeed()  * trans.right);
        rb.AddForce(CreateRandomSpeed() * trans.up);
    }

    static float CreateRandomSpeed()
    {
        var speed = Random.Range(200f, 800f);
        var selector = Random.Range(0, 2);
        var dir = selector == 1 ? -1 : 1;

        return speed * dir;
    }

}