

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
        public float ConsignaMarcha, ConsignaRPMActual, AnguloRuedaDerecha, AnguloRuedaIzquierda, ESC_VoltajeEntrada, ESC_rpmActual, ESC_avgMotorCurrent, ESC_avgInputCurrent, ESC_Dutycycle;
        public E_modo modo_motor_actual;
        public E_tipo_control tipo_control_actual;
        public E_regulacion_direccion tipo_regulacion_direccion;
        public E_regulacion_potencia tipo_regulacion_potencia;

        public float bar_temperatura, bar_presion;
        public int tipo_control;
        public int[] gyro;
        public int[] aceleracion;
        public float[] angulos;
        public double tiempoRespuesta;
        public DateTime initPing;

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
            ping=1,
            auto_parametros,
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
            pong=1,
            auto_parametros ,
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
            e_modo_manual = 0,
            e_modo_sistema_salida = 1,
            e_modo_semi_auto = 2,
            e_modo_full_auto = 3
        };

        public enum E_tipo_control
        {
            e_control_RC = 0,
            e_control_BT = 1
        };

        public enum E_regulacion_potencia
        {
            e_reg_pot_directa = 0,
            e_reg_pot_incremental,
            e_reg_pot_max,
            e_reg_pot_agresiva,
            e_reg_pot_conservadora
        };

        public enum E_regulacion_direccion
        {
            e_reg_dir_directa = 0,
            e_reg_dir_Ackermann = 1
        };

        public Telemetria()
        {
            punteroEntrada = 0;
            conexionCorrecta = false;
            bufferEntradaSerie = new byte[80];
            ultimposicion = new float[4];
            gyro = new int[3];
            aceleracion = new int[3];
            angulos = new float[4];
            this.serialPortBluetooth = new SerialPort();
            this.serialPortBluetooth.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.SerialPortBluetooth_DataReceived);
            this.serialPortBluetooth.ErrorReceived += new System.IO.Ports.SerialErrorReceivedEventHandler(this.SerialPortBluetooth_Error);
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
              
                LogearMSG(String.Format("establecida connexion serie , puerto {1} a {0} kbs\n", serialPortBluetooth.BaudRate, serialPortBluetooth.PortName));
                OnCambioConexionSerie(serialPortBluetooth.IsOpen ? 1 : 0, 0);
                return true;
            }
            catch (Exception ex)
            {
                if (serialPortBluetooth != null)
                {
                    OnCambioConexionSerie(serialPortBluetooth.IsOpen ? 1 : 0, 0);
                } else
                {
                    OnCambioConexionSerie(0, 0);
                }
                    LogearMSG(String.Format("{0} ||Exception:  {1}", "Error en la conexion", ex));
                return false;
            }
        }

        public Boolean Cerrar_puerto_serie()
        {
            try
            {
                if (serialPortBluetooth != null)
                {
                    if (serialPortBluetooth.IsOpen)
                    {
                        serialPortBluetooth.Close();
                    }
                }

                LogearMSG(String.Format("Cerrado puerto {1} a {0} kbs\n", serialPortBluetooth.BaudRate, serialPortBluetooth.PortName));
                OnCambioConexionSerie(serialPortBluetooth.IsOpen ? 1 : 0, 0);
                return true;
            }
            catch (Exception ex)
            {
                if (serialPortBluetooth != null)
                {
                    OnCambioConexionSerie(serialPortBluetooth.IsOpen ? 1 : 0, 0);
                }
                else
                {
                    OnCambioConexionSerie(0, 0);
                }
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
                Enviar_comando_Bluetooth(ComandosBluetooth.PedirEstado,0);
            }
        }

        public void ping()
        {
            initPing = DateTime.Now;
            if (CheckSerialConection())
            {
                Enviar_comando_Bluetooth(ComandosBluetooth.ping,0);
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
        private void OnRecividosDatosCoche(float p_ConsignaMarcha, float p_ConsignaRPMActual, float p_AnguloRuedaDerecha, float p_AnguloRuedaIzquierda, float p_ESC_VoltajeEntrada, float p_ESC_rpmActual, float p_ESC_avgMotorCurrent, float p_ESC_avgInputCurrent, float p_ESC_Dutycycle,E_modo p_modo_motor_actual,E_tipo_control p_tipo_control_actual, E_regulacion_direccion p_tipo_regulacion_direccion, E_regulacion_potencia p_tipo_regulacion_potencia)
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
                Modo_motor_actual=p_modo_motor_actual,
                Tipo_control_actual = p_tipo_control_actual,
                Tipo_regulacion_direccion = p_tipo_regulacion_direccion,
                Tipo_regulacion_potencia= p_tipo_regulacion_potencia,
                Time =DateTime.UtcNow
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
            public E_modo Modo_motor_actual { get; set; }
            public E_tipo_control Tipo_control_actual { get; set; }
            public E_regulacion_direccion Tipo_regulacion_direccion { get; set; }
            public E_regulacion_potencia Tipo_regulacion_potencia { get; set; }
        }
        /******************************************************************/
        private void OnRecividosDatosIMU(float p_pitch, float p_roll, float p_yaw, int[] p_gyro, int[] p_aceleracion, float[] p_angulos, int p_tipo_control, float p_imu_temp, float p_bar_temp, float p_bar_presion)
        {
            DatosIMUEventArgs parametros = new DatosIMUEventArgs()
            {
                Time = DateTime.UtcNow,
                Pitch = p_pitch,
                Roll=p_roll,
                Yaw=p_yaw,
                Gyro= p_gyro,
                Aceleracion=p_aceleracion,
                Angulos=p_angulos,
                Tipo_control=p_tipo_control,
                Imu_temp=p_imu_temp,
                bar_presion=p_bar_presion,
                bar_temp=p_bar_temp
            };

            RecividosDatosIMUEvent?.Invoke(this, parametros);
        }

        public event EventHandler<DatosIMUEventArgs> RecividosDatosIMUEvent;

        public class DatosIMUEventArgs : EventArgs
        {
            public float Pitch { get; set; }
            public float Roll { get; set; }
            public float Yaw { get; set; }
            public int[] Gyro { get; set; }
            public int[] Aceleracion { get; set; }
            public float[] Angulos { get; set; }
            public int Tipo_control { get; set; }
            public float Imu_temp { get; set; }
            public float bar_temp { get; set; }
            public float bar_presion { get; set; }
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
        private void OnRecividosDatosManual(float[] p_parRecibido)
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
            public float[] ParRecibido { get; set; }
        }
        /******************************************************************/
        private void OnRecividosEstado(E_modo p_modo_motor_actual, E_tipo_control p_tipo_control_actual, E_regulacion_direccion p_tipo_regulacion_direccion, E_regulacion_potencia p_tipo_regulacion_potencia)
        {
            DatosEstadoEventArgs parametros = new DatosEstadoEventArgs()
            {
                Modo_motor_actual=p_modo_motor_actual,
                Tipo_control_actual=p_tipo_control_actual,
                Tipo_regulacion_direccion=p_tipo_regulacion_direccion,
                Tipo_regulacion_potencia= p_tipo_regulacion_potencia
            };

            RecividosEstado?.Invoke(this, parametros);
        }

        public event EventHandler<DatosEstadoEventArgs> RecividosEstado;

        public class DatosEstadoEventArgs : EventArgs
        {
            public DateTime Time { get; set; }
            public E_modo Modo_motor_actual { get; set; }
            public E_tipo_control Tipo_control_actual { get; set; }
            public E_regulacion_direccion Tipo_regulacion_direccion { get; set; }
            public E_regulacion_potencia Tipo_regulacion_potencia { get; set; }
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
            /*
#define LENGH_RESPUESTAS_BLUETOOTH_DATOS_COCHE 26
		#define LENGH_RESPUESTAS_BLUETOOTH_DATOS_IMU 65
		#define LENGH_RESPUESTAS_BLUETOOTH_VALORES_CALIBRA 63
		#define LENGH_RESPUESTAS_BLUETOOTH_VALORESMANUAL 13
		#define LENGH_RESPUESTAS_BLUETOOTH_ESTADO 9*/
            if (bytesRecibidos <= 3) return (50 - 1); // aun no se ha recibido el ID de comando(NOTA al recibirlo en el array se suma por tanto el indice ha de ser 4 o mas para q este leido), suponer longitud maxima


            RespuestasBluetooth comando = (RespuestasBluetooth)bufferEntradaSerie[2];


            //todos los comandos basicos tienen un solo byte de datos
            /*
            if (comando == RespuestasBluetooth.msg || comando == RespuestasBluetooth.auto_imu || comando == RespuestasBluetooth.auto_parametros || comando == RespuestasBluetooth.cambiar_modo || comando == RespuestasBluetooth.cambio_ok || comando == RespuestasBluetooth.control_remoto) // recepcion estado
            {
                return 6;
            }
            */
            if (comando == RespuestasBluetooth.datos_coche) //recepcion datos
            {
                return 26;
            }

            if (comando == RespuestasBluetooth.datosn_imu) //recepcion msg
            {
                return 65;
            }

            if (comando == RespuestasBluetooth.valores_calibracion) //recepcion calibracion
            {
                return 63;
            }

            if (comando == RespuestasBluetooth.valores_manual) //recepcion msg
            {
                return 13;
            }


            if (comando == RespuestasBluetooth.estado) //recepcion msg
            {
                return 9;
            }

            //todos los comandos basicos tienen un solo byte de datos
            return 6;
        }

      
        private void ProcesarComandoSerie()
        {
            int dato, nuevoEstado;
            short dato16;


            int posBuffer = 3;

            RespuestasBluetooth comandoChar = (RespuestasBluetooth)bufferEntradaSerie[2];
            switch (comandoChar)
            {
                case RespuestasBluetooth.pong:
                    tiempoRespuesta = (DateTime.Now - initPing).TotalMilliseconds;
                    break;
                case RespuestasBluetooth.estado: //recepcion nuevo estado
                    modo_motor_actual = (E_modo)bufferEntradaSerie[3];
                    tipo_control_actual = (E_tipo_control)bufferEntradaSerie[4];
                    tipo_regulacion_direccion = (E_regulacion_direccion)bufferEntradaSerie[5];
                    tipo_regulacion_potencia = (E_regulacion_potencia)bufferEntradaSerie[6];
                    OnRecividosEstado(modo_motor_actual, tipo_control_actual, tipo_regulacion_direccion, tipo_regulacion_potencia);

        LogearMSG(String.Format(string.Format("Recibido nuevo estado del sistema: motor: {0} control: {1}  direccion: {2} potencia {3}", modo_motor_actual, tipo_control_actual,tipo_regulacion_direccion,tipo_regulacion_potencia)));
                    break;

                case RespuestasBluetooth.datos_coche: //recepcion medida
                    posBuffer = 3;
              

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
                    modo_motor_actual = (E_modo) dato;
                    dato = (bufferEntradaSerie[posBuffer++] & 0xff);
                    tipo_control_actual =(E_tipo_control) dato;
                    dato = (bufferEntradaSerie[posBuffer++] & 0xff);
                    tipo_regulacion_direccion =(E_regulacion_direccion) dato;
                    dato = (bufferEntradaSerie[posBuffer++] & 0xff);
                    tipo_regulacion_potencia = (E_regulacion_potencia) dato;


                    OnRecividosDatosCoche(ConsignaMarcha, ConsignaRPMActual, AnguloRuedaDerecha, AnguloRuedaIzquierda, ESC_VoltajeEntrada, ESC_rpmActual, ESC_avgInputCurrent, ESC_avgInputCurrent, ESC_Dutycycle, modo_motor_actual, tipo_control_actual, tipo_regulacion_direccion, tipo_regulacion_potencia);
                    // LogearMSG(String.Format(string.Format(@"RPM: {0} RuedaDer {1} RuedaIzq {2}  Marcha {3} ESC_Voltaje {4} ESC_rpm {5} ESC_avgMotorCurrent {6} ESC_avgInputCurrent {7} ESC_Dutycycle {8}",modo_motor_actual = dato;
                   
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
                    gyro[0] = dato;
                    dato = (bufferEntradaSerie[posBuffer++]) | ((bufferEntradaSerie[posBuffer++]) << 8) | ((bufferEntradaSerie[posBuffer++]) << 16) | ((bufferEntradaSerie[posBuffer++]) << 24);
                    gyro[1] = dato;
                    dato = (bufferEntradaSerie[posBuffer++]) | ((bufferEntradaSerie[posBuffer++]) << 8) | ((bufferEntradaSerie[posBuffer++]) << 16) | ((bufferEntradaSerie[posBuffer++]) << 24);
                    gyro[2] = dato;
                    dato = (bufferEntradaSerie[posBuffer++]) | ((bufferEntradaSerie[posBuffer++]) << 8) | ((bufferEntradaSerie[posBuffer++]) << 16) | ((bufferEntradaSerie[posBuffer++]) << 24);
                    aceleracion[0] = dato;
                    dato = (bufferEntradaSerie[posBuffer++]) | ((bufferEntradaSerie[posBuffer++]) << 8) | ((bufferEntradaSerie[posBuffer++]) << 16) | ((bufferEntradaSerie[posBuffer++]) << 24);
                    aceleracion[1] = dato;
                    dato = (bufferEntradaSerie[posBuffer++]) | ((bufferEntradaSerie[posBuffer++]) << 8) | ((bufferEntradaSerie[posBuffer++]) << 16) | ((bufferEntradaSerie[posBuffer++]) << 24);
                    aceleracion[2] = dato;
                    dato = (bufferEntradaSerie[posBuffer++]) | ((bufferEntradaSerie[posBuffer++]) << 8) | ((bufferEntradaSerie[posBuffer++]) << 16) | ((bufferEntradaSerie[posBuffer++]) << 24);
                    angulos[0] = dato / 10000.0f;
                    dato = (bufferEntradaSerie[posBuffer++]) | ((bufferEntradaSerie[posBuffer++]) << 8) | ((bufferEntradaSerie[posBuffer++]) << 16) | ((bufferEntradaSerie[posBuffer++]) << 24);
                    angulos[1] = dato / 10000.0f;
                    dato = (bufferEntradaSerie[posBuffer++]) | ((bufferEntradaSerie[posBuffer++]) << 8) | ((bufferEntradaSerie[posBuffer++]) << 16) | ((bufferEntradaSerie[posBuffer++]) << 24);
                    angulos[2] = dato / 10000.0f;
                    dato = (bufferEntradaSerie[posBuffer++]) | ((bufferEntradaSerie[posBuffer++]) << 8) | ((bufferEntradaSerie[posBuffer++]) << 16) | ((bufferEntradaSerie[posBuffer++]) << 24);
                    angulos[3] = dato / 10000.0f;
                    dato = (bufferEntradaSerie[posBuffer++]) | ((bufferEntradaSerie[posBuffer++]) << 8) | ((bufferEntradaSerie[posBuffer++]) << 16) | ((bufferEntradaSerie[posBuffer++]) << 24);
                    bar_temperatura = dato / 1000.0f;
                    dato = (bufferEntradaSerie[posBuffer++]) | ((bufferEntradaSerie[posBuffer++]) << 8) | ((bufferEntradaSerie[posBuffer++]) << 16) | ((bufferEntradaSerie[posBuffer++]) << 24);
                    bar_presion = dato / 1000.0f;
                    

                    OnRecividosDatosIMU(pitch, roll, yaw, gyro, aceleracion, angulos, tipo_control, imu_temp,bar_temperatura,bar_presion);
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
                        LogearMSG(String.Format(string.Format("{0} {1}", "-", comandoChar)));
                    }
                    else if (dato == 5)
                    {
                        LogearMSG(String.Format(string.Format("{0} {1}", "-", comandoChar)));
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
                    else if (dato == 11)
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
                    float[] valores = new float[4];


                    dato = (short)((bufferEntradaSerie[posBuffer++]) | ((bufferEntradaSerie[posBuffer++]) << 8));
                    valores[0] = (dato) / 10.0f;
                    dato = (short)((bufferEntradaSerie[posBuffer++]) | ((bufferEntradaSerie[posBuffer++]) << 8));
                    valores[1] = (dato) / 10.0f;
                    dato = (short)((bufferEntradaSerie[posBuffer++]) | ((bufferEntradaSerie[posBuffer++]) << 8));
                    valores[2] = (dato) / 10.0f;
                    dato = (short)((bufferEntradaSerie[posBuffer++]) | ((bufferEntradaSerie[posBuffer++]) << 8));
                    valores[3] = (dato) / 10.0f;


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




        private void SerialPortBluetooth_Error(object sender, SerialErrorReceivedEventArgs e)
        {
            if (serialPortBluetooth == null) return;
            OnCambioConexionSerie(serialPortBluetooth.IsOpen ? 1 : 0, 0);
            LogearMSG(String.Format("{0} Error Puerto Serie***********************\n {1}  {2}", e.EventType, "||Exception:  ", e.ToString()));
        }
        public void Peticion_calibracion() { 
            Enviar_comando_Bluetooth(ComandosBluetooth.peticion_calibracion, 1);
        }
        public void Peticion_Valoresmanuales()
        {
            Enviar_comando_Bluetooth(ComandosBluetooth.pedir_valoresmanual, 1);
        }

        public void CambiarModo(E_modo modo)
        {
            Enviar_comando_Bluetooth(ComandosBluetooth.cambiar_modo, (int)modo);
        }

        public event EventHandler<CambioEstadoConexionArgs> CambioEstadoConexion;

        public class CambioEstadoConexionArgs : EventArgs
        {
            public int EstadoAnterior { get; set; }
            public int EstadoActual { get; set; }
        }

        private void OnCambioConexionSerie(int p_estadoActual, int p_estadoAnterior)
        { 
            CambioEstadoConexionArgs parametros = new CambioEstadoConexionArgs()
            {
                EstadoActual= p_estadoActual,
                EstadoAnterior= p_estadoAnterior
            };

            CambioEstadoConexion?.Invoke(this, parametros);
        }
    }
}
