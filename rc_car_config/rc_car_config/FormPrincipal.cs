using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.IO.Ports;
using System.Windows.Forms.DataVisualization.Charting;



namespace rc_car_config
{
    public partial class FormPrincipal : Form
    {

        //SerialPort serialPortBluetooth;
        bool dibujarDatos = true;
        const int DIBUJO_2D_OFFSET = 50;
        Point ultimoMouse;
        Telemetria telemetria;
        ControlConJoystick joystick;
        DateTime tInicio, tUltimaRecepcionTout;


        public List<TextBox> textCalibracion;
        

       
        delegate void cambiarModoManualDelegado(Boolean estado);
        delegate void anadirPuntoDelegado(double x, double y, byte dato);

        delegate void logearMSGDelegado(object sender, Telemetria.LogearMSGEventArgs e);
        delegate void rellenarCalibracionDelegado(object sender, Telemetria.DatosCalibracionEventArgs e);
        delegate void rellenarValoresManualDelegado(object sender, Telemetria.DatosManualEventArgs e);
        delegate void nuevosValoresCocheDelegado(object sender, Telemetria.DatosCocheEventArgs e);
        delegate void nuevosValoresImuDelegado(object sender, Telemetria.DatosIMUEventArgs e);
        delegate void ValoresJoystickDelegado(object sender, ControlConJoystick.CambioEstadoJoystickArgs e);


        

        public FormPrincipal()
        {
            InitializeComponent();
          
            textCalibracion = new List<TextBox>();
            telemetria = new Telemetria();
            telemetria.LogearMSGevent += new EventHandler<Telemetria.LogearMSGEventArgs>(LogearMSG);
            telemetria.RecividosDatosCalibracionEvent += new EventHandler<Telemetria.DatosCalibracionEventArgs>(RellenarCalibracion);
            telemetria.RecividosDatosCocheEvent += new EventHandler<Telemetria.DatosCocheEventArgs>(RellenarValoresCoche);
            telemetria.RecividosDatosIMUEvent += new EventHandler<Telemetria.DatosIMUEventArgs>(RellenarValoresIMU);
            telemetria.RecividosDatosManual += new EventHandler<Telemetria.DatosManualEventArgs>(RellenarValoresManual);

            joystick = new ControlConJoystick();
            joystick.CambioEstadoJoystickevent += new EventHandler<ControlConJoystick.CambioEstadoJoystickArgs>(ValoresJoystick);
           //

        }

        private void FormPrincipal_Load(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            foreach (string puertoSerieX in ports)
            {
                comboPuertoSerie.Items.Add(puertoSerieX);
            }
            if (ports.Length > 0) comboPuertoSerie.SelectedIndex = 0;
            //comboEnsayo.SelectedIndex = 0;

            textCalibracion.Add(textconfig1);
            textCalibracion.Add(textconfig2);
            textCalibracion.Add(textconfig3);
            textCalibracion.Add(textconfig4);
            textCalibracion.Add(textconfig5);
            textCalibracion.Add(textconfig6);
            textCalibracion.Add(textconfig7);
            textCalibracion.Add(textconfig8);
            textCalibracion.Add(textconfig9);
            textCalibracion.Add(textconfig10);
            textCalibracion.Add(textconfig11);
            textCalibracion.Add(textconfig12);
            textCalibracion.Add(textconfig13);
            textCalibracion.Add(textconfig14);
            textCalibracion.Add(textconfig15);
            textCalibracion.Add(textconfig16);
            textCalibracion.Add(textconfig17);
            textCalibracion.Add(textconfig18);
          

            Iniciar_grafica();
            chartDatos.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            chartDatos.ChartAreas[0].AxisY.ScaleView.Zoomable = true;

            // Set automatic scrolling 
            chartDatos.ChartAreas[0].CursorX.AutoScroll = true;
            chartDatos.ChartAreas[0].CursorY.AutoScroll = true;

            // Allow user selection for Zoom
            chartDatos.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            chartDatos.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;
            ultimoMouse = new Point(0, 0);

            //TODO: evitar flicker. Investigar doble buffer para ello?
            /*
            typeof(Panel).InvokeMember("DoubleBuffered",
           BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
           null, panelGrafico, new object[] { true });
          

            panelGrafico.GetType()
                .GetProperty("DoubleBuffered",
                             BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(panelGrafico, true, null);
            */

            joystick.StartCaptureFirstJostick();
        }

      


