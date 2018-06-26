
    partial class MarkPanel
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.taskNameBox = new System.Windows.Forms.TextBox();
            this.taskDescBox = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.InnerPanel = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rbGroup = new System.Windows.Forms.RadioButton();
            this.rbIndividual = new System.Windows.Forms.RadioButton();
            this.InnerPanel.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // taskNameBox
            // 
            this.taskNameBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.taskNameBox.Location = new System.Drawing.Point(105, 18);
            this.taskNameBox.Name = "taskNameBox";
            this.taskNameBox.Size = new System.Drawing.Size(672, 20);
            this.taskNameBox.TabIndex = 0;
            // 
            // taskDescBox
            // 
            this.taskDescBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.taskDescBox.Location = new System.Drawing.Point(105, 52);
            this.taskDescBox.Name = "taskDescBox";
            this.taskDescBox.Size = new System.Drawing.Size(672, 69);
            this.taskDescBox.TabIndex = 1;
            this.taskDescBox.Text = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(27, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Task name";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(28, 55);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Description";
            // 
            // InnerPanel
            // 
            this.InnerPanel.AutoSize = true;
            this.InnerPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.InnerPanel.Controls.Add(this.groupBox1);
            this.InnerPanel.Controls.Add(this.label2);
            this.InnerPanel.Controls.Add(this.label1);
            this.InnerPanel.Controls.Add(this.taskDescBox);
            this.InnerPanel.Controls.Add(this.taskNameBox);
            this.InnerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.InnerPanel.Location = new System.Drawing.Point(0, 0);
            this.InnerPanel.Name = "InnerPanel";
            this.InnerPanel.Size = new System.Drawing.Size(800, 450);
            this.InnerPanel.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rbIndividual);
            this.groupBox1.Controls.Add(this.rbGroup);
            this.groupBox1.Location = new System.Drawing.Point(31, 135);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(131, 80);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Group task?";
            // 
            // rbGroup
            // 
            this.rbGroup.AutoSize = true;
            this.rbGroup.Location = new System.Drawing.Point(7, 20);
            this.rbGroup.Name = "rbGroup";
            this.rbGroup.Size = new System.Drawing.Size(54, 17);
            this.rbGroup.TabIndex = 0;
            this.rbGroup.TabStop = true;
            this.rbGroup.Text = "Group";
            this.rbGroup.UseVisualStyleBackColor = true;
            // 
            // rbIndividual
            // 
            this.rbIndividual.AutoSize = true;
            this.rbIndividual.Location = new System.Drawing.Point(7, 44);
            this.rbIndividual.Name = "rbIndividual";
            this.rbIndividual.Size = new System.Drawing.Size(70, 17);
            this.rbIndividual.TabIndex = 1;
            this.rbIndividual.TabStop = true;
            this.rbIndividual.Text = "Individual";
            this.rbIndividual.UseVisualStyleBackColor = true;
            // 
            // MarkPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.InnerPanel);
            this.Name = "MarkPanel";
            this.Size = new System.Drawing.Size(800, 450);
            this.InnerPanel.ResumeLayout(false);
            this.InnerPanel.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox taskNameBox;
        private System.Windows.Forms.RichTextBox taskDescBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel InnerPanel;
    private System.Windows.Forms.GroupBox groupBox1;
    private System.Windows.Forms.RadioButton rbIndividual;
    private System.Windows.Forms.RadioButton rbGroup;
}
