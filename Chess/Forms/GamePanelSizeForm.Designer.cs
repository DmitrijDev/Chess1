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
            this.TextBox = new System.Windows.Forms.TextBox();
            this.Label = new System.Windows.Forms.Label();
            this.SelectButton = new System.Windows.Forms.Button();
            this.CancelButton = new System.Windows.Forms.Button();
            this.PlusButton = new System.Windows.Forms.Button();
            this.MinusButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // TextBox
            // 
            this.TextBox.Location = new System.Drawing.Point(96, 51);
            this.TextBox.Name = "TextBox";
            this.TextBox.Size = new System.Drawing.Size(48, 23);
            this.TextBox.TabIndex = 0;
            this.TextBox.Leave += new System.EventHandler(this.TextBox_Leave);
            // 
            // Label
            // 
            this.Label.AutoSize = true;
            this.Label.Location = new System.Drawing.Point(60, 22);
            this.Label.Name = "Label";
            this.Label.Size = new System.Drawing.Size(130, 15);
            this.Label.TabIndex = 1;
            this.Label.Text = "Выбрать размер поля:";
            // 
            // SelectButton
            // 
            this.SelectButton.Location = new System.Drawing.Point(34, 137);
            this.SelectButton.Name = "SelectButton";
            this.SelectButton.Size = new System.Drawing.Size(74, 23);
            this.SelectButton.TabIndex = 2;
            this.SelectButton.Text = "Выбрать";
            this.SelectButton.UseVisualStyleBackColor = true;
            this.SelectButton.Click += new System.EventHandler(this.SelectButton_Click);
            // 
            // CancelButton
            // 
            this.CancelButton.Location = new System.Drawing.Point(131, 137);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(75, 23);
            this.CancelButton.TabIndex = 3;
            this.CancelButton.Text = "Отмена";
            this.CancelButton.UseVisualStyleBackColor = true;
            // 
            // PlusButton
            // 
            this.PlusButton.Location = new System.Drawing.Point(34, 51);
            this.PlusButton.Name = "PlusButton";
            this.PlusButton.Size = new System.Drawing.Size(29, 23);
            this.PlusButton.TabIndex = 4;
            this.PlusButton.Text = "+";
            this.PlusButton.UseVisualStyleBackColor = true;
            this.PlusButton.Click += new System.EventHandler(this.PlusButton_Click);
            // 
            // MinusButton
            // 
            this.MinusButton.Location = new System.Drawing.Point(177, 50);
            this.MinusButton.Name = "MinusButton";
            this.MinusButton.Size = new System.Drawing.Size(29, 23);
            this.MinusButton.TabIndex = 5;
            this.MinusButton.Text = "-";
            this.MinusButton.UseVisualStyleBackColor = true;
            this.MinusButton.Click += new System.EventHandler(this.MinusButton_Click);
            // 
            // GamePanelSizeForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(238, 185);
            this.Controls.Add(this.MinusButton);
            this.Controls.Add(this.PlusButton);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.SelectButton);
            this.Controls.Add(this.Label);
            this.Controls.Add(this.TextBox);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GamePanelSizeForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TextBox TextBox;
        private Label Label;
        private Button SelectButton;
        private Button CancelButton;
        private Button PlusButton;
        private Button MinusButton;
    }
}