using System;

namespace Kbg.NppPluginNET
{
    partial class FrmSNDlg
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
            this.SNListBox = new System.Windows.Forms.ListBox();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripRefresh = new System.Windows.Forms.ToolStripButton();
            this.toolStripToTop = new System.Windows.Forms.ToolStripButton();
            this.toolStripESections = new System.Windows.Forms.ToolStripButton();
            this.toolStripRSections = new System.Windows.Forms.ToolStripButton();
            this.toolStripUSections = new System.Windows.Forms.ToolStripButton();
            this.toolStripToBottom = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // SNListBox
            // 
            this.SNListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SNListBox.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SNListBox.FormattingEnabled = true;
            this.SNListBox.Location = new System.Drawing.Point(0, 25);
            this.SNListBox.Name = "SNListBox";
            this.SNListBox.Size = new System.Drawing.Size(284, 456);
            this.SNListBox.TabIndex = 0;
            this.SNListBox.DoubleClick += new System.EventHandler(this.SNListBox_DoubleClick);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripRefresh,
            this.toolStripSeparator1,
            this.toolStripToTop,
            this.toolStripESections,
            this.toolStripRSections,
            this.toolStripUSections,
            this.toolStripToBottom});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(284, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripRefresh
            // 
            this.toolStripRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripRefresh.Image = global::COBOLInsights.Properties.Resources.refresh_icon;
            this.toolStripRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripRefresh.Name = "toolStripRefresh";
            this.toolStripRefresh.Size = new System.Drawing.Size(23, 22);
            this.toolStripRefresh.Text = "Refresh list";
            this.toolStripRefresh.Click += new System.EventHandler(this.toolStripButton1_Click);
            // 
            // toolStripToTop
            // 
            this.toolStripToTop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripToTop.Image = global::COBOLInsights.Properties.Resources.icon_top;
            this.toolStripToTop.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripToTop.Name = "toolStripToTop";
            this.toolStripToTop.Size = new System.Drawing.Size(23, 22);
            this.toolStripToTop.Text = "To Top";
            this.toolStripToTop.Click += new System.EventHandler(this.toolStripToTop_Click);
            // 
            // toolStripESections
            // 
            this.toolStripESections.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripESections.Image = global::COBOLInsights.Properties.Resources.icon_e_section;
            this.toolStripESections.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripESections.Name = "toolStripESections";
            this.toolStripESections.Size = new System.Drawing.Size(23, 22);
            this.toolStripESections.Text = "E-Sections";
            this.toolStripESections.Click += new System.EventHandler(this.toolStripESections_Click);
            // 
            // toolStripRSections
            // 
            this.toolStripRSections.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripRSections.Image = global::COBOLInsights.Properties.Resources.icon_r_section;
            this.toolStripRSections.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripRSections.Name = "toolStripRSections";
            this.toolStripRSections.Size = new System.Drawing.Size(23, 22);
            this.toolStripRSections.Text = "R-Sections";
            this.toolStripRSections.Click += new System.EventHandler(this.toolStripRSections_Click);
            // 
            // toolStripUSections
            // 
            this.toolStripUSections.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripUSections.Image = global::COBOLInsights.Properties.Resources.icon_u_section;
            this.toolStripUSections.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripUSections.Name = "toolStripUSections";
            this.toolStripUSections.Size = new System.Drawing.Size(23, 22);
            this.toolStripUSections.Text = "U-Sections";
            this.toolStripUSections.Click += new System.EventHandler(this.toolStripUSections_Click);
            // 
            // toolStripToBottom
            // 
            this.toolStripToBottom.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripToBottom.Image = global::COBOLInsights.Properties.Resources.icon_bottom;
            this.toolStripToBottom.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripToBottom.Name = "toolStripToBottom";
            this.toolStripToBottom.Size = new System.Drawing.Size(23, 22);
            this.toolStripToBottom.Text = "To Bottom";
            this.toolStripToBottom.Click += new System.EventHandler(this.toolStripToBottom_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // FrmSNDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 481);
            this.Controls.Add(this.SNListBox);
            this.Controls.Add(this.toolStrip1);
            this.Name = "FrmSNDlg";
            this.Text = "Source Navigation";
            this.Shown += new System.EventHandler(this.FrmSNDlg_Shown);
            this.VisibleChanged += new System.EventHandler(this.frmSNDlg_VisibleChanged);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox SNListBox;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripRefresh;
        private System.Windows.Forms.ToolStripButton toolStripToTop;
        private System.Windows.Forms.ToolStripButton toolStripESections;
        private System.Windows.Forms.ToolStripButton toolStripRSections;
        private System.Windows.Forms.ToolStripButton toolStripUSections;
        private System.Windows.Forms.ToolStripButton toolStripToBottom;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
    }
}