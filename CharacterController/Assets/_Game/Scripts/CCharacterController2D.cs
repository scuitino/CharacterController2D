using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(BoxCollider2D))]
public class CCharacterController2D : MonoBehaviour {

    // collision mask
    [SerializeField]
    LayerMask _collisionMask;

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

    //instance of collisions info
    public CollisionsInfo _collisionsInfo;

    private void Start()
    {
        // controller collider
        _collider = this.GetComponent<BoxCollider2D>();

        // calculate space between rays
        CalculateRaySpacing();
    }

    // apply velocity to character controller
    public void Move(Vector3 aVelocity)
    {
        // updating and calculating rays
        UpdateRaycastOrigins();

        // reset collisions info
        _collisionsInfo.Reset();

        // applying vertical collisions
        if (aVelocity.x != 0)
            HorizontalCollisions(ref aVelocity);

        if (aVelocity.y != 0)
            VerticalCollisions(ref aVelocity);

        // move transform
        transform.Translate(aVelocity);
    }

    // checking and applying Horizontal collisions
    public void HorizontalCollisions(ref Vector3 aVelocity)
    {
        // return positive (1) or negative (-1)
        float tDirectionX = Mathf.Sign(aVelocity.x);

        // return abs of y velocity + skinwidth of the controller
        float tRayLenght = Mathf.Abs(aVelocity.x) + _skinWidth;

        // throwing vertical rays
        for (int i = 0; i < _horizontalRayCount; i++)
        {
            // if the direction is left use bottonLeft else use bottonRight
            Vector2 tRayOrigin = (tDirectionX == -1) ? _raycarsOrigins._bottonLeft : _raycarsOrigins._bottonRight;

            // positioning each ray origin
            tRayOrigin += Vector2.up * (_horizontalRaySpacing * i);

            // throwing ray
            RaycastHit2D tHit = Physics2D.Raycast(tRayOrigin, Vector2.right * tDirectionX, tRayLenght, _collisionMask);

            // debuging ray
            Debug.DrawRay(tRayOrigin, Vector2.right * tDirectionX * tRayLenght, Color.red);

            // if the ray touch something
            if (tHit)
            {
                // calculating the Y velocity needed to reach the collider 
                aVelocity.x = (tHit.distance - _skinWidth) * tDirectionX; // tHit.distance is always positive 

                // to avoid the next rays hit a Farther object
                tRayLenght = tHit.distance;

                // if tDirectionX == -1 = left is true, if == 1 right is true
                _collisionsInfo._left = tDirectionX == -1;
                _collisionsInfo._right = tDirectionX == 1;
            }
        }
    }

    // checking and applying vertical collisions
    public void VerticalCollisions(ref Vector3 aVelocity)
    {
        // return positive (1) or negative (-1)
        float tDirectionY = Mathf.Sign(aVelocity.y);

        // return abs of y velocity + skinwidth of the controller
        float tRayLenght = Mathf.Abs(aVelocity.y) + _skinWidth;

        // throwing vertical rays
        for (int i = 0; i < _verticalRayCount; i++)
        {
            // if the direction is down use bottonLeft else use topleft
            Vector2 tRayOrigin = (tDirectionY == -1) ? _raycarsOrigins._bottonLeft : _raycarsOrigins._topLeft;

            // positioning each ray origin
            tRayOrigin += Vector2.right * (_verticalRaySpacing * i + aVelocity.x);

            // throwing ray
            RaycastHit2D tHit = Physics2D.Raycast(tRayOrigin, Vector2.up * tDirectionY, tRayLenght, _collisionMask);

            // debuging ray
            Debug.DrawRay(tRayOrigin, Vector2.up * tDirectionY * tRayLenght, Color.red);

            // if the ray touch something
            if (tHit)
            {
                // calculating the Y velocity needed to reach the collider 
                aVelocity.y = (tHit.distance - _skinWidth) * tDirectionY; // tHit.distance is always positive 

                // to avoid the next rays hit a Farther object
                tRayLenght = tHit.distance;

                // if tDirectionY == -1 = left is true, if == 1 right is true
                _collisionsInfo._isGrounded = tDirectionY == -1;
                _collisionsInfo._above = tDirectionY == 1;
            }            
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

    // info about the collisions
    public struct CollisionsInfo
    {
        public bool _above, _isGrounded;
        public bool _left, _right;

        // turn all collisions info to false
        public void Reset()
        {
            _above = false;
            _isGrounded = false; // isgrounded?
            _left = false;
            _right = false;
        }
    }
}
