using Fusion;
using UnityEngine;

public class Projectile : NetworkBehaviour {

    [Networked] public int hitPoints { get; set; }
    private int maxHitPoints;

    [Networked] private NetworkString<_16> profileName { get; set; }

    [Networked] public PlayerRef owner { get; set; }
    [SerializeField] private Rigidbody2D _rigidbody;
    [SerializeField] private ProfileDatabase _profileDatabase;
    [SerializeField] private GameObject _projectileKillVFX;

    private ProjectileData _data;

    private bool isBotShot;

    public void Init(Vector2 velocity, PlayerRef owner, NetworkString<_16> profileName, bool isBotShot = false) {
        _rigidbody.velocity = velocity;
        this.owner = owner;
        this.profileName = profileName;
        this.isBotShot = isBotShot;
    }

    public override void Spawned() {
        _data = _profileDatabase.GetProfileByName(profileName.ToString()).projectileData;

        hitPoints = _data.projectileHitPoints;
        maxHitPoints = hitPoints;

        transform.localScale = Vector3.one * _data.projectileSize;

        if (Runner.IsClient) {
            Runner.SetIsSimulated(Object, true);
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState) {
        Instantiate(_projectileKillVFX, transform.position, Quaternion.identity);
    }

    public override void FixedUpdateNetwork() {
    }

    void OnCollisionEnter2D(Collision2D collision) {

        if (Object == null || Object.IsValid == false) {
            return;
        }


        if (Object.HasStateAuthority == false) {
            return;
        }

        var hit = TryHitPlayer(collision.collider.attachedRigidbody);

        if (Runner.IsSinglePlayer) {
            hit = hit || TryHitBot(collision.collider.attachedRigidbody);
        }

        if (hit) {
            RpcPlayHitVFX(Runner, collision.contacts[0].point, _data.projectileDamage);
        }

        hit = hit || TryHitProjectile(collision.collider.attachedRigidbody);

        hitPoints--;

        if (hitPoints == 0 || hit) {
            // Spawn a pop VFX.
            Runner.Despawn(Object);
        }
        else {
            RpcPlayBounceSFX();
        }
    }

    bool TryHitPlayer(Rigidbody2D rb) {
        if (rb == null) {
            return false;
        }

        var player = rb.GetComponent<Player>();

        if (player == null || player.Object == null || player.Object.IsValid == false) {
            return false;
        }

        if (player.Object.InputAuthority == owner) {
            if (maxHitPoints == hitPoints) {
                return false;
            }
        }

        player.Hit(owner, _data.projectileDamage);

        return true;
    }

    bool TryHitProjectile(Rigidbody2D rb) {
        if (rb == null) {
            return false;
        }

        var projectile = rb.GetComponent<Projectile>();

        if (projectile == null || projectile.Object == null || projectile.Object.IsValid == false) {
            return false;
        }

        Runner.Despawn(projectile.Object);

        return true;
    }

    bool TryHitBot(Rigidbody2D rb) {
        if (rb == null) {
            return false;
        }

        var botshot = rb.GetComponent<Botshot>();

        if (botshot == null) {
            return false;
        }

        if (isBotShot && hitPoints == maxHitPoints) {
            return false;
        }

        botshot.DeathByHit(!isBotShot, _data.projectileDamage);
        return true;
    }


    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RpcPlayBounceSFX() {
        FindObjectOfType<AudioManager>().PlayBounce();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public static void RpcPlayHitVFX(NetworkRunner runner, Vector2 position, float damage) {
        FindObjectOfType<FxManager>().ShowHitVFX(position, damage);
    }

}