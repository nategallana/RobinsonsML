using RobinsonMallLinkingWBOX.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinSCP;

namespace RobinsonMallLinkingWBOX.Helpers
{
    public class SFTP
    {
        
        public SettingsWriter _settings = null;
        public Configuration _systemConfig = null;
        public List<Configuration> _system = new List<Configuration>();
        Logdata logs = new Logdata();
        //SftpConnectionChecker checker = new SftpConnectionChecker();
        private string ServerName, Username, Password, directory, TenantName, TempDB;
        public void InitializeConfiguration()
        {
            _systemConfig = _settings.Read<Configuration>("Configuration");
            if (_systemConfig != null)
            {
                //txtXML.Text = _systemConfig.XMLPath.Trim().ToString();
                ServerName = _systemConfig.Host.Trim().ToString();
                Username = _systemConfig.Username.Trim().ToString();
                Password = _systemConfig.Password.Trim().ToString();
                directory = _systemConfig.Directory.Trim().ToString();
                TenantName = _systemConfig.TenantName.Trim().ToString();
                //txtTerminal.Text = _systemConfig.TerminalNo.Trim().ToString();
                TempDB = _systemConfig.TempMDB.Trim().ToString();
            }
        }
        private const string UnsentDirectoryName = "Pending File";
        private const string SentDirectoryName = "Send File";
#pragma warning disable CS0169 // The field 'SFTP._notifyIcon' is never used
        private static NotifyIcon _notifyIcon;
#pragma warning restore CS0169 // The field 'SFTP._notifyIcon' is never used
        private static System.Threading.Timer connectionTimer;

        public static SessionOptions SessionOptions1 { get; set; }
        private class ConnectionTimerState
        {
            public string SftpHost { get; set; }
            public string SftpUsername { get; set; }
            public string SftpPassword { get; set; }
        }
        public static void StartTimerIfNeeded(string condition, string sftpHost, string sftpUsername, string sftpPassword)
        {
            if (condition == "start" && connectionTimer == null)
            {
                connectionTimer = new System.Threading.Timer(ConnectionTimerCallback, new ConnectionTimerState
                {
                    SftpHost = sftpHost,
                    SftpUsername = sftpUsername,
                    SftpPassword = sftpPassword
                }, 0, 5000); // Set the interval to 60,000 milliseconds (60 seconds)
            }
        }
        private static void ConnectionTimerCallback(object state)
        {
            //ReadConfigurations();
            ConnectionTimerState timerState = (ConnectionTimerState)state;

            if (CheckSftpConnection())
            {
                connectionTimer?.Dispose();
                TrySendUnsentFiles(timerState.SftpHost, timerState.SftpUsername, timerState.SftpPassword);
            }
            else
            {
                // ShowMessage("Still unable to establish a connection. Continuing the timer.");
            }
        }

