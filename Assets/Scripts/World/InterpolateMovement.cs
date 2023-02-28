using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
/// <summary>
/// The Interpolate Movement of an entity
/// </summary>
public class InterpolateMovement : MonoBehaviour
{
    private Vector3? _targetPosition;
    /// <summary>
    /// The velocity of target position
    /// </summary>
    private Vector3? _targetVelocity;

    private float _lastSetPositionTime;
    public const float DisplacementRate = 0.05f;
    /// <summary>
    /// If the distance between now position and target position is smaller than LerpMinDistance, stop lerping.
    /// </summary>
    //public const float LerpMinDistance = 0.1f;
    public const float InterpolationMinVelocity = 0.02f;
    public void SetTargetPosition(Vector3 targetPosition)
    {
        if (this._targetPosition != null)
        {
            this._targetVelocity = (targetPosition - this._targetPosition) / (Time.time - _lastSetPositionTime);
        }
        this._targetPosition = targetPosition;
        _lastSetPositionTime = Time.time;
    }
    private void Interpolation()
    {
        //if (_targetPosition == null || this._targetVelocity == null)
        //{
        //    return;
        //}

        //if (((Vector3)this._targetVelocity).magnitude > InterpolationMinVelocity)
        //{
        //    Vector3 velocity = (Vector3)this._targetVelocity + ((Vector3)this._targetPosition - this.transform.position) * DisplacementRate;
        //    this.transform.Translate(velocity * Time.deltaTime);
        //}
        //else
        //{
        // this.transform.position = (Vector3)this._targetPosition;
        //}

        // Close The Interpolation Temporarily
        if (this._targetPosition != null)
            this.transform.position = (Vector3)this._targetPosition;
    }
    private void Update()
    {
        //if (Vector3.Distance(this.transform.position, this._targetPosition) > LerpMinDistance)
        Interpolation();
    }
}
