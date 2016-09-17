using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DataBrowser.ViewModels;
using System.Data.SqlClient;

namespace DataBrowser
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private SqlConnection connection;

        private void btnConnect_Click(object sender, EventArgs e)
        {
            var connectionForm = new ConnectionForm();
            if (connectionForm.ShowDialog() == DialogResult.OK)
            {
                //get connection parameters
                var connectionData = connectionForm.GetConnectionData();
                ConnectDatabaseAsync(connectionData);
            }
        }

        private async void ConnectDatabaseAsync(ConnectionData connectionData)
        {
            var cnnString = connectionData.GetConnectionString();
            Text = $"SEDC Data Browser - connecting to {connectionData.ServerName}";

            connection = new SqlConnection(cnnString);
            try
            {
                await connection.OpenAsync();

                DataTable databases = connection.GetSchema("Databases");
                var databaseNames = new List<string>();
                foreach (DataRow database in databases.Rows)
                {
                    databaseNames.Add(database.Field<string>("database_name"));
                }
                cbxDatabases.DataSource = databaseNames;

                Text = $"SEDC Data Browser - connected to {connectionData.ServerName}";
            }
            catch (SqlException ex)
            {
                Text = $"SEDC Data Browser - connecting to {connectionData.ServerName} failed";
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {

        }

        private void cbxDatabases_SelectedIndexChanged(object sender, EventArgs e)
        {
            var databaseName = (string)cbxDatabases.SelectedItem;
            connection.ChangeDatabase(databaseName);
            var dataTables = connection.GetSchema("Tables");
            var tableNames = new List<string>();
            foreach (DataRow table in dataTables.Rows)
            {
                tableNames.Add(table.Field<string>("TABLE_NAME"));
            }
            lbxTables.DataSource = tableNames;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
                connection.Dispose();
            }
            base.Dispose(disposing);
        }

        private async void lbxTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            var databaseName = (string)cbxDatabases.SelectedItem;
            var tableName = (string)lbxTables.SelectedItem;

            var dataColumns = connection.GetSchema("Columns", new string[] { null, null, tableName });//look at the documentation for the meaning of these values

            var columnNames = new List<string>();
            foreach (DataRow table in dataColumns.Rows)
            {
                columnNames.Add(table.Field<string>("COLUMN_NAME"));
            }

            dgvData.Columns.Clear();

            foreach (var cname in columnNames)
            {
                dgvData.Columns.Add(cname, cname);
            }

            using (var command = new SqlCommand())
            {
                command.Connection = connection;
                command.CommandText = $"select top 10 * from {tableName}";

                //old

                //var result = command.BeginExecuteReader();
                //MessageBox.Show("started");
                //using (var dataReader = command.EndExecuteReader(result))
                //{
                //    MessageBox.Show("done");
                //}

                //new

                using (var dataReader = await command.ExecuteReaderAsync())
                {
                    while (dataReader.Read())
                    {
                        var values = new object[dataReader.FieldCount];
                        dataReader.GetValues(values);
                        dgvData.Rows.Add(values);
                    }
                    
                }
                
            }
        }
    }
}