        public void SftpTrans(string filepath,string sftpHost, string sftpUsername, string sftpPassword)
        {
            Logdata logger = new Logdata();
            try
            {
                if (CheckSftpConnection())
                {
                    using (Session session = new Session())
                    {
                        SessionOptions1 = new SessionOptions
                        {
                            Protocol = Protocol.Sftp,
                            HostName = sftpHost,
                            UserName = sftpUsername,
                            Password = sftpPassword,
                            PortNumber = 22,
                            SshHostKeyFingerprint = GlobalVar.SSH,
                            TimeoutInMilliseconds = 60000
                        };
                        session.Open(SessionOptions1);

                        TransferOptions transferOptions = new TransferOptions { TransferMode = TransferMode.Ascii };
                        string searchPattern = "*.*";
                        string[] filesToTransfer = Directory.GetFiles(GlobalVar.DIRECTORY, searchPattern);

                        foreach (string filePath in filesToTransfer)
                        {
                            string fileName = Path.GetFileName(filePath);
                            string successDirectoryPath = Path.Combine(GlobalVar.DIRECTORY, SentDirectoryName);
                            string remoteFilePath = fileName;
                            string sentfilepath = Path.Combine(successDirectoryPath, fileName);

                            if (File.Exists(sentfilepath))
                            {
                                continue;
                            }

                            TransferOperationResult transferResult = session.PutFiles(filepath, remoteFilePath, false, transferOptions);
                            transferResult.Check();

                            logger.log(fileName, GlobalVar.batchcount, "Sent");

                            string sent = Path.Combine(successDirectoryPath, fileName);
                            File.Copy(filePath, sent, true);
                            //File.Delete(filePath);
                        }
                        GlobalVar.SuccessSending = true;
                        GlobalVar.ErrorSending = false;
                        GlobalVar.SuccessSending2 = false;
                        sftpConnectionChecker.StopChecking();
                    }
                }
                else
                {
                    BackupUnsentFiles(GlobalVar.DIRECTORY, logger);
                    StartTimerIfNeeded("start", sftpHost, sftpUsername, sftpPassword);
                }
            }
            catch(Exception ex)
            {
                StartTimerIfNeeded("start", sftpHost, sftpUsername, sftpPassword);
                //ErrorLogger.LogError(ex.Message + " || " + ex.StackTrace);
                //SftpConnectionChecker.StartChecking();
                HandleException(ex);
            }
        }
        public static void TrySendUnsentFiles(string sftpHost, string sftpUsername, string sftpPassword)
        {
            Logdata logger = new Logdata();
            //MessageBox.Show("Sending unsent files");
            if (CheckSftpConnection())
            {

                string backupDirectoryPath = Path.Combine(GlobalVar.DIRECTORY, UnsentDirectoryName);
                if (Directory.Exists(backupDirectoryPath))
                {
                    // Adjust the search pattern as needed
                    string searchPattern = "*.*";
                    // string[] unsentFiles = Directory.GetFiles(MallLinkConfig.filepath, searchPattern);
                    string[] unsentFiles = Directory.GetFiles(backupDirectoryPath, searchPattern);
                    if (unsentFiles.Length > 0)
                    {
                        try
                        {
                            using (Session session = new Session())
                            {
                                SessionOptions1 = new SessionOptions
                                {
                                    Protocol = Protocol.Sftp,
                                    HostName = sftpHost,
                                    UserName = sftpUsername,         
                                    Password = sftpPassword,
                                    PortNumber = 22,
                                    SshHostKeyFingerprint = GlobalVar.SSH,
                                    TimeoutInMilliseconds = 60000// Ensure this is the correct fingerprint
                                };

                                session.Open(SessionOptions1);

                                TransferOptions transferOptions = new TransferOptions { TransferMode = TransferMode.Ascii };

                                foreach (string unsentFile in unsentFiles)
                                {
                                    string fileName = Path.GetFileName(unsentFile);
                                    string remoteFilePath = fileName; // Only the filename, no directory structure

                                    //if(session.FileExists(remoteFilePath))
                                    //{
                                    //    continue;
                                    //}

                                    TransferOperationResult transferResult = session.PutFiles(
                                    Path.Combine(GlobalVar.DIRECTORY, "*.*"), // Source: All files
                                    "/", // Destination: Root folder or specify a path
                                    false, // Do not remove original files after upload
                                    transferOptions
                                    );

                                    logger.log(fileName,GlobalVar.batchcount, "Sent");

                                    string successDirectoryPath = Path.Combine(GlobalVar.DIRECTORY, SentDirectoryName);

                                    string sentFilePath = Path.Combine(successDirectoryPath, fileName);
                                    File.Copy(unsentFile, sentFilePath, true); // Move the file to the sent directory
                                    //File.Delete(unsentFile); // Delete the file from the backup directory after successful transfer

                                }

                                // ShowMessage("Trying to send unsent files... Successfully");
                                GlobalVar.SuccessSending2 = true;
                                GlobalVar.ErrorSending = false;
                                GlobalVar.SuccessSending = false;
                            }

                            // SftpConnectionChecker.StopChecking();
                        }
                        catch (Exception ex)
                        {
                            StartTimerIfNeeded("start", GlobalVar.HOST, GlobalVar.USERNAME, GlobalVar.PASSWORD);
                            HandleException(ex);
                            sftpConnectionChecker.StartChecking();
                            //ShowMessageError("Sales file is not sent to RLC server. Please contact your POS vendor");
                        }
                    }
                }
            }
        }
        private static void BackupUnsentFiles(string directoryPath, Logdata logger)
        {
            string backupDirectoryPath = Path.Combine(GlobalVar.DIRECTORY, UnsentDirectoryName);
           // CreateDirectoryIfNotExists(backupDirectoryPath);

            // Adjust the search pattern as needed
            string searchPattern = "*.*";
            string[] filesToBackup = Directory.GetFiles(GlobalVar.DIRECTORY, searchPattern);

            foreach (string filePath in filesToBackup)
            {
                string fileName = Path.GetFileName(filePath);
                string successDirectoryPath = Path.Combine(GlobalVar.DIRECTORY, SentDirectoryName);
                string backupFilePath = Path.Combine(backupDirectoryPath, fileName);
                string remoteFilePath = fileName;
                string sentfilepath = Path.Combine(successDirectoryPath, fileName);

                if (File.Exists(sentfilepath))
                {
                    continue;
                }

                File.Copy(filePath, backupFilePath, true); // Move the file to the backup directory
                                                           //File.Delete(filePath);
                logger.log(fileName,GlobalVar.batchcount, "Unsent");
            }

            GlobalVar.ErrorSending = true;
            GlobalVar.SuccessSending = false;
            GlobalVar.SuccessSending2 = false;
        }
        public static bool CheckSftpConnection()
        {
            try
            {
                
                SessionOptions1 = new SessionOptions
                {
                    Protocol = Protocol.Sftp,
                    HostName = GlobalVar.HOST,
                    UserName = GlobalVar.USERNAME,
                    Password = GlobalVar.PASSWORD,
                    PortNumber = 22,
                    SshHostKeyFingerprint = GlobalVar.SSH, // Ensure this is the correct fingerprint
                    TimeoutInMilliseconds = 120000,

                };
                using (Session session = new Session())
                {
                    session.Open(SessionOptions1);

                    string presentedFingerprint = SessionOptions1.SshHostKeyFingerprint;
                    // ShowMessage($"Presented SSH Host Key Fingerprint: {presentedFingerprint}");

                    // Check if it matches the expected fingerprint
                    if (presentedFingerprint != SessionOptions1.SshHostKeyFingerprint)
                    {
                        throw new InvalidOperationException("SSH host key fingerprint does not match the expected pattern.");
                    }
                    return session.Opened;
                    //return true;
                }
            }
            catch (SessionRemoteException ex)
            {
                ErrorLogger.LogError($"SFTP connection error: {ex.Message} || {ex.StackTrace}");

                // Implement retry logic if necessary
                // Retry up to 3 times with a delay between retries
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        Thread.Sleep(60000); // Wait for 2 seconds before retrying
                        using (Session session = new Session())
                        {
                            session.Open(SessionOptions1);
                            return session.Opened;
                        }
                    }
                    catch
                    {
                        // Continue to retry
                    }
                }

                // After retries, return false if connection couldn't be established
                return false;
            }
            catch (Exception ex)
            {
                StartTimerIfNeeded("start", Settings.HOST, Settings.USERNAME, Settings.PASSWORD);
                HandleException(ex);
                return false;
            }
        }
        private static void ShowMessage(string message)
        {
            //if (!messageShown) {
            //     MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //     messageShown = true;
            // }
            MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //     messageShown = true;
        }
        private static void HandleException(Exception ex)
        {
            if (ex.Message.Contains("SSH host key fingerprint does not match the expected pattern"))
            {
                ShowMessage("SSH host key fingerprint mismatch. Please verify the server's SSH host key fingerprint.");
            }
            else
            {
                // MessageBox.Show($"{ex.Message} || {ex.StackTrace}");
                ErrorLogger.LogError ($"{ex.Message} || {ex.StackTrace}");

            }
            StartTimerIfNeeded("start", Settings.HOST, Settings.USERNAME, Settings.PASSWORD);
        }
    }
}
