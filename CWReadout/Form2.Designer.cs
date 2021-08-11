

namespace BRSReadout
{
    partial class Form2
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form2));
            this.imagePlot = new LiveCharts.WinForms.CartesianChart();
            this.anglePlot = new LiveCharts.WinForms.CartesianChart();
            this.SuspendLayout();
            // 
            // imagePlot
            // 
            this.imagePlot.Location = new System.Drawing.Point(12, 12);
            this.imagePlot.Name = "imagePlot";
            this.imagePlot.Size = new System.Drawing.Size(902, 674);
            this.imagePlot.TabIndex = 0;
            this.imagePlot.Text = "image";
            // 
            // anglePlot
            // 
            this.anglePlot.Location = new System.Drawing.Point(920, 57);
            this.anglePlot.Name = "anglePlot";
            this.anglePlot.Size = new System.Drawing.Size(625, 629);
            this.anglePlot.TabIndex = 1;
            this.anglePlot.Text = "anglePlot";
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1541, 698);
            this.Controls.Add(this.anglePlot);
            this.Controls.Add(this.imagePlot);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form2";
            this.Text = "BRS Graph";
            this.ResumeLayout(false);

        }
        #endregion

        public LiveCharts.WinForms.CartesianChart imagePlot;
        public LiveCharts.WinForms.CartesianChart anglePlot;
    }
}