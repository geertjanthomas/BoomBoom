using BoomBoom.Properties;
using System.ComponentModel;

namespace BoomBoom;

public class BombTile : UserControl, IBombTile
{
    private Label? _label;
    private PictureBox? _picture;
    private readonly ButtonPanel _button;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public GameCell GameCell { get; set; } = new GameCell();

    public event BombTileClick? Clicked;
    public event BombTileClick? Flagged;

    public BombTile()
    {
        _button = BuildButton();
        Controls.Add(_button);
    }

    public void Initialize(GameCell gameCell)
    {
        GameCell = gameCell;
        gameCell.Tile = this;
        gameCell.Flagged = false;
        Name = $"tile_{gameCell.Column}_{gameCell.Row}";
        _button.Visible = true;
        _button.GameCell = GameCell;
        _button.BackgroundImage = NullImage;
        
        if (_picture != null)
        {
            _picture.Visible = false;
        }
        
        if (_label == null)
        {
            return;
        }

        _label.Visible = false;
    }

    private ButtonPanel BuildButton()
    {
        var buttonPanel = new ButtonPanel();
        buttonPanel.BackColor = SystemColors.ControlLight;
        buttonPanel.Margin = Padding.Empty;
        buttonPanel.Dock = DockStyle.Fill;
        buttonPanel.Location = new Point(0, 0);
        buttonPanel.Name = "button";
        buttonPanel.Size = new Size(150, 150);
        buttonPanel.TabIndex = 2;
        buttonPanel.BackgroundImageLayout = ImageLayout.Stretch;
        buttonPanel.BorderColor = Color.AliceBlue;
        buttonPanel.ActiveBorderColor = Color.Blue;
        buttonPanel.MouseClick += new MouseEventHandler(button_Click);
        return buttonPanel;
    }

    private Image NullImage
    {
        get
        {
            var bitmap = new Bitmap(1, 1);
            Graphics.FromImage((Image)bitmap).Clear(Color.Transparent);
            return (Image)bitmap;
        }
    }

    private void button_Click(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left && !GameCell.Flagged)
        {
            Clicked?.Invoke(this, GameCell);
        }
        if (e.Button != MouseButtons.Right)
        {
            return;
        }
        GameCell.Flagged = !GameCell.Flagged;
        _button.BackgroundImage = GameCell.Flagged ? (Image)Resources.redflag : NullImage;
        Flagged?.Invoke(this, GameCell);
    }

    private void BuildPicture(Image image)
    {
        _picture = new PictureBox();
        _picture.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        _picture.BackColor = Color.Transparent;
        _picture.Image = image;
        _picture.Location = new Point(0, 0);
        _picture.Name = "picture";
        _picture.Size = Size;
        _picture.SizeMode = PictureBoxSizeMode.StretchImage;
        _picture.TabIndex = 1;
        _picture.TabStop = false;
        _picture.Visible = true;
    }

    private void BuildLabel()
    {
        var labelColor = Color.Black;
        if (!GameCell.HasBomb)
        {
            var numberOfAdjacentBombs = GameCell.NumberOfAdjacentBombs;
            switch (numberOfAdjacentBombs)
            {
                case 1:
                    labelColor = Color.Blue;
                    break;
                case 2:
                    labelColor = Color.Green;
                    break;
                case 3:
                    labelColor = Color.Red;
                    break;
                case 4:
                    labelColor = Color.Purple;
                    break;
                case 5:
                    labelColor = Color.Magenta;
                    break;
                default:
                    labelColor = Color.Black;
                    break;
            }
        }

        _label = new Label
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 16f, FontStyle.Bold),
            Location = new Point(0, 0),
            Name = "label",
            Size = Size,
            TabIndex = 3,
            TextAlign = ContentAlignment.MiddleCenter,
            Visible = true,
            Text = GameCell.HasBomb ? "B" : (GameCell.NumberOfAdjacentBombs > 0 ? GameCell.NumberOfAdjacentBombs.ToString() : ""),
            ForeColor = labelColor
        };
    }

    public void Expose(bool endOfGame = false)
    {
        SuspendLayout();
        _button.Visible = false;
        if (endOfGame)
        {
            if (GameCell.HasBomb)
            {
                BuildPicture((Image)Resources.BlackMine);
                Controls.Add(_picture);
            }
            else
            {
                _button.Visible = true;
            }
        }
        else if (GameCell.HasBomb)
        {
            BuildPicture((Image)Resources.Explosion);
            Controls.Add(_picture);
        }
        else
        {
            BuildLabel();
            Controls.Add(_label);
        }
        ResumeLayout();
    }
}

public delegate void BombTileClick(BombTile sender, GameCell cell);

