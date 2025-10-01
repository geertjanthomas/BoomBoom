using System.ComponentModel;

namespace BoomBoom;

[DesignerCategory("Code")]
public class ButtonPanel : Panel
{
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public GameCell GameCell { get; set; } = new GameCell();

    public ButtonPanel()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true);
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public Color BorderColor { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public Color ActiveBorderColor { get; set; }

    protected override void OnMouseEnter(EventArgs e)
    {
        if (GameCell.Flagged)
            return;
        var pen1 = new Pen(ActiveBorderColor);
        var graphics = Graphics.FromHwnd(Handle);
        var pen2 = pen1;
        var clientSize = ClientSize;
        var width = clientSize.Width - 1;
        clientSize = ClientSize;
        var height = clientSize.Height - 1;
        graphics.DrawRectangle(pen2, 0, 0, width, height);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        var pen1 = new Pen(BorderColor);
        var graphics = Graphics.FromHwnd(Handle);
        var pen2 = pen1;
        var clientSize = ClientSize;
        var width = clientSize.Width - 1;
        clientSize = ClientSize;
        var height = clientSize.Height - 1;
        graphics.DrawRectangle(pen2, 0, 0, width, height);
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        base.OnPaintBackground(e);
        if (GameCell.Flagged)
            return;
        var pen = new Pen(BorderColor);
        e.Graphics.DrawRectangle(pen, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
    }
}
