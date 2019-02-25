#include "Telemetria.h"

Telemetria::Telemetria(void)
{
  envio_parametros_coche = false;
  envio_parametros_imu = false;
  //TODO: BORRAR
//test para pruebas
  angulos[0]=1;
  angulos[1]=2;
  angulos[2]=3;
  angulos[3]=4;
		pitch=5;
    roll=6;
    yaw=7;
		rpmActual=8;
		ConsignaRPMActual=9;
		ConsignaDireccionActual=10;
		AnguloRuedaDerecha=11;
		AnguloRuedaIzquierda=12;
		velocidad[0]=13;
    velocidad[1]=14;
    velocidad[2]=15;
		aceleracion[0]=16;
    aceleracion[1]=17;
    aceleracion[2]=18;
    tipoControl=19;
    tipoControl=20;
		ESC_Dutycycle=21;
    ESC_avgInputCurrent=22;
    ESC_avgMotorCurrent=23;
    ESC_rpmActual=24;
    ESC_VoltajeEntrada=25;

}

void Telemetria::setSerialPort(Stream *port)
{
  serialPort = port;
}

void Telemetria::setImu(MPU9250_DMP *imu_obj)
{
  imu = imu_obj;
}

void Telemetria::setCar(Rc_car *p_rc_car)
{
  rc_car = p_rc_car;
}

void Telemetria::setConfig(Configuracion *p_rc_Configuracion)
{
  rc_Configuracion = p_rc_Configuracion;
}

void Telemetria::NuevosValoresImu()
{

  if (tel_Debug)
  {
    float q0 = imu->calcQuat(imu->qw);
    float q1 = imu->calcQuat(imu->qx);
    float q2 = imu->calcQuat(imu->qy);
    float q3 = imu->calcQuat(imu->qz);

    angulos[0] = q0;
    angulos[1] = q1;
    angulos[2] = q2;
    angulos[3] = q3;

    pitch = q3;
    roll = q3;
    yaw = q3;
    velocidad[0] = q0;
    velocidad[1] = q1;
    velocidad[2] = q2;

    aceleracion[0] = q0;
    aceleracion[1] = q1;
    aceleracion[2] = q2;

    serialPort->println("Q: " + String(q0, 4) + ", " +
                        String(q1, 4) + ", " + String(q2, 4) +
                        ", " + String(q3, 4));
    serialPort->println("R/P/Y: " + String(imu->roll) + ", " + String(imu->pitch) + ", " + String(imu->yaw));
    serialPort->println("Time: " + String(imu->time) + " ms");
    serialPort->println();

#ifdef OUTPUT_TEAPOT
    float q0 *= 16384.0f;
    float q1 *= 16384.0f;
    float q2 *= 16384.0f;
    float q3 *= 16384.0f;

    uint8_t teapotPacket[14] = {'$', 0x02, 0, 0, 0, 0, 0, 0, 0, 0, 0x00, 0x00, '\r', '\n'};
    // display quaternion values in InvenSense Teapot demo format:
    teapotPacket[2] = (uint8_t)(((uint32_t)(q0)&0xFF00) >> 8);
    teapotPacket[3] = (uint8_t)((uint32_t)(q0)&0xFF);
    teapotPacket[4] = (uint8_t)(((uint32_t)(q1)&0xFF00) >> 8);
    teapotPacket[5] = (uint8_t)((uint32_t)(q1)&0xFF);
    teapotPacket[6] = (uint8_t)(((uint32_t)(q2)&0xFF00) >> 8);
    teapotPacket[7] = (uint8_t)((uint32_t)(q2)&0xFF);
    teapotPacket[8] = (uint8_t)(((uint32_t)(q3)&0xFF00) >> 8);
    teapotPacket[9] = (uint8_t)((uint32_t)(q3)&0xFF);
    if (OUTPUT_TEAPOT)
      serialPort->write(teapotPacket, 14);
    teapotPacket[11]++; // packetCount, loops at 0xFF on purpose
#endif
  }
}

