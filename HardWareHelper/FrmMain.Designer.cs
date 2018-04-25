namespace HardWareHelper
{
    partial class FrmMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
            this.txtLog = new System.Windows.Forms.TextBox();
            this.axCpuCardOCX1 = new AxCPUCARDOCXLib.AxCpuCardOCX();
            ((System.ComponentModel.ISupportInitialize)(this.axCpuCardOCX1)).BeginInit();
            this.SuspendLayout();
            // 
            // txtLog
            // 
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Location = new System.Drawing.Point(0, 0);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.Size = new System.Drawing.Size(778, 544);
            this.txtLog.TabIndex = 0;
            // 
            // axCpuCardOCX1
            // 
            this.axCpuCardOCX1.Enabled = true;
            this.axCpuCardOCX1.Location = new System.Drawing.Point(431, 216);
            this.axCpuCardOCX1.Name = "axCpuCardOCX1";
            this.axCpuCardOCX1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axCpuCardOCX1.OcxState")));
            this.axCpuCardOCX1.Size = new System.Drawing.Size(100, 50);
            this.axCpuCardOCX1.TabIndex = 1;
            this.axCpuCardOCX1.Visible = false;
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(778, 544);
            this.Controls.Add(this.axCpuCardOCX1);
            this.Controls.Add(this.txtLog);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FrmMain";
            this.Text = "硬件助手";
            this.Load += new System.EventHandler(this.FrmMain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.axCpuCardOCX1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtLog;
        private AxCPUCARDOCXLib.AxCpuCardOCX axCpuCardOCX1;
    }
}