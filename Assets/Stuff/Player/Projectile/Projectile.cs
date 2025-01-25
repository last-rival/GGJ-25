using Fusion;
using UnityEngine;

public class Projectile : NetworkBehaviour {

    [Networked] public int hitPoints { get; set; }
    private int maxHitPoints;
    [Networked] public PlayerRef owner { get; set; }
    [Networked] public float spawnScale { get; set; }
    [Networked] public float damage { get; set; }

    [SerializeField] private Rigidbody2D _rigidbody;

    public void Init(float scale, Vector2 velocity, PlayerRef owner, int hitPoints, float damage) {
        _rigidbody.velocity = velocity;
        spawnScale = scale;

        this.owner = owner;
        this.hitPoints = hitPoints;
        this.damage = damage;
        maxHitPoints = hitPoints;
    }

    public override void Spawned() {
        transform.localScale = Vector3.one * spawnScale;
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

        ProcessHit(collision.collider.attachedRigidbody);

        hitPoints--;

        if (hitPoints == 0) {
            Runner.Despawn(Object);
        }
    }

    void ProcessHit(Rigidbody2D rb) {
        if (rb == null) {
            return;
        }

        var player = rb.GetComponent<Player>();

        if (player == null) {
            return;
        }

        if (player.Object.InputAuthority == owner) {
            if (maxHitPoints == hitPoints) {
                return;
            }
        }

        player.HitPlayer(owner, damage);
    }

}