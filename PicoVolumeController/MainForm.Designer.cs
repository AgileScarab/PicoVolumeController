namespace PicoVolumeController
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            startButton = new Button();
            trayIcon = new NotifyIcon(components);
            comLabel = new Label();
            comTextBox = new TextBox();
            foregroundCheckBox = new CheckBox();
            processGroup1Box = new ListBox();
            processGroup2Box = new ListBox();
            SuspendLayout();
            // 
            // startButton
            // 
            startButton.Location = new Point(115, 200);
            startButton.Name = "startButton";
            startButton.Size = new Size(99, 27);
            startButton.TabIndex = 2;
            startButton.Text = "Start";
            startButton.UseVisualStyleBackColor = true;
            startButton.Click += startButton_Click;
            // 
            // trayIcon
            // 
            trayIcon.Icon = (Icon)resources.GetObject("trayIcon.Icon");
            trayIcon.Text = "PVController";
            trayIcon.MouseDoubleClick += trayIcon_MouseDoubleClick;
            // 
            // comLabel
            // 
            comLabel.AutoSize = true;
            comLabel.Location = new Point(12, 206);
            comLabel.Name = "comLabel";
            comLabel.Size = new Size(38, 15);
            comLabel.TabIndex = 3;
            comLabel.Text = "COM:";
            // 
            // comTextBox
            // 
            comTextBox.Location = new Point(56, 203);
            comTextBox.Name = "comTextBox";
            comTextBox.Size = new Size(47, 23);
            comTextBox.TabIndex = 4;
            // 
            // foregroundCheckBox
            // 
            foregroundCheckBox.AutoSize = true;
            foregroundCheckBox.Location = new Point(12, 179);
            foregroundCheckBox.Name = "foregroundCheckBox";
            foregroundCheckBox.Size = new Size(202, 19);
            foregroundCheckBox.TabIndex = 5;
            foregroundCheckBox.Text = "Encoder 2  = Foreground window";
            foregroundCheckBox.UseVisualStyleBackColor = true;
            // 
            // processGroup1Box
            // 
            processGroup1Box.FormattingEnabled = true;
            processGroup1Box.ItemHeight = 15;
            processGroup1Box.Location = new Point(12, 12);
            processGroup1Box.Name = "processGroup1Box";
            processGroup1Box.SelectionMode = SelectionMode.None;
            processGroup1Box.Size = new Size(207, 64);
            processGroup1Box.TabIndex = 6;
            // 
            // processGroup2Box
            // 
            processGroup2Box.FormattingEnabled = true;
            processGroup2Box.ItemHeight = 15;
            processGroup2Box.Location = new Point(12, 98);
            processGroup2Box.Name = "processGroup2Box";
            processGroup2Box.SelectionMode = SelectionMode.None;
            processGroup2Box.Size = new Size(207, 64);
            processGroup2Box.TabIndex = 7;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(235, 237);
            Controls.Add(processGroup2Box);
            Controls.Add(processGroup1Box);
            Controls.Add(foregroundCheckBox);
            Controls.Add(comTextBox);
            Controls.Add(comLabel);
            Controls.Add(startButton);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "MainForm";
            Text = "PicoVolumeController";
            FormClosed += MainForm_FormClosed;
            Load += Form1_Load;
            Resize += MainForm_Resize;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button startButton;
        private NotifyIcon trayIcon;
        private Label comLabel;
        private TextBox comTextBox;
        private CheckBox foregroundCheckBox;
        private ListBox processGroup1Box;
        private ListBox processGroup2Box;
    }
}