void Telemetria::setDebug(bool estado)
{
  tel_Debug = estado;
}

// ================================================================
// ===             Comunicaciones Bluetooth                     ===
// ================================================================

//Envia el valor de estado del coche por bluetooth
void Telemetria::enviarDatosCoche()
{
  uint8_t datosEnvioSerial[23] = {'$', 0x02, RESPUESTAS_BLUETOOTH_DATOS_COCHE, 0,0, 0,0 ,0,0 ,0,0, 0,0, 0,0, 0,0, 0,0 , 0,  0,'\r', '\n'};

  int posicion = 3;
  int16_t tmpshort = 0;

  tmpshort = round(ConsignaRPMActual / 10); //16bit (64bit en C# PC). /10 en saltos de 10rpms es resolucion suficiente ya que el error de calculo es mayor
  datosEnvioSerial[posicion++] = (tmpshort & 0xff);
  datosEnvioSerial[posicion++] = ((tmpshort & 0xff00) >> 8);

  tmpshort = round(ConsignaDireccionActual * 100); //16bit (64bit en C# PC). X100 para enviar 2 decimales
  datosEnvioSerial[posicion++] = (tmpshort & 0xff);
  datosEnvioSerial[posicion++] = ((tmpshort & 0xff00) >> 8);

  tmpshort = round(AnguloRuedaDerecha ); //16bit (64bit en C# PC). X100 para enviar 2 decimales
  datosEnvioSerial[posicion++] = (tmpshort & 0xff);
  datosEnvioSerial[posicion++] = ((tmpshort & 0xff00) >> 8);

  tmpshort = round(AnguloRuedaIzquierda); //16bit (64bit en C# PC). X100 para enviar 2 decimales
  datosEnvioSerial[posicion++] = (tmpshort & 0xff);
  datosEnvioSerial[posicion++] = ((tmpshort & 0xff00) >> 8);

  tmpshort = round(ESC_VoltajeEntrada * 10); //16bit (64bit en C# PC). X10 para enviar 1 decimales
  datosEnvioSerial[posicion++] = (tmpshort & 0xff);
  datosEnvioSerial[posicion++] = ((tmpshort & 0xff00) >> 8);

  tmpshort = round(ESC_rpmActual / 10); //16bit (64bit en C# PC). /10 en saltos de 10rpms es resolucion suficiente ya que el error de calculo es mayor
  datosEnvioSerial[posicion++] = (tmpshort & 0xff);
  datosEnvioSerial[posicion++] = ((tmpshort & 0xff00) >> 8);

  tmpshort = round( ESC_avgMotorCurrent * 100); //16bit (64bit en C# PC). X100 para enviar 2 decimales
  datosEnvioSerial[posicion++] = (tmpshort & 0xff);
  datosEnvioSerial[posicion++] = ((tmpshort & 0xff00) >> 8);

  tmpshort = round( ESC_avgInputCurrent * 100); //16bit (64bit en C# PC). X100 para enviar 2 decimales
  datosEnvioSerial[posicion++] = (tmpshort & 0xff);
  datosEnvioSerial[posicion++] = ((tmpshort & 0xff00) >> 8);

  tmpshort = round(ESC_Dutycycle *100); 
  datosEnvioSerial[posicion++] = ESC_Dutycycle;


  datosEnvioSerial[posicion++] = tipoControl;

  serialPort->write(datosEnvioSerial, 23);
}

