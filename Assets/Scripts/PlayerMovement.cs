using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private SpriteRenderer bodySpriteRenderer;
    [SerializeField] private SpriteRenderer rightLegSpriteRenderer;
    [SerializeField] private SpriteRenderer leftLegSpriteRenderer;
    
    [Header("Parameters")]
    [SerializeField] private float speed;
    
    private Rigidbody _rigidbody;
    
    private Animator _animator;
    private float _xInput;
    private float _zInput;
    
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
    }
    
    private void Update()
    {
        _xInput = Input.GetAxis("Horizontal");
        _zInput = Input.GetAxis("Vertical");
        if (_xInput != 0 || _zInput != 0)
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
        _rigidbody.velocity = new Vector3(_xInput, 0, _zInput).normalized * speed;
        //FlipX();
    }
    
    // private void FlipX()
    // {
    //     bodySpriteRenderer.flipX = _xInput switch
    //     {
    //         > 0 => false,
    //         < 0 => true,
    //         _ => bodySpriteRenderer.flipX
    //     };
    //     rightLegSpriteRenderer.flipX = _xInput switch
    //     {
    //         > 0 => false,
    //         < 0 => true,
    //         _ => rightLegSpriteRenderer.flipX
    //     };
    //     leftLegSpriteRenderer.flipX = _xInput switch
    //     {
    //         > 0 => false,
    //         < 0 => true,
    //         _ => leftLegSpriteRenderer.flipX
    //     };
    // }
}
