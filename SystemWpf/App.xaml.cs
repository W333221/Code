using Serilog;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using SystemWpf.ViewModels;
using SystemWpf.Views;

namespace SystemWpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        public App()
        {
            InitializeComponent();
        }
        protected override Window CreateShell()
        {            
            return Container.Resolve<MainWindow>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                InitializeLogger();
                this.DispatcherUnhandledException += App_DispatcherUnhandledException;
                TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                Process proceMain = Process.GetCurrentProcess();
                System.Diagnostics.Process[] processes = System.Diagnostics.Process.GetProcessesByName(proceMain.ProcessName);
                string appStartupPath = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                //Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                if (processes.Length > 1)
                {
                    //MessageBox.Show("其他用户正在使用或应用程序正在运行中...");
                    //Thread.Sleep(500);
                    //Environment.Exit(1);
                    var args = Environment.GetCommandLineArgs();
                    bool isRestart = args.Contains("--restart");
                    if (isRestart)
                    {
                        foreach (Process process in processes)//获取所有同名进程id
                        {
                            if (process.Id != proceMain.Id)//根据进程id删除所有除本进程外的所有相同进程
                                process.Kill();
                        }
                    }
                    else
                    {
                        System.Windows.MessageBoxResult dialogResult = HandyControl.Controls.MessageBox.Show($"已运行{processes.Length - 1}个重复的程序,确认是否关闭其他程序？", "提示", System.Windows.MessageBoxButton.YesNoCancel, System.Windows.MessageBoxImage.Warning);
                        if (dialogResult == System.Windows.MessageBoxResult.Yes)
                        {
                            foreach (Process process in processes)//获取所有同名进程id
                            {
                                if (process.Id != proceMain.Id)//根据进程id删除所有除本进程外的所有相同进程
                                    process.Kill();
                            }
                        }
                        else//没有关闭其他的进程，不允许打开新的程序
                        {
                            proceMain.Kill();
                            return;
                        }
                    }

                }
                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message.ToString() + ",堆栈:" + ex.StackTrace);
            }

        }
        #region 服务注册
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            RegisterViews(containerRegistry);
        }

        private void RegisterViews(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<MainWindow, MainWindowViewModel>();
            containerRegistry.RegisterForNavigation<Main,MainViewModel>();
        }
        #endregion

            #region 初始化日志

            /// <summary>
            /// 初始化日志
            /// </summary>
        private void InitializeLogger()
        {
            string output = "{Timestamp:HH:mm:ss.fff}: [{Level:u3}]{Message:lj}{NewLine}{Exception}";
            var log = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Verbose()
                //.Enrich.WithProperty("Application", "Test123") // 为每个项目添加一个唯一标识属性
                //.WriteTo.Seq("http://localhost:5341")       // 发送到本地 Seq 服务器

                .WriteTo.Debug(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.Logger(l => l.Filter.ByIncludingOnly(f => f.Level == Serilog.Events.LogEventLevel.Debug)
                   .WriteTo.Async(a => a.File("Log\\Debug\\Debug.txt", rollingInterval: RollingInterval.Day, outputTemplate: output, retainedFileCountLimit: 120)))
                .WriteTo.Logger(l => l.Filter.ByIncludingOnly(f => f.Level == Serilog.Events.LogEventLevel.Information)
                    .WriteTo.Async(a => a.File("Log\\Info\\Info.txt", rollingInterval: RollingInterval.Day, outputTemplate: output, retainedFileCountLimit: 120)))
                .WriteTo.Logger(l => l.Filter.ByIncludingOnly(f => f.Level >= Serilog.Events.LogEventLevel.Error)
                    .WriteTo.Async(a => a.File("Log\\Error\\Error.txt", rollingInterval: RollingInterval.Day, outputTemplate: output, retainedFileCountLimit: 120)))
                 .WriteTo.Logger(l => l.Filter.ByIncludingOnly(f => f.Level == Serilog.Events.LogEventLevel.Warning)
                    .WriteTo.Async(a => a.File("Log\\Warning\\Warning.txt", rollingInterval: RollingInterval.Day, outputTemplate: output, retainedFileCountLimit: 120)))
                .WriteTo.Logger(l => l.Filter.ByIncludingOnly(f => f.Level == Serilog.Events.LogEventLevel.Verbose)
                   .WriteTo.Async(a => a.File("Log\\API\\ApiLog.txt", rollingInterval: RollingInterval.Day, outputTemplate: output, retainedFileCountLimit: 120)))
                .CreateLogger();
            Log.Logger = log;
            AppDomain.CurrentDomain.ProcessExit += (s, e) => Log.CloseAndFlush();
            Log.Information("\r\n");
            Log.Information("***************系统启动*****************");
        }

        #endregion

        #region 异常处理
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                e.Handled = true;
                Log.Error("全局UI异常捕获" + e.Exception.Message.ToString() + ",堆栈:" + e.Exception?.StackTrace?.ToString());
            }
            catch (Exception ex)
            {
                Log.Error("App_DispatcherUnhandledException异常" + ex.Message + ",堆栈:" + ex.StackTrace);
            }
        }
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                var exception = e.ExceptionObject as Exception;
                Log.Error("全局应用程序异常" + exception?.Message.ToString() + ",堆栈:" + exception?.StackTrace?.ToString());
            }
            catch (Exception ex)
            {
                Log.Error("CurrentDomain_UnhandledException异常" + ex.Message + ",堆栈:" + ex.StackTrace);
            }
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            try
            {
                foreach (var exception in e.Exception?.InnerExceptions)
                {
                    Log.Error("全局应用程序异常" + exception?.Message);
                }
                Log.Error("全局应用程序异常" + ",堆栈:" + e.Exception.StackTrace?.ToString());
            }
            catch (Exception ex)
            {
                Log.Error("TaskScheduler_UnobservedTaskException异常" + ex.Message + ",堆栈:" + ex.StackTrace);
            }

        }
        #endregion

    }

}