void Telemetria::enviarDatosImu()
{
  uint8_t datosEnvioSerial[58] = {'$', 0x02, RESPUESTAS_BLUETOOTH_DATOS_IMU, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, '\r', '\n'};

  int posicion = 3;

  int32_t tmpLong = round(pitch * 1000); //32bit (64bit en C# PC). X1000 para enviar 3 decimales, se podria enviar directamente en coma flotante
  datosEnvioSerial[posicion++] = (tmpLong & 0xff);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff00) >> 8);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff0000) >> 16);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff000000) >> 24);

  tmpLong = round(roll * 1000); //32bit (64bit en C# PC). X1000 para enviar 3 decimales, se podria enviar directamente en coma flotante
  datosEnvioSerial[posicion++] = (tmpLong & 0xff);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff00) >> 8);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff0000) >> 16);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff000000) >> 24);

  tmpLong = round(yaw * 1000); //32bit (64bit en C# PC). X1000 para enviar 3 decimales, se podria enviar directamente en coma flotante
  datosEnvioSerial[posicion++] = (tmpLong & 0xff);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff00) >> 8);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff0000) >> 16);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff000000) >> 24);

  tmpLong = round(velocidad[0] * 1000); //16bit (64bit en C# PC). /10 en saltos de 10rpms es resolucion suficiente ya que el error de calculo es mayor
  datosEnvioSerial[posicion++] = (tmpLong & 0xff);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff00) >> 8);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff0000) >> 16);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff000000) >> 24);

  tmpLong = round(velocidad[1] * 1000); //16bit (64bit en C# PC). /10 en saltos de 10rpms es resolucion suficiente ya que el error de calculo es mayor
  datosEnvioSerial[posicion++] = (tmpLong & 0xff);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff00) >> 8);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff0000) >> 16);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff000000) >> 24);

  tmpLong = round(velocidad[2] * 1000); //16bit (64bit en C# PC). X100 para enviar 2 decimales
  datosEnvioSerial[posicion++] = (tmpLong & 0xff);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff00) >> 8);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff0000) >> 16);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff000000) >> 24);

  tmpLong = round(aceleracion[0] * 1000); //16bit (64bit en C# PC). /10 en saltos de 10rpms es resolucion suficiente ya que el error de calculo es mayor
  datosEnvioSerial[posicion++] = (tmpLong & 0xff);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff00) >> 8);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff0000) >> 16);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff000000) >> 24);

  tmpLong = round(aceleracion[1] * 1000); //16bit (64bit en C# PC). /10 en saltos de 10rpms es resolucion suficiente ya que el error de calculo es mayor
  datosEnvioSerial[posicion++] = (tmpLong & 0xff);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff00) >> 8);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff0000) >> 16);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff000000) >> 24);

  tmpLong = round(aceleracion[2] * 1000); //16bit (64bit en C# PC). X100 para enviar 2 decimales
  datosEnvioSerial[posicion++] = (tmpLong & 0xff);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff00) >> 8);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff0000) >> 16);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff000000) >> 24);

  tmpLong = round(angulos[0] * 1000); //16bit (64bit en C# PC). X100 para enviar 2 decimales
  datosEnvioSerial[posicion++] = (tmpLong & 0xff);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff00) >> 8);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff0000) >> 16);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff000000) >> 24);

  tmpLong = round(angulos[1] * 1000); //16bit (64bit en C# PC). X100 para enviar 2 decimales
  datosEnvioSerial[posicion++] = (tmpLong & 0xff);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff00) >> 8);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff0000) >> 16);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff000000) >> 24);

  tmpLong = round(angulos[2] * 1000); //16bit (64bit en C# PC). X100 para enviar 2 decimales
  datosEnvioSerial[posicion++] = (tmpLong & 0xff);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff00) >> 8);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff0000) >> 16);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff000000) >> 24);

  tmpLong = round(angulos[3] * 1000); //16bit (64bit en C# PC). X100 para enviar 2 decimales
  datosEnvioSerial[posicion++] = (tmpLong & 0xff);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff00) >> 8);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff0000) >> 16);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff000000) >> 24);

  datosEnvioSerial[posicion++] = tipoControl;

  serialPort->write(datosEnvioSerial, 58);
}

