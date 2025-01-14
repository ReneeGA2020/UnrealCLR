namespace UnrealEngine.Tests;
public class ExternalConsistency : ISystem
{
    public void OnBeginPlay()
    {
        AssetRegistryTest();
        CommandLineArgumentsTest();
        ReferencesEqualityTest();
        NamingTest();
        HashCodesTest();
        ActorsHierarchyTest();
        ChildActorsTest();
        ComponentsAttachmentTest();
        ComponentsMatchingTest();
        NonSceneComponentTest();
        ObjectIDsTest();
        MaxFramesPerSecondTest();
        TagsTest();

        Debug.AddOnScreenMessage(-1, 10.0f, Color.MediumTurquoise, "Verify " + MethodBase.GetCurrentMethod()?.DeclaringType + " results in output log!");
    }

    private void AssetRegistryTest()
    {
        Debug.Log(LogLevel.Display, "Starting " + MethodBase.GetCurrentMethod()?.Name + "...");

        AssetRegistry assetRegistry = new();

        if (!assetRegistry.HasAssets("/Game/Tests", true))
        {
            Debug.Log(LogLevel.Error, "Asset registry assets path test failed!");

            return;
        }

        bool assetFound = false;

        void OnAsset(Asset asset)
        {
            if (asset.Path == "/Game/Tests/Tests")
            {
                assetFound = true;
            }
        }

        assetRegistry.ForEachAsset(OnAsset, "/Game/Tests", true);

        if (!assetFound)
        {
            Debug.Log(LogLevel.Error, "Asset registry path to asset test failed!");

            return;
        }

        Debug.Log(LogLevel.Display, "Test passed successfully");
    }

    private static void CommandLineArgumentsTest()
    {
        Debug.Log(LogLevel.Display, "Starting " + MethodBase.GetCurrentMethod()?.Name + "...");

        string append = " -test 1";

        CommandLine.Append(append);

        string arguments = CommandLine.Get();

        if (!arguments.Contains(append, StringComparison.CurrentCulture))
        {
            Debug.Log(LogLevel.Error, "Command-line append operation failed!");

            return;
        }

        append = "-test 2";

        CommandLine.Set(append);

        arguments = CommandLine.Get();

        if (!arguments.Contains(append, StringComparison.CurrentCulture))
        {
            Debug.Log(LogLevel.Error, "Command-line set operation failed!");

            return;
        }

        ushort exceptions = 0;

        try
        {
            CommandLine.Append(null!);
        }

        catch
        {
            exceptions++;
        }

        try
        {
            CommandLine.Set(null!);
        }

        catch
        {
            exceptions++;
        }

        if (exceptions != 2)
        {
            Debug.Log(LogLevel.Error, "Command-line null checks failed!");

            return;
        }

        Debug.Log(LogLevel.Display, "Test passed successfully");
    }

    private static void ReferencesEqualityTest()
    {
        Debug.Log(LogLevel.Display, "Starting " + MethodBase.GetCurrentMethod()?.Name + "...");

        TriggerBox actorLeft = new();
        TriggerSphere actorRight = new();
        SceneComponent sceneComponentLeft = new(actorLeft);
        SceneComponent sceneComponentRight = new(actorRight);

        if (sceneComponentLeft.Equals(sceneComponentRight) || !sceneComponentLeft.Equals(sceneComponentLeft))
        {
            Debug.Log(LogLevel.Error, "Scene components equality check failed!");

            return;
        }

        if (actorLeft.Equals(sceneComponentRight.GetActor<TriggerSphere>()) || !actorLeft.Equals(sceneComponentLeft.GetActor<TriggerBox>()))
        {
            Debug.Log(LogLevel.Error, "Scene components owners equality check failed!");

            return;
        }

        sceneComponentLeft.Destroy();

        if (sceneComponentRight.Equals(sceneComponentLeft))
        {
            Debug.Log(LogLevel.Error, "Scene components equality check after destruction failed!");

            return;
        }

        sceneComponentRight.Destroy();

        if (sceneComponentRight.Equals(sceneComponentRight))
        {
            Debug.Log(LogLevel.Error, "Scene components equality check with null failed!");

            return;
        }

        if (actorLeft.Equals(actorRight) || !actorLeft.Equals(actorLeft))
        {
            Debug.Log(LogLevel.Error, "Actors equality check failed!");

            return;
        }

        _ = actorLeft.Destroy();

        if (actorRight.Equals(actorLeft))
        {
            Debug.Log(LogLevel.Error, "Actors equality check after destruction failed!");

            return;
        }

        _ = actorRight.Destroy();

        if (actorRight.Equals(actorRight))
        {
            Debug.Log(LogLevel.Error, "Actors equality check with null failed!");

            return;
        }

        Debug.Log(LogLevel.Display, "Test passed successfully");
    }

