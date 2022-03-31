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
        font = Font.Load("/Game/Tests/TestFont");
        material = Material.Load("/Game/Tests/PlayFontMaterial_2");
    }

    public void OnBeginPlay()
    {
        textRenderComponent.SetFont(font);
        textRenderComponent.SetTextMaterial(material);
        textRenderComponent.SetText("中文字体测试，这是用C#代码生成的字体");
        textRenderComponent.SetTextRenderColor(Color.Red);
        textRenderComponent.SetHorizontalSpacingAdjustment(2.0f);
        textRenderComponent.SetHorizontalAlignment(HorizontalTextAligment.Center);
        textRenderComponent.SetVerticalAlignment(VerticalTextAligment.Center);
        textRenderComponent.SetWorldLocation(new Vector3(200.0f, 0.0f, 0.0f));
        textRenderComponent.SetWorldRotation(Maths.Euler(0.0f, 0.0f, 180.0f));
    }
}
