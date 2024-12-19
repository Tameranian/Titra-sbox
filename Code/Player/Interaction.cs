using System;
using System.Threading.Tasks;
using Sandbox;

public sealed class Interaction : Component
{
	PlayerController playerController;
	public Interactable heldInteractable;
	public bool IsPlacementAllowed = false;
	[Property, Range(.1f, 1)] public float MovementSmoothness { get; set; } = 1.0f;
	[Property, Range(1, 100)] public float GridSize { get; set; } = 1.0f;
	private Rotation targetHeldObjectRotation;
	public bool snapEnabled = false;
	public bool placingBlocked = false;
	private bool rotationAdjusted;
	private float scrollDistance = 1.0f;
	private Vector3 targetHeldObjectPosition;
	[Property]
	private Curve scaleEffectCurve;

	protected override void OnStart()
	{
		playerController = Components.Get<PlayerController>();
	}

	protected override void OnUpdate()
	{
		if (heldInteractable != null)
		{
			HandleHeldItem(heldInteractable);
		}
	}

	private void HandleHeldItem(Interactable interactable)
	{
		if (interactable != null )
		{	
			var color = snapEnabled ? (placingBlocked ? Color.Red : Color.Green) : Color.White;
			var alpha = snapEnabled ? 0.5f : 1.0f;
			if(!interactable.isBeingPlaced)
				UpdateHeldItemTint(interactable, color, alpha);

			if (CanPlace() && InputActions.TryUse)
			{
				if (snapEnabled)
					ReleaseHeldItem(interactable, true);
				else
					ReleaseHeldItem(interactable, false);
			}
			else
			{
				HoldItem(interactable);
			}
		}
	}

	private void UpdateHeldItemTint(Interactable interactable, Color color, float alpha)
	{
		interactable.modelRenderer.Tint = new Color(color.r, color.g, color.b, alpha);
	}

	private void HoldItem(Interactable interactable)
	{		
		float scrollDelta = Input.MouseWheel.y;
		scrollDistance = MathX.Clamp(scrollDistance + scrollDelta * 0.1f, .45f, 1.25f);
		var finalReachLength =  playerController.EyeTransform.Position + playerController.EyeTransform.Forward*
			(playerController.ReachLength - 30) * scrollDistance;

		var holdTrace = Scene.Trace.Ray(playerController.EyeTransform.Position + playerController.EyeTransform.Forward,
			finalReachLength)
			.WithoutTags("trigger", "player", "held")
			.Run();

		if (InputActions.TryWalk)
		{
			snapEnabled = !snapEnabled;
			rotationAdjusted = false;
		}

		var traceTargetPosition = holdTrace.Hit ? holdTrace.HitPosition + holdTrace.Normal * 2 : finalReachLength;

		var cameraRotation = Scene.Camera.WorldRotation;
		if (snapEnabled)
		{
			if(!interactable.isBeingPlaced)
			targetHeldObjectPosition = SnapToGrid(traceTargetPosition, GridSize);

			if(!rotationAdjusted)
			{
				targetHeldObjectRotation = SnapToRotation(Rotation.FromAxis(Vector3.Up, cameraRotation.Angles().yaw), 45);
			}
			interactable.ToggleCollider(false);
		}
		else
		{
			targetHeldObjectPosition = traceTargetPosition;
			interactable.ToggleCollider(true);
			targetHeldObjectRotation = Rotation.FromAxis(Vector3.Up, cameraRotation.Angles().yaw);
		}

		UpdateHeldObjectRotation();

		interactable.physicsBody.SmoothMove(targetHeldObjectPosition, snapEnabled ? MovementSmoothness * .5f : MovementSmoothness, Time.Delta);
		interactable.physicsBody.SmoothRotate(targetHeldObjectRotation, snapEnabled ? MovementSmoothness * .5f : MovementSmoothness, Time.Delta);
	}