    private static void NamingTest()
    {
        Debug.Log(LogLevel.Display, "Starting " + MethodBase.GetCurrentMethod()?.Name + "...");

        const string actorName = "TestActorName";
        const string actorShortName = "TestActor";
        const string componentName = "TestComponentName";
        const string componentShortName = "TestComponent";
        const string renamedSuffix = "Renamed";

        Actor actor = new(actorName);
        SceneComponent sceneComponent = new(actor, componentName);

        if (actor.Name != actorName || sceneComponent.Name != componentName)
        {
            Debug.Log(LogLevel.Error, "Names check failed!");

            return;
        }

        Actor namedActor = World.GetActor<Actor>(actorName)!;

        if (!actor.Equals(namedActor))
        {
            Debug.Log(LogLevel.Error, "Actor from world with a name check after removing failed!");

            return;
        }

        actor.Rename(actorName + renamedSuffix);
        sceneComponent.Rename(componentName + renamedSuffix);

        if (actor.Name != actorName + renamedSuffix || sceneComponent.Name != componentName + renamedSuffix)
        {
            Debug.Log(LogLevel.Error, "Renaming to more characters check failed!");

            return;
        }

        actor.Rename(actorShortName);
        sceneComponent.Rename(componentShortName);

        if (actor.Name != actorShortName || sceneComponent.Name != componentShortName)
        {
            Debug.Log(LogLevel.Error, "Renaming to less characters check failed!");

            return;
        }

        Debug.Log(LogLevel.Display, "Test passed successfully");
    }

    private static void HashCodesTest()
    {
        Debug.Log(LogLevel.Display, "Starting " + MethodBase.GetCurrentMethod()?.Name + "...");

        Actor actorLeft = new();
        Actor actorRight = new();
        SceneComponent sceneComponentLeft = new(actorLeft);
        SceneComponent sceneComponentRight = new(actorRight);

        if (sceneComponentLeft.GetHashCode() == sceneComponentRight.GetHashCode() || sceneComponentLeft.GetHashCode() != sceneComponentLeft.GetHashCode())
        {
            Debug.Log(LogLevel.Error, "Scene components hash codes check failed!");

            return;
        }

        sceneComponentLeft.Destroy();

        if (sceneComponentRight.GetHashCode() == sceneComponentLeft.GetHashCode())
        {
            Debug.Log(LogLevel.Error, "Scene components hash codes check after destruction failed!");

            return;
        }

        if (actorLeft.GetHashCode() == actorRight.GetHashCode() || actorLeft.GetHashCode() != actorLeft.GetHashCode())
        {
            Debug.Log(LogLevel.Error, "Actors hash codes check failed!");

            return;
        }

        _ = actorLeft.Destroy();

        if (actorRight.GetHashCode() == actorLeft.GetHashCode())
        {
            Debug.Log(LogLevel.Error, "Actors hash codes check after destruction failed!");

            return;
        }

        Debug.Log(LogLevel.Display, "Test passed successfully");
    }

    private static void ActorsHierarchyTest()
    {
        Debug.Log(LogLevel.Display, "Starting " + MethodBase.GetCurrentMethod()?.Name + "...");

        const string actorName = "TestPlayerController";

        PlayerController playerController = new(actorName);
        Actor actor = World.GetActor<Actor>(actorName)!;

        if (playerController == null || actor == null)
        {
            Debug.Log(LogLevel.Error, "Actor valid references check failed!");

            return;
        }

        AmbientSound ambientSound = World.GetActor<AmbientSound>(actorName)!;
        Brush brush = World.GetActor<Brush>(actorName)!;

        if (ambientSound != null || brush != null)
        {
            Debug.Log(LogLevel.Error, "Actor invalid references check failed!");

            return;
        }

        Debug.Log(LogLevel.Display, "Test passed successfully");
    }

