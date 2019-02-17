namespace rc_car_config
{
    partial class FormPrincipal
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea5 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend5 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series5 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.chartDatos = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.buttonResetZoomGraf = new System.Windows.Forms.Button();
            this.buttonLimpiarGraf = new System.Windows.Forms.Button();
            this.buttonContinarGrafica = new System.Windows.Forms.Button();
            this.buttonDetenerGrafica = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.panelGrafico = new System.Windows.Forms.Panel();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.textLogEventos = new System.Windows.Forms.RichTextBox();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.textconfig6 = new System.Windows.Forms.TextBox();
            this.configL6 = new System.Windows.Forms.Label();
            this.textconfig5 = new System.Windows.Forms.TextBox();
            this.configL5 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textconfig3 = new System.Windows.Forms.TextBox();
            this.textconfig1 = new System.Windows.Forms.TextBox();
            this.textconfig2 = new System.Windows.Forms.TextBox();
            this.textconfig4 = new System.Windows.Forms.TextBox();
            this.configL1 = new System.Windows.Forms.Label();
            this.configL2 = new System.Windows.Forms.Label();
            this.configL3 = new System.Windows.Forms.Label();
            this.configL4 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.configL18 = new System.Windows.Forms.Label();
            this.configL17 = new System.Windows.Forms.Label();
            this.configL16 = new System.Windows.Forms.Label();
            this.configL15 = new System.Windows.Forms.Label();
            this.textconfig18 = new System.Windows.Forms.TextBox();
            this.textconfig17 = new System.Windows.Forms.TextBox();
            this.textconfig16 = new System.Windows.Forms.TextBox();
            this.textconfig15 = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.configL14 = new System.Windows.Forms.Label();
            this.configL13 = new System.Windows.Forms.Label();
            this.configL12 = new System.Windows.Forms.Label();
            this.configL11 = new System.Windows.Forms.Label();
            this.textconfig14 = new System.Windows.Forms.TextBox();
            this.textconfig13 = new System.Windows.Forms.TextBox();
            this.textconfig12 = new System.Windows.Forms.TextBox();
            this.textconfig11 = new System.Windows.Forms.TextBox();
            this.labelFirmareVer = new System.Windows.Forms.Label();
            this.labelFirmware = new System.Windows.Forms.Label();
            this.groupBoxTipoRegula = new System.Windows.Forms.GroupBox();
            this.radioButtonLQR = new System.Windows.Forms.RadioButton();
            this.radioButtonPID = new System.Windows.Forms.RadioButton();
            this.label6 = new System.Windows.Forms.Label();
            this.textconfig7 = new System.Windows.Forms.TextBox();
            this.textconfig8 = new System.Windows.Forms.TextBox();
            this.textconfig9 = new System.Windows.Forms.TextBox();
            this.textconfig10 = new System.Windows.Forms.TextBox();
            this.configL7 = new System.Windows.Forms.Label();
            this.configL8 = new System.Windows.Forms.Label();
            this.configL9 = new System.Windows.Forms.Label();
            this.configL10 = new System.Windows.Forms.Label();
            this.buttonGuardarCalibracion = new System.Windows.Forms.Button();
            this.comboPuertoSerie = new System.Windows.Forms.ComboBox();
            this.buttonConectar = new System.Windows.Forms.Button();
            this.serialPortBluetooth = new System.IO.Ports.SerialPort(this.components);
            this.timerActualizar = new System.Windows.Forms.Timer(this.components);
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartDatos)).BeginInit();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBoxTipoRegula.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Location = new System.Drawing.Point(12, 42);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1211, 539);
            this.tabControl1.TabIndex = 20;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.chartDatos);
            this.tabPage1.Controls.Add(this.buttonResetZoomGraf);
            this.tabPage1.Controls.Add(this.buttonLimpiarGraf);
            this.tabPage1.Controls.Add(this.buttonContinarGrafica);
            this.tabPage1.Controls.Add(this.buttonDetenerGrafica);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1203, 513);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Graficas";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // chartDatos
            // 
            chartArea5.Name = "ChartArea1";
            this.chartDatos.ChartAreas.Add(chartArea5);
            legend5.Name = "Legend1";
            this.chartDatos.Legends.Add(legend5);
            this.chartDatos.Location = new System.Drawing.Point(4, 4);
            this.chartDatos.Name = "chartDatos";
            series5.ChartArea = "ChartArea1";
            series5.Legend = "Legend1";
            series5.Name = "Series1";
            this.chartDatos.Series.Add(series5);
            this.chartDatos.Size = new System.Drawing.Size(1196, 503);
            this.chartDatos.TabIndex = 14;
            this.chartDatos.Text = "grafica";
            // 
            // buttonResetZoomGraf
            // 
            this.buttonResetZoomGraf.Location = new System.Drawing.Point(219, 480);
            this.buttonResetZoomGraf.Name = "buttonResetZoomGraf";
            this.buttonResetZoomGraf.Size = new System.Drawing.Size(81, 27);
            this.buttonResetZoomGraf.TabIndex = 13;
            this.buttonResetZoomGraf.Text = "Reset zoom";
            this.buttonResetZoomGraf.UseVisualStyleBackColor = true;
            // 
            // buttonLimpiarGraf
            // 
            this.buttonLimpiarGraf.Location = new System.Drawing.Point(958, 480);
            this.buttonLimpiarGraf.Name = "buttonLimpiarGraf";
            this.buttonLimpiarGraf.Size = new System.Drawing.Size(81, 27);
            this.buttonLimpiarGraf.TabIndex = 12;
            this.buttonLimpiarGraf.Text = "Limpiar";
            this.buttonLimpiarGraf.UseVisualStyleBackColor = true;
            // 
            // buttonContinarGrafica
            // 
            this.buttonContinarGrafica.Location = new System.Drawing.Point(113, 480);
            this.buttonContinarGrafica.Name = "buttonContinarGrafica";
            this.buttonContinarGrafica.Size = new System.Drawing.Size(81, 27);
            this.buttonContinarGrafica.TabIndex = 11;
            this.buttonContinarGrafica.Text = "Continuar";
            this.buttonContinarGrafica.UseVisualStyleBackColor = true;
            // 
            // buttonDetenerGrafica
            // 
            this.buttonDetenerGrafica.Location = new System.Drawing.Point(16, 480);
            this.buttonDetenerGrafica.Name = "buttonDetenerGrafica";
            this.buttonDetenerGrafica.Size = new System.Drawing.Size(81, 27);
            this.buttonDetenerGrafica.TabIndex = 10;
            this.buttonDetenerGrafica.Text = "detener";
            this.buttonDetenerGrafica.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.panelGrafico);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1203, 513);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Visual";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // panelGrafico
            // 
            this.panelGrafico.Location = new System.Drawing.Point(0, 0);
            this.panelGrafico.Name = "panelGrafico";
            this.panelGrafico.Size = new System.Drawing.Size(1051, 513);
            this.panelGrafico.TabIndex = 2;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.textLogEventos);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(1203, 513);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Log";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // textLogEventos
            // 
            this.textLogEventos.Location = new System.Drawing.Point(6, 3);
            this.textLogEventos.Name = "textLogEventos";
            this.textLogEventos.Size = new System.Drawing.Size(1134, 472);
            this.textLogEventos.TabIndex = 0;
            this.textLogEventos.Text = "";
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.textconfig6);
            this.tabPage4.Controls.Add(this.configL6);
            this.tabPage4.Controls.Add(this.textconfig5);
            this.tabPage4.Controls.Add(this.configL5);
            this.tabPage4.Controls.Add(this.groupBox1);
            this.tabPage4.Controls.Add(this.label16);
            this.tabPage4.Controls.Add(this.configL18);
            this.tabPage4.Controls.Add(this.configL17);
            this.tabPage4.Controls.Add(this.configL16);
            this.tabPage4.Controls.Add(this.configL15);
            this.tabPage4.Controls.Add(this.textconfig18);
            this.tabPage4.Controls.Add(this.textconfig17);
            this.tabPage4.Controls.Add(this.textconfig16);
            this.tabPage4.Controls.Add(this.textconfig15);
            this.tabPage4.Controls.Add(this.label11);
            this.tabPage4.Controls.Add(this.configL14);
            this.tabPage4.Controls.Add(this.configL13);
            this.tabPage4.Controls.Add(this.configL12);
            this.tabPage4.Controls.Add(this.configL11);
            this.tabPage4.Controls.Add(this.textconfig14);
            this.tabPage4.Controls.Add(this.textconfig13);
            this.tabPage4.Controls.Add(this.textconfig12);
            this.tabPage4.Controls.Add(this.textconfig11);
            this.tabPage4.Controls.Add(this.labelFirmareVer);
            this.tabPage4.Controls.Add(this.labelFirmware);
            this.tabPage4.Controls.Add(this.groupBoxTipoRegula);
            this.tabPage4.Controls.Add(this.buttonGuardarCalibracion);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(1203, 513);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Calibrar";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // textconfig6
            // 
            this.textconfig6.Enabled = false;
            this.textconfig6.Location = new System.Drawing.Point(120, 315);
            this.textconfig6.Name = "textconfig6";
            this.textconfig6.Size = new System.Drawing.Size(100, 20);
            this.textconfig6.TabIndex = 54;
            this.textconfig6.Tag = "6";
            this.textconfig6.Visible = false;
            // 
            // configL6
            // 
            this.configL6.AutoSize = true;
            this.configL6.Location = new System.Drawing.Point(34, 318);
            this.configL6.Name = "configL6";
            this.configL6.Size = new System.Drawing.Size(20, 13);
            this.configL6.TabIndex = 55;
            this.configL6.Tag = "6";
            this.configL6.Text = "K4";
            this.configL6.Visible = false;
            // 
            // textconfig5
            // 
            this.textconfig5.Enabled = false;
            this.textconfig5.Location = new System.Drawing.Point(120, 268);
            this.textconfig5.Name = "textconfig5";
            this.textconfig5.Size = new System.Drawing.Size(100, 20);
            this.textconfig5.TabIndex = 52;
            this.textconfig5.Tag = "5";
            this.textconfig5.Visible = false;
            // 
            // configL5
            // 
            this.configL5.AutoSize = true;
            this.configL5.Location = new System.Drawing.Point(34, 271);
            this.configL5.Name = "configL5";
            this.configL5.Size = new System.Drawing.Size(20, 13);
            this.configL5.TabIndex = 53;
            this.configL5.Tag = "5";
            this.configL5.Text = "K4";
            this.configL5.Visible = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.textconfig3);
            this.groupBox1.Controls.Add(this.textconfig1);
            this.groupBox1.Controls.Add(this.textconfig2);
            this.groupBox1.Controls.Add(this.textconfig4);
            this.groupBox1.Controls.Add(this.configL1);
            this.groupBox1.Controls.Add(this.configL2);
            this.groupBox1.Controls.Add(this.configL3);
            this.groupBox1.Controls.Add(this.configL4);
            this.groupBox1.Location = new System.Drawing.Point(25, 38);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(215, 202);
            this.groupBox1.TabIndex = 51;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Ackermann";
            // 
            // textconfig3
            // 
            this.textconfig3.Enabled = false;
            this.textconfig3.Location = new System.Drawing.Point(95, 117);
            this.textconfig3.Name = "textconfig3";
            this.textconfig3.Size = new System.Drawing.Size(100, 20);
            this.textconfig3.TabIndex = 2;
            this.textconfig3.Tag = "3";
            // 
            // textconfig1
            // 
            this.textconfig1.Enabled = false;
            this.textconfig1.Location = new System.Drawing.Point(95, 36);
            this.textconfig1.Name = "textconfig1";
            this.textconfig1.Size = new System.Drawing.Size(100, 20);
            this.textconfig1.TabIndex = 0;
            this.textconfig1.Tag = "1";
            // 
            // textconfig2
            // 
            this.textconfig2.Enabled = false;
            this.textconfig2.Location = new System.Drawing.Point(95, 76);
            this.textconfig2.Name = "textconfig2";
            this.textconfig2.Size = new System.Drawing.Size(100, 20);
            this.textconfig2.TabIndex = 1;
            this.textconfig2.Tag = "2";
            // 
            // textconfig4
            // 
            this.textconfig4.Enabled = false;
            this.textconfig4.Location = new System.Drawing.Point(95, 158);
            this.textconfig4.Name = "textconfig4";
            this.textconfig4.Size = new System.Drawing.Size(100, 20);
            this.textconfig4.TabIndex = 3;
            this.textconfig4.Tag = "4";
            // 
            // configL1
            // 
            this.configL1.AutoSize = true;
            this.configL1.Location = new System.Drawing.Point(9, 39);
            this.configL1.Name = "configL1";
            this.configL1.Size = new System.Drawing.Size(20, 13);
            this.configL1.TabIndex = 4;
            this.configL1.Tag = "1";
            this.configL1.Text = "K1";
            // 
            // configL2
            // 
            this.configL2.AutoSize = true;
            this.configL2.Location = new System.Drawing.Point(9, 79);
            this.configL2.Name = "configL2";
            this.configL2.Size = new System.Drawing.Size(20, 13);
            this.configL2.TabIndex = 5;
            this.configL2.Tag = "2";
            this.configL2.Text = "K2";
            // 
            // configL3
            // 
            this.configL3.AutoSize = true;
            this.configL3.Location = new System.Drawing.Point(9, 120);
            this.configL3.Name = "configL3";
            this.configL3.Size = new System.Drawing.Size(20, 13);
            this.configL3.TabIndex = 6;
            this.configL3.Tag = "3";
            this.configL3.Text = "K3";
            // 
            // configL4
            // 
            this.configL4.AutoSize = true;
            this.configL4.Location = new System.Drawing.Point(9, 161);
            this.configL4.Name = "configL4";
            this.configL4.Size = new System.Drawing.Size(20, 13);
            this.configL4.TabIndex = 7;
            this.configL4.Tag = "4";
            this.configL4.Text = "K4";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(852, 74);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(76, 13);
            this.label16.TabIndex = 50;
            this.label16.Text = "Parametros X2";
            this.label16.Visible = false;
            // 
            // configL18
            // 
            this.configL18.AutoSize = true;
            this.configL18.Location = new System.Drawing.Point(838, 232);
            this.configL18.Name = "configL18";
            this.configL18.Size = new System.Drawing.Size(20, 13);
            this.configL18.TabIndex = 49;
            this.configL18.Tag = "18";
            this.configL18.Text = "K4";
            this.configL18.Visible = false;
            // 
            // configL17
            // 
            this.configL17.AutoSize = true;
            this.configL17.Location = new System.Drawing.Point(838, 191);
            this.configL17.Name = "configL17";
            this.configL17.Size = new System.Drawing.Size(20, 13);
            this.configL17.TabIndex = 48;
            this.configL17.Tag = "17";
            this.configL17.Text = "K3";
            this.configL17.Visible = false;
            // 
            // configL16
            // 
            this.configL16.AutoSize = true;
            this.configL16.Location = new System.Drawing.Point(838, 150);
            this.configL16.Name = "configL16";
            this.configL16.Size = new System.Drawing.Size(20, 13);
            this.configL16.TabIndex = 47;
            this.configL16.Tag = "16";
            this.configL16.Text = "K2";
            this.configL16.Visible = false;
            // 
            // configL15
            // 
            this.configL15.AutoSize = true;
            this.configL15.Location = new System.Drawing.Point(838, 110);
            this.configL15.Name = "configL15";
            this.configL15.Size = new System.Drawing.Size(20, 13);
            this.configL15.TabIndex = 46;
            this.configL15.Tag = "15";
            this.configL15.Text = "K1";
            this.configL15.Visible = false;
            // 
            // textconfig18
            // 
            this.textconfig18.Enabled = false;
            this.textconfig18.Location = new System.Drawing.Point(874, 229);
            this.textconfig18.Name = "textconfig18";
            this.textconfig18.Size = new System.Drawing.Size(100, 20);
            this.textconfig18.TabIndex = 45;
            this.textconfig18.Tag = "18";
            this.textconfig18.Visible = false;
            // 
            // textconfig17
            // 
            this.textconfig17.Enabled = false;
            this.textconfig17.Location = new System.Drawing.Point(874, 188);
            this.textconfig17.Name = "textconfig17";
            this.textconfig17.Size = new System.Drawing.Size(100, 20);
            this.textconfig17.TabIndex = 44;
            this.textconfig17.Tag = "17";
            this.textconfig17.Visible = false;
            // 
            // textconfig16
            // 
            this.textconfig16.Enabled = false;
            this.textconfig16.Location = new System.Drawing.Point(874, 147);
            this.textconfig16.Name = "textconfig16";
            this.textconfig16.Size = new System.Drawing.Size(100, 20);
            this.textconfig16.TabIndex = 43;
            this.textconfig16.Tag = "16";
            this.textconfig16.Visible = false;
            // 
            // textconfig15
            // 
            this.textconfig15.Enabled = false;
            this.textconfig15.Location = new System.Drawing.Point(874, 107);
            this.textconfig15.Name = "textconfig15";
            this.textconfig15.Size = new System.Drawing.Size(100, 20);
            this.textconfig15.TabIndex = 42;
            this.textconfig15.Tag = "15";
            this.textconfig15.Visible = false;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(584, 65);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(76, 13);
            this.label11.TabIndex = 41;
            this.label11.Text = "Parametros X1";
            this.label11.Visible = false;
            // 
            // configL14
            // 
            this.configL14.AutoSize = true;
            this.configL14.Location = new System.Drawing.Point(570, 223);
            this.configL14.Name = "configL14";
            this.configL14.Size = new System.Drawing.Size(20, 13);
            this.configL14.TabIndex = 40;
            this.configL14.Tag = "14";
            this.configL14.Text = "K4";
            this.configL14.Visible = false;
            // 
            // configL13
            // 
            this.configL13.AutoSize = true;
            this.configL13.Location = new System.Drawing.Point(570, 182);
            this.configL13.Name = "configL13";
            this.configL13.Size = new System.Drawing.Size(20, 13);
            this.configL13.TabIndex = 39;
            this.configL13.Tag = "13";
            this.configL13.Text = "K3";
            this.configL13.Visible = false;
            // 
            // configL12
            // 
            this.configL12.AutoSize = true;
            this.configL12.Location = new System.Drawing.Point(570, 141);
            this.configL12.Name = "configL12";
            this.configL12.Size = new System.Drawing.Size(20, 13);
            this.configL12.TabIndex = 38;
            this.configL12.Tag = "12";
            this.configL12.Text = "K2";
            this.configL12.Visible = false;
            // 
            // configL11
            // 
            this.configL11.AutoSize = true;
            this.configL11.Location = new System.Drawing.Point(570, 101);
            this.configL11.Name = "configL11";
            this.configL11.Size = new System.Drawing.Size(20, 13);
            this.configL11.TabIndex = 37;
            this.configL11.Tag = "11";
            this.configL11.Text = "K1";
            this.configL11.Visible = false;
            // 
            // textconfig14
            // 
            this.textconfig14.Enabled = false;
            this.textconfig14.Location = new System.Drawing.Point(606, 220);
            this.textconfig14.Name = "textconfig14";
            this.textconfig14.Size = new System.Drawing.Size(100, 20);
            this.textconfig14.TabIndex = 36;
            this.textconfig14.Tag = "14";
            this.textconfig14.Visible = false;
            // 
            // textconfig13
            // 
            this.textconfig13.Enabled = false;
            this.textconfig13.Location = new System.Drawing.Point(606, 179);
            this.textconfig13.Name = "textconfig13";
            this.textconfig13.Size = new System.Drawing.Size(100, 20);
            this.textconfig13.TabIndex = 35;
            this.textconfig13.Tag = "13";
            this.textconfig13.Visible = false;
            // 
            // textconfig12
            // 
            this.textconfig12.Enabled = false;
            this.textconfig12.Location = new System.Drawing.Point(606, 138);
            this.textconfig12.Name = "textconfig12";
            this.textconfig12.Size = new System.Drawing.Size(100, 20);
            this.textconfig12.TabIndex = 34;
            this.textconfig12.Tag = "12";
            this.textconfig12.Visible = false;
            // 
            // textconfig11
            // 
            this.textconfig11.Enabled = false;
            this.textconfig11.Location = new System.Drawing.Point(606, 98);
            this.textconfig11.Name = "textconfig11";
            this.textconfig11.Size = new System.Drawing.Size(100, 20);
            this.textconfig11.TabIndex = 33;
            this.textconfig11.Tag = "11";
            this.textconfig11.Visible = false;
            // 
            // labelFirmareVer
            // 
            this.labelFirmareVer.AutoSize = true;
            this.labelFirmareVer.Location = new System.Drawing.Point(84, 409);
            this.labelFirmareVer.Name = "labelFirmareVer";
            this.labelFirmareVer.Size = new System.Drawing.Size(22, 13);
            this.labelFirmareVer.TabIndex = 23;
            this.labelFirmareVer.Text = "0.0";
            // 
            // labelFirmware
            // 
            this.labelFirmware.AutoSize = true;
            this.labelFirmware.Location = new System.Drawing.Point(23, 409);
            this.labelFirmware.Name = "labelFirmware";
            this.labelFirmware.Size = new System.Drawing.Size(55, 13);
            this.labelFirmware.TabIndex = 22;
            this.labelFirmware.Text = "Firmware: ";
            // 
            // groupBoxTipoRegula
            // 
            this.groupBoxTipoRegula.Controls.Add(this.radioButtonLQR);
            this.groupBoxTipoRegula.Controls.Add(this.radioButtonPID);
            this.groupBoxTipoRegula.Controls.Add(this.label6);
            this.groupBoxTipoRegula.Controls.Add(this.textconfig7);
            this.groupBoxTipoRegula.Controls.Add(this.textconfig8);
            this.groupBoxTipoRegula.Controls.Add(this.textconfig9);
            this.groupBoxTipoRegula.Controls.Add(this.textconfig10);
            this.groupBoxTipoRegula.Controls.Add(this.configL7);
            this.groupBoxTipoRegula.Controls.Add(this.configL8);
            this.groupBoxTipoRegula.Controls.Add(this.configL9);
            this.groupBoxTipoRegula.Controls.Add(this.configL10);
            this.groupBoxTipoRegula.Location = new System.Drawing.Point(255, 38);
            this.groupBoxTipoRegula.Name = "groupBoxTipoRegula";
            this.groupBoxTipoRegula.Size = new System.Drawing.Size(231, 273);
            this.groupBoxTipoRegula.TabIndex = 20;
            this.groupBoxTipoRegula.TabStop = false;
            this.groupBoxTipoRegula.Text = "Tipo de regulación";
            // 
            // radioButtonLQR
            // 
            this.radioButtonLQR.AutoSize = true;
            this.radioButtonLQR.Checked = true;
            this.radioButtonLQR.Location = new System.Drawing.Point(17, 23);
            this.radioButtonLQR.Name = "radioButtonLQR";
            this.radioButtonLQR.Size = new System.Drawing.Size(47, 17);
            this.radioButtonLQR.TabIndex = 18;
            this.radioButtonLQR.TabStop = true;
            this.radioButtonLQR.Text = "LQR";
            this.radioButtonLQR.UseVisualStyleBackColor = true;
            // 
            // radioButtonPID
            // 
            this.radioButtonPID.AutoSize = true;
            this.radioButtonPID.Location = new System.Drawing.Point(116, 23);
            this.radioButtonPID.Name = "radioButtonPID";
            this.radioButtonPID.Size = new System.Drawing.Size(43, 17);
            this.radioButtonPID.TabIndex = 19;
            this.radioButtonPID.Text = "PID";
            this.radioButtonPID.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(58, 51);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(85, 13);
            this.label6.TabIndex = 32;
            this.label6.Text = "Parametros LQR";
            // 
            // textconfig7
            // 
            this.textconfig7.Enabled = false;
            this.textconfig7.Location = new System.Drawing.Point(80, 84);
            this.textconfig7.Name = "textconfig7";
            this.textconfig7.Size = new System.Drawing.Size(100, 20);
            this.textconfig7.TabIndex = 24;
            this.textconfig7.Tag = "7";
            // 
            // textconfig8
            // 
            this.textconfig8.Enabled = false;
            this.textconfig8.Location = new System.Drawing.Point(80, 124);
            this.textconfig8.Name = "textconfig8";
            this.textconfig8.Size = new System.Drawing.Size(100, 20);
            this.textconfig8.TabIndex = 25;
            this.textconfig8.Tag = "8";
            // 
            // textconfig9
            // 
            this.textconfig9.Enabled = false;
            this.textconfig9.Location = new System.Drawing.Point(80, 165);
            this.textconfig9.Name = "textconfig9";
            this.textconfig9.Size = new System.Drawing.Size(100, 20);
            this.textconfig9.TabIndex = 26;
            this.textconfig9.Tag = "9";
            // 
            // textconfig10
            // 
            this.textconfig10.Enabled = false;
            this.textconfig10.Location = new System.Drawing.Point(80, 206);
            this.textconfig10.Name = "textconfig10";
            this.textconfig10.Size = new System.Drawing.Size(100, 20);
            this.textconfig10.TabIndex = 27;
            this.textconfig10.Tag = "10";
            // 
            // configL7
            // 
            this.configL7.AutoSize = true;
            this.configL7.Location = new System.Drawing.Point(44, 87);
            this.configL7.Name = "configL7";
            this.configL7.Size = new System.Drawing.Size(20, 13);
            this.configL7.TabIndex = 28;
            this.configL7.Tag = "7";
            this.configL7.Text = "K1";
            // 
            // configL8
            // 
            this.configL8.AutoSize = true;
            this.configL8.Location = new System.Drawing.Point(44, 127);
            this.configL8.Name = "configL8";
            this.configL8.Size = new System.Drawing.Size(20, 13);
            this.configL8.TabIndex = 29;
            this.configL8.Tag = "8";
            this.configL8.Text = "K2";
            // 
            // configL9
            // 
            this.configL9.AutoSize = true;
            this.configL9.Location = new System.Drawing.Point(44, 168);
            this.configL9.Name = "configL9";
            this.configL9.Size = new System.Drawing.Size(20, 13);
            this.configL9.TabIndex = 30;
            this.configL9.Tag = "9";
            this.configL9.Text = "K3";
            // 
            // configL10
            // 
            this.configL10.AutoSize = true;
            this.configL10.Location = new System.Drawing.Point(44, 209);
            this.configL10.Name = "configL10";
            this.configL10.Size = new System.Drawing.Size(20, 13);
            this.configL10.TabIndex = 31;
            this.configL10.Tag = "10";
            this.configL10.Text = "K4";
            // 
            // buttonGuardarCalibracion
            // 
            this.buttonGuardarCalibracion.Enabled = false;
            this.buttonGuardarCalibracion.Location = new System.Drawing.Point(25, 379);
            this.buttonGuardarCalibracion.Name = "buttonGuardarCalibracion";
            this.buttonGuardarCalibracion.Size = new System.Drawing.Size(81, 27);
            this.buttonGuardarCalibracion.TabIndex = 9;
            this.buttonGuardarCalibracion.Text = "Guardar";
            this.buttonGuardarCalibracion.UseVisualStyleBackColor = true;
            // 
            // comboPuertoSerie
            // 
            this.comboPuertoSerie.FormattingEnabled = true;
            this.comboPuertoSerie.Location = new System.Drawing.Point(8, 15);
            this.comboPuertoSerie.Name = "comboPuertoSerie";
            this.comboPuertoSerie.Size = new System.Drawing.Size(101, 21);
            this.comboPuertoSerie.TabIndex = 19;
            // 
            // buttonConectar
            // 
            this.buttonConectar.Location = new System.Drawing.Point(115, 11);
            this.buttonConectar.Name = "buttonConectar";
            this.buttonConectar.Size = new System.Drawing.Size(81, 27);
            this.buttonConectar.TabIndex = 18;
            this.buttonConectar.Text = "Conectar";
            this.buttonConectar.UseVisualStyleBackColor = true;
            this.buttonConectar.Click += new System.EventHandler(this.ButtonConectar_Click);
            // 
            // serialPortBluetooth
            // 
            this.serialPortBluetooth.BaudRate = 115200;
            this.serialPortBluetooth.PortName = "COM8";
            this.serialPortBluetooth.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.serialPortBluetooth_DataReceived);
            // 
            // timerActualizar
            // 
            this.timerActualizar.Tick += new System.EventHandler(this.timerActualizar_Tick);
            // 
            // FormPrincipal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1230, 584);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.comboPuertoSerie);
            this.Controls.Add(this.buttonConectar);
            this.Name = "FormPrincipal";
            this.Text = "Configurador de Despacito";
            this.Load += new System.EventHandler(this.FormPrincipal_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.chartDatos)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBoxTipoRegula.ResumeLayout(false);
            this.groupBoxTipoRegula.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Button buttonResetZoomGraf;
        private System.Windows.Forms.Button buttonLimpiarGraf;
        private System.Windows.Forms.Button buttonContinarGrafica;
        private System.Windows.Forms.Button buttonDetenerGrafica;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Panel panelGrafico;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.RichTextBox textLogEventos;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.Label labelFirmareVer;
        private System.Windows.Forms.Label labelFirmware;
        private System.Windows.Forms.GroupBox groupBoxTipoRegula;
        private System.Windows.Forms.RadioButton radioButtonLQR;
        private System.Windows.Forms.RadioButton radioButtonPID;
        private System.Windows.Forms.Button buttonGuardarCalibracion;
        private System.Windows.Forms.Label configL4;
        private System.Windows.Forms.Label configL3;
        private System.Windows.Forms.Label configL2;
        private System.Windows.Forms.Label configL1;
        private System.Windows.Forms.TextBox textconfig4;
        private System.Windows.Forms.TextBox textconfig3;
        private System.Windows.Forms.TextBox textconfig2;
        private System.Windows.Forms.TextBox textconfig1;
        private System.Windows.Forms.ComboBox comboPuertoSerie;
        private System.Windows.Forms.Button buttonConectar;
        private System.IO.Ports.SerialPort serialPortBluetooth;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartDatos;
        private System.Windows.Forms.Timer timerActualizar;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label configL18;
        private System.Windows.Forms.Label configL17;
        private System.Windows.Forms.Label configL16;
        private System.Windows.Forms.Label configL15;
        private System.Windows.Forms.TextBox textconfig18;
        private System.Windows.Forms.TextBox textconfig17;
        private System.Windows.Forms.TextBox textconfig16;
        private System.Windows.Forms.TextBox textconfig15;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label configL14;
        private System.Windows.Forms.Label configL13;
        private System.Windows.Forms.Label configL12;
        private System.Windows.Forms.Label configL11;
        private System.Windows.Forms.TextBox textconfig14;
        private System.Windows.Forms.TextBox textconfig13;
        private System.Windows.Forms.TextBox textconfig12;
        private System.Windows.Forms.TextBox textconfig11;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label configL10;
        private System.Windows.Forms.Label configL9;
        private System.Windows.Forms.Label configL8;
        private System.Windows.Forms.Label configL7;
        private System.Windows.Forms.TextBox textconfig10;
        private System.Windows.Forms.TextBox textconfig9;
        private System.Windows.Forms.TextBox textconfig8;
        private System.Windows.Forms.TextBox textconfig7;
        private System.Windows.Forms.TextBox textconfig6;
        private System.Windows.Forms.Label configL6;
        private System.Windows.Forms.TextBox textconfig5;
        private System.Windows.Forms.Label configL5;
        private System.Windows.Forms.GroupBox groupBox1;
    }
}

