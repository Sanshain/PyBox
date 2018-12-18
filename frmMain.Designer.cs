using Neo_RTB = MeFastTextBox.NeoRTB;
//using Neo_RTB = sSystem.Windows.Forms.RichTextBox;

namespace MeFastTextBox
{
    partial class MainTestForm
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainTestForm));
            this.button1 = new System.Windows.Forms.Button();
            this.listBox1 = new System.Windows.Forms.TreeView();
            this.status = new System.Windows.Forms.Label();
            this.FatBox = new MeFastTextBox.NeoRTB();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(587, 409);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(156, 54);
            this.button1.TabIndex = 2;
            this.button1.TabStop = false;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // listBox1
            // 
            this.listBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBox1.Location = new System.Drawing.Point(587, 15);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(154, 388);
            this.listBox1.TabIndex = 3;
            this.listBox1.DoubleClick += new System.EventHandler(this.listBox1_DoubleClick);
            // 
            // status
            // 
            this.status.AutoSize = true;
            this.status.Location = new System.Drawing.Point(-2, 466);
            this.status.Name = "status";
            this.status.Size = new System.Drawing.Size(0, 13);
            this.status.TabIndex = 4;
            // 
            // FatBox
            // 
            this.FatBox.AcceptsTab = true;
            this.FatBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.FatBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FatBox.GloBlocks = ((System.Collections.Generic.List<string>)(resources.GetObject("FatBox.GloBlocks")));            
            this.FatBox.Location = new System.Drawing.Point(1, 12);
            this.FatBox.Name = "FatBox";
            this.FatBox.Size = new System.Drawing.Size(580, 451);
            this.FatBox.TabIndex = 0;
            this.FatBox.Text = "class Astra:\n\ta=6\n\tdef D():";
            this.FatBox.TextChanged += new System.EventHandler(this.richTextBox1_TextChanged);
            this.FatBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FatBox_KeyDown);
            this.FatBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.FatBox_KeyPress);
            // 
            // MainTestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(759, 492);
            this.Controls.Add(this.status);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.FatBox);
            this.Name = "MainTestForm";
            this.Load += new System.EventHandler(this.MainTestForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Neo_RTB FatBox;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TreeView listBox1;
        private System.Windows.Forms.Label status;
    }
}

