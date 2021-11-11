namespace Milestone
{
    partial class OptionsDlg
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
            this.label1 = new System.Windows.Forms.Label();
            this.udPoints = new System.Windows.Forms.NumericUpDown();
            this.cbSounds = new System.Windows.Forms.CheckBox();
            this.cbAlwaysStart = new System.Windows.Forms.CheckBox();
            this.OKBtn = new System.Windows.Forms.Button();
            this.CancelBtn = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.udPoints)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(145, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "&Points Needed to Win Game:";
            // 
            // udPoints
            // 
            this.udPoints.Increment = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.udPoints.Location = new System.Drawing.Point(163, 12);
            this.udPoints.Maximum = new decimal(new int[] {
            50000,
            0,
            0,
            0});
            this.udPoints.Minimum = new decimal(new int[] {
            2500,
            0,
            0,
            0});
            this.udPoints.Name = "udPoints";
            this.udPoints.Size = new System.Drawing.Size(70, 20);
            this.udPoints.TabIndex = 1;
            this.udPoints.Value = new decimal(new int[] {
            2500,
            0,
            0,
            0});
            this.udPoints.ValueChanged += new System.EventHandler(this.udPoints_ValueChanged);
            // 
            // cbSounds
            // 
            this.cbSounds.AutoSize = true;
            this.cbSounds.Checked = true;
            this.cbSounds.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbSounds.Location = new System.Drawing.Point(15, 40);
            this.cbSounds.Name = "cbSounds";
            this.cbSounds.Size = new System.Drawing.Size(159, 17);
            this.cbSounds.TabIndex = 2;
            this.cbSounds.Text = "&Incidental Game Sounds On";
            this.cbSounds.UseVisualStyleBackColor = true;
            this.cbSounds.CheckedChanged += new System.EventHandler(this.cbSounds_CheckedChanged);
            // 
            // cbAlwaysStart
            // 
            this.cbAlwaysStart.AutoSize = true;
            this.cbAlwaysStart.Location = new System.Drawing.Point(15, 63);
            this.cbAlwaysStart.Name = "cbAlwaysStart";
            this.cbAlwaysStart.Size = new System.Drawing.Size(156, 17);
            this.cbAlwaysStart.TabIndex = 3;
            this.cbAlwaysStart.Text = "Player &Always Starts Round";
            this.cbAlwaysStart.UseVisualStyleBackColor = true;
            this.cbAlwaysStart.CheckedChanged += new System.EventHandler(this.cbAlwaysStart_CheckedChanged);
            // 
            // OKBtn
            // 
            this.OKBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKBtn.Location = new System.Drawing.Point(15, 90);
            this.OKBtn.Name = "OKBtn";
            this.OKBtn.Size = new System.Drawing.Size(75, 23);
            this.OKBtn.TabIndex = 4;
            this.OKBtn.Text = "&OK";
            this.OKBtn.UseVisualStyleBackColor = true;
            // 
            // CancelBtn
            // 
            this.CancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelBtn.Location = new System.Drawing.Point(96, 90);
            this.CancelBtn.Name = "CancelBtn";
            this.CancelBtn.Size = new System.Drawing.Size(75, 23);
            this.CancelBtn.TabIndex = 5;
            this.CancelBtn.Text = "&Cancel";
            this.CancelBtn.UseVisualStyleBackColor = true;
            // 
            // OptionsDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(247, 125);
            this.Controls.Add(this.CancelBtn);
            this.Controls.Add(this.OKBtn);
            this.Controls.Add(this.cbAlwaysStart);
            this.Controls.Add(this.cbSounds);
            this.Controls.Add(this.udPoints);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OptionsDlg";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Options";
            this.Load += new System.EventHandler(this.OptionsDlg_Load);
            ((System.ComponentModel.ISupportInitialize)(this.udPoints)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown udPoints;
        private System.Windows.Forms.CheckBox cbSounds;
        private System.Windows.Forms.CheckBox cbAlwaysStart;
        private System.Windows.Forms.Button OKBtn;
        private System.Windows.Forms.Button CancelBtn;
    }
}