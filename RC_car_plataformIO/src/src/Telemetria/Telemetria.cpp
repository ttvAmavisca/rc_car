#include "Telemetria.h"
//#define OUTPUT_TEAPOT true

Telemetria::Telemetria(void)
{
  envio_parametros_coche = false;
  envio_parametros_imu = false;
  //TODO: BORRAR
  //test para pruebas
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

void Telemetria::setBar(BMP280 *p_bar)
{
  barometro = p_bar;
}

void Telemetria::setObjects(Stream *port, MPU9250_DMP *imu_obj, Rc_car *p_rc_car, Configuracion *p_rc_Configuracion, BMP280 *p_bar)
{
  setSerialPort(port);
  setImu(imu_obj);
  setCar(p_rc_car);
  setConfig(p_rc_Configuracion);
  setBar(p_bar);
}

void Telemetria::NuevosValoresImu()
{
  float q0 = imu->calcQuat(imu->qw);
  float q1 = imu->calcQuat(imu->qx);
  float q2 = imu->calcQuat(imu->qy);
  float q3 = imu->calcQuat(imu->qz);

  rc_car->angulos[0] = q0;
  rc_car->angulos[1] = q1;
  rc_car->angulos[2] = q2;
  rc_car->angulos[3] = q3;

  rc_car->pitch = imu->pitch;
  rc_car->roll = imu->roll;
  rc_car->yaw = imu->yaw;

  rc_car->giro_val[0] = imu->gx;
  rc_car->giro_val[1] = imu->gy;
  rc_car->giro_val[2] = imu->gz;

  rc_car->aceleracion[0] = imu->ax;
  rc_car->aceleracion[1] = imu->ay;
  rc_car->aceleracion[2] = imu->az;

  rc_car->imu_temp = imu->temperature;

#ifdef OUTPUT_TEAPOT
  q0 *= 16384.0f;
  q1 *= 16384.0f;
  q2 *= 16384.0f;
  q3 *= 16384.0f;

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

  if (tel_Debug)
  {

    serialPort->println("Q: " + String(q0, 4) + ", " +
                        String(q1, 4) + ", " + String(q2, 4) +
                        ", " + String(q3, 4));
    serialPort->println("R/P/Y: " + String(imu->roll) + ", " + String(imu->pitch) + ", " + String(imu->yaw));
    serialPort->println("Time: " + String(imu->time) + " ms");
    serialPort->println();
  }
}

void Telemetria::NuevosValoresBar()
{

  rc_car->bar_presion = barometro->last_presure;
  rc_car->bar_temperatura = barometro->last_temperature;

  if (tel_Debug)
  {

    serialPort->println("Presion: " + String(rc_car->bar_presion, 4));
    serialPort->println("Temp: " + String(rc_car->bar_temperatura, 4));
    serialPort->println("Time: " + String(imu->time) + " ms");
    serialPort->println();
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
  uint8_t datosEnvioSerial[LENGH_RESPUESTAS_BLUETOOTH_DATOS_COCHE];

  int posicion = 0;
  datosEnvioSerial[posicion++] = '$';
  datosEnvioSerial[posicion++] = 0x02;
  datosEnvioSerial[posicion++] = RespuestasBluetooth::e_res_datos_coche;

  int16_t tmpshort = 0;

  tmpshort = round(rc_car->consigna[Rc_car::e_servo_motor] * 10.0); //16bit (64bit en C# PC). /10 en saltos de 10rpms es resolucion suficiente ya que el error de calculo es mayor
  datosEnvioSerial[posicion++] = (tmpshort & 0xff);
  datosEnvioSerial[posicion++] = ((tmpshort & 0xff00) >> 8);

  tmpshort = round(rc_car->consigna[Rc_car::e_servo_rueda_derecha] * 10.0); //16bit (64bit en C# PC). X100 para enviar 2 decimales
  datosEnvioSerial[posicion++] = (tmpshort & 0xff);
  datosEnvioSerial[posicion++] = ((tmpshort & 0xff00) >> 8);

  tmpshort = round(rc_car->consigna[Rc_car::e_servo_rueda_izquierda] * 10.0); //16bit (64bit en C# PC). X100 para enviar 2 decimales
  datosEnvioSerial[posicion++] = (tmpshort & 0xff);
  datosEnvioSerial[posicion++] = ((tmpshort & 0xff00) >> 8);

  tmpshort = round(rc_car->consigna[Rc_car::e_servo_marcha] * 10.0); //16bit (64bit en C# PC). X100 para enviar 2 decimales
  datosEnvioSerial[posicion++] = (tmpshort & 0xff);
  datosEnvioSerial[posicion++] = ((tmpshort & 0xff00) >> 8);

  tmpshort = round(rc_car->ESC_VoltajeEntrada * 10); //16bit (64bit en C# PC). X10 para enviar 1 decimales
  datosEnvioSerial[posicion++] = (tmpshort & 0xff);
  datosEnvioSerial[posicion++] = ((tmpshort & 0xff00) >> 8);

  tmpshort = round(rc_car->ESC_rpmActual / 10); //16bit (64bit en C# PC). /10 en saltos de 10rpms es resolucion suficiente ya que el error de calculo es mayor
  datosEnvioSerial[posicion++] = (tmpshort & 0xff);
  datosEnvioSerial[posicion++] = ((tmpshort & 0xff00) >> 8);

  tmpshort = round(rc_car->ESC_avgMotorCurrent * 100); //16bit (64bit en C# PC). X100 para enviar 2 decimales
  datosEnvioSerial[posicion++] = (tmpshort & 0xff);
  datosEnvioSerial[posicion++] = ((tmpshort & 0xff00) >> 8);

  tmpshort = round(rc_car->ESC_avgInputCurrent * 100); //16bit (64bit en C# PC). X100 para enviar 2 decimales
  datosEnvioSerial[posicion++] = (tmpshort & 0xff);
  datosEnvioSerial[posicion++] = ((tmpshort & 0xff00) >> 8);

  tmpshort = round(rc_car->ESC_Dutycycle * 100);
  datosEnvioSerial[posicion++] = rc_car->ESC_Dutycycle;

  datosEnvioSerial[posicion++] = rc_car->modo_motor_actual;
  datosEnvioSerial[posicion++] = rc_car->tipo_control_actual;
  datosEnvioSerial[posicion++] = rc_car->tipo_regulacion_direccion;
  datosEnvioSerial[posicion++] = rc_car->tipo_regulacion_potencia;

  datosEnvioSerial[posicion++] = '\r';
  datosEnvioSerial[posicion++] = '\n';

  serialPort->write(datosEnvioSerial, LENGH_RESPUESTAS_BLUETOOTH_DATOS_COCHE);
}

void Telemetria::enviarDatosEstado()
{
  uint8_t datosEnvioSerial[LENGH_RESPUESTAS_BLUETOOTH_ESTADO];

  int posicion = 0;
  datosEnvioSerial[posicion++] = '$';
  datosEnvioSerial[posicion++] = 0x02;
  datosEnvioSerial[posicion++] = RespuestasBluetooth::e_res_estado;

  datosEnvioSerial[posicion++] = rc_car->modo_motor_actual;
  datosEnvioSerial[posicion++] = rc_car->tipo_control_actual;
  datosEnvioSerial[posicion++] = rc_car->tipo_regulacion_direccion;
  datosEnvioSerial[posicion++] = rc_car->tipo_regulacion_potencia;

  datosEnvioSerial[posicion++] = '\r';
  datosEnvioSerial[posicion++] = '\n';

  serialPort->write(datosEnvioSerial, LENGH_RESPUESTAS_BLUETOOTH_ESTADO);
}

void Telemetria::enviarDatosManual()
{
  uint8_t datosEnvioSerial[LENGH_RESPUESTAS_BLUETOOTH_VALORESMANUAL];

  int posicion = 0;
  datosEnvioSerial[posicion++] = '$';
  datosEnvioSerial[posicion++] = 0x02;
  datosEnvioSerial[posicion++] = RespuestasBluetooth::e_res_valores_manual;

  int16_t tmpshort = 0;

  tmpshort = rc_car->consigna_manual[Rc_car::e_servo_rueda_derecha] * 10; //16bit (64bit en C# PC).  en saltos de 10rpms es resolucion suficiente ya que el error de calculo es mayor
  datosEnvioSerial[posicion++] = (tmpshort & 0xff);
  datosEnvioSerial[posicion++] = ((tmpshort & 0xff00) >> 8);

  tmpshort = rc_car->consigna_manual[Rc_car::e_servo_rueda_izquierda] * 10; //16bit (64bit en C# PC).para enviar 2 decimales
  datosEnvioSerial[posicion++] = (tmpshort & 0xff);
  datosEnvioSerial[posicion++] = ((tmpshort & 0xff00) >> 8);

  tmpshort = rc_car->consigna_manual[Rc_car::e_servo_marcha] * 10; //16bit (64bit en C# PC).  para enviar 2 decimales
  datosEnvioSerial[posicion++] = (tmpshort & 0xff);
  datosEnvioSerial[posicion++] = ((tmpshort & 0xff00) >> 8);

  tmpshort = rc_car->consigna_manual[Rc_car::e_servo_motor] * 10; //16bit (64bit en C# PC). para enviar 2 decimales
  datosEnvioSerial[posicion++] = (tmpshort & 0xff);
  datosEnvioSerial[posicion++] = ((tmpshort & 0xff00) >> 8);

  datosEnvioSerial[posicion++] = '\r';
  datosEnvioSerial[posicion++] = '\n';

  serialPort->write(datosEnvioSerial, LENGH_RESPUESTAS_BLUETOOTH_VALORESMANUAL);
}

void Telemetria::enviarDatosImu()
{
  uint8_t datosEnvioSerial[LENGH_RESPUESTAS_BLUETOOTH_DATOS_IMU];

  int posicion = 0;
  datosEnvioSerial[posicion++] = '$';
  datosEnvioSerial[posicion++] = 0x02;

  datosEnvioSerial[posicion++] = RespuestasBluetooth::e_res_datosn_imu;

  int32_t tmpLong = round(rc_car->pitch * 1000); //32bit (64bit en C# PC). X1000 para enviar 3 decimales, se podria enviar directamente en coma flotante
  datosEnvioSerial[posicion++] = (tmpLong & 0xff);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff00) >> 8);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff0000) >> 16);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff000000) >> 24);

  tmpLong = round(rc_car->roll * 1000); //32bit (64bit en C# PC). X1000 para enviar 3 decimales, se podria enviar directamente en coma flotante
  datosEnvioSerial[posicion++] = (tmpLong & 0xff);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff00) >> 8);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff0000) >> 16);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff000000) >> 24);

  tmpLong = round(rc_car->yaw * 1000); //32bit (64bit en C# PC). X1000 para enviar 3 decimales, se podria enviar directamente en coma flotante
  datosEnvioSerial[posicion++] = (tmpLong & 0xff);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff00) >> 8);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff0000) >> 16);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff000000) >> 24);

  tmpLong = round(rc_car->giro_val[0]); //32bit
  datosEnvioSerial[posicion++] = (tmpLong & 0xff);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff00) >> 8);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff0000) >> 16);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff000000) >> 24);

  tmpLong = round(rc_car->giro_val[1]); //32bit
  datosEnvioSerial[posicion++] = (tmpLong & 0xff);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff00) >> 8);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff0000) >> 16);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff000000) >> 24);

  tmpLong = round(rc_car->giro_val[2]); //32bit
  datosEnvioSerial[posicion++] = (tmpLong & 0xff);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff00) >> 8);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff0000) >> 16);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff000000) >> 24);

  tmpLong = round(rc_car->aceleracion[0]); //32bit
  datosEnvioSerial[posicion++] = (tmpLong & 0xff);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff00) >> 8);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff0000) >> 16);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff000000) >> 24);

  tmpLong = round(rc_car->aceleracion[1]); //32bit
  datosEnvioSerial[posicion++] = (tmpLong & 0xff);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff00) >> 8);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff0000) >> 16);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff000000) >> 24);

  tmpLong = round(rc_car->aceleracion[2]); //32bit
  datosEnvioSerial[posicion++] = (tmpLong & 0xff);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff00) >> 8);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff0000) >> 16);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff000000) >> 24);

  tmpLong = round(rc_car->angulos[0] * 1000); //32bit
  datosEnvioSerial[posicion++] = (tmpLong & 0xff);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff00) >> 8);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff0000) >> 16);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff000000) >> 24);

  tmpLong = round(rc_car->angulos[1] * 10000); //32bit (64bit en C# PC). X10000 para enviar 4 decimales
  datosEnvioSerial[posicion++] = (tmpLong & 0xff);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff00) >> 8);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff0000) >> 16);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff000000) >> 24);

  tmpLong = round(rc_car->angulos[2] * 10000); //32bit (64bit en C# PC). X10000 para enviar 4 decimales
  datosEnvioSerial[posicion++] = (tmpLong & 0xff);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff00) >> 8);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff0000) >> 16);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff000000) >> 24);

  tmpLong = round(rc_car->angulos[3] * 10000); //32bit (64bit en C# PC). X10000 para enviar 4 decimales
  datosEnvioSerial[posicion++] = (tmpLong & 0xff);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff00) >> 8);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff0000) >> 16);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff000000) >> 24);
  /*
  tmpLong = round(rc_car->imu_temp * 100); //32bit (64bit en C# PC). X10000 para enviar 4 decimales
  datosEnvioSerial[posicion++] = (tmpLong & 0xff);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff00) >> 8);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff0000) >> 16);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff000000) >> 24);
*/
  tmpLong = round(rc_car->bar_temperatura * 1000); //32bit (64bit en C# PC). X1000 para enviar 3 decimales
  datosEnvioSerial[posicion++] = (tmpLong & 0xff);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff00) >> 8);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff0000) >> 16);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff000000) >> 24);

  tmpLong = round(rc_car->bar_presion * 1000); //32bit (64bit en C# PC). X1000 para enviar 3 decimales
  datosEnvioSerial[posicion++] = (tmpLong & 0xff);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff00) >> 8);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff0000) >> 16);
  datosEnvioSerial[posicion++] = ((tmpLong & 0xff000000) >> 24);

  datosEnvioSerial[posicion++] = '\r';
  datosEnvioSerial[posicion++] = '\n';

  serialPort->write(datosEnvioSerial, LENGH_RESPUESTAS_BLUETOOTH_DATOS_IMU);
}

