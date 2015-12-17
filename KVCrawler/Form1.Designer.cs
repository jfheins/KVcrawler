namespace KVCrawler
{
    partial class Form1
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			this.start_btn = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.listBox1 = new System.Windows.Forms.ListBox();
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.label4 = new System.Windows.Forms.Label();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.stopp_btn = new System.Windows.Forms.Button();
			this.save_btn = new System.Windows.Forms.Button();
			this.threads_num = new System.Windows.Forms.NumericUpDown();
			this.label2 = new System.Windows.Forms.Label();
			this.threads_lbl = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.threads_num)).BeginInit();
			this.SuspendLayout();
			// 
			// start_btn
			// 
			this.start_btn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.start_btn.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.start_btn.Location = new System.Drawing.Point(464, 9);
			this.start_btn.Name = "start_btn";
			this.start_btn.Size = new System.Drawing.Size(73, 29);
			this.start_btn.TabIndex = 0;
			this.start_btn.Text = "Los";
			this.start_btn.UseVisualStyleBackColor = true;
			this.start_btn.Click += new System.EventHandler(this.start_btn_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(136, 29);
			this.label1.TabIndex = 1;
			this.label1.Text = "KV-Crawler";
			// 
			// listBox1
			// 
			this.listBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.listBox1.FormattingEnabled = true;
			this.listBox1.IntegralHeight = false;
			this.listBox1.Location = new System.Drawing.Point(214, 80);
			this.listBox1.Name = "listBox1";
			this.listBox1.Size = new System.Drawing.Size(323, 407);
			this.listBox1.TabIndex = 4;
			// 
			// progressBar1
			// 
			this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.progressBar1.Location = new System.Drawing.Point(12, 493);
			this.progressBar1.MarqueeAnimationSpeed = 20;
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.Size = new System.Drawing.Size(525, 17);
			this.progressBar1.Step = 1;
			this.progressBar1.TabIndex = 8;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(14, 64);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(75, 13);
			this.label4.TabIndex = 9;
			this.label4.Text = "Postleitzahlen:";
			// 
			// textBox1
			// 
			this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.textBox1.Location = new System.Drawing.Point(12, 80);
			this.textBox1.Multiline = true;
			this.textBox1.Name = "textBox1";
			this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textBox1.Size = new System.Drawing.Size(196, 407);
			this.textBox1.TabIndex = 10;
			this.textBox1.Text = resources.GetString("textBox1.Text");
			this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
			// 
			// stopp_btn
			// 
			this.stopp_btn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.stopp_btn.Enabled = false;
			this.stopp_btn.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.stopp_btn.Location = new System.Drawing.Point(464, 44);
			this.stopp_btn.Name = "stopp_btn";
			this.stopp_btn.Size = new System.Drawing.Size(73, 29);
			this.stopp_btn.TabIndex = 11;
			this.stopp_btn.Text = "Stopp";
			this.stopp_btn.UseVisualStyleBackColor = true;
			this.stopp_btn.Click += new System.EventHandler(this.stopp_btn_Click);
			// 
			// save_btn
			// 
			this.save_btn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.save_btn.Enabled = false;
			this.save_btn.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.save_btn.Location = new System.Drawing.Point(443, 516);
			this.save_btn.Name = "save_btn";
			this.save_btn.Size = new System.Drawing.Size(94, 29);
			this.save_btn.TabIndex = 12;
			this.save_btn.Text = "Speichern";
			this.save_btn.UseVisualStyleBackColor = true;
			this.save_btn.Click += new System.EventHandler(this.save_btn_Click);
			// 
			// threads_num
			// 
			this.threads_num.Location = new System.Drawing.Point(72, 41);
			this.threads_num.Maximum = new decimal(new int[] {
            20,
            0,
            0,
            0});
			this.threads_num.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.threads_num.Name = "threads_num";
			this.threads_num.Size = new System.Drawing.Size(76, 20);
			this.threads_num.TabIndex = 14;
			this.threads_num.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(14, 43);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(49, 13);
			this.label2.TabIndex = 15;
			this.label2.Text = "Threads:";
			// 
			// threads_lbl
			// 
			this.threads_lbl.AutoSize = true;
			this.threads_lbl.Location = new System.Drawing.Point(154, 43);
			this.threads_lbl.Name = "threads_lbl";
			this.threads_lbl.Size = new System.Drawing.Size(0, 13);
			this.threads_lbl.TabIndex = 16;
			// 
			// label3
			// 
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label3.Location = new System.Drawing.Point(3, 542);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(158, 12);
			this.label3.TabIndex = 17;
			this.label3.Text = "https://github.com/jfheins/KVcrawler";
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(549, 557);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.threads_lbl);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.threads_num);
			this.Controls.Add(this.save_btn);
			this.Controls.Add(this.stopp_btn);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.progressBar1);
			this.Controls.Add(this.listBox1);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.start_btn);
			this.Name = "Form1";
			this.Text = "KV-Crawler (Version 1.1)";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
			((System.ComponentModel.ISupportInitialize)(this.threads_num)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button start_btn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button stopp_btn;
        private System.Windows.Forms.Button save_btn;
        private System.Windows.Forms.NumericUpDown threads_num;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label threads_lbl;
		  private System.Windows.Forms.Label label3;
    }
}