//Envia los valores de calibracion por bluetooth
void Telemetria::enviarConfiguracionActual()
{

  uint8_t datosEnvioSerial[64] = {'$', 0x02, RESPUESTAS_BLUETOOTH_VALORES_CALIBRA, 0, 0, 0, 0,  0, 0, 0, 0,  0, 0, 0, 0,  0, 0, 0, 0,  0, 0, 0, 0,  0, 0, 0, 0,  0, 0, 0, 0,  0, 0, 0, 0,  0, 0, 0, 0,  0, 0, 0, 0,  0, 0, 0, 0,  0, 0, 0, 0,  0, 0, 0, 0,  0, 0, 0, 0,  0,  0,  0, '\r', '\n'};
  int32_t tmpLong = round(rc_Configuracion->configActual.C1 * 1000.0);
  int posicion = 3;
  datosEnvioSerial[posicion++] = (tmpLong & 0xff);
  datosEnvioSerial[posicion++] = (tmpLong & 0xff00) >> 8;
  datosEnvioSerial[posicion++] = (tmpLong & 0xff0000) >> 16;
  datosEnvioSerial[posicion++] = (tmpLong & 0xff000000) >> 24;

  tmpLong = round(rc_Configuracion->configActual.C2 * 1000.0);
  datosEnvioSerial[posicion++] = (tmpLong & 0xff);
  datosEnvioSerial[posicion++] = (tmpLong & 0xff00) >> 8;
  datosEnvioSerial[posicion++] = (tmpLong & 0xff0000) >> 16;
  datosEnvioSerial[posicion++] = (tmpLong & 0xff000000) >> 24;

  tmpLong = round(rc_Configuracion->configActual.C3 * 1000.0);
  datosEnvioSerial[posicion++] = (tmpLong & 0xff);
  datosEnvioSerial[posicion++] = (tmpLong & 0xff00) >> 8;
  datosEnvioSerial[posicion++] = (tmpLong & 0xff0000) >> 16;
  datosEnvioSerial[posicion++] = (tmpLong & 0xff000000) >> 24;

  tmpLong = round(rc_Configuracion->configActual.C4 * 1000.0);
  datosEnvioSerial[posicion++] = (tmpLong & 0xff);
  datosEnvioSerial[posicion++] = (tmpLong & 0xff00) >> 8;
  datosEnvioSerial[posicion++] = (tmpLong & 0xff0000) >> 16;
  datosEnvioSerial[posicion++] = (tmpLong & 0xff000000) >> 24;

  tmpLong = round(rc_Configuracion->configActual.C5 * 1000.0);
  datosEnvioSerial[posicion++] = (tmpLong & 0xff);
  datosEnvioSerial[posicion++] = (tmpLong & 0xff00) >> 8;
  datosEnvioSerial[posicion++] = (tmpLong & 0xff0000) >> 16;
  datosEnvioSerial[posicion++] = (tmpLong & 0xff000000) >> 24;

  tmpLong = round(rc_Configuracion->configActual.C6 * 1000.0);
  datosEnvioSerial[posicion++] = (tmpLong & 0xff);
  datosEnvioSerial[posicion++] = (tmpLong & 0xff00) >> 8;
  datosEnvioSerial[posicion++] = (tmpLong & 0xff0000) >> 16;
  datosEnvioSerial[posicion++] = (tmpLong & 0xff000000) >> 24;

  tmpLong = round(rc_Configuracion->configActual.C7 * 1000.0);
  datosEnvioSerial[posicion++] = (tmpLong & 0xff);
  datosEnvioSerial[posicion++] = (tmpLong & 0xff00) >> 8;
  datosEnvioSerial[posicion++] = (tmpLong & 0xff0000) >> 16;
  datosEnvioSerial[posicion++] = (tmpLong & 0xff000000) >> 24;

  tmpLong = round(rc_Configuracion->configActual.C8 * 1000.0);
  datosEnvioSerial[posicion++] = (tmpLong & 0xff);
  datosEnvioSerial[posicion++] = (tmpLong & 0xff00) >> 8;
  datosEnvioSerial[posicion++] = (tmpLong & 0xff0000) >> 16;
  datosEnvioSerial[posicion++] = (tmpLong & 0xff000000) >> 24;

  tmpLong = round(rc_Configuracion->configActual.C9 * 1000.0);
  datosEnvioSerial[posicion++] = (tmpLong & 0xff);
  datosEnvioSerial[posicion++] = (tmpLong & 0xff00) >> 8;
  datosEnvioSerial[posicion++] = (tmpLong & 0xff0000) >> 16;
  datosEnvioSerial[posicion++] = (tmpLong & 0xff000000) >> 24;

  tmpLong = round(rc_Configuracion->configActual.C10 * 1000.0);
  datosEnvioSerial[posicion++] = (tmpLong & 0xff);
  datosEnvioSerial[posicion++] = (tmpLong & 0xff00) >> 8;
  datosEnvioSerial[posicion++] = (tmpLong & 0xff0000) >> 16;
  datosEnvioSerial[posicion++] = (tmpLong & 0xff000000) >> 24;

  tmpLong = round(rc_Configuracion->configActual.C11 * 1000.0);
  datosEnvioSerial[posicion++] = (tmpLong & 0xff);
  datosEnvioSerial[posicion++] = (tmpLong & 0xff00) >> 8;
  datosEnvioSerial[posicion++] = (tmpLong & 0xff0000) >> 16;
  datosEnvioSerial[posicion++] = (tmpLong & 0xff000000) >> 24;

  tmpLong = round(rc_Configuracion->configActual.C12 * 1000.0);
  datosEnvioSerial[posicion++] = (tmpLong & 0xff);
  datosEnvioSerial[posicion++] = (tmpLong & 0xff00) >> 8;
  datosEnvioSerial[posicion++] = (tmpLong & 0xff0000) >> 16;
  datosEnvioSerial[posicion++] = (tmpLong & 0xff000000) >> 24;

  tmpLong = round(rc_Configuracion->configActual.C13 * 1000.0);
  datosEnvioSerial[posicion++] = (tmpLong & 0xff);
  datosEnvioSerial[posicion++] = (tmpLong & 0xff00) >> 8;
  datosEnvioSerial[posicion++] = (tmpLong & 0xff0000) >> 16;
  datosEnvioSerial[posicion++] = (tmpLong & 0xff000000) >> 24;

  tmpLong = round(rc_Configuracion->configActual.C14 * 1000.0);
  datosEnvioSerial[posicion++] = (tmpLong & 0xff);
  datosEnvioSerial[posicion++] = (tmpLong & 0xff00) >> 8;
  datosEnvioSerial[posicion++] = (tmpLong & 0xff0000) >> 16;
  datosEnvioSerial[posicion++] = (tmpLong & 0xff000000) >> 24;

  datosEnvioSerial[posicion++] = tipoControl;

  datosEnvioSerial[posicion++] = rc_Configuracion->VERSION_H;
  datosEnvioSerial[posicion++] = rc_Configuracion->VERSION_L;

  serialPort->write(datosEnvioSerial, 64);
}

