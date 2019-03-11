using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Windows.Forms;


namespace rc_car_config
{
    class Telemetria
    {
        public SerialPort serialPortBluetooth;
        public String PuertoSerie;
        public System.Windows.Forms.RichTextBox textLogEventos;
       

        public float pitch, roll, yaw, imu_temp;
        public float ConsignaMarcha, ConsignaRPMActual, AnguloRuedaDerecha, AnguloRuedaIzquierda, ESC_VoltajeEntrada, ESC_rpmActual, ESC_avgMotorCurrent, ESC_avgInputCurrent, ESC_Dutycycle, modo_actual;
        public int tipo_control;
        public float[] velocidad;
        public float[] aceleracion;
        public float[] angulos;

        public int punteroEntrada; //puntero para el angulo plataforma

        public int estadoSistema; // ultimo estado del sistema recibido
        public float[] ultimposicion; // ultimo angulo de plataforma recibido
        DateTime tInicio, tUltimaRecepcionTout;
       
        const long SERIE_TIMEOUT = 3000; //ms maximos para considerar perdida de conexion
        byte[] bufferEntradaSerie;
       
        int bytesRecibidos;
        bool conexionCorrecta;
     

       public enum ComandosBluetooth : int
        {
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

        public enum RespuestasBluetooth : int
        {
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


       public enum E_modo
        {
            enum_manual = 0,
            enum_sistema_salida = 1,
            enum_semi_auto = 2,
            enum_full_auto = 3
        };

        public Telemetria()
        {
            punteroEntrada = 0;
            conexionCorrecta = false;
            bufferEntradaSerie = new byte[80];
            ultimposicion = new float[4];
            velocidad = new float[3];
            aceleracion = new float[3];
            angulos = new float[4];
            this.serialPortBluetooth = new SerialPort();
            this.serialPortBluetooth.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.SerialPortBluetooth_DataReceived);
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
                        serialPortBluetooth.PortName = PuertoSerie;
                        serialPortBluetooth.Open();
                    }
                    else
                    {
                        serialPortBluetooth.PortName = PuertoSerie;
                        serialPortBluetooth.BaudRate = 115200;
                        serialPortBluetooth.Parity = Parity.None;
                        serialPortBluetooth.StopBits = StopBits.One;
                        serialPortBluetooth.DataBits = 8;
                        serialPortBluetooth.Handshake = Handshake.None;
                        serialPortBluetooth.NewLine = "\r\n";
                        //serialPortBluetooth.RtsEnable = true;
                        serialPortBluetooth.Open();
                    }
                
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

        private bool CheckSerialConection()
        {
            try
            {
                if (serialPortBluetooth != null)
                {
                    if (serialPortBluetooth.IsOpen)
                    {
                        return true;
                    } 
                }

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
                
                return true;
            }
            catch 
            {
                return false;
            }
        }

        public void Change_autoTelemetry(bool coche, bool imu)
        {
            if (CheckSerialConection()) { 
                Enviar_comando_Bluetooth(ComandosBluetooth.auto_parametros, coche ? 1 : 0);
                Enviar_comando_Bluetooth(ComandosBluetooth.auto_imu, imu ? 1 : 0);
            }
        }

        public bool Conectado()
        {
            if (serialPortBluetooth != null)
            {
                if (serialPortBluetooth.IsOpen)
                {
                    return true;
                }
            }
            return false;
        }
        /******************************************************************/
        private void  LogearMSG( string mensage)
        {
            LogearMSGEventArgs parametros = new LogearMSGEventArgs
            {
                Msg = mensage,
                Time = DateTime.Now
            };
            LogearMSGevent?.Invoke(this, parametros);
        }

        public event EventHandler<LogearMSGEventArgs> LogearMSGevent;

        public class LogearMSGEventArgs : EventArgs
        {
            public string Msg { get; set; }
            public DateTime Time { get; set; }
        }

        
        /******************************************************************/
        private void OnRecividosDatosCoche(float p_ConsignaMarcha, float p_ConsignaRPMActual, float p_AnguloRuedaDerecha, float p_AnguloRuedaIzquierda, float p_ESC_VoltajeEntrada, float p_ESC_rpmActual, float p_ESC_avgMotorCurrent, float p_ESC_avgInputCurrent, float p_ESC_Dutycycle, float p_modo_actual)
        {
            DatosCocheEventArgs parametros = new DatosCocheEventArgs() {
                ConsignaMarcha=p_ConsignaMarcha,
                ConsignaRPMActual=p_ConsignaRPMActual,
                AnguloRuedaDerecha=p_AnguloRuedaDerecha,
                AnguloRuedaIzquierda=p_AnguloRuedaIzquierda,
                ESC_VoltajeEntrada=p_ESC_VoltajeEntrada,
                ESC_avgInputCurrent=p_ESC_avgInputCurrent,
                ESC_avgMotorCurrent=p_ESC_avgMotorCurrent,
                ESC_Dutycycle=p_ESC_Dutycycle,
                ESC_rpmActual=p_ESC_rpmActual,
                Modo_actual=p_modo_actual,
                 Time=DateTime.UtcNow
            };

            RecividosDatosCocheEvent?.Invoke(this, parametros);
        }

