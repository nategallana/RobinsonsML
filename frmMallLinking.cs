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
//using RobinsonMallLinkingWBOX.ModelsRequest;

namespace RobinsonMallLinkingWBOX
{
    public partial class frmMallLinking : Form
    {

        #region --
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
        public string xml, host, username, password, dir, tenantname, terminal = null;
        //private System.Windows.Forms.Timer scenarioTimer;
        private System.Windows.Forms.Timer workerTimer;
        private System.Timers.Timer eodTimer;
        private System.Windows.Forms.Timer appCloseTimer;
        private bool isEODTriggered = false;
        private readonly object eodLock = new object();
        private bool isAutoEODRunning = false;
        DateTime lastAutoEODRun = DateTime.MinValue; // Put this as a class-level field
        private bool hasShownEODMissingWarning = false;
        private bool hasShownRestrictedTimeWarning = false;
        public int TotalRecords, EODToday1, EODYesterday1, perTrans1, perTransYestday1;
        #endregion

        #region EODVariables
        public string EODTENTID;
        public string EODTERMNO;
        public decimal EODGROSS;
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
        private bool isMessageBoxShown = false;
        //public ExternalTransactionRequest request = null;
        //private BackgroundWorker backgroundWorker;
        FileSystemWatcher watcher;



        public frmMallLinking()
        {
            InitializeComponent();
            GlobalVar.InitializeConfiguration();

            xml = GlobalVar.XMLPath;
            host = GlobalVar.HOST;
            username = GlobalVar.USERNAME;
            password = GlobalVar.PASSWORD;
            dir = GlobalVar.DIRECTORY;
            tenantname = GlobalVar.TENANTNAME;
            terminal = GlobalVar.TERMINALNO;

            InitializeEODHandler();
            InitializeAppCloseHandler();
            InitializeFolder();

            watcher = new FileSystemWatcher();
            watcher.Path = xml;
            watcher.EnableRaisingEvents = true;
            watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
            watcher.Filter = "*.xml";
            watcher.Created += new FileSystemEventHandler(OnChanged);

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
        private void Form1_Load(object sender, EventArgs e)
        {
            try
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

                sftpConnectionChecker.StartChecking(); // Keep this if you're using SFTP monitoring

                // No more backgroundWorker or workerTimer needed here!

                BackupXML(xml); // Keep this if still needed
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred during form load:\n{ex.Message}", "Check Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

        #region Initialize EOD Timer
        private void InitializeEODHandler()
        {
            eodTimer = new System.Timers.Timer(60000); // Check every minute
            eodTimer.Elapsed += CheckEODTrigger;
            eodTimer.Start();
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
            if (IsProcessRunning("notepad"))
            {
                try
                {
                    RunScenarios("notepad");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error in RunScenarios: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // Optional: Stop the timer or log error if needed
                }
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
                    MessageBox.Show($"Error processing file '{xmlFile}': {ex.Message}. \n{ex.StackTrace}");
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
                MessageBox.Show($"Error in ProcessNewXML: {ex.Message}\n{ex.StackTrace}");
            }
        }

        #endregion
        #region buttons
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you want to close this application?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                notifyIcon1.Dispose();
                Environment.Exit(0);
            }
        }

        private void configurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if the form is null or has been disposed
            if (settings == null || settings.IsDisposed)
            {
                // If so, create a new instance and show it
                settings = new Settings();
                settings.Show();
            }
            else
            {
                // If it's already open, bring it to the front
                settings.BringToFront();
            }
        }

