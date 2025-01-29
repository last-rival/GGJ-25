using Fusion;
using UnityEngine;

public class Botshot : MonoBehaviour {

    [SerializeField] private Rigidbody2D _rigidbody;
    [SerializeField] private Transform _visualHolder;

    [SerializeField] private Profile _profile;

    [SerializeField] private float _currHp;
    [SerializeField] private float _maxHp;
    [SerializeField] private float _currAir;
    [SerializeField] private float _maxAir;

    [SerializeField] private Projectile _projectile;

    [SerializeField] private float turnRatePerSecond = 180;

    [SerializeField] private PlayerStatusUI _statusUI;

    [SerializeField] public bool IsGod = true;
    public bool IsMortal => IsGod == false;

    [SerializeField] public bool CanShoot = true;
    [SerializeField] public bool CanMove = true;

    public bool IsDead => Mathf.Approximately(_currHp, 0);

    private float lastShotTime;

    private Player _target;

    private Transform _shootOrigin;
    private Transform _rotationRoot;
    private PlayerVisuals _visuals;

    public void SetTarget(Player target) {
        _target = target;
    }

    private void Start() {
        _visuals = Instantiate(_profile.PlayerPrefab, _visualHolder);
        _shootOrigin = _visuals.projectileSpawnPoint;
        _rotationRoot = _visuals.transform;

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
        LookAndShoot();
    }

    void InitProfile() {
        _currHp = _maxHp = _profile.maxHp;
        _currAir = _maxAir = _profile.maxAir;

        _visuals.SetScale(Vector2.one * 2);
        _visuals.thruster.gameObject.SetActive(true);


        UpdateBubbleAirStatus();
        // Set bubble size.
        // Update all UI.
        // Make Tip Top Shape.
    }

    public void LookAndShoot() {
        if (_visuals == null) {
            return;
        }

        if (_target == null || _target.IsDead) {
            _rotationRoot.Rotate(Vector3.forward, turnRatePerSecond * Time.deltaTime);
            _visuals.SetThrusterActive(true);

            return;
        }

        var elapsedTime = Time.time - lastShotTime;
        var targetVector =  _target.transform.position - transform.position;
        var angleDelta = Vector2.SignedAngle(_rotationRoot.right, targetVector);
        var absAngleDelta = Mathf.Abs(angleDelta);

        if (absAngleDelta > 3f) {
            var rotationPerSec = turnRatePerSecond * Time.deltaTime * Mathf.Sign(angleDelta);
            _rotationRoot.Rotate(Vector3.forward, rotationPerSec);
        }

        _statusUI.SetFireCooldown(Mathf.Clamp01(elapsedTime / _profile.fireCooldown));

        if (elapsedTime < _profile.fireCooldown) {
            EngageThrusters();

            return;
        }

        DisengageThrusters();

        if (CanShoot == false) {
            return;
        }

        if (absAngleDelta > 5) {
            return;
        }

        lastShotTime = Time.time;

        if (IsMortal) {
            _currAir -= _profile.fireCost;
        }

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

    private float power;

    public void EngageThrusters() {
        if (_currAir <= _profile.thrusterFailureThreshold || CanMove == false) {
            power = 0;

            return;
        }

        var cost = _profile.thrusterCostPerSec * _target.Runner.DeltaTime;

        if (IsMortal) {
            _currAir -= cost;
        }
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

        if (Mathf.Approximately(_currAir, 0)) {
            InitProfile();
            FindObjectOfType<UIManager>().AnnounceMessage("Botshot's bubble air supply did not last!");
        }
    }

    public void Hit(bool wasShotByPlayer, float dataProjectileDamage) {
        if (IsMortal) {
            _currHp -= dataProjectileDamage;
        }

        _currHp = Mathf.Max(_currHp, 0);

        UpdateHpStatus(wasShotByPlayer);

        if (wasShotByPlayer == false) {
            GrantKill(-1);
        }

        FindObjectOfType<ArenaManager>().BrawlerWasKilled();
    }

    public int KillCounter;

    public void GrantKill(int kills) {
        KillCounter += kills;
    }

    private void UpdateHpStatus(bool wasShotByPlayer) {
        _statusUI.SetHp(Mathf.Max(_currHp / _maxHp));

        if (Mathf.Approximately(_currHp, 0)) {
            InitProfile();
            FindObjectOfType<UIManager>().AnnounceMessage(wasShotByPlayer ? "Botshot was shot to death in blublust!" : "Botshot was short circuited by their own bubble!");
        }
    }

    public void ReviveAndHeal() {
        _currAir = _maxAir;
        lastShotTime = 0;

        if (IsDead) {
            _currHp = _maxHp;
        }
        else {
            var fill = _currHp / _maxHp;

            if (fill < 0.5f) {
                _currHp = _maxHp * 0.5f;
            }
        }
        
        UpdateHpStatus(false);
        UpdateBubbleAirStatus();
    }
}