using Fusion;
using UnityEngine;

public class Player : NetworkBehaviour {

    [SerializeField] private NetworkCharacterController _cc;
    [SerializeField] private Projectile _projectilePrefab;
    [SerializeField] private float _maxChargeTime = 5f;
    [SerializeField] private float _minChargeTime = 0.5f;

    private bool isFireHeld { get; set; }
    private bool isFireAltHeld { get; set; }
    private bool isJumpHeld { get; set; }

    private NetworkButtons _previousButtons;

    TickTimer fireActionDownTick { get; set; }

    public override void FixedUpdateNetwork() {
        if (GetInput(out NetworkInputData data)) {
            data.direction.Normalize();

            // Remove this and use character rotation to do stuff.
            _cc.Move(5 * data.direction * Runner.DeltaTime);

            ProcessInput(data);

            _previousButtons = data.buttons;
        }
    }

    private void ProcessInput(NetworkInputData data) {
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
        fireActionDownTick = TickTimer.CreateFromSeconds(Runner, _maxChargeTime);
    }

    private void ProcessFire() {
        //print($"Fire is held down. {Runner.Tick}");
    }

    private void ProcessFireReleased() {
        //print($"Fire was released. {Runner.Tick}");
        isFireHeld = false;

        if (Runner.IsResimulation) {
            return;
        }

        Runner.Spawn(_projectilePrefab, transform.position + Vector3.up * 2, Quaternion.identity, Object.InputAuthority,
            onBeforeSpawned: (runner, o) => { o.GetBehaviour<Projectile>().Init(scale: Mathf.Max(_maxChargeTime - fireActionDownTick.RemainingTime(runner)!.Value, _minChargeTime)); });
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

    private void OnCollisionEnter(Collision other) {
        // Do a nice bounce to the players. 
    }

}