//Envia los valores de calibracion por bluetooth
void Telemetria::enviarConfiguracionActual()
{

  uint8_t datosEnvioSerial[LENGH_RESPUESTAS_BLUETOOTH_VALORES_CALIBRA];

  int posicion = 0;
  datosEnvioSerial[posicion++] = '$';
  datosEnvioSerial[posicion++] = 0x02;
  datosEnvioSerial[posicion++] = RespuestasBluetooth::e_res_valores_calibracion;

  int32_t tmpLong = round(rc_Configuracion->configActual.C1 * 1000.0);
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

  datosEnvioSerial[posicion++] = rc_Configuracion->VERSION_H;
  datosEnvioSerial[posicion++] = rc_Configuracion->VERSION_L;

  datosEnvioSerial[posicion++] = '\r';
  datosEnvioSerial[posicion++] = '\n';

  serialPort->write(datosEnvioSerial, LENGH_RESPUESTAS_BLUETOOTH_VALORES_CALIBRA);
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
          rc_car->modo_motor_actual = 0;
        if (inByte == 'D')
          rc_car->modo_motor_actual = 1;
        if (inByte == 'a')
          rc_car->tipo_control_actual = 0;
        if (inByte == 'e')
          rc_car->tipo_control_actual = 1;
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

  int64_t tiempo_ahora = esp_timer_get_time();

  if (envio_parametros_coche)
  {
    if (tiempo_ahora > _timAutoTelemetria_coche)
    {
      _timAutoTelemetria_coche = tiempo_ahora + round(rc_Configuracion->configActual.C5 * 1000.0);

      // Si habilitado envia cada Xms datos de telemetria
      enviarDatosCoche();
    }
  }

  if (envio_parametros_imu)
  {
    if (tiempo_ahora > _timAutoTelemetria_imu)
    {
      _timAutoTelemetria_imu = tiempo_ahora + round(rc_Configuracion->configActual.C6 * 1000.0);

      // Si habilitado envia cada Xms datos de telemetria
      enviarDatosImu();
    }
  }
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

  if (comando == ComandosBluetooth::e_com_nueva_calibracion)
  {
    return 61;
  }

  if (comando == ComandosBluetooth::e_com_nuevos_valoresmanual)
  {
    return 13;
  }

  //todos los comandos basicos tienen un solo byte de datos
  return 6;
}

