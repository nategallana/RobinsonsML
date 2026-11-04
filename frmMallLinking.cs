 using RobinsonMallLinkingWBOX.Constants;
using RobinsonMallLinkingWBOX.LocalStorage;
using RobinsonMallLinkingWBOX.Models;
using RobinsonMallLinkingWBOX.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

using System.Xml.Linq;
using Tarsier.Extensions;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Net.Sockets;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Runtime.InteropServices;
using RobMLWBOX;
using RobMLWBOX.Helpers;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
//using RobinsonMallLinkingWBOX.ModelsRequest;

namespace RobinsonMallLinkingWBOX
{
    public partial class frmMallLinking : Form
    {

        #region --
        public DbParadox _dbParadox = null;
        public string _databaseFileName = Path.Combine(Application.StartupPath, TempDBName.DATABASE);
        public TransactionsDataStorage transactionDataStorage = null;
        public BatchFileDataStorage batchfileDataStorage = null;
        public DiscountDataStorage discountDataStorage = null;
        public ItemDataStorage itemDataStorage = null;
        public PaymentDataStorage paymentDataStorage = null;
        public TaxDataStorage taxDataStorage = null;
        public VoidDataStorage voidDataStorage = null;
        //public EODDataStorage eodDataStorage = null;
        public EODSLSDataStorage eodSLSDataStorage = null;
        public Configuration configuration = null;
        public Settings settings = null;
        public string xml, host, username, password, dir, tenantname, terminal, secpass, starttime, endtime = null;
        //private System.Windows.Forms.Timer scenarioTimer;
#pragma warning disable CS0169 // The field 'frmMallLinking.workerTimer' is never used
        private System.Windows.Forms.Timer workerTimer;
#pragma warning restore CS0169 // The field 'frmMallLinking.workerTimer' is never used
        private System.Timers.Timer eodTimer;
        private System.Windows.Forms.Timer appCloseTimer;
        private System.Windows.Forms.Timer appCloseTimer1;
        private bool isEODTriggered = false;
        private readonly object eodLock = new object();
        DateTime lastAutoEODRun = DateTime.MinValue; // Put this as a class-level field
        public int TotalRecords, SalesYesterday1, EODToday1, EODYesterday1, perTrans1, perTransYestday1;
        private bool hasShownEODMissingWarning = false;
        private bool hasShownRestrictedTimeWarning = false;
        private bool isAutoEODRunning = false;
        private bool isMessageBoxShown = false;
        private bool wDesktopWasRunning = false;
        #endregion

        #region EODVariables
        public string EODTENTID;
        public string EODTERMNO;
        public decimal EODGROSS;
        private DateTime dateNow;
        public decimal EODTOTVAT;
        public decimal EODTOTVOID;
        public int EODNOVOID;
        public decimal EODTOTDISC;
        public int EODNODISC;
        public decimal EODTOTRFD;
        public int EODNORFD;
        public decimal EODOTHRNEGA;
        public int EODNOOTHRNEGA;
        public decimal EODTOTSRCHR;
        public int EODPRVCNTR;
        public decimal EODPRVGRNTOT;
        public int EODCRRCNTR;
        public decimal EODCRRGRNTOT;
        public string EODTRNSDATE;
        public decimal EODNOV;
        public decimal EODMISC;
        public decimal EODLOCTAX;
        public decimal EODTOTCRDSLS;
        public decimal EODTOTCRDVAT;
        public decimal EODTOTNONVAT;
        private decimal grossSales;
        public decimal EODPHRMSLS;
        public decimal EODNONPHRMSLS;
        public decimal EODDISABLEDISC;
        public decimal EODPERCRENT;
        public decimal EODTOTAMNTREPRNT;
        public int EODNOREPRNT;


        public string LAST_DATE_MMDDYYYY;
        public string LAST_DATE_YYYYMMDD;
        public string YESTERDAY_MMDDYYYY;
        public string YESTERDAY_YYYYMMDD;
        #endregion
        SFTP sftp2 = new SFTP();
#pragma warning disable CS0169 // The field 'frmMallLinking._notifyIcon' is never used
        private static NotifyIcon _notifyIcon;
#pragma warning restore CS0169 // The field 'frmMallLinking._notifyIcon' is never used
        //public ExternalTransactionRequest request = null;
        //private BackgroundWorker backgroundWorker;
        FileSystemWatcher watcher;
#pragma warning disable CS0169 // The field 'frmMallLinking.instance' is never used
        private static frmMallLinking instance;
#pragma warning restore CS0169 // The field 'frmMallLinking.instance' is never used
        private decimal nonVat;
        private decimal discwithVat;
        private int otherDiscCnt;
        private decimal nonV;
        private decimal addscpwed;
        private decimal forNvGross;

        private BackgroundWorker backgroundWorker;

        //KIM_07042025
        public SettingsWriter _settings = null;

        public frmMallLinking()
        {
            try
            {
                InitializeComponent();
                EnsureSingleTrayIcon();
                InitializeProgram();
                GlobalVar.InitializeConfiguration();


                //KIM_07042025
                _settings = new SettingsWriter(Path.Combine(Application.StartupPath, "Settings"), false);

                xml = GlobalVar.XMLPath;
                host = GlobalVar.HOST;
                username = GlobalVar.USERNAME;
                password = GlobalVar.PASSWORD;
                dir = GlobalVar.DIRECTORY;
                tenantname = GlobalVar.TENANTNAME;
                terminal = GlobalVar.TERMINALNO;
                secpass = GlobalVar.SECPASS;
                starttime = GlobalVar.STARTTIME;
                endtime = GlobalVar.ENDTIME;

                InitializeEODHandler();
                InitializeFolder();

                backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += BackgroundWorker_DoWork;
                backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;


                this.Resize += frmMallLinking_Resize;



                watcher = new FileSystemWatcher();
                watcher.Path = xml;
                watcher.EnableRaisingEvents = true;
                watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
                watcher.Filter = "*.xml";
                watcher.Created += new FileSystemEventHandler(OnChanged);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during form load:\n{ex.Message}", "Check Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ErrorLogger.LogError(ex.Message.ToString() + " || " + ex.StackTrace.ToString());
            }
            
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            appCloseTimer1.Start();
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(60000);
            if (sftp2.CheckSftpConnection1())
            {
                UnsentFile();
            }

            //UnsentFile();
        }

        private void frmMallLinking_Load(object sender, EventArgs e)
        {
            try
            {

                // Always ensure config is loaded early 
                //KIM_07042025
                //if (Settings._systemConfig == null)
                //{
                //    Settings.LoadSystemConfig();
                //}

                Thread.Sleep(5000);
                BackupXML(xml);
                // ✅ Check for target process
                if (CheckCallingApp("wDesktop"))//Call the Mall Link from wBox
                {
                    ErrorLogger.LogError("wDesktop is running.");
                    //MessageBox.Show($"wDesktop is running.", "Correct Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    InitializeParadoxDB();
                    RunEODRange("Manual"); //EOD process
                    RemoveDuplicateNotifyIcons();
                    CloseOtherAppIfRunning();
                    //this.Close();

                }
                InitializeResendTimer();
                //UnsentFile();
                sftpConnectionChecker.StartChecking();
                InitializeAppCloseHandler();

            }
            catch (Exception ex)
            {
                //MessageBox.Show($"An error occurred during form load:\n{ex.Message}", "Check Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ErrorLogger.LogError(ex.Message.ToString() + " || " + ex.StackTrace.ToString());
            }
        }
        private void InitializeProgram()
        {
            Program.DatabaseFile = _databaseFileName;
            batchfileDataStorage = new BatchFileDataStorage();
            transactionDataStorage = new TransactionsDataStorage();
            discountDataStorage = new DiscountDataStorage();
            itemDataStorage = new ItemDataStorage();
            paymentDataStorage = new PaymentDataStorage();
            taxDataStorage = new TaxDataStorage();
            voidDataStorage = new VoidDataStorage();
            eodSLSDataStorage = new EODSLSDataStorage();
        }

        public void UnsentFile()
        {
            // Create and show the loading form
            try
            {
                GlobalVar.fromUnsent = false;
                string unsentPath = Path.Combine(GlobalVar.DIRECTORY, "Pending File");

                // Get all files in the folder
                string[] files = Directory.GetFiles(unsentPath);

                if (files.Length > 0)
                {
                    GlobalVar.fromUnsent = true;
                    appCloseTimer1.Stop();
                    // Print each file path
                    foreach (string filepath in files)
                    {
                        SendSalesFileToServer2(filepath);
                    }
                    CheckFileSendingStatus();
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError($"Error: {ex.Message}\n{ex.StackTrace}");
            }
        }


        private bool CheckCallingApp(string targetProcessName)
        {
            return IsProcessRunning(targetProcessName);
        }
        private void InitializeParadoxDB()
        {
            string wBoxRestrnt = @"C:\Restrnt\Data";
            string wBoxPass = "5A*281";
            _dbParadox = new DbParadox(wBoxRestrnt, wBoxPass);
        }
        private void RemoveDuplicateNotifyIcons()
        {
            foreach (var form in Application.OpenForms.OfType<frmMallLinking>())
            {
                if (form != this && form.notifyIcon1 != null)
                {
                    form.notifyIcon1.Visible = false;
                    form.notifyIcon1.Dispose();
                    Debug.WriteLine("Removed duplicate NotifyIcon.");
                }
            }
        }

        private void InitializeFolder()
        {
            try
            {
                string sentFolder = "Send File";
                string unsentFolder = "Pending File";

                string sent = Path.Combine(dir, sentFolder);
                string unsent = Path.Combine(dir, unsentFolder);

                if (!Directory.Exists(sent) || !Directory.Exists(unsent))
                {
                    Directory.CreateDirectory(sent);
                    Directory.CreateDirectory(unsent);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError($"Error: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public void InitializeEODValues()
        {
            EODTENTID = String.Empty;
            EODTERMNO = String.Empty;
            EODGROSS = 0.00M;
            EODTOTVAT = 0.00M;
            EODTOTVOID = 0.00M;
            EODNOVOID = 0;
            EODTOTDISC = 0.00M;
            EODNODISC = 0;
            EODTOTRFD = 0.00M;
            EODNORFD = 0;
            EODOTHRNEGA = 0.00M;
            EODNOOTHRNEGA = 0;
            EODTOTSRCHR = 0.00M;
            EODPRVCNTR = 0;
            EODPRVGRNTOT = 0.00M;
            EODCRRCNTR = 0;
            EODCRRGRNTOT = 0.00M;
            EODTRNSDATE = String.Empty;
            EODNOV = 0.00M;
            EODMISC = 0.00M;
            EODLOCTAX = 0.00M;
            EODTOTCRDSLS = 0.00M;
            EODTOTCRDVAT = 0.00M;
            EODTOTNONVAT = 0.00M;
            EODPHRMSLS = 0.00M;
            EODNONPHRMSLS = 0.00M;
            EODDISABLEDISC = 0.00M;
            EODPERCRENT = 0.00M;
            EODTOTAMNTREPRNT = 0.00M;
            EODNOREPRNT = 0;
        }

        public void EnsureSingleTrayIcon()
        {
            if (Application.OpenForms.Count > 1) // A second instance is trying to open
            {
                Debug.WriteLine("Closing extra instance before creating a new tray icon.");
                this.Close();
                return;
            }

            if (notifyIcon1 != null)
            {
                notifyIcon1.Visible = true;
            }
        }

        public void ShowForm()
        {
            this.Invoke((MethodInvoker)delegate
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
            });
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (notifyIcon1 != null)
            {
                notifyIcon1.Visible = false;
                notifyIcon1.Dispose();
                notifyIcon1 = null;
            }
        }



        #region Initialize EOD Timer
        private void InitializeEODHandler()
        {
            eodTimer = new System.Timers.Timer(60000); // Check every minute
            eodTimer.Elapsed += CheckEODTrigger;
            eodTimer.Start();
        }

        private void InitializeResendTimer()
        {
            appCloseTimer1 = new System.Windows.Forms.Timer();
            appCloseTimer1.Interval = 1000; // Check every 2 seconds
            appCloseTimer1.Tick += AppCloseTimer1_Tick;
            appCloseTimer1.Start();
        }

        private void InitializeAppCloseHandler()
        {
            appCloseTimer = new System.Windows.Forms.Timer();
            appCloseTimer.Interval = 2000; // Check every 2 seconds
            appCloseTimer.Tick += AppCloseTimer_Tick;
            appCloseTimer.Start();
        }
        private void AppCloseTimer_Tick(object sender, EventArgs e)
        {
            if (IsProcessRunning("wDesktop"))
            {
                try
                {
                    RunScenarios("wDesktop");
                }
                catch (Exception ex)
                {
                    //messagebox.show($"error in runscenarios: {ex.message}", "error", messageboxbuttons.ok, messageboxicon.error);
                    // optional: stop the timer or log error if needed

                    ErrorLogger.LogError(ex.Message.ToString() + " || " + ex.StackTrace.ToString());
                }
            }
        }

        private void AppCloseTimer1_Tick(object sender, EventArgs e)
        {
            if (!backgroundWorker.IsBusy)
            {
                backgroundWorker.RunWorkerAsync();
            }
        }

        private void CheckEODTrigger(object sender, ElapsedEventArgs e)
        {
            DateTime now = DateTime.Now;

            lock (eodLock) // Ensure thread safety
            {
                // Automatically trigger EOD at 4:00 AM if not yet performed
                if (!isEODTriggered && now.TimeOfDay >= new TimeSpan(4, 0, 0) && now.TimeOfDay < new TimeSpan(9, 0, 0))
                {
                    //PerformEOD();
                    isEODTriggered = true;
                }

                // Reset EOD trigger after 9:00 AM
                if (now.TimeOfDay >= new TimeSpan(9, 0, 0))
                {
                    isEODTriggered = false;
                }
            }
        }
        #endregion

        #region Temp DB and API Path
        public void BackupXML(string xmlPath)
        {
            string xmlFolderPath = xmlPath;

            if (!Directory.Exists(xmlFolderPath))
            {
                Directory.CreateDirectory(xmlFolderPath);
            }

            string[] xmlFiles = Directory.GetFiles(xmlFolderPath, "*.XML");

            foreach (string xmlFile in xmlFiles)
            {
                try
                {
                    string xmlContent = File.ReadAllText(xmlFile);
                    SalesPerTransaction(xmlContent);

                    string processedFolderPath = Path.Combine(xmlFolderPath, "ProcessedFiles");
                    if (!Directory.Exists(processedFolderPath))
                    {
                        Directory.CreateDirectory(processedFolderPath);
                    }

                    string destinationFilePath = Path.Combine(processedFolderPath, Path.GetFileName(xmlFile));
                    File.Move(xmlFile, destinationFilePath);

                }
                catch (Exception ex)
                {
                    //MessageBox.Show($"Error processing file '{xmlFile}': {ex.Message}. \n{ex.StackTrace}");

                    ErrorLogger.LogError(ex.Message.ToString() + " || " + ex.StackTrace.ToString());
                }
            }
        }

        #endregion
        #region Read DAta on API
        private void readRequest(FileSystemEventArgs file)
        {
            string filename = Path.GetFileNameWithoutExtension(file.Name);
            BackupXML(filename);
        }

        private async void FillListWatch(FileSystemEventArgs e)
        {
            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Created:
                    MessageBox.Show("Created!");
                    await Task.Delay(500); // short delay to allow full file write
                    BackupXML(xml);
                    break;

                case WatcherChangeTypes.Changed:
                    // Optional: handle if you want to reprocess updated files
                    MessageBox.Show("Changed!");
                    break;

                case WatcherChangeTypes.Renamed:
                    MessageBox.Show("Renamed!");
                    break;

                case WatcherChangeTypes.Deleted:
                    MessageBox.Show("Deleted!");
                    break;
            }
        }
        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            // Process only the newly created file (not everything again)
            ProcessNewXML(e.FullPath);
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            FillListWatch(e);
        }
        private async void ProcessNewXML(string filePath)
        {
            try
            {
                // Wait a little to ensure file is fully written
                await Task.Delay(500);

                if (Path.GetExtension(filePath).ToLower() != ".xml") return;

                if (File.Exists(filePath))
                {
                    string xmlContent = File.ReadAllText(filePath);
                    SalesPerTransaction(xmlContent);

                    // Move to ProcessedFiles folder
                    string processedFolderPath = Path.Combine(xml, "ProcessedFiles");
                    if (!Directory.Exists(processedFolderPath))
                        Directory.CreateDirectory(processedFolderPath);

                    string destinationPath = Path.Combine(processedFolderPath, Path.GetFileName(filePath));
                    File.Move(filePath, destinationPath);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Error in ProcessNewXML: {ex.Message}\n{ex.StackTrace}");

                ErrorLogger.LogError(ex.Message.ToString() + " || " + ex.StackTrace.ToString());
            }
        }

        #endregion
        #region buttons
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you want to close this application?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                Process.GetCurrentProcess().Kill();
            }
        }

        //KIM_07042025
        private void configurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Always re-read config if needed

            //if (string.IsNullOrWhiteSpace(secPass))
            //{
            //    MessageBox.Show("No security password is set in the configuration.", "Error",
            //        MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}

            // Pass the correct password to the security form
            frmSecPassword secForm = new frmSecPassword(secpass);
            secForm.ShowDialog();

            if (secForm.IsAuthorized)
            {
                if (settings == null || settings.IsDisposed)
                {
                    settings = new Settings();
                    settings.Show();
                }
                else
                {
                    settings.BringToFront();
                }
            }
            else
            {
                // Optional: you can handle failed attempt here too
            }
        }

        private void viewDataSentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form _frm = new FileView();
            _frm.Show();
        }


        private void eODToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string wBoxRestrnt = @"C:\Restrnt\Data";
                string wBoxPass = "5A*281";
                _dbParadox = new DbParadox(wBoxRestrnt, wBoxPass);

                RunEODRange("Manual");
                //this.Close();
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex.Message.ToString() + " || " + ex.StackTrace.ToString());
                MessageBox.Show($"Error initializing EOD: {ex.Message}", "EOD Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Save every transaction into the Database
        private void SalesPerTransaction(string xmlContent)
        {
            XDocument doc = XDocument.Parse(xmlContent);
            XElement orderElement = doc.Element("Order");

            try
            {
                if (orderElement != null)
                {

                    // Extract Order details
                    string version = orderElement.Attribute("version")?.Value;
                    string storeName = orderElement.Attribute("storename")?.Value;
                    string orderNo = orderElement.Attribute("orderNo")?.Value;
                    decimal subtotal = decimal.Parse(orderElement.Attribute("subtotal")?.Value ?? "0", CultureInfo.InvariantCulture);
                    decimal tips = decimal.Parse(orderElement.Attribute("tips")?.Value ?? "0", CultureInfo.InvariantCulture);
                    decimal serviceCharge = decimal.Parse(orderElement.Attribute("serviceCharge")?.Value ?? "0", CultureInfo.InvariantCulture);
                    decimal total = decimal.Parse(orderElement.Attribute("total")?.Value ?? "0", CultureInfo.InvariantCulture);
                    int voided = int.Parse(orderElement.Attribute("void")?.Value ?? "0");
                    string accDate = orderElement.Attribute("accDate")?.Value;
                    int salesType = int.Parse(orderElement.Attribute("salestype")?.Value ?? "0");

                    var salesOrder = new OrderData
                    {
                        Version = version,
                        StoreName = storeName,
                        OrderNo = orderNo,
                        Subtotal = subtotal,
                        Tips = tips,
                        ServiceCharge = serviceCharge,
                        Total = total,
                        Void = voided,
                        AccDate = accDate,
                        SalesType = salesType
                    };

                    transactionDataStorage.Add(salesOrder, false);

                    // Handle Discounts
                    var discountsElement = orderElement.Element("Discounts");
                    if (discountsElement != null)
                    {
                        var discountElement = discountsElement.Element("Discount");
                        if (discountElement != null)
                        {
                            string disctype = discountElement.Attribute("type")?.Value;
                            decimal discvalue = decimal.Parse(discountElement.Attribute("value")?.Value ?? "0", CultureInfo.InvariantCulture);

                            var discount = new DiscountData
                            {
                                OrderNo = orderNo,
                                Type = disctype,
                                Amount = discvalue
                            };

                            discountDataStorage.Add(discount, false);
                        }
                    }

                    // Handle Taxes
                    var taxesElement = orderElement.Element("Taxes");
                    if (taxesElement != null)
                    {
                        var taxElement = taxesElement.Element("Tax");
                        if (taxElement != null)
                        {
                            string taxtype = taxElement.Attribute("type")?.Value;
                            string taxname = taxElement.Attribute("name")?.Value;
                            decimal taxamount = decimal.Parse(taxElement.Attribute("amount")?.Value ?? "0", CultureInfo.InvariantCulture);

                            var tax = new TaxData
                            {
                                OrderNo = orderNo,
                                Type = taxtype,
                                Name = taxname,
                                Value = taxamount
                            };

                            taxDataStorage.Add(tax, false);
                        }
                    }

                    // Handle Items
                    var itemsElement = orderElement.Element("Items");
                    if (itemsElement != null)
                    {
                        foreach (var itemElement in itemsElement.Elements("Item"))
                        {
                            int seqNo = int.Parse(itemElement.Attribute("SeqNo")?.Value ?? "0");
                            string menuKey = itemElement.Attribute("MenuKey")?.Value;
                            string menuNo = itemElement.Attribute("MenuNo")?.Value;
                            string menuName = itemElement.Attribute("MenuName")?.Value;
                            int qty = int.Parse(itemElement.Attribute("Qty")?.Value ?? "0");
                            decimal priceBefDisc = decimal.Parse(itemElement.Attribute("PriceBefDisc")?.Value ?? "0", CultureInfo.InvariantCulture);
                            decimal priceAftDisc = decimal.Parse(itemElement.Attribute("PriceAftDisc")?.Value ?? "0", CultureInfo.InvariantCulture);
                            decimal price = decimal.Parse(itemElement.Attribute("Price")?.Value ?? "0", CultureInfo.InvariantCulture);

                            var item = new ItemData
                            {
                                OrderNo = orderNo,
                                SeqNo = seqNo,
                                MenuKey = menuKey,
                                MenuNo = menuNo,
                                MenuName = menuName,
                                Qty = qty,
                                PriceBefDisc = priceBefDisc,
                                PriceAftDisc = priceAftDisc,
                                Price = price
                            };

                            itemDataStorage.Add(item, false);
                        }
                    }

                    // Handle Payments
                    var paymentsElement = orderElement.Element("Payments");
                    if (paymentsElement != null)
                    {
                        foreach (var paymentElement in paymentsElement.Elements("Payment"))
                        {
                            string type = paymentElement.Attribute("type")?.Value;
                            string name = paymentElement.Attribute("name")?.Value;
                            decimal amount = decimal.Parse(paymentElement.Attribute("amount")?.Value ?? "0", CultureInfo.InvariantCulture);

                            var payment = new PaymentData
                            {
                                OrderNo = orderNo,
                                Type = type,
                                Name = name,
                                Amount = amount
                            };

                            paymentDataStorage.Add(payment, false);
                        }
                    }
                }

                // Handle VoidOrder
                XElement requestElement = doc.Element("Request");
                if (requestElement != null)
                {
                    // Navigate to the nested <Request> element
                    XElement nestedRequestElement = requestElement.Element("Request");
                    if (nestedRequestElement != null)
                    {
                        var voidOrderElement = nestedRequestElement.Element("VoidOrder");
                        if (voidOrderElement != null)
                        {
                            string empId = voidOrderElement.Attribute("empid")?.Value;
                            string orderId = voidOrderElement.Attribute("orderid")?.Value;
                            string cloudId = voidOrderElement.Attribute("CloudID")?.Value;
                            string reason = voidOrderElement.Attribute("reason")?.Value;
                            string empName = voidOrderElement.Attribute("empname")?.Value;

                            var voidOrder = new VoidData
                            {
                                EmpID = empId,
                                OrderID = orderId,
                                CloudID = cloudId,
                                Reason = reason,
                                EmpName = empName
                            };

                            voidDataStorage.Add(voidOrder, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception or show it for debugging
                //MessageBox.Show($"Error: {ex.Message}");

                ErrorLogger.LogError(ex.Message.ToString() + " || " + ex.StackTrace.ToString());
            }
        }

        private void dateRangeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form _frm = new frmDateRange();
            _frm.Show();
        }

        #endregion

        #region Read API Data
        private void ReadRequest(FileSystemEventArgs file)
        {
            string filename = Path.GetFileNameWithoutExtension(file.Name);
            BackupXML(filename);
        }
        #endregion

        #region App Closing Logic (During 4 AM to 9 AM)

        private void RunScenarios(string targetProcessName)
        {
            //UnsentFile();
            //Thread.Sleep(5000);
            if (isMessageBoxShown || isAutoEODRunning) return;

            string lastData;
            bool isFromEODTable; // <-- added to match the new method signature
            CheckSalesData(out lastData, out isFromEODTable);

            DateTime now = DateTime.Now;
            if (!DateTime.TryParseExact(lastData, "MMddyyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime lastDataDate))
            {
                if (!targetProcessName.Equals("wDesktop", StringComparison.OrdinalIgnoreCase))
                {
                    ShowWarningOnce("Invalid or missing Business Date. Proceeding with system date.", "Date Parsing Warning");
                }
                return;
            }

            string dateYesterday = now.AddDays(-1).ToString("MMddyyyy");
            DataTable EODYesterday = eodSLSDataStorage.SelectEOD("eodslsdata", "TransDate", dateYesterday);
            foreach (DataRow row in EODYesterday.Rows)
            {
                EODYesterday1 = int.Parse(row["COUNT(*)"].ToString());
            }

            string dateToday = now.ToString("MMddyyyy");
            DataTable EODToday = eodSLSDataStorage.SelectEOD("eodslsdata", "TransDate", dateToday);
            foreach (DataRow row in EODToday.Rows)
            {
                EODToday1 = int.Parse(row["COUNT(*)"].ToString());
            }


            string salesdateYesterday = now.AddDays(-1).ToString("yyyyMMdd");
            DataTable SalesYesterday = transactionDataStorage.SelectSales("orderdata", "AccDate", salesdateYesterday);
            foreach (DataRow row in SalesYesterday.Rows)
            {
                SalesYesterday1 = int.Parse(row["COUNT(*)"].ToString());
            }


            //DataTable firstEOD = eodSLSDataStorage.FirstEOD();
            //string firstEOD1 = string.Empty;
            //DateTime fdate;
            //foreach (DataRow row in firstEOD.Rows)
            //{
            //    firstEOD1 = row["TransDate"].ToString();
            //    fdate = DateTime.ParseExact(firstEOD1, "MMddyyyy", null);
            //}

            //if (string.IsNullOrEmpty(firstEOD1))
            //{
            //    DataTable firstTrans = transactionDataStorage.FirstEOD();
            //    string firstTrans1 = string.Empty;
            //    foreach (DataRow row in firstTrans.Rows)
            //    {
            //        firstTrans1 = row["AccDate"].ToString();
            //        fdate = DateTime.ParseExact(firstEOD1, "yyyyMMdd", null);
            //    }
            //}

            DataTable firstEOD = eodSLSDataStorage.FirstEOD();
            string firstEOD1 = string.Empty;
            DateTime fdate = DateTime.MinValue;

            // Try getting from EOD table first
            if (firstEOD.Rows.Count > 0)
            {
                foreach (DataRow row in firstEOD.Rows)
                {
                    firstEOD1 = row["TransDate"].ToString();
                    fdate = DateTime.ParseExact(firstEOD1, "MMddyyyy", null);
                }
            }
            else
            {
                // No EOD yet — fallback to first transaction date
                DataTable firstTrans = transactionDataStorage.FirstEOD(); // This returns first transaction record
                string firstTrans1 = string.Empty;

                if (firstTrans.Rows.Count > 0)
                {
                    foreach (DataRow row in firstTrans.Rows)
                    {
                        firstTrans1 = row["AccDate"].ToString(); // Assuming format is yyyyMMdd
                        fdate = DateTime.ParseExact(firstTrans1, "yyyyMMdd", null);
                    }
                }
                else
                {
                    // No transactions either, default to today
                    fdate = DateTime.Now.Date;
                }
            }

            DateTime ydate = DateTime.ParseExact(dateYesterday, "MMddyyyy", null);
            //KIM_07052025
            TimeSpan nowTime = DateTime.Now.TimeOfDay;

            TimeSpan startTime = TimeSpan.Zero;
            TimeSpan endTime = TimeSpan.Zero;

            TimeSpan.TryParse(GlobalVar.STARTTIME, out startTime);
            TimeSpan.TryParse(GlobalVar.ENDTIME, out endTime);

            bool isAllowedEarlyTime = nowTime < startTime;
            bool isTimeToAutoEOD = nowTime >= endTime;
            bool isRestrictedTime = nowTime >= startTime && nowTime < endTime - TimeSpan.FromSeconds(1);

            //bool isAllowedEarlyTime = now.TimeOfDay < new TimeSpan(4, 0, 0);
            //bool isTimeToAutoEOD = now.TimeOfDay >= new TimeSpan(9, 0, 0);
            //bool isRestrictedTime = now.TimeOfDay >= new TimeSpan(4, 0, 0) && now.TimeOfDay < new TimeSpan(8, 59, 59);
            bool isDifferentDate = lastDataDate.Date != now.Date;
            bool isEODMissing = EODYesterday1 == 0;
            bool hasEOD = EODToday1 > 0;
            bool hasEODYesterday = EODYesterday1 > 0;
            bool validFirstDate = now <= fdate;
            bool isZeroSales = SalesYesterday1 == 0;
            

            if (isRestrictedTime && !isAutoEODRunning && !hasShownRestrictedTimeWarning)
            {
                hasShownRestrictedTimeWarning = true;
                ShowWarningOnce($"Transactions are not allowed between {startTime} and {endTime}.", "Transaction Restricted");
                //ShowWarningOnce($"Transactions are not allowed between 4:00 AM and 9:00 AM.", "Transaction Restricted");
                hasShownRestrictedTimeWarning = false;
                return;
            }
            else if (validFirstDate)
            {
                ShowWarningOnce("End of Day has already been performed.", "Transaction Restricted");
                return;
            }
            else if (hasEOD)
            {
                ShowWarningOnce("End of Day has already been performed.", "Transaction Restricted");
            }
            else if (isAllowedEarlyTime)
            {
                if (hasEODYesterday)
                {
                    ShowWarningOnce("End of Day has already been performed.", "Transaction Restricted");
                }
                return;
            }
            else if (nowTime >= endTime && isDifferentDate && isEODMissing && !hasShownEODMissingWarning)
            {
                hasShownEODMissingWarning = true;
                frmPrevWarning frmprevwarning = new frmPrevWarning();
                frmprevwarning.ShowDialog();
                //DialogResult result = MessageBox.Show(
                //"Previous' day’s EOD was not performed.",
                //"EOD Check",
                //MessageBoxButtons.OK,
                //MessageBoxIcon.Information
                //);

                //if (result == DialogResult.OK)
                //{
                // User acknowledged the message, proceed with auto EOD
                isAutoEODRunning = true;
                appCloseTimer.Stop();
                lastAutoEODRun = now;

                isAutoEODRunning = false;
                appCloseTimer.Start();

                return;

            }
        }


        private void ShowWarningOnce(string message, string title)
        {
            if (!isMessageBoxShown)
            {
                isMessageBoxShown = true;
                DialogResult result = ShowMessage(message, title, MessageBoxIcon.Warning);

                if (result == DialogResult.OK)
                {
                    CloseOtherAppIfRunning();
                    isMessageBoxShown = false;
                }
            }
        }

        private DialogResult ShowMessage(string text, string caption, MessageBoxIcon icon)
        {
            if (this.InvokeRequired)
            {
                return (DialogResult)this.Invoke(new Func<DialogResult>(() =>
                    MessageBox.Show(text, caption, MessageBoxButtons.OK, icon,
                                    MessageBoxDefaultButton.Button1,
                                    MessageBoxOptions.DefaultDesktopOnly)));
            }
            else
            {
                return MessageBox.Show(text, caption, MessageBoxButtons.OK, icon,
                                       MessageBoxDefaultButton.Button1,
                                       MessageBoxOptions.DefaultDesktopOnly);
            }
        }
        //private void CheckTimeAndCloseApp(object sender, EventArgs e)
        //{
        //    //Debug.WriteLine($"Timer ticked at {DateTime.Now}");

        //    string[] targetProcesses = { "wDesktop" };//, "WirelessLink", "wBoxBridge" };

        //    foreach (var processName in targetProcesses)
        //    {
        //        if (IsProcessRunning(processName))
        //        {
        //            //Debug.WriteLine($"{processName} is running, calling RunScenarios.");
        //            this.Invoke((MethodInvoker)(() => RunScenarios(processName)));
        //        }
        //    }
        //}
        private bool IsProcessRunning(string processName)
        {
            return Process.GetProcessesByName(processName).Any();
        }

        private void CloseOtherAppIfRunning()
        {
            string[] allProcessesToKill = { "wDesktop" }; //, "WirelessLink", "wBoxBridge" };
            List<string> closedApps = new List<string>();
            List<string> failedApps = new List<string>();

            foreach (var procName in allProcessesToKill)
            {
                var processes = Process.GetProcessesByName(procName);
                bool hasError = false;

                if (processes.Any())
                {
                    foreach (var process in processes)
                    {
                        try
                        {
                            process.Kill();
                            process.WaitForExit();
                        }
                        catch
                        {
                            hasError = true;
                        }
                    }

                    if (hasError)
                        failedApps.Add(procName);
                    else
                        closedApps.Add(procName);
                }
            }

            // Log summary silently
            if (closedApps.Count > 0 || failedApps.Count > 0)
            {
                string summary = "";

                if (closedApps.Count > 0)
                    summary += "Closed applications:\n- " + string.Join("\n- ", closedApps) + "\n\n";

                if (failedApps.Count > 0)
                    summary += "Failed to close:\n- " + string.Join("\n- ", failedApps);

                ErrorLogger.LogError("[AppClosure] " + summary.Trim());
            }
        }


        private void ScenarioTimer_Tick(object sender, EventArgs e)
        {
            if (!backgroundWorker1.IsBusy)
            {
                backgroundWorker1.RunWorkerAsync();
            }
        }


        #endregion

        #region Perform EOD
        public bool PerformEOD(string mmddyyyy, string yyyymmdd, string yesterday, bool silentMode = false)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dir))
                {
                    dir = GlobalVar.DIRECTORY;
                }

                if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
                {
                    ErrorLogger.LogError($"The system cannot find the path: '{dir}'. EOD process cancelled.");
                    if (!silentMode)
                    {
                        MessageBox.Show($"The system cannot find the path: '{dir}'\n\nEOD process has been cancelled.", "EOD Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    return false;
                }

                InitializeEODValues();

                EODTENTID = tenantname;
                EODTERMNO = terminal;
                EODTRNSDATE = mmddyyyy;


            #region rengiel edited

            #region line 28 Buy GC
            DataTable dtItem = itemDataStorage.getGCItem(yyyymmdd);
            foreach (DataRow row in dtItem.Rows)
            {
                decimal GCamount = row["PriceAftDisc"].ToSafeDecimal();
                if (row["MenuName"].ToString().ToUpper().Contains("GC"))
                {
                    EODPERCRENT = EODPERCRENT + GCamount;
                }
            }
            #endregion


            #region line 05 06 void

            DataTable dtVoid = voidDataStorage.GetVOID(yyyymmdd);
            foreach(DataRow row in dtVoid.Rows)
            {
                EODTOTVOID = row["TotalVoid"].ToSafeDecimal();
                EODNOVOID = row["CountVoid"].ToSafeInteger();
            }

            #endregion

            #region getTaxAmount line 04
            DataTable dtTax = taxDataStorage.GetTaxAmount(yyyymmdd);
            foreach (DataRow rowTax in dtTax.Rows)
            {
                if (!voidDataStorage.getifVoided(Convert.ToString(rowTax["OrderNo"])))
                {
                    EODTOTVAT = EODTOTVAT + rowTax["Value"].ToSafeDecimal();
                }
            }
            #endregion

            #region getServCharge line 13
            DataTable dtServ = transactionDataStorage.GetOrders(yyyymmdd);
            foreach (DataRow rowServ in dtServ.Rows)
            {
                decimal servCharge = rowServ["ServiceCharge"].ToSafeDecimal();
                if (!voidDataStorage.getifVoided(Convert.ToString(rowServ["OrderNo"])))
                {
                    EODTOTSRCHR = EODTOTSRCHR + servCharge;
                }
            }
            #endregion

            #region getVoidAmount and Count line 05 06

            DataTable voidTABLE = voidDataStorage.GetVOID(yyyymmdd);
            foreach (DataRow row in voidTABLE.Rows)
            {
                EODTOTVOID = row["TotalVoid"].ToSafeDecimal();
                EODNOVOID = row["CountVoid"].ToSafeInteger();
            }

            #endregion

            #region getDiscountTotal line 08 
            DataTable discTABLE = transactionDataStorage.GetDISCOUNT(yyyymmdd);
            foreach (DataRow row in discTABLE.Rows)
            {
                if (!voidDataStorage.getifVoided(Convert.ToString(row["OrderNo"])))
                {
                    //EODTOTDISC = row["DiscountValue"].ToSafeDecimal();
                    EODNODISC++;
                }
            }
            #endregion

            #region getSeniorPWD line 11 12 27
            DataTable dtSeniorPWD = discountDataStorage.GetDISCOUNT(yyyymmdd);
            foreach (DataRow row in dtSeniorPWD.Rows)
            {
                if (!voidDataStorage.getifVoided(Convert.ToString(row["OrderNo"])))
                {
                    decimal positiveAmount = row["Amount"].ToSafeDecimal() * -1M;
                    if (row["Type"].ToString().ToUpper().Contains("SENIOR") || row["Type"].ToString().ToUpper().Contains("SCD"))
                    {
                        EODOTHRNEGA = EODOTHRNEGA + positiveAmount;
                        EODNOOTHRNEGA = EODNOOTHRNEGA + 1;
                    }
                    if (row["Type"].ToString().ToUpper().Contains("PWD"))
                    {
                        EODDISABLEDISC = EODDISABLEDISC + positiveAmount;
                    }
                }
            }
            #endregion

            #region getCreditSales line 22 23
            DataTable dtPayment = paymentDataStorage.getPayment(yyyymmdd);
            decimal notCashAmount = 0M;
            foreach (DataRow row in dtPayment.Rows)
            {
                if (!voidDataStorage.getifVoided(Convert.ToString(row["OrderNo"])))
                {
                    if (!row["Name"].ToString().ToUpper().Contains("CASH"))
                    {
                        if (!row["Name"].ToString().ToUpper().Contains("GC"))
                        {
                            notCashAmount = notCashAmount + row["Amount"].ToSafeDecimal();
                        }
                    }
                }
            }
            EODTOTCRDSLS = notCashAmount;
            EODTOTCRDVAT = notCashAmount / 1.12M * 0.12M;

            #endregion

            #region getNonVatSales line 24
            DataTable dtNonVat = discountDataStorage.GetDISCOUNT(yyyymmdd);
            nonVat = 0M;
            discwithVat = 0M;
            otherDiscCnt = 0;
            foreach (DataRow row in dtNonVat.Rows)
            {
                if (!voidDataStorage.getifVoided(Convert.ToString(row["OrderNo"])))
                {
                    decimal positiveAmount = row["Amount"].ToSafeDecimal() * -1M;
                    if (row["Type"].ToString().ToUpper().Contains("SENIOR") || row["Type"].ToString().ToUpper().Contains("SCD"))
                    {
                        nonVat = nonVat + row["Total"].ToSafeDecimal() + positiveAmount;
                    }
                    else if (row["Type"].ToString().ToUpper().Contains("PWD"))
                    {
                        nonVat = nonVat + row["Total"].ToSafeDecimal() + positiveAmount;
                    }
                    else
                    {
                        otherDiscCnt++;
                        discwithVat = discwithVat + positiveAmount;
                    }
                }
            }
            addscpwed = EODOTHRNEGA + EODDISABLEDISC;
            nonV = addscpwed / 0.2M;
            nonV = nonV - addscpwed;
            EODTOTNONVAT = nonV;
            forNvGross = addscpwed / 0.2M;
            #endregion

            #region getGrossSales line 03
            grossSales = EODTOTVAT / 0.12M * 1.12M;
            EODGROSS = grossSales + forNvGross + discwithVat;
            #endregion

            #region line 07 08 04 17
            EODNODISC = otherDiscCnt;
            EODTOTDISC = discwithVat;

            EODTOTVAT = (EODGROSS - EODOTHRNEGA - EODTOTNONVAT - EODDISABLEDISC - EODPERCRENT) / 1.12M * 0.12M;
            EODCRRGRNTOT = (EODGROSS - EODTOTDISC - EODOTHRNEGA - EODDISABLEDISC) + EODPRVGRNTOT;
            #endregion

            #region Reprinted Transaction from Wbox Data
            try
            {
                dateNow = DateTime.Now;
                string getOrders = string.Format(Queries.OrderList, "orders", dateNow);
                DataTable dtOrder = _dbParadox.GetDataTable(getOrders);
                if (dtOrder.Rows.Count > 0)
                {
                    foreach (DataRow dr in dtOrder.Rows)
                    {
                        decimal reprintAmount = dr["Total"].ToSafeDecimal();
                        int reprintCnt = dr["Number8"].ToSafeInteger();
                        if (dr["Number8"].ToSafeInteger() > 0)
                        {
                            EODTOTAMNTREPRNT = EODTOTAMNTREPRNT + reprintAmount;
                            EODNOREPRNT = EODNOREPRNT + reprintCnt;
                        }
                    }
                }
            }
            catch(Exception ex)
             {
                ErrorLogger.LogError(ex.Message.ToString() + " || " + ex.StackTrace.ToString());
            }
            #endregion

            #region GROSS VATTOTAL TOTALSERVCHARGE
            DataTable ALLdatas = transactionDataStorage.GetALLDATAS(yyyymmdd);
            foreach (DataRow row in ALLdatas.Rows)
            {
                EODTRNSDATE = DateTime.ParseExact(row["TranscDate"].ToString(), "yyyyMMdd", null).ToString("MMddyyyy");
            }
            #endregion

            #region Previous and Current Grand total

            DateTime date = DateTime.ParseExact(yesterday, "yyyyMMdd", null);
            string formattedYesterday = date.ToString("MMddyyyy"); // "04042025"
            DataTable yestData = transactionDataStorage.GetDataYesterday(formattedYesterday);
            foreach (DataRow row in yestData.Rows)
            {
                EODPRVCNTR = row["CurrEODcntr"].ToSafeInteger();
                EODPRVGRNTOT = row["CurrGrndTot"].ToSafeDecimal();
            }

            decimal eodPrvGnTot = EODPRVGRNTOT;
            EODCRRCNTR = EODPRVCNTR + 1;
            EODCRRGRNTOT = EODCRRGRNTOT + EODPRVGRNTOT;
            #endregion

                var EODSLS = new EODSLSData
                {
                    TenantID = EODTENTID,
                    TermNo = EODTERMNO,
                    Gross = Convert.ToDecimal(EODGROSS.ToDecimalPlaces(2)),
                    TotVat = Convert.ToDecimal(EODTOTVAT.ToDecimalPlaces(2)),
                    TotVoid = Convert.ToDecimal(EODTOTVOID.ToDecimalPlaces(2)),
                    NoVoid = EODNOVOID.ToSafeInteger(),
                    TotDisc = Convert.ToDecimal(EODTOTDISC.ToDecimalPlaces(2)),
                    NoDisc = EODNODISC.ToSafeInteger(),
                    TotRfnd = Convert.ToDecimal(EODTOTRFD.ToDecimalPlaces(2)),
                    NoRfnd = EODNORFD.ToSafeInteger(),
                    OthrNegaAdjst = Convert.ToDecimal(EODOTHRNEGA.ToDecimalPlaces(2)),
                    NoNegaAdjst = EODNOOTHRNEGA.ToSafeInteger(),
                    TotSrvChrg = Convert.ToDecimal(EODTOTSRCHR.ToDecimalPlaces(2)),
                    PrvEODcntr = EODPRVCNTR.ToSafeInteger(),
                    PrvGrndTot = Convert.ToDecimal(EODPRVGRNTOT.ToDecimalPlaces(2)),
                    CurrEODcntr = EODCRRCNTR.ToSafeInteger(),
                    CurrGrndTot = Convert.ToDecimal(EODCRRGRNTOT.ToDecimalPlaces(2)),
                    TransDate = EODTRNSDATE,
                    Novelty = Convert.ToDecimal(EODNOV.ToDecimalPlaces(2)),
                    Misc = Convert.ToDecimal(EODMISC.ToDecimalPlaces(2)),
                    LocTax = Convert.ToDecimal(EODLOCTAX.ToDecimalPlaces(2)),
                    TotCrdSls = Convert.ToDecimal(EODTOTCRDSLS.ToDecimalPlaces(2)),
                    TotCrdVat = Convert.ToDecimal(EODTOTCRDVAT.ToDecimalPlaces(2)),
                    TotNonVat = Convert.ToDecimal(EODTOTNONVAT.ToDecimalPlaces(2)),
                    PhrmSls = Convert.ToDecimal(EODPHRMSLS.ToDecimalPlaces(2)),
                    NonPhrmSls = Convert.ToDecimal(EODNONPHRMSLS.ToDecimalPlaces(2)),
                    DisabDisc = Convert.ToDecimal(EODDISABLEDISC.ToDecimalPlaces(2)),
                    PercRent = Convert.ToDecimal(EODPERCRENT.ToDecimalPlaces(2)),
                    TotAmntRePrnt = Convert.ToDecimal(EODTOTAMNTREPRNT.ToDecimalPlaces(2)),
                    NoRePrnt = EODNOREPRNT.ToSafeInteger()
                };

                eodSLSDataStorage.Add(EODSLS, false);

                string lastFourDigits = EODTENTID.Substring(EODTENTID.Length - 4);
                string lastTransac = yyyymmdd.Substring(yyyymmdd.Length - 4);
                string folderPath = dir;
                string fileName = lastFourDigits + lastTransac + "." + EODTERMNO;

                List<string> filePaths = new List<string>();
                GenerateFixedWidthFile(EODSLS, folderPath, fileName, filePaths);

                int batchSize = 5;
                for (int i = 0; i < filePaths.Count; i += batchSize)
                {
                    List<string> batchFiles = filePaths.GetRange(i, Math.Min(batchSize, filePaths.Count - i));
                    SendFilesInBatch(batchFiles);
                }

                CheckFileSendingStatus(); //comment muna 04072025
                return true;
            }
            catch (Exception ex)
            {

                ErrorLogger.LogError(ex.Message.ToString() + " || " + ex.StackTrace.ToString());
                if (!silentMode)
                {
                    MessageBox.Show($"Error: {ex.Message}", "EOD Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return false;
            }
        }
        #endregion

        #region Generate EOD Textfile
        private void GenerateFixedWidthFile(EODSLSData eodData, string folderPath, string fileName, List<string> filePaths)
        {
            string batchDate = DateTime.Today.ToString("MMM-dd-yyyy");
            string batchTime = DateTime.Now.ToString("hh:mm:ss tt");

            int batchCount = 0;
            string status = "unsent";

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            DataTable checkBatchFileName = batchfileDataStorage.CheckBatchFileName(); // Get current batch records

            int latestBatchCount = 0;

            // Loop through filenames to find the highest batch count
            foreach (DataRow row in checkBatchFileName.Rows)
            {
                string existingFileName = row["FileName"].ToString(); // e.g., '67890412.0112'

                if (existingFileName.StartsWith(fileName)) // Match prefix
                {
                    string suffix = existingFileName.Substring(fileName.Length); // Get the count portion
                    if (int.TryParse(suffix, out int parsedCount))
                    {
                        if (parsedCount > latestBatchCount)
                            latestBatchCount = parsedCount;
                    }
                }
            }

            batchCount = latestBatchCount + 1; // Determine next batch count
            GlobalVar.batchcount = batchCount;

            // Save batch file info
            var batchfile = new BatchFile
            {
                FileName = fileName,
                TransDate = batchDate,
                TransTime = batchTime,
                Count = batchCount,
                Status = status
            };
            //batchfileDataStorage.Add(batchfile, false);

            // Generate the actual output file
            string fullFilePath = Path.Combine(folderPath, fileName + batchCount.ToString());

            using (StreamWriter writer = new StreamWriter(fullFilePath))
            {
                const int totalWidth = 18;
                string input = eodData.TransDate.ToString(); // MMddyyyy format
                DateTime date = DateTime.ParseExact(input, "MMddyyyy", null);
                string formattedDate = date.ToString("MM/dd/yyyy");

                writer.WriteLine($"01{eodData.TenantID.PadLeft(totalWidth - 2, '0')}");
                writer.WriteLine($"02{eodData.TermNo.PadLeft(totalWidth - 2, '0')}");
                writer.WriteLine($"03{eodData.Gross.ToString("0.00").PadLeft(totalWidth - 2, '0')}");
                writer.WriteLine($"04{eodData.TotVat.ToString("0.00").PadLeft(totalWidth - 2, '0')}");
                writer.WriteLine($"05{eodData.TotVoid.ToString("0.00").PadLeft(totalWidth - 2, '0')}");
                writer.WriteLine($"06{eodData.NoVoid.ToString().PadLeft(totalWidth - 2, '0')}");
                writer.WriteLine($"07{eodData.TotDisc.ToString("0.00").PadLeft(totalWidth - 2, '0')}");
                writer.WriteLine($"08{eodData.NoDisc.ToString().PadLeft(totalWidth - 2, '0')}");
                writer.WriteLine($"09{eodData.TotRfnd.ToString("0.00").PadLeft(totalWidth - 2, '0')}");
                writer.WriteLine($"10{eodData.NoRfnd.ToString().PadLeft(totalWidth - 2, '0')}");
                writer.WriteLine($"11{eodData.OthrNegaAdjst.ToString("0.00").PadLeft(totalWidth - 2, '0')}");
                writer.WriteLine($"12{eodData.NoNegaAdjst.ToString().PadLeft(totalWidth - 2, '0')}");
                writer.WriteLine($"13{eodData.TotSrvChrg.ToString("0.00").PadLeft(totalWidth - 2, '0')}");
                writer.WriteLine($"14{eodData.PrvEODcntr.ToString().PadLeft(totalWidth - 2, '0')}");
                writer.WriteLine($"15{eodData.PrvGrndTot.ToString("0.00").PadLeft(totalWidth - 2, '0')}");
                writer.WriteLine($"16{eodData.CurrEODcntr.ToString().PadLeft(totalWidth - 2, '0')}");
                writer.WriteLine($"17{eodData.CurrGrndTot.ToString("0.00").PadLeft(totalWidth - 2, '0')}");
                writer.WriteLine($"18{formattedDate.PadLeft(totalWidth - 2, '0')}");
                writer.WriteLine($"19{eodData.Novelty.ToString("0.00").PadLeft(totalWidth - 2, '0')}");
                writer.WriteLine($"20{eodData.Misc.ToString("0.00").PadLeft(totalWidth - 2, '0')}");
                writer.WriteLine($"21{eodData.LocTax.ToString("0.00").PadLeft(totalWidth - 2, '0')}");
                writer.WriteLine($"22{eodData.TotCrdSls.ToString("0.00").PadLeft(totalWidth - 2, '0')}");
                writer.WriteLine($"23{eodData.TotCrdVat.ToString("0.00").PadLeft(totalWidth - 2, '0')}");
                writer.WriteLine($"24{eodData.TotNonVat.ToString("0.00").PadLeft(totalWidth - 2, '0')}");
                writer.WriteLine($"25{eodData.PhrmSls.ToString("0.00").PadLeft(totalWidth - 2, '0')}");
                writer.WriteLine($"26{eodData.NonPhrmSls.ToString("0.00").PadLeft(totalWidth - 2, '0')}");
                writer.WriteLine($"27{eodData.DisabDisc.ToString("0.00").PadLeft(totalWidth - 2, '0')}");
                writer.WriteLine($"28{eodData.PercRent.ToString("0.00").PadLeft(totalWidth - 2, '0')}");
                writer.WriteLine($"29{eodData.TotAmntRePrnt.ToString("0.00").PadLeft(totalWidth - 2, '0')}");
                writer.WriteLine($"30{eodData.NoRePrnt.ToString().PadLeft(totalWidth - 2, '0')}");
            }

            filePaths.Add(fullFilePath);
        }


        #endregion


        private void frmMallLinking_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (notifyIcon1 != null)
            {
                notifyIcon1.Visible = false;
                notifyIcon1.Dispose();
            }
        }

        private void frmMallLinking_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
                {
                    this.Hide();
                    notifyIcon1.Visible = true;
                }
        }

        private void SendFilesInBatch(List<string> filePaths)
        {
            foreach (string filePath in filePaths) //comment muna 04072025
            {
                SendSalesFileToServer(filePath);
            }
        }
        private void SendSalesFileToServer(string filepath)
        {
            // Simulate server communication (replace with actual logic)
            try
            {
                if (sftp2 != null)
                {
                    sftp2.SftpTrans(filepath, host, username, password);
                }
                else
                {
                    MessageBox.Show("sftp2 is null");
                    return;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex.Message.ToString() + " || " + ex.StackTrace.ToString());
            }
        }
        private void SendSalesFileToServer2(string filepath)
        {
            // Simulate server communication (replace with actual logic)
            try
            {
                if (sftp2 != null)
                {
                    sftp2.ResendUnsentFile(filepath, host, username, password);
                }
                else
                {
                   // MessageBox.Show("sftp2 is null");
                    return;
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex.Message.ToString() + " || " + ex.StackTrace.ToString());
            }
        }
        #endregion
        private void CheckFileSendingStatus()
        {
            //if (GlobalVar.SuccessSending)
            //{
            //    _notifyIcon.ShowBalloonTip(300, "Mall Linking", "Sale file Successfully sent to RLC server", ToolTipIcon.Info);
            //    this.Close();
            //}
            //else if (GlobalVar.SuccessSending2 && !GlobalVar.SuccessSending)
            //{
            //    _notifyIcon.ShowBalloonTip(300, "Mall Linking", "Trying to send unsent files...Successfully", ToolTipIcon.Info);
            //}
            //else
            //{
            //    _notifyIcon.ShowBalloonTip(300, "Mall Linking", "Sales file is not sent to RLC server. Please contact your POS vendor", ToolTipIcon.Error);
            //    //SftpConnectionChecker.StartChecking();
            //}

            if (GlobalVar.fromUnsent)
            {
                if (GlobalVar.SuccessSending2)
                {
                    MessageBox.Show("Trying to send unsent files... Successfully.", "Mall Linking", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Unsent sales file is not sent to RLC server. Please contact your POS vendor.", "Mall Linking", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //return;
                    //Application.Restart();
                    //System.Diagnostics.Process.Start(Application.ExecutablePath);
                }
            }
            else
            {
                if (GlobalVar.SuccessSending)
                {
                    MessageBox.Show("Sale file successfully sent to RLC server.", "Mall Linking", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    //this.Close();
                }
                //else if (GlobalVar.SuccessSending2 && !GlobalVar.SuccessSending)
                //{
                //    MessageBox.Show("Trying to send unsent files... Successfully.", "Mall Linking", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //}
                else
                {
                    MessageBox.Show("Sales file is not sent to RLC server. Please contact your POS vendor.", "Mall Linking", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //SftpConnectionChecker.StartChecking();
                    //Application.Restart();
                    //System.Diagnostics.Process.Start(Application.ExecutablePath);
                }
            }

            // Optional: Reset flags here if needed
            GlobalVar.SuccessSending = false;
            GlobalVar.SuccessSending2 = false;
            GlobalVar.fromUnsent = false;
            GlobalVar.ErrorSending = false;
        }




        #region Transaction Logic
        public void checkEODpreviousDates()
        {

            DataTable ACCdate = transactionDataStorage.GetDataAccDate();
            if (ACCdate != null && ACCdate.Rows.Count > 0)
            {
                DateTime now = DateTime.Now;
                string dateToday = now.ToString("MMddyyyy");
                string dateYesterday = now.AddDays(-1).ToString("MMddyyyy");

                string dateToday_MMddyyyy = dateToday; // Default to today
                string dateYesterday_MMddyyyy = dateYesterday;
                string dateToday_yyyyMMdd = now.ToString("yyyyMMdd");
                string dateYesterday_yyyyMMdd = now.AddDays(-1).ToString("yyyyMMdd");

                bool isAllowedDate = now.TimeOfDay < new TimeSpan(4, 0, 0);
                if (isAllowedDate)
                {
                    dateToday_MMddyyyy = now.AddDays(-1).ToString("MMddyyyy"); // Corrected
                    dateYesterday_MMddyyyy = now.AddDays(-2).ToString("MMddyyyy");

                    dateToday_yyyyMMdd = now.AddDays(-1).ToString("yyyyMMdd"); // Corrected
                    dateYesterday_yyyyMMdd = now.AddDays(-2).ToString("yyyyMMdd");
                }
                string summaryMessage = "";

                foreach (DataRow row in ACCdate.Rows)
                {
                    string LAST_DATE_YYYYMMDD = row["TranscDate"].ToString();

                    // Convert to MMddyyyy
                    DateTime parsedDate = DateTime.ParseExact(LAST_DATE_YYYYMMDD, "yyyyMMdd", CultureInfo.InvariantCulture);
                    LAST_DATE_MMDDYYYY = parsedDate.ToString("MMddyyyy");

                    // Also get yesterday of LAST_DATE_MMDDYYYY (in MMddyyyy format)
                    YESTERDAY_MMDDYYYY = parsedDate.AddDays(-1).ToString("MMddyyyy");
                }

                // ✅ All variables now use MMddyyyy format
                TotalRecords = GetCountFromTable(eodSLSDataStorage.CheckEODtable(), "CountEOD");
                EODToday1 = GetCountFromTable(eodSLSDataStorage.SelectEOD("eodslsdata", "TransDate", dateToday_MMddyyyy), "COUNT(*)");
                EODYesterday1 = GetCountFromTable(eodSLSDataStorage.SelectEOD("eodslsdata", "TransDate", dateYesterday_MMddyyyy), "COUNT(*)");
                perTrans1 = GetCountFromTable(eodSLSDataStorage.SelectPERTRANS("orderdata", "AccDate", dateToday_yyyyMMdd), "COUNT(*)");
                perTransYestday1 = GetCountFromTable(eodSLSDataStorage.SelectPERTRANS("orderdata", "AccDate", dateYesterday_yyyyMMdd), "COUNT(*)");

                try
                {
                    if (EODYesterday1 != 0 && perTrans1 != 0 && EODToday1 == 0)
                    {
                        DialogResult result = MessageBox.Show(
                            "Sales Detected. Do you want to perform End of Day?",
                            "EOD Check",
                            MessageBoxButtons.OKCancel,
                            MessageBoxIcon.Information
                        );

                        if (result != DialogResult.OK)
                        {
                            return;
                        }
                        RunEODRange("Manual");
                    }
                }
                catch (Exception ex)
                {
                    //summaryMessage += $"❌ An unexpected error occurred during Auto EOD:\n{ex.Message}\n";

                    ErrorLogger.LogError(ex.Message.ToString() + " || " + ex.StackTrace.ToString());
                }

                // One summary MessageBox at the end (or you can just log this instead if needed)
                if (!string.IsNullOrEmpty(summaryMessage))
                {
                    //MessageBox.Show(summaryMessage.Trim(), "EOD Summary", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }


        }

        public void checkEODpreviousDates_Auto()
        {
            RunEODRange("Auto");

        }


        private void RunEODRange(string source = "Manual")
        {
            try
            {
                string lastData;
                bool isFromEODTable;

                CheckSalesData(out lastData, out isFromEODTable);

                if (!string.IsNullOrWhiteSpace(lastData))
                {
                    DateTime startDate = DateTime.ParseExact(lastData, "MMddyyyy", CultureInfo.InvariantCulture);

                    // Advance if from EOD table
                    if (isFromEODTable)
                        startDate = startDate.AddDays(1);

                    DateTime systemToday = DateTime.Now.Date;
                    DateTime lastTransactionDate = GetLastTransactionDate();

                    DateTime endDate = (lastTransactionDate < systemToday) ? systemToday.AddDays(-1) : systemToday;

                    if (endDate < startDate)
                    {
                        if (source == "Manual")
                            MessageBox.Show("EOD is already up to date. No pending days to process.", "Manual EOD", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    string strStartDate = startDate.ToString("MMddyyyy");
                    string strEndDate = endDate.ToString("MMddyyyy");

                    convertFormatDate(strStartDate, strEndDate, silent: source == "Auto");

                    //if (source == "Auto")
                    //{
                    //    ShowWarningOnce("✔ Automatic End of Day has been performed successfully.", "Auto EOD");
                    //}
                }
                else
                {
                    //MessageBox.Show("Unable to determine starting EOD date.", "EOD Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"[{source} EOD Error]: {ex.Message}", "EOD Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                ErrorLogger.LogError(ex.Message.ToString() + " || " + ex.StackTrace.ToString());
            }
        }

        private int GetCountFromTable(DataTable table, string columnName)
        {
            foreach (DataRow row in table.Rows)
            {
                if (int.TryParse(row[columnName].ToString(), out int count))
                    return count;
            }
            return 0;
        }


        public void CheckSalesData(out string lastData, out bool isFromEODTable)
        {
            lastData = string.Empty;
            isFromEODTable = false;

            // Try to get the last successful EOD date
            DataTable ACCdate = eodSLSDataStorage.GetDataEodDate();

            if (ACCdate != null && ACCdate.Rows.Count > 0)
            {
                DataRow row = ACCdate.Rows[0];
                string lastDateRaw = row["TransDate"].ToString();

                if (!string.IsNullOrWhiteSpace(lastDateRaw))
                {
                    try
                    {
                        DateTime parsedDate = DateTime.ParseExact(lastDateRaw, "MMddyyyy", CultureInfo.InvariantCulture);
                        lastData = parsedDate.ToString("MMddyyyy");
                        isFromEODTable = true; // ✅ Confirm source is EOD table
                        return;
                    }
                    catch (FormatException)
                    {
                        // fallback continues below
                        lastData = string.Empty;
                    }
                }
            }

            // Fallback: Try to get the last transaction date
            DataTable transDateTable = transactionDataStorage.GetDataAccDate();
            if (transDateTable != null && transDateTable.Rows.Count > 0)
            {
                DataRow row = transDateTable.Rows[0];
                string transDateRaw = row["TranscDate"].ToString();

                if (!string.IsNullOrWhiteSpace(transDateRaw))
                {
                    try
                    {
                        DateTime parsedDate = DateTime.ParseExact(transDateRaw, "yyyyMMdd", CultureInfo.InvariantCulture);
                        lastData = parsedDate.ToString("MMddyyyy");
                        isFromEODTable = false; // ✅ Source is transaction table fallback
                    }
                    catch (FormatException)
                    {
                        lastData = string.Empty;
                    }
                }
            }
        }


        private DateTime GetLastTransactionDate()
        {
            DataTable transDateTable = transactionDataStorage.GetDataAccDate();

            if (transDateTable != null && transDateTable.Rows.Count > 0)
            {
                string transDateRaw = transDateTable.Rows[0]["TranscDate"].ToString(); // in yyyyMMdd
                if (!string.IsNullOrWhiteSpace(transDateRaw))
                {
                    try
                    {
                        return DateTime.ParseExact(transDateRaw, "yyyyMMdd", CultureInfo.InvariantCulture).Date;
                    }
                    catch (FormatException)
                    {
                        // fallback to system date if parse fails
                    }
                }
            }

            return DateTime.Now.AddDays(-1).Date; // Fallback if no data
        }

        public void convertFormatDate(string startDate, string endDate, bool silent = true)
        {
            if (string.IsNullOrWhiteSpace(dir))
            {
                dir = GlobalVar.DIRECTORY;
            }

            if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
            {
                ErrorLogger.LogError($"The system cannot find the path: '{dir}'. EOD process cancelled.");
                if (!silent)
                {
                    MessageBox.Show($"The system cannot find the path: '{dir}'\n\nEOD process has been cancelled.", "EOD Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return;
            }

            // Create and show the loading form
            frmRobProcess loadingForm = new frmRobProcess();
            loadingForm.Show();
            loadingForm.Refresh(); // Ensures the form is drawn immediately

            bool allSucceeded = true;

            try
            {
                DateTime start = DateTime.ParseExact(startDate, "MMddyyyy", null);
                DateTime end = DateTime.ParseExact(endDate, "MMddyyyy", null);

                for (DateTime date = start; date <= end; date = date.AddDays(1))
                {
                    string mmddyyyy = date.ToString("MMddyyyy");
                    string yyyymmdd = date.ToString("yyyyMMdd");
                    string yesterday = date.AddDays(-1).ToString("yyyyMMdd");

                    bool success = PerformEOD(mmddyyyy, yyyymmdd, yesterday, silentMode: silent);
                    if (!success)
                    {
                        allSucceeded = false;
                        break;
                    }
                }

                if (allSucceeded && !silent)
                {
                    MessageBox.Show("EOD Process Complete.", "EOD Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Error: {ex.Message}\n{ex.StackTrace}", "convertFormatDate Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                ErrorLogger.LogError(ex.Message.ToString() + " || " + ex.StackTrace.ToString());
            }
            finally
            {
                // Close the loading form after process completion
                loadingForm.Close();
            }
        }

        //public void convertFormatDate(string startDate, string endDate) 
        //{
        //    try
        //    {
        //        DateTime start = DateTime.ParseExact(startDate, "MMddyyyy", null);
        //        DateTime end = DateTime.ParseExact(endDate, "MMddyyyy", null);

        //        for (DateTime date = start; date <= end; date = date.AddDays(1))
        //        {

        //            string mmddyyyy = date.ToString("MMddyyyy");
        //            string yyyymmdd = date.ToString("yyyyMMdd");
        //            string yesterday = date.AddDays(-1).ToString("yyyyMMdd");

        //            PerformEOD(mmddyyyy, yyyymmdd, yesterday, silentMode: true);
        //        }

        //        //MessageBox.Show("EOD operations completed for the date range.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Error: {ex.Message}\n{ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }

        //    //PerformEOD(LAST_DATE_MMDDYYYY, LAST_DATE_YYYYMMDD, YESTERDAY_MMDDYYYY);
        //}
        #endregion
    }
}