using RobinsonMallLinkingWBOX.Constants;
using RobinsonMallLinkingWBOX.Helpers;
using RobinsonMallLinkingWBOX.LocalStorage;
using RobinsonMallLinkingWBOX.Models;
using RobMLWBOX;
using RobMLWBOX.Helpers;
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
using System.Windows.Forms;
using Tarsier.Extensions;

namespace RobinsonMallLinkingWBOX
{
    public partial class frmDateRange : Form
    {

        public frmDateRange()
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

            InitializeFolder();
        }


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
        SFTP sftp2 = new SFTP();
#pragma warning disable CS0649 // Field 'frmDateRange._notifyIcon' is never assigned to, and will always have its default value null
#pragma warning disable CS0169 // The field 'frmDateRange._notifyIcon' is never used
        private static NotifyIcon _notifyIcon;
#pragma warning restore CS0169 // The field 'frmDateRange._notifyIcon' is never used
#pragma warning restore CS0649 // Field 'frmDateRange._notifyIcon' is never assigned to, and will always have its default value null
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
        private decimal grossSales;


        public string LAST_DATE_MMDDYYYY;
        public string LAST_DATE_YYYYMMDD;
        public string YESTERDAY_MMDDYYYY;
        public string YESTERDAY_YYYYMMDD;
        #endregion
        public string xml, host, username, password, dir, tenantname, terminal = null;
        private decimal nonVat;
        private decimal discwithVat;
        private int otherDiscCnt;
        private decimal nonV;
        private decimal addscpwed;
        private decimal forNvGross;
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



        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //private void btnGenerate_Click(object sender, EventArgs e)
        //{
        //    // Create and show the loading form
        //    frmRobProcess loadingForm = new frmRobProcess();
        //    loadingForm.Show();
        //    loadingForm.Refresh(); // Ensures the form is drawn immediately
        //    try
        //    {
        //        DateTime start = dtpFrom.Value.Date;
        //        DateTime end = dtpTo.Value.Date;

        //        for (var day = start; day <= end; day = day.AddDays(1))
        //        {
        //            PerformEOD(day.ToString("MMddyyyy"), day.ToString("yyyyMMdd"), day.AddDays(-1).ToString("yyyyMMdd"));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //MessageBox.Show($"Error: {ex.Message}\n{ex.StackTrace}", "Date Range Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

        //        ErrorLogger.LogError(ex.Message.ToString() + " || " + ex.StackTrace.ToString());
        //    }
        //    finally
        //    {
        //        // Close the loading form after process completion
        //        loadingForm.Close();
        //    }
        //}

        public void CheckFirstSalesData(out string firstData)
        {
            firstData = string.Empty;

            // Attempt to get the last successful EOD date
            DataTable accDate = eodSLSDataStorage.FirstEOD();

            if (accDate != null && accDate.Rows.Count > 0)
            {
                DataRow row = accDate.Rows[0];
                string lastDateRaw = row["TransDate"].ToString();

                if (!string.IsNullOrWhiteSpace(lastDateRaw))
                {
                    try
                    {
                        DateTime parsedDate = DateTime.ParseExact(lastDateRaw, "MMddyyyy", CultureInfo.InvariantCulture);
                        firstData = parsedDate.ToString("MMddyyyy");
                    }
                    catch (FormatException)
                    {
                        // Handle parsing error if necessary
                        firstData = string.Empty;
                    }
                }
            }
        }


        public void CheckLastSalesData(out string lastData)
        {
            lastData = string.Empty;

            // Attempt to get the last successful EOD date
            DataTable accDate = eodSLSDataStorage.GetDataEodDate();

            if (accDate != null && accDate.Rows.Count > 0)
            {
                DataRow row = accDate.Rows[0];
                string lastDateRaw = row["TransDate"].ToString();

                if (!string.IsNullOrWhiteSpace(lastDateRaw))
                {
                    try
                    {
                        DateTime parsedDate = DateTime.ParseExact(lastDateRaw, "MMddyyyy", CultureInfo.InvariantCulture);
                        lastData = parsedDate.ToString("MMddyyyy");
                    }
                    catch (FormatException)
                    {
                        // Handle parsing error if necessary
                        lastData = string.Empty;
                    }
                }
            }
        }


