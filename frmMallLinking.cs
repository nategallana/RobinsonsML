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

        private System.Timers.Timer eodTimer;
        private System.Timers.Timer appCloseTimer;
        private bool isEODTriggered = false;
        private readonly object eodLock = new object();

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
        private static NotifyIcon _notifyIcon;
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
        }
        private void InitializeFolder()
        {
            try
            {
                string sentFolder = "Send File";
                string unsentFolder = "Pending Filer";

                string sent = Path.Combine(dir, sentFolder);
                string unsent = Path.Combine(dir, unsentFolder);

                if(!Directory.Exists(sent) || !Directory.Exists(unsent))
                {
                    Directory.CreateDirectory(sent);
                    Directory.CreateDirectory(unsent);
                }
            }
            catch(Exception ex)
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
                //eodDataStorage = new EODDataStorage();
                eodSLSDataStorage = new EODSLSDataStorage();

                notifyIcon1.BalloonTipText = " ";
                notifyIcon1.ShowBalloonTip(2000);

                BackupXML(xml);
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
            appCloseTimer = new System.Timers.Timer(2000); // Check every minute
            appCloseTimer.Elapsed += CheckTimeAndCloseApp;
            appCloseTimer.Start();
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

            string[] xmlFiles = Directory.GetFiles(xmlFolderPath, "*.xml");

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

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }

        private void eODToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Scenarios();
            checkEODpreviousDates();
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
        #endregion

        #region App Closing Logic (During 4 AM to 9 AM)
        private void CheckTimeAndCloseApp(object sender, ElapsedEventArgs e)
        {
            DateTime now = DateTime.Now;

            // Check if the time is between 4:00 AM and 9:00 AM
            if (now.TimeOfDay >= new TimeSpan(4, 0, 0) && now.TimeOfDay < new TimeSpan(9, 0, 0))
            {
                CloseOtherAppIfRunning();
            }
        }

        private void CloseOtherAppIfRunning()
        {
            string targetProcessName = "wDesktop"; // Replace with the actual process name (without .exe)

            var processes = Process.GetProcessesByName(targetProcessName);
            if (processes.Any())
            {
                foreach (var process in processes)
                {
                    try
                    {
                        process.Kill(); // Forcefully terminate the process
                        process.WaitForExit(); // Wait for the process to exit
                        MessageBox.Show($"The application '{targetProcessName}' was closed successfully.",
                                        "Application Closed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to close the application '{targetProcessName}'. Error: {ex.Message}",
                                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                //MessageBox.Show($"No running instance of '{targetProcessName}' was found.", "Application Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        #endregion

        #region Perform EOD
        public void PerformEOD(string mmddyyyy, string yyyymmdd, string yesterday)
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
                //EODTOTRFD = row["TaxAmount"].ToSafeDecimal(); no refund in wbox
                //EODNORFD = row["TaxAmount"].ToSafeInteger(); no refund in wbox
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
                //EODTOTNONVAT = row["TaxAmount"].ToSafeDecimal());
                //EODPHRMSLS = row["TaxAmount"].ToSafeDecimal();
                //EODNONPHRMSLS = row["TaxAmount"].ToSafeDecimal());
                //EODDISABLEDISC = row["TaxAmount"].ToSafeDecimal();
                //EODPERCRENT = row["TaxAmount"].ToSafeDecimal();
                //EODTOTAMNTREPRNT = row["TaxAmount"].ToSafeDecimal();
                //EODNOREPRNT = EODNOREPRNT + 1;

                //EODTRNSDATE = DateTime.Parse(row["TranscDate"].ToString());
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
                // Generate Fixed-Width File
                GenerateFixedWidthFile(EODSLS, folderPath, fileName, filePaths);

                int batchSize = 5; // Example: Batch size of 5
                for (int i = 0; i < filePaths.Count; i += batchSize)
                {
                    List<string> batchFiles = filePaths.GetRange(i, Math.Min(batchSize, filePaths.Count - i));
                    SendFilesInBatch(batchFiles);
                }
                CheckFileSendingStatus();
                #region Notif
                // Simulate sending sales file to server
                //if (SendSalesFileToServer())
                //{
                //    MessageBox.Show("Sales file successfully sent to RLC server.", "EOD Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //}
                //else
                //{
                //    MessageBox.Show("Sales file is not sent to RLC server. Please contact your POS vendor.", "EOD Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //}
                #endregion
            }
            catch (Exception ex) {
                MessageBox.Show($"Error: {ex.Message}\n{ex.StackTrace}");
                ErrorLogger.LogError($"Error: {ex.Message}\n{ex.StackTrace}");
            }
        }

        #region Generate EOD Textfile
        private void GenerateFixedWidthFile(EODSLSData eodData, string folderPath, string fileName, List<string>filePaths)
        {
            //SFTP sftpClient = new SFTP();
            string batchDate = DateTime.Today.ToString("MMM-dd-yyyy");
            string batchTime = DateTime.Now.ToString("hh-mm-tt");

            int batchCount = 0;
            string status = "sent";

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            DataTable checkBatchFileName = batchfileDataStorage.CheckBatchFileName();

            bool fileExists = false;

            // Check if the file already exists in the DataTable
            foreach (DataRow row in checkBatchFileName.Rows)
            {
                string existingFileName = row["FileName"].ToString();
                int existingBatchCount = Convert.ToInt32(row["Count"]);

                if (existingFileName == fileName)
                {
                    fileExists = true;
                    batchCount = existingBatchCount + 1;

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

                batchfileDataStorage.Add(batchfile, false);
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

                batchfileDataStorage.Add(batchfile, false);
            }

            // Combine folder path and file name to get the full file path
            string fullFilePath = Path.Combine(folderPath, fileName);

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
                writer.WriteLine($"18{DateTime.Parse(eodData.TransDate).ToString("MM/dd/yy").PadLeft(totalWidth - 2, '0')}");
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
            if(GlobalVar.SuccessSending)
            {
                _notifyIcon.ShowBalloonTip(300, "Mall Linking", "Sale file Successfully sent to RLC server", ToolTipIcon.Info);
                this.Close();
            }
            else if (GlobalVar.SuccessSending2 && !GlobalVar.SuccessSending)
            {
                _notifyIcon.ShowBalloonTip(300, "Mall Linking", "Trying to send unsent files...Successfully", ToolTipIcon.Info);
            }
            else
            {
                _notifyIcon.ShowBalloonTip(300, "Mall Linking", "Sales file is not sent to RLC server. Please contact your POS vendor", ToolTipIcon.Error);
                //SftpConnectionChecker.StartChecking();
            }
        }
        #region Transaction Logic
        public void checkEODpreviousDates() 
        {

            DialogResult result = MessageBox.Show("Previous’ day’s EOD was not performed", " ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            if (result == DialogResult.OK)
            {
                DataTable ACCdate = transactionDataStorage.GetDataAccDate();
                foreach (DataRow row in ACCdate.Rows)
                {
                    //MessageBox.Show($"TransDATE: {row["TranscDate"]}");
                    LAST_DATE_YYYYMMDD = row["TranscDate"].ToString();
                    DateTime parsedDate = DateTime.ParseExact(LAST_DATE_YYYYMMDD, "yyyyMMdd", CultureInfo.InvariantCulture);
                    string formattedDate = parsedDate.ToString("MM/dd/yyyy");
                    LAST_DATE_MMDDYYYY = formattedDate;
                    string formattedDate1 = parsedDate.ToString("yyyyMMdd");
                    LAST_DATE_YYYYMMDD = formattedDate1;

                    DateTime yesterday = parsedDate.AddDays(-1);
                    YESTERDAY_MMDDYYYY = yesterday.ToString("MM/dd/yyyy");
                    DateTime yesterday1 = parsedDate.AddDays(-1);
                    YESTERDAY_YYYYMMDD = yesterday1.ToString("yyyyMMdd");
                }

                DataTable checkEODtable = eodSLSDataStorage.CheckEODtable();
                foreach (DataRow row in checkEODtable.Rows) 
                {
                    TotalRecords = int.Parse(row["CountEOD"].ToString());
                }

                DataTable EODToday = eodSLSDataStorage.SelectEOD("eodslsdata", "TransDate", "");
                foreach (DataRow row in EODToday.Rows)
                {
                    EODToday1 = int.Parse(row["TranscDate"].ToString());
                }

                DataTable EODYesterday = eodSLSDataStorage.SelectEOD("eodslsdata", "TransDate", "");
                foreach (DataRow row in EODYesterday.Rows)
                {
                    EODYesterday1 = int.Parse(row["TranscDate"].ToString());
                }

                DataTable perTrans = eodSLSDataStorage.SelectPERTRANS("orderdata", "AccDate", "");
                foreach (DataRow row in perTrans.Rows)
                {
                    perTrans1 = int.Parse(row["AccDate"].ToString());
                }

                DataTable perTransYestday = eodSLSDataStorage.SelectPERTRANS("orderdata", "AccDate", "");
                foreach (DataRow row in perTransYestday.Rows)
                {
                    perTransYestday1 = int.Parse(row["AccDate"].ToString());
                }

                try
                {
                    #region LOGIC
                    // Scenario 1: Generate EOD for today with sales (if sales exist today and no EOD for today)
                    if (EODYesterday1 == 0 && perTrans1 != 0 && EODToday1 == 0)
                    {
                        convertFormatDate(LAST_DATE_MMDDYYYY, YESTERDAY_MMDDYYYY);
                    }
                    // Scenario 2: EOD has already been performed for today
                    else if (EODToday1 != 0)
                    {
                        MessageBox.Show("EOD has already been performed for today.", "EOD Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    // Scenario 3: No EOD record found for yesterday
                    else if (EODYesterday1 == 0 && (perTransYestday1 != 0 || perTransYestday1 == 0))
                    {
                        MessageBox.Show("No EOD record found for yesterday. Please complete EOD for yesterday before proceeding.", "Missing EOD", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        Scenarios();
                        return;
                    }
                    // Scenario 4: No sales today, user confirmation for EOD
                    else if (EODToday1 == 0 && perTrans1 == 0)
                    {
                        DialogResult result1 = MessageBox.Show("No sales for today. Do you still want to perform EOD for today?", "EOD Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (result1 == DialogResult.Yes)
                        {
                            convertFormatDate(LAST_DATE_MMDDYYYY, YESTERDAY_MMDDYYYY);
                        }
                        else
                        {
                            return;
                        }
                    }
                    // Scenario 5: Fallback case for generating EOD
                    else
                    {
                        convertFormatDate(LAST_DATE_MMDDYYYY, YESTERDAY_MMDDYYYY);
                        //MessageBox.Show("Generating EOD.", "EOD Generation", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

                return;
            }
        }

        public void Scenarios() 
        {
            DateTime now = DateTime.Now;

            if (now.TimeOfDay >= new TimeSpan(4, 0, 0) && now.TimeOfDay < new TimeSpan(9, 0, 0))
            {
                MessageBox.Show("Transactions are not allowed between 4:00 AM and 9:00 AM.", "Transaction Restricted", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                // Close the different application (e.g., "OtherApp")
                string targetProcessName = "wDesktop"; // Replace with the actual process name without ".exe"

                var processes = Process.GetProcessesByName(targetProcessName);
                if (processes.Any())
                {
                    foreach (var process in processes)
                    {
                        try
                        {
                            process.Kill(); 
                            process.WaitForExit();
                            MessageBox.Show($"Not allowed to transact at this time.", "Application Closed", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Failed to close the application '{targetProcessName}'. Error: {ex.Message}",
                                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                else
                {
                    MessageBox.Show($"No running instance of '{targetProcessName}' was found.",
                                    "Application Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return;
            }
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

                    PerformEOD(mmddyyyy, yyyymmdd, yesterday);
                }

                MessageBox.Show("EOD operations completed for the date range.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
