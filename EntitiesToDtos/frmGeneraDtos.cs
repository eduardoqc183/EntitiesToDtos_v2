using Dapper;
using EntitiesToDtos.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EntitiesToDtos
{
    public partial class frmGeneraDtos : Form
    {
        public int Total { get; set; }
        public static string CurrentTableName { get; set; }

        private int _Current;

        public int Current
        {
            get { return _Current; }
            set
            {
                if (value == _Current) return;
                _Current = value;
                var percent = (value * 100) / Total;
                percent = percent <= 100 ? percent : 100;
                progressBar1.Value = percent;
                progressBar1.Refresh();

                if (percent == 100)
                    SetStatus("Terminado");
            }
        }

        public string ConnectionString
        {
            get
            {
                var sb = new SqlConnectionStringBuilder
                {
                    UserID = txtUsername.Text,
                    Password = txtPassword.Text,
                    InitialCatalog = txtNombreBaseDatos.Text,
                    DataSource = txtHostname.Text
                };

                return sb.ConnectionString;
            }
        }

        public frmGeneraDtos()
        {
            InitializeComponent();
        }

        private void txtPassword_Validated(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            var fbd = new FolderBrowserDialog();
            var result = fbd.ShowDialog();
            if (result != DialogResult.OK) return;
            txtRutaDestinoDto.Text = fbd.SelectedPath;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var fbd = new FolderBrowserDialog();
            var result = fbd.ShowDialog();
            if (result != DialogResult.OK) return;
            txtRutaDestinoAssemblers.Text = fbd.SelectedPath;
        }

        private async void btnGenerar_Click(object sender, EventArgs e)
        {
            try
            {
                SetStatus("Conectando...");
                using (var cnx = new SqlConnection(ConnectionString))
                {
                    if (cnx.State != ConnectionState.Open) await cnx.OpenAsync();

                    cnx.InfoMessage += Cnx_InfoMessage_Dto;

                    var tablas = await cnx.QueryAsync<string>("SELECT name FROM SYSOBJECTS WHERE xtype = 'U' and name <> 'sysdiagrams';");
                    Total = tablas.Count() * 2;
                    Current = 1;

                    SetStatus("Creando Dtos...");
                    foreach (var tabla in tablas)
                    {
                        var qDto = System.IO.File.ReadAllText(@"Querys\QueryDTO.txt");
                        qDto = qDto.Replace("{xtablename}", tabla);
                        qDto = qDto.Replace("{xnsentities}", txtNamespaceEntidades.Text);
                        CurrentTableName = tabla;
                        await cnx.ExecuteAsync(qDto);
                        await Task.Delay(300);
                        Current++;
                    }

                    cnx.InfoMessage -= Cnx_InfoMessage_Dto;
                    cnx.InfoMessage += Cnx_InfoMessage_Assembler;

                    SetStatus("Creando Assemblers...");
                    foreach (var tabla in tablas)
                    {
                        var qAssemblers = System.IO.File.ReadAllText(@"Querys\QueryASSEMBLER.txt");
                        qAssemblers = qAssemblers.Replace("{xtablename}", tabla);
                        qAssemblers = qAssemblers.Replace("{xnsdatamodel}", txtNamespaceDatamodel.Text);
                        qAssemblers = qAssemblers.Replace("{xnsentities}", txtNamespaceEntidades.Text);
                        CurrentTableName = tabla;
                        await cnx.ExecuteAsync(qAssemblers);
                        await Task.Delay(300);
                        Current++;
                    }

                    cnx.InfoMessage -= Cnx_InfoMessage_Assembler;
                }
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message);
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Cnx_InfoMessage_Assembler(object sender, SqlInfoMessageEventArgs e)
        {
            var fileName = $"{CurrentTableName}Assembler.cs";
            System.IO.File.WriteAllText($"{txtRutaDestinoAssemblers.Text}\\{fileName}", e.Message);
        }

        private void Cnx_InfoMessage_Dto(object sender, SqlInfoMessageEventArgs e)
        {
            var fileName = $"{CurrentTableName}Dto.cs";
            System.IO.File.WriteAllText($"{txtRutaDestinoDto.Text}\\{fileName}", e.Message);
        }

        private void Cnx_InfoMessage_BL(object sender, SqlInfoMessageEventArgs e)
        {
            var fileName = $"{CurrentTableName.FirstCharToUpper()}Bl.cs";
            System.IO.File.WriteAllText($"{txtRutaDestinoBL.Text}\\{fileName}", e.Message);
        }

        private void Cnx_InfoMessage_Controller(object sender, SqlInfoMessageEventArgs e)
        {
            var fileName = $"{CurrentTableName.FirstCharToUpper()}Controller.cs";
            System.IO.File.WriteAllText($"{txtRutaDestinoControllers.Text}\\{fileName}", e.Message);
        }

        private void SetStatus(string s)
        {
            lblStatus.Text = s;
            lblStatus.Refresh();
        }

        private void frmGeneraDtos_Load(object sender, EventArgs e)
        {
            var config = Settings.Default;
            txtHostname.Text = config.hostname;
            txtUsername.Text = config.username;
            txtPassword.Text = config.password;
            txtNombreBaseDatos.Text = config.basedatos;
            txtNamespaceEntidades.Text = config.nsentidades;
            txtNamespaceDatamodel.Text = config.nsdatamodel;
            txtRutaDestinoDto.Text = config.rutadtos;
            txtRutaDestinoAssemblers.Text = config.rutaassemblers;
        }

        private void frmGeneraDtos_FormClosing(object sender, FormClosingEventArgs e)
        {
            var config = Settings.Default;
            config.hostname = txtHostname.Text;
            config.username = txtUsername.Text;
            config.password = txtPassword.Text;
            config.basedatos = txtNombreBaseDatos.Text;
            config.nsentidades = txtNamespaceEntidades.Text;
            config.nsdatamodel = txtNamespaceDatamodel.Text;
            config.rutadtos = txtRutaDestinoDto.Text;
            config.rutaassemblers = txtRutaDestinoAssemblers.Text;
            config.Save();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var fbd = new FolderBrowserDialog();
            var result = fbd.ShowDialog();
            if (result != DialogResult.OK) return;
            txtRutaDestinoBL.Text = fbd.SelectedPath;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            var fbd = new FolderBrowserDialog();
            var result = fbd.ShowDialog();
            if (result != DialogResult.OK) return;
            txtRutaDestinoControllers.Text = fbd.SelectedPath;
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            try
            {
                SetStatus("Conectando...");
                using (var cnx = new SqlConnection(ConnectionString))
                {
                    if (cnx.State != ConnectionState.Open) await cnx.OpenAsync();

                    cnx.InfoMessage += Cnx_InfoMessage_BL;

                    var tablas = await cnx.QueryAsync<string>("SELECT name FROM SYSOBJECTS WHERE xtype = 'U' and name <> 'sysdiagrams';");
                    Total = tablas.Count() /** 2*/;
                    Current = 1;

                    SetStatus("Creando BLs...");
                    foreach (var tabla in tablas)
                    {
                        var qBl = System.IO.File.ReadAllText(@"Querys\QueryBL.txt");
                        qBl = qBl.Replace("{xtablename}", tabla);
                        qBl = qBl.Replace("{xnsentities}", txtNamespaceEntidades.Text);
                        qBl = qBl.Replace("{xnsdatamodel}", txtNamespaceDatamodel.Text);
                        qBl = qBl.Replace("{xnsbussineslayer}", txtNamespaceBussinesLayer.Text);
                        CurrentTableName = tabla;
                        await cnx.ExecuteAsync(qBl);
                        await Task.Delay(300);
                        Current++;
                    }

                    cnx.InfoMessage -= Cnx_InfoMessage_BL;
                    cnx.InfoMessage += Cnx_InfoMessage_Controller;

                    SetStatus("Creando Controllers...");
                    foreach (var tabla in tablas)
                    {
                        var qControllers = System.IO.File.ReadAllText(@"Querys\QueryControllers.txt");
                        qControllers = qControllers.Replace("{xtablename}", tabla);
                        CurrentTableName = tabla;
                        await cnx.ExecuteAsync(qControllers);
                        await Task.Delay(300);
                        Current++;
                    }

                    cnx.InfoMessage -= Cnx_InfoMessage_Controller;
                }
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message);
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Cnx_InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            throw new NotImplementedException();
        }

        private async void button6_Click(object sender, EventArgs e)
        {
            try
            {
                SetStatus("Conectando...");
                using (var cnx = new SqlConnection(ConnectionString))
                {
                    if (cnx.State != ConnectionState.Open) await cnx.OpenAsync();
                    
                    var tablas = await cnx.QueryAsync<string>("SELECT name FROM SYSOBJECTS WHERE xtype = 'U' and name <> 'sysdiagrams';");
                    Total = tablas.Count() /** 2*/;
                    Current = 1;

                    cnx.InfoMessage += Cnx_InfoMessage_Controller;

                    SetStatus("Creando Controllers...");
                    foreach (var tabla in tablas)
                    {
                        var qControllers = System.IO.File.ReadAllText(@"Querys\QueryControllers.txt");
                        qControllers = qControllers.Replace("{xtablename}", tabla);
                        CurrentTableName = tabla;
                        await cnx.ExecuteAsync(qControllers);
                        await Task.Delay(300);
                        Current++;
                    }

                    cnx.InfoMessage -= Cnx_InfoMessage_Controller;
                }
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message);
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