//Evento/Interrupcion de recepcion por bluetooth
void Telemetria::serialEvent2()
{

  if (!serialPort->available())
    return;

  int buclerino = 0; // maximo de caracteres a procesar 100 antes de continuar con el resto de operaciones

  if (!nuevoComando)
  { //ignorar si no se proceso el anterior
    while (serialPort->available() && !nuevoComando && buclerino < 100)
    {
      buclerino++;

      uint8_t inByte = (uint8_t)serialPort->read();
      //Serial.println(inByte);
      if (!conexionCorrecta && inByte != '$')
      {
        //Compatibilidad con app por defecto android
        if (inByte == 'U')
          modo_motor_actual = 0;
        if (inByte == 'D')
          modo_motor_actual = 1;
        if (inByte == 'a')
          tipoControl = 0;
        if (inByte == 'e')
          tipoControl = 1;
        return;
      }
      conexionCorrecta = true;

      if ((bytesRecibidos == 1 && inByte != 2) || combrobarFinCadena())
      {
        bytesRecibidos = 0;
        conexionCorrecta = false;
        enviarMensaje(2); // error fin de cadena no esperado
        return;
      }

      if (bytesRecibidos > 0 || inByte == '$')
      {

        datosRecibidosSerial[bytesRecibidos] = inByte;
        bytesRecibidos++;

        if (bytesRecibidos == reconocerComando())
        {
          //bytesRecibidos = 0; //movido a momento lectura comando
          nuevoComando = true;
          procesaComando();
        }
      }
    }
  }
}

