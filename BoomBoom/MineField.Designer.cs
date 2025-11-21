namespace BoomBoom
{
    partial class MineField
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MineField));
            menuStrip = new MenuStrip();
            mnuGame = new ToolStripMenuItem();
            mnuGameBeginner = new ToolStripMenuItem();
            mnuGameIntermediate = new ToolStripMenuItem();
            mnuGameExpert = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            mnuSound = new ToolStripMenuItem();
            mnuHighScores = new ToolStripMenuItem();
            mnuTimer = new ToolStripMenuItem();
            mnuBombCount = new ToolStripMenuItem();
            mnuRestart = new ToolStripMenuItem();
            HighScores = new Panel();
            HighScoreLabel = new Label();
            HighScoresTitle = new Label();
            menuStrip.SuspendLayout();
            HighScores.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip
            // 
            menuStrip.Items.AddRange(new ToolStripItem[] { mnuGame, mnuTimer, mnuRestart, mnuBombCount });
            menuStrip.Location = new Point(0, 0);
            menuStrip.Name = "menuStrip";
            menuStrip.Size = new Size(800, 24);
            menuStrip.TabIndex = 0;
            menuStrip.Text = "menuStrip";
            // 
            // mnuGame
            // 
            mnuGame.DropDownItems.AddRange(new ToolStripItem[] { mnuGameBeginner, mnuGameIntermediate, mnuGameExpert, toolStripSeparator1, mnuSound, mnuHighScores });
            mnuGame.Name = "mnuGame";
            mnuGame.Size = new Size(50, 20);
            mnuGame.Text = "Game";
            // 
            // mnuGameBeginner
            // 
            mnuGameBeginner.Name = "mnuGameBeginner";
            mnuGameBeginner.Size = new Size(141, 22);
            mnuGameBeginner.Text = "Beginner";
            mnuGameBeginner.Click += mnuGameBeginner_Click;
            // 
            // mnuGameIntermediate
            // 
            mnuGameIntermediate.Name = "mnuGameIntermediate";
            mnuGameIntermediate.Size = new Size(141, 22);
            mnuGameIntermediate.Text = "Intermediate";
            mnuGameIntermediate.Click += mnuGameIntermediate_Click;
            // 
            // mnuGameExpert
            // 
            mnuGameExpert.Name = "mnuGameExpert";
            mnuGameExpert.Size = new Size(141, 22);
            mnuGameExpert.Text = "Expert";
            mnuGameExpert.Click += mnuGameExpert_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(138, 6);
            // 
            // mnuSound
            // 
            mnuSound.CheckOnClick = true;
            mnuSound.Name = "mnuSound";
            mnuSound.Size = new Size(141, 22);
            mnuSound.Text = "Sound";
            // 
            // mnuHighScores
            // 
            mnuHighScores.Name = "mnuHighScores";
            mnuHighScores.Size = new Size(141, 22);
            mnuHighScores.Text = "High scores";
            mnuHighScores.Click += mnuHighScores_Click;
            // 
            // mnuTimer
            // 
            mnuTimer.Alignment = ToolStripItemAlignment.Right;
            mnuTimer.Enabled = false;
            mnuTimer.Name = "mnuTimer";
            mnuTimer.Size = new Size(46, 20);
            mnuTimer.Text = "00:00";
            // 
            // mnuBombCount
            // 
            mnuBombCount.Alignment = ToolStripItemAlignment.Right;
            mnuBombCount.Enabled = false;
            mnuBombCount.Name = "mnuBombCount";
            mnuBombCount.Size = new Size(46, 20);
            mnuBombCount.Text = "-1";
            // 
            // mnuRestart
            // 
            mnuRestart.Name = "mnuRestart";
            mnuRestart.Size = new Size(55, 20);
            mnuRestart.Text = "Restart";
            mnuRestart.Click += mnuRestart_Click;
            // 
            // HighScores
            // 
            HighScores.Anchor = AnchorStyles.None;
            HighScores.BackColor = Color.SteelBlue;
            HighScores.Controls.Add(HighScoreLabel);
            HighScores.Controls.Add(HighScoresTitle);
            HighScores.Location = new Point(129, 92);
            HighScores.Margin = new Padding(30);
            HighScores.Name = "HighScores";
            HighScores.Size = new Size(257, 256);
            HighScores.TabIndex = 1;
            HighScores.Visible = false;
            HighScores.Click += HighScores_Click;
            // 
            // HighScoreLabel
            // 
            HighScoreLabel.Font = new Font("Courier New", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            HighScoreLabel.ForeColor = Color.White;
            HighScoreLabel.Location = new Point(14, 71);
            HighScoreLabel.Name = "HighScoreLabel";
            HighScoreLabel.Size = new Size(222, 171);
            HighScoreLabel.TabIndex = 1;
            HighScoreLabel.Text = "HIGH SCORES";
            HighScoreLabel.TextAlign = ContentAlignment.TopCenter;
            // 
            // HighScoresTitle
            // 
            HighScoresTitle.AutoSize = true;
            HighScoresTitle.Font = new Font("Stencil", 24F, FontStyle.Regular, GraphicsUnit.Point, 0);
            HighScoresTitle.ForeColor = Color.Gainsboro;
            HighScoresTitle.Location = new Point(14, 15);
            HighScoresTitle.Name = "HighScoresTitle";
            HighScoresTitle.Size = new Size(222, 38);
            HighScoresTitle.TabIndex = 0;
            HighScoresTitle.Text = "HIGH SCORES";
            // 
            // MineField
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(HighScores);
            Controls.Add(menuStrip);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip;
            Name = "MineField";
            Text = "BoomBoom";
            Activated += BoomForm_Activated;
            Deactivate += BoomForm_Deactivate;
            Load += BoomForm_Load;
            menuStrip.ResumeLayout(false);
            menuStrip.PerformLayout();
            HighScores.ResumeLayout(false);
            HighScores.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip;
        private ToolStripMenuItem mnuGame;
        private ToolStripMenuItem mnuRestart;
        private ToolStripMenuItem mnuTimer;
        private ToolStripMenuItem mnuBombCount;
        private ToolStripMenuItem mnuGameBeginner;
        private ToolStripMenuItem mnuGameIntermediate;
        private ToolStripMenuItem mnuGameExpert;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem mnuSound;
        private ToolStripMenuItem mnuHighScores;
        private Panel HighScores;
        private Label HighScoresTitle;
        private Label HighScoreLabel;
    }
}