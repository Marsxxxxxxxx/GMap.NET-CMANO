using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Demo.WindowsForms.Forms;

public class Message : Form
{
	private IContainer components;

	private Button button1;

	private Button button2;

	internal RichTextBox richTextBox1;

	public Message()
	{
		InitializeComponent();
	}

	private void button2_Click(object sender, EventArgs e)
	{
		Close();
	}

	private void button1_Click(object sender, EventArgs e)
	{
		Close();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Demo.WindowsForms.Forms.Message));
		this.button1 = new System.Windows.Forms.Button();
		this.button2 = new System.Windows.Forms.Button();
		this.richTextBox1 = new System.Windows.Forms.RichTextBox();
		base.SuspendLayout();
		this.button1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
		this.button1.DialogResult = System.Windows.Forms.DialogResult.Yes;
		this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.button1.Location = new System.Drawing.Point(346, 414);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(268, 67);
		this.button1.TabIndex = 1;
		this.button1.Text = "YES, I accept full responsability for using this software";
		this.button1.UseVisualStyleBackColor = true;
		this.button1.Click += new System.EventHandler(this.button1_Click);
		this.button2.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
		this.button2.DialogResult = System.Windows.Forms.DialogResult.No;
		this.button2.Location = new System.Drawing.Point(8, 414);
		this.button2.Name = "button2";
		this.button2.Size = new System.Drawing.Size(111, 67);
		this.button2.TabIndex = 2;
		this.button2.Text = "NO, thanks";
		this.button2.UseVisualStyleBackColor = true;
		this.button2.Click += new System.EventHandler(this.button2_Click);
		this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Top;
		this.richTextBox1.Location = new System.Drawing.Point(5, 5);
		this.richTextBox1.Name = "richTextBox1";
		this.richTextBox1.ReadOnly = true;
		this.richTextBox1.Size = new System.Drawing.Size(609, 394);
		this.richTextBox1.TabIndex = 3;
		this.richTextBox1.Text = resources.GetString("richTextBox1.Text");
		base.AutoScaleDimensions = new System.Drawing.SizeF(8f, 16f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.Gray;
		base.ClientSize = new System.Drawing.Size(619, 489);
		base.Controls.Add(this.richTextBox1);
		base.Controls.Add(this.button2);
		base.Controls.Add(this.button1);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "Message";
		base.Padding = new System.Windows.Forms.Padding(5);
		base.ShowIcon = false;
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "<< WARNING >>";
		base.ResumeLayout(false);
	}
}