        private void Iniciar_grafica()
        {
            chartDatos.Series.Clear();
            chartDatos.Series.Add("Roll");
            chartDatos.Series["Roll"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chartDatos.Series["Roll"].YAxisType = AxisType.Primary;
            chartDatos.Series.Add("Pitch");
            chartDatos.Series["Pitch"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            chartDatos.Series["Pitch"].YAxisType = AxisType.Primary;
        }



        private void AnadirPunto(double x, double y, byte dato)
        {
            try
            {
                if (chartDatos.InvokeRequired)
                {
                    anadirPuntoDelegado delegado = new anadirPuntoDelegado(AnadirPunto);
                    //ya que el delegado invocará a CambiarProgreso debemos indicarle los parámetros 
                    object[] parametros = new object[] { x, y, dato };
                    //invocamos el método a través del mismo contexto del formulario (this) y enviamos los parámetros 
                    this.BeginInvoke(delegado, parametros);
                }
                else
                {
                    if (dato == 1)
                    {
                        chartDatos.Series["Roll"].Points.AddXY(x, y);
                    }
                    else
                    {
                        chartDatos.Series["Pitch"].Points.AddXY(x, y);
                    }
                }
            }
            catch { }
        }

       
       


        private void ButtonConectar_Click(object sender, EventArgs e)
        {
            tInicio = DateTime.Now;
            String PuertoSerie = "COM1";
            if (comboPuertoSerie.SelectedItem != null) {
                PuertoSerie = comboPuertoSerie.SelectedItem.ToString();
            }
            telemetria.PuertoSerie = PuertoSerie;
            telemetria.Abrir_puerto_serie();
            timerActualizar.Enabled = true;

            
        }


        private void LogearMSG(string msg)
        {
            Telemetria.LogearMSGEventArgs parametros = new Telemetria.LogearMSGEventArgs() { Msg = msg, Time = DateTime.Now };

            LogearMSG(this, parametros);
        }


        private void LogearMSG(object sender, Telemetria.LogearMSGEventArgs e)
        {
            try
            {
                
                if (textLogEventos.InvokeRequired)
                {
                    logearMSGDelegado delegado = new logearMSGDelegado(LogearMSG);
                    //ya que el delegado invocará a CambiarProgreso debemos indicarle los parámetros 
                    object[] parametros = new object[] {  sender,  e };
                    //invocamos el método a través del mismo contexto del formulario (this) y enviamos los parámetros 
                    this.BeginInvoke(delegado, parametros);
                }
                else
                {
                
                string textoInicial = textLogEventos.Text;
                if (textLogEventos.Lines.Length > 500)
                {
                    textLogEventos.Text = "";
                }
                textLogEventos.Text = textoInicial + "\r" + e.Msg;
                textLogEventos.SelectionStart = textLogEventos.Text.Length;
                textLogEventos.ScrollToCaret();
                 }
            }
            catch { }
        }


       
        private void Actualizar_datos_manual( float voltage, float corriente, float eRPM, float temp, float mAh)
        {
            labelVoltage.Text = "V: "+ voltage.ToString();
            labelCorriente.Text = "A: " + corriente.ToString();
            labelRPM.Text = "RPM: " + eRPM.ToString();
            labelTemperatura.Text = "temp: " + temp.ToString();
            labelMAH.Text = "MaH: " + mAh.ToString();
        }
        
        private void TimerActualizar_Tick(object sender, EventArgs e)
        {
            if (telemetria.Conectado())
            {
               
                    //TODO: debug, quitar para release
                    //textInfo1.Text = punteroEntrada.ToString() + ". Plat: " + ultimoAnguloPlataforma.ToString() + "º. Pen: " + ultimoAnguloPendulo.ToString() + "º. E: " + arrastrandoPendulo.ToString() + " X: " + calcularAnguloRaton().ToString() + " pt "+ultimoMouse.ToString() + "|||| "+ (ultimoMouse.X - (panelGrafico.Width / 2 - DIBUJO_2D_OFFSET + 5) -46).ToString() + "/"+ (-(ultimoMouse.Y - (panelGrafico.Height / 2)) -46).ToString();
                    //textInfo1.Refresh();

                    if (panelGrafico.Visible && (tabControlPrincipal.SelectedIndex == 1)) panelGrafico.Invalidate();
                    //panelGrafico.Refresh();

                    if (chartDatos.Visible && (tabControlPrincipal.SelectedIndex == 0))
                    {
                        chartDatos.Series[0].Points.ResumeUpdates(); chartDatos.Series[1].Points.ResumeUpdates();
                        chartDatos.Invalidate(); //TODO: resume updates ya parece pintar igualmente sin necesidad de invalidad
                    }
                    chartDatos.Series[0].Points.SuspendUpdates(); chartDatos.Series[1].Points.SuspendUpdates();

                    //Actualizar_datos_manual(telemetria.ESC_VoltajeEntrada, telemetria.ESC_avgInputCurrent, telemetria.ESC_rpmActual, telemetria.ESC_Dutycycle, telemetria.ESC_avgMotorCurrent);

              
            }
        }

        
        private void TabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControlPrincipal.SelectedIndex == 3)
            {
                telemetria.Peticion_calibracion();
            }
        }
        

