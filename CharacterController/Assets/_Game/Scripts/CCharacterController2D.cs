using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(BoxCollider2D))]
public class CCharacterController2D : MonoBehaviour {

    // to have space to throw the rays when the player is grounded
    const float _skinWidth = 0.015f;

    // how many horizontal raycasts
    [SerializeField]
    int _horizontalRayCount = 4;

    // how many vertical raycasts
    [SerializeField]
    int _verticalRayCount = 4;

    // to calculate the spacing between the rays
    float _horizontalRaySpacing;
    float _verticalRaySpacing;

    // reference to character box collider
    BoxCollider2D _collider;

    // reference to the corners of the character box colliders
    RaycastOrigins _raycarsOrigins;

    private void Start()
    {
        _collider = this.GetComponent<BoxCollider2D>();   
    }

    private void Update()
    {
        // updating and calculating rays
        UpdateRaycastOrigins();
        CalculateRaySpacing();

        // debug vertical rays
        for (int i = 0; i < _verticalRayCount; i++)
        {
            Debug.DrawRay(_raycarsOrigins._bottonLeft + Vector2.right * _verticalRaySpacing * i, Vector2.down, Color.red);
        }
    }

    // update raycast origin positions
    void UpdateRaycastOrigins()
    {
        Bounds tBounds = _collider.bounds;

        // substract skinwidth for each side (left, right, up, down)
        tBounds.Expand(_skinWidth * -2);

        // setting raycast origins
        _raycarsOrigins._bottonLeft = new Vector2(tBounds.min.x, tBounds.min.y);
        _raycarsOrigins._bottonRight = new Vector2(tBounds.max.x, tBounds.min.y);
        _raycarsOrigins._topLeft = new Vector2(tBounds.min.x, tBounds.max.y);
        _raycarsOrigins._topRight = new Vector2(tBounds.max.x, tBounds.max.y);
    }

    // calculate the spacing between the rays
    void CalculateRaySpacing()
    {
        Bounds tBounds = _collider.bounds;

        // substract skinwidth for each side (left, right, up, down)
        tBounds.Expand(_skinWidth * -2);

        // we need at least 2 rays per axis
        _horizontalRayCount = Mathf.Clamp(_horizontalRayCount, 2, int.MaxValue);
        _verticalRayCount = Mathf.Clamp(_verticalRayCount, 2, int.MaxValue);

        // calculate spacing  = bound size / spaces between rays
        _horizontalRaySpacing = tBounds.size.y / (_horizontalRayCount - 1);
        _verticalRaySpacing = tBounds.size.x / (_verticalRayCount - 1);
    }

    // reference to the corners of the character box colliders
    struct RaycastOrigins
    {
        public Vector2 _topLeft, _topRight;
        public Vector2 _bottonLeft, _bottonRight;
    }
}
