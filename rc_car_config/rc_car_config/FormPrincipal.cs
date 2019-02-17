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
        

        delegate void logearMSGDelegado(string nuevoMsg);
        delegate void cambiarModoManualDelegado(Boolean estado);
        delegate void anadirPuntoDelegado(double x, double y, byte dato);

        delegate void rellenarCalibracionDelegado(float k1Recibido, float k2Recibido, float k3Recibido, float k4Recibido, byte modoRegulacion, byte vHigh, byte vLow);

        enum ComandosBluetooth : int { Iniciar_Regulacion = 1, Parar_Regulacion, peticion_calibracion, Valores_Calibracion, Mover_Posicion, Cambiar_Tipo_Regula, PedirEstado };


        public FormPrincipal()
        {
            InitializeComponent();
            punteroEntrada = 0;
            conexionCorrecta = false;
            bufferEntradaSerie = new byte[50];
            ultimposicion = new float[4];
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

        public bool Enviar_calibracion_Bluetooth(int K1_nueva, int K2_nueva, int K3_nueva, int K4_nueva)
        {

            byte[] cadenaEnvio = new byte[21];
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
                        cadenaEnvio[indice++] = (byte) ComandosBluetooth.Valores_Calibracion;
                        //NOTA: K1 es un int de 32 bit( - 2,147,483,647 <-> 2,147,483,647)
                        cadenaEnvio[indice++] = (byte)(K1_nueva & 0xFF);
                        cadenaEnvio[indice++] = (byte)((K1_nueva & 0xff00) >> 8);
                        cadenaEnvio[indice++] = (byte)((K1_nueva & 0xff0000) >> 16);
                        cadenaEnvio[indice++] = (byte)((K1_nueva & 0xff000000) >> 24);
                        //NOTA: K2 es un int de 32 bit( - 2,147,483,647 <-> 2,147,483,647)
                        cadenaEnvio[indice++] = (byte)(K2_nueva & 0xFF);
                        cadenaEnvio[indice++] = (byte)((K2_nueva & 0xff00) >> 8);
                        cadenaEnvio[indice++] = (byte)((K2_nueva & 0xff0000) >> 16);
                        cadenaEnvio[indice++] = (byte)((K2_nueva & 0xff000000) >> 24);
                        //NOTA: K3 es un int de 32 bit( - 2,147,483,647 <-> 2,147,483,647)
                        cadenaEnvio[indice++] = (byte)(K3_nueva & 0xFF);
                        cadenaEnvio[indice++] = (byte)((K3_nueva & 0xff00) >> 8);
                        cadenaEnvio[indice++] = (byte)((K3_nueva & 0xff0000) >> 16);
                        cadenaEnvio[indice++] = (byte)((K3_nueva & 0xff000000) >> 24);
                        //NOTA: K4 es un int de 32 bit( - 2,147,483,647 <-> 2,147,483,647)
                        cadenaEnvio[indice++] = (byte)(K4_nueva & 0xFF);
                        cadenaEnvio[indice++] = (byte)((K4_nueva & 0xff00) >> 8);
                        cadenaEnvio[indice++] = (byte)((K4_nueva & 0xff0000) >> 16);
                        cadenaEnvio[indice++] = (byte)((K4_nueva & 0xff000000) >> 24);

                        cadenaEnvio[indice++] = (byte)'\r';
                        cadenaEnvio[indice++] = (byte)'\n';

                        serialPortBluetooth.Write(cadenaEnvio, 0, 21);
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


            byte comando = bufferEntradaSerie[2];


            //todos los comandos basicos tienen un solo byte de datos
            if (comando == 0 || comando == 3 || comando == 2) // recepcion estado
            {
                return 6;
            }
            if (comando == 1) //recepcion datos
            {
                return 28;
            }
            /*
            if (comando == 2) //recepcion msg
            {
                return 50;
            }
            */
            if (comando == 4) //recepcion calibracion
            {
                return 24;
            }

            return 0;
        }

        private void RellenarCalibracion(float k1Recibido, float k2Recibido, float k3Recibido, float k4Recibido, byte modoRegulacion, byte vHigh, byte vLow)
        {

            try
            {
                if (textconfig1.InvokeRequired)
                {
                    rellenarCalibracionDelegado delegado = new rellenarCalibracionDelegado(RellenarCalibracion);
                    //ya que el delegado invocará a CambiarProgreso debemos indicarle los parámetros 
                    object[] parametros = new object[] { k1Recibido, k2Recibido, k3Recibido, k4Recibido, modoRegulacion, vHigh, vLow };
                    //invocamos el método a través del mismo contexto del formulario (this) y enviamos los parámetros 
                    this.BeginInvoke(delegado, parametros);
                }
                else
                {
                    textconfig1.Text = k1Recibido.ToString(); textconfig1.Enabled = true;
                    textconfig2.Text = k2Recibido.ToString(); textconfig2.Enabled = true;
                    textconfig3.Text = k3Recibido.ToString(); textconfig3.Enabled = true;
                    textconfig4.Text = k4Recibido.ToString(); textconfig4.Enabled = true;
                    buttonGuardarCalibracion.Enabled = true;
                    labelFirmareVer.Text = vHigh.ToString() + "." + vLow.ToString();
                }
            }
            catch { }
        }

        private void ProcesarComandoSerie()
        {
            int dato, nuevoEstado;
            float pitch, roll, yaw;
            float rpmActual, ConsignaRPMActual;
            float ConsignaDireccionActual, AnguloRuedaDerecha, AnguloRuedaIzquierda;

            byte comandoChar = bufferEntradaSerie[2];
            switch (comandoChar)
            {
                case 0: //recepcion nuevo estado
                    estadoSistema = bufferEntradaSerie[3];

                    LogearMSG(String.Format(string.Format("{0} {1}", "Recibido nuevo estado del sistema: ", estadoSistema)));
                    break;
                case 1: //recepcion medida
                    dato = (bufferEntradaSerie[3] & 0xff) | ((bufferEntradaSerie[4] & 0xff) << 8) | ((bufferEntradaSerie[5] & 0xff) << 16) | ((bufferEntradaSerie[6] & 0xff) << 24);
                    pitch = (dato) / 1000.0f * 180.0f / (float)Math.PI;
                    dato = (bufferEntradaSerie[7] & 0xff) | ((bufferEntradaSerie[8] & 0xff) << 8) | ((bufferEntradaSerie[9] & 0xff) << 16) | ((bufferEntradaSerie[10] & 0xff) << 24);
                    roll = dato / 1000.0f * 180.0f / (float)Math.PI;
                    nuevoEstado = bufferEntradaSerie[11];


                    //LogearMSG(String.Format(string.Format("medida: {0} {1} {2} {3} {4}", bufferEntradaSerie[7], bufferEntradaSerie[8], bufferEntradaSerie[9], bufferEntradaSerie[10], posicion)));
                    //procesaAnguloPlataforma(anguloPlataforma);
                    //procesaAnguloPendulo(AnguloPendulo);

                    if (nuevoEstado != estadoSistema)
                    {

                    }

                    break;
                case 2: // Mensajes

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
                    else
                    {
                        LogearMSG(String.Format(string.Format("{0} {1}", "Recibido msg: ", dato)));
                    }

                    break;
                case 4: //Valores de calibracion recibidos
                    float k1Recibido;
                    float k2Recibido;
                    float k3Recibido;
                    float k4Recibido;

                    dato = (bufferEntradaSerie[3] & 0xff) | ((bufferEntradaSerie[4] & 0xff) << 8) | ((bufferEntradaSerie[5] & 0xff) << 16) | ((bufferEntradaSerie[6] & 0xff) << 24);
                    k1Recibido = (dato) / 1000.0f;
                    dato = (bufferEntradaSerie[7] & 0xff) | ((bufferEntradaSerie[8] & 0xff) << 8) | ((bufferEntradaSerie[9] & 0xff) << 16) | ((bufferEntradaSerie[10] & 0xff) << 24);
                    k2Recibido = dato / 1000.0f;
                    dato = (bufferEntradaSerie[11] & 0xff) | ((bufferEntradaSerie[12] & 0xff) << 8) | ((bufferEntradaSerie[13] & 0xff) << 16) | ((bufferEntradaSerie[14] & 0xff) << 24);
                    k3Recibido = (dato) / 1000.0f;
                    dato = (bufferEntradaSerie[15] & 0xff) | ((bufferEntradaSerie[16] & 0xff) << 8) | ((bufferEntradaSerie[17] & 0xff) << 16) | ((bufferEntradaSerie[18] & 0xff) << 24);
                    k4Recibido = dato / 1000.0f;


                    RellenarCalibracion(k1Recibido, k2Recibido, k3Recibido, k4Recibido, bufferEntradaSerie[19], bufferEntradaSerie[20], bufferEntradaSerie[21]);
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
            //Enviar_comando_Bluetooth(ComandosBluetooth.PedirEstado, 1);
        }



        private void timerActualizar_Tick(object sender, EventArgs e)
        {
            if (serialPortBluetooth != null)
            {
                if (serialPortBluetooth.IsOpen)
                {
                    //TODO: debug, quitar para release
                    //textInfo1.Text = punteroEntrada.ToString() + ". Plat: " + ultimoAnguloPlataforma.ToString() + "º. Pen: " + ultimoAnguloPendulo.ToString() + "º. E: " + arrastrandoPendulo.ToString() + " X: " + calcularAnguloRaton().ToString() + " pt "+ultimoMouse.ToString() + "|||| "+ (ultimoMouse.X - (panelGrafico.Width / 2 - DIBUJO_2D_OFFSET + 5) -46).ToString() + "/"+ (-(ultimoMouse.Y - (panelGrafico.Height / 2)) -46).ToString();
                    //textInfo1.Refresh();

                    if (panelGrafico.Visible && (tabControl1.SelectedIndex == 1)) panelGrafico.Invalidate();
                    //panelGrafico.Refresh();

                    if (chartDatos.Visible && (tabControl1.SelectedIndex == 0))
                    {
                        chartDatos.Series[0].Points.ResumeUpdates(); chartDatos.Series[1].Points.ResumeUpdates();
                        chartDatos.Invalidate(); //TODO: resume updates ya parece pintar igualmente sin necesidad de invalidad
                    }
                    chartDatos.Series[0].Points.SuspendUpdates(); chartDatos.Series[1].Points.SuspendUpdates();



                }
                else
                {
                    //textInfo1.Text = punteroEntrada.ToString() + ". Sin conexion";
                }
            }
        }

        private void serialPortBluetooth_DataReceived(object sender, SerialDataReceivedEventArgs e)
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

        private void ButtonGuardarCalibracion_Click(object sender, EventArgs e)
        {
            int K1_nueva = 0, K2_nueva = 0, K3_nueva = 0, K4_nueva = 0;
            double valor;

            bool valido1 = double.TryParse(textconfig1.Text, out valor);
            if (valido1)
            {
                K1_nueva = (int)Math.Round(valor * 1000);
            }

            bool valido2 = double.TryParse(textconfig2.Text, out valor);
            if (valido2)
            {
                K2_nueva = (int)Math.Round(valor * 1000);
            }

            bool valido3 = double.TryParse(textconfig3.Text, out valor);
            if (valido3)
            {
                K3_nueva = (int)Math.Round(valor * 1000);
            }

            bool valido4 = double.TryParse(textconfig4.Text, out valor);
            if (valido4)
            {
                K4_nueva = (int)Math.Round(valor * 1000);
            }

            if (valido1 && valido2 && valido3 && valido4)
            {
                Enviar_calibracion_Bluetooth(K1_nueva, K2_nueva, K3_nueva, K4_nueva);
            }
            else
            {
                LogearMSG("Error, valores de calibracion no validos");
            }

        }

    }
}
