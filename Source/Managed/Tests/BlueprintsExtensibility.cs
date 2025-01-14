namespace UnrealEngine.Tests;
public class BlueprintsExtensibility : ISystem
{
    private readonly Blueprint? blueprintActor;
    private readonly Blueprint? blueprintSceneComponent;
    private readonly Actor actor;
    private readonly SceneComponent sceneComponent;
    private readonly LevelScript? levelScript;
    private readonly SkeletalMeshComponent? skeletalMeshComponent;
    private readonly AnimationInstance? animationInstance;
    private const string boolProperty = "Test Bool";
    private const string byteProperty = "Test Byte";
    private const string intProperty = "Test Int";
    private const string floatProperty = "Test Float";
    private const string stringProperty = "Test String";
    private const string textProperty = "Test Text";

    public BlueprintsExtensibility()
    {
        blueprintActor = Blueprint.Load("/Game/Tests/BlueprintActor");
        blueprintSceneComponent = Blueprint.Load("/Game/Tests/BlueprintSceneComponent");
        actor = new(blueprint: blueprintActor);
        sceneComponent = new(actor, blueprint: blueprintSceneComponent);
        levelScript = World.GetActor<LevelScript>();
        skeletalMeshComponent = actor.GetComponent<SkeletalMeshComponent>();
        animationInstance = skeletalMeshComponent?.GetAnimationInstance();
    }

    public void OnBeginPlay()
    {
        const string eventMessage = "Blueprint event dispatched";
        const float eventValue = 100.0f;

        Assert.IsTrue(levelScript?.Invoke($"TestEvent \"{eventMessage}: \" {eventValue}") == true);
        Assert.IsTrue(actor.IsSpawned);
        Assert.IsTrue(sceneComponent.IsCreated);

        TestActorBoolProperty();
        TestActorByteProperty();
        TestActorIntProperty();
        TestActorFloatProperty();
        TestActorStringProperty();
        TestActorTextProperty();
        TestSceneComponentBoolProperty();
        TestSceneComponentByteProperty();
        TestSceneComponentIntProperty();
        TestSceneComponentFloatProperty();
        TestSceneComponentStringProperty();
        TestSceneComponentTextProperty();
        TestLevelScriptBoolProperty();
        TestLevelScriptByteProperty();
        TestLevelScriptIntProperty();
        TestLevelScriptFloatProperty();
        TestLevelScriptStringProperty();
        TestLevelScriptTextProperty();
        TestAnimationBoolProperty();
        TestAnimationByteProperty();
        TestAnimationIntProperty();
        TestAnimationFloatProperty();
        TestAnimationStringProperty();
        TestAnimationTextProperty();
    }

    public void OnEndPlay()
    {
        Debug.ClearOnScreenMessages();
    }