        public event EventHandler<DatosCocheEventArgs> RecividosDatosCocheEvent;

        public class DatosCocheEventArgs : EventArgs
        {
            public DateTime Time { get; set; }

            public float ConsignaMarcha { get; set; }
            public float ConsignaRPMActual { get; set; }
            public float AnguloRuedaDerecha { get; set; }
            public float AnguloRuedaIzquierda { get; set; }
            public float ESC_VoltajeEntrada { get; set; }
            public float ESC_rpmActual { get; set; }
            public float ESC_avgMotorCurrent { get; set; }
            public float ESC_avgInputCurrent { get; set; }
            public float ESC_Dutycycle { get; set; }
            public float Modo_actual { get; set; }
        }
        /******************************************************************/
        private void OnRecividosDatosIMU(float p_pitch, float p_roll, float p_yaw, float[] p_velocidad, float[] p_aceleracion, float[] p_angulos, int p_tipo_control, float p_imu_temp)
        {
            DatosIMUEventArgs parametros = new DatosIMUEventArgs()
            {
                Time = DateTime.UtcNow,
                Pitch = p_pitch,
                Roll=p_roll,
                Yaw=p_yaw,
                Velocidad=p_velocidad,
                Aceleracion=p_aceleracion,
                Angulos=p_angulos,
                Tipo_control=p_tipo_control,
                Imu_temp=p_imu_temp
            };

            RecividosDatosIMUEvent?.Invoke(this, parametros);
        }

        public event EventHandler<DatosIMUEventArgs> RecividosDatosIMUEvent;

        public class DatosIMUEventArgs : EventArgs
        {
            public float Pitch { get; set; }
            public float Roll { get; set; }
            public float Yaw { get; set; }
            public float[] Velocidad { get; set; }
            public float[] Aceleracion { get; set; }
            public float[] Angulos { get; set; }
            public int Tipo_control { get; set; }
            public float Imu_temp { get; set; }
            public DateTime Time { get; set; }
        }
        /******************************************************************/
        private void OnRecividosDatosCalibracion(float[] p_parRecibido, int p_x, int p_verHight, int p_verLow)
        {
            DatosCalibracionEventArgs parametros = new DatosCalibracionEventArgs() {
                ParRecibido=p_parRecibido,
                X=p_x,
                VerHight=p_verHight,
                VerLow=p_verLow
            };

            RecividosDatosCalibracionEvent?.Invoke(this, parametros);
        }
      
        public event EventHandler<DatosCalibracionEventArgs> RecividosDatosCalibracionEvent;

        public class DatosCalibracionEventArgs : EventArgs
        {
            public DateTime Time { get; set; }
            public float[] ParRecibido { get; set; }
            public int X { get; set; }
            public int VerHight { get; set; }
            public int VerLow { get; set; }
        }
        /******************************************************************/
        private void OnRecividosDatosManual(int[] p_parRecibido)
        {
            DatosManualEventArgs parametros = new DatosManualEventArgs()
            {
                ParRecibido = p_parRecibido
            };

            RecividosDatosManual?.Invoke(this, parametros);
        }

        public event EventHandler<DatosManualEventArgs> RecividosDatosManual;

        public class DatosManualEventArgs : EventArgs
        {
            public DateTime Time { get; set; }
            public int[] ParRecibido { get; set; }
        }
        /******************************************************************/
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
                        cadenaEnvio[indice++] = (byte)ComandosBluetooth.nueva_calibracion;

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


            RespuestasBluetooth comando = (RespuestasBluetooth)bufferEntradaSerie[2];


            //todos los comandos basicos tienen un solo byte de datos
            if (comando == RespuestasBluetooth.msg || comando == RespuestasBluetooth.auto_imu || comando == RespuestasBluetooth.auto_parametros || comando == RespuestasBluetooth.cambiar_modo || comando == RespuestasBluetooth.cambio_ok || comando == RespuestasBluetooth.control_remoto) // recepcion estado
            {
                return 6;
            }
            if (comando == RespuestasBluetooth.datos_coche) //recepcion datos
            {
                return 23;
            }

