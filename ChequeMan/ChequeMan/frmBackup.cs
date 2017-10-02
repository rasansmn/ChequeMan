using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace ChequeMan
{
    public partial class frmBackup : Form
    {
        public frmBackup()
        {
            InitializeComponent();
        }

        private void btnBackup_Click(object sender, EventArgs e)
        {
            string CurrentDatabasePath = Environment.CurrentDirectory + @"\ChequeDB.mdb";
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                string PathtobackUp = fbd.SelectedPath.ToString();
                File.Copy(CurrentDatabasePath, PathtobackUp + @"\ChequeDB.mdb", true);
                MessageBox.Show("Back Up SuccessFull!","Backup", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Exception in btnBackup_Click(): " + ex.Message, "Exception Handler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            string PathToRestoreDB = Environment.CurrentDirectory + @"\ChequeDB.mdb";
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (ofd.SafeFileName != "ChequeDB.mdb")
                {
                    MessageBox.Show("Invalid File!", "Restore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    try
                    {
                    string Filetorestore = ofd.FileName;
                    // Rename Current Database to .Bak
                    File.Delete(PathToRestoreDB + ".Bak"); //If already exist delete it.
                    File.Move(PathToRestoreDB, PathToRestoreDB + ".Bak");
                    //Restore the Databse From Backup Folder
                    File.Copy(Filetorestore, PathToRestoreDB, true);
                    MessageBox.Show("Database Restored Successfully!", "Restore", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Exception in btnRestore_Click(): " + ex.Message, "Exception Handler", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
