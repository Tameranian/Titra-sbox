using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Sandbox;

public abstract class Interactable : Component, Component.IPressable, Component.ITriggerListener, Component.IDamageable
{
	[Property] public InteractableResource interactableResource = default;
	public ModelRenderer modelRenderer;
	public Rigidbody rigidbody;
	public Collider collider;
	public bool isHeld = false;
	public bool isPlaced = false;
	public bool isBeingPlaced = false;
	protected Interaction playerInteracting;
	[Property] public SoundEvent placeSound = default;
	[Property] public SoundEvent hurtSound = default;
	[Property] public Vector3Int gridPosition;
	public Rotation gridRotation;
 	public TimeSince timeSinceLastDrop; 
	public Curve scaleEffectCurve;
	public Vector3 savedScale;
	public bool canBePickedUp;
	public bool canTakeDamage;
	public bool canDie;
	public int currentHealth;
    

	protected override void OnStart()
	{
		if(interactableResource != null )
		{
			if(placeSound == null && interactableResource.PlaceSound != null)
			{
				placeSound = interactableResource.PlaceSound;
			}
			else
			{
				placeSound = ResourceLibrary.Get<SoundEvent>("sounds/impacts/melee/impact-melee-concrete.sound");
			}

			if(hurtSound == null && interactableResource.HurtSound != null)
			{
				hurtSound = interactableResource.HurtSound;
			}
			else
			{
				hurtSound = ResourceLibrary.Get<SoundEvent>("sounds/impacts/bullets/impact-bullet-generic.sound");
			}
			canBePickedUp = interactableResource.CanPickup;
			canTakeDamage = interactableResource.CanHurt;
			currentHealth = interactableResource.BaseHealth;
			canDie = interactableResource.CanDie;
		}
		else
		{
			Log.Warning("Interactable resource not set for " + GameObject.Name);
		}
		
		savedScale = GameObject.LocalScale;

		Log.Info(placeSound);
		modelRenderer = Components.Get<ModelRenderer>();
		collider = Components.Get<Collider>();
		rigidbody = Components.Get<Rigidbody>();


	}

	protected override void OnUpdate()
	{
	}

	public void OnTriggerEnter(Collider other)
	{
		if (other.GameObject.Components.Get<Interactable>() != null && isHeld && playerInteracting?.snapEnabled == true)
		{
			// Log.Info("Object entered trigger: " + other.GameObject.Name);
			playerInteracting.placingBlocked = true;
		}
	}

	public void OnTriggerExit(Collider other)
	{
		if (other.GameObject.Components.Get<Interactable>() != null && isHeld && playerInteracting?.snapEnabled == true)
		{
			// Log.Info("Object exited trigger: " + other.GameObject.Name);
			playerInteracting.placingBlocked = IsAnyInteractableInside();
		}
	}

	private bool IsAnyInteractableInside()
	{
		foreach (var gameobject in collider.Touching)
		{
			if (gameobject.Components.Get<Interactable>() != null)
			{
				return true;
			}
		}
		return false;
	}


	public void ToggleCollider(bool hasCollision)
	{
		GameObject.Components.Get<Collider>().IsTrigger = !hasCollision;
	}

	public void ToggleRigidbodyLock(bool locked)
	{
		if (Components.TryGet<Rigidbody>(out var rb))
		{
			var locking = rb.Locking;
			locking.X = locked;
			locking.Y = locked;
			locking.Z = locked;
			locking.Pitch = locked;
			locking.Yaw = locked;
			locking.Roll = locked;
			rb.Locking = locking;
		}
	}

  	public abstract void InteractWith(GameObject other, Vector3Int relativePosition);


  	public virtual void HoldAction(Interaction interaction)
	{
		isHeld = true;
        playerInteracting.heldInteractable = this;
        rigidbody.Gravity = false;
        ToggleRigidbodyLock(false);
        Tags.Remove("placed");
	}

  	public abstract void PlaceAction();

	
    public GameObject GetObjectsAWorldPosition(Vector3 worldPosition, GameObject gameObject)
    {
        var objects = Scene.FindInPhysics(new Sphere(worldPosition, GridManager.Instance.cellSize / 2)).Where(x => x.Tags.Contains("placed"));
        foreach (GameObject interactable in objects)
        {
            if (interactable.WorldPosition == worldPosition && interactable != gameObject)
            {
                Log.Info("Found object at grid position: " + worldPosition);
                return interactable;
            }
        }
        return null;
    }

