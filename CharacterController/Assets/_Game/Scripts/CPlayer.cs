using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CCharacterController2D))]
public class CPlayer : MonoBehaviour {

    // player move speed
    [SerializeField]
    float _moveSpeed;

    // player gravity
    [SerializeField]
    float _gravity;

    // player velocity
    Vector3 _velocity;

    // reference to character controller 2D
    CCharacterController2D _characterController;

    private void Start()
    {
        _characterController = this.GetComponent<CCharacterController2D>();
    }

    private void Update()
    {
        // player input
        Vector2 tInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // calculate x velocity with player input
        _velocity.x = tInput.x * _moveSpeed;

        // apply Gravity 
        _velocity.y += _gravity * Time.deltaTime;

        // move player
        _characterController.Move(_velocity * Time.deltaTime);
    }
}