//envia un mensage que se mostrara en el GUI
void Telemetria::enviarMensaje(uint8_t idMensaje)
{
  uint8_t datosEnvioSerial[6] = {'$', 0x02, RespuestasBluetooth::e_res_msg, idMensaje, '\r', '\n'};
  serialPort->write(datosEnvioSerial, 6);
}

void Telemetria::pong()
{
  uint8_t datosEnvioSerial[6] = {'$', 0x02, RespuestasBluetooth::e_res_pong, 0, '\r', '\n'};
  serialPort->write(datosEnvioSerial, 6);
}

//Procesa la informacion almacenada en el buffer
void Telemetria::procesaComando()
{

  uint8_t accion = datosRecibidosSerial[2];
  bool procesado = false;

  //Activar / desactivar el envio de info automatico de datos del coche
  if (accion == ComandosBluetooth::e_com_auto_parametros)
  {
    envio_parametros_coche = (datosRecibidosSerial[3] == 1);
    enviarMensaje(8);
    procesado = true;
  }

  //Ping
  if (accion == ComandosBluetooth::e_com_ping)
  {
    rc_car->last_manual_received = esp_timer_get_time() + 2000000ul;
    pong();
    procesado = true;
  }

  //Activar / desactivar el envio de info automatico de datos de la IMU
  if (accion == ComandosBluetooth::e_com_auto_imu)
  {
    envio_parametros_imu = (datosRecibidosSerial[3] == 1);
    enviarMensaje(9);
    procesado = true;
  }

  //Envio de datos del coche
  if (accion == ComandosBluetooth::e_com_peticion_coche)
  {
    enviarDatosCoche();
    procesado = true;
  }

  //envio datos de la IMU
  if (accion == ComandosBluetooth::e_com_peticion_imu)
  {
    enviarDatosImu();
    procesado = true;
  }

  //Request calibration
  if (accion == ComandosBluetooth::e_com_peticion_calibracion)
  {
    enviarConfiguracionActual();
    procesado = true;
  }

  //Receive calibration
  if (accion == ComandosBluetooth::e_com_nueva_calibracion)
  {
    //TODO REFACTOR
    int32_t tmpindice = 3;
    int32_t tmpNum1 = datosRecibidosSerial[tmpindice++];
    int32_t tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 8);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 16);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    rc_Configuracion->configActual.C1 = (tmpNum1 | (tmpNum2 << 24)) / 1000.0;

    tmpNum1 = datosRecibidosSerial[tmpindice++];
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 8);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 16);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    rc_Configuracion->configActual.C2 = (tmpNum1 | (tmpNum2 << 24)) / 1000.0;

    tmpNum1 = datosRecibidosSerial[tmpindice++];
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 8);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 16);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    rc_Configuracion->configActual.C3 = (tmpNum1 | (tmpNum2 << 24)) / 1000.0;

    tmpNum1 = datosRecibidosSerial[tmpindice++];
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 8);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 16);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    rc_Configuracion->configActual.C4 = (tmpNum1 | (tmpNum2 << 24)) / 1000.0;

    tmpNum1 = datosRecibidosSerial[tmpindice++];
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 8);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 16);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    rc_Configuracion->configActual.C5 = (tmpNum1 | (tmpNum2 << 24)) / 1000.0;

    tmpNum1 = datosRecibidosSerial[tmpindice++];
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 8);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 16);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    rc_Configuracion->configActual.C6 = (tmpNum1 | (tmpNum2 << 24)) / 1000.0;

    tmpNum1 = datosRecibidosSerial[tmpindice++];
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 8);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 16);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    rc_Configuracion->configActual.C7 = (tmpNum1 | (tmpNum2 << 24)) / 1000.0;

    tmpNum1 = datosRecibidosSerial[tmpindice++];
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 8);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 16);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    rc_Configuracion->configActual.C8 = (tmpNum1 | (tmpNum2 << 24)) / 1000.0;

    tmpNum1 = datosRecibidosSerial[tmpindice++];
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 8);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 16);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    rc_Configuracion->configActual.C9 = (tmpNum1 | (tmpNum2 << 24)) / 1000.0;

    tmpNum1 = datosRecibidosSerial[tmpindice++];
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 8);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 16);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    rc_Configuracion->configActual.C10 = (tmpNum1 | (tmpNum2 << 24)) / 1000.0;

    tmpNum1 = datosRecibidosSerial[tmpindice++];
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 8);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 16);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    rc_Configuracion->configActual.C11 = (tmpNum1 | (tmpNum2 << 24)) / 1000.0;

    tmpNum1 = datosRecibidosSerial[tmpindice++];
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 8);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 16);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    rc_Configuracion->configActual.C12 = (tmpNum1 | (tmpNum2 << 24)) / 1000.0;

    tmpNum1 = datosRecibidosSerial[tmpindice++];
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 8);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 16);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    rc_Configuracion->configActual.C13 = (tmpNum1 | (tmpNum2 << 24)) / 1000.0;

    tmpNum1 = datosRecibidosSerial[tmpindice++];
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 8);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 16);
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    rc_Configuracion->configActual.C14 = (tmpNum1 | (tmpNum2 << 24)) / 1000.0;

    rc_Configuracion->saveToEEPROM();

    enviarMensaje(7); // Nueva configuracion de calibracion recibida
    procesado = true;
  }

  //Manual move
  if (accion == ComandosBluetooth::e_com_cambiar_modo)
  {
    rc_car->modo_motor_actual = datosRecibidosSerial[3];
    enviarMensaje(11); //cambio modo OK
    procesado = true;
  }

  //Cambiar tipo de control
  if (accion == ComandosBluetooth::e_com_control_remoto)
  {
    if (datosRecibidosSerial[3] == 1)
    {
      rc_car->tipo_control_actual = 1;
    }
    else
    {
      rc_car->tipo_control_actual = 0;
    }
    procesado = true;
  }

  //Peticion estado
  if (accion == ComandosBluetooth::e_com_PedirEstado)
  {
    enviarDatosEstado(); // TODO: crear una respuesta mas detallada?
    procesado = true;
  }

  //Peticion valores manual
  if (accion == ComandosBluetooth::e_com_pedir_valoresmanual)
  {
    enviarDatosManual();
    procesado = true;
  }

  //Nuevos valores manuales
  if (accion == ComandosBluetooth::e_com_nuevos_valoresmanual)
  {
    int32_t tmpindice = 3;
    int32_t tmpNum1 = datosRecibidosSerial[tmpindice++];
    int32_t tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 8);
    rc_car->consigna_manual[Rc_car::e_servo_rueda_derecha] = (tmpNum1 - 5000.0f) / 50.0f;

    tmpNum1 = datosRecibidosSerial[tmpindice++];
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 8);
    rc_car->consigna_manual[Rc_car::e_servo_rueda_izquierda] = (tmpNum1 - 5000.0f) / 50.0f;

    tmpNum1 = datosRecibidosSerial[tmpindice++];
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 8);
    rc_car->consigna_manual[Rc_car::e_servo_marcha] = (tmpNum1 - 5000.0f) / 50.0f;

    tmpNum1 = datosRecibidosSerial[tmpindice++];
    tmpNum2 = datosRecibidosSerial[tmpindice++] & 0xFF;
    tmpNum1 = tmpNum1 | (tmpNum2 << 8);
    rc_car->consigna_manual[Rc_car::e_servo_motor] = (tmpNum1 - 5000.0f) / 50.0f;
    enviarMensaje(10); // Nuevos valores manuales
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