        private void btnGenerate_Click(object sender, EventArgs e)
        {
            // Retrieve the first EOD date from the eodSLSDataStorage
            string firstEODDateStr;
            CheckFirstSalesData(out firstEODDateStr);

            if (string.IsNullOrEmpty(firstEODDateStr))
            {
                MessageBox.Show("No EOD data available. Cannot proceed.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //ErrorLogger.LogError("No EOD data available. Cannot proceed.");
                return;
            }

            // Parse the first EOD date string to a DateTime object
            DateTime firstEODDate;
            try
            {
                firstEODDate = DateTime.ParseExact(firstEODDateStr, "MMddyyyy", CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                MessageBox.Show("Invalid EOD date format. Cannot proceed.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //ErrorLogger.LogError("Invalid EOD date format. Cannot proceed.");
                return;
            }



            // Retrieve the last EOD date from the eodSLSDataStorage
            string lastEODDateStr;
            CheckLastSalesData(out lastEODDateStr);

            if (string.IsNullOrEmpty(lastEODDateStr))
            {
                MessageBox.Show("No EOD data available. Cannot proceed.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //ErrorLogger.LogError("No EOD data available. Cannot proceed.");
                return;
            }

            // Parse the last EOD date string to a DateTime object
            DateTime lastEODDate;
            try
            {
                lastEODDate = DateTime.ParseExact(lastEODDateStr, "MMddyyyy", CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                MessageBox.Show("Invalid EOD date format. Cannot proceed.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //ErrorLogger.LogError("Invalid EOD date format. Cannot proceed.");
                return;
            }


            // Create and show the loading form
            DateTime start = dtpFrom.Value.Date;
            DateTime end = dtpTo.Value.Date;


            if (end < start)
            {
                MessageBox.Show($"Invalid Date Parameters.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate that the start date is not after the last EOD date
            if (start < firstEODDate || end < firstEODDate)
            {
                MessageBox.Show($"Selected date range goes beyond the first available EOD date. Cannot proceed.",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate that the start date is not after the last EOD date
            if (start > lastEODDate || end > lastEODDate)
            {
                MessageBox.Show($"Selected date range goes beyond the last available EOD date. Cannot proceed.",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(dir))
            {
                dir = GlobalVar.DIRECTORY;
            }

            if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
            {
                ErrorLogger.LogError($"The system cannot find the path: '{dir}'. EOD process cancelled.");
                MessageBox.Show($"The system cannot find the path: '{dir}'\n\nEOD process has been cancelled.", "EOD Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            frmRobProcess loadingForm = new frmRobProcess();
            loadingForm.Show();
            loadingForm.Refresh(); // Ensures the form is drawn immediately

            bool allSucceeded = true;

            try
            {
                for (var day = start; day <= end; day = day.AddDays(1))
                {
                    bool success = PerformEOD(day.ToString("MMddyyyy"), day.ToString("yyyyMMdd"), day.AddDays(-1).ToString("yyyyMMdd"));
                    if (!success)
                    {
                        allSucceeded = false;
                        break;
                    }
                }

                if (allSucceeded)
                {
                    MessageBox.Show("EOD Process Complete.", "EOD Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Error: {ex.Message}\n{ex.StackTrace}", "Date Range Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                ErrorLogger.LogError(ex.Message.ToString() + " || " + ex.StackTrace.ToString());
            }
            finally
            {
                // Close the loading form after process completion
                loadingForm.Close();
            }
        }




        private void frmDateRange_Load(object sender, EventArgs e)
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

            }
            catch (Exception ex)
            {
                //MessageBox.Show($"An error occurred during form load:\n{ex.Message}", "Check Settings", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                ErrorLogger.LogError(ex.Message.ToString() + " || " + ex.StackTrace.ToString());
            }
        }


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
            foreach (DataRow row in dtVoid.Rows)
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







            //InitializeEODValues();

            //EODTENTID = tenantname;
            //EODTERMNO = terminal;
            //EODTRNSDATE = mmddyyyy;


            //#region rengiel edited

            //#region getTaxAmount line 04
            //DataTable dtTax = taxDataStorage.GetTaxAmount(yyyymmdd);
            //foreach (DataRow rowTax in dtTax.Rows)
            //{
            //    if (!voidDataStorage.getifVoided(Convert.ToString(rowTax["OrderNo"])))
            //    {
            //        EODTOTVAT = EODTOTVAT + rowTax["Value"].ToSafeDecimal();
            //    }
            //}
            //#endregion

            //#region getVoidAmount and Count line 05 06

            //DataTable voidTABLE = voidDataStorage.GetVOID(yyyymmdd);
            //foreach (DataRow row in voidTABLE.Rows)
            //{
            //    EODTOTVOID = row["TotalVoid"].ToSafeDecimal();
            //    EODNOVOID = row["CountVoid"].ToSafeInteger();
            //}

            //#endregion

            //#region getDiscountTotal line 08 
            //DataTable discTABLE = transactionDataStorage.GetDISCOUNT(yyyymmdd);
            //foreach (DataRow row in discTABLE.Rows)
            //{
            //    //EODTOTDISC = row["DiscountValue"].ToSafeDecimal();
            //    EODNODISC = row["CountDiscount"].ToSafeInteger();
            //}
            //#endregion

            //#region getSeniorPWD line 11 12 27
            //DataTable dtSeniorPWD = discountDataStorage.GetDISCOUNT(yyyymmdd);
            //foreach (DataRow row in dtSeniorPWD.Rows)
            //{
            //    decimal positiveAmount = row["Amount"].ToSafeDecimal() * -1M;
            //    if (row["Type"].ToString().ToUpper().Contains("SENIOR"))
            //    {
            //        EODOTHRNEGA = EODOTHRNEGA + positiveAmount;
            //        EODNOOTHRNEGA = EODNOOTHRNEGA + 1;
            //    }
            //    if (row["Type"].ToString().ToUpper().Contains("PWD"))
            //    {
            //        EODDISABLEDISC = EODDISABLEDISC + positiveAmount;
            //    }
            //}
            //#endregion

            //#region getCreditSales line 22 23
            //DataTable dtPayment = paymentDataStorage.getPayment(yyyymmdd);
            //decimal notCashAmount = 0M;
            //foreach (DataRow row in dtPayment.Rows)
            //{
            //    if (!row["Name"].ToString().ToUpper().Contains("CASH"))
            //    {
            //        notCashAmount = notCashAmount + row["Amount"].ToSafeDecimal();
            //    }
            //}
            //EODTOTCRDSLS = notCashAmount;
            //EODTOTCRDVAT = notCashAmount / 1.12M * 0.12M;

            //#endregion

            //#region getNonVatSales line 24
            //DataTable dtNonVat = discountDataStorage.GetDISCOUNT(yyyymmdd);
            //nonVat = 0M;
            //foreach (DataRow row in dtNonVat.Rows)
            //{
            //    decimal positiveAmount = row["Amount"].ToSafeDecimal() * -1M;
            //    if (row["Type"].ToString().ToUpper().Contains("SENIOR"))
            //    {
            //        nonVat = nonVat + row["Total"].ToSafeDecimal() + positiveAmount;
            //    }
            //    if (row["Type"].ToString().ToUpper().Contains("PWD"))
            //    {
            //        nonVat = nonVat + row["Total"].ToSafeDecimal() + positiveAmount;
            //    }
            //}
            //EODTOTNONVAT = nonVat - EODOTHRNEGA - EODDISABLEDISC;
            //#endregion

            //#region getGrossSales line 03
            //grossSales = EODTOTVAT / 0.12M * 1.12M;
            //EODGROSS = grossSales + nonVat;
            //#endregion


            //#endregion


            //#region GROSS VATTOTAL TOTALSERVCHARGE
            //DataTable ALLdatas = transactionDataStorage.GetALLDATAS(yyyymmdd);
            //foreach (DataRow row in ALLdatas.Rows)
            //{

            //    EODTRNSDATE = DateTime.ParseExact(row["TranscDate"].ToString(), "yyyyMMdd", null).ToString("MMddyyyy");
            //}
            //#endregion

            //#region Previous and Current Grand total

            //DateTime date = DateTime.ParseExact(yesterday, "yyyyMMdd", null);
            //string formattedYesterday = date.ToString("MMddyyyy"); // "04042025"
            //DataTable yestData = transactionDataStorage.GetDataYesterday(formattedYesterday);
            //foreach (DataRow row in yestData.Rows)
            //{
            //    EODPRVCNTR = row["CurrEODcntr"].ToSafeInteger();
            //    EODPRVGRNTOT = row["CurrGrndTot"].ToSafeDecimal();
            //}

            //decimal eodPrvGnTot = EODPRVGRNTOT;
            //EODCRRCNTR = EODPRVCNTR + 1;
            //EODCRRGRNTOT = EODPRVGRNTOT + EODGROSS;
            //#endregion

            //try
            //{
            //    var EODSLS = new EODSLSData
            //    {
            //        TenantID = EODTENTID,
            //        TermNo = EODTERMNO,
            //        Gross = Convert.ToDecimal(EODGROSS.ToDecimalPlaces(2)),
            //        TotVat = Convert.ToDecimal(EODTOTVAT.ToDecimalPlaces(2)),
            //        TotVoid = Convert.ToDecimal(EODTOTVOID.ToDecimalPlaces(2)),
            //        NoVoid = EODNOVOID.ToSafeInteger(),
            //        TotDisc = Convert.ToDecimal(EODTOTDISC.ToDecimalPlaces(2)),
            //        NoDisc = EODNODISC.ToSafeInteger(),
            //        TotRfnd = Convert.ToDecimal(EODTOTRFD.ToDecimalPlaces(2)),
            //        NoRfnd = EODNORFD.ToSafeInteger(),
            //        OthrNegaAdjst = Convert.ToDecimal(EODOTHRNEGA.ToDecimalPlaces(2)),
            //        NoNegaAdjst = EODNOOTHRNEGA.ToSafeInteger(),
            //        TotSrvChrg = Convert.ToDecimal(EODTOTSRCHR.ToDecimalPlaces(2)),
            //        PrvEODcntr = EODPRVCNTR.ToSafeInteger(),
            //        PrvGrndTot = Convert.ToDecimal(EODPRVGRNTOT.ToDecimalPlaces(2)),
            //        CurrEODcntr = EODCRRCNTR.ToSafeInteger(),
            //        CurrGrndTot = Convert.ToDecimal(EODCRRGRNTOT.ToDecimalPlaces(2)),
            //        TransDate = EODTRNSDATE,
            //        Novelty = Convert.ToDecimal(EODNOV.ToDecimalPlaces(2)),
            //        Misc = Convert.ToDecimal(EODMISC.ToDecimalPlaces(2)),
            //        LocTax = Convert.ToDecimal(EODLOCTAX.ToDecimalPlaces(2)),
            //        TotCrdSls = Convert.ToDecimal(EODTOTCRDSLS.ToDecimalPlaces(2)),
            //        TotCrdVat = Convert.ToDecimal(EODTOTCRDVAT.ToDecimalPlaces(2)),
            //        TotNonVat = Convert.ToDecimal(EODTOTNONVAT.ToDecimalPlaces(2)),
            //        PhrmSls = Convert.ToDecimal(EODPHRMSLS.ToDecimalPlaces(2)),
            //        NonPhrmSls = Convert.ToDecimal(EODNONPHRMSLS.ToDecimalPlaces(2)),
            //        DisabDisc = Convert.ToDecimal(EODDISABLEDISC.ToDecimalPlaces(2)),
            //        PercRent = Convert.ToDecimal(EODPERCRENT.ToDecimalPlaces(2)),
            //        TotAmntRePrnt = Convert.ToDecimal(EODTOTAMNTREPRNT.ToDecimalPlaces(2)),
            //        NoRePrnt = EODNOREPRNT.ToSafeInteger()
            //    };

            //    eodSLSDataStorage.Add(EODSLS, false);

            //    string lastFourDigits = EODTENTID.Substring(EODTENTID.Length - 4);
            //    string lastTransac = yyyymmdd.Substring(yyyymmdd.Length - 4);
            //    string folderPath = dir;
            //    string fileName = lastFourDigits + lastTransac + "." + EODTERMNO;

            //    List<string> filePaths = new List<string>();
            //    GenerateFixedWidthFile(EODSLS, folderPath, fileName, filePaths);

            //    int batchSize = 5;
            //    for (int i = 0; i < filePaths.Count; i += batchSize)
            //    {
            //        List<string> batchFiles = filePaths.GetRange(i, Math.Min(batchSize, filePaths.Count - i));
            //        SendFilesInBatch(batchFiles);
            //    }

            //    CheckFileSendingStatus();
            //}
            //catch (Exception ex)
            //{
            //    ErrorLogger.LogError($"EOD ERROR: {ex.Message}\n{ex.StackTrace}");
            //    if (!silentMode)
            //    {
            //        MessageBox.Show($"Error: {ex.Message}", "EOD Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    }
            //}
        //}


        //private void GenerateFixedWidthFile(EODSLSData eodData, string folderPath, string fileName, List<string> filePaths)
        //{
        //    //SFTP sftpClient = new SFTP();
        //    string batchDate = DateTime.Today.ToString("MMM-dd-yyyy");
        //    string batchTime = DateTime.Now.ToString("hh:mm:ss tt");

        //    int batchCount = 0;
        //    string status = "unsent";

        //    if (!Directory.Exists(folderPath))
        //    {
        //        Directory.CreateDirectory(folderPath);
        //    }



        //    DataTable checkBatchFileName = batchfileDataStorage.CheckBatchFileName(); // Ensure this gets fresh data

        //    bool fileExists = false;
        //    int latestBatchCount = 0;

        //    // Get the latest batch count for the matching file
        //    var latestRow = checkBatchFileName.AsEnumerable()
        //        .Where(row => row["FileName"].ToString().Length >= 11 &&
        //                      row["FileName"].ToString().Substring(0, 11) == fileName) // Ensure correct file match
        //        .OrderByDescending(row => Convert.ToInt32(row["Count"])) // Get the highest count
        //        .FirstOrDefault();

        //    if (latestRow != null)
        //    {
        //        latestBatchCount = Convert.ToInt32(latestRow["Count"]);
        //    }

        //    // Check if the file already exists in the DataTable
        //    foreach (DataRow row in checkBatchFileName.Rows)
        //    {
        //        string existingFileName = row["FileName"].ToString(); // Example: '67890214.011'
        //        string extractedPart = existingFileName.Length >= 11 ? existingFileName.Substring(0, 11) : existingFileName;
        //        //int existingBatchCount = Convert.ToInt32(row["Count"]);

        //        if (extractedPart == fileName)
        //        {
        //            fileExists = true;
        //            batchCount = latestBatchCount + 1; // Use the latest count

        //            break; // Exit the loop once the match is found
        //        }
        //    }


        //    // Create or update the batch file entry
        //    if (fileExists)
        //    {
        //        var batchfile = new BatchFile
        //        {
        //            FileName = fileName,
        //            TransDate = batchDate,
        //            TransTime = batchTime,
        //            Count = batchCount,
        //            Status = status
        //        };
        //        GlobalVar.batchcount = batchCount;
        //        //batchfileDataStorage.Add(batchfile, false);
        //    }
        //    else
        //    {
        //        batchCount = 1;

        //        var batchfile = new BatchFile
        //        {
        //            FileName = fileName,
        //            TransDate = batchDate,
        //            TransTime = batchTime,
        //            Count = batchCount,
        //            Status = status
        //        };
        //        GlobalVar.batchcount = batchCount;
        //        //batchfileDataStorage.Add(batchfile, false);
        //    }

        //    // Combine folder path and file name to get the full file path
        //    string fullFilePath = Path.Combine(folderPath, fileName + batchCount.ToString());

        //    using (StreamWriter writer = new StreamWriter(fullFilePath))
        //    {
        //        // Define the total width for the entire line (18 characters)
        //        const int totalWidth = 18;

        //        string input = eodData.TransDate.ToString();  // MMddyyyy format
        //        DateTime date = DateTime.ParseExact(input, "MMddyyyy", null);
        //        string formattedDate = date.ToString("MM/dd/yyyy");


        //        // Write each line with the field ID followed by the value, padded with zeros and formatted as needed
        //        writer.WriteLine($"01{eodData.TenantID.PadLeft(totalWidth - 2, '0')}");
        //        writer.WriteLine($"02{eodData.TermNo.PadLeft(totalWidth - 2, '0')}");
        //        writer.WriteLine($"03{eodData.Gross.ToString("0.00").PadLeft(totalWidth - 2, '0')}");
        //        writer.WriteLine($"04{eodData.TotVat.ToString("0.00").PadLeft(totalWidth - 2, '0')}");
        //        writer.WriteLine($"05{eodData.TotVoid.ToString("0.00").PadLeft(totalWidth - 2, '0')}");
        //        writer.WriteLine($"06{eodData.NoVoid.ToString().PadLeft(totalWidth - 2, '0')}");
        //        writer.WriteLine($"07{eodData.TotDisc.ToString("0.00").PadLeft(totalWidth - 2, '0')}");
        //        writer.WriteLine($"08{eodData.NoDisc.ToString().PadLeft(totalWidth - 2, '0')}");
        //        writer.WriteLine($"09{eodData.TotRfnd.ToString("0.00").PadLeft(totalWidth - 2, '0')}");
        //        writer.WriteLine($"10{eodData.NoRfnd.ToString().PadLeft(totalWidth - 2, '0')}");
        //        writer.WriteLine($"11{eodData.OthrNegaAdjst.ToString("0.00").PadLeft(totalWidth - 2, '0')}");
        //        writer.WriteLine($"12{eodData.NoNegaAdjst.ToString().PadLeft(totalWidth - 2, '0')}");
        //        writer.WriteLine($"13{eodData.TotSrvChrg.ToString("0.00").PadLeft(totalWidth - 2, '0')}");
        //        writer.WriteLine($"14{eodData.PrvEODcntr.ToString().PadLeft(totalWidth - 2, '0')}");
        //        writer.WriteLine($"15{eodData.PrvGrndTot.ToString("0.00").PadLeft(totalWidth - 2, '0')}");
        //        writer.WriteLine($"16{eodData.CurrEODcntr.ToString().PadLeft(totalWidth - 2, '0')}");
        //        writer.WriteLine($"17{eodData.CurrGrndTot.ToString("0.00").PadLeft(totalWidth - 2, '0')}");
        //        writer.WriteLine($"18{formattedDate.PadLeft(totalWidth - 2, '0')}");
        //        writer.WriteLine($"19{eodData.Novelty.ToString("0.00").PadLeft(totalWidth - 2, '0')}");
        //        writer.WriteLine($"20{eodData.Misc.ToString("0.00").PadLeft(totalWidth - 2, '0')}");
        //        writer.WriteLine($"21{eodData.LocTax.ToString("0.00").PadLeft(totalWidth - 2, '0')}");
        //        writer.WriteLine($"22{eodData.TotCrdSls.ToString("0.00").PadLeft(totalWidth - 2, '0')}");
        //        writer.WriteLine($"23{eodData.TotCrdVat.ToString("0.00").PadLeft(totalWidth - 2, '0')}");
        //        writer.WriteLine($"24{eodData.TotNonVat.ToString("0.00").PadLeft(totalWidth - 2, '0')}");
        //        writer.WriteLine($"25{eodData.PhrmSls.ToString("0.00").PadLeft(totalWidth - 2, '0')}");
        //        writer.WriteLine($"26{eodData.NonPhrmSls.ToString("0.00").PadLeft(totalWidth - 2, '0')}");
        //        writer.WriteLine($"27{eodData.DisabDisc.ToString("0.00").PadLeft(totalWidth - 2, '0')}");
        //        writer.WriteLine($"28{eodData.PercRent.ToString("0.00").PadLeft(totalWidth - 2, '0')}");
        //        writer.WriteLine($"29{eodData.TotAmntRePrnt.ToString("0.00").PadLeft(totalWidth - 2, '0')}");
        //        writer.WriteLine($"30{eodData.NoRePrnt.ToString().PadLeft(totalWidth - 2, '0')}");

        //    }
        //    filePaths.Add(fullFilePath);
        //    //sftpClient.SftpTrans(fullFilePath, host, username, password);
        //}
        #endregion

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


        private void SendFilesInBatch(List<string> filePaths)
        {
            foreach (string filePath in filePaths)
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

        private void CheckFileSendingStatus()
        {

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
    }
}
