using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace TodaysMVPTool;

internal sealed class MvpToolMenu : IClickableMenu
{
    private const int MenuWidth = 640;
    private const int MenuHeight = 384;

    private readonly string title;
    private readonly string toolName;
    private readonly string usageText;
    private readonly string okText;
    private readonly Tool tool;

    private Rectangle okButtonBounds;
    private bool isOkButtonHovered;

    public MvpToolMenu(
        string title,
        string toolName,
        string usageText,
        string okText,
        Tool tool
    )
        : base(
            (Game1.uiViewport.Width - MenuWidth) / 2,
            (Game1.uiViewport.Height - MenuHeight) / 2,
            MenuWidth,
            MenuHeight
        )
    {
        this.title = title;
        this.toolName = toolName;
        this.usageText = usageText;
        this.okText = okText;
        this.tool = tool;
        this.RepositionOkButton();
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        if (this.okButtonBounds.Contains(x, y))
            this.Close();
    }

    public override void receiveGamePadButton(Buttons button)
    {
        if (button is Buttons.A or Buttons.B or Buttons.Start)
            this.Close();
    }

    public override void performHoverAction(int x, int y)
    {
        this.isOkButtonHovered = this.okButtonBounds.Contains(x, y);
    }

    public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
    {
        this.xPositionOnScreen = (newBounds.Width - this.width) / 2;
        this.yPositionOnScreen = (newBounds.Height - this.height) / 2;
        this.RepositionOkButton();
    }

    public override void draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(
            Game1.fadeToBlackRect,
            new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height),
            Color.Black * 0.75f
        );

        drawTextureBox(
            spriteBatch,
            this.xPositionOnScreen,
            this.yPositionOnScreen,
            this.width,
            this.height,
            Color.White
        );

        this.DrawCenteredText(spriteBatch, this.title, Game1.dialogueFont, this.yPositionOnScreen + 44, Game1.textColor);

        this.tool?.drawInMenu(
            spriteBatch,
            new Vector2(this.xPositionOnScreen + this.width / 2 - 32, this.yPositionOnScreen + 116),
            1f,
            1f,
            0.95f,
            StackDrawType.Hide,
            Color.White,
            true
        );

        this.DrawCenteredText(spriteBatch, this.toolName, Game1.dialogueFont, this.yPositionOnScreen + 202, Game1.textColor);
        this.DrawCenteredText(spriteBatch, this.usageText, Game1.smallFont, this.yPositionOnScreen + 258, Game1.textColor);

        Color buttonColor = this.isOkButtonHovered ? Color.Wheat : Color.White;
        drawTextureBox(
            spriteBatch,
            this.okButtonBounds.X,
            this.okButtonBounds.Y,
            this.okButtonBounds.Width,
            this.okButtonBounds.Height,
            buttonColor
        );
        this.DrawCenteredText(spriteBatch, this.okText, Game1.smallFont, this.okButtonBounds.Y + 12, Game1.textColor);

        this.drawMouse(spriteBatch);
    }

    private void Close()
    {
        Game1.playSound("bigDeSelect");
        this.exitThisMenu(playSound: false);
    }

    private void RepositionOkButton()
    {
        this.okButtonBounds = new Rectangle(
            this.xPositionOnScreen + this.width / 2 - 80,
            this.yPositionOnScreen + this.height - 76,
            160,
            56
        );
    }

    private void DrawCenteredText(SpriteBatch spriteBatch, string text, SpriteFont font, int y, Color color)
    {
        float x = this.xPositionOnScreen + (this.width - font.MeasureString(text).X) / 2f;
        Utility.drawTextWithShadow(spriteBatch, text, font, new Vector2(x, y), color);
    }
}