        private void Button1_Click(object sender, EventArgs e)
        {
            telemetria.CambiarModo(Telemetria.E_modo.enum_manual);
          
        }

        private void ButtonAuto_Click(object sender, EventArgs e)
        {
            telemetria.CambiarModo(Telemetria.E_modo.enum_full_auto);
        }

        private void ButtonSemiAuto_Click(object sender, EventArgs e)
        {
            telemetria.CambiarModo(Telemetria.E_modo.enum_semi_auto);
        }

        private void ButtonSemiControler_Click(object sender, EventArgs e)
        {
            telemetria.CambiarModo(Telemetria.E_modo.enum_sistema_salida);
        }

        private void TrackBarRuedaDer_Scroll(object sender, EventArgs e)
        {
            Nuevos_valores_manuales();
        }

        private void TrackBarRuedaIzq_Scroll(object sender, EventArgs e)
        {
            Nuevos_valores_manuales();
        }

        private void TrackBarMarcha_Scroll(object sender, EventArgs e)
        {
            Nuevos_valores_manuales();
        }

        private void TrackBarMotor_Scroll(object sender, EventArgs e)
        {
            Nuevos_valores_manuales();
        }


        private void Nuevos_valores_manuales()
        {
            int[] nueva_valores = new int[4];

            nueva_valores[0] = trackBarRuedaDer.Value;
            nueva_valores[1] = trackBarRuedaIzq.Value;
            nueva_valores[2] = trackBarMarcha.Value;
            nueva_valores[3] = trackBarMotor.Value;

            telemetria.Enviar_valores_manuales(nueva_valores);

        }

        

