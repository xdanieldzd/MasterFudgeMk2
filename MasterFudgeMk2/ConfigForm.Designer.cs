namespace MasterFudgeMk2
{
    partial class ConfigForm
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
            this.btnOkay = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.tlpInputConfig = new System.Windows.Forms.TableLayoutPanel();
            this.tcConfig = new System.Windows.Forms.TabControl();
            this.tpMainConfig = new System.Windows.Forms.TabPage();
            this.tpInputConfig = new System.Windows.Forms.TabPage();
            this.tlpMainConfig = new System.Windows.Forms.TableLayoutPanel();
            this.tcConfig.SuspendLayout();
            this.tpMainConfig.SuspendLayout();
            this.tpInputConfig.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnOkay
            // 
            this.btnOkay.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOkay.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOkay.Location = new System.Drawing.Point(210, 325);
            this.btnOkay.Name = "btnOkay";
            this.btnOkay.Size = new System.Drawing.Size(88, 25);
            this.btnOkay.TabIndex = 2;
            this.btnOkay.Text = "&OK";
            this.btnOkay.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(304, 325);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 25);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // tlpInputConfig
            // 
            this.tlpInputConfig.AutoScroll = true;
            this.tlpInputConfig.ColumnCount = 3;
            this.tlpInputConfig.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpInputConfig.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpInputConfig.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpInputConfig.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpInputConfig.Location = new System.Drawing.Point(3, 3);
            this.tlpInputConfig.Name = "tlpInputConfig";
            this.tlpInputConfig.RowCount = 1;
            this.tlpInputConfig.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpInputConfig.Size = new System.Drawing.Size(366, 275);
            this.tlpInputConfig.TabIndex = 4;
            // 
            // tcConfig
            // 
            this.tcConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tcConfig.Controls.Add(this.tpMainConfig);
            this.tcConfig.Controls.Add(this.tpInputConfig);
            this.tcConfig.Location = new System.Drawing.Point(12, 12);
            this.tcConfig.Name = "tcConfig";
            this.tcConfig.SelectedIndex = 0;
            this.tcConfig.Size = new System.Drawing.Size(380, 307);
            this.tcConfig.TabIndex = 5;
            // 
            // tpMainConfig
            // 
            this.tpMainConfig.Controls.Add(this.tlpMainConfig);
            this.tpMainConfig.Location = new System.Drawing.Point(4, 22);
            this.tpMainConfig.Name = "tpMainConfig";
            this.tpMainConfig.Padding = new System.Windows.Forms.Padding(3);
            this.tpMainConfig.Size = new System.Drawing.Size(372, 281);
            this.tpMainConfig.TabIndex = 1;
            this.tpMainConfig.Text = "Main";
            this.tpMainConfig.UseVisualStyleBackColor = true;
            // 
            // tpInputConfig
            // 
            this.tpInputConfig.Controls.Add(this.tlpInputConfig);
            this.tpInputConfig.Location = new System.Drawing.Point(4, 22);
            this.tpInputConfig.Name = "tpInputConfig";
            this.tpInputConfig.Padding = new System.Windows.Forms.Padding(3);
            this.tpInputConfig.Size = new System.Drawing.Size(372, 281);
            this.tpInputConfig.TabIndex = 0;
            this.tpInputConfig.Text = "Input";
            this.tpInputConfig.UseVisualStyleBackColor = true;
            // 
            // tlpMainConfig
            // 
            this.tlpMainConfig.AutoScroll = true;
            this.tlpMainConfig.ColumnCount = 3;
            this.tlpMainConfig.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMainConfig.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpMainConfig.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlpMainConfig.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMainConfig.Location = new System.Drawing.Point(3, 3);
            this.tlpMainConfig.Name = "tlpMainConfig";
            this.tlpMainConfig.RowCount = 1;
            this.tlpMainConfig.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlpMainConfig.Size = new System.Drawing.Size(366, 275);
            this.tlpMainConfig.TabIndex = 5;
            // 
            // ConfigForm
            // 
            this.AcceptButton = this.btnOkay;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(404, 362);
            this.Controls.Add(this.tcConfig);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOkay);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConfigForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ConfigForm_FormClosing);
            this.tcConfig.ResumeLayout(false);
            this.tpMainConfig.ResumeLayout(false);
            this.tpInputConfig.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnOkay;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TableLayoutPanel tlpInputConfig;
        private System.Windows.Forms.TabControl tcConfig;
        private System.Windows.Forms.TabPage tpInputConfig;
        private System.Windows.Forms.TabPage tpMainConfig;
        private System.Windows.Forms.TableLayoutPanel tlpMainConfig;
    }
}