void Telemetria::autoTelemetria()
{
 if(envio_parametros_coche) enviarDatosCoche();
	if(envio_parametros_imu) enviarDatosImu();
		
  
}

//Comprueba si el mensaje a terminado
bool Telemetria::combrobarFinCadena()
{
  int longitudMensaje = reconocerComando();

  if (bytesRecibidos > longitudMensaje)
    return true; //error longitud excesiba

  if ((bytesRecibidos == (longitudMensaje - 1)) && (datosRecibidosSerial[longitudMensaje - 2] != '\r'))
    return true; //error terminador 1
  if ((bytesRecibidos == longitudMensaje) && (datosRecibidosSerial[longitudMensaje - 1] != '\n'))
    return true; //error terminador 2

  return false;
}

//Retorna la longitud del comando que se esta recibiendo actualmente basado en el codigo de operacion
int Telemetria::reconocerComando()
{

  if (bytesRecibidos <= 3)
    return (LONGITUD_BUFFER_BLUETOOTH - 1); // aun no se ha recibido el ID de comando(NOTA al recibirlo en el array se suma por tanto el indice ha de ser 4 o mas para q este leido), suponer longitud maxima

  uint8_t comando = datosRecibidosSerial[2];
  //todos los comandos basicos tienen un solo byte de datos
  if (comando != COMANDOS_BLUETOOTH_VALORES_CALIBRA)
  {
    return 6;
  }
  //calibracion
  if (comando == COMANDOS_BLUETOOTH_VALORES_CALIBRA)
  {
    return 61;
  }

  return 0;
}

//envia un mensage que se mostrara en el GUI
void Telemetria::enviarMensaje(uint8_t idMensaje)
{
  uint8_t datosEnvioSerial[6] = {'$', 0x02, RESPUESTAS_BLUETOOTH_MSG, idMensaje, '\r', '\n'};
  serialPort->write(datosEnvioSerial, 6);
}