            if (comando == RespuestasBluetooth.datosn_imu) //recepcion msg
            {
                return 62;
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

      
        private void ProcesarComandoSerie()
        {
            int dato, nuevoEstado;
            short dato16;


            int posBuffer = 3;

            RespuestasBluetooth comandoChar = (RespuestasBluetooth)bufferEntradaSerie[2];
            switch (comandoChar)
            {
                case RespuestasBluetooth.estado: //recepcion nuevo estado
                    estadoSistema = bufferEntradaSerie[3];

                    LogearMSG(String.Format(string.Format("{0} {1}", "Recibido nuevo estado del sistema: ", estadoSistema)));
                    break;

                case RespuestasBluetooth.datos_coche: //recepcion medida
                    posBuffer = 3;
                    short test = (short)((bufferEntradaSerie[3]) | ((bufferEntradaSerie[4]) << 8) & 0xffff);
                    short test2 = (short)((bufferEntradaSerie[3]) | ((bufferEntradaSerie[4]) << 8));
                    short test3 = (short)((bufferEntradaSerie[4]) << 8);

                    dato = (short)((bufferEntradaSerie[posBuffer++]) | ((bufferEntradaSerie[posBuffer++]) << 8));
                    ConsignaRPMActual = (dato) / 10.0f;
                    dato = (short)((bufferEntradaSerie[posBuffer++]) | ((bufferEntradaSerie[posBuffer++]) << 8));
                    ConsignaMarcha = dato / 10.0f;
                    dato = (short)((bufferEntradaSerie[posBuffer++]) | ((bufferEntradaSerie[posBuffer++]) << 8));
                    AnguloRuedaDerecha = dato / 10.0f;
                    dato = (short)((bufferEntradaSerie[posBuffer++]) | ((bufferEntradaSerie[posBuffer++]) << 8));
                    AnguloRuedaIzquierda = dato / 10.0f;
                    dato = (short)((bufferEntradaSerie[posBuffer++]) | ((bufferEntradaSerie[posBuffer++]) << 8));
                    ESC_VoltajeEntrada = dato / 10.0f;
                    dato = (bufferEntradaSerie[posBuffer++]) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 8);
                    ESC_rpmActual = dato * 10.0f;
                    dato = (bufferEntradaSerie[posBuffer++]) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 8);
                    ESC_avgMotorCurrent = dato / 100.0f;
                    dato = (bufferEntradaSerie[posBuffer++]) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 8);
                    ESC_avgInputCurrent = dato / 100.0f;
                    dato = (bufferEntradaSerie[posBuffer++] & 0xff);
                    ESC_Dutycycle = dato;
                    dato = (bufferEntradaSerie[posBuffer++] & 0xff);
                    modo_actual = dato;

                    OnRecividosDatosCoche(ConsignaMarcha, ConsignaRPMActual, AnguloRuedaDerecha, AnguloRuedaIzquierda, ESC_VoltajeEntrada, ESC_rpmActual, ESC_avgInputCurrent, ESC_avgInputCurrent, ESC_Dutycycle, modo_actual);
                    // LogearMSG(String.Format(string.Format(@"RPM: {0} RuedaDer {1} RuedaIzq {2}  Marcha {3} ESC_Voltaje {4} ESC_rpm {5} ESC_avgMotorCurrent {6} ESC_avgInputCurrent {7} ESC_Dutycycle {8}",
                    //     ConsignaRPMActual, ConsignaDireccionActual, AnguloRuedaDerecha, AnguloRuedaIzquierda, ESC_VoltajeEntrada, ESC_rpmActual, ESC_avgMotorCurrent, ESC_avgInputCurrent, ESC_Dutycycle)));


                    break;

