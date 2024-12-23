public static class InputActions
{
	public const string LeftClick = "attack1";
	public const string RightClick = "attack2";
	public const string Reload = "reload";
	public const string Flashlight = "Flashlight";
	public const string Jump = "Jump";
	public const string Duck = "Duck";
	public const string Walk = "Walk";
	public const string Drop = "Drop";
    public const string Run = "Run";
    public const string Use = "Use";
    public const string View = "View";
    public static Vector2 scrollVector = Input.MouseWheel;
    public static float scrollDeltaY = Input.MouseWheel.y;


    public static bool IsJumping => Input.Down(Jump);
    public static bool TryJump => Input.Pressed(Jump);
    public static bool IsDucking => Input.Down(Duck);
    public static bool IsRunning => Input.Down(Run);
    public static bool IsWalking => Input.Down(Walk);
    public static bool TryWalk => Input.Pressed(Walk);
    public static bool TryDrop => Input.Pressed(Drop);
    public static bool TryReload => Input.Pressed(Reload);
    public static bool TryLeftClick => Input.Pressed(LeftClick);
    public static bool TryRightClick=> Input.Pressed(RightClick);
    public static bool IsReloading=> Input.Pressed(Reload);
    public static bool IsUsingFlashlight => Input.Pressed(Flashlight);
    public static bool TryUse => Input.Pressed(Use);
    public static bool IsUsing => Input.Down(Use);
    public static bool TryRagdoll => Input.Pressed(View);
    
}