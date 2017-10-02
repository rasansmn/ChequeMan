using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CrystalDecisions.CrystalReports.Engine;

namespace ChequeMan
{
    public partial class ChequeReport : Form
    {
        DataSet reportds;

        public ChequeReport(DataSet ds)
        {
            InitializeComponent();
            reportds = ds;
        }

        private void ChequeReport_Load(object sender, EventArgs e)
        {
            ReportDocument rdoc = new rptDFCC_CHEQUE();
            rdoc.SetDataSource(reportds);
            crystalReportViewer1.ReportSource = rdoc;
        }
    }
}
