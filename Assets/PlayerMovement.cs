using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    
    [SerializeField] private float speed;
    
    private Rigidbody2D _rigidbody2D;
    //private SpriteRenderer _spriteRenderer;
    private Animator _animator;
    private float _xInput;
    private float _yInput;
    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        //_spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
    }
    
    private void Update()
    {
        _xInput = Input.GetAxis("Horizontal");
        _yInput = Input.GetAxis("Vertical");
        if (_xInput != 0 || _yInput != 0)
        {
            _animator.SetBool("isRunning",true);
        }
        else
        {
            _animator.SetBool("isRunning",false);
        }
    }
    
    private void FixedUpdate()
    {
        _rigidbody2D.velocity = new Vector2(_xInput, _yInput).normalized * speed;
        //FlipX();
    }
    
    // private void FlipX()
    // {
    //     _spriteRenderer.flipX = _xInput switch
    //     {
    //         > 0 => false,
    //         < 0 => true,
    //         _ => _spriteRenderer.flipX
    //     };
    // }
}
