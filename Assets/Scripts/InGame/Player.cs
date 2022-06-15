using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Controls")]
    [Tooltip("A distância entre cada faixa da caminhada")]
    [SerializeField] private float _trackStrides;
    [SerializeField] private float _jumpHeight;
    [SerializeField] private float _fastDescentSpeed;
    [SerializeField] private float _crouchDuration;
    [SerializeField] [Range(1f, 5f)] private float _fallGravityMultiplier;

    [Header("Collision Handling")]
    [SerializeField] [Range(0f, 90f)] private float _maxSlopeAngle;
    [SerializeField] [Range(0f, 1.5f)] private float _maxStepHeight;

    private int _currentTrack; // -1 0 1  
    private Rigidbody _rigidbody;
    private Collider _collider;
    private PlayerStats _playerStats;
    private bool _isForcedFalling;
    private bool _isGrounded;
    private bool _wasGroundedLastFrame;
    private float _crouchTimer;
    private WalkController _walkController;
    private Animator _animator;

    private void Start()
    {
        this._rigidbody = GetComponent<Rigidbody>();
        this._collider = GetComponent<Collider>();
        this._playerStats = GetComponent<PlayerStats>();
        this._walkController = FindObjectOfType<WalkController>();
        this._animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (this._walkController.IsGameOver) return;

        UpdateWalkSpeed();

        UpdateGroundStatus();
        HandleSidewaysMoves();
        //HandleJumping();
        //HandleFastFalling();
        //HandleFastDescent();
        //HandleCrouching();
        //HandleRigidBody();
    }

    public void OnDeath()
    {
        this._animator.SetBool("IsDead", true);
        this._rigidbody.isKinematic = true;
    }

    private void UpdateWalkSpeed()
    {
        this._animator.SetFloat("WalkSpeed", this._walkController.IsGamePaused ? 0f : this._walkController.CurrentSpeed * .2f);
    }

    private void HandleRigidBody()
    {
        this.transform.rotation = Quaternion.identity;
        this._rigidbody.angularVelocity = Vector3.zero;
        this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, 0f);
    }

    private void HandleCrouching()
    {

        if (InputController.Instance.GoingDown())
        {
            this._crouchTimer = this._crouchDuration;
            this.transform.localScale = new Vector3(1f, .5f, 1f);
        }

        if (this._crouchTimer > 0f)
        {
            this._crouchTimer -= Time.deltaTime;
            this._crouchTimer = Mathf.Clamp01(this._crouchTimer);

            if (this._crouchTimer == 0f)
            {
                this.transform.localScale = new Vector3(1f, 1f, 1f);
            }
        }
    }

    private void UpdateGroundStatus()
    {
        this._isGrounded = IsGrounded();

        if (!this._wasGroundedLastFrame && this._isGrounded)
        {
            OnPlayerLanding();
        }

        this._wasGroundedLastFrame = this._isGrounded;
    }

    private void OnPlayerLanding()
    {
        this._isForcedFalling = false;
    }

    private void HandleFastFalling()
    {
        if (this._rigidbody.velocity.y < 0f)
        {
            this._rigidbody.velocity += Physics.gravity * (this._fallGravityMultiplier - 1f) * Time.deltaTime;
        }
    }

    private void HandleFastDescent()
    {
        if (!this._isForcedFalling && !this._isGrounded && InputController.Instance.GoingDown())
        {
            this._rigidbody.velocity = Vector3.down * this._fastDescentSpeed;
            this._isForcedFalling = true;
        }
    }

    private void HandleJumping()
    {
        if (this._isGrounded && InputController.Instance.GoingUp())
        {
            this._crouchTimer = 0.01f;
            this._rigidbody.AddForce(Vector3.up * CalculateJumpSpeed(this._jumpHeight, Physics.gravity.magnitude), ForceMode.Impulse);
        }
    }

    private void HandleSidewaysMoves()
    {
        if (!_walkController.IsGamePaused)
        {
            if (InputController.Instance.GoingLeft())
            {
                this._currentTrack--;
            }
            else if (InputController.Instance.GoingRight())
            {
                this._currentTrack++;
            }
        }
            this._currentTrack = Mathf.Clamp(this._currentTrack, -1, 1);

            float oldX = this.transform.position.x;
            float newX = this._currentTrack * this._trackStrides;
            this.transform.position = new Vector3(Mathf.Lerp(oldX, newX, Time.deltaTime * 7.5f), this.transform.position.y, this.transform.position.z);

    }

    private bool IsGrounded()
    {
        const float rayDist = 0.15f;
        return Physics.Raycast(this.transform.position + Vector3.up * rayDist, Vector3.down, rayDist * 2f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
    }

    private float CalculateJumpSpeed(float jumpHeight, float gravity)
    {
        return Mathf.Sqrt(2f * jumpHeight * gravity);
    }

    private void OnCollisionEnter(Collision other)
    {
        bool collisionHandled = HandleCollision(other);

        if (!collisionHandled)
        { // Todas colisões vão ser tratadas 
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Spawnable spawnable = other.GetComponent<Spawnable>();

        if (spawnable != null)
        {
            spawnable.OnHitPlayer(this, this._playerStats);
        }

        if (other.CompareTag("FinishLine"))
        {
            this._animator.SetBool("HasWon", true);
            this._rigidbody.isKinematic = true;
            this._walkController.OnReacheadFinishLine();
        }
    }

    private bool HandleCollision(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            if (Vector3.Angle(Vector3.up, contact.normal) <= this._maxSlopeAngle)
            { // sliding 
                continue;
            }

            Vector3 point = contact.point;
            Vector3 normal = contact.normal;
            Vector3 horizontalSearchPoint = new Vector3(-normal.x, 0f, -normal.z);

            const float forwardSearchOffset = 0.15f;
            horizontalSearchPoint *= forwardSearchOffset;

            Vector3 rayOrigin = new Vector3(point.x, this.transform.position.y + this._maxStepHeight, point.z) + horizontalSearchPoint;

            Collider collider = contact.otherCollider;
            Ray ray = new Ray(rayOrigin + new Vector3(0f, 0.05f, 0f), Vector3.down);
            if (!collider.Raycast(ray, out RaycastHit hitInfo, this._maxStepHeight + 0.1f))
            {
                return false;
            }

            Vector3 pos = this.transform.position;
            pos.y = hitInfo.point.y + 0.01f; // step up  
            this.transform.position = pos;
        }

        return true;
    }
}
