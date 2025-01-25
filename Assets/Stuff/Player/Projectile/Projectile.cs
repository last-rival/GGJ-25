using Fusion;
using UnityEngine;

public class Projectile : NetworkBehaviour {

    [Networked] private TickTimer tickTimer { get; set; }
    [Networked] public PlayerRef owner { get; set; }
    [Networked] public float power { get; set; }
    [SerializeField] private float _lifeTime;
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private Vector3 _velocity;

    public void Init(float scale) {
        power = scale;
        tickTimer = TickTimer.CreateFromSeconds(Runner, _lifeTime);
        _rigidbody.velocity = _velocity;
    }

    public override void Spawned() {
        transform.localScale = Vector3.one * power;
        print($"Power set {power}");
    }

    public override void FixedUpdateNetwork() {
        if (tickTimer.ExpiredOrNotRunning(Runner)) {
            Runner.Despawn(Object);
        }
    }

}