	private void UpdateHeldObjectRotation()
	{
		float rotationAngle = 0;

		if (InputActions.TryRightClick)
		{
			rotationAngle = 45;
		}
		else if (InputActions.TryLeftClick)
		{
			rotationAngle = -45;
		}

		if (rotationAngle != 0)
		{
			rotationAdjusted = true;
			targetHeldObjectRotation *= Rotation.FromAxis(Vector3.Up, rotationAngle);
		}

		if (InputActions.TryWalk)
		{
			targetHeldObjectRotation = SnapToRotation(targetHeldObjectRotation, 45);
		}
	}

	private async void ReleaseHeldItem(Interactable interactable, bool placing)
	{
		interactable.isBeingPlaced = placing;
		if (placing)
		{
			Log.Info("Placing Interactable");
			Sound.Play( interactable.placeSound, WorldPosition );
			interactable.ToggleRigidbodyLock(true);
					await Task.WhenAll(ScaleUpAndDownEffect(interactable),
			UpdateTintAsync(interactable, interactable.modelRenderer.Tint, Color.White, interactable.modelRenderer.Tint.a, 1.0f, 100));
			
		}
		else
		{
			Log.Info("Dropping Interactable");
			interactable.ToggleRigidbodyLock(false);
			
		}

		IsPlacementAllowed = false;
		interactable.ToggleCollider(true);
		interactable.isHeld = false;
		scrollDistance = 1.0f;
		interactable.isBeingPlaced = false;
		interactable.Tags.Remove("held");
		heldInteractable = null;
	}

	private async Task UpdateTintAsync(Interactable interactable, Color initialColor,
		Color targetColor, float initialAlpha, float targetAlpha, int duration)
	{
		var elapsedTime = 0f;

		while (elapsedTime < duration)
		{
			float t = elapsedTime / duration;
			Color lerpedColor = Color.Lerp(initialColor, targetColor, t);
			float lerpedAlpha = MathX.Lerp(initialAlpha, targetAlpha, t);
			interactable.modelRenderer.Tint = new Color(lerpedColor.r, lerpedColor.g, lerpedColor.b, lerpedAlpha);
			elapsedTime += Time.Delta * 1000; 
			await Task.Delay(1);
		}
		interactable.modelRenderer.Tint = new Color(targetColor.r, targetColor.g, targetColor.b, targetAlpha);
	}

	private async Task ScaleUpAndDownEffect(Interactable interactable)
	{
		var scale = interactable.LocalScale;
		var shrunkScale = scale * .65f; 
		var expandedScale = scale *  1.1f; 
		var duration = 200;

		await TweenScaleAsync(interactable, scale, shrunkScale, duration / 6);

		await TweenScaleAsync(interactable, shrunkScale, expandedScale, duration / 2);
		
		await TweenScaleAsync(interactable, expandedScale, scale, duration / 2);
	}

	private async Task TweenScaleAsync(Interactable interactable, Vector3 initialScale, Vector3 targetScale, int duration)
	{
		var elapsedTime = 0f;

		while (elapsedTime < duration)
		{
			float t = elapsedTime / duration;
			float curveValue = scaleEffectCurve.Evaluate(t);
			interactable.LocalScale = Vector3.Lerp(initialScale, targetScale, curveValue);
			elapsedTime += Time.Delta * 1000; 
			await Task.Delay(1);
		}
		interactable.LocalScale = targetScale;
	}

	private Rotation SnapToRotation(Rotation rotation, float angleIncrement)
	{
		var angles = rotation.Angles();
		angles.yaw = MathF.Round(angles.yaw / angleIncrement) * angleIncrement;
		return Rotation.From(angles);
	}

	private Vector3 SnapToGrid(Vector3 position, float gridSize)
	{
		return new Vector3(
			MathF.Round(position.x / gridSize) * gridSize,
			MathF.Round(position.y / gridSize) * gridSize,
			MathF.Round(position.z / gridSize) * gridSize
		);
	}

	private bool CanPlace()
	{
		foreach (var other in Scene.GetAll<Interactable>())
		{
			if (other != heldInteractable &&
				other.rigidbody.PhysicsBody.CheckOverlap(heldInteractable.Components.Get<Rigidbody>().PhysicsBody))
			{
				return false;
			}
		}
		return true;
	}
}