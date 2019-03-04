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

        public int punteroEntrada; //puntero para el angulo plataforma

        public int estadoSistema; // ultimo estado del sistema recibido
        public  float[] ultimposicion; // ultimo angulo de plataforma recibido
        DateTime tInicio, tUltimaRecepcionTout;
        const int DIBUJO_2D_OFFSET = 50;
        const long SERIE_TIMEOUT = 3000; //ms maximos para considerar perdida de conexion
        byte[] bufferEntradaSerie;
        Point ultimoMouse;
        int bytesRecibidos;
        bool conexionCorrecta;
        bool dibujarDatos = true;

        public List<TextBox> textCalibracion;
        

        delegate void logearMSGDelegado(string nuevoMsg);
        delegate void cambiarModoManualDelegado(Boolean estado);
        delegate void anadirPuntoDelegado(double x, double y, byte dato);

        delegate void rellenarCalibracionDelegado(float[] parRecibido, int x, int verHight, int verLow);
        delegate void rellenarValoresManualDelegado(int[] valoresManual);

        delegate void nuevosValoresCocheDelegado(float ConsignaMarcha,float ConsignaRPMActual,float AnguloRuedaDerecha,float AnguloRuedaIzquierda,float ESC_VoltajeEntrada,float ESC_rpmActual,float ESC_avgMotorCurrent, float ESC_avgInputCurrent,float ESC_Dutycycle,float modo_actual);
        delegate void nuevosValoresImuDelegado(float pitch, float roll, float yaw, float[] velocidad, float[] aceleracion, float[] angulos, int tipo_control);


        float pitch, roll, yaw;
            float ConsignaMarcha, ConsignaRPMActual, AnguloRuedaDerecha, AnguloRuedaIzquierda, ESC_VoltajeEntrada, ESC_rpmActual, ESC_avgMotorCurrent, ESC_avgInputCurrent, ESC_Dutycycle, modo_actual, tipo_control;
            float[] velocidad ;
            float[] aceleracion;
            float[] angulos;
        

        enum ComandosBluetooth : int {
            auto_parametros = 1,
            auto_imu,
            peticion_coche,
            peticion_imu,
            peticion_calibracion,
            nueva_calibracion,
            cambiar_modo,
            control_remoto,
            PedirEstado,
            pedir_valoresmanual,
            nuevos_valoresmanual
        };

        enum RespuestasBluetooth : int {
            auto_parametros = 1,
            auto_imu,
            datos_coche,
            datosn_imu,
            valores_calibracion,
            cambio_ok,
            cambiar_modo,
            control_remoto,
            estado,
            msg,
            valores_manual
        };


        enum E_modo
        {
            enum_manual = 0,
            enum_sistema_salida = 1,
            enum_semi_auto = 2,
            enum_full_auto = 3
        };

        public FormPrincipal()
        {
            InitializeComponent();
            punteroEntrada = 0;
            conexionCorrecta = false;
            bufferEntradaSerie = new byte[80];
            ultimposicion = new float[4];
            textCalibracion = new List<TextBox>();
             velocidad = new float[3];
            aceleracion = new float[3];
            angulos = new float[4];
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




        public Boolean Abrir_puerto_serie()
        {
            try
            {
                if (serialPortBluetooth != null)
                {
                    if (serialPortBluetooth.IsOpen)
                    {
                        serialPortBluetooth.Close();
                        serialPortBluetooth.PortName = comboPuertoSerie.SelectedItem.ToString();
                        serialPortBluetooth.Open();
                    }
                    else
                    {
                        serialPortBluetooth.PortName = comboPuertoSerie.SelectedItem.ToString();
                        serialPortBluetooth.BaudRate = 115200;
                        serialPortBluetooth.Parity = Parity.None;
                        serialPortBluetooth.StopBits = StopBits.One;
                        serialPortBluetooth.DataBits = 8;
                        serialPortBluetooth.Handshake = Handshake.None;
                        serialPortBluetooth.NewLine = "\r\n";
                        //serialPortBluetooth.RtsEnable = true;
                        serialPortBluetooth.Open();
                    }
                    Iniciar_grafica();
                }
                else
                {
                    // serialPortBluetooth = new SerialPort(comboPuertoSerie.SelectedItem.ToString()); // Crear un objeto nuevo
                    // Usar el integrado en el form
                    serialPortBluetooth.BaudRate = 115200;
                    serialPortBluetooth.Parity = Parity.None;
                    serialPortBluetooth.StopBits = StopBits.One;
                    serialPortBluetooth.DataBits = 8;
                    serialPortBluetooth.Handshake = Handshake.None;
                    serialPortBluetooth.NewLine = "\r\n";
                    //serialPortBluetooth.RtsEnable = true;
                    // serialPortBluetooth.DataReceived += new SerialDataReceivedEventHandler(recibirPuertoSerie);// Crear evento de recepcion
                    serialPortBluetooth.Open();
                }
               LogearMSG(String.Format("establecida connexion serie , puerto {1} a {0} kbs\n", serialPortBluetooth.BaudRate, serialPortBluetooth.PortName)); 
            return true;
            }
            catch (Exception ex)
            {
                LogearMSG(String.Format("{0} ||Exception:  {1}", "Error en la conexion", ex));
                return false;
            }
        }


        private void LogearMSG(string nuevoMsg)
        {
            try
            {
                if (textLogEventos.InvokeRequired)
                {
                    logearMSGDelegado delegado = new logearMSGDelegado(LogearMSG);
                    //ya que el delegado invocará a CambiarProgreso debemos indicarle los parámetros 
                    object[] parametros = new object[] { nuevoMsg };
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
                    textLogEventos.Text = textoInicial + "\r" + nuevoMsg;
                    textLogEventos.SelectionStart = textLogEventos.Text.Length;
                    textLogEventos.ScrollToCaret();
                }
            }
            catch { }
        }

        public bool Enviar_comando_Bluetooth(int comando, int parametro)
        {
            if (serialPortBluetooth != null)
            {
                if (serialPortBluetooth.IsOpen)
                {
                    try
                    {
                        byte[] comandoBuffer = new byte[6];
                        comandoBuffer[0] = (byte)'$';
                        comandoBuffer[1] = (2);
                        comandoBuffer[2] = (byte)(comando);
                        comandoBuffer[3] = (byte)(parametro);
                        comandoBuffer[4] = (byte)'\r';
                        comandoBuffer[5] = (byte)'\n';
                        //string sBuffer= System.Text.ASCIIEncoding.ASCII.GetString(comandoBuffer)

                        serialPortBluetooth.Write(comandoBuffer, 0, comandoBuffer.GetLength(0));
                        //serialPortBluetooth.WriteLine(sBuffer);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        LogearMSG(String.Format("{0} ||Exception:  {1}", "Error en el envio", ex));
                    }
                }
                else
                {
                    // TODO: Abrir el puerto si se cerro?
                    //Abrir_puerto_serie();
                    //timer1.Enabled = true;
                };
            }
            return false;
        }

        public bool Enviar_calibracion_Bluetooth(int[] nuevaConfig)
        {

            byte[] cadenaEnvio = new byte[61];
            if (serialPortBluetooth != null)
            {
                if (serialPortBluetooth.IsOpen)
                {
                    try
                    {
                        //comando
                        int indice = 0;

                    
                       

                        cadenaEnvio[indice++] = (byte)'$';
                        cadenaEnvio[indice++] = 2;
                        cadenaEnvio[indice++] = (byte) ComandosBluetooth.nueva_calibracion;

                        for (int indiPar = 1; indiPar <= 14; indiPar++)
                        {
                            //NOTA: K1 es un int de 32 bit( - 2,147,483,647 <-> 2,147,483,647
                            cadenaEnvio[indice++] = (byte)(nuevaConfig[indiPar] & 0xFF);
                            cadenaEnvio[indice++] = (byte)((nuevaConfig[indiPar] & 0xff00) >> 8);
                            cadenaEnvio[indice++] = (byte)((nuevaConfig[indiPar] & 0xff0000) >> 16);
                            cadenaEnvio[indice++] = (byte)((nuevaConfig[indiPar] & 0xff000000) >> 24);
                        }

                       

                        cadenaEnvio[indice++] = (byte)'\r';
                        cadenaEnvio[indice++] = (byte)'\n';

                        serialPortBluetooth.Write(cadenaEnvio, 0, cadenaEnvio.Length);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        LogearMSG(String.Format("{0} ||Exception:  {1}", "Error en el envio", ex));
                    }
                }
                else
                {
                    // TODO: Abrir el puerto si se cerro?
                    //Abrir_puerto_serie();
                    //timer1.Enabled = true;
                };
            }
            return false;
        }

        public bool Enviar_valores_manuales(int[] valores_manual)
        {

            byte[] cadenaEnvio = new byte[13];
            if (serialPortBluetooth != null)
            {
                if (serialPortBluetooth.IsOpen)
                {
                    try
                    {
                        //comando
                        int indice = 0;

                        cadenaEnvio[indice++] = (byte)'$';
                        cadenaEnvio[indice++] = 2;
                        cadenaEnvio[indice++] = (byte)ComandosBluetooth.nuevos_valoresmanual;

                        for (int indiPar = 0; indiPar < valores_manual.Length; indiPar++)
                        {
                            //NOTA: K1 es un int de 32 bit( - 2,147,483,647 <-> 2,147,483,647
                            cadenaEnvio[indice++] = (byte)(valores_manual[indiPar] & 0xFF);
                            cadenaEnvio[indice++] = (byte)((valores_manual[indiPar] & 0xff00) >> 8);
                        }



                        cadenaEnvio[indice++] = (byte)'\r';
                        cadenaEnvio[indice++] = (byte)'\n';

                        serialPortBluetooth.Write(cadenaEnvio, 0, cadenaEnvio.Length);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        LogearMSG(String.Format("{0} ||Exception:  {1}", "Error en el envio", ex));
                    }
                }
                else
                {
                    // TODO: Abrir el puerto si se cerro?
                    //Abrir_puerto_serie();
                    //timer1.Enabled = true;
                };
            }
            return false;
        }

        private bool Enviar_comando_Bluetooth(ComandosBluetooth comando, int parametro)
        {
            return Enviar_comando_Bluetooth((int)comando, parametro);
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

        private void CambiarModoManual(Boolean estado)
        {

        }

       
       



        bool CombrobarFinCadena()
        {
            int longitudMensaje = ReconocerComando();

            if (bytesRecibidos > longitudMensaje) return true; //error longitud excesiba

            if ((bytesRecibidos == (longitudMensaje - 1)) && (bufferEntradaSerie[longitudMensaje - 2] != '\r')) return true; //error terminador 1
            if ((bytesRecibidos == longitudMensaje) && (bufferEntradaSerie[longitudMensaje - 1] != '\n')) return true; //error terminador 2

            return false;
        }

        int ReconocerComando()
        {

            if (bytesRecibidos <= 3) return (50 - 1); // aun no se ha recibido el ID de comando(NOTA al recibirlo en el array se suma por tanto el indice ha de ser 4 o mas para q este leido), suponer longitud maxima


            RespuestasBluetooth comando = (RespuestasBluetooth) bufferEntradaSerie[2];


            //todos los comandos basicos tienen un solo byte de datos
            if (comando == RespuestasBluetooth.msg ||  comando == RespuestasBluetooth.auto_imu || comando == RespuestasBluetooth.auto_parametros || comando== RespuestasBluetooth.cambiar_modo || comando == RespuestasBluetooth.cambio_ok || comando == RespuestasBluetooth.control_remoto) // recepcion estado
            {
                return 6;
            }
            if (comando == RespuestasBluetooth.datos_coche) //recepcion datos
            {
                return 23;
            }
            
            if (comando == RespuestasBluetooth.datosn_imu) //recepcion msg
            {
                return 58;
            }
           
            if (comando == RespuestasBluetooth.valores_calibracion) //recepcion calibracion
            {
                return 64;
            }

            if (comando == RespuestasBluetooth.valores_manual) //recepcion msg
            {
                return 13;
            }

            return 0;
        }

        private void RellenarCalibracion(float[] parRecibido, int x , int verHight, int verLow)
        {

            try
            {
                if (textconfig1.InvokeRequired)
                {
                    rellenarCalibracionDelegado delegado = new rellenarCalibracionDelegado(RellenarCalibracion);
                    //ya que el delegado invocará a CambiarProgreso debemos indicarle los parámetros 
                    object[] parametros = new object[] { parRecibido,  x,  verHight,  verLow };
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
                           if ((valor1 >= parRecibido.GetLowerBound(0)) && (valor1 <= parRecibido.GetUpperBound(0)))
                            {
                                t.Text= parRecibido[valor1].ToString();
                                t.Enabled = true;
                            }

                        }

                    }

                    buttonGuardarCalibracion.Enabled = true;
                    labelFirmareVer.Text = verHight.ToString() + "." + verLow.ToString();
                }
            }
            catch { }
        }

        private void ProcesarComandoSerie()
        {
            int dato, nuevoEstado;
           
            int posBuffer = 3;

            RespuestasBluetooth comandoChar = (RespuestasBluetooth) bufferEntradaSerie[2];
            switch (comandoChar)
            {
                case RespuestasBluetooth.estado: //recepcion nuevo estado
                    estadoSistema = bufferEntradaSerie[3];

                    LogearMSG(String.Format(string.Format("{0} {1}", "Recibido nuevo estado del sistema: ", estadoSistema)));
                    break;

                case RespuestasBluetooth.datos_coche: //recepcion medida
                    posBuffer = 3;
                    dato = (bufferEntradaSerie[posBuffer++] & 0xff) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 8);
                    ConsignaRPMActual = (dato) /  10.0f ;
                    dato = (bufferEntradaSerie[posBuffer++] & 0xff) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 8) ;
                    ConsignaMarcha = dato / 10.0f;
                    dato = (bufferEntradaSerie[posBuffer++] & 0xff) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 8);
                    AnguloRuedaDerecha = dato / 10.0f;
                    dato = (bufferEntradaSerie[posBuffer++] & 0xff) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 8);
                    AnguloRuedaIzquierda = dato / 10.0f;
                    dato = (bufferEntradaSerie[posBuffer++] & 0xff) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 8);
                    ESC_VoltajeEntrada = dato / 10.0f;
                    dato = (bufferEntradaSerie[posBuffer++] & 0xff) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 8);
                    ESC_rpmActual = dato * 10.0f;
                    dato = (bufferEntradaSerie[posBuffer++] & 0xff) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 8);
                    ESC_avgMotorCurrent = dato / 100.0f;
                    dato = (bufferEntradaSerie[posBuffer++] & 0xff) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 8);
                    ESC_avgInputCurrent = dato / 100.0f;
                    dato = (bufferEntradaSerie[posBuffer++] & 0xff);
                    ESC_Dutycycle = dato ;
                    dato = (bufferEntradaSerie[posBuffer++] & 0xff);
                    modo_actual = dato;
                    
                    // LogearMSG(String.Format(string.Format(@"RPM: {0} RuedaDer {1} RuedaIzq {2}  Marcha {3} ESC_Voltaje {4} ESC_rpm {5} ESC_avgMotorCurrent {6} ESC_avgInputCurrent {7} ESC_Dutycycle {8}",
                    //     ConsignaRPMActual, ConsignaDireccionActual, AnguloRuedaDerecha, AnguloRuedaIzquierda, ESC_VoltajeEntrada, ESC_rpmActual, ESC_avgMotorCurrent, ESC_avgInputCurrent, ESC_Dutycycle)));


                    break;

                case RespuestasBluetooth.datosn_imu: //recepcion medida
                    posBuffer = 3;
                    dato = (bufferEntradaSerie[posBuffer++] & 0xff) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 8) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 16) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 24);
                    pitch = (dato) / 1000.0f;
                    dato = (bufferEntradaSerie[posBuffer++] & 0xff) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 8) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 16) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 24);
                    roll = dato / 1000.0f;
                    dato = (bufferEntradaSerie[posBuffer++] & 0xff) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 8) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 16) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 24);
                    yaw = dato / 1000.0f;
                    dato = (bufferEntradaSerie[posBuffer++] & 0xff) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 8) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 16) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 24);
                    velocidad[0] = dato / 1000.0f;
                    dato = (bufferEntradaSerie[posBuffer++] & 0xff) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 8) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 16) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 24);
                    velocidad[1] = dato / 1000.0f;
                    dato = (bufferEntradaSerie[posBuffer++] & 0xff) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 8) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 16) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 24);
                    velocidad[2] = dato / 1000.0f;
                    dato = (bufferEntradaSerie[posBuffer++] & 0xff) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 8) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 16) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 24);
                    aceleracion[0] = dato / 1000.0f;
                    dato = (bufferEntradaSerie[posBuffer++] & 0xff) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 8) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 16) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 24);
                    aceleracion[1] = dato / 1000.0f;
                    dato = (bufferEntradaSerie[posBuffer++] & 0xff) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 8) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 16) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 24);
                    aceleracion[2] = dato / 1000.0f;
                    dato = (bufferEntradaSerie[posBuffer++] & 0xff) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 8) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 16) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 24);
                    angulos[0] = dato / 1000.0f;
                    dato = (bufferEntradaSerie[posBuffer++] & 0xff) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 8) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 16) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 24);
                    angulos[1] = dato / 1000.0f;
                    dato = (bufferEntradaSerie[posBuffer++] & 0xff) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 8) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 16) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 24);
                    angulos[2] = dato / 1000.0f;
                    dato = (bufferEntradaSerie[posBuffer++] & 0xff) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 8) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 16) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 24);
                    angulos[3] = dato / 1000.0f;
                    dato = (bufferEntradaSerie[posBuffer++] & 0xff);
                    tipo_control = dato;

                    //LogearMSG(String.Format(string.Format("medida: {0} {1} {2} {3} {4}", pitch, roll, yaw, velocidad[0], tipo_control)));



                    break;
                case RespuestasBluetooth.msg: // Mensajes

                    dato = bufferEntradaSerie[3];
                    if (dato == 1)
                    {
                        LogearMSG(String.Format(string.Format("{0} {1}", "Respuesta de error 'comando no reconocido' de Bluetooth", comandoChar)));
                    }
                    else if (dato == 2)
                    {
                        LogearMSG(String.Format(string.Format("{0} {1}", "Respuesta de error, recibido fin de cadena no esperado", comandoChar)));
                    }
                    else if (dato == 3)
                    {
                        LogearMSG(String.Format(string.Format("{0} {1}", "Respuesta de error 'comando demasiado largo' de Bluetooth", comandoChar)));
                    }
                    else if (dato == 4)
                    {
                        LogearMSG(String.Format(string.Format("{0} {1}", "Iniciada medida EN388", comandoChar)));
                    }
                    else if (dato == 5)
                    {
                        LogearMSG(String.Format(string.Format("{0} {1}", "iniciada medida MedidaA STM F1342", comandoChar)));
                    }
                    else if (dato == 6)
                    {
                        LogearMSG(String.Format(string.Format("{0} {1}", "Iniciado ciclo rapido", comandoChar)));
                    }
                    else if (dato == 7)
                    {
                        LogearMSG(String.Format(string.Format("{0} {1}", "Nueva configuracion de calibracion recibida", comandoChar)));
                    }
                    else if (dato == 8)
                    {
                        LogearMSG(String.Format(string.Format("{0} {1}", "Activado envio automatico de datos del coche", comandoChar)));
                    }
                    else if (dato == 9)
                    {
                        LogearMSG(String.Format(string.Format("{0} {1}", "Activado envio automatico de telemetria", comandoChar)));
                    }
                    else if (dato == 10)
                    {
                        LogearMSG(String.Format(string.Format("{0} {1}", "Nuevos valores manuales OK", comandoChar)));
                    }
                    else if (dato == 10)
                    {
                        LogearMSG(String.Format(string.Format("{0} {1}", "Cambio modo OK", comandoChar)));
                    }
                    else
                    {
                        LogearMSG(String.Format(string.Format("{0} {1}", "Recibido msg: ", dato)));
                    }

                    break;
                case RespuestasBluetooth.valores_calibracion: //Valores de calibracion recibidos
                    float[] parRecibido = new float[19];
                    
                    posBuffer = 3;
                    for (int indiPar = 1; indiPar <=14; indiPar++) { 
                        dato = (bufferEntradaSerie[posBuffer++] & 0xff) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 8) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 16) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 24);
                        parRecibido[indiPar] = (dato) / 1000.0f;
                    }
                    int x, verHight, verLow;

                    x = (bufferEntradaSerie[posBuffer++] & 0xff);
                    verHight = (bufferEntradaSerie[posBuffer++] & 0xff);
                    verLow = (bufferEntradaSerie[posBuffer++] & 0xff);
                    
                    RellenarCalibracion(parRecibido, x, verHight, verLow);
                    break;

                case RespuestasBluetooth.valores_manual: //Valores de manual recibidos
                    posBuffer = 3;
                    int[] valores = new int[4];

                    valores[0] = (bufferEntradaSerie[posBuffer++] & 0xff) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 8);
                    valores[1] = (bufferEntradaSerie[posBuffer++] & 0xff) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 8);
                    valores[2] = (bufferEntradaSerie[posBuffer++] & 0xff) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 8);
                    valores[3] = (bufferEntradaSerie[posBuffer++] & 0xff) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 8);
                    

                    RellenarValoresManual(valores);
                    break;
                default:
                    LogearMSG(String.Format(string.Format("{0} {1}", "Error comando no reconocido recibido", comandoChar)));
                    break;

            }
        }

        private void ButtonConectar_Click(object sender, EventArgs e)
        {
            tInicio = DateTime.Now;
            Abrir_puerto_serie();
            timerActualizar.Enabled = true;
            Enviar_comando_Bluetooth(ComandosBluetooth.auto_parametros, 1);
            Enviar_comando_Bluetooth(ComandosBluetooth.auto_imu, 1);
        }


       
        private void Actualizar_datos_manual(float voltage, float corriente, float eRPM, float temp, float mAh)
        {
            labelVoltage.Text = "V: "+ voltage.ToString();
            labelCorriente.Text = "A: " + corriente.ToString();
            labelRPM.Text = "RPM: " + eRPM.ToString();
            labelTemperatura.Text = "temp: " + temp.ToString();
            labelMAH.Text = "MaH: " + mAh.ToString();
        }

        private void TimerActualizar_Tick(object sender, EventArgs e)
        {
            if (serialPortBluetooth != null)
            {
                if (serialPortBluetooth.IsOpen)
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

                    Actualizar_datos_manual(ESC_VoltajeEntrada, ESC_avgInputCurrent, ESC_rpmActual, ESC_Dutycycle, ESC_avgMotorCurrent);

                }
                else
                {
                    //textInfo1.Text = punteroEntrada.ToString() + ". Sin conexion";
                }
            }
        }

        

        private void SerialPortBluetooth_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            while (sp.BytesToRead > 0)
            {
                byte inByte = (byte)sp.ReadByte();

                try
                {
                    if (!conexionCorrecta && inByte != '$') return;
                    conexionCorrecta = true;

                    if ((bytesRecibidos == 1 && inByte != 2) || CombrobarFinCadena())
                    {
                        LogearMSG(String.Format("{0} Error de recepcion de datos , final inesperado {1} \n", bytesRecibidos, bufferEntradaSerie));
                        bytesRecibidos = 0;
                        conexionCorrecta = false;
                        return;
                    }

                    tUltimaRecepcionTout = DateTime.Now.AddMilliseconds(SERIE_TIMEOUT); //TODO: arreglar
                    if (bytesRecibidos > 0 || inByte == '$')
                    {

                        bufferEntradaSerie[bytesRecibidos] = inByte;
                        bytesRecibidos++;

                        if (bytesRecibidos == ReconocerComando()) //|| DateTime.Now > tUltimaRecepcionTout
                        {
                            ProcesarComandoSerie();
                            bytesRecibidos = 0;
                        };
                    };




                }
                catch (Exception ex) { LogearMSG(String.Format("{0} {1} ***********************\n {2}", inByte, bytesRecibidos, "||Exception:  ", ex)); }
            }
        }

        private void TabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControlPrincipal.SelectedIndex == 3)
            {
                Enviar_comando_Bluetooth(ComandosBluetooth.peticion_calibracion, 1);
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            Enviar_comando_Bluetooth(ComandosBluetooth.cambiar_modo, (int)E_modo.enum_manual);
        }

        private void ButtonAuto_Click(object sender, EventArgs e)
        {
            Enviar_comando_Bluetooth(ComandosBluetooth.cambiar_modo, (int)E_modo.enum_full_auto);
        }

        private void ButtonSemiAuto_Click(object sender, EventArgs e)
        {
            Enviar_comando_Bluetooth(ComandosBluetooth.cambiar_modo, (int)E_modo.enum_semi_auto);
        }

        private void ButtonSemiControler_Click(object sender, EventArgs e)
        {
            Enviar_comando_Bluetooth(ComandosBluetooth.cambiar_modo, (int)E_modo.enum_sistema_salida);
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

            Enviar_valores_manuales(nueva_valores);

        }



        private void RellenarValoresManual(int[] valoresManual)
        {
            try
            {
                if (trackBarRuedaDer.InvokeRequired)
                {
                    rellenarValoresManualDelegado delegado = new rellenarValoresManualDelegado(RellenarValoresManual);
                    //ya que el delegado invocará a CambiarProgreso debemos indicarle los parámetros 
                    object[] parametros = new object[] { valoresManual };
                    //invocamos el método a través del mismo contexto del formulario (this) y enviamos los parámetros 
                    this.BeginInvoke(delegado, parametros);
                }
                else
                {
                    if (valoresManual.Length >= 3){ 

                        trackBarRuedaDer.Value = valoresManual[0];
                        trackBarRuedaIzq.Value = valoresManual[1];
                        trackBarMarcha.Value = valoresManual[2];
                        trackBarMotor.Value = valoresManual[3];
                    }

                }
            }
            catch { }
        }


        private void RellenarValoresCoche(float ConsignaMarcha, float ConsignaRPMActual, float AnguloRuedaDerecha, float AnguloRuedaIzquierda, float ESC_VoltajeEntrada, float ESC_rpmActual, float ESC_avgMotorCurrent, float ESC_avgInputCurrent, float ESC_Dutycycle, float modo_actual)
        {
            if (trackBarRuedaDer.InvokeRequired)
            {
                nuevosValoresCocheDelegado delegado = new nuevosValoresCocheDelegado(RellenarValoresCoche);
                //ya que el delegado invocará a CambiarProgreso debemos indicarle los parámetros 
                object[] parametros = new object[] { ConsignaMarcha,  ConsignaRPMActual,  AnguloRuedaDerecha,  AnguloRuedaIzquierda,  ESC_VoltajeEntrada,  ESC_rpmActual,  ESC_avgMotorCurrent,  ESC_avgInputCurrent,  ESC_Dutycycle,  modo_actual };
                //invocamos el método a través del mismo contexto del formulario (this) y enviamos los parámetros 
                this.BeginInvoke(delegado, parametros);
            }
            else
            {
                

            }

        }
        private void RellenarValoresIMU(float pitch, float roll, float yaw, float[] velocidad, float[] aceleracion, float[] angulos, int tipo_control)
        {
            if (trackBarRuedaDer.InvokeRequired)
            {
                nuevosValoresImuDelegado delegado = new nuevosValoresImuDelegado(RellenarValoresIMU);
                //ya que el delegado invocará a CambiarProgreso debemos indicarle los parámetros 
                object[] parametros = new object[] {  pitch,  roll,  yaw,  velocidad,  aceleracion, angulos,  tipo_control };
                //invocamos el método a través del mismo contexto del formulario (this) y enviamos los parámetros 
                this.BeginInvoke(delegado, parametros);
            }
            else
            {


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
                Enviar_calibracion_Bluetooth(nueva_config);
            }
            else
            {
                LogearMSG("Error, valores de calibracion no validos");
            }

        }

    }
}