                case RespuestasBluetooth.datosn_imu: //recepcion medida
                    posBuffer = 3;
                    dato = (bufferEntradaSerie[posBuffer++]) | ((bufferEntradaSerie[posBuffer++]) << 8) | ((bufferEntradaSerie[posBuffer++]) << 16) | ((bufferEntradaSerie[posBuffer++]) << 24);
                    pitch = (dato) / 1000.0f;
                    dato = (bufferEntradaSerie[posBuffer++]) | ((bufferEntradaSerie[posBuffer++]) << 8) | ((bufferEntradaSerie[posBuffer++]) << 16) | ((bufferEntradaSerie[posBuffer++]) << 24);
                    roll = dato / 1000.0f;
                    dato = (bufferEntradaSerie[posBuffer++]) | ((bufferEntradaSerie[posBuffer++]) << 8) | ((bufferEntradaSerie[posBuffer++]) << 16) | ((bufferEntradaSerie[posBuffer++]) << 24);
                    yaw = dato / 1000.0f;
                    dato = (bufferEntradaSerie[posBuffer++]) | ((bufferEntradaSerie[posBuffer++]) << 8) | ((bufferEntradaSerie[posBuffer++]) << 16) | ((bufferEntradaSerie[posBuffer++]) << 24);
                    velocidad[0] = dato / 1000.0f;
                    dato = (bufferEntradaSerie[posBuffer++]) | ((bufferEntradaSerie[posBuffer++]) << 8) | ((bufferEntradaSerie[posBuffer++]) << 16) | ((bufferEntradaSerie[posBuffer++]) << 24);
                    velocidad[1] = dato / 1000.0f;
                    dato = (bufferEntradaSerie[posBuffer++]) | ((bufferEntradaSerie[posBuffer++]) << 8) | ((bufferEntradaSerie[posBuffer++]) << 16) | ((bufferEntradaSerie[posBuffer++]) << 24);
                    velocidad[2] = dato / 1000.0f;
                    dato = (bufferEntradaSerie[posBuffer++]) | ((bufferEntradaSerie[posBuffer++]) << 8) | ((bufferEntradaSerie[posBuffer++]) << 16) | ((bufferEntradaSerie[posBuffer++]) << 24);
                    aceleracion[0] = dato / 1000.0f;
                    dato = (bufferEntradaSerie[posBuffer++]) | ((bufferEntradaSerie[posBuffer++]) << 8) | ((bufferEntradaSerie[posBuffer++]) << 16) | ((bufferEntradaSerie[posBuffer++]) << 24);
                    aceleracion[1] = dato / 1000.0f;
                    dato = (bufferEntradaSerie[posBuffer++]) | ((bufferEntradaSerie[posBuffer++]) << 8) | ((bufferEntradaSerie[posBuffer++]) << 16) | ((bufferEntradaSerie[posBuffer++]) << 24);
                    aceleracion[2] = dato / 1000.0f;
                    dato = (bufferEntradaSerie[posBuffer++]) | ((bufferEntradaSerie[posBuffer++]) << 8) | ((bufferEntradaSerie[posBuffer++]) << 16) | ((bufferEntradaSerie[posBuffer++]) << 24);
                    angulos[0] = dato / 1000.0f;
                    dato = (bufferEntradaSerie[posBuffer++]) | ((bufferEntradaSerie[posBuffer++]) << 8) | ((bufferEntradaSerie[posBuffer++]) << 16) | ((bufferEntradaSerie[posBuffer++]) << 24);
                    angulos[1] = dato / 1000.0f;
                    dato = (bufferEntradaSerie[posBuffer++]) | ((bufferEntradaSerie[posBuffer++]) << 8) | ((bufferEntradaSerie[posBuffer++]) << 16) | ((bufferEntradaSerie[posBuffer++]) << 24);
                    angulos[2] = dato / 1000.0f;
                    dato = (bufferEntradaSerie[posBuffer++]) | ((bufferEntradaSerie[posBuffer++]) << 8) | ((bufferEntradaSerie[posBuffer++]) << 16) | ((bufferEntradaSerie[posBuffer++]) << 24);
                    angulos[3] = dato / 1000.0f;
                    dato = (bufferEntradaSerie[posBuffer++]) | ((bufferEntradaSerie[posBuffer++]) << 8);
                    imu_temp = dato / 10.0f; ;
                    dato = (bufferEntradaSerie[posBuffer++] & 0xff);
                    tipo_control = dato;

                    OnRecividosDatosIMU(pitch, roll, yaw, velocidad, aceleracion, angulos, tipo_control, imu_temp);
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
                    for (int indiPar = 1; indiPar <= 14; indiPar++)
                    {
                        dato = (bufferEntradaSerie[posBuffer++] & 0xff) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 8) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 16) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 24);
                        parRecibido[indiPar] = (dato) / 1000.0f;
                    }
                    int x, verHight, verLow;

                    x = (bufferEntradaSerie[posBuffer++] & 0xff);
                    verHight = (bufferEntradaSerie[posBuffer++] & 0xff);
                    verLow = (bufferEntradaSerie[posBuffer++] & 0xff);

                    OnRecividosDatosCalibracion(parRecibido, x, verHight, verLow);
                    break;

                case RespuestasBluetooth.valores_manual: //Valores de manual recibidos
                    posBuffer = 3;
                    int[] valores = new int[4];

                    valores[0] = (bufferEntradaSerie[posBuffer++] & 0xff) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 8);
                    valores[1] = (bufferEntradaSerie[posBuffer++] & 0xff) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 8);
                    valores[2] = (bufferEntradaSerie[posBuffer++] & 0xff) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 8);
                    valores[3] = (bufferEntradaSerie[posBuffer++] & 0xff) | ((bufferEntradaSerie[posBuffer++] & 0xff) << 8);


                    OnRecividosDatosManual(valores);
                    break;
                default:
                    LogearMSG(String.Format(string.Format("{0} {1}", "Error comando no reconocido recibido", comandoChar)));
                    break;

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

        public void Peticion_calibracion() { 
            Enviar_comando_Bluetooth(ComandosBluetooth.peticion_calibracion, 1);
        }

        public void CambiarModo(E_modo modo)
        {
            Enviar_comando_Bluetooth(ComandosBluetooth.cambiar_modo, (int)modo);
        }
    }
}
