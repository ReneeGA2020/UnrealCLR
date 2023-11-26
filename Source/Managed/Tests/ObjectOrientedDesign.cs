namespace UnrealEngine.Tests;
public class ObjectOrientedDesign : ISystem
{
    private readonly Entity[] entities;
    private readonly Material material;
    private readonly Random random;
    private const int maxEntities = 10;

    public ObjectOrientedDesign()
    {
        entities = new Entity[maxEntities];
        material = Material.Load("/Game/Tests/BasicMaterial");
        random = new();
    }

    public void OnBeginPlay()
    {
        World.GetFirstPlayerController().SetViewTarget(World.GetActor<Camera>("MainCamera"));

        for (int i = 0; i < maxEntities; i++)
        {
            string entityName = "Entity" + (i > 0 ? i.ToString() : string.Empty);

            entities[i] = new(entityName);
            entities[i].CreateMesh(material, 1.0f, "StateComponent", true);
            entities[i].StateComponent.SetRelativeRotation(Maths.CreateFromYawPitchRoll(5.0f * i, 0.0f, 0.0f));
            entities[i].StateComponent.CreateAndSetMaterialInstanceDynamic(0).SetVectorParameterValue("Color", new((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble()));
            entities[i].StateComponent.SetRelativeLocation(new(0.0f, 0.0f, 120.0f * i));
            entities[i].StateComponent.AddLocalOffset(new(0.0f, 0.0f, -420.0f));

            Entity entity = World.GetActor<Entity>(entityName);
            StateComponent component = entity.GetComponent<StateComponent>();

            Assert.IsTrue(entity.Equals(entities[i]));
            Assert.IsTrue(component.Equals(entities[i].StateComponent));
        }

        Debug.AddOnScreenMessage(-1, 3.0f, Color.LightGreen, "Actors are spawned! Number of actors in the world: " + World.ActorCount);
    }

    public void OnTick(float deltaTime)
    {
        for (int i = 0; i < maxEntities; i++)
        {
            entities[i].StateComponent.AddLocalRotation(new(Vector3.UnitZ * entities[i].StateComponent.RotationSpeed * deltaTime, -1.0f));
        }
    }

    public void OnEndPlay()
    {
        Debug.ClearOnScreenMessages();
    }

    private class Entity : Actor
    {
        public StateComponent StateComponent { get; private set; }

        public Entity(string name = null) : base(name) { }

        public void CreateMesh(Material material, float rotationSpeed, string name = null, bool setAsRoot = false)
        {
            StateComponent = new(this, rotationSpeed, name, setAsRoot);
            _ = StateComponent.SetStaticMesh(StaticMesh.Cube);
            StateComponent.SetMaterial(0, material);
        }
    }

    private class StateComponent : StaticMeshComponent
    {
        public float RotationSpeed { get; set; }

        public StateComponent(Entity entity, float rotationSpeed, string name = null, bool setAsRoot = false) : base(entity, name, setAsRoot)
        {
            RotationSpeed = rotationSpeed;
        }
    }
}