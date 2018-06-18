using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CCharacterController2D))]
public class CPlayer : MonoBehaviour {

    // reference to character controller 2D
    CCharacterController2D _characterController;

    private void Start()
    {
        _characterController = this.GetComponent<CCharacterController2D>();
    }
}