        private void ValoresJoystick(object sender, ControlConJoystick.CambioEstadoJoystickArgs e)
        {
            try
            {
                if (trackBarRuedaDer.InvokeRequired)
                {
                    ValoresJoystickDelegado delegado = new ValoresJoystickDelegado(ValoresJoystick);
                    //ya que el delegado invocará a CambiarProgreso debemos indicarle los parámetros 
                    object[] parametros = new object[] { sender, e };
                    //invocamos el método a través del mismo contexto del formulario (this) y enviamos los parámetros 
                    this.BeginInvoke(delegado, parametros);
                }
                else
                {
                    if (this.joystick.Habilitado)
                    {

                        trackBarRuedaDer.Value =(int)Math.Round( e.DireccionLX*50.0+5000);
                        trackBarRuedaIzq.Value = (int)Math.Round(e.DireccionRX * 50.0 + 5000);
                        trackBarMarcha.Value = (int)Math.Round(e.Marcha * 50.0 + 5000);
                        trackBarMotor.Value = (int)Math.Round(e.Motor * 50.0 + 5000);
                        Nuevos_valores_manuales();
                    }

                }
            }
            catch { }
        }

        private void RellenarValoresManual(object sender, Telemetria.DatosManualEventArgs e)
        {
            try
            {
                if (trackBarRuedaDer.InvokeRequired)
                {
                    rellenarValoresManualDelegado delegado = new rellenarValoresManualDelegado(RellenarValoresManual);
                    //ya que el delegado invocará a CambiarProgreso debemos indicarle los parámetros 
                    object[] parametros = new object[] { sender, e };
                    //invocamos el método a través del mismo contexto del formulario (this) y enviamos los parámetros 
                    this.BeginInvoke(delegado, parametros);
                }
                else
                {
                    if (e.ParRecibido.Length >= 3){ 

                        trackBarRuedaDer.Value = e.ParRecibido[0];
                        trackBarRuedaIzq.Value = e.ParRecibido[1];
                        trackBarMarcha.Value = e.ParRecibido[2];
                        trackBarMotor.Value = e.ParRecibido[3];
                    }

                }
            }
            catch { }
        }


        private void RellenarValoresCoche(object sender, Telemetria.DatosCocheEventArgs e) //float ConsignaMarcha, float ConsignaRPMActual, float AnguloRuedaDerecha, float AnguloRuedaIzquierda, float ESC_VoltajeEntrada, float ESC_rpmActual, float ESC_avgMotorCurrent, float ESC_avgInputCurrent, float ESC_Dutycycle, float modo_actual)
        {
            if (trackBarRuedaDer.InvokeRequired)
            {
                nuevosValoresCocheDelegado delegado = new nuevosValoresCocheDelegado(RellenarValoresCoche);
                //ya que el delegado invocará a CambiarProgreso debemos indicarle los parámetros 
                object[] parametros = new object[] { sender, e };
                //invocamos el método a través del mismo contexto del formulario (this) y enviamos los parámetros 
                this.BeginInvoke(delegado, parametros);
            }
            else
            {

                labelCmarcha.Text = e.ConsignaMarcha.ToString();
                labelCmotor.Text = e.ConsignaRPMActual.ToString();
                labelCRuedaDer .Text = e.AnguloRuedaDerecha.ToString();
                labelCRuedaIzq.Text = e.AnguloRuedaIzquierda.ToString();
                labelT_control.Text = e.Modo_actual.ToString();

                labelVoltage.Text = "V: " + e.ESC_VoltajeEntrada.ToString();
                labelCorriente.Text = "A: " + e.ESC_avgMotorCurrent.ToString();
                labelRPM.Text = "RPM: " + e.ESC_rpmActual.ToString();
                labelTemperatura.Text = "temp: " + e.ESC_Dutycycle.ToString();
                labelMAH.Text = "MaH: " + e.ESC_avgInputCurrent.ToString();

            }

        }

