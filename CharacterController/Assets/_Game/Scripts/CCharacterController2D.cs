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

    // to limit the max ascend and descend slope angle
    [SerializeField]
    float _maxSlopeClimbAngle = 75;

    [SerializeField]
    float _maxSlopeDescendAngle = 75;

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

        _collisionsInfo._oldVelocity = aVelocity;

        if (aVelocity.y < 0)
        {
            DescendSlope(ref aVelocity);
        }

        // applying collisions
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

            Debug.DrawRay(tRayOrigin, Vector2.right * tDirectionX * tRayLenght, Color.red);

            if (tHit)
            {
                // obtain slope angle comparing slope normal with vector.up
                float tSlopeAngle = Vector2.Angle(tHit.normal, Vector2.up);

                // check only with botton horizontal raycast
                if (i == 0 && tSlopeAngle <= _maxSlopeClimbAngle) 
                {
                    // to stop descending when a ascending start
                    if (_collisionsInfo._isDescendingSlope) 
                    {
                        _collisionsInfo._isDescendingSlope = false;
                        aVelocity = _collisionsInfo._oldVelocity;
                    }

                    float tDistanceToSlopeStart = 0;

                    //when a new slop is starting
                    if (tSlopeAngle != _collisionsInfo._oldSlopeAngle)
                    {
                        // distance from the skinwidth to the slope
                        tDistanceToSlopeStart = tHit.distance - _skinWidth;

                        // substract to use only the x velocity corresponding to the part of the slope
                        aVelocity.x -= tDistanceToSlopeStart * tDirectionX;
                    }

                    ClimbSlope(ref aVelocity, tSlopeAngle);

                    // add again to move the player to correct position
                    aVelocity.x += tDistanceToSlopeStart * tDirectionX;
                }

                // is not climbing or the actual ray is colliding with a wall
                if (!_collisionsInfo._isClimbingSlope || tSlopeAngle > _maxSlopeClimbAngle)
                {
                    // calculating the X velocity needed to reach the collider 
                    aVelocity.x = Mathf.Min(Mathf.Abs(aVelocity.x), (tHit.distance - _skinWidth)) * tDirectionX; // tHit.distance is always positive 

                    // to avoid the next rays hit a Farther object
                    tRayLenght = Mathf.Min(Mathf.Abs(aVelocity.x) + _skinWidth, tHit.distance);

                    // to fix the shaking when it hits a wall when sloping
                    if (_collisionsInfo._isClimbingSlope)
                    {
                        // opposite = tan(slopeangle) * adjacent
                        aVelocity.y = Mathf.Tan(_collisionsInfo._slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(aVelocity.x);
                    }

                    // if tDirectionX == -1 = left is true, if == 1 right is true
                    _collisionsInfo._left = tDirectionX == -1;
                    _collisionsInfo._right = tDirectionX == 1;
                }
                
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

                // to fix the shaking when it hits a roof when sloping
                if (_collisionsInfo._isClimbingSlope)
                {
                    // adjacent = opposite / tan(slopeangle)
                    aVelocity.x = aVelocity.y / Mathf.Tan(_collisionsInfo._slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(aVelocity.x);
                }

                // if tDirectionY == -1 = isGrounded is true, if == 1 above is true
                _collisionsInfo._isGrounded = tDirectionY == -1;
                _collisionsInfo._above = tDirectionY == 1;
            }            
        }

        // to fix the bug when the player go into a new slope
        if (_collisionsInfo._isClimbingSlope)
        {
            // i need to throw a horizontal ray from the new position using bottom origin
            float tDirectionX = Mathf.Sign(aVelocity.x);
            tRayLenght = Mathf.Abs(aVelocity.x) + _skinWidth;
            Vector2 tRayOrigin = ((tDirectionX == -1) ? _raycarsOrigins._bottonLeft : _raycarsOrigins._bottonRight) + Vector2.up * aVelocity.y;
            RaycastHit2D tHit = Physics2D.Raycast(tRayOrigin, Vector2.right * tDirectionX, tRayLenght, _collisionMask);

            if (tHit)
            {
                float tSlopeAngle = Vector2.Angle(tHit.normal, Vector2.up);

                // if a new slope start 
                if (tSlopeAngle != _collisionsInfo._slopeAngle && tSlopeAngle < _maxSlopeClimbAngle) 
                {
                    // adjust x and y velocity
                    aVelocity.y = Mathf.Sin(tSlopeAngle * Mathf.Deg2Rad) * aVelocity.magnitude;
                    aVelocity.x = Mathf.Cos(tSlopeAngle * Mathf.Deg2Rad) * aVelocity.magnitude;
                    _collisionsInfo._slopeAngle = tSlopeAngle;
                }
            }
        }
    }

    // use to climb slopes
    void ClimbSlope(ref Vector3 aVelocity, float aSlopeAngle)
    {
        // to move the same distance as when is not slope use aVelocity.x as hypotenuse
        float tMoveDistanceTarget = Mathf.Abs(aVelocity.x);

        // y velocity = sin(slopeAngle) * hypotenuse
        float tClimbVelocityY = Mathf.Sin(aSlopeAngle * Mathf.Deg2Rad) * tMoveDistanceTarget;

        // if is not jumping
        if (aVelocity.y <= tClimbVelocityY)
        {
            aVelocity.y = tClimbVelocityY;

            // x velocity = cos(slopeAngle) * hypotenuse * sign of x (to maintain the maintain direction)
            aVelocity.x = Mathf.Cos(aSlopeAngle * Mathf.Deg2Rad) * tMoveDistanceTarget * Mathf.Sign(aVelocity.x);

            // to enable jump when the player is climbing
            _collisionsInfo._isGrounded = true;

            // updating climbing info
            _collisionsInfo._isClimbingSlope = true;
            _collisionsInfo._slopeAngle = aSlopeAngle;
        }        
    }

    // use to descend slopes
    void DescendSlope(ref Vector3 aVelocity)
    {
        float tDirectionX = Mathf.Sign(aVelocity.x);
        // if direction is right use left bottom ray else use left bottom
        Vector2 tRayOrigin = (tDirectionX == -1) ? _raycarsOrigins._bottonRight : _raycarsOrigins._bottonLeft;
        RaycastHit2D tHit = Physics2D.Raycast(tRayOrigin, -Vector2.up, Mathf.Infinity, _collisionMask);

        if (tHit)
        {
            float tSlopeAngle = Vector2.Angle(tHit.normal, Vector2.up);

            // there is a slope that the player can walk?
            if(tSlopeAngle != 0 && tSlopeAngle <= _maxSlopeDescendAngle)
            {
                // to know if the slope is in the same direction than the player
                if(Mathf.Sign(tHit.normal.x) == tDirectionX)
                {
                    // if the ray lenght is less or equal than how much you must move to reach the slope according to the angle
                    // in this case descend. Else is jumping or falling.
                    if (tHit.distance - _skinWidth <= Mathf.Tan(tSlopeAngle * Mathf.Deg2Rad) * Mathf.Abs(aVelocity.x))
                    {
                        // to move the same distance as when is not slope use aVelocity.x as hypotenuse
                        float tMoveDistanceTarget = Mathf.Abs(aVelocity.x);

                        // opposite = sin(slopeAngle) * adjacent
                        float tDescendVelocityY = Mathf.Sin(tSlopeAngle * Mathf.Deg2Rad) * tMoveDistanceTarget;

                        // x velocity = cos(slopeAngle) * hypotenuse * sign of x (to maintain the maintain direction)
                        aVelocity.x = Mathf.Cos(tSlopeAngle * Mathf.Deg2Rad) * tMoveDistanceTarget * Mathf.Sign(aVelocity.x);
                        aVelocity.y -= tDescendVelocityY;

                        // updating info
                        _collisionsInfo._slopeAngle = tSlopeAngle;
                        _collisionsInfo._isDescendingSlope = true;
                        _collisionsInfo._isGrounded = true;
                    }
                }
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
        public bool _above;
        public bool _left, _right;

        // isgrounded?
        public bool _isGrounded;

        // slope info
        public bool _isClimbingSlope;
        public bool _isDescendingSlope;
        public float _slopeAngle, _oldSlopeAngle;

        public Vector3 _oldVelocity;

        // turn all collisions info to false
        public void Reset()
        {
            _above = false;
            _isGrounded = false;
            _left = false;
            _right = false;
            _isClimbingSlope = false;
            _isDescendingSlope = false;
            _oldSlopeAngle = _slopeAngle;
        }
    }
}
