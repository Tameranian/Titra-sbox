using System;
using Sandbox;

public partial class Player : Component
{
	public PlayerController playerController;
	private SkinnedModelRenderer ragdollBodyRenderer;
	private bool isRagdolling = false;
	private bool returningCamera = false;
	private GameObject ragdoll;
 	public TimeSince timeSinceRagdoll;
	private Rotation cachedRotation;
	public bool grabbingItem;
	public bool isSleeping;
	
	protected override void OnStart()
	{
		playerController = Components.Get<PlayerController>();
		InitializeStats();
	}

	protected override void OnUpdate()
	{
		UpdateUI();

		// Log.Info(timeSinceRagdoll);

		if(InputActions.TryRagdoll && !isRagdolling && !isSleeping)
		{
			EnableRagdoll();
			return;
		}

		if(isRagdolling)
		{
			if (InputActions.TryJump || InputActions.TryRagdoll)
			{
				ToggleRagdoll();
				return;
			}

			UpdateRagdollCamera();
		}

		if(returningCamera)
		{
			ReturnCamera();
		}
		
	}

	private void EnableRagdoll()
    {
		ragdoll = playerController.CreateRagdoll();
		Scene.Camera.GameObject.SetParent(ragdoll);
		ragdollBodyRenderer = ragdoll.Components.Get<SkinnedModelRenderer>();
		ragdollBodyRenderer.CreateBoneObjects = true;
		ragdollBodyRenderer.SetBodyGroup(0, 1);
		cachedRotation = playerController.Renderer.GetBoneObject("spine_1").Transform.World.Rotation;
		playerController.Enabled = false;
		playerController.Renderer.Enabled = false;
		isRagdolling = true;
		playerController.UseCameraControls = false;

    }

	private void ToggleRagdoll()
    {
		Scene.Camera.GameObject.SetParent(null);
		isRagdolling = false;
		playerController.Body.WorldRotation = cachedRotation;
		playerController.Body.WorldPosition = ragdoll.WorldPosition;
		playerController.Enabled = true;
		playerController.Renderer.Enabled = true;
		returningCamera = true;
		playerController.WorldPosition = Scene.Camera.WorldPosition;
		ragdoll.Destroy();

    }

	private void ReturnCamera()
    {
		if(Vector3.DistanceBetween(Scene.Camera.WorldPosition, playerController.EyeTransform.Position) < 1)
		{
			returningCamera = false;
			playerController.UseCameraControls = true;
			return;
		}
		Scene.Camera.WorldPosition = Vector3.Lerp(Scene.Camera.WorldPosition, playerController.EyeTransform.Position, 10 * Time.Delta);
		Scene.Camera.WorldRotation = Rotation.Slerp(Scene.Camera.WorldRotation, playerController.EyeTransform.Rotation, 10 * Time.Delta);
    }

	private void UpdateRagdollCamera()
    {
        var eyeLTransform = ragdollBodyRenderer.GetBoneObject("eye_L").Transform;
        var eyeRTransform = ragdollBodyRenderer.GetBoneObject("eye_R").Transform;

        var midpoint = (eyeLTransform.World.Position + eyeRTransform.World.Position) / 2;
        var forwardDirection = (eyeLTransform.World.Rotation.Forward + eyeRTransform.World.Rotation.Forward).Normal;
        var upDirection = (eyeLTransform.World.Rotation.Left + eyeRTransform.World.Rotation.Left).Normal;

        var backwardOffset = forwardDirection * -10f; 
        var targetPosition = midpoint + backwardOffset;
        if (Scene.Camera != null)
        {
            Scene.Camera.WorldPosition = Scene.Camera.WorldPosition.LerpTo(targetPosition, 30 * Time.Delta);
            Scene.Camera.WorldRotation  = Rotation.Slerp(Scene.Camera.WorldRotation , Rotation.LookAt(forwardDirection, upDirection), 10 * Time.Delta);
        }
    }
}