    private void ChildActorsTest()
    {
        Debug.Log(LogLevel.Display, "Starting " + MethodBase.GetCurrentMethod()?.Name + "...");

        Actor actor = new();
        ChildActorComponent childActorComponent = new(actor, setAsRoot: true);
        TriggerBox childActor = childActorComponent.SetChildActor<TriggerBox>()!;

        if (childActor == null)
        {
            Debug.Log(LogLevel.Error, "Child actor creation check failed!");

            return;
        }

        childActor = childActorComponent.GetChildActor<TriggerBox>()!;

        if (childActor == null)
        {
            Debug.Log(LogLevel.Error, "Child actor obtainment check failed!");

            return;
        }

        BoxComponent boxComponent = childActor.GetComponent<BoxComponent>()!;
        Vector3 initialExtent = new(100.0f, 100.0f, 100.0f);

        boxComponent.InitBoxExtent(initialExtent);

        if (initialExtent != boxComponent.GetUnscaledBoxExtent())
        {
            Debug.Log(LogLevel.Error, "Child actor box extent check failed!");

            return;
        }

        int attachedActorCount = 0;

        void OnAttachedActor(Actor actor)
        {
            attachedActorCount++;
        }

        actor.ForEachAttachedActor((Action<Actor>)OnAttachedActor);

        if (attachedActorCount != 1)
        {
            Debug.Log(LogLevel.Error, "Batched attached actor check failed!");

            return;
        }

        int childActorCount = 0;

        void OnChildActor(Actor actor)
        {
            childActorCount++;
        }

        actor.ForEachChildActor((Action<Actor>)OnChildActor);

        if (childActorCount != 1)
        {
            Debug.Log(LogLevel.Error, "Batched child actor check failed!");

            return;
        }

        Debug.Log(LogLevel.Display, "Test passed successfully");
    }

    private void ComponentsAttachmentTest()
    {
        Debug.Log(LogLevel.Display, "Starting " + MethodBase.GetCurrentMethod()?.Name + "...");

        Actor actor = new();
        StaticMeshComponent staticMeshComponent = new(actor, setAsRoot: true);
        InstancedStaticMeshComponent instancedStaticMeshComponent = new(actor);
        SceneComponent sceneComponent = new(actor);
        BoxComponent boxComponent = new(actor);
        SphereComponent sphereComponent = new(actor);

        _ = sceneComponent.AttachToComponent(instancedStaticMeshComponent, AttachmentTransformRule.KeepRelativeTransform);

        if (!sceneComponent.IsAttachedToComponent(instancedStaticMeshComponent))
        {
            Debug.Log(LogLevel.Error, "Component attachment check failed!");

            return;
        }

        if (!sceneComponent.IsAttachedToActor(actor))
        {
            Debug.Log(LogLevel.Error, "Component attachment to actor check failed!");

            return;
        }

        int componentCount = 0;

        void OnComponent(SceneComponent component)
        {
            componentCount++;
        }

        actor.ForEachComponent((Action<SceneComponent>)OnComponent);

        if (componentCount != 5)
        {
            Debug.Log(LogLevel.Error, "Batched component check failed!");

            return;
        }

        int attachedChildCount = 0;

        void OnAttachedChild(SceneComponent component)
        {
            attachedChildCount++;
        }

        staticMeshComponent.ForEachAttachedChild((Action<SceneComponent>)OnAttachedChild);

        if (attachedChildCount != 3)
        {
            Debug.Log(LogLevel.Error, "Batched component child attachment check failed!");

            return;
        }

        Debug.Log(LogLevel.Display, "Test passed successfully");
    }

