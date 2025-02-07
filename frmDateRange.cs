using RobinsonMallLinkingWBOX.Constants;
using RobinsonMallLinkingWBOX.Helpers;
using RobinsonMallLinkingWBOX.LocalStorage;
using RobinsonMallLinkingWBOX.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
        private static NotifyIcon _notifyIcon;
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
        public string xml, host, username, password, dir, tenantname, terminal = null;
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
        public frmDateRange()
        {
            InitializeComponent();
        }
        private void btnClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        private void btnGenerate_Click(object sender, EventArgs e)
        {
            DateTime start = dtpFrom.Value;
            DateTime end = dtpTo.Value;

            for(var day = start; start <= end; day = day.AddDays(1))
            {
                PerformEOD(day.ToString("mmddyyyy"), day.ToString("yyyymmdd"), day.AddDays(-1).ToString("yyyymmdd"));
            }
        }

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
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}\n{ex.StackTrace}");
                ErrorLogger.LogError($"Error: {ex.Message}\n{ex.StackTrace}");
            }
        }
        private void GenerateFixedWidthFile(EODSLSData eodData, string folderPath, string fileName, List<string> filePaths)
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
                GlobalVar.batchcount = batchCount;
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
                GlobalVar.batchcount = batchCount;
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
    }
}
