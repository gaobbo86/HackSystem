﻿using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

using HackSystem.LogonTemplate;

namespace Leon.DefaultLogon
{
    public partial class DefaultLogonForm : Form
    {
        public LogonTemplateClass ParentLogon = null;
        TextBox InnerTextBox = new TextBox() { MaxLength = 12, Visible = true, BorderStyle= BorderStyle.None, Size= Size.Empty };
        bool AllowToClose = false;

        public DefaultLogonForm()
        {
            this.InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            this.LogonPanel.Controls.Add(this.InnerTextBox);
            this.InnerTextBox.Location = this.PasswordInputBox.Location;
            this.FormClosing += new FormClosingEventHandler(
                (Leon, Mathilda) => {
                    if (!this.AllowToClose)
                        Mathilda.Cancel = true;
                    else
                        this.ParentLogon?.OnLogonFinished(EventArgs.Empty);
                });
            this.HeadPortraitPictureBox.BackgroundImage = new Bitmap(LogonTemplateClass.CircularHeadPortrait, 159, 159);

            this.LogonButton.MouseEnter += delegate { this.LogonButton.Image = DefaultLogonResource.LogonButton_2; };
            this.LogonButton.MouseDown += delegate { this.LogonButton.Image = DefaultLogonResource.LogonButton_3; };
            this.LogonButton.MouseUp += delegate { this.LogonButton.Image = DefaultLogonResource.LogonButton_2; };
            this.LogonButton.MouseLeave += delegate { this.LogonButton.Image = DefaultLogonResource.LogonButton_1; };

            this.PasswordInputBox.Click += delegate { this.InnerTextBox.Focus(); };
            this.PasswordInputBox.MouseEnter += delegate { this.PasswordInputBox.Image = DefaultLogonResource.PasswordInputBox_Enter; };
            this.PasswordInputBox.MouseDown += delegate { this.PasswordInputBox.Image = DefaultLogonResource.PasswordInputBox_Down; };
            this.PasswordInputBox.MouseUp += delegate { this.PasswordInputBox.Image = DefaultLogonResource.PasswordInputBox_Enter; };
            this.PasswordInputBox.MouseLeave += delegate { this.PasswordInputBox.Image = DefaultLogonResource.PasswordInputBox_Normal; };

            this.InnerTextBox.TextChanged += delegate { this.PasswordInputBox.Text = string.Empty.PadRight(this.InnerTextBox.Text.Length, '♋'); };
            this.InnerTextBox.KeyDown += new KeyEventHandler((s, e) => { if (e.KeyCode == Keys.Enter) { this.CheckPassword(); } });
        }

        private void LogonButton_Click(object sender, EventArgs e)
        {
            this.CheckPassword();
        }

        private void CheckPassword()
        {
            if (this.InnerTextBox.Text != DefaultLogonClass.Password)
            {
                this.AllowToClose = false;
                //TODO: 等系统提供了弹窗或浮窗的API，使用系统弹出错误提示
                MessageBox.Show("密码不正确！");
                this.InnerTextBox.Focus();
            }
            else
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(
                    (ILoveU) => {
                        try
                        {
                            while (this.Opacity > 0)
                            {
                                Thread.Sleep(100);
                                this.Opacity -= 0.1;
                            }
                        }
                        catch { }
                        this.AllowToClose = true;
                        this.Close();
                    }));
            }
        }

    }

    class PanelEx : Panel
    {
        public PanelEx()
        {
            this.SetStyle(ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor,
                true);
        }
    }

}
