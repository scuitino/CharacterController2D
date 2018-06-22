using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CCharacterController2D))]
public class CPlayer : MonoBehaviour {

    // player jump height
    [SerializeField]
    float _jumpHeight;

    // time to reach the max height
    [SerializeField]
    float _timeToJumpApex;

    // to smooth the X velocity when grounded
    [SerializeField]
    float _accelerationTimeGrounded;

    // to smooth the X velocity when airborne
    [SerializeField]
    float _accelerationTimeAirborne;

    // player move speed
    [SerializeField]
    float _moveSpeed;

    // player jump velocity
    float _jumpVelocity;

    // player gravity
    float _gravity;

    // player velocity
    Vector3 _velocity;

    // to use with Mathf.SmoothDamp
    float _velocitySmoothing;

    // reference to character controller 2D
    CCharacterController2D _characterController;

    private void Start()
    {
        _characterController = this.GetComponent<CCharacterController2D>();

        // calculate gravity
        _gravity = -(2 * _jumpHeight) / Mathf.Pow(_timeToJumpApex, 2);

        // calculate jump velocity needed to reach the jump
        _jumpVelocity = Mathf.Abs(_gravity) * _timeToJumpApex;
    }

    private void Update()
    {
        // if there is collision above or below set Y vel to 0
        if (_characterController._collisionsInfo._above || _characterController._collisionsInfo._isGrounded)
        {
            _velocity.y = 0;
        }

        // player input
        #region Player Input
        // DPad inputs
        Vector2 tInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // Jump button
        if (Input.GetKeyDown(KeyCode.Space) && _characterController._collisionsInfo._isGrounded)
        {
            _velocity.y = _jumpVelocity;
        }
        #endregion

        // calculate x velocity with player input
        float tTargetVelocityX = tInput.x * _moveSpeed;

        // applying x velocity with smooth
        _velocity.x = Mathf.SmoothDamp(_velocity.x, tTargetVelocityX, ref _velocitySmoothing, (_characterController._collisionsInfo._isGrounded)? _accelerationTimeGrounded: _accelerationTimeAirborne);

        // apply Gravity 
        _velocity.y += _gravity * Time.deltaTime;

        // move player
        _characterController.Move(_velocity * Time.deltaTime);
    }
}
