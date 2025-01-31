namespace UnrealEngine.Tests;
public class RuntimeConsistency : ISystem
{
    public void OnBeginPlay()
    {
        ActorMemoryManagementTest();
        ComponentMemoryManagementTest();
        ActorBlueprintClassesMatchingTest();
        ComponentBlueprintClassesMatchingTest();
        DuplicateActorMemoryManagementTest();
        DuplicateComponentMemoryManagementTest();
        ConsoleVariablesMemoryManagementTest();

        Debug.AddOnScreenMessage(-1, 10.0f, Color.MediumTurquoise, "Verify " + MethodBase.GetCurrentMethod()!.DeclaringType + " results in output log!");
    }

    private static void ActorMemoryManagementTest()
    {
        Debug.Log(LogLevel.Display, "Starting " + MethodBase.GetCurrentMethod()!.Name + "...");

        try
        {
            Actor actor = new();
            SceneComponent sceneComponent = new(actor, setAsRoot: true);

            _ = actor.Destroy();

            Debug.Log(LogLevel.Display, "Triggering invalid action after the destruction");

            sceneComponent.Destroy();
        }

        catch (Exception exception)
        {
            Debug.Log(LogLevel.Display, "The exception has successfully reached: " + exception.Message);

            return;
        }

        Debug.Log(LogLevel.Error, MethodBase.GetCurrentMethod()!.Name + " test failed!");
    }

    private static void ComponentMemoryManagementTest()
    {
        Debug.Log(LogLevel.Display, "Starting " + MethodBase.GetCurrentMethod()!.Name + "...");

        try
        {
            Actor actor = new();
            SceneComponent sceneComponent = new(actor, setAsRoot: true);

            sceneComponent.Destroy();

            Debug.Log(LogLevel.Display, "Triggering invalid action after the destruction");

            sceneComponent.AddLocalOffset(default);
        }

        catch (Exception exception)
        {
            Debug.Log(LogLevel.Display, "The exception has successfully reached: " + exception.Message);

            return;
        }

        Debug.Log(LogLevel.Error, MethodBase.GetCurrentMethod()!.Name + " test failed!");
    }

    private static void ActorBlueprintClassesMatchingTest()
    {
        Debug.Log(LogLevel.Display, "Starting " + MethodBase.GetCurrentMethod()!.Name + "...");

        try
        {
            Blueprint blueprintActor = Blueprint.Load("/Game/Tests/BlueprintCameraActor")!;

            Debug.Log(LogLevel.Display, "Triggering invalid action after the blueprint loading");

            Actor actor = new(blueprint: blueprintActor);
        }

        catch (Exception exception)
        {
            Debug.Log(LogLevel.Display, "The exception has successfully reached: " + exception.Message);

            return;
        }

        Debug.Log(LogLevel.Error, MethodBase.GetCurrentMethod()!.Name + " test failed!");
    }

    private static void ComponentBlueprintClassesMatchingTest()
    {
        Debug.Log(LogLevel.Display, "Starting " + MethodBase.GetCurrentMethod()!.Name + "...");

        try
        {
            Blueprint blueprintComponent = Blueprint.Load("/Game/Tests/BlueprintActor")!;
            Actor actor = new();

            Debug.Log(LogLevel.Display, "Triggering invalid action after the blueprint loading");

            SceneComponent sceneComponent = new(actor, blueprint: blueprintComponent);
        }

        catch (Exception exception)
        {
            Debug.Log(LogLevel.Display, "The exception has successfully reached: " + exception.Message);

            return;
        }

        Debug.Log(LogLevel.Error, MethodBase.GetCurrentMethod()!.Name + " test failed!");
    }

    private static void DuplicateActorMemoryManagementTest()
    {
        Debug.Log(LogLevel.Display, "Starting " + MethodBase.GetCurrentMethod()!.Name + "...");

        const string actorName = "Player";

        try
        {
            Pawn pawn = new();
            Pawn namedPawn = new(actorName);
            Pawn duplicateActor = World.GetActor<Pawn>(actorName)!;

            if (!namedPawn.Equals(duplicateActor))
            {
                Debug.Log(LogLevel.Error, "Actor reference equality check failed!");

                return;
            }

            _ = namedPawn.Destroy();

            Debug.Log(LogLevel.Display, "Triggering invalid actions after the destruction");

            duplicateActor.SetLifeSpan(5.0f);
            _ = duplicateActor.Destroy();
        }

        catch (Exception exception)
        {
            Debug.Log(LogLevel.Display, "The exception has successfully reached: " + exception.Message);

            return;
        }

        Debug.Log(LogLevel.Error, MethodBase.GetCurrentMethod()!.Name + " test failed!");
    }

    private static void DuplicateComponentMemoryManagementTest()
    {
        Debug.Log(LogLevel.Display, "Starting " + MethodBase.GetCurrentMethod()!.Name + "...");

        try
        {
            Actor actor = new();
            SceneComponent sceneComponent = new(actor, setAsRoot: true);
            SceneComponent duplicateReference = actor.GetRootComponent<SceneComponent>()!;

            if (!sceneComponent.Equals(duplicateReference))
            {
                Debug.Log(LogLevel.Error, "Scene component reference equality check failed!");

                return;
            }

            sceneComponent.Destroy();

            Debug.Log(LogLevel.Display, "Triggering invalid actions after the destruction");

            duplicateReference.AddLocalOffset(default);
            duplicateReference.Destroy();
        }

        catch (Exception exception)
        {
            Debug.Log(LogLevel.Display, "The exception has successfully reached: " + exception.Message);

            return;
        }

        Debug.Log(LogLevel.Error, MethodBase.GetCurrentMethod()!.Name + " test failed!");
    }

    private static void ConsoleVariablesMemoryManagementTest()
    {
        Debug.Log(LogLevel.Display, "Starting " + MethodBase.GetCurrentMethod()!.Name + "...");

        const string variableName = "TestVariable";

        try
        {
            ConsoleVariable variable = ConsoleManager.RegisterVariable(variableName, "A test variable", 64)!;

            if (!ConsoleManager.IsRegisteredVariable(variableName))
            {
                Debug.Log(LogLevel.Error, "Variable registration check failed!");

                return;
            }

            ConsoleManager.UnregisterObject(variableName);

            Debug.Log(LogLevel.Display, "Triggering invalid actions after the destruction");

            _ = variable?.GetInt();
        }

        catch (Exception exception)
        {
            Debug.Log(LogLevel.Display, "The exception has successfully reached: " + exception.Message);

            return;
        }

        Debug.Log(LogLevel.Error, MethodBase.GetCurrentMethod()!.Name + " test failed!");
    }
}