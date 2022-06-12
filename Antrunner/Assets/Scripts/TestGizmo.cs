using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TestGizmo : MonoBehaviour
{
    private const float rightBounds = 137.25f;
    private const float upperBounds = 80f;
    void OnDrawGizmos()
    {
        Vector2 forward =  transform.up * 3f;
        AntSpawner.Ant ant = new AntSpawner.Ant();
        ant.pos = transform.position;
        ant.rot = transform.rotation.eulerAngles.z;
        ant.forward = forward;
        Vector2 target = new Vector2(ant.forward.x, ant.forward.y);
        Vector2 oldTarget = target;
        target = deflect(1f, ant);
        if (target.x != oldTarget.x || target.y != oldTarget.y)
        {
            ant.rot = newRotation(target);
            // Debug.Log("New rotation: " + ant.rot);
        }
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + target);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + forward);

        Handles.Label(transform.position, "Ant: " + ant.pos + " | " + ant.rot);
    }
    float newRotation(Vector2 v1)
    {
        Vector2 v2 = new Vector2(0, 1);
        // float angle = 0;
        float dotProduct = Vector4.Dot(new Vector4(v1.x, v1.y, 0, 0), new Vector4(v2.x, v2.y, 0, 0));
        float angle = Mathf.Acos(dotProduct / (v1.magnitude * v2.magnitude)) * 180 / 3.1415926538f;
        if(v1.x > 0){
            angle = 360 - angle;
        }
        return angle;
    }
    Vector2 deflect(float distance, AntSpawner.Ant ant)
    {
        float futureY = ant.pos.y + ant.forward.y * distance;
        float futureX = ant.pos.x + ant.forward.x * distance;
        Vector2 result = new Vector2(ant.forward.x, ant.forward.y);
        // Deflect the ant's forward vector if below lowerBounds
        if (futureY < -upperBounds)
        {
            result = new Vector2(ant.forward.x, -ant.forward.y);
        }
        // Deflect the ant's forward vector if above upperBounds
        if (futureY > upperBounds)
        {
            result = new Vector2(ant.forward.x, -ant.forward.y);
        }
        // Deflect the ant's forward vector if to the left of leftBounds
        if (futureX < -rightBounds)
        {
            result = new Vector2(-ant.forward.x, ant.forward.y);
        }
        // Deflect the ant's forward vector if to the right of rightBounds
        if (futureX > rightBounds)
        {
            result = new Vector2(-ant.forward.x, ant.forward.y);
        }

        return result;
    }
}
