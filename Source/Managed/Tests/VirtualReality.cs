namespace UnrealEngine.Tests;
public class VirtualReality : ISystem
{
    private readonly Pawn pawnVR;
    private readonly MotionControllerComponent leftHand;
    private readonly MotionControllerComponent rightHand;

    public VirtualReality()
    {
        pawnVR = new("PawnVR");
        leftHand = new(pawnVR, "LeftHand");
        rightHand = new(pawnVR, "RightHand");
    }

    public void OnBeginPlay()
    {
        Camera mainCamera = World.GetActor<Camera>("MainCamera")!;

        mainCamera.GetComponent<CameraComponent>()!.LockToHeadMountedDisplay = true;

        PlayerController playerController = World.GetFirstPlayerController()!;

        playerController.SetViewTarget(mainCamera);
        playerController.Possess(pawnVR);

        Pawn playerPawn = playerController.GetPawn()!;

        Assert.IsTrue(playerPawn.IsControlled);
        Assert.IsTrue(playerPawn.IsPlayerControlled);

        leftHand.DisplayDeviceModel = true;
        leftHand.SetTrackingMotionSource("Left");

        rightHand.DisplayDeviceModel = true;
        rightHand.SetTrackingMotionSource("Right");
    }

    public void OnTick(float deltaTime)
    {
        Debug.AddOnScreenMessage(4, 3.0f, Color.Orange, "Left motion controller is tracked: " + leftHand.IsTracked);
        Debug.AddOnScreenMessage(5, 3.0f, Color.Orange, "Right motion controller is tracked: " + rightHand.IsTracked);
    }

    public void OnEndPlay()
    {
        Debug.ClearOnScreenMessages();
    }
}