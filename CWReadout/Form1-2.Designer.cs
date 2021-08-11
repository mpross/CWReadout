using System.Configuration;

namespace BRSReadout
{
    partial class Form1
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
        /// 

        /*
         *  Initializes all the visual elements
         */

        private void InitializeComponent()
        {
            string location = ConfigurationManager.AppSettings.Get("location");
            this.components = new System.ComponentModel.Container();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.buRecord = new System.Windows.Forms.Button();
            this.buGraph = new System.Windows.Forms.Button();
            this.buRecap = new System.Windows.Forms.Button();
            this.buDamp = new System.Windows.Forms.Button();
            this.buClear = new System.Windows.Forms.Button();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown2 = new System.Windows.Forms.NumericUpDown();
            this.numericUpDown3 = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown3)).BeginInit();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(10, 29);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 20);
            this.textBox1.TabIndex = 0;

            this.buGraph.Location = new System.Drawing.Point(456, 29);
            this.buGraph.Name = "buGraph";
            this.buGraph.Size = new System.Drawing.Size(100, 23);
            this.buGraph.TabIndex = 2;
            this.buGraph.Text = "Graph";
            this.buGraph.UseVisualStyleBackColor = true;
            this.buGraph.Click += new System.EventHandler(this.buGraph_Click);

            this.buDamp.Location = new System.Drawing.Point(456, 54);
            this.buDamp.Name = "cOver";
            this.buDamp.Size = new System.Drawing.Size(100, 23);
            this.buDamp.TabIndex = 2;
            this.buDamp.Text = "Damping Override";
            this.buDamp.UseVisualStyleBackColor = true;
            this.buDamp.Click += new System.EventHandler(this.buDamp_Click);

            this.buRecap.Location = new System.Drawing.Point(456, 79);
            this.buRecap.Name = "recap";
            this.buRecap.Size = new System.Drawing.Size(100, 23);
            this.buRecap.TabIndex = 2;
            this.buRecap.Text = "Recapture Frame";
            this.buRecap.UseVisualStyleBackColor = true;
            this.buRecap.Click += new System.EventHandler(this.buRecap_Click);

            this.buRecord.Location = new System.Drawing.Point(456, 29);
            this.buRecord.Name = "buRecord";
            this.buRecord.Size = new System.Drawing.Size(100, 23);
            this.buRecord.TabIndex = 2;
            this.buRecord.Text = "Record To Disk";
            this.buRecord.UseVisualStyleBackColor = true;
            this.buRecord.Click += new System.EventHandler(this.buRecord_Click);

            this.buClear.Location = new System.Drawing.Point(356, 29);
            this.buClear.Name = "buClear";
            this.buClear.Size = new System.Drawing.Size(100, 23);
            this.buClear.TabIndex = 2;
            this.buClear.Text = "Clear Graph";
            this.buClear.UseVisualStyleBackColor = true;
            this.buClear.Click += new System.EventHandler(this.buClear_Click);
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(66, 29);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(100, 20);
            this.textBox2.TabIndex = 3;
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(122, 29);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(100, 20);
            this.textBox3.TabIndex = 4;
            // 
            // textBox4
            // 
            this.textBox4.Location = new System.Drawing.Point(178, 29);
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(100, 20);
            this.textBox4.TabIndex = 5;
           
            // numericUpDown1
            // 
            this.numericUpDown1.DecimalPlaces = 3;
            this.numericUpDown1.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numericUpDown1.Location = new System.Drawing.Point(235, 29);
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(100, 20);
            this.numericUpDown1.TabIndex = 7;
            // 
            // numericUpDown2
            // 
            this.numericUpDown2.DecimalPlaces = 3;
            this.numericUpDown2.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numericUpDown2.Location = new System.Drawing.Point(291, 29);
            this.numericUpDown2.Name = "numericUpDown2";
            this.numericUpDown2.Size = new System.Drawing.Size(100, 20);
            this.numericUpDown2.TabIndex = 8;
            this.numericUpDown2.Value = new decimal(new int[] {
            2,
            0,
            0,
            65536});
            // 
            // numericUpDown3
            // 
            this.numericUpDown3.Location = new System.Drawing.Point(347, 29);
            this.numericUpDown3.Name = "numericUpDown3";
            this.numericUpDown3.Size = new System.Drawing.Size(100, 20);
            this.numericUpDown3.TabIndex = 9;
            this.numericUpDown3.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            //this.numericUpDown3.ValueChanged += new System.EventHandler(this.numericUpDown3_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("MingLiU", 8.15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(15, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 11);
            this.label1.TabIndex = 10;
            this.label1.Text = "Time";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("MingLiU", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(100, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 11);
            this.label2.TabIndex = 11;
            this.label2.Text = "Camera Queue";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("MingLiU", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(200, 10);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(75, 11);
            this.label3.TabIndex = 12;
            this.label3.Text = "Graphing Queue";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("MingLiU", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(176, 10);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 11);
            this.label4.TabIndex = 13;
            //this.label4.Text = "Rate";
            this.label4.Text = "Voltage Out";

           
            
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(534, 120);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox4);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.buClear);
            this.Controls.Add(this.buRecord);
            this.Controls.Add(this.buGraph);
            this.Controls.Add(this.buRecap);
            this.Controls.Add(this.buDamp);
            this.Controls.Add(this.textBox1);
            this.MinimumSize = new System.Drawing.Size(550, 150); // Original 550, 350
            this.Name = "Form1";
            this.Text = "BRS " + location;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown3)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.SuspendLayout();
            // 
            // Form1
            this.ClientSize = new System.Drawing.Size(278, 244);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button buRecord;
        private System.Windows.Forms.Button buGraph;
        private System.Windows.Forms.Button buDamp;
        private System.Windows.Forms.Button buRecap;
        private System.Windows.Forms.Button buClear;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.NumericUpDown numericUpDown2;
        private System.Windows.Forms.NumericUpDown numericUpDown3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label19;
    }
         
}