    public virtual bool Press(IPressable.Event e)
    {
		
		return true;
    }

	public bool Pickup(IPressable.Event e)
	{
		if (timeSinceLastDrop < 0.05f)  
		{
			return false;
		}

		playerInteracting = e.Source.Components.Get<Interaction>();
		if (playerInteracting.IsPlacementAllowed)
		{
			Log.Info("Placement Disabled");
			return false;
		}
		else
		{
			HoldAction(playerInteracting);
			Tags.Add("held");
			return true;
		}
	}
	
	public void Release(IPressable.Event e)
	{
		// Log.Info("Releasing Interactable");
		e.Source.Components.Get<Interaction>().IsPlacementAllowed = true;
		// Logic for when the press finishes
	}
	public void Hover(IPressable.Event e)
	{
		// Logic for when the player starts looking at this object
	}

	public void Look(IPressable.Event e)
	{
		// Logic for when the player is still looking at this object
	}

	public void Blur(IPressable.Event e)
	{
		// Logic for when the player stops looking at this object
	}

	public bool Pressing(IPressable.Event e)
	{
		if(canBePickedUp)
		{	
			if(GameObject.Components.Get<Item>() != null)
			{
				if(InputActions.TryWalk && !e.Source.Components.Get<Interaction>().grabbingItem)
				{
					Log.Info("Releasing Item");
					e.Source.Components.Get<Interaction>().grabbingItem = true;
					e.Source.Components.Get<Interaction>().ReleaseHeldItem(e.Source.Components.Get<Interaction>().heldItem, false, 0);
					e.Source.Components.Get<Interaction>().heldItem.Grab(e.Source.GameObject);
					
				}
				if(GameObject.Components.Get<Item>()?.isGrabbed == false)
				{
					e.Source.Components.Get<Interaction>().HoldItem(this);
					Log.Info("Holding Item");
				}
				// if(InputActions.TryLeftClick)
				// {
				// 	e.Source.Components.Get<Interaction>().ReleaseHeldItem(this, false, 0);
				// 	rigidbody.Velocity = playerInteracting.GameObject.Components.Get<PlayerController>().EyeTransform.Forward * 1000f;
				// }

			}
			else 
			{
				e.Source.Components.Get<Interaction>().HoldItem(this);
			}
		}
		return true;
	}


	public bool CanPress(IPressable.Event e)
	{
		// Logic to determine if the object can be pressed
		return true;
	}

	public virtual async void TakeDamage( int damage )
	{
		if(canTakeDamage && isPlaced || !canBePickedUp)
		{
			if(hurtSound != null)
			{
				Sound.Play(hurtSound, WorldPosition).Volume = 3;
			}

			currentHealth -= damage;
			currentHealth = (int)MathF.Max(currentHealth, 0f);
			if(currentHealth <= 0)
			{
				Kill();
			}
			await ScaleUpAndDownEffect(GridManager.Instance.scaleEffectCurve, .95f, 1.05f);


			Log.Info("Current Health is: " + currentHealth);
		}
		
	}

	public async Task ScaleUpAndDownEffect(Curve curve, float scaleShrinkModifier, float scaleExpandModifier )
	{
		var baseScale = savedScale;
		var shrunkScale = baseScale * scaleShrinkModifier; 
		var expandedScale = baseScale * scaleExpandModifier; 
		var duration = 200;

		await TweenScaleAsync(baseScale, shrunkScale, duration / 6, curve);

		await TweenScaleAsync(shrunkScale, expandedScale, duration / 2, curve);
		
		await TweenScaleAsync(expandedScale, baseScale, duration / 2, curve);
	}

	
	private async Task TweenScaleAsync(Vector3 initialScale, Vector3 targetScale, int duration, Curve scaleEffectCurve)
	{
		var elapsedTime = 0f;

		while (elapsedTime < duration)
		{
			float t = elapsedTime / duration;
			float curveValue = scaleEffectCurve.Evaluate(t);
			LocalScale = Vector3.Lerp(initialScale, targetScale, curveValue);
			elapsedTime += Time.Delta * 1000; 
			await Task.Delay(1);
		}
		LocalScale = targetScale;
	}

	public virtual void Kill()
	{
		if(canDie)
		{
			GameObject.Destroy();
		}
	}

	public void OnDamage( in DamageInfo damage )
	{
		throw new NotImplementedException();
	}
}