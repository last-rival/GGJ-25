using Fusion;
using Fusion.Addons.Physics;
using UnityEngine;

public class Player : NetworkBehaviour {

    [SerializeField] private NetworkRigidbody2D _rigidbody2D;
    [SerializeField] private Projectile _projectilePrefab;

    ChangeDetector changeDetector { get; set; }

    public override void Spawned() {
        changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        InitUI();
        SetClass(classId);

        if (Runner.IsClient) {
            Runner.SetIsSimulated(Object, true);
        }
    }

    public override void Render() {
        foreach (var change in changeDetector.DetectChanges(this)) {
            switch (change) {
                case nameof(zLook):
                    SetLook(zLook);

                    break;

                case nameof(classId):
                    SetClass(classId);

                    break;
            }
        }

        if (Object != null && Object.IsValid &&  Object.HasInputAuthority) {
            UpdateUI();
        }
    }

    private void SetLook(float lookAngle) {
        classVisuals.transform.rotation = Quaternion.Euler(0f, 0f, lookAngle);
    }

    private bool isFireHeld { get; set; }
    private bool isFireAltHeld { get; set; }
    private bool isJumpHeld { get; set; }

    private NetworkButtons _previousButtons;

    public override void FixedUpdateNetwork() {
        if (GetInput(out NetworkInputData data)) {
            ProcessLook(data.cursorPosition);
            ProcessKeyPress(data);
            _previousButtons = data.buttons;
        }
    }

    [field: SerializeField]
    [Networked] private float zLook { get; set; }

    private void ProcessLook(Vector2 cursorWorldPos) {
        var playerPos = (Vector2)_rigidbody2D.RBPosition;
        var direction = cursorWorldPos - playerPos;
        var angle = Vector2.SignedAngle(Vector2.right, direction.normalized);
        zLook = angle;

        Debug.DrawRay(playerPos, direction, Color.red);
    }

    private void ProcessKeyPress(NetworkInputData data) {
        if (data.buttons.WasPressed(_previousButtons, ActionButtons.Fire)) {
            ProcessFirePressed();
        }

        if (data.buttons.WasReleased(_previousButtons, ActionButtons.Fire)) {
            ProcessFireReleased();
        }

        if (data.buttons.WasPressed(_previousButtons, ActionButtons.FireAlt)) {
            ProcessFireAltPressed();
        }

        if (data.buttons.WasReleased(_previousButtons, ActionButtons.FireAlt)) {
            ProcessFireAltReleased();
        }

        if (isFireHeld) {
            ProcessFire();
        }

        if (isFireAltHeld) {
            ProcessAltFire();
        }

        if (isJumpHeld) {
            ProcessJump();
        }
    }

    private void ProcessFirePressed() {
        //print($"Fire Main Pressed. {Runner.Tick}");
        isFireHeld = true;

        if (Runner.IsResimulation || Runner.IsClient) {
            return;
        }

        TryShootProjectile();
    }

    private void ProcessFire() {
        //print($"Fire is held down. {Runner.Tick}");
    }

    private void ProcessFireReleased() {
        //print($"Fire was released. {Runner.Tick}");
        isFireHeld = false;
    }

    private void ProcessFireAltPressed() {
        //print($"Fire Alt Main Pressed. {Runner.Tick}");
        isFireAltHeld = true;
    }

    private void ProcessAltFire() {
        //print($"Fire Alt is held down. {Runner.Tick}");
    }

    private void ProcessFireAltReleased() {
        //print($"Fire Alt was released. {Runner.Tick}");
        isFireAltHeld = false;
    }

    private void ProcessJumpPressed() {
        //print($"Jump is Pressed. {Runner.Tick}");
        isJumpHeld = true;
    }

    private void ProcessJump() {
        //print($"Jump is held down. {Runner.Tick}");
    }

    private void ProcessJumpReleased() {
        //print($"Jump was released. {Runner.Tick}");
        isJumpHeld = false;
    }


    private void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.CompareTag("Player")) {
            print("Hit a player");
        }
    }

    #region Warfare

    float lastShotTime;

    [Networked] private float hitPoints { get; set; }
    private float maxHitPoints;

    public bool TryShootProjectile() {
        var timeDelta = Time.time - lastShotTime;

        if (timeDelta < _currentProfile.fireCooldown) {
            return false;
        }

        lastShotTime = Time.time;
        Runner.Spawn(_projectilePrefab, classVisuals.projectileSpawnPoint.position, Quaternion.identity, Object.InputAuthority,
            onBeforeSpawned: (_, o) => {
                o.GetBehaviour<Projectile>().Init(
                    scale: _currentProfile.projectileSize,
                    velocity: classVisuals.transform.right * (_currentProfile.projectileSpeed + _rigidbody2D.Rigidbody.velocity.magnitude),
                    owner: Object.InputAuthority,
                    hitPoints: _currentProfile.projectileHitPoints,
                    damage: _currentProfile.projectileDamage
                );
            });

        return true;
    }

    public void Hit(PlayerRef owner, float damage) {
        if (Object.HasStateAuthority == false) {
            return;
        }

        if (owner == Object.InputAuthority) {
            print($"Let it be known that this smart ass were nicked by their own bubble for {damage}");
        }
        else {
            print($"Woah buddy... good shot! Worth {damage}");
        }

        hitPoints = Mathf.Max(0, hitPoints - damage);

        if (Mathf.Approximately(hitPoints, 0)) {
            print($"Player {Object.InputAuthority} has died in the battle field.");
            Runner.Despawn(Object);
        }
    }

    #endregion

    #region  Classes

    [Networked] private ClassType classId { get; set; }

    [Header("Profiles")]
    [SerializeField] private Profile _currentProfile;

    [SerializeField] private Profile[] _profiles;
    [SerializeField] private Transform _visualHolder;
    public ClassVisuals classVisuals { get; set; }

    void SetClass(ClassType classType) {
        foreach (var profile in _profiles) {
            if (profile.id.Equals(classType)) {
                _currentProfile = profile;

                break;
            }
        }

        hitPoints = _currentProfile.maxHp;
        maxHitPoints = hitPoints;

        if (classVisuals != null) {
            Destroy(classVisuals.gameObject);
        }

        classVisuals = Instantiate(_currentProfile.classPrefab, _visualHolder);
        // Update class visuals
    }


    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RpcChangeClassTo(ClassType classType) {
        SetClass(classType);
    }

    #endregion

    #region UI Updates

    private UIManager _uiManager;

    public void InitUI() {
        if (Object.HasInputAuthority) {
            _uiManager = FindObjectOfType<UIManager>();
        }
    }

    public void UpdateUI() {
        if (_uiManager == null) {
            return;
        }
    }

    #endregion

}