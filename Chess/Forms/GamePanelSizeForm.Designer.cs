namespace Chess
{
    partial class GamePanelSizeForm
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
            this.textBox = new System.Windows.Forms.TextBox();
            this.label = new System.Windows.Forms.Label();
            this.selectButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.plusButton = new System.Windows.Forms.Button();
            this.minusButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBox
            // 
            this.textBox.Location = new System.Drawing.Point(96, 51);
            this.textBox.Name = "textBox";
            this.textBox.Size = new System.Drawing.Size(48, 23);
            this.textBox.TabIndex = 0;
            this.textBox.TextChanged += (sender, e) => { };//new System.EventHandler(this.TextBox_TextChanged);
            // 
            // label
            // 
            this.label.AutoSize = true;
            this.label.Location = new System.Drawing.Point(60, 22);
            this.label.Name = "label";
            this.label.Size = new System.Drawing.Size(130, 15);
            this.label.TabIndex = 1;
            this.label.Text = "Выбрать размер поля:";
            // 
            // selectButton
            // 
            this.selectButton.Location = new System.Drawing.Point(24, 137);
            this.selectButton.Name = "selectButton";
            this.selectButton.Size = new System.Drawing.Size(74, 23);
            this.selectButton.TabIndex = 2;
            this.selectButton.Text = "Выбрать";
            this.selectButton.UseVisualStyleBackColor = true;
            this.selectButton.Click += new System.EventHandler(this.SelectButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(143, 137);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Отмена";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += (sender, e) => Close();
            // 
            // plusButton
            // 
            this.plusButton.Location = new System.Drawing.Point(24, 50);
            this.plusButton.Name = "plusButton";
            this.plusButton.Size = new System.Drawing.Size(29, 23);
            this.plusButton.TabIndex = 4;
            this.plusButton.Text = "+";
            this.plusButton.UseVisualStyleBackColor = true;
            // 
            // minusButton
            // 
            this.minusButton.Location = new System.Drawing.Point(189, 51);
            this.minusButton.Name = "minusButton";
            this.minusButton.Size = new System.Drawing.Size(29, 23);
            this.minusButton.TabIndex = 5;
            this.minusButton.Text = "-";
            this.minusButton.UseVisualStyleBackColor = true;
            // 
            // GamePanelSizeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(240, 185);
            this.Controls.Add(this.minusButton);
            this.Controls.Add(this.plusButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.selectButton);
            this.Controls.Add(this.label);
            this.Controls.Add(this.textBox);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GamePanelSizeForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TextBox textBox;
        private Label label;
        private Button selectButton;
        private Button cancelButton;
        private Button plusButton;
        private Button minusButton;
    }
}