using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Sandbox;

public partial class Player : Component
{
   
    [Property] 
    [Category("Hunger")]
    public float maxHunger = 100f;
    [Property] 
    [Category("Hunger")]
    public float timeBeforeHungerRegenStarts = 3f;
    [Property] 
    [Category("Hunger")]
    public float hungerValueIncrement = 1;
    [Property] 
    [Category("Hunger")]
    public float hungerTimeIncrement = 0.1f;
    [Property] 
    [Category("Hunger")]
    public Vector2 passiveHungerLossRange = new Vector2(0.3f, 0.8f);
    [Property] 
    [Category("Hunger")]
    private Vector2 hungerDecreaseIntervalRange = new Vector2(1f, 3f);
    [Property] 
    [Category("Hunger")]
    public float currentHunger;
    private float nextHungerDecreaseTime;
    public static Action<float> OnEat;
    public bool isTemporaryHungerLossRange = false;
    public Vector2 originalHungerLossRange;
    // private float timeSinceLastHungerDecrease;

    [Property] 
    [Category("Energy")]
    public float maxEnergy = 100f;
    [Property] 
    [Category("Energy")]
    public float timeBeforeEnergyRegenStarts = 3f;
    [Property] 
    [Category("Energy")]
    public float energyValueSleepIncrement = 1f;
    [Property] 
    [Category("Energy")]
    public float energyTimeSleepIncrement = 0.1f;
    [Property] 
    [Category("Energy")]
    public Vector2 passiveEnergyLossRange = new Vector2(0.3f, 0.8f);
    [Property] 
    [Category("Energy")]
    private Vector2 energyDecreaseIntervalRange = new Vector2(1f, 3f);
    [Property] 
    [Category("Energy")]
    public float currentEnergy;
    private float nextEnergyDecreaseTime;
    public bool isTemporaryEnergyLossRange = false;
    public Vector2 originalEnergyLossRange;
    // private float timeSinceLastEnergyDecrease;
    public static Action<float> OnEnergyConsumed;
    [Property] 
    [Category("Energy")]
    private float sprintEnergyConsumptionRate = 0.1f;
    // [Property] 
    // [Category("Energy")]
    // private float jumpEnergyConsumption = 1f;


	public string energyText;
	public string hungerText;
	public string healthText;
    
    [Property] 
    [Category("Health")]
    public float maxHealth = 100f;
    [Property] 
    [Category("Health")]
    public float currentHealth;
    [Property] 
    [Category("Health")]
    public float healthValueIncrement = 1f;
    [Property] 
    [Category("Health")]
    public float timeBeforeRegenStarts = 5f;


    private Task regeneratingHealthTask;
    private CancellationTokenSource  regenerationCancellationTokenSource;
    public CancellationTokenSource energyRegenerationCancellationTokenSource;


    private void InitializeStats()
    {

        currentHunger = maxHunger;
        currentEnergy = maxEnergy;
        currentHealth = maxHealth;

        originalHungerLossRange = passiveHungerLossRange;
        OnEat += UpdateHungerUI;
        nextHungerDecreaseTime = Time.Now + timeBeforeHungerRegenStarts;

        originalEnergyLossRange = passiveEnergyLossRange;
        nextEnergyDecreaseTime = Time.Now + timeBeforeEnergyRegenStarts;

        UpdateHungerUI(currentHunger);
        UpdateEnergyUI(currentEnergy);
        UpdateHealthUI(currentHealth);
    }

    private void UpdateHungerUI(float hunger)
    {
        float normalizedHunger = hunger / maxHunger;
        float percentage = normalizedHunger * 100f;
        hungerText = percentage > 100f ? "F: 100+" : $"F: {(percentage == 100f ? percentage.ToString("F0") : percentage.ToString("F1"))}";
    }

    private void UpdateEnergyUI(float energy)
    {
        float normalizedEnergy = energy / maxEnergy;
        float percentage = normalizedEnergy * 100f;
        energyText = percentage > 100f ? "E: 100+" : $"E: {(percentage == 100f ? percentage.ToString("F0") : percentage.ToString("F1"))}";
    }

    private void UpdateHealthUI(float health)
    {
        float normalizedHealth = health / maxHealth;
        float percentage = normalizedHealth * 100f;
        healthText = percentage > 100f ? "H: 100+" : $"H: {(percentage == 100f ? percentage.ToString("F0") : percentage.ToString("F1"))}";
    }

