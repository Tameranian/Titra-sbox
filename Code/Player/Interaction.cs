using System;
using System.Threading.Tasks;
using Sandbox;

public sealed class Interaction : Component
{
	PlayerController playerController;
	public Interactable heldInteractable;
	[Property, Range(.1f, 1)] public float MovementSmoothness { get; set; } = 1.0f;
	private Rotation targetHeldObjectRotation;
	private float scrollDistance = 1.0f;
	private Vector3 targetHeldObjectPosition;
	public bool snapEnabled = false;
	public bool placingBlocked = false;
	private bool rotationAdjusted;
	public bool IsPlacementAllowed = false;
	public Item heldItem;
	public bool grabbingItem = false;
	

	protected override void OnStart()
	{
		playerController = Components.Get<PlayerController>();
	}

	protected override void OnUpdate()
	{
		// if (heldInteractable != null)
		// {
		// 	HandleHeldItem(heldInteractable);
		// }
		// Log.Info(heldInteractable);
		// if (InputActions.TryLeftClick)
		// {
		// 	if (playerController.Hovered != null && playerController.Hovered.Components.Get<Interactable>() != null)
		// 	{
		// 		playerController.Hovered.Components.Get<Interactable>().TakeDamage(20);
		// 	}
		// }
		// if(heldItem != null && InputActions.TryWalk)
		// {
		// 	// ReleaseHeldItem(heldItem, false, 0);
		// 	heldItem.Grab(GameObject);
		// }
	}

	private void HandleHeldItem(Interactable interactable)
	{
		if (interactable != null && interactable.Components.Get<Item>() == null)
		{   
			bool canPlace = CanPlace();
			var color = snapEnabled ? (canPlace ? Color.Green : Color.Red) : Color.White;
			var alpha = snapEnabled ? 0.5f : 1.0f;
			if(!interactable.isBeingPlaced)
				UpdateHeldItemTint(interactable, color, alpha);

			if (InputActions.TryUse)
			{
				if (snapEnabled && canPlace)
				{
					ReleaseHeldItem(interactable, true, 0); // Place
				}
				else if(!snapEnabled)
				{
					ReleaseHeldItem(interactable, false, 0); // Drop
				}
			}
			else 
				HoldItem(interactable);
			
			if(InputActions.TryDrop )
			{
				ReleaseHeldItem(interactable, false, 0); // Drop
			}
			
			if(InputActions.TryReload && snapEnabled && interactable.gridPosition != Vector3Int.Zero)
			{
				CancelPlacement(interactable); // Cancel
			}
			if(InputActions.TryLeftClick && !snapEnabled)
			{
				ReleaseHeldItem(interactable, false, 1000);
			}
		}
		else if (interactable != null && interactable.Components.Get<Item>() != null)
		{
			if (InputActions.TryUse)
			{
				ReleaseHeldItem(interactable, false, 0); // Drop

			}
			else 
				HoldItem(interactable);
			
		}
	}
	

	private void UpdateHeldItemTint(Interactable interactable, Color color, float alpha)
	{
		interactable.modelRenderer.Tint = new Color(color.r, color.g, color.b, alpha);
	}