        private void RellenarValoresIMU(object sender, Telemetria.DatosIMUEventArgs e)//(float pitch, float roll, float yaw, float[] velocidad, float[] aceleracion, float[] angulos, int tipo_control, float imu_temp)
        {
            if (trackBarRuedaDer.InvokeRequired)
            {
                nuevosValoresImuDelegado delegado = new nuevosValoresImuDelegado(RellenarValoresIMU);
                //ya que el delegado invocará a CambiarProgreso debemos indicarle los parámetros 
                object[] parametros = new object[] { sender, e };
                //invocamos el método a través del mismo contexto del formulario (this) y enviamos los parámetros 
                this.BeginInvoke(delegado, parametros);
            }
            else
            {
                labelIMUAceleracion1.Text = e.Aceleracion[0].ToString();
                labelIMUAceleracion2.Text = e.Aceleracion[1].ToString();
                labelIMUAceleracion3.Text = e.Aceleracion[2].ToString();

                labelIMUVelocidad1.Text = e.Velocidad[0].ToString();
                labelIMUVelocidad2.Text = e.Velocidad[1].ToString();
                labelIMUVelocidad3.Text = e.Velocidad[2].ToString();

                labelIMUAngulo1.Text = e.Angulos[0].ToString();
                labelIMUAngulo2.Text = e.Angulos[1].ToString();
                labelIMUAngulo3.Text = e.Angulos[2].ToString();
                labelIMUAngulo4.Text = e.Angulos[3].ToString();

                labelIMUPitch.Text = e.Pitch.ToString();
                labelIMURoll.Text = e.Roll.ToString();
                labelIMUYaw.Text = e.Yaw.ToString();

                labelIMUTemp.Text = e.Imu_temp.ToString();


            }
           
        }


        private void ButtonGuardarCalibracion_Click(object sender, EventArgs e)
        {
            int[] nueva_config = new int[19];
            int valor1;
            double valor;
            bool error = false;


            foreach (TextBox t in textCalibracion)
            {
                if(int.TryParse(t.Tag.ToString(), out valor1))
                {
                    if (double.TryParse(t.Text, out valor))
                    {
                        nueva_config[valor1] = (int)Math.Round(valor * 1000);
                    } else
                    {
                        error = true;
                    }
                }

            }
            

            if (!error)
            {
                telemetria.Enviar_calibracion_Bluetooth(nueva_config);
            }
            else
            {
                LogearMSG("Error, valores de calibracion no validos");
            }

        }

        private void checkBoxXinput_CheckedChanged(object sender, EventArgs e)
        {
            joystick.Habilitado = this.checkBoxXinput.Checked;
        }

        private void checkBoxActualizarCoche_CheckedChanged(object sender, EventArgs e)
        {
            telemetria.Change_autoTelemetry(checkBoxActualizarCoche.Checked, checkBoxActualizarImu.Checked);
        }

        private void checkBoxActualizarImu_CheckedChanged(object sender, EventArgs e)
        {
            telemetria.Change_autoTelemetry(checkBoxActualizarCoche.Checked, checkBoxActualizarImu.Checked);
        }

        private void RellenarCalibracion(object sender, Telemetria.DatosCalibracionEventArgs e)//float[] parRecibido, int x, int verHight, int verLow)
        {

            try
            {
                if (textconfig1.InvokeRequired)
                {
                    rellenarCalibracionDelegado delegado = new rellenarCalibracionDelegado(RellenarCalibracion);
                    //ya que el delegado invocará a CambiarProgreso debemos indicarle los parámetros 
                    object[] parametros = new object[] { sender , e };
                    //invocamos el método a través del mismo contexto del formulario (this) y enviamos los parámetros 
                    this.BeginInvoke(delegado, parametros);
                }
                else
                {
                    int valor1;

                    foreach (TextBox t in textCalibracion)
                    {
                        if (int.TryParse(t.Tag.ToString(), out valor1))
                        {
                            if ((valor1 >= e.ParRecibido.GetLowerBound(0)) && (valor1 <= e.ParRecibido.GetUpperBound(0)))
                            {
                                t.Text = e.ParRecibido[valor1].ToString();
                                t.Enabled = true;
                            }

                        }

                    }

                    buttonGuardarCalibracion.Enabled = true;
                    labelFirmareVer.Text = e.VerHight.ToString() + "." + e.VerLow.ToString();
                }
            }
            catch { }
        }


    }
}