    private void TestActorBoolProperty()
    {
        bool value = false;

        Assert.IsTrue(actor.SetBool(boolProperty, true));

        if (actor.GetBool(boolProperty, ref value))
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.LimeGreen, value.GetType() + " actor property value retrieved: " + value);
        }
        else
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.Red, value.GetType() + " actor property value retrievement failed!");
        }
    }

    private void TestActorByteProperty()
    {
        byte value = 0;

        Assert.IsTrue(actor.SetByte(byteProperty, 128));

        if (actor.GetByte(byteProperty, ref value))
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.LimeGreen, value.GetType() + " actor property value retrieved: " + value);
        }
        else
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.Red, value.GetType() + " actor property value retrievement failed!");
        }
    }

    private void TestActorIntProperty()
    {
        int value = 0;

        Assert.IsTrue(actor.SetInt(intProperty, 500));

        if (actor.GetInt(intProperty, ref value))
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.LimeGreen, value.GetType() + " actor property value retrieved: " + value);
        }
        else
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.Red, value.GetType() + " actor property value retrievement failed!");
        }
    }

    private void TestActorFloatProperty()
    {
        double value = 0;

        Assert.IsTrue(actor.SetDouble(floatProperty, 250.5f));

        if (actor.GetDouble(floatProperty, ref value))
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.LimeGreen, value.GetType() + " actor property value retrieved: " + value);
        }
        else
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.Red, value.GetType() + " actor property value retrievement failed!");
        }
    }

    private void TestActorStringProperty()
    {
        string value = string.Empty;

        Assert.IsTrue(actor.SetString(stringProperty, "Test string from managed code"));

        if (actor.GetString(stringProperty, ref value))
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.LimeGreen, value.GetType() + " actor property value retrieved: " + value);
        }
        else
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.Red, value.GetType() + " actor property value retrievement failed!");
        }
    }

    private void TestActorTextProperty()
    {
        string value = string.Empty;

        Assert.IsTrue(actor.SetText(textProperty, "Test text from managed code"));

        if (actor.GetText(textProperty, ref value))
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.LimeGreen, value.GetType() + " actor property value retrieved: " + value);
        }
        else
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.Red, value.GetType() + " actor property value retrievement failed!");
        }
    }

    private void TestSceneComponentBoolProperty()
    {
        bool value = false;

        Assert.IsTrue(sceneComponent.SetBool(boolProperty, true));

        if (sceneComponent.GetBool(boolProperty, ref value))
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.LimeGreen, value.GetType() + " scene component property value retrieved: " + value);
        }
        else
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.Red, value.GetType() + " scene component property value retrievement failed!");
        }
    }

    private void TestSceneComponentByteProperty()
    {
        byte value = 0;

        Assert.IsTrue(sceneComponent.SetByte(byteProperty, 128));

        if (sceneComponent.GetByte(byteProperty, ref value))
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.LimeGreen, value.GetType() + " scene component property value retrieved: " + value);
        }
        else
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.Red, value.GetType() + " scene component property value retrievement failed!");
        }
    }

    private void TestSceneComponentIntProperty()
    {
        int value = 0;

        Assert.IsTrue(sceneComponent.SetInt(intProperty, 500));

        if (sceneComponent.GetInt(intProperty, ref value))
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.LimeGreen, value.GetType() + " scene component property value retrieved: " + value);
        }
        else
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.Red, value.GetType() + " scene component property value retrievement failed!");
        }
    }

    private void TestSceneComponentFloatProperty()
    {
        double value = 0;

        Assert.IsTrue(sceneComponent.SetDouble(floatProperty, 250.5d));

        if (sceneComponent.GetDouble(floatProperty, ref value))
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.LimeGreen, value.GetType() + " scene component property value retrieved: " + value);
        }
        else
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.Red, value.GetType() + " scene component property value retrievement failed!");
        }
    }

    private void TestSceneComponentStringProperty()
    {
        string value = string.Empty;

        Assert.IsTrue(sceneComponent.SetString(stringProperty, "Test string from managed code"));

        if (sceneComponent.GetString(stringProperty, ref value))
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.LimeGreen, value.GetType() + " scene component property value retrieved: " + value);
        }
        else
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.Red, value.GetType() + " scene component property value retrievement failed!");
        }
    }

    private void TestSceneComponentTextProperty()
    {
        string value = string.Empty;

        Assert.IsTrue(sceneComponent.SetText(textProperty, "Test text from managed code"));

        if (sceneComponent.GetText(textProperty, ref value))
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.LimeGreen, value.GetType() + " scene component property value retrieved: " + value);
        }
        else
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.Red, value.GetType() + " scene component property value retrievement failed!");
        }
    }

    private void TestLevelScriptBoolProperty()
    {
        bool value = false;

        Assert.IsTrue(levelScript?.SetBool(boolProperty, true) == true);

        if (levelScript?.GetBool(boolProperty, ref value) == true)
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.LimeGreen, value.GetType() + " level script property value retrieved: " + value);
        }
        else
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.Red, value.GetType() + " level script property value retrievement failed!");
        }
    }

    private void TestLevelScriptByteProperty()
    {
        byte value = 0;

        Assert.IsTrue(levelScript?.SetByte(byteProperty, 128) == true);

        if (levelScript?.GetByte(byteProperty, ref value) == true)
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.LimeGreen, value.GetType() + " level script property value retrieved: " + value);
        }
        else
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.Red, value.GetType() + " level script property value retrievement failed!");
        }
    }

    private void TestLevelScriptIntProperty()
    {
        int value = 0;

        Assert.IsTrue(levelScript?.SetInt(intProperty, 500) == true);

        if (levelScript?.GetInt(intProperty, ref value) == true)
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.LimeGreen, value.GetType() + " level script property value retrieved: " + value);
        }
        else
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.Red, value.GetType() + " level script property value retrievement failed!");
        }
    }

    private void TestLevelScriptFloatProperty()
    {
        double value = 0;

        Assert.IsTrue(levelScript?.SetDouble(floatProperty, 250.5f) == true);

        if (levelScript?.GetDouble(floatProperty, ref value) == true)
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.LimeGreen, value.GetType() + " level script property value retrieved: " + value);
        }
        else
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.Red, value.GetType() + " level script property value retrievement failed!");
        }
    }

    private void TestLevelScriptStringProperty()
    {
        string value = string.Empty;

        Assert.IsTrue(levelScript?.SetString(stringProperty, "Test string message from managed code") == true);

        if (levelScript?.GetString(stringProperty, ref value) == true)
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.LimeGreen, value.GetType() + " level script property value retrieved: " + value);
        }
        else
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.Red, value.GetType() + " level script property value retrievement failed!");
        }
    }

    private void TestLevelScriptTextProperty()
    {
        string value = string.Empty;

        Assert.IsTrue(levelScript?.SetText(textProperty, "Test text message from managed code") == true);

        if (levelScript?.GetText(textProperty, ref value) == true)
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.LimeGreen, value.GetType() + " level script property value retrieved: " + value);
        }
        else
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.Red, value.GetType() + " level script property value retrievement failed!");
        }
    }

    private void TestAnimationBoolProperty()
    {
        bool value = false;

        Assert.IsTrue(animationInstance?.SetBool(boolProperty, true) == true);

        if (animationInstance?.GetBool(boolProperty, ref value) == true)
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.LimeGreen, value.GetType() + " animation property value retrieved: " + value);
        }
        else
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.Red, value.GetType() + " animation property value retrievement failed!");
        }
    }

    private void TestAnimationByteProperty()
    {
        byte value = 0;

        Assert.IsTrue(animationInstance?.SetByte(byteProperty, 128) == true);

        if (animationInstance?.GetByte(byteProperty, ref value) == true)
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.LimeGreen, value.GetType() + " animation property value retrieved: " + value);
        }
        else
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.Red, value.GetType() + " animation property value retrievement failed!");
        }
    }

    private void TestAnimationIntProperty()
    {
        int value = 0;

        Assert.IsTrue(animationInstance?.SetInt(intProperty, 500) == true);

        if (animationInstance?.GetInt(intProperty, ref value) == true)
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.LimeGreen, value.GetType() + " animation property value retrieved: " + value);
        }
        else
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.Red, value.GetType() + " animation property value retrievement failed!");
        }
    }

    private void TestAnimationFloatProperty()
    {
        double value = 0;

        Assert.IsTrue(animationInstance?.SetDouble(floatProperty, 250.5f) == true);

        if (animationInstance?.GetDouble(floatProperty, ref value) == true)
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.LimeGreen, value.GetType() + " animation property value retrieved: " + value);
        }
        else
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.Red, value.GetType() + " animation property value retrievement failed!");
        }
    }

    private void TestAnimationStringProperty()
    {
        string value = string.Empty;

        Assert.IsTrue(animationInstance?.SetString(stringProperty, "Test string message from managed code") == true);

        if (animationInstance?.GetString(stringProperty, ref value) == true)
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.LimeGreen, value.GetType() + " animation property value retrieved: " + value);
        }
        else
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.Red, value.GetType() + " animation property value retrievement failed!");
        }
    }

    private void TestAnimationTextProperty()
    {
        string value = string.Empty;

        Assert.IsTrue(animationInstance?.SetText(textProperty, "Test text message from managed code") == true);

        if (animationInstance?.GetText(textProperty, ref value) == true)
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.LimeGreen, value.GetType() + " animation property value retrieved: " + value);
        }
        else
        {
            Debug.AddOnScreenMessage(-1, 30.0f, Color.Red, value.GetType() + " animation property value retrievement failed!");
        }
    }

    public static void TestBlueprintActorFunction(ObjectReference self)
    {
        Actor? blueprintActor = self.ToActor<Actor>();

        Debug.AddOnScreenMessage(-1, 30.0f, Color.Orange, "Cheers from managed function of the " + blueprintActor?.Name);
    }

    public static void TestBlueprintComponentFunction(ObjectReference self)
    {
        SceneComponent? blueprintSceneComponent = self.ToComponent<SceneComponent>();

        Debug.AddOnScreenMessage(-1, 30.0f, Color.Orange, "Cheers from managed function of the " + blueprintSceneComponent?.Name);
    }
}