    private static void ComponentsMatchingTest()
    {
        Debug.Log(LogLevel.Display, "Starting " + MethodBase.GetCurrentMethod()?.Name + "...");

        Actor actor = new();
        _ = new StaticMeshComponent(actor, setAsRoot: true);
        SceneComponent sceneComponent = actor.GetRootComponent<SceneComponent>()!;
        InstancedStaticMeshComponent instancedStaticMeshComponent = actor.GetRootComponent<InstancedStaticMeshComponent>()!;

        if (sceneComponent == null)
        {
            Debug.Log(LogLevel.Error, "Component obtainment with inherited type matching to the root component failed!");

            return;
        }

        if (instancedStaticMeshComponent != null)
        {
            Debug.Log(LogLevel.Error, "Component obtainment with type matching to the root component failed!");

            return;
        }

        if (actor.GetComponent<StaticMeshComponent>() == null)
        {
            Debug.Log(LogLevel.Error, "Component type matching failed!");

            return;
        }

        if (actor.GetRootComponent<StaticMeshComponent>() == null)
        {
            Debug.Log(LogLevel.Error, "Component type matching to the root component failed!");

            return;
        }

        Debug.Log(LogLevel.Display, "Test passed successfully");
    }

    private static void NonSceneComponentTest()
    {
        Debug.Log(LogLevel.Display, "Starting " + MethodBase.GetCurrentMethod()?.Name + "...");

        Actor actor = new();
        RotatingMovementComponent rotatingMovementComponent = new(actor, "TestName");

        if (rotatingMovementComponent == null)
        {
            Debug.Log(LogLevel.Error, "Component was not instantiated!");

            return;
        }

        Debug.Log(LogLevel.Display, "Test passed successfully");
    }

    private static void ObjectIDsTest()
    {
        Debug.Log(LogLevel.Display, "Starting " + MethodBase.GetCurrentMethod()?.Name + "...");

        Actor actor = new();
        _ = new SceneComponent(actor, setAsRoot: true);

        Actor actorByID = World.GetActorByID<Actor>(actor.ID)!;

        if (actorByID == null)
        {
            Debug.Log(LogLevel.Error, "Actor with a valid ID was not found!");

            return;
        }

        SceneComponent sceneComponentByID = actorByID.GetComponent<SceneComponent>()!;

        if (sceneComponentByID == null)
        {
            Debug.Log(LogLevel.Error, "Component with a valid ID was not found!");

            return;
        }

        Debug.Log(LogLevel.Display, "Test passed successfully");
    }

    private static void MaxFramesPerSecondTest()
    {
        Debug.Log(LogLevel.Display, "Starting " + MethodBase.GetCurrentMethod()?.Name + "...");

        float currentMaxFPS = Engine.MaxFPS;
        float newMaxFPS = 60.0f;

        Engine.MaxFPS = newMaxFPS;

        float maxFPS = Engine.MaxFPS;

        if (!maxFPS.Equals(newMaxFPS))
        {
            Debug.Log(LogLevel.Error, "Max FPS check failed!");

            return;
        }

        Engine.MaxFPS = currentMaxFPS;

        Debug.Log(LogLevel.Display, "Test passed successfully");
    }

    private static void TagsTest()
    {
        Debug.Log(LogLevel.Display, "Starting " + MethodBase.GetCurrentMethod()?.Name + "...");

        Actor actor = new();
        SceneComponent sceneComponent = new(actor);

        const string tag = "TestTag";

        actor.AddTag(tag);

        if (!actor.HasTag(tag))
        {
            Debug.Log(LogLevel.Error, "Actor tag check failed!");

            return;
        }

        Actor taggedActor = World.GetActorByTag<Actor>(tag)!;

        if (!actor.Equals(taggedActor))
        {
            Debug.Log(LogLevel.Error, "Actor from world with a tag check failed!");

            return;
        }

        actor.RemoveTag(tag);

        if (actor.HasTag(tag))
        {
            Debug.Log(LogLevel.Error, "Actor tag check after removing failed!");

            return;
        }

        sceneComponent.AddTag(tag);

        if (!sceneComponent.HasTag(tag))
        {
            Debug.Log(LogLevel.Error, "Component tag check failed!");

            return;
        }

        SceneComponent taggedSceneComponent = taggedActor.GetComponentByTag<SceneComponent>(tag)!;

        if (!sceneComponent.Equals(taggedSceneComponent))
        {
            Debug.Log(LogLevel.Error, "Component from actor with a tag check failed!");

            return;
        }

        sceneComponent.RemoveTag(tag);

        if (sceneComponent.HasTag(tag))
        {
            Debug.Log(LogLevel.Error, "Component tag check after removing failed!");

            return;
        }

        Debug.Log(LogLevel.Display, "Test passed successfully");
    }
}