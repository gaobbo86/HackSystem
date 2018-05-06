﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using StartUpTemplate;

namespace HackSystem
{
    static class Program
    {
        private volatile static StartUpTemplateClass _startUp = null;

        /// <summary>
        /// 系统的启动画面
        /// </summary>
        public static StartUpTemplateClass StartUp
        {
            get => _startUp;
            set
            {
                _startUp = value;
                UnityModule.DebugPrint("系统当前 StartUp : {0} ({1})", value.Name, value.Description);
            }
        }

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            UnityModule.DebugPrint("{0} (V {1}) 启动！=>>> Main()", Application.ProductName, Application.ProductVersion);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //为 Program 监听全局异常捕获；
            //Application.ThreadException += Application_ThreadException;
            //AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.ApplicationExit += Application_ApplicationExit;

            //初始化目录
            CheckDirectory();
            //补齐默认配置
            ConfigController.LoadDefaultConfig();


            /* TODO : 放到登录窗口
            StartUpTemplateClass.UserName = ConfigController.GetConfig("UserName");
            StartUpTemplateClass.Password = ConfigController.GetConfig("Password");
            StartUpTemplateClass.HeadPortrait = Base64Controller.Base64ToImage(ConfigController.GetConfig("HeadPortrait"));
             */

            // 显示所有 StartUpForm
            /*
            foreach (StartUpTemplateClass a in StartUpController.ScanStartUpPlugins(UnityModule.StartUpDirectory))
            {
                new Form() { FormBorderStyle = FormBorderStyle.None, Size = new Size(160, 90), BackgroundImage = a.Preview }.ShowDialog();
                a.StartUpForm.ShowDialog();
            }
            */

            //初始化启动画面
            try
            {
                StartUp = InitializeStartUp();
            }
            catch (Exception ex)
            {
                UnityModule.DebugPrint("初始化 StartUp 遇到异常 : {0}", ex.Message);
                MessageBox.Show("初始化 StartUp 遇到异常 : "+ ex.Message);
                return;
            }
            
            //显示 StartUp.Preview
            //new Form() { FormBorderStyle = FormBorderStyle.None, Size = new Size(160, 90), BackgroundImage = StartUp.Preview }.ShowDialog();

            StartUp.StartUpForm.ShowDialog();
            GC.Collect();

            Application.Run(new DesktopForm());
        }

        /// <summary>
        /// 初始化目录
        /// </summary>
        private static void CheckDirectory()
        {
            //TODO : 程序需要的目录在这里初始化
            try
            {
                if (!Directory.Exists(UnityModule.StartUpDirectory))
                    Directory.CreateDirectory(UnityModule.StartUpDirectory);
            }catch(Exception ex)
            {
                UnityModule.DebugPrint("创建工作目录遇到异常：{0}", ex.Message);
            }
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception UnhandledException = e.ExceptionObject as Exception;
            MessageBox.Show(string.Format("捕获到未处理异常：{0}\r\n异常信息：{1}\r\n异常堆栈：{2}\r\nCLR即将退出：{3}", UnhandledException.GetType(), UnhandledException.Message, UnhandledException.StackTrace, e.IsTerminating));
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            Exception ThreadException = e.Exception;
            MessageBox.Show(string.Format("捕获到未处理异常：{0}\r\n异常信息：{1}\r\n异常堆栈：{2}", ThreadException.GetType(), ThreadException.Message, ThreadException.StackTrace));
        }

        static void Application_ApplicationExit(object sender, EventArgs e)
        {
            UnityModule.DebugPrint("程序退出 ...");
        }

        private static void SwitchToLogin(object s, EventArgs e)
        {
            UnityModule.DebugPrint("启动完成！");
            
        }

        /// <summary>
        /// 初始化启动画面
        /// </summary>
        private static StartUpTemplateClass InitializeStartUp()
        {
            StartUpTemplateClass.StartUpIcon = UnityResource.HackSystemLogoIcon;
            string StartUpPath = FileController.PathCombine(UnityModule.StartUpDirectory, ConfigController.GetConfig("StartUpFile"));
            StartUpTemplateClass StartUpInstance = StartUpController.GetStartUpPlugin(
                StartUpPath,
                ConfigController.GetConfig("StartUpName"));

            //无法创建指定DLL内指定CLASS的StartUp对象时，尝试扫描整个目录
            if (StartUpInstance == null)
            {
                UnityModule.DebugPrint("无法创建指定的 StartUp，尝试扫描 StartUp 目录 ...");
                StartUpInstance = StartUpController.ScanStartUpPlugins(UnityModule.StartUpDirectory).FirstOrDefault();
            }

            if (StartUpInstance == null)
                throw new Exception("无法创建 StartUp 对象");
            if (StartUpInstance.StartUpForm == null)
                throw new Exception("无法创建 StartUp.StartUpForm 对象");

            StartUpInstance.StartUpFinished += new EventHandler<EventArgs>((s, e) => { SwitchToLogin(s, e); });
            return StartUpInstance;
        }

    }
}
