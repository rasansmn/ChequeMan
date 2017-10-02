using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;
using System.Globalization;
using System.Drawing.Printing;
using Calendar.NET;
using System.Management;

namespace ChequeMan
{
    public partial class frmMain : Form
    {
        OleDbConnection dbcon = new OleDbConnection("PROVIDER=Microsoft.Jet.OLEDB.4.0; Data Source=ChequeDB.mdb");
        DataSet dset = new DataSet();
        OleDbDataAdapter dadapt;
        int reference = 1;
        String[] selectedrecord = new String[6];

        public frmMain()
        {
            InitializeComponent();
        }

        private void loadgrid(int reference)
        {
            string query = null;
            switch (reference)
            {
                case (1): //showing all
                    query = "SELECT [ID] as [Cheque No], [Name], FORMAT([Date], 'dd/MM/yyyy') as [Date], [Pay], [Rupees], [Rs] FROM tblCheque";
                    lblGridCaption.Text = "Showing all records";
                    break;
                case (2): //calenderpick
                    query = "SELECT [ID] as [Cheque No], [Name], FORMAT([Date], 'dd/MM/yyyy') as [Date], [Pay], [Rupees], [Rs] FROM tblCheque WHERE [Date] = #" + calendar1.SelectedEventDate + "#";
                    lblGridCaption.Text = "Showing records on: " + calendar1.SelectedEventDate.ToString("dd/MM/yyyy");
                    break;
                case (3): //search
                    var builder = new System.Data.SqlClient.SqlCommandBuilder();
                    string fieldname = builder.QuoteIdentifier(cmbLookin.Text);
                    if ((cmbLookin.Text.Trim() == "")||(cmbLookin.Text=="Cheque No"))
                    {
                        fieldname = "[ID]";
                    }
                    query = "SELECT [ID] as [Cheque No], [Name], FORMAT([Date], 'dd/MM/yyyy') as [Date], [Pay], [Rupees], [Rs] FROM tblCheque WHERE " + fieldname + " LIKE '%' + @value + '%'";
                    lblGridCaption.Text = "Search " + fieldname + " - " + txtSearch.Text;
                    break;
            }
            try
            {
                dset.Clear();
                OleDbCommand cmd = new OleDbCommand(query, dbcon);
                cmd.Parameters.Add(new OleDbParameter("value", txtSearch.Text)); //parameter when use search
                dadapt = new OleDbDataAdapter(cmd);
                dadapt.Fill(dset, "tblCheque");
                dgvCheque.DataSource = dset;
                dgvCheque.DataMember = "tblCheque";
                dgvCheque.ClearSelection();
                selectedrecord[0] = null;
                selectedrecord[1] = null;
                selectedrecord[2] = null;
                selectedrecord[3] = null;
                selectedrecord[4] = null;
                btnPrint.Enabled = false;
                btnEdit.Enabled = false;
                btnDelete.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception in loadgrid(): " + ex.Message, "Exception Handler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void loadcalendar()
        {
            try
            {
                calendar1.Clearall();
                DataSet dsDates = new DataSet();
                dsDates.Clear();
                dadapt = new OleDbDataAdapter("SELECT DISTINCT [Date] from tblCheque", dbcon);
                dadapt.Fill(dsDates, "tblDates");
                DataSet dsChequesOfDay = new DataSet();
                String ChequesOfDay;
                foreach (DataRow Date in dsDates.Tables["tblDates"].Rows)
                {
                    OleDbDataAdapter daItems = new OleDbDataAdapter("SELECT [Name], [Rs] FROM tblCheque WHERE [Date] = #" + Date["Date"]+ "#", dbcon);
                    dsChequesOfDay.Clear();
                    daItems.Fill(dsChequesOfDay, "tblChequesOfDay");
                    ChequesOfDay = "";
                    foreach (DataRow item in dsChequesOfDay.Tables["tblChequesOfDay"].Rows)
                    {
                        ChequesOfDay += item["Name"] + ": " + item["Rs"] + "\n";
                    }
                    var chequeday = new CustomEvent();
                    chequeday.Date = Convert.ToDateTime(Date["Date"]);
                    chequeday.EventText = "DFCC";
                    chequeday.EventItems = ChequesOfDay;
                    chequeday.IgnoreTimeComponent = true;
                    calendar1.AddEvent(chequeday);
                    daItems.Dispose();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Exception in loadcalendar(): " + ex.Message, "Exception Handler", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintChequeReport(String Date, String Pay, String Rupees, String Rs)
        {
            //Create new table in a dataset
            DataSet ds = new DataSet();
            DataTable dt = new DataTable("tblCheque");
            dt.Columns.Add(new DataColumn("Date", typeof(string)));
            dt.Columns.Add(new DataColumn("Pay", typeof(string)));
            dt.Columns.Add(new DataColumn("Rupees", typeof(string)));
            dt.Columns.Add(new DataColumn("Rs", typeof(string)));
            //inserting new record to the table
            DataRow dr = dt.NewRow();
            dr["Date"] = BuildDateToPrint(Date);
            dr["Pay"] = Pay;
            dr["Rupees"] = Rupees;
            dr["Rs"] = Rs;
            dt.Rows.Add(dr);
            ds.Tables.Add(dt);//inserted record is now ready to view in crystal report (ds) 

            PrinterSettings settings = new PrinterSettings();
            string printerName = settings.PrinterName;
            if (!isPrinteroffline(printerName))
            {
                //direct print!
                rptDFCC_CHEQUE cr = new rptDFCC_CHEQUE(); // here rptDFCC_CHEQUE is the Crystal Report
                System.Drawing.Printing.PrintDocument doctoprint = new System.Drawing.Printing.PrintDocument();
                doctoprint.PrinterSettings.PrinterName = printerName;
                int rawKind = 0;
                for (int i = 0; i <= doctoprint.PrinterSettings.PaperSizes.Count - 1; i++)
                {
                    if (doctoprint.PrinterSettings.PaperSizes[i].PaperName == "DFCC-CHEQUE") // "DFCC-CHEQUE : Paper Size"
                    {
                        rawKind = Convert.ToInt32(doctoprint.PrinterSettings.PaperSizes[i].GetType().GetField
                         ("kind",
                        System.Reflection.BindingFlags.Instance |
                        System.Reflection.BindingFlags.NonPublic).GetValue(doctoprint.PrinterSettings.PaperSizes[i]));
                        break;
                    }
                }
                cr.PrintOptions.PaperSize = (CrystalDecisions.Shared.PaperSize)rawKind;
                cr.SetDataSource(ds);
                cr.PrintToPrinter(1, false, 1, 2);
                MessageBox.Show("Cheque is sent to the " + printerName);
            }
            else
            {
                MessageBox.Show(printerName + " is workoffline!");
            }
            btnPrint.Text = "Print";
            btnPrint.Enabled = true;
           
            //Show in cristal report viewer!
            //ChequeReport chkrpt = new ChequeReport(ds);
            //chkrpt.Show();  
        }

        private void ResetForm()
        {
            txtD1.Text = DateTime.Now.ToString("ddMMyy").Substring(0, 1);
            txtD2.Text = DateTime.Now.ToString("ddMMyy").Substring(1, 1);
            txtM1.Text = DateTime.Now.ToString("ddMMyy").Substring(2, 1);
            txtM2.Text = DateTime.Now.ToString("ddMMyy").Substring(3, 1);
            txtY3.Text = DateTime.Now.ToString("ddMMyy").Substring(4, 1);
            txtY4.Text = DateTime.Now.ToString("ddMMyy").Substring(5, 1);
            txtID.Clear();
            txtName.Clear();
            txtPay.Clear();
            txtRupees.Clear();
            txtRs.Clear();
            this.errorProvider1.Clear();
        }

        private bool IsFormValidated(String RefControl)
        {
            bool no_errors = true;
            DateTime temp;
            int x;
            if ((RefControl == "txtID") || (RefControl == "Submit"))
            {
                if (!int.TryParse(txtID.Text, out x))
                {
                    this.errorProvider1.SetError(txtID, "Enter an valid Cheque No.");
                    no_errors = false;
                }
                else
                {
                    try
                    {
                        dbcon.Open();
                        OleDbCommand cmd = new OleDbCommand("SELECT COUNT (*) FROM tblCheque WHERE ID =" + txtID.Text, dbcon);
                        if ((int)cmd.ExecuteScalar() > 0)
                        {
                            this.errorProvider1.SetError(txtID, "This Cheque Number is already exist");
                            no_errors = false;
                        }
                        else
                        {
                            this.errorProvider1.SetError(txtID, "");
                        }
                        dbcon.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Exception in IsFormValidated(): " + ex.Message, "Exception Handler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            if ((RefControl == "txtName") || (RefControl == "Submit"))
            {
                if (txtName.Text == string.Empty)
                {
                    this.errorProvider1.SetError(txtName, "Enter the name");
                    no_errors = false;
                }
                else
                {
                    this.errorProvider1.SetError(txtName, "");
                }
            }
            if ((RefControl == "txtPay") || (RefControl == "Submit"))
            {
                if (txtPay.Text == string.Empty)
                {
                    this.errorProvider1.SetError(txtPay, "Enter text Pay");
                    no_errors = false;
                }
                else
                {
                    this.errorProvider1.SetError(txtPay, "");
                }
            }
            if ((RefControl == "txtRupees") || (RefControl == "Submit"))
            {
                if (txtRupees.Text == string.Empty)
                {
                    this.errorProvider1.SetError(txtRupees, "Enter text Rupees");
                    no_errors = false;
                }
                else
                {
                    this.errorProvider1.SetError(txtRupees, "");
                }
            }
            if ((RefControl == "txtRs") || (RefControl == "Submit"))
            {
                if (txtRs.Text == string.Empty)
                {
                    this.errorProvider1.SetError(txtRs, "Enter text Rs");
                    no_errors = false;
                }
                else
                {
                    this.errorProvider1.SetError(txtRs, "");
                }
            }
            if ((RefControl == "Date") || (RefControl == "Submit"))
            {
                if (!DateTime.TryParseExact(BuildDateOnForm(), "dd/MM/yyyy", new CultureInfo("en-US"), DateTimeStyles.None, out temp))
                {
                    this.errorProvider1.SetError(txtY4, "Date must be in DDMMYYYY format");
                    no_errors = false;
                }
                else
                {
                    this.errorProvider1.SetError(txtY4, "");
                }
            }
            return no_errors;
        }

        private string BuildDateOnForm() // "DD/MM/YYYY"
        {
            String FormDate;
            FormDate = txtD1.Text;
            FormDate += txtD2.Text;
            FormDate += "/";
            FormDate += txtM1.Text;
            FormDate += txtM2.Text;
            FormDate += "/";
            FormDate += txtY1.Text;
            FormDate += txtY2.Text;
            FormDate += txtY3.Text;
            FormDate += txtY4.Text;
            return FormDate;
        }

        private string BuildDateToPrint(String Date) // "DDMM  YY"
        {
            String DateToPrint = Date.Substring(0, 2);
            DateToPrint += Date.Substring(3, 2);
            DateToPrint += "  ";
            DateToPrint += Date.Substring(8, 2);
            return DateToPrint;
        }

        private string NumberToWords(int number)
        {
            if (number == 0)
                return "Zero";
            if (number < 0)
                return "Minus " + NumberToWords(Math.Abs(number));
            string words = "";
            if ((number / 1000000) > 0)
            {
                words += NumberToWords(number / 1000000) + " Million ";
                number %= 1000000;
            }
            if ((number / 1000) > 0)
            {
                words += NumberToWords(number / 1000) + " Thousand ";
                number %= 1000;
            }
            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + " Hundred ";
                number %= 100;
            }
            if (number > 0)
            {
                if (words != "")
                    words += "and ";
                var unitsMap = new[] { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
                var tensMap = new[] { "Zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };
                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }
            return words.TrimEnd();
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if (IsFormValidated("Submit"))
            {
                try
                {
                    dbcon.Open();
                    OleDbCommand cmdin = new OleDbCommand("INSERT INTO tblCheque ([ID], [Name], [Date], [Pay], [Rupees], [Rs]) VALUES (@ID, @Name, @Date, @Pay, @Rupees, @RS)", dbcon);
                    CultureInfo culture = new CultureInfo("ru-RU");
                    cmdin.Parameters.Add(new OleDbParameter("ID", txtID.Text));
                    cmdin.Parameters.Add(new OleDbParameter("Name", txtName.Text));
                    cmdin.Parameters.Add(new OleDbParameter("Date", Convert.ToDateTime(BuildDateOnForm(),culture)));
                    cmdin.Parameters.Add(new OleDbParameter("Pay", txtPay.Text));
                    cmdin.Parameters.Add(new OleDbParameter("Rupees", txtRupees.Text));
                    cmdin.Parameters.Add(new OleDbParameter("Rs", txtRs.Text));
                    cmdin.ExecuteNonQuery();
                    reference = 1;
                    loadgrid(reference);
                    loadcalendar();
                    if (MessageBox.Show("Record added successfully.\n\nDo you want to print this cheque now?", "Cheque", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        PrintChequeReport(BuildDateOnForm(), txtPay.Text, txtRupees.Text, txtRs.Text);
                    }
                    dbcon.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Exception in btnSubmit_Click: " + ex.Message, "Exception Handler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    ResetForm();
                }
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            ResetForm();
            loadgrid(reference);
            loadcalendar();
            calendar1.CalendarDate = DateTime.Now;
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            reference = 3;
            loadgrid(reference);
        }

        private void btnShowall_Click(object sender, EventArgs e)
        {
            reference = 1;
            loadgrid(reference);
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            btnPrint.Text = "Processing...";
            btnPrint.Enabled = false;
            PrintChequeReport(selectedrecord[2], selectedrecord[3], selectedrecord[4], selectedrecord[5]);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Cheque No: " + selectedrecord[0] + "\n\nDo you want to delete this record?", "Delete", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                try
                {
                    dbcon.Open();
                    OleDbCommand del = new OleDbCommand("DELETE FROM tblCheque WHERE ID =" + selectedrecord[0], dbcon);
                    del.ExecuteNonQuery();
                    loadgrid(reference); //showing current query
                    loadcalendar();
                    dbcon.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Exception in btnDelete_Click(): " + ex.Message, "Exception Handler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            ResetForm();
        }

        private void txtID_Validating(object sender, CancelEventArgs e)
        {
            IsFormValidated("txtID");
        }

        private void txtName_Validating(object sender, CancelEventArgs e)
        {
            IsFormValidated("txtName");
        }

        private void txtPay_Validating(object sender, CancelEventArgs e)
        {
            IsFormValidated("txtPay");
        }

        private void txtRupees_Validating(object sender, CancelEventArgs e)
        {
            IsFormValidated("txtRupees");
        }

        private void txtRs_Validating(object sender, CancelEventArgs e)
        {
            IsFormValidated("txtRs");
        }

        private void txtD1_Validating(object sender, CancelEventArgs e)
        {
            IsFormValidated("Date");
        }

        private void txtD2_Validating(object sender, CancelEventArgs e)
        {
            IsFormValidated("Date");
        }

        private void txtM1_Validating(object sender, CancelEventArgs e)
        {
            IsFormValidated("Date");
        }

        private void txtM2_Validating(object sender, CancelEventArgs e)
        {
            IsFormValidated("Date");
        }

        private void txtY3_Validating(object sender, CancelEventArgs e)
        {
            IsFormValidated("Date");
        }

        private void txtY4_Validating(object sender, CancelEventArgs e)
        {
            IsFormValidated("Date");
        }

        private void dgvCheque_SelectionChanged(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvCheque.SelectedRows)
            {
                selectedrecord[0] = row.Cells[0].Value.ToString();
                selectedrecord[1] = row.Cells[1].Value.ToString();
                selectedrecord[2] = row.Cells[2].Value.ToString();
                selectedrecord[3] = row.Cells[3].Value.ToString();
                selectedrecord[4] = row.Cells[4].Value.ToString();
                selectedrecord[5] = row.Cells[5].Value.ToString();
            }
            if (Convert.ToUInt32(selectedrecord[0]) > 0)
            {
                btnPrint.Enabled = true;
                btnEdit.Enabled = true;
                btnDelete.Enabled = true;
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            var frmEditRecord = new frmEdit(Convert.ToInt32(selectedrecord[0]), selectedrecord[1], selectedrecord[2], selectedrecord[3], selectedrecord[4], selectedrecord[5]);
            if (frmEditRecord.ShowDialog() == DialogResult.OK)
            {
                loadgrid(reference);
                loadcalendar();
            }
        }

        private void calendar1_Click(object sender, EventArgs e)
        {
            if (calendar1.Pointedtoevent)
            {
                reference = 2;
                loadgrid(reference);
            }
        }

        private void txtRs_TextChanged(object sender, EventArgs e)
        {
            int val;
            if (int.TryParse(txtRs.Text, out val))
            {
                txtRupees.Text = NumberToWords(val) + " Rupees Only";
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void printerSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var PrintSettings = new frmPrinterSettings();
            PrintSettings.ShowDialog();
        }

        private void backupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var Backup = new frmBackup();
            Backup.ShowDialog();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var About = new frmAbout();
            About.ShowDialog();
        }

        private bool isPrinteroffline(String printername)
        {
            string query = string.Format("SELECT * from Win32_Printer WHERE Name LIKE '%{0}'", printername);
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            ManagementObjectCollection coll = searcher.Get();
            foreach (ManagementObject printer in coll)
            {
                PropertyData prop = printer.Properties["WorkOffline"];
                return (bool)prop.Value;
            }
            return true;
        }
    }
}