	public void HoldItem(Interactable interactable)
	{		
		// Log.Info("Holding Item");
		// interactable.Tags.Add("held");
		if(interactable.Components.Get<Item>() != null)
		{
			heldItem = interactable.Components.Get<Item>();
		}
		
		float scrollDelta = Input.MouseWheel.y;
		scrollDistance = MathX.Clamp(scrollDistance + scrollDelta * 0.1f, .45f, 1.25f);
		var finalReachLength =  playerController.EyeTransform.Position + playerController.EyeTransform.Forward *
			(playerController.ReachLength - 30) * scrollDistance;
		var holdTrace = Scene.Trace.Ray(playerController.EyeTransform.Position + playerController.EyeTransform.Forward,
			finalReachLength)
			.WithoutTags("trigger", "player", "placed")
			.IgnoreGameObject(interactable.GameObject)
			.Run();

		// if (InputActions.TryWalk && interactable.Components.Get<Item>() == null)
		// {
		// 	snapEnabled = !snapEnabled;
		// 	rotationAdjusted = false;
		// }

		var traceTargetPosition = holdTrace.Hit ? holdTrace.HitPosition + holdTrace.Normal : finalReachLength;

		var cameraRotation = Scene.Camera.WorldRotation;
		// if (snapEnabled)
		// {	

		// 	UpdateHeldObjectRotation();
		// 	if(!interactable.isBeingPlaced)
		// 	targetHeldObjectPosition = SnapToGrid(traceTargetPosition, GridManager.Instance.cellSize);

		// 	if(!rotationAdjusted)
		// 	{
		// 		targetHeldObjectRotation = SnapToRotation(Rotation.FromAxis(Vector3.Up, cameraRotation.Angles().yaw), 45);
		// 	}
		// 	interactable.ToggleCollider(false);
		// }
		// else
		// {
			targetHeldObjectPosition = traceTargetPosition;
			interactable.ToggleCollider(true);
			targetHeldObjectRotation = Rotation.FromAxis(Vector3.Up, cameraRotation.Angles().yaw);
		// }
		
		interactable.rigidbody.PhysicsBody.SmoothMove(targetHeldObjectPosition, snapEnabled ? MovementSmoothness * .5f : MovementSmoothness, Time.Delta);
		interactable.rigidbody.PhysicsBody.SmoothRotate(targetHeldObjectRotation, snapEnabled ? MovementSmoothness * .5f : MovementSmoothness, Time.Delta);

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

	public async void ReleaseHeldItem(Interactable interactable, bool placing, float throwForce)
	{
		interactable.timeSinceLastDrop = 0;
		interactable.isBeingPlaced = placing;
		if (placing)
		{
			interactable.PlaceAction();
			interactable.Tags.Add("placed");
			// Log.Info("Placing Interactable");
			Sound.Play( interactable.placeSound, WorldPosition );
			interactable.isPlaced = true;
			interactable.gridPosition = GridManager.Instance.GetGridPosition(
				new Vector3Int((int)targetHeldObjectPosition.x, (int)targetHeldObjectPosition.y, (int)targetHeldObjectPosition.z));
			interactable.gridRotation = targetHeldObjectRotation;
			if(interactable is Cog)
				((Cog)interactable).CheckForAdjacentCogs();
			interactable.ToggleRigidbodyLock(true);
					await Task.WhenAll(interactable.ScaleUpAndDownEffect(GridManager.Instance.scaleEffectCurve, .65f, 1.1f),
			UpdateTintAsync(interactable, interactable.modelRenderer.Tint, Color.White, interactable.modelRenderer.Tint.a, 1.0f, 100));			
		}
		else
		{
			// Log.Info("Dropping Interactable");
			interactable.ToggleRigidbodyLock(false);
			interactable.rigidbody.Gravity = true;
			interactable.gridPosition = Vector3Int.Zero;
			// await UpdateTintAsync(interactable, interactable.modelRenderer.Tint, Color.White, interactable.modelRenderer.Tint.a, 1.0f, 100);
		}		


		interactable.Tags.Remove("held");
		IsPlacementAllowed = false;
		interactable.ToggleCollider(true);
		interactable.isHeld = false;
		scrollDistance = 1.0f;
		interactable.isBeingPlaced = false;
		if(throwForce > 0)
		{
			Log.Info("Applying Impulse");
			interactable.rigidbody.ApplyImpulse( interactable.WorldRotation.Forward * throwForce);
		}
		heldInteractable = null;
	}

	private async void CancelPlacement(Interactable interactable)
	{
		interactable.timeSinceLastDrop = 0;
		interactable.isBeingPlaced = true;
		Sound.Play( interactable.placeSound, WorldPosition );
		interactable.isPlaced = true;
		interactable.WorldPosition = interactable.gridPosition * GridManager.Instance.cellSize;
		interactable.WorldRotation = interactable.gridRotation;
		if(interactable is Cog)
			((Cog)interactable).CheckForAdjacentCogs();
		interactable.ToggleRigidbodyLock(true);
				await Task.WhenAll(interactable.ScaleUpAndDownEffect(GridManager.Instance.scaleEffectCurve, .65f, 1.1f),
		UpdateTintAsync(interactable, interactable.modelRenderer.Tint, Color.White, interactable.modelRenderer.Tint.a, 1.0f, 100));			

		IsPlacementAllowed = false;
		interactable.ToggleCollider(true);
		interactable.isHeld = false;
		scrollDistance = 1.0f;
		interactable.isBeingPlaced = false;
		interactable.Tags.Add("placed");
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
		// Special handling for Cogs
		if (heldInteractable is Cog)
		{
			bool foundValidHolder = false;
			
			// Check if there's a CogHolder at the target position
			foreach (var other in Scene.GetAll<CogHolder>())
			{
				if (other.gridPosition == GridManager.Instance.GetGridPosition(targetHeldObjectPosition))
				{
					foundValidHolder = true;
					break;
				}
			}
			
			// If we're trying to place a cog, it must be on a holder
			if (!foundValidHolder)
			{
				return false;
			}
		}

		// Check for overlapping with other interactables
		foreach (var other in Scene.GetAll<Interactable>())
		{
			if (other != heldInteractable && other.Components.Get<Rigidbody>() != null)
			{
				// Skip the overlap check if this is a Cog being placed on a CogHolder
				if (heldInteractable is Cog && other is CogHolder && 
					other.gridPosition == GridManager.Instance.GetGridPosition(targetHeldObjectPosition))
				{
					continue;
				}
				
				if (other.rigidbody.PhysicsBody.CheckOverlap(heldInteractable.Components.Get<Rigidbody>().PhysicsBody))
				{
					return false;
				}
			}
		}
		
		return true;
	}
	
	// private bool CanPlace()
	// {
	// 	foreach (var other in Scene.GetAll<Interactable>())
	// 	{
	// 		if (other != heldInteractable)
	// 		{
	// 			// Allow Cog to overlap with CogHolder
	// 			if (heldInteractable is Cog && other is CogHolder)
	// 			{
	// 				continue;
	// 			}

	// 			if (other.rigidbody.PhysicsBody.CheckOverlap(heldInteractable.Components.Get<Rigidbody>().PhysicsBody))
	// 			{
	// 				return false;
	// 			}
	// 		}
	// 	}
	// 	return true;
	// }
}