    private void UpdateUI()
    {;
        UpdateHungerUI(currentHunger);
        UpdateEnergyUI(currentEnergy);
        if(!isSleeping)
        {
            DecreaseHungerOverTime();
            DecreaseEnergyOverTime();
        }

        if (InputActions.IsRunning && Enabled && currentEnergy > 0f)
        {
            //Debug.Log("Draining energy  from sprinting");
            currentEnergy -= sprintEnergyConsumptionRate * Time.Delta;
            OnEnergyConsumed?.Invoke(currentEnergy);
        }

        //if (player.inputHandler.IsJumping() && player.PlayerController.isGrounded);
        //{
        //    //Debug.Log("Draining energy from jumping");
        //    currentEnergy -= jumpEnergyConsumption * Time.Delta;
        //    OnEnergyConsumed?.Invoke(currentEnergy);
        //}
    }

    private void DecreaseHungerOverTime()
    {
        if (Time.Now > nextHungerDecreaseTime)
        {
            float hungerDecreaseAmount = Game.Random.Float(passiveHungerLossRange.x, passiveHungerLossRange.y);
            currentHunger -= hungerDecreaseAmount;
            currentHunger = currentHunger.Clamp(0f, maxHunger * 2f);
            nextHungerDecreaseTime = Time.Now + Game.Random.Float(hungerDecreaseIntervalRange.x, hungerDecreaseIntervalRange.y);
        }
    }

    private void DecreaseEnergyOverTime()
    {
        if (Time.Now > nextEnergyDecreaseTime)
        {
            float energyDecreaseAmount = Game.Random.Float(passiveEnergyLossRange.x, passiveEnergyLossRange.y);
            currentEnergy -= energyDecreaseAmount;
            currentEnergy = currentEnergy.Clamp(0f, maxEnergy * 2f);
            UpdateEnergyUI(currentEnergy);
            nextEnergyDecreaseTime = Time.Now + Game.Random.Float(energyDecreaseIntervalRange.x, energyDecreaseIntervalRange.y);
        }
    }

    public async Task ResetHungerLossRangeAfterDelay()
    {
        // Wait for a certain duration
        await Task.Delay(60000); 

        // Revert to the original range
        isTemporaryHungerLossRange = false;
        passiveHungerLossRange = originalHungerLossRange;
    }


    public async Task RegenerateEnergy(CancellationToken cancellationToken)
    {
        await Task.Delay((int)(timeBeforeEnergyRegenStarts * 1000), cancellationToken);

        while (currentEnergy < maxEnergy)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            currentEnergy += energyValueSleepIncrement;
            currentEnergy = currentEnergy.Clamp(0f, maxEnergy);
            UpdateEnergyUI(currentEnergy);

            await Task.Delay((int)(energyTimeSleepIncrement * 1000), cancellationToken);
        }
    }

    public async Task ResetEnergyLossRangeAfterDelay()
    {
        // Wait for a certain duration
        await Task.Delay(60000); // 60 seconds in milliseconds

        // Revert to the original range
        isTemporaryEnergyLossRange = false;
        passiveEnergyLossRange = originalEnergyLossRange;
    }

        public void ApplyDamage(float dmg)
    {
        currentHealth -= dmg;
        if (currentHealth <= 0)
        {
            KillPlayer();
        }
        else
        {
            if (regeneratingHealthTask != null && !regeneratingHealthTask.IsCompleted)
            {
                regenerationCancellationTokenSource.Cancel();
            }
            regenerationCancellationTokenSource = new CancellationTokenSource();
            regeneratingHealthTask = RegenerateHealth(regenerationCancellationTokenSource.Token);
        }
    }

        private void KillPlayer()
        {
            currentHealth = 0;
            // Handle player death
        }
    private async Task RegenerateHealth(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay((int)timeBeforeRegenStarts * 1000, cancellationToken);
            while (currentHealth < maxHealth)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                currentHealth += healthValueIncrement;
                if (currentHealth > maxHealth)
                {
                    currentHealth = maxHealth;
                }

                await Task.Delay(1000, cancellationToken);
            }
        }
        catch (TaskCanceledException)
        {
            // Handle the task cancellation if needed
        }
        finally
        {
            regeneratingHealthTask = null;
        }
    }
}
