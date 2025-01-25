using Fusion;

public class Player : NetworkBehaviour {

    private NetworkCharacterController _cc;

    private void Awake() {
        _cc = GetComponent<NetworkCharacterController>();
    }

    private NetworkButtons _previousButtons;

    [Networked] private bool isFireHeld { get; set; }
    [Networked] private bool isFireAltHeld { get; set; }

    [Networked] private bool isJumpHeld { get; set; }

    public override void FixedUpdateNetwork() {
        if (GetInput(out NetworkInputData data)) {
            data.direction.Normalize();

            // Remove this and use character rotation to do stuff.
            _cc.Move(5 * data.direction * Runner.DeltaTime);

            ProcessKeyPresses(data);

            _previousButtons = data.buttons;
        }
    }

    private void ProcessKeyPresses(NetworkInputData data) {
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
        print($"Fire Main Pressed. {Runner.Tick}");
        isFireHeld = true;
    }

    private void ProcessFire() {
        print($"Fire is held down. {Runner.Tick}");
    }

    private void ProcessFireReleased() {
        print($"Fire was released. {Runner.Tick}");
        isFireHeld = false;
    }

    private void ProcessFireAltPressed() {
        print($"Fire Alt Main Pressed. {Runner.Tick}");
        isFireAltHeld = true;
    }

    private void ProcessAltFire() {
        print($"Fire Alt is held down. {Runner.Tick}");
    }

    private void ProcessFireAltReleased() {
        print($"Fire Alt was released. {Runner.Tick}");
        isFireAltHeld = false;
    }

    private void ProcessJumpPressed() {
        print($"Jump is Pressed. {Runner.Tick}");
        isJumpHeld = true;
    }

    private void ProcessJump() {
        print($"Jump is held down. {Runner.Tick}");
    }

    private void ProcessJumpReleased() {
        print($"Jump was released. {Runner.Tick}");
        isJumpHeld = false;
    }

}