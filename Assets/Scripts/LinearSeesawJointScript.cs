using UnityEngine;

public class LinearSeesawJointScript : MonoBehaviour
{
    public Rigidbody myRB, otherRB;
    public ConfigurableJoint myJoint, otherJoint;
    public float spring;

    float originY;

    void Start() {
        originY = myRB.position.y;
    }

    void FixedUpdate() {
        Vector3 myTarget = myRB.position;
        myTarget.y = originY - (otherRB.position.y - originY);
        myJoint.targetPosition = myTarget;

        Vector3 otherTarget = otherRB.position;
        otherTarget.y = originY - (myRB.position.y - originY);
        otherJoint.targetPosition = otherTarget;

        float delta = originY - myRB.position.y;
        myRB.AddForce(Vector3.up * delta * spring);
        delta = originY - otherRB.position.y;
        otherRB.AddForce(Vector3.up * delta * spring);

        //myJoint.targetVelocity = -otherRB.linearVelocity;
        //otherJoint.targetVelocity = -myRB.linearVelocity;
    }
}
