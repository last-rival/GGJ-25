using Fusion;
using Fusion.Addons.Physics;
using UnityEngine;

public class Player : NetworkBehaviour {

    [SerializeField] private NetworkRigidbody2D _rigidbody2D;
    [SerializeField] private Projectile _projectilePrefab;
    [SerializeField] private PlayerStatusUI _canvas;
    [SerializeField] private GameObject _deathFX;

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

    public override void Despawned(NetworkRunner runner, bool hasState) {
        Instantiate(_deathFX, Object.transform.position, Quaternion.identity);
        var ui = FindObjectOfType<UIManager>();
        ui?.AnnounceMessage($"{_currentProfile.name} has perished");
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

                case nameof(thrustersEngaged):
                    DoThrusterEffects(thrustersEngaged);

                    break;
            }
        }

        if (Object != null && Object.IsValid &&  Object.HasInputAuthority) {
            UpdateUI();
        }
    }

    private void Update() {
        if (visual == null) {
            return;
        }

        DoReloadFill();
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
        thrustersEngaged = true;
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
        thrustersEngaged = false;

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
        if (Object == null || Object.IsValid == false) {
            return;
        }

        if (other.collider.attachedRigidbody?.GetComponent<Projectile>()) {
            return;
        }

        if (Object.HasStateAuthority) {
            RpcPlayBounceSFX();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcPlayBounceSFX() {
        FindObjectOfType<AudioManager>().PlayBounce();
    }

    #region Warfare

    [Networked] private TickTimer lastShotTimer { get; set; }

    [field: SerializeField]
    [Networked] private float hitPoints { get; set; }
    private float maxHitPoints;

    [field: SerializeField]
    [Networked] private float airCapacity { get; set; }
    private float maxAirCapacity;

    public bool TryShootProjectile() {
        if (lastShotTimer.ExpiredOrNotRunning(Runner) == false) {
            return false;
        }

        if (visual == null) {
            return false;
        }

        lastShotTimer = TickTimer.CreateFromSeconds(Runner, _currentProfile.fireCooldown);

        var remainingAir = airCapacity - _currentProfile.fireCost;
        airCapacity = Mathf.Max(0, remainingAir);

        var facingDir = visual.transform.right;
        _rigidbody2D.Rigidbody.AddForce(_currentProfile.projectileData.shotKnockBack * facingDir * -1, ForceMode2D.Impulse);

        Runner.Spawn(_projectilePrefab, visual.projectileSpawnPoint.position, Quaternion.identity, Object.InputAuthority,
            onBeforeSpawned: (_, o) => {
                o.GetBehaviour<Projectile>().Init(
                    velocity: facingDir * _currentProfile.projectileData.projectileSpeed,
                    owner: Object.InputAuthority,
                    profileName: profileName
                );
            });

        if (airCapacity == 0) {
            KillPlayer();
        }

        return true;
    }

    private float power = 0;
    [Networked]
    private bool thrustersEngaged { get; set; }

    public void EngageThrusters() {
        if (airCapacity <= _currentProfile.thrusterFailureThreshold) {
            power = 0;
            thrustersEngaged = false;

            return;
        }

        thrustersEngaged = true;

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
        thrustersEngaged = false;
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
            KillPlayer();
        }
    }

    private void DoHitEffects(float currHealth) {
        if (_canvas == null) {
            return;
        }

        _canvas.SetHp(currHealth / maxHitPoints);
    }

    private void DoThrusterEffects(bool engaged) {
        if (visual == null) {
            return;
        }

        visual.SetThrusterActive(engaged);
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

        if (Object.HasInputAuthority == false) {
            return;
        }

        if (airCapacity < _currentProfile.thrusterFailureThreshold) {
            if (visual.thruster.activeSelf) {
                visual.DropThruster();
                RpcDropThruster();
            }
        }

    }

    private void DoReloadFill() {
        if (_canvas == null) {
            return;
        }

        var elapsedTime = lastShotTimer.RemainingTime(Runner).GetValueOrDefault(0);
        var fill = (_currentProfile.fireCooldown - elapsedTime) / _currentProfile.fireCooldown;
        _canvas.SetFireCooldown(fill);
    }

    private void KillPlayer() {
        if (Object.HasStateAuthority) {
            Runner.Despawn(Object);
        }
    }

    #endregion

    #region  Profile

    [Networked] private NetworkString<_16> profileName { get; set; }

    [Header("Profiles")]
    [SerializeField] private Profile _currentProfile;

    [SerializeField] private ProfileDatabase _profiles;
    [SerializeField] private Transform _visualHolder;

    public PlayerVisuals visual { get; set; }

    void SetProfile(string profileName) {
        this.profileName = profileName;
        _currentProfile = _profiles.GetProfileByName(profileName);

        hitPoints = _currentProfile.maxHp;
        maxHitPoints = hitPoints;

        airCapacity = _currentProfile.maxAir;
        maxAirCapacity = airCapacity;

        thrustersEngaged = false;

        if (visual != null) {
            Destroy(visual.gameObject);
        }

        visual = Instantiate(_currentProfile.PlayerPrefab, _visualHolder);
        FindObjectOfType<UIManager>().AnnounceMessage($"{_currentProfile.Name} has arrived as a Brawler!");
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RpcChangeClassTo(NetworkString<_16> className) {
        SetProfile(className.ToString());
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RpcDropThruster() {
        if (visual == null) {
            return;
        }

        visual.DropThruster();
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