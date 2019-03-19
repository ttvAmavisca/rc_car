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
using System.Threading;

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
        CocheSolar3D coche3d;


        public List<TextBox> textCalibracion;
        

       
        delegate void cambiarModoManualDelegado(Boolean estado);
        delegate void anadirPuntoDelegado(double x, double y, byte dato);

        delegate void logearMSGDelegado(object sender, Telemetria.LogearMSGEventArgs e);
        delegate void rellenarCalibracionDelegado(object sender, Telemetria.DatosCalibracionEventArgs e);
        delegate void rellenarValoresManualDelegado(object sender, Telemetria.DatosManualEventArgs e);
        delegate void nuevosValoresCocheDelegado(object sender, Telemetria.DatosCocheEventArgs e);
        delegate void nuevosValoresImuDelegado(object sender, Telemetria.DatosIMUEventArgs e);
        delegate void nuevosValoresEstadoDelegado(object sender, Telemetria.DatosEstadoEventArgs e);
        delegate void nuevosValoresESCDelegado(object sender, Telemetria.DatosESCEventArgs e);
        delegate void nuevosValoresPotenciaDelegado(object sender, Telemetria.DatosPotenciaEventArgs e);
        delegate void conexionSerieDelegado(object sender, Telemetria.CambioEstadoConexionArgs e);

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
            telemetria.RecividosDatosESCEvent += new EventHandler<Telemetria.DatosESCEventArgs>(RellenarValoresESC);
            telemetria.RecividosDatosPotenciaEvent += new EventHandler<Telemetria.DatosPotenciaEventArgs>(RellenarValoresPotencia);
            telemetria.RecividosEstado += new EventHandler<Telemetria.DatosEstadoEventArgs>(RellenarValoresEstado);
            telemetria.CambioEstadoConexion += new EventHandler<Telemetria.CambioEstadoConexionArgs>(CambioEstadoConexion);

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

      
        private void Iniciar_grafica_con(ref Chart grafica, string [] ejes)
        {
            grafica.Series.Clear();
            foreach( string cadena in ejes)
            {
                grafica.Series.Add(cadena);
                grafica.Series[cadena].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
                grafica.Series[cadena].YAxisType = AxisType.Primary;
            }


            grafica.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
            grafica.ChartAreas[0].AxisY.ScaleView.Zoomable = true;

            // Scroll automatico
            grafica.ChartAreas[0].CursorX.AutoScroll = true;
            grafica.ChartAreas[0].CursorY.AutoScroll = true;

            // Permitir zoom manual
            grafica.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
            grafica.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;
        }

        private void Iniciar_grafica()
        {
            Iniciar_grafica_con(ref chartAccel, new string[] { "AccelX","AccelY","AccelZ" });
            Iniciar_grafica_con(ref chartGyro, new string[] { "GiroX","GiroY","GiroZ" });
            Iniciar_grafica_con(ref chartCoche, new string[] { "RuedaDer","RuedaIz","Motor", "Marcha" });
            Iniciar_grafica_con(ref chartESC, new string[] { "Voltage","Corriente","rmp","voltage_ina","corriente_ina","potencia_ina" });

        }



        private void AnadirPunto(double x, double y, byte dato)
        {
            if (!dibujarDatos) return;

            try
            {
                if (chartAccel.InvokeRequired)
                {
                    anadirPuntoDelegado delegado = new anadirPuntoDelegado(AnadirPunto);
                    //ya que el delegado invocará a CambiarProgreso debemos indicarle los parámetros 
                    object[] parametros = new object[] { x, y, dato };
                    //invocamos el método a través del mismo contexto del formulario (this) y enviamos los parámetros 
                    this.BeginInvoke(delegado, parametros);
                }
                else
                {
                    if (x < 0)x=(DateTime.Now - tInicio).TotalSeconds;

                    if (dato == 1)
                    {
                        chartAccel.Series["AccelX"].Points.AddXY(x, y);
                    }
                    else if (dato == 2)
                    {
                        chartAccel.Series["AccelY"].Points.AddXY(x, y);
                    }
                    else if (dato == 3)
                    {
                        chartAccel.Series["AccelZ"].Points.AddXY(x, y);
                    }
                    else if (dato == 4)
                    {
                        chartGyro.Series["GiroX"].Points.AddXY(x, y);
                    }
                    else if (dato == 5)
                    {
                        chartGyro.Series["GiroY"].Points.AddXY(x, y);
                    }
                    else if (dato == 6)
                    {
                        chartGyro.Series["GiroZ"].Points.AddXY(x, y);
                    }
                    else if (dato == 7)
                    {
                        chartCoche.Series[0].Points.AddXY(x, y);
                    }
                    else if (dato == 8)
                    {
                        chartCoche.Series[1].Points.AddXY(x, y);
                    }
                    else if (dato == 9)
                    {
                        chartCoche.Series[2].Points.AddXY(x, y);
                    }
                    else if (dato == 10)
                    {
                        chartCoche.Series[3].Points.AddXY(x, y);
                    }
                    else if (dato == 11)
                    {
                        chartESC.Series[0].Points.AddXY(x, y);
                    }
                    else if (dato == 12)
                    {
                        chartESC.Series[1].Points.AddXY(x, y);
                    }
                    else if (dato == 13)
                    {
                        chartESC.Series[2].Points.AddXY(x, y);
                    }
                    else if (dato == 14)
                    {
                        chartESC.Series[3].Points.AddXY(x, y);
                    }
                    else if (dato == 15)
                    {
                        chartESC.Series[4].Points.AddXY(x, y);
                    }
                    else if (dato == 16)
                    {
                        chartESC.Series[5].Points.AddXY(x, y);
                    }
                    else if (dato == 17)
                    {
                        chartESC.Series[6].Points.AddXY(x, y);
                    }
                }
            }
            catch { }
        }

       
       


        private void ButtonConectar_Click(object sender, EventArgs e)
        {
            if (!telemetria.Conectado()) { 

                tInicio = DateTime.Now;
                String PuertoSerie = "COM1";
                if (comboPuertoSerie.SelectedItem != null) {
                    PuertoSerie = comboPuertoSerie.SelectedItem.ToString();
                }
                telemetria.PuertoSerie = PuertoSerie;
                telemetria.Abrir_puerto_serie();
                telemetria.Pedir_estado();
                telemetria.Pedir_Valores(0xFF);
                timerActualizar.Enabled = true;
               
            } else
            {
                telemetria.Cerrar_puerto_serie();
            }

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
            /*
            if (telemetria.Conectado())
            {
               
                    //TODO: debug, quitar para release
                    //textInfo1.Text = punteroEntrada.ToString() + ". Plat: " + ultimoAnguloPlataforma.ToString() + "º. Pen: " + ultimoAnguloPendulo.ToString() + "º. E: " + arrastrandoPendulo.ToString() + " X: " + calcularAnguloRaton().ToString() + " pt "+ultimoMouse.ToString() + "|||| "+ (ultimoMouse.X - (panelGrafico.Width / 2 - DIBUJO_2D_OFFSET + 5) -46).ToString() + "/"+ (-(ultimoMouse.Y - (panelGrafico.Height / 2)) -46).ToString();
                    //textInfo1.Refresh();

                    if (panelGrafico.Visible && (tabControlPrincipal.SelectedIndex == 1)) panelGrafico.Invalidate();
                    //panelGrafico.Refresh();

                    if (chartAccel.Visible && (tabControlPrincipal.SelectedIndex == 0))
                    {
                        chartAccel.Series[0].Points.ResumeUpdates(); chartAccel.Series[1].Points.ResumeUpdates();
                        chartAccel.Invalidate(); //TODO: resume updates ya parece pintar igualmente sin necesidad de invalidad
                    }
                    chartAccel.Series[0].Points.SuspendUpdates(); chartAccel.Series[1].Points.SuspendUpdates();

                    //Actualizar_datos_manual(telemetria.ESC_VoltajeEntrada, telemetria.ESC_avgInputCurrent, telemetria.ESC_rpmActual, telemetria.ESC_Dutycycle, telemetria.ESC_avgMotorCurrent);

              
            }
            */
        }

        
        private void TabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControlPrincipal.SelectedIndex == tabControlPrincipal.TabPages.IndexOf(tab3Calibracion))
            {
                telemetria.Peticion_calibracion();
            }
            if (tabControlPrincipal.SelectedIndex == tabControlPrincipal.TabPages.IndexOf(tab4Manual))
            {
                telemetria.Peticion_Valoresmanuales();
            }
        }
        

        private void Button1_Click(object sender, EventArgs e)
        {
            telemetria.CambiarModo(Telemetria.E_modo.e_modo_manual);
          
        }

        private void ButtonAuto_Click(object sender, EventArgs e)
        {
            telemetria.CambiarModo(Telemetria.E_modo.e_modo_full_auto);
        }

        private void ButtonSemiAuto_Click(object sender, EventArgs e)
        {
            telemetria.CambiarModo(Telemetria.E_modo.e_modo_semi_auto);
        }

        private void ButtonSemiControler_Click(object sender, EventArgs e)
        {
            telemetria.CambiarModo(Telemetria.E_modo.e_modo_sistema_salida);
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

                        trackBarRuedaDer.Value = (int)Math.Round(e.ParRecibido[0] * 50.0 + 5000);
                        trackBarRuedaIzq.Value = (int)Math.Round(e.ParRecibido[1] * 50.0 + 5000);
                        trackBarMarcha.Value = (int)Math.Round(e.ParRecibido[2] * 50.0 + 5000);
                        trackBarMotor.Value = (int)Math.Round(e.ParRecibido[3] * 50.0 + 5000);
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
                labelCmotor.Text = e.ConsignaMotor.ToString();
                labelCRuedaDer .Text = e.ConsignaRuedaDerecha.ToString();
                labelCRuedaIzq.Text = e.ConsignaRuedaIzquierda.ToString();
                labelT_control.Text = e.Modo_motor_actual.ToString();

                labelT_control.Text = e.Tipo_control_actual.ToString();
                labelT_control.Text = e.Tipo_regulacion_direccion.ToString();
                labelT_control.Text = e.Tipo_regulacion_potencia.ToString();

                labelVoltage.Text = "V: " + e.ESC_VoltajeEntrada.ToString();
                labelCorriente.Text = "A: " + e.ESC_avgMotorCurrent.ToString();
                labelRPM.Text = "RPM: " + e.ESC_rpmActual.ToString();
                labelTemperatura.Text = "temp: " + e.ESC_Dutycycle.ToString();
                labelMAH.Text = "MaH: " + e.ESC_avgInputCurrent.ToString();


                //TODO: no enviar esta informacion?
                Telemetria.DatosEstadoEventArgs par = new Telemetria.DatosEstadoEventArgs()
                {
                    Modo_motor_actual = e.Modo_motor_actual,
                    Tipo_control_actual = e.Tipo_control_actual,
                    Tipo_regulacion_direccion = e.Tipo_regulacion_direccion,
                    Tipo_regulacion_potencia = e.Tipo_regulacion_potencia
                };
                RellenarValoresEstado(sender, par);

                //graficas
                AnadirPunto(-1, e.ConsignaRuedaDerecha, 7);
                AnadirPunto(-1, e.ConsignaRuedaIzquierda, 8);
                AnadirPunto(-1, e.ConsignaMotor, 9);
                AnadirPunto(-1, e.ConsignaMarcha, 10);

                AnadirPunto(-1, e.ESC_VoltajeEntrada, 11);
                AnadirPunto(-1, e.ESC_avgInputCurrent, 12);
                AnadirPunto(-1, e.ESC_rpmActual, 13);
                AnadirPunto(-1, e.ESC_Dutycycle, 14);


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
                labelIMUAceleracion1.Text ="Acel.x: " + e.Aceleracion[0].ToString();
                labelIMUAceleracion2.Text = "Acel.y: " + e.Aceleracion[1].ToString();
                labelIMUAceleracion3.Text = "Acel.z: " + e.Aceleracion[2].ToString();

                labelIMUVelocidad1.Text = "Gyro.x: " + e.Gyro[0].ToString();
                labelIMUVelocidad2.Text = "Gyro.y: " + e.Gyro[1].ToString();
                labelIMUVelocidad3.Text = "Gyro.z: " + e.Gyro[2].ToString();

                labelIMUAngulo1.Text = "Quad.1: " + e.Angulos[0].ToString();
                labelIMUAngulo2.Text = "Quad.2: " + e.Angulos[1].ToString();
                labelIMUAngulo3.Text = "Quad.3: " + e.Angulos[2].ToString();
                labelIMUAngulo4.Text = "Quad.4: " + e.Angulos[3].ToString();

                labelIMUPitch.Text = "Pitch: " + e.Pitch.ToString();
                labelIMURoll.Text = "Roll: " + e.Roll.ToString();
                labelIMUYaw.Text = "Yaw: " + e.Yaw.ToString();

                labelIMUTemp.Text = "Imu temp: " +  e.Imu_temp.ToString();

                labelBARPresion.Text = "Bar press: " + e.bar_presion.ToString();
                labelBARTemp.Text = "Bar temp: " + e.bar_temp.ToString();

                if (coche3d != null) { 
                    coche3d.pitch =  e.Pitch * (float) Math.PI /180.0f;
                    coche3d.yaw = e.Yaw * (float)Math.PI / 180.0f;
                    coche3d.roll =  e.Roll * (float)Math.PI / 180.0f;
                }

                AnadirPunto(-1, e.Aceleracion[0], 1);
                AnadirPunto(-1, e.Aceleracion[1], 2);
                AnadirPunto(-1, e.Aceleracion[2], 3);

                AnadirPunto(-1, e.Gyro[0], 4);
                AnadirPunto(-1, e.Gyro[1], 5);
                AnadirPunto(-1, e.Gyro[2], 6);

            }

        }



        private void RellenarValoresESC(object sender, Telemetria.DatosESCEventArgs e) //float ConsignaMarcha, float ConsignaRPMActual, float AnguloRuedaDerecha, float AnguloRuedaIzquierda, float ESC_VoltajeEntrada, float ESC_rpmActual, float ESC_avgMotorCurrent, float ESC_avgInputCurrent, float ESC_Dutycycle, float modo_actual)
        {
            if (trackBarRuedaDer.InvokeRequired)
            {
                nuevosValoresESCDelegado delegado = new nuevosValoresESCDelegado(RellenarValoresESC);
                //ya que el delegado invocará a CambiarProgreso debemos indicarle los parámetros 
                object[] parametros = new object[] { sender, e };
                //invocamos el método a través del mismo contexto del formulario (this) y enviamos los parámetros 
                this.BeginInvoke(delegado, parametros);
            }
            else
            {

                labelVoltage.Text = "V: " + e.ESC_VoltajeEntrada.ToString();
                labelCorriente.Text = "A: " + e.ESC_avgMotorCurrent.ToString();
                labelRPM.Text = "RPM: " + e.ESC_rpmActual.ToString();
                labelTemperatura.Text = "temp: " + e.ESC_Dutycycle.ToString();
                labelMAH.Text = "MaH: " + e.ESC_avgInputCurrent.ToString();

                AnadirPunto(-1, e.ESC_VoltajeEntrada, 11);
                AnadirPunto(-1, e.ESC_avgInputCurrent, 12);
                AnadirPunto(-1, e.ESC_rpmActual, 13);
                AnadirPunto(-1, e.ESC_Dutycycle, 14);

            }

        }
        private void RellenarValoresPotencia(object sender, Telemetria.DatosPotenciaEventArgs e)//(float pitch, float roll, float yaw, float[] velocidad, float[] aceleracion, float[] angulos, int tipo_control, float imu_temp)
        {
            if (trackBarRuedaDer.InvokeRequired)
            {
                nuevosValoresPotenciaDelegado delegado = new nuevosValoresPotenciaDelegado(RellenarValoresPotencia);
                //ya que el delegado invocará a CambiarProgreso debemos indicarle los parámetros 
                object[] parametros = new object[] { sender, e };
                //invocamos el método a través del mismo contexto del formulario (this) y enviamos los parámetros 
                this.BeginInvoke(delegado, parametros);
            }
            else
            {
                labelInaVoltage.Text = "Volt(V): " + e.ina_busvoltage.ToString();
                labelInaCorriente.Text = "Cor(mA): " + e.ina_current_mA.ToString();
                labelInaVoltageLoad.Text = "Load(V): " + e.ina_loadvoltage.ToString();
                labelInaPotencia.Text = "Power(mW): " + e.ina_power_mW.ToString();
          

                AnadirPunto(-1, e.ina_busvoltage, 15);
                AnadirPunto(-1, e.ina_current_mA, 16);
                AnadirPunto(-1, e.ina_power_mW, 17);

            }

        }

        private void RellenarValoresEstado(object sender, Telemetria.DatosEstadoEventArgs e)//(float pitch, float roll, float yaw, float[] velocidad, float[] aceleracion, float[] angulos, int tipo_control, float imu_temp)
        {
            if (labelControl.InvokeRequired)
            {
                nuevosValoresEstadoDelegado delegado = new nuevosValoresEstadoDelegado(RellenarValoresEstado);
                //ya que el delegado invocará a CambiarProgreso debemos indicarle los parámetros 
                object[] parametros = new object[] { sender, e };
                //invocamos el método a través del mismo contexto del formulario (this) y enviamos los parámetros 
                this.BeginInvoke(delegado, parametros);
            }
            else
            {
                switch (e.Modo_motor_actual)
                {
                    case Telemetria.E_modo.e_modo_manual:
                        labelModo.BackColor = Color.LightSteelBlue;
                        labelModo.Text = "manual";
                        break;

                    case Telemetria.E_modo.e_modo_full_auto:
                        labelModo.BackColor = Color.Green;
                        labelModo.Text = "Auto";
                        break;
                    case Telemetria.E_modo.e_modo_semi_auto:
                        labelModo.BackColor = Color.LightYellow;
                        labelModo.Text = "Semi-auto";
                        break;
                    case Telemetria.E_modo.e_modo_sistema_salida:
                        labelModo.BackColor = Color.Magenta;
                        labelModo.Text = "Sistema salida";
                        break;
                    default:
                        labelModo.BackColor = Color.Gray;
                        labelModo.Text = "-";
                        break;
                }

                switch (e.Tipo_control_actual)
                {
                    case Telemetria.E_tipo_control.e_control_BT:
                        labelControl.BackColor = Color.LightSkyBlue;
                        labelControl.Text = "Control Bluetooth";
                        break;

                    case Telemetria.E_tipo_control.e_control_RC:
                        labelControl.BackColor = Color.LightGreen;
                        labelControl.Text = "Control RC";
                        break;
                    default:
                        labelControl.BackColor = Color.Gray;
                        labelControl.Text = "-";
                        break;
                }

                switch (e.Tipo_regulacion_potencia)
                {
                    case Telemetria.E_regulacion_potencia.e_reg_pot_agresiva:
                        labelPotencia.BackColor = Color.LightGoldenrodYellow;
                        labelPotencia.Text = "pot. agresiva";
                        break;

                    case Telemetria.E_regulacion_potencia.e_reg_pot_incremental:
                        labelPotencia.BackColor = Color.Coral;
                        labelPotencia.Text = "pot. incremental";
                        break;
                    case Telemetria.E_regulacion_potencia.e_reg_pot_max:
                        labelPotencia.BackColor = Color.Red;
                        labelPotencia.Text = "pot. max";
                        break;
                    case Telemetria.E_regulacion_potencia.e_reg_pot_conservadora:
                        labelPotencia.BackColor = Color.LightSeaGreen;
                        labelPotencia.Text = "pot. conservadora";
                        break;
                    case Telemetria.E_regulacion_potencia.e_reg_pot_directa:
                        labelPotencia.BackColor = Color.AliceBlue;
                        labelPotencia.Text = "pot. directa";
                        break;
                    default:
                        labelPotencia.BackColor = Color.Gray;
                        labelPotencia.Text = "-";
                        break;
                }

                switch (e.Tipo_regulacion_direccion)
                {
                    case Telemetria.E_regulacion_direccion.e_reg_dir_directa:
                        labelDireccion.BackColor = Color.LightSalmon;
                        labelDireccion.Text = "Direcion norm";
                        break;

                    case Telemetria.E_regulacion_direccion.e_reg_dir_Ackermann:
                        labelDireccion.BackColor = Color.Green;
                        labelDireccion.Text = "Ackermann";
                        break;
                    default:
                        labelDireccion.BackColor = Color.Gray;
                        labelDireccion.Text = "-";
                        break;
                }

            }

        }

        private void CambioEstadoConexion(object sender, Telemetria.CambioEstadoConexionArgs e)
        {
            if (buttonConectar.InvokeRequired)
            {
                conexionSerieDelegado delegado = new conexionSerieDelegado(CambioEstadoConexion);
                //ya que el delegado invocará a CambiarProgreso debemos indicarle los parámetros 
                object[] parametros = new object[] { sender, e };
                //invocamos el método a través del mismo contexto del formulario (this) y enviamos los parámetros 
                this.BeginInvoke(delegado, parametros);
            }
            else
            {
               switch (e.EstadoActual)
                {
                    case 1:
                        buttonConectar.BackColor = Color.Green;
                        timerPing.Enabled = true;
                        break;

                   case 0:
                        buttonConectar.BackColor = Color.Red;
                        timerPing.Enabled = false;
                        break;
                    default:
                        buttonConectar.BackColor = Color.Gray;
                        break;
                }

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

        private void CheckBoxXinput_CheckedChanged(object sender, EventArgs e)
        {
            joystick.Habilitado = this.checkBoxXinput.Checked;
        }

        private void CheckBoxActualizarCoche_CheckedChanged(object sender, EventArgs e)
        {
            telemetria.Change_autoTelemetry(checkBoxActualizarCoche.Checked, checkBoxActualizarImu.Checked);
        }

        private void CheckBoxActualizarImu_CheckedChanged(object sender, EventArgs e)
        {
            telemetria.Change_autoTelemetry(checkBoxActualizarCoche.Checked, checkBoxActualizarImu.Checked);
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            coche3d = new CocheSolar3D();
            var thread = new Thread(coche3d.Run);
            thread.TrySetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private void timerPing_Tick(object sender, EventArgs e)
        {
            //En modo de control remoto hacer ping para detectar perdida de conexion
            if (telemetria.modo_motor_actual == Telemetria.E_modo.e_modo_manual || telemetria.tipo_control_actual == Telemetria.E_tipo_control.e_control_BT) { 
                telemetria.ping();

                labelLatencia.Text = Math.Round( telemetria.tiempoRespuesta).ToString();
            }
        }


        private void ExportarCSV(Chart grafica)
        {

            string[] csvLine=null;
            string csvContent = "";

            foreach (Series series in grafica.Series)
            {
                string seriesName = series.Name;
                int pointCount = series.Points.Count;
                if (pointCount <= 0) return;
                string seriesType = series.GetType().ToString();
                if (csvLine == null) csvLine = new string[pointCount];
                if (String.IsNullOrEmpty(csvLine[0])) csvLine[0] = "Tiempo;";
                csvLine[0] += seriesName + ";";
                for (int p = 1; p < pointCount; p++)
                {
                    DataPoint point = series.Points[p];

                    string yValuesCSV = String.Empty;

                    int count = point.YValues.Length;

                    for (int i = 0; i < count; i++)

                    {

                        yValuesCSV += point.YValues[i];

                        if (i != count - 1)

                            yValuesCSV += ",";

                    }

                   if (String.IsNullOrEmpty( csvLine[p] )) csvLine[p] = point.XValue + ";";
                   csvLine[p] += yValuesCSV + ";";

                }

            }

            for (int p = 0; p < csvLine.Length; p++)
            {
                csvContent += csvLine[p] + "\r" + "\n";
            }
            
            //csvContent += csvLine + "\r\n";

            System.IO.Stream myStream;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "archivos csv (*.csv)|*.csv|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if ((myStream = saveFileDialog1.OpenFile()) != null)
                {
                    using (System.IO.BinaryWriter writer = new System.IO.BinaryWriter(myStream, Encoding.UTF8))
                    {
                        writer.Write(csvContent);
                    }
                    myStream.Close();
                }
            }

        }

        private void chartAccel_DoubleClick(object sender, EventArgs e)
        {
            ExportarCSV(chartAccel);
        }

        private void chartCoche_DoubleClick(object sender, EventArgs e)
        {
            ExportarCSV(chartCoche);
        }

        private void chartESC_Click(object sender, EventArgs e)
        {
            ExportarCSV(chartESC);
        }

        private void chartGyro_DoubleClick(object sender, EventArgs e)
        {
            ExportarCSV(chartGyro);
        }

        private void buttonDetenerGrafica_Click(object sender, EventArgs e)
        {
            dibujarDatos = false;
        }

        private void buttonContinarGrafica_Click(object sender, EventArgs e)
        {
            dibujarDatos = true;
        }

        private void buttonResetZoomGraf_Click(object sender, EventArgs e)
        {
            chartGyro.ChartAreas[0].AxisX.ScaleView.ZoomReset();
            chartGyro.ChartAreas[0].AxisY.ScaleView.ZoomReset();

            chartAccel.ChartAreas[0].AxisX.ScaleView.ZoomReset();
            chartAccel.ChartAreas[0].AxisY.ScaleView.ZoomReset();

            chartCoche.ChartAreas[0].AxisX.ScaleView.ZoomReset();
            chartCoche.ChartAreas[0].AxisY.ScaleView.ZoomReset();

            chartESC.ChartAreas[0].AxisX.ScaleView.ZoomReset();
            chartESC.ChartAreas[0].AxisY.ScaleView.ZoomReset();

        }

        private void buttonLimpiarGraf_Click(object sender, EventArgs e)
        {
            Iniciar_grafica();
            tInicio = DateTime.Now;
        }

        private void labelControl_Click(object sender, EventArgs e)
        {
            if (telemetria.Conectado())
            {
                if (telemetria.tipo_control_actual == Telemetria.E_tipo_control.e_control_BT)
                {
                    telemetria.CambiarControl(Telemetria.E_tipo_control.e_control_RC);
                } else
                {
                    telemetria.CambiarControl(Telemetria.E_tipo_control.e_control_BT);
                }
            }
        }

        private void FormPrincipal_FormClosed(object sender, FormClosedEventArgs e)
        {
            joystick.loop = false;
        }

        private void labelPotencia_Click(object sender, EventArgs e)
        {
            if (telemetria.tipo_regulacion_potencia == Telemetria.E_regulacion_potencia.e_reg_pot_directa)
            {
                telemetria.CambiarControlPotencia(Telemetria.E_regulacion_potencia.e_reg_pot_conservadora);

            } else if (telemetria.tipo_regulacion_potencia == Telemetria.E_regulacion_potencia.e_reg_pot_conservadora)
            {
                telemetria.CambiarControlPotencia(Telemetria.E_regulacion_potencia.e_reg_pot_incremental);
            }
            else if (telemetria.tipo_regulacion_potencia == Telemetria.E_regulacion_potencia.e_reg_pot_incremental)
            {
                telemetria.CambiarControlPotencia(Telemetria.E_regulacion_potencia.e_reg_pot_agresiva);
            }
            else if (telemetria.tipo_regulacion_potencia == Telemetria.E_regulacion_potencia.e_reg_pot_agresiva)
            {
                telemetria.CambiarControlPotencia(Telemetria.E_regulacion_potencia.e_reg_pot_max);
            }
            else
            {
                telemetria.CambiarControlPotencia(Telemetria.E_regulacion_potencia.e_reg_pot_directa);
            }
        }

        private void labelDireccion_Click(object sender, EventArgs e)
        {
            if (telemetria.tipo_regulacion_direccion == Telemetria.E_regulacion_direccion.e_reg_dir_Ackermann)
            {
                telemetria.CambiarControlDireccion(Telemetria.E_regulacion_direccion.e_reg_dir_directa);
            } else
            {
                telemetria.CambiarControlDireccion(Telemetria.E_regulacion_direccion.e_reg_dir_Ackermann);
            }
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
