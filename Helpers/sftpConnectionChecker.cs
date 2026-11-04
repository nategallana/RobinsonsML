using RobinsonMallLinkingWBOX.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
#pragma warning disable CS0105 // The using directive for 'RobinsonMallLinkingWBOX.Models' appeared previously in this namespace
using RobinsonMallLinkingWBOX.Models;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
#pragma warning restore CS0105 // The using directive for 'RobinsonMallLinkingWBOX.Models' appeared previously in this namespace

namespace RobinsonMallLinkingWBOX.Helpers
{
    public static class sftpConnectionChecker
    {
        private static readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private static Task _connectionCheckTask;
#pragma warning disable CS0169 // The field 'sftpConnectionChecker._notifyIcon' is never used
        private static NotifyIcon _notifyIcon;
#pragma warning restore CS0169 // The field 'sftpConnectionChecker._notifyIcon' is never used

        public static void StartChecking()
        {
            if (_connectionCheckTask == null || _connectionCheckTask.Status != TaskStatus.Running)
            {
                _connectionCheckTask = Task.Run(() => CheckConnectionLoop(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
            }
        }

        //public static void StartChecking()
        //{
        //    if (_cancellationTokenSource.IsCancellationRequested)
        //    {
        //        _cancellationTokenSource.Dispose();
        //        _cancellationTokenSource = new CancellationTokenSource();
        //    }

        //    if (_connectionCheckTask == null || _connectionCheckTask.IsCompleted || _connectionCheckTask.IsFaulted)
        //    {
        //        _connectionCheckTask = Task.Run(async () =>
        //        {
        //            try
        //            {
        //                await CheckConnectionLoop(_cancellationTokenSource.Token);
        //            }
        //            catch (Exception ex)
        //            {
        //                ErrorLogger.LogError($"Connection check loop failed: {ex.Message} || {ex.StackTrace}");
        //            }
        //        });
        //    }
        //}


        public static void StopChecking()
        {
            _cancellationTokenSource.Cancel();
            //_notifyIcon.Visible = false; // Hide the tray icon
            //_notifyIcon.Dispose(); // Dispose the tray icon
        }

        private static async Task CheckConnectionLoop(CancellationToken cancellationToken)
        {
            //_notifyIcon = new NotifyIcon
            //{
            //    Icon = SystemIcons.Information,
            //    Visible = true,
            //    Text = "SFTP Connection Checker",
            //    BalloonTipTitle = "SFTP Connection Status"
            //};

            //while (!cancellationToken.IsCancellationRequested)
            //{
            //    try
            //    {
            //        string UnsentDirectoryName = "Pending File";
            //        GlobalVar.SuccessSending2 = false;
            //        // Replace with your SFTP connection check logic
            //        bool isConnected = SFTP.CheckSftpConnection();
            //        string backupDirectoryPath = Path.Combine(GlobalVar.DIRECTORY, UnsentDirectoryName);
            //        if (isConnected)
            //        {
            //            if (Directory.Exists(backupDirectoryPath))
            //            {
            //                string[] files = Directory.GetFiles(backupDirectoryPath);

            //                if (files.Length > 0)
            //                {
            //                    //SFTP.TrySendUnsentFiles(GlobalVar.DIRECTORY, GlobalVar.HOST, GlobalVar.PASSWORD);
            //                    SFTP.TrySendUnsentFiles(GlobalVar.HOST, GlobalVar.USERNAME, GlobalVar.PASSWORD);
            //                    //_notifyIcon.ShowBalloonTip(3000, "Connection Established", "Connected", ToolTipIcon.Info);
            //                    if (GlobalVar.SuccessSending2 == true)
            //                    {
            //                        MessageBox.Show("Trying to send unsent files... successful.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //                        // _notifyIcon.ShowBalloonTip(3000, "Connection Established", "Trying to send unsent files... successful", ToolTipIcon.Info);
            //                    }
            //                }
            //                else
            //                {
            //                    StopChecking();
            //                    //_notifyIcon.ShowBalloonTip(3000, "Connection Established", "Successfully Connected", ToolTipIcon.Info);
            //                }
            //            }
            //        }
            //        else
            //        {
            //            // StopChecking();
            //            SFTP.StartTimerIfNeeded("start", GlobalVar.HOST, GlobalVar.USERNAME, GlobalVar.PASSWORD);
            //            //MessageBox.Show("Connection Lost", "Program closed.");
            //            //Application.Exit();
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        // Log exception
            //        ErrorLogger.LogError($"{ex.Message} || {ex.StackTrace}");
            //    }

            //    // Wait for a specified interval before checking again
            //    await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
            // }
            //MessageBox.Show("Check");
        }

    }
}