using Sandbox;
using System.Threading;
using System.Threading.Tasks;

public class Bed : Interactable
{
	Player player;
	[Property]
	public GameObject sleepCamTransform;
	private bool canStopSleeping = false;
	private Rotation cachedCameraRotation;

	protected override void OnStart()
	{
		interactableResource = ResourceLibrary.Get<InteractableResource>("game resources/bed.interact");
		base.OnStart();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if(player != null)
		{
			if(canStopSleeping && (player.isSleeping && player.currentEnergy == 100 || InputActions.TryUse))
			{
				StopSleeping();
			}
		}
	}

	public void Sleep()
	{
		player.isSleeping = true;
		cachedCameraRotation = player.Components.Get<PlayerController>().EyeAngles;
		player.playerController.UseCameraControls = false;
		player.playerController.Enabled = false;
		Scene.Camera.WorldPosition = sleepCamTransform.WorldPosition;
		Scene.Camera.WorldRotation = sleepCamTransform.WorldRotation;
		Scene.TimeScale = 2f;

		player.energyRegenerationCancellationTokenSource = new CancellationTokenSource();
		_ = StartStopSleepingTimer();
		_ = player.RegenerateEnergy(player.energyRegenerationCancellationTokenSource.Token);
	}

	private async Task StartStopSleepingTimer()
	{
		await Task.Delay(250);
		canStopSleeping = true;
	}

	public void StopSleeping()
	{
		player.isSleeping = false;
		player.playerController.Enabled = true;
		player.Components.Get<PlayerController>().EyeAngles = cachedCameraRotation;
		player.playerController.UseCameraControls = true;
		canStopSleeping = false;
		Scene.TimeScale = 1f;

		player.energyRegenerationCancellationTokenSource.Cancel();
	}

    public override bool Press(IPressable.Event e)
    {
		player = e.Source.Components.Get<Player>();
		if (player.currentEnergy < 100)
		{
			Sleep();
			Release(e);
		}
		return true;
    }

	public override void InteractWith( GameObject other, Vector3Int relativePosition )
	{
		throw new System.NotImplementedException();
	}

	public override void PlaceAction()
	{
		throw new System.NotImplementedException();
	}
}