//Procesa la informacion almacenada en el buffer
void Telemetria::procesaComando()
{

  uint8_t accion = datosRecibidosSerial[2];
  bool procesado = false;

  //Activar / desactivar el envio de info automatico de datos del coche
  if (accion == COMANDOS_BLUETOOTH_ENVIO_PARAMETROS)
  {
    envio_parametros_coche = (datosRecibidosSerial[3] == 1);
     enviarMensaje(8);
    procesado = true;
  }

  //Activar / desactivar el envio de info automatico de datos de la IMU
  if (accion == COMANDOS_BLUETOOTH_ENVIO_IMU)
  {
    envio_parametros_imu = (datosRecibidosSerial[3] == 1);
     enviarMensaje(9);
    procesado = true;
  }

   //Envio de datos del coche
  if (accion == COMANDOS_BLUETOOTH_PEDIR_DATOS_COCHE)
  {
    enviarDatosCoche();
    procesado = true;
  }

  //envio datos de la IMU
  if (accion == COMANDOS_BLUETOOTH_PEDIR_DATOS_IMU)
  {
    enviarDatosImu();
    procesado = true;
  }

  //Request calibration
  if (accion == COMANDOS_BLUETOOTH_PETICION_CALIBRA)
  {
    enviarConfiguracionActual();
    procesado = true;
  }

  //Receive calibration
  if (accion == COMANDOS_BLUETOOTH_VALORES_CALIBRA)
  {
    //TODO REFACTOR
    int32_t tmpindice =3;
    int32_t tmpNum1 = datosRecibidosSerial[tmpindice++];
    int32_t tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 8);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 16);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    rc_Configuracion->configActual.C1 = (tmpNum1 | (tmpNum2 << 24))/1000.0;

    tmpNum1 = datosRecibidosSerial[tmpindice++];
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 8);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 16);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    rc_Configuracion->configActual.C2 = (tmpNum1 | (tmpNum2 << 24))/1000.0;

   tmpNum1 = datosRecibidosSerial[tmpindice++];
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 8);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 16);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    rc_Configuracion->configActual.C3 = (tmpNum1 | (tmpNum2 << 24))/1000.0;

    tmpNum1 = datosRecibidosSerial[tmpindice++];
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 8);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 16);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    rc_Configuracion->configActual.C4 = (tmpNum1 | (tmpNum2 << 24))/1000.0;

    tmpNum1 = datosRecibidosSerial[tmpindice++];
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 8);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 16);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    rc_Configuracion->configActual.C5 = (tmpNum1 | (tmpNum2 << 24))/1000.0;

    tmpNum1 = datosRecibidosSerial[tmpindice++];
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 8);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 16);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    rc_Configuracion->configActual.C6 = (tmpNum1 | (tmpNum2 << 24))/1000.0;

    tmpNum1 = datosRecibidosSerial[tmpindice++];
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 8);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 16);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    rc_Configuracion->configActual.C7 = (tmpNum1 | (tmpNum2 << 24))/1000.0;
    
    tmpNum1 = datosRecibidosSerial[tmpindice++];
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 8);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 16);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    rc_Configuracion->configActual.C8 = (tmpNum1 | (tmpNum2 << 24))/1000.0;

    tmpNum1 = datosRecibidosSerial[tmpindice++];
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 8);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 16);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    rc_Configuracion->configActual.C9 = (tmpNum1 | (tmpNum2 << 24))/1000.0;

    tmpNum1 = datosRecibidosSerial[tmpindice++];
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 8);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 16);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    rc_Configuracion->configActual.C10 =(tmpNum1 | (tmpNum2 << 24))/1000.0;

    tmpNum1 = datosRecibidosSerial[tmpindice++];
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 8);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 16);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    rc_Configuracion->configActual.C11 = (tmpNum1 | (tmpNum2 << 24))/1000.0;

    tmpNum1 = datosRecibidosSerial[tmpindice++];
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 8);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 16);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    rc_Configuracion->configActual.C12 = (tmpNum1 | (tmpNum2 << 24))/1000.0;

    tmpNum1 = datosRecibidosSerial[tmpindice++];
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 8);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 16);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    rc_Configuracion->configActual.C13 = (tmpNum1 | (tmpNum2 << 24))/1000.0;

    tmpNum1 = datosRecibidosSerial[tmpindice++];
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 8);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 16);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    rc_Configuracion->configActual.C14 = (tmpNum1 | (tmpNum2 << 24))/1000.0;

    rc_Configuracion->saveToEEPROM();

    enviarMensaje(7); // Nueva configuracion de calibracion recibida
    procesado = true;
  }

  //Manual move
  if (accion == COMANDOS_BLUETOOTH_CAMBIAR_MODO)
  {

    procesado = true;
  }

  //Cambiar tipo de control
  if (accion == COMANDOS_BLUETOOTH_CONTROL_REMOTO)
  {
    if (datosRecibidosSerial[3] == 1)
    {
      tipoControl = 1;
    }
    else
    {
      tipoControl = 0;
    }
    procesado = true;
  }

  //Peticion estado
  if (accion == COMANDOS_BLUETOOTH_PEDIR_ESTADO)
  {
    enviarDatosCoche(); // TODO: crear una respuesta mas detallada?
    procesado = true;
  }

  if (!procesado)
  {
    //TODO: mensage de error, comando no reconocido
    enviarMensaje(1);
  }

  nuevoComando = false;
  bytesRecibidos = 0;
}