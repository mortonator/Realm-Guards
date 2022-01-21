using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretController : MonoBehaviour
{
    [SerializeField] Transform yAxis;
    [SerializeField] Transform xAxis;
    [SerializeField] Transform turretEnd;

    [SerializeField] float turnSpeed;

    Vector3 pos;

    void Update()
    {
        if (RG_NetworkManager.localPlayer != null)
            LookAtTarget(RG_NetworkManager.localPlayer.playerCon.transform.position + Vector3.up);
    }

    void OnDrawGizmos()
    {
        Vector3 targetFlatDir = new Vector3(pos.x, 0, pos.z) - turretEnd.position;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(turretEnd.position, turretEnd.position + (targetFlatDir * 10));
        Gizmos.color = Color.red;
        Gizmos.DrawLine(turretEnd.position, turretEnd.position + (yAxis.forward * 10));
    }
    void LookAtTarget(Vector3 targetPosition)
    {
        pos = targetPosition;
        Vector3 targetFlatDir = new Vector3(targetPosition.x, 0, targetPosition.z) - turretEnd.position;

        float yAngle = Vector3.Angle(yAxis.forward, targetFlatDir);
        if ((int)yAngle != 0)
        {
            Vector3 cross = Vector3.Cross(yAxis.forward, targetFlatDir);
            if (cross.y < 0)
                yAngle = -yAngle;

            yAxis.Rotate(Vector3.up * yAngle * turnSpeed * Time.deltaTime);
        }

        float xAngle = Vector3.Angle(targetFlatDir, targetPosition - turretEnd.position) - xAxis.eulerAngles.x;
        if ((int)xAngle != 0)
        {
            Vector3 cross = Vector3.Cross(targetFlatDir, targetPosition - turretEnd.position);
            if (cross.y < 0)
                xAngle = -xAngle;

            xAxis.Rotate(Vector3.right * xAngle * turnSpeed * Time.deltaTime);
        }

        Debug.Log((int)xAngle);
    }
}
