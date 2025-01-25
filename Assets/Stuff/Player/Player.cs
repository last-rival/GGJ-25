using Fusion;
using Fusion.Addons.Physics;
using UnityEngine;

public class Player : NetworkBehaviour {

    [SerializeField] private NetworkRigidbody2D _rigidbody2D;
    [SerializeField] private Projectile _projectilePrefab;
    [SerializeField] private PlayerStatusUI _canvas;

    ChangeDetector changeDetector { get; set; }

    public override void Spawned() {
        changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        InitUI();

        if (Object.HasInputAuthority) {
            RpcChangeClassTo(FindObjectOfType<GameRunner>().profileName);
        }
        else {
            SetProfile(profileName.ToString());
        }

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

                case nameof(profileName):
                    SetProfile(profileName.ToString());

                    break;

                case nameof(hitPoints):
                    DoHitEffects(hitPoints);

                    break;

                case nameof(airCapacity):
                    DoAirEffects(airCapacity);

                    break;
            }
        }

        DoReloadFill();
        DoThrusterEffects();

        if (Object != null && Object.IsValid &&  Object.HasInputAuthority) {
            UpdateUI();
        }
    }

    private void SetLook(float lookAngle) {
        if (visual == null) {
            return;
        }

        visual.transform.rotation = Quaternion.Euler(0f, 0f, lookAngle);
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

        if (data.buttons.WasPressed(_previousButtons, ActionButtons.Jump)) {
            ProcessJumpPressed();
        }

        if (data.buttons.WasReleased(_previousButtons, ActionButtons.Jump)) {
            ProcessJumpReleased();
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
        if (Object == null || Object.HasStateAuthority == false) {
            return;
        }

        EngageThrusters();
    }

    private void ProcessFireAltReleased() {
        //print($"Fire Alt was released. {Runner.Tick}");
        isFireAltHeld = false;

        if (Object.HasInputAuthority == false) {
            return;
        }

        DisengageThrusters();
    }

    private void ProcessJumpPressed() {
        //print($"Jump is Pressed. {Runner.Tick}");
        isJumpHeld = true;
    }

    private void ProcessJump() {
        //print($"Jump is held. {Runner.Tick}");
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

    [field: SerializeField]
    [Networked] private float hitPoints { get; set; }
    private float maxHitPoints;

    [field: SerializeField]
    [Networked] private float airCapacity { get; set; }
    private float maxAirCapacity;

    public bool TryShootProjectile() {
        var timeDelta = Time.time - lastShotTime;

        if (timeDelta < _currentProfile.fireCooldown) {
            return false;
        }

        if (visual == null) {
            return false;
        }

        lastShotTime = Time.time;

        var remainingAir = airCapacity - _currentProfile.fireCost;
        airCapacity = Mathf.Max(0, remainingAir);

        var facingDir = visual.transform.right;
        _rigidbody2D.Rigidbody.AddForce(_currentProfile.projectileData.shotKnockBack * facingDir * -1, ForceMode2D.Impulse);

        Runner.Spawn(_projectilePrefab, visual.projectileSpawnPoint.position, Quaternion.identity, Object.InputAuthority,
            onBeforeSpawned: (_, o) => {
                o.GetBehaviour<Projectile>().Init(
                    velocity: facingDir * (_currentProfile.projectileData.projectileSpeed + _rigidbody2D.Rigidbody.velocity.magnitude),
                    owner: Object.InputAuthority,
                    profileName: profileName
                );
            });

        if (airCapacity == 0) {
            Runner.Despawn(Object);
        }

        return true;
    }

    private float power = 0;

    public void EngageThrusters() {
        if (airCapacity <= _currentProfile.thrusterFailureThreshold) {
            power = 0;

            return;
        }

        var cost = _currentProfile.thrusterCostPerSec * Runner.DeltaTime;
        airCapacity -= cost;
        airCapacity = Mathf.Max(0, airCapacity);
        var rb = _rigidbody2D.Rigidbody;
        var powerPerSec = _currentProfile.thrusterMaxPower / _currentProfile.fullThrustTime;
        power += powerPerSec * Runner.DeltaTime;
        power = Mathf.Min(_currentProfile.thrusterMaxPower, power);
        var direction = visual.transform.right;
        rb.AddForce(direction * power, ForceMode2D.Force);
    }

    public void DisengageThrusters() {
        power = 0;
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

    private void DoHitEffects(float currHealth) {
        _canvas.SetHp(currHealth / maxHitPoints);

        if (currHealth == 0) {
            print("Dead VFX play");
        }
        else {
            print("Hit VFX play");
        }
    }

    private void DoThrusterEffects() {
        if (visual == null) {
            return;
        }

        if (Mathf.Approximately(power, 0)) {
            visual.SetThrusterActive(false);

            return;
        }

        visual.SetThrusterActive(true);
    }

    private void DoAirEffects(float airCapacity) {
        if (visual == null) {
            return;
        }

        var fill = airCapacity / maxAirCapacity;
        _canvas.SetAirLeft(fill);

        var min = _currentProfile.minSize;
        var max = _currentProfile.maxSize;
        var origScale = _currentProfile.PlayerPrefab.bubble.localScale;
        var scale = Mathf.Lerp(min, max, fill);
        visual.SetScale(origScale * scale);

        if (airCapacity < _currentProfile.thrusterFailureThreshold) {
            // Show VFX of the thruster basting off.
            visual.thruster.SetActive(false);
        }

    }

    private void DoReloadFill() {
        var elapsedTime = Time.time - lastShotTime;
        var fill = elapsedTime / _currentProfile.fireCooldown;
        _canvas.SetFireCooldown(fill);
    }

    #endregion

    #region  Classes

    [Networked] private NetworkString<_16> profileName { get; set; }

    [Header("Profiles")]
    [SerializeField] private Profile _currentProfile;

    [SerializeField] private ProfileDatabase _profiles;
    [SerializeField] private Transform _visualHolder;

    public PlayerVisuals visual { get; set; }

    void SetProfile(string profileName) {
        _currentProfile = _profiles.GetProfileByName(profileName);

        hitPoints = _currentProfile.maxHp;
        maxHitPoints = hitPoints;

        airCapacity = _currentProfile.maxAir;
        maxAirCapacity = airCapacity;

        if (visual != null) {
            Destroy(visual.gameObject);
        }

        visual = Instantiate(_currentProfile.PlayerPrefab, _visualHolder);
        // Update class visuals
    }


    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RpcChangeClassTo(NetworkString<_16> className) {
        SetProfile(className.ToString());
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