        private void viewDataSentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form _frm = new FileView();
            _frm.Show();
        }

        //private void eODToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    //Scenarios();
        //    //checkEODpreviousDates();

        //    //string lastData;
        //    //CheckSalesData(out lastData);

        //    //DateTime now = DateTime.Now;

        //    //DateTime sDate = DateTime.Now.AddDays(-1);

        //    //string startDate = lastData;
        //    //string endDate = now.ToString("MMddyyyy");

        //    //convertFormatDate(startDate, endDate);

        //    string lastData;
        //    CheckSalesData(out lastData); // gets last EOD date in MMddyyyy format

        //    DateTime startDate = DateTime.ParseExact(lastData, "MMddyyyy", null);
        //    DateTime endDate = DateTime.Now.AddDays(-1); // up to yesterday

        //    if (startDate > endDate)
        //    {
        //        MessageBox.Show("All EODs are already up to date.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //        return;
        //    }

        //    // You may call convertFormatDate OR PerformEOD loop directly
        //    for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
        //    {
        //        string mmddyyyy = date.ToString("MMddyyyy");
        //        string yyyymmdd = date.ToString("yyyyMMdd");
        //        string yesterday = date.AddDays(-1).ToString("yyyyMMdd");

        //        PerformEOD(mmddyyyy, yyyymmdd, yesterday);
        //    }

        //    MessageBox.Show("EOD operations completed.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

        //}

        private void eODToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string lastData;
                bool isFromEODTable;

                // 📌 Check latest EOD or transaction date
                CheckSalesData(out lastData, out isFromEODTable);

                if (!string.IsNullOrWhiteSpace(lastData))
                {
                    DateTime startDate = DateTime.ParseExact(lastData, "MMddyyyy", CultureInfo.InvariantCulture);

                    // ➕ Move startDate forward if it's from EOD table
                    if (isFromEODTable)
                        startDate = startDate.AddDays(1);

                    // 📆 Get system date (today)
                    DateTime systemToday = DateTime.Now.Date;

                    // 📅 Get last transaction date (from TranscDate yyyyMMdd)
                    DateTime lastTransactionDate = GetLastTransactionDate();

                    // ✅ Determine end date logic
                    DateTime endDate;

                    if (lastTransactionDate < systemToday)
                    {
                        // 👉 No sales today → End date = yesterday
                        endDate = systemToday.AddDays(-1);
                    }
                    else
                    {
                        // 👉 Sales today → End date = system date
                        endDate = systemToday;
                    }

                    // 🔒 Ensure endDate is not before startDate
                    if (endDate < startDate)
                    {
                        MessageBox.Show("EOD is already up to date. No pending days to process.", "Manual EOD", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    // 🔁 Call convertFormatDate
                    string strStartDate = startDate.ToString("MMddyyyy");
                    string strEndDate = endDate.ToString("MMddyyyy");

                    convertFormatDate(strStartDate, strEndDate);
                }
                else
                {
                    MessageBox.Show("Unable to determine starting EOD date.", "EOD Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error occurred during manual EOD: {ex.Message}", "EOD Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void dateRangeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form _frm = new frmDateRange();
            _frm.Show();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

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
            if (isMessageBoxShown || isAutoEODRunning) return;

            string lastData;
            bool isFromEODTable; // <-- added to match the new method signature
            CheckSalesData(out lastData, out isFromEODTable);

            DateTime now = DateTime.Now;
            if (!DateTime.TryParseExact(lastData, "MMddyyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime lastDataDate))
            {
                if (!targetProcessName.Equals("notepad", StringComparison.OrdinalIgnoreCase))
                {
                    ShowWarningOnce("Invalid or missing Business Date. Proceeding with system date.", "Date Parsing Warning");
                }
                return;
            }

            string dateYesterday = now.AddDays(-1).ToString("yyyyMMdd");
            DataTable EODYesterday = eodSLSDataStorage.SelectEOD("eodslsdata", "TransDate", dateYesterday);
            foreach (DataRow row in EODYesterday.Rows)
            {
                EODYesterday1 = int.Parse(row["COUNT(*)"].ToString());
            }

            bool isAllowedEarlyTime = now.TimeOfDay < new TimeSpan(4, 0, 0);
            bool isTimeToAutoEOD = now.TimeOfDay >= new TimeSpan(4, 0, 0);
            bool isRestrictedTime = now.TimeOfDay >= new TimeSpan(4, 0, 0) && now.TimeOfDay < new TimeSpan(9, 0, 0);
            bool isDifferentDate = lastDataDate.Date != now.Date;
            bool isEODMissing = EODYesterday1 == 0;

            if (isAllowedEarlyTime)
            {
                return;
            }
            else if (isTimeToAutoEOD && isEODMissing && lastAutoEODRun.Date != now.Date)
            {
                isAutoEODRunning = true;
                appCloseTimer.Stop();
                ShowWarningOnce("Automatic End of Day is being processed.", "End of Day");
                lastAutoEODRun = now;
                checkEODpreviousDates_Auto();

                isAutoEODRunning = false;
                appCloseTimer.Start();

                //ShowWarningOnce("Automatic End of Day has been processed.", "End of Day");
                return;
            }
            else if (now.TimeOfDay >= new TimeSpan(9, 0, 0) && isDifferentDate && isEODMissing && !hasShownEODMissingWarning)
            {
                hasShownEODMissingWarning = true;
                ShowWarningOnce($"No EOD record found for yesterday.\nPlease complete EOD first. Last Business Date was '{lastData}'", "Missing EOD");
                return;
            }
            else if (isRestrictedTime && !isAutoEODRunning && !hasShownRestrictedTimeWarning)
            {
                hasShownRestrictedTimeWarning = true;
                ShowWarningOnce("Transactions are not allowed between 4:00 AM and 9:00 AM.", "Transaction Restricted");
                return;
            }
        }



        //private void RunScenarios(string targetProcessName)
        //{
        //    if (isMessageBoxShown) return;

        //    string lastData;
        //    CheckSalesData(out lastData);

        //    DateTime now = DateTime.Now;
        //    DateTime lastDataDate;
        //    bool isValidBusinessDate = DateTime.TryParseExact(lastData, "MMddyyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out lastDataDate);

        //    //Allow notepad to proceed silently if business date is invalid / missing
        //    if (!isValidBusinessDate)
        //    {
        //        if (targetProcessName.Equals("notepad", StringComparison.OrdinalIgnoreCase))
        //        {
        //            // Proceed silently for notepad
        //            return;
        //        }
        //        else
        //        {
        //            // For other processes, still show message and allow normal flow
        //            ShowWarningAndClose($"Invalid or missing Business Date: '{lastData ?? "null"}'. Proceeding with system date.", "Date Parsing Warning");
        //            return;
        //        }
        //    }

        //    // Load EOD check data here (ensure EODYesterday1 gets value)
        //    string dateYesterday = now.AddDays(-1).ToString("MMddyyyy");
        //    string dateYesterday1 = now.AddDays(-1).ToString("yyyyMMdd");
        //    DataTable EODYesterday = eodSLSDataStorage.SelectEOD("eodslsdata", "TransDate", dateYesterday1);
        //    foreach (DataRow row in EODYesterday.Rows)
        //    {
        //        EODYesterday1 = int.Parse(row["COUNT(*)"].ToString());
        //    }

        //    bool isDifferentDate = lastDataDate.Date != now.Date;
        //    bool isRestrictedTime = now.TimeOfDay > new TimeSpan(4, 1, 0) && now.TimeOfDay < new TimeSpan(8, 59, 0);
        //    bool isAllowedEarlyTime = now.TimeOfDay >= new TimeSpan(0, 0, 0) && now.TimeOfDay < new TimeSpan(3, 59, 0);
        //    bool isEODMissing = now.TimeOfDay >= new TimeSpan(9, 0, 0) && EODYesterday1 == 0;
        //    bool isTimeToAutoEOD = now.Hour == 4 && now.Minute == 0 && !eodTriggeredToday && EODYesterday1 == 0;

        //    if (isAllowedEarlyTime)
        //        return;

        //    if (isDifferentDate)
        //    {
        //        if (isEODMissing)
        //        {
        //            ShowWarningAndClose($"No EOD record found for yesterday. Please complete EOD for yesterday before proceeding. Last Business Date was '{lastData}'", "Missing EOD");
        //        }
        //        else if (isAutomatedEOD)
        //        {
        //            checkEODpreviousDates();
        //            ShowWarningAndClose("Automatic End of Day has been processed.", "End of Day");
        //        }
        //        else if (isRestrictedTime)
        //        {
        //            ShowWarningAndClose("Transactions are not allowed between 4:00 AM and 9:00 AM.", "Transaction Restricted");
        //        }
        //        //else
        //        //{
        //        //    ShowWarningAndClose($"Different system date detected. Business Date was '{lastData}'", "Transaction Restricted");
        //        //}
        //    }
        //    else // lastData == today
        //    {
        //        if (isRestrictedTime)
        //        {
        //            ShowWarningAndClose("Transactions are not allowed between 4:00 AM and 9:00 AM.", "Transaction Restricted");
        //        }
        //        //else if (isEODMissing)
        //        //{
        //        //    ShowWarningAndClose("No EOD record found for yesterday. Please complete EOD for yesterday before proceeding.", "Missing EOD");
        //        //}
        //    }
        //}

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
        private void CheckTimeAndCloseApp(object sender, EventArgs e)
        {
            //Debug.WriteLine($"Timer ticked at {DateTime.Now}");

            string[] targetProcesses = { "notepad", "WirelessLink", "wBoxBridge" };

            foreach (var processName in targetProcesses)
            {
                if (IsProcessRunning(processName))
                {
                    //Debug.WriteLine($"{processName} is running, calling RunScenarios.");
                    this.Invoke((MethodInvoker)(() => RunScenarios(processName)));
                }
            }
        }
        private bool IsProcessRunning(string processName)
        {
            return Process.GetProcessesByName(processName).Any();
        }

        private void CloseOtherAppIfRunning()
        {
            string[] allProcessesToKill = { "notepad", "WirelessLink", "wBoxBridge" };
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
        public void PerformEOD(string mmddyyyy, string yyyymmdd, string yesterday, bool silentMode = false)
        {
            InitializeEODValues();

            EODTENTID = tenantname;
            EODTERMNO = terminal;
            EODTRNSDATE = mmddyyyy;

            #region Discount
            DataTable discTABLE = transactionDataStorage.GetDISCOUNT(yyyymmdd);
            foreach (DataRow row in discTABLE.Rows)
            {
                EODTOTDISC = row["DiscountValue"].ToSafeDecimal();
                EODNODISC = row["CountDiscount"].ToSafeInteger();
            }
            #endregion

            #region Void
            DataTable voidTABLE = voidDataStorage.GetVOID(yyyymmdd);
            foreach (DataRow row in voidTABLE.Rows)
            {
                EODTOTVOID = row["TotalVoid"].ToSafeDecimal();
                EODNOVOID = row["CountVoid"].ToSafeInteger();
            }
            #endregion

            #region GROSS VATTOTAL TOTALSERVCHARGE
            DataTable ALLdatas = transactionDataStorage.GetALLDATAS(yyyymmdd);
            foreach (DataRow row in ALLdatas.Rows)
            {
                EODGROSS = row["Subtotal"].ToSafeDecimal(); // done
                EODTOTVAT = row["TaxAmount"].ToSafeDecimal(); // done
                //EODTOTVOID = row["TaxAmount"].ToSafeDecimal(); // done
                //EODNOVOID = row["TaxAmount"].ToSafeInteger(); // done
                //EODTOTDISC = row["DiscountValue"].ToSafeDecimal(); // done
                //EODNODISC = row["TaxAmount"].ToSafeInteger(); // done
                //EODTOTRFD = row["TaxAmount"].ToSafeDecimal(); // no refund in wbox
                //EODNORFD = row["TaxAmount"].ToSafeInteger(); // no refund in wbox
                //EODOTHRNEGA = row["-"].ToSafeDecimal();
                //EODNOOTHRNEGA = row["-"].ToSafeInteger();
                EODTOTSRCHR = row["ServiceCharge"].ToSafeDecimal(); // done
                //EODPRVCNTR = row["TaxAmount"].ToSafeInteger(); // data yesterday // done
                //EODPRVGRNTOT = row["TaxAmount"].ToSafeDecimal(); // data yesterday
                //EODCRRCNTR = row[].ToSafeInteger(); // done
                //EODCRRGRNTOT = row["TaxAmount"].ToSafeDecimal(); // done
                //EODNOV = row["TaxAmount"].ToSafeDecimal();
                //EODMISC = row["TaxAmount"].ToSafeDecimal();
                //EODLOCTAX = row["TaxAmount"].ToSafeDecimal();
                //EODTOTCRDSLS = row["TaxAmount"].ToSafeDecimal();
                //EODTOTCRDVAT = row["TaxAmount"].ToSafeDecimal();
                //EODTOTNONVAT = row["TaxAmount"].ToSafeDecimal();
                //EODPHRMSLS = row["TaxAmount"].ToSafeDecimal();
                //EODNONPHRMSLS = row["TaxAmount"].ToSafeDecimal();
                //EODDISABLEDISC = row["TaxAmount"].ToSafeDecimal();
                //EODPERCRENT = row["TaxAmount"].ToSafeDecimal();
                //EODTOTAMNTREPRNT = row["TaxAmount"].ToSafeDecimal();
                //EODNOREPRNT = EODNOREPRNT + 1;

                EODTRNSDATE = DateTime.ParseExact(row["TranscDate"].ToString(), "yyyyMMdd", null).ToString("MMddyyyy");
            }
            #endregion

            #region Previous and Current Grand total
            DataTable yestData = transactionDataStorage.GetDataYesterday(yesterday);
            foreach (DataRow row in yestData.Rows)
            {
                EODPRVCNTR = row["CurrEODcntr"].ToSafeInteger();
                EODPRVGRNTOT = row["CurrGrndTot"].ToSafeDecimal();
            }

            EODCRRCNTR = EODPRVCNTR + 1;
            EODCRRGRNTOT = EODPRVGRNTOT + EODGROSS;
            #endregion

            try
            {
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

                CheckFileSendingStatus();

                // Optional Success Notification (only if not silent)
                if (!silentMode)
                {
                    MessageBox.Show("EOD Process Complete.", "EOD Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError($"EOD ERROR: {ex.Message}\n{ex.StackTrace}");
                if (!silentMode)
                {
                    MessageBox.Show($"Error: {ex.Message}", "EOD Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #region Generate EOD Textfile
        private void GenerateFixedWidthFile(EODSLSData eodData, string folderPath, string fileName, List<string>filePaths)
        {
            //SFTP sftpClient = new SFTP();
            string batchDate = DateTime.Today.ToString("MMM-dd-yyyy");
            string batchTime = DateTime.Now.ToString("hh:mm:ss tt");

            int batchCount = 0;
            string status = "unsent";

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }



            DataTable checkBatchFileName = batchfileDataStorage.CheckBatchFileName(); // Ensure this gets fresh data

            bool fileExists = false;
            int latestBatchCount = 0;

            // Get the latest batch count for the matching file
            var latestRow = checkBatchFileName.AsEnumerable()
                .Where(row => row["FileName"].ToString().Length >= 11 &&
                              row["FileName"].ToString().Substring(0, 11) == fileName) // Ensure correct file match
                .OrderByDescending(row => Convert.ToInt32(row["Count"])) // Get the highest count
                .FirstOrDefault();

            if (latestRow != null)
            {
                latestBatchCount = Convert.ToInt32(latestRow["Count"]);
            }

            // Check if the file already exists in the DataTable
            foreach (DataRow row in checkBatchFileName.Rows)
            {
                string existingFileName = row["FileName"].ToString(); // Example: '67890214.011'
                string extractedPart = existingFileName.Length >= 11 ? existingFileName.Substring(0, 11) : existingFileName;
                //int existingBatchCount = Convert.ToInt32(row["Count"]);

                if (extractedPart == fileName)
                {
                    fileExists = true;
                    batchCount = latestBatchCount + 1; // Use the latest count

                    break; // Exit the loop once the match is found
                }
            }




            // Create or update the batch file entry
            if (fileExists)
            {
                var batchfile = new BatchFile
                {
                    FileName = fileName,
                    TransDate = batchDate,
                    TransTime = batchTime,
                    Count = batchCount,
                    Status = status
                };
                GlobalVar.batchcount = batchCount;
                //batchfileDataStorage.Add(batchfile, false);
            }
            else
            {
                batchCount = 1;

                var batchfile = new BatchFile
                {
                    FileName = fileName,
                    TransDate = batchDate,
                    TransTime = batchTime,
                    Count = batchCount,
                    Status = status
                };
                GlobalVar.batchcount = batchCount;
                //batchfileDataStorage.Add(batchfile, false);
            }

            // Combine folder path and file name to get the full file path
            string fullFilePath = Path.Combine(folderPath, fileName + batchCount.ToString());

            using (StreamWriter writer = new StreamWriter(fullFilePath))
            {
                // Define the total width for the entire line (18 characters)
                const int totalWidth = 18;

                // Write each line with the field ID followed by the value, padded with zeros and formatted as needed
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
                //writer.WriteLine($"18{DateTime.Parse(eodData.TransDate).ToString("MM/dd/yy").PadLeft(totalWidth - 2, '0')}");
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
            //sftpClient.SftpTrans(fullFilePath, host, username, password);
        }
        #endregion
        private void SendFilesInBatch(List<string> filePaths)
        {
            foreach(string filePath in filePaths)
            {
                SendSalesFileToServer(filePath);
            }
        }
        private void SendSalesFileToServer(string filepath)
        {
            // Simulate server communication (replace with actual logic)
            try
            {
                if(sftp2 != null)
                {
                    sftp2.SftpTrans(filepath, host, username, password);
                }
                else
                {
                    MessageBox.Show("sftp2 is null");
                    return;
                }
            }
            catch(Exception ex)
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

            if (GlobalVar.SuccessSending)
            {
                MessageBox.Show("Sale file successfully sent to RLC server.", "Mall Linking", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            else if (GlobalVar.SuccessSending2 && !GlobalVar.SuccessSending)
            {
                MessageBox.Show("Trying to send unsent files... Successfully.", "Mall Linking", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Sales file is not sent to RLC server. Please contact your POS vendor.", "Mall Linking", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //SftpConnectionChecker.StartChecking();
            }
        }




        #region Transaction Logic


        public void checkEODpreviousDates()
        {
            DialogResult result = MessageBox.Show(
                "Do you want to perform End of Day?",
                "EOD Check",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Information
            );

            if (result != DialogResult.OK)
            {
                // User clicked Cancel - do not proceed
                return;
            }

            DataTable ACCdate = transactionDataStorage.GetDataAccDate();

            if (ACCdate != null && ACCdate.Rows.Count > 0)
            {
                string dateToday = DateTime.Now.ToString("yyyyMMdd");
                string dateYesterday = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");

                foreach (DataRow row in ACCdate.Rows)
                {
                    LAST_DATE_YYYYMMDD = row["TranscDate"].ToString();
                    DateTime parsedDate = DateTime.ParseExact(LAST_DATE_YYYYMMDD, "yyyyMMdd", CultureInfo.InvariantCulture);

                    LAST_DATE_MMDDYYYY = parsedDate.ToString("MMddyyyy");
                    LAST_DATE_YYYYMMDD = parsedDate.ToString("yyyyMMdd");

                    DateTime yesterday = DateTime.Now.AddDays(-1);
                    YESTERDAY_MMDDYYYY = yesterday.ToString("MMddyyyy");

                    DateTime yesterday1 = parsedDate.AddDays(-1);
                    YESTERDAY_YYYYMMDD = yesterday1.ToString("yyyyMMdd");
                }

                DataTable checkEODtable = eodSLSDataStorage.CheckEODtable();
                foreach (DataRow row in checkEODtable.Rows)
                {
                    TotalRecords = int.Parse(row["CountEOD"].ToString());
                }

                DataTable EODToday = eodSLSDataStorage.SelectEOD("eodslsdata", "TransDate", dateToday);
                foreach (DataRow row in EODToday.Rows)
                {
                    EODToday1 = int.Parse(row["COUNT(*)"].ToString());
                }

                DataTable EODYesterday = eodSLSDataStorage.SelectEOD("eodslsdata", "TransDate", dateYesterday);
                foreach (DataRow row in EODYesterday.Rows)
                {
                    EODYesterday1 = int.Parse(row["COUNT(*)"].ToString());
                }

                DataTable perTrans = eodSLSDataStorage.SelectPERTRANS("orderdata", "AccDate", dateToday);
                foreach (DataRow row in perTrans.Rows)
                {
                    perTrans1 = int.Parse(row["COUNT(*)"].ToString());
                }

                DataTable perTransYestday = eodSLSDataStorage.SelectPERTRANS("orderdata", "AccDate", dateYesterday);
                foreach (DataRow row in perTransYestday.Rows)
                {
                    perTransYestday1 = int.Parse(row["COUNT(*)"].ToString());
                }

                try
                {
                    #region EOD Scenarios Logic

                    // Scenario 1: Generate EOD for today if sales exist but no EOD yet
                    if (EODYesterday1 != 0 && perTrans1 != 0 && EODToday1 == 0)
                    {
                        convertFormatDate(LAST_DATE_MMDDYYYY, LAST_DATE_MMDDYYYY);
                    }
                    // Scenario 2: EOD already performed for today
                    else if (EODToday1 != 0)
                    {
                        MessageBox.Show("EOD has already been performed for today.", "EOD Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    // Scenario 4: No sales today - ask user if EOD should still proceed
                    else if (EODToday1 == 0 && perTrans1 == 0)
                    {
                        DialogResult result1 = MessageBox.Show(
                            "No sales for today. Do you still want to perform EOD for today?",
                            "EOD Confirmation",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question
                        );

                        if (result1 == DialogResult.Yes)
                        {
                            convertFormatDate(LAST_DATE_MMDDYYYY, LAST_DATE_MMDDYYYY);
                        }
                        else
                        {
                            return;
                        }
                    }
                    // Scenario 5: Fallback case - generate EOD using yesterday's date
                    else
                    {
                        convertFormatDate(LAST_DATE_MMDDYYYY, LAST_DATE_MMDDYYYY);
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An unexpected error occurred: {ex.Message}",
                                    "Error",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                }
            }
        }


        public void checkEODpreviousDates_Auto()
        {
            DataTable ACCdate = transactionDataStorage.GetDataAccDate();

            if (ACCdate != null && ACCdate.Rows.Count > 0)
            {
                string dateToday_MMddyyyy = DateTime.Now.ToString("MMddyyyy");
                string dateYesterday_MMddyyyy = DateTime.Now.AddDays(-1).ToString("MMddyyyy");
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
                perTrans1 = GetCountFromTable(eodSLSDataStorage.SelectPERTRANS("orderdata", "AccDate", dateToday_MMddyyyy), "COUNT(*)");
                perTransYestday1 = GetCountFromTable(eodSLSDataStorage.SelectPERTRANS("orderdata", "AccDate", dateYesterday_MMddyyyy), "COUNT(*)");

                try
                {
                    #region EOD Scenarios Logic

                    if (EODYesterday1 != 0 && perTrans1 != 0 && EODToday1 == 0)
                    {
                        convertFormatDate(LAST_DATE_MMDDYYYY, LAST_DATE_MMDDYYYY);
                        summaryMessage += "✔ Automatic End of Day has been performed successfully.\n";
                    }
                    else if (EODToday1 != 0)
                    {
                        //summaryMessage += "ℹ EOD has already been performed for today.\n";
                    }
                    else if (EODToday1 == 0 && perTrans1 == 0)
                    {
                        convertFormatDate(LAST_DATE_MMDDYYYY, LAST_DATE_MMDDYYYY);
                        summaryMessage += "✔ No sales for today. Automatic End of Day has still been processed.\n";
                    }
                    else
                    {
                        convertFormatDate(LAST_DATE_MMDDYYYY, LAST_DATE_MMDDYYYY);
                        //summaryMessage += "⚠ Automatic End of Day fallback executed.\n";
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    summaryMessage += $"❌ An unexpected error occurred during Auto EOD:\n{ex.Message}\n";
                }

                // One summary MessageBox at the end (or you can just log this instead if needed)
                if (!string.IsNullOrEmpty(summaryMessage))
                {
                    //MessageBox.Show(summaryMessage.Trim(), "EOD Summary", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }



        //public void checkEODpreviousDates_Auto()
        //{
        //    DataTable ACCdate = transactionDataStorage.GetDataAccDate();
        //    if (ACCdate != null && ACCdate.Rows.Count > 0)
        //    {
        //        string dateToday = DateTime.Now.ToString("yyyyMMdd");
        //        string dateYesterday = DateTime.Now.AddDays(-1).ToString("yyyyMMdd");
        //        string summaryMessage = "";

        //        foreach (DataRow row in ACCdate.Rows)
        //        {
        //            LAST_DATE_YYYYMMDD = row["TranscDate"].ToString();
        //            DateTime parsedDate = DateTime.ParseExact(LAST_DATE_YYYYMMDD, "yyyyMMdd", CultureInfo.InvariantCulture);

        //            LAST_DATE_MMDDYYYY = parsedDate.ToString("MMddyyyy");
        //            LAST_DATE_YYYYMMDD = parsedDate.ToString("yyyyMMdd");

        //            DateTime yesterday = DateTime.Now.AddDays(-1);
        //            YESTERDAY_MMDDYYYY = yesterday.ToString("MMddyyyy");

        //            DateTime yesterday1 = parsedDate.AddDays(-1);
        //            YESTERDAY_YYYYMMDD = yesterday1.ToString("yyyyMMdd");
        //        }

        //        // Load all your count variables
        //        TotalRecords = GetCountFromTable(eodSLSDataStorage.CheckEODtable(), "CountEOD");
        //        EODToday1 = GetCountFromTable(eodSLSDataStorage.SelectEOD("eodslsdata", "TransDate", dateToday), "COUNT(*)");
        //        EODYesterday1 = GetCountFromTable(eodSLSDataStorage.SelectEOD("eodslsdata", "TransDate", dateYesterday), "COUNT(*)");
        //        perTrans1 = GetCountFromTable(eodSLSDataStorage.SelectPERTRANS("orderdata", "AccDate", dateToday), "COUNT(*)");
        //        perTransYestday1 = GetCountFromTable(eodSLSDataStorage.SelectPERTRANS("orderdata", "AccDate", dateYesterday), "COUNT(*)");

        //        try
        //        {
        //            #region EOD Scenarios Logic

        //            if (EODYesterday1 != 0 && perTrans1 != 0 && EODToday1 == 0)
        //            {
        //                convertFormatDate(LAST_DATE_MMDDYYYY, LAST_DATE_MMDDYYYY);
        //                summaryMessage += "✔ Automatic End of Day has been performed successfully.\n";
        //            }
        //            else if (EODToday1 != 0)
        //            {
        //                summaryMessage += "ℹ EOD has already been performed for today.\n";
        //            }
        //            else if (EODToday1 == 0 && perTrans1 == 0)
        //            {
        //                convertFormatDate(LAST_DATE_MMDDYYYY, LAST_DATE_MMDDYYYY);
        //                summaryMessage += "✔ No sales for today. Automatic End of Day has still been processed.\n";
        //            }
        //            else
        //            {
        //                convertFormatDate(LAST_DATE_MMDDYYYY, LAST_DATE_MMDDYYYY);
        //                summaryMessage += "⚠ Automatic End of Day fallback executed.\n";
        //            }

        //            #endregion
        //        }
        //        catch (Exception ex)
        //        {
        //            summaryMessage += $"❌ An unexpected error occurred during Auto EOD:\n{ex.Message}\n";
        //        }

        //        // Show just one summary popup at the end
        //        if (!string.IsNullOrEmpty(summaryMessage))
        //        {
        //            MessageBox.Show(summaryMessage.Trim(), "EOD Summary", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //        }
        //    }
        //}

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


        public void convertFormatDate(string startDate, string endDate) 
        {
            try
            {
                DateTime start = DateTime.ParseExact(startDate, "MMddyyyy", null);
                DateTime end = DateTime.ParseExact(endDate, "MMddyyyy", null);

                for (DateTime date = start; date <= end; date = date.AddDays(1))
                {

                    string mmddyyyy = date.ToString("MMddyyyy");
                    string yyyymmdd = date.ToString("yyyyMMdd");
                    string yesterday = date.AddDays(-1).ToString("yyyyMMdd");

                    PerformEOD(mmddyyyy, yyyymmdd, yesterday, silentMode: true);
                }

                //MessageBox.Show("EOD operations completed for the date range.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}\n{ex.StackTrace}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            //PerformEOD(LAST_DATE_MMDDYYYY, LAST_DATE_YYYYMMDD, YESTERDAY_MMDDYYYY);
        }
        #endregion
    }
}
