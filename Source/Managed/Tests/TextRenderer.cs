namespace UnrealEngine.Tests;
public class TextRenderer : ISystem
{
    private readonly Actor actor;
    private readonly TextRenderComponent textRenderComponent;
    private readonly Font font;
    private readonly Material material;

    public TextRenderer()
    {
        actor = new("TextActor");
        textRenderComponent = new(actor);
        font = Font.Load("/Game/Tests/PlayFont");
        material = Material.Load("/Game/Tests/PlayFontMaterial");
    }

    public void OnBeginPlay()
    {
        textRenderComponent.SetFont(font);
        textRenderComponent.SetTextMaterial(material);
        textRenderComponent.SetText("WELCOME TO THE WORLD");
        textRenderComponent.SetHorizontalSpacingAdjustment(2.0f);
        textRenderComponent.SetHorizontalAlignment(HorizontalTextAlignment.Center);
        textRenderComponent.SetVerticalAlignment(VerticalTextAlignment.Center);
        textRenderComponent.SetWorldLocation(new Vector3(200.0f, 0.0f, 0.0f));
        textRenderComponent.SetWorldRotation(Maths.Euler(0.0f, 0.0f, 180.0f));
    }
}