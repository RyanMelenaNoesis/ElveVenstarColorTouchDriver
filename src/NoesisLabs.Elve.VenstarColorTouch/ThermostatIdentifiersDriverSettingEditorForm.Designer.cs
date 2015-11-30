namespace NoesisLabs.Elve.VenstarColorTouch
{
	partial class ThermostatIdentifiersDriverSettingEditorForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.dataGridView1 = new System.Windows.Forms.DataGridView();
			this.macAddress = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.name = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.url = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.SaveButton = new System.Windows.Forms.Button();
			this.DiscoverButton = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
			this.SuspendLayout();
			// 
			// dataGridView1
			// 
			this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.macAddress,
            this.name,
            this.url});
			this.dataGridView1.Location = new System.Drawing.Point(12, 12);
			this.dataGridView1.Name = "dataGridView1";
			this.dataGridView1.Size = new System.Drawing.Size(760, 351);
			this.dataGridView1.TabIndex = 0;
			// 
			// macAddress
			// 
			this.macAddress.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.macAddress.HeaderText = "MAC Address";
			this.macAddress.Name = "macAddress";
			// 
			// name
			// 
			this.name.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.name.FillWeight = 50F;
			this.name.HeaderText = "Name";
			this.name.Name = "name";
			// 
			// url
			// 
			this.url.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.url.HeaderText = "URL";
			this.url.Name = "url";
			// 
			// SaveButton
			// 
			this.SaveButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.SaveButton.Location = new System.Drawing.Point(697, 465);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.Size = new System.Drawing.Size(75, 23);
			this.SaveButton.TabIndex = 1;
			this.SaveButton.Text = "Save";
			this.SaveButton.UseVisualStyleBackColor = true;
			// 
			// DiscoverButton
			// 
			this.DiscoverButton.Location = new System.Drawing.Point(13, 465);
			this.DiscoverButton.Name = "DiscoverButton";
			this.DiscoverButton.Size = new System.Drawing.Size(75, 23);
			this.DiscoverButton.TabIndex = 2;
			this.DiscoverButton.Text = "Discover";
			this.DiscoverButton.UseVisualStyleBackColor = true;
			this.DiscoverButton.Click += new System.EventHandler(this.DiscoverButton_Click);
			// 
			// ThermostatIdentifiersDriverSettingEditorForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(784, 500);
			this.Controls.Add(this.DiscoverButton);
			this.Controls.Add(this.SaveButton);
			this.Controls.Add(this.dataGridView1);
			this.Name = "ThermostatIdentifiersDriverSettingEditorForm";
			this.Text = "ThermostatsDriverSettingEditorForm";
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.DataGridView dataGridView1;
		private System.Windows.Forms.DataGridViewTextBoxColumn macAddress;
		private System.Windows.Forms.DataGridViewTextBoxColumn name;
		private System.Windows.Forms.DataGridViewTextBoxColumn url;
		private System.Windows.Forms.Button SaveButton;
		private System.Windows.Forms.Button DiscoverButton;
	}
}