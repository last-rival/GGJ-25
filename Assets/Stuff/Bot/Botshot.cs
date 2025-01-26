using Fusion;
using UnityEngine;
using UnityEngine.Serialization;

public class Botshot : MonoBehaviour {

    [SerializeField] private Rigidbody2D _rigidbody;
    [SerializeField] private Transform _shootOrigin;
    [SerializeField] private Transform _rotationRoot;
    [SerializeField] private PlayerVisuals _visuals;

    [FormerlySerializedAs("_botProfile")]
    [SerializeField] private Profile _profile;

    [SerializeField] private float _currHp;
    [SerializeField] private float _maxHp;
    [SerializeField] private float _currAir;
    [SerializeField] private float _maxAir;

    [SerializeField] private Projectile _projectile;

    [SerializeField] private float turnRatePerSecond = 180;

    [SerializeField] private PlayerStatusUI _statusUI;

    private float lastShotTime;

    private Player _target;

    public void SetTarget(Player target) {
        _target = target;
    }

    private void Start() {
        FindTarget();
        InitProfile();
    }

    private void FindTarget() {
        if (_target != null) {
            return;
        }

        var player = FindObjectOfType<Player>();

        if (player != null) {
            SetTarget(player);
        }
    }

    private void Update() {
        if (_target == null) {
            return;
        }

        TryShoot();
    }

    void InitProfile() {
        _currHp = _maxHp = _profile.maxHp;
        _currAir = _maxAir = _profile.maxAir;

        // Set bubble size.
        // Update all UI.
        // Make Tip Top Shape.
    }

    public void TryShoot() {
        var elapsedTime = Time.time - lastShotTime;

        _rotationRoot.Rotate(Vector3.forward, turnRatePerSecond * Time.deltaTime);
        _statusUI.SetFireCooldown(Mathf.Clamp01(elapsedTime / _profile.fireCooldown));

        if (elapsedTime < _profile.fireCooldown) {
            EngageThrusters();
            return;
        }

        DisengageThrusters();

        var targetVector =  _target.transform.position - transform.position;
        var angleDelta = Vector2.SignedAngle(_rotationRoot.right, targetVector);

        if (Mathf.Abs(angleDelta) <= 5) {
            lastShotTime = Time.time;

            _currAir -= _profile.fireCost;
            _currAir = Mathf.Max(0, _currAir);
            UpdateBubbleAirStatus();

            var facingDir = _rotationRoot.right;
            _rigidbody.AddForce(facingDir * (_profile.projectileData.shotKnockBack * -1), ForceMode2D.Impulse);

            _target.Runner.Spawn(_projectile, _shootOrigin.position, Quaternion.identity, PlayerRef.None,
                onBeforeSpawned: (_, o) => {
                    o.GetBehaviour<Projectile>().Init(
                        velocity: facingDir * _profile.projectileData.projectileSpeed,
                        owner: PlayerRef.None,
                        profileName: _profile.Name,
                        isBotShot: true
                    );
                });
        }
    }

    private float power;

    public void EngageThrusters() {
        if (_currAir <= _profile.thrusterFailureThreshold) {
            power = 0;

            return;
        }

        var cost = _profile.thrusterCostPerSec * _target.Runner.DeltaTime;
        _currAir -= cost;
        _currAir = Mathf.Max(0, _currAir);

        _visuals.SetThrusterActive(true);
        UpdateBubbleAirStatus();

        var rb = _rigidbody;
        var powerPerSec = _profile.thrusterMaxPower / _profile.fullThrustTime;
        power += powerPerSec * _target.Runner.DeltaTime;
        power = Mathf.Min(_profile.thrusterMaxPower, power);
        var direction = _rotationRoot.transform.right;
        rb.AddForce(direction * power, ForceMode2D.Force);
    }

    public void DisengageThrusters() {
        power = 0;

        _visuals.SetThrusterActive(false);
        UpdateBubbleAirStatus();
    }

    private void UpdateBubbleAirStatus() {
        var fill = Mathf.Clamp01(_currAir / _maxAir);
        _statusUI.SetAirLeft(fill);
        _visuals.SetScale(Vector2.one * (2 * Mathf.Lerp(_profile.minSize, _profile.maxSize, fill)));
    }

    public void Hit(PlayerRef owner, float dataProjectileDamage) {
        _currHp -= dataProjectileDamage;
        _currHp = Mathf.Max(_currHp, 0);

        _statusUI.SetHp(Mathf.Max(_currHp / _maxHp));

        if (_currHp <= Mathf.Epsilon) {
            InitProfile();
            FindObjectOfType<UIManager>().AnnounceMessage("Botshot was shot to death in bubble lust!");
        }
    }

    public void DeathByAirLoss() {
        if (_currAir > Mathf.Epsilon) {
            return;
        }

        FindObjectOfType<UIManager>().AnnounceMessage("Boshot was consumed by eternal darkness of the depths because their bubble burst.");
    }

}