using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sandbox;

public class Collectable : Component, Component.ITriggerListener
{
    private BoxCollider boxCollider;
    private SphereCollider sphereCollider;
    private ModelRenderer modelRenderer;
    private Rigidbody rigidbody;
    private GameObject visualObject;

    public bool beingCollected;
	private bool collected;
    private GameObject player;
	private bool playerTargeted;

	protected override void OnStart()
    {
        visualObject = GameObject.Children[0];
        modelRenderer = visualObject.Components.Get<ModelRenderer>();
        rigidbody = visualObject.Components.Get<Rigidbody>();
        boxCollider = Components.Get<BoxCollider>();
        sphereCollider = Components.Get<SphereCollider>();
        if(!Tags.Contains("collectable"))
        {
            Tags.Add("collectable");
        }
    }

    protected override void OnUpdate()
    {
        if (beingCollected)
        {
            Collect();
        }
        else
        {
            FloatIdle(visualObject);
        }

        if(playerTargeted)
        {
            var distance = Vector3.DistanceBetween(visualObject.WorldPosition, player.WorldPosition);
            LookAtPlayer(player);
            if (distance > 100f)
            {
                playerTargeted = true;
            }
            else if (distance < 120f)
            {
                playerTargeted = false;
                boxCollider.Enabled = false;
                beingCollected = true;
            }
        }

    }

    public void OnTriggerEnter(Collider other)
	{
		if (other.GameObject.Tags.Contains("player") && !collected)
		{
            playerTargeted = true;
            player = other.GameObject;

		}
	}

	public void OnTriggerExit(Collider other)
	{
		if (other.GameObject.Components.Get<PlayerController>() != null)
		{
            playerTargeted = false;
            player =  null;
        }
	}

    private void Collect()
    {
        LookAtPlayer(player);
        GoToPlayer(player);
        ShrinkToPlayer(player);

    }

    private void GoToPlayer(GameObject player)
    {
        WorldPosition = Vector3.Lerp(WorldPosition, player.WorldPosition + new Vector3(0, 0, 48), Time.Delta * 5);
    }

    private void LookAtPlayer(GameObject player)
    {
        var direction = (player.WorldPosition - visualObject.WorldPosition).Normal;
        Rotation toRotation = Rotation.FromToRotation(visualObject.WorldRotation.Forward, direction);
        visualObject.WorldRotation = Rotation.Lerp(visualObject.WorldRotation, toRotation, Time.Delta);
    }

    private void ShrinkToPlayer(GameObject player)
    {
        var distance = Vector3.DistanceBetween(visualObject.WorldPosition, player.WorldPosition);
        visualObject.LocalScale = Vector3.Lerp(visualObject.LocalScale, Vector3.Zero,Time.Delta * 10);


        if (distance < 10f || visualObject.LocalScale.Length < 0.05f)
        {
            Sound.Play("sounds/kenney/ui/ui.button.press.sound", WorldPosition);
            collected = true;
            GameObject.Destroy();
        }
    }

	private void FloatIdle(GameObject visualObject)
    {
        visualObject.LocalPosition = new Vector3(visualObject.LocalPosition.x, visualObject.LocalPosition.y, visualObject.LocalPosition.z + MathF.Sin(Time.Now * 2) * 0.05f);
	}

    
	public async Task ScaleUpAndDownEffect(Curve curve, float scaleShrinkModifier, float scaleExpandModifier )
	{
		var baseScale = 1f;
		var shrunkScale = baseScale * scaleShrinkModifier; 
		var expandedScale = baseScale * scaleExpandModifier; 
		var duration = 200;

		await TweenScaleAsync(baseScale, shrunkScale, duration / 6, curve);

		await TweenScaleAsync(shrunkScale, expandedScale, duration / 2, curve);
		
		await TweenScaleAsync(expandedScale, 0, duration / 2, curve);
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

}
