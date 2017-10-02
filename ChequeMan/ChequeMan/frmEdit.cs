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

namespace ChequeMan
{
    public partial class frmEdit : Form
    {
        OleDbConnection dbcon = new OleDbConnection("PROVIDER=Microsoft.Jet.OLEDB.4.0; Data Source=ChequeDB.mdb");
        int EditID;

        public frmEdit(int ID, String Name, String Date, String Pay, String Rupees, String Rs)
        {
            InitializeComponent();
            EditID = ID;
            txtName.Text = Name;
            txtDate.Text = Date;
            txtPay.Text = Pay;
            txtRupees.Text = Rupees;
            txtRs.Text = Rs;
        }

        private bool IsFormValidated(String RefControl)
        {
            DateTime temp;
            bool no_errors = true;
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
            if ((RefControl == "txtPay") || (RefControl == "Update"))
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
            if ((RefControl == "txtRupees") || (RefControl == "Update"))
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
            if ((RefControl == "txtRs") || (RefControl == "Update"))
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
            if ((RefControl == "txtDate") || (RefControl == "Update"))
            {
                if (!DateTime.TryParseExact(txtDate.Text, "dd/MM/yyyy", new CultureInfo("en-US"), DateTimeStyles.None, out temp))
                {
                    this.errorProvider1.SetError(txtDate, "Date must be in DD/MM/YYYY format");
                    no_errors = false;
                } 
                else
                {
                    this.errorProvider1.SetError(txtDate, "");
                }
            }
            return no_errors;
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (IsFormValidated("Update"))
            {
                try
                {
                    dbcon.Open();
                    OleDbCommand cmdup = new OleDbCommand("UPDATE tblCheque SET [Name]=@name, [Date]=@date, [Pay]=@pay, [Rupees]=@rupees, [Rs]=@rs WHERE [ID] = @id", dbcon);
                    CultureInfo culture = new CultureInfo("ru-RU");
                    cmdup.Parameters.Add(new OleDbParameter("name", txtName.Text));
                    cmdup.Parameters.Add(new OleDbParameter("date", Convert.ToDateTime(txtDate.Text,culture)));
                    cmdup.Parameters.Add(new OleDbParameter("pay", txtPay.Text));
                    cmdup.Parameters.Add(new OleDbParameter("rupees", txtRupees.Text));
                    cmdup.Parameters.Add(new OleDbParameter("rs", txtRs.Text));
                    cmdup.Parameters.Add(new OleDbParameter("id", EditID));
                    cmdup.ExecuteNonQuery();
                    dbcon.Close();
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Exception in btnUpdate_Click(): " + ex.Message, "Exception Handler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void txtName_Validating(object sender, CancelEventArgs e)
        {
            IsFormValidated("txtName");
        }

        private void txtDate_Validating(object sender, CancelEventArgs e)
        {
            IsFormValidated("txtDate");
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
    }
}
