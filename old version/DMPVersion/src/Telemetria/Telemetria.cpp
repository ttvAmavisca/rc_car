#include "Telemetria.h"


Telemetria::Telemetria(void){
	
}


void Telemetria::setSerialPort(Stream* port)
{
	serialPort = port;
}

void Telemetria::setImu(MPU9250_DMP * imu_obj)
{
	imu = imu_obj;
}

void Telemetria::setCar(Rc_car * p_rc_car){
	rc_car = p_rc_car;
}

void Telemetria::setConfig(Configuracion * p_rc_Configuracion){
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
		
		angulos[0]=q0;
		angulos[1]=q1;
		angulos[2]=q2;
		angulos[3]=q3;
		
		pitch=q3; roll=q3; yaw=q3;
		velocidad[0]=q0;
		velocidad[1]=q1;
		velocidad[2]=q2;
	
		aceleracion[0]=q0;
		aceleracion[1]=q1;
		aceleracion[2]=q2;
		

        serialPort->println("Q: " + String(q0, 4) + ", " +
                           String(q1, 4) + ", " + String(q2, 4) +
                           ", " + String(q3, 4));
        serialPort->println("R/P/Y: " + String(imu->roll) + ", "
                           + String(imu->pitch) + ", " + String(imu->yaw));
        serialPort->println("Time: " + String(imu->time) + " ms");
        serialPort->println();
		
		#ifdef OUTPUT_TEAPOT
			  float q0 *= 16384.0f;
			  float q1 *= 16384.0f;
			  float q2 *= 16384.0f;
			  float q3 *= 16384.0f;

			  uint8_t teapotPacket[14] = { '$', 0x02, 0, 0, 0, 0, 0, 0, 0, 0, 0x00, 0x00, '\r', '\n' };
			  // display quaternion values in InvenSense Teapot demo format:
			  teapotPacket[2] =(uint8_t) (((uint32_t)(q0 ) & 0xFF00) >> 8); 
			  teapotPacket[3] =(uint8_t) ((uint32_t)(q0 ) & 0xFF); 
			  teapotPacket[4] =(uint8_t) (((uint32_t)(q1 ) & 0xFF00) >> 8);
			  teapotPacket[5] =(uint8_t) ((uint32_t)(q1 ) & 0xFF);
			  teapotPacket[6] =(uint8_t) (((uint32_t)(q2) & 0xFF00) >> 8);
			  teapotPacket[7] =(uint8_t) ((uint32_t)(q2) & 0xFF);
			  teapotPacket[8] =(uint8_t) (((uint32_t)(q3) & 0xFF00) >> 8);
			  teapotPacket[9] =(uint8_t) ((uint32_t)(q3 ) & 0xFF);
			  if(OUTPUT_TEAPOT) serialPort->write(teapotPacket, 14);
			  teapotPacket[11]++; // packetCount, loops at 0xFF on purpose
		#endif
      }
}

void Telemetria::setDebug(bool estado){
	tel_Debug = estado;
}

// ================================================================
// ===             Comunicaciones Bluetooth                     ===
// ================================================================

//Envia el valor de estado del coche por bluetooth
void Telemetria::enviarDatosCoche(){
  uint8_t datosEnvioSerial[16] = { '$', 0x02,1 ,0,0 ,0,0 ,0,0 ,0,0 ,0,0 ,0 ,'\r', '\n' };
  
  int posicion=3;
  int16_t tmpshort=0;
 
  tmpshort= round(rpmActual /10 );  //16bit (64bit en C# PC). /10 en saltos de 10rpms es resolucion suficiente ya que el error de calculo es mayor
  datosEnvioSerial[posicion++]=(tmpshort & 0xff);
  datosEnvioSerial[posicion++]=((tmpshort & 0xff00)>>8);
  
  tmpshort= round(ConsignaRPMActual /10 );  //16bit (64bit en C# PC). /10 en saltos de 10rpms es resolucion suficiente ya que el error de calculo es mayor
  datosEnvioSerial[posicion++]=(tmpshort & 0xff);
  datosEnvioSerial[posicion++]=((tmpshort & 0xff00)>>8);
  
  
  tmpshort= round(ConsignaDireccionActual * 100 );  //16bit (64bit en C# PC). X100 para enviar 2 decimales
  datosEnvioSerial[posicion++]=(tmpshort & 0xff);
  datosEnvioSerial[posicion++]=((tmpshort & 0xff00)>>8);
  
  tmpshort= round(AnguloRuedaDerecha /10 ); //16bit (64bit en C# PC). X100 para enviar 2 decimales
  datosEnvioSerial[posicion++]=(tmpshort & 0xff);
  datosEnvioSerial[posicion++]=((tmpshort & 0xff00)>>8);
  
  tmpshort= round(AnguloRuedaIzquierda /10 );  //16bit (64bit en C# PC). X100 para enviar 2 decimales
  datosEnvioSerial[posicion++]=(tmpshort & 0xff);
  datosEnvioSerial[posicion++]=((tmpshort & 0xff00)>>8);
  
  
  
  datosEnvioSerial[posicion++]=tipoControl;
  
  serialPort->write(datosEnvioSerial,16);
}

void Telemetria::enviarDatosImu(){
  uint8_t datosEnvioSerial[28] = { '$', 0x02,1 ,0,0,0,0 ,0,0,0,0 ,0,0,0,0 ,0,0 ,0,0 ,0,0 ,0,0 ,0,0 ,0 ,'\r', '\n' };
  
  int posicion=3;
  int16_t tmpshort=0;
 
  int32_t tmpLong= round(pitch *1000); //32bit (64bit en C# PC). X1000 para enviar 3 decimales, se podria enviar directamente en coma flotante
  datosEnvioSerial[posicion++]=(tmpLong & 0xff);
  datosEnvioSerial[posicion++]=((tmpLong & 0xff00)>>8);
  datosEnvioSerial[posicion++]=((tmpLong & 0xff0000)>>16);
  datosEnvioSerial[posicion++]=((tmpLong & 0xff000000)>>24);
  
  tmpLong= round(roll *1000);  //32bit (64bit en C# PC). X1000 para enviar 3 decimales, se podria enviar directamente en coma flotante
  datosEnvioSerial[posicion++]=(tmpLong & 0xff);
  datosEnvioSerial[posicion++]=((tmpLong & 0xff00)>>8);
  datosEnvioSerial[posicion++]=((tmpLong & 0xff0000)>>16);
  datosEnvioSerial[posicion++]=((tmpLong & 0xff000000)>>24);
  
  tmpLong= round(yaw *1000);  //32bit (64bit en C# PC). X1000 para enviar 3 decimales, se podria enviar directamente en coma flotante
  datosEnvioSerial[posicion++]=(tmpLong & 0xff);
  datosEnvioSerial[posicion++]=((tmpLong & 0xff00)>>8);
  datosEnvioSerial[posicion++]=((tmpLong & 0xff0000)>>16);
  datosEnvioSerial[posicion++]=((tmpLong & 0xff000000)>>24);
		
  tmpLong= round(velocidad[0]*1000 );  //16bit (64bit en C# PC). /10 en saltos de 10rpms es resolucion suficiente ya que el error de calculo es mayor
  datosEnvioSerial[posicion++]=(tmpLong & 0xff);
  datosEnvioSerial[posicion++]=((tmpLong & 0xff00)>>8);
  datosEnvioSerial[posicion++]=((tmpLong & 0xff0000)>>16);
  datosEnvioSerial[posicion++]=((tmpLong & 0xff000000)>>24);
  
  tmpLong= round(velocidad[1]*1000 );  //16bit (64bit en C# PC). /10 en saltos de 10rpms es resolucion suficiente ya que el error de calculo es mayor
  datosEnvioSerial[posicion++]=(tmpLong & 0xff);
  datosEnvioSerial[posicion++]=((tmpLong & 0xff00)>>8);
  datosEnvioSerial[posicion++]=((tmpLong & 0xff0000)>>16);
  datosEnvioSerial[posicion++]=((tmpLong & 0xff000000)>>24);
 
  tmpLong= round(velocidad[2] *1000 );  //16bit (64bit en C# PC). X100 para enviar 2 decimales
  datosEnvioSerial[posicion++]=(tmpLong & 0xff);
  datosEnvioSerial[posicion++]=((tmpLong & 0xff00)>>8);
  datosEnvioSerial[posicion++]=((tmpLong & 0xff0000)>>16);
  datosEnvioSerial[posicion++]=((tmpLong & 0xff000000)>>24);
	
  tmpLong= round(aceleracion[0]*1000 );  //16bit (64bit en C# PC). /10 en saltos de 10rpms es resolucion suficiente ya que el error de calculo es mayor
  datosEnvioSerial[posicion++]=(tmpLong & 0xff);
  datosEnvioSerial[posicion++]=((tmpLong & 0xff00)>>8);
  datosEnvioSerial[posicion++]=((tmpLong & 0xff0000)>>16);
  datosEnvioSerial[posicion++]=((tmpLong & 0xff000000)>>24);
  
  tmpLong= round(aceleracion[1]*1000 );  //16bit (64bit en C# PC). /10 en saltos de 10rpms es resolucion suficiente ya que el error de calculo es mayor
  datosEnvioSerial[posicion++]=(tmpLong & 0xff);
  datosEnvioSerial[posicion++]=((tmpLong & 0xff00)>>8);
  datosEnvioSerial[posicion++]=((tmpLong & 0xff0000)>>16);
  datosEnvioSerial[posicion++]=((tmpLong & 0xff000000)>>24);
 
  tmpLong= round(aceleracion[2] *1000 );  //16bit (64bit en C# PC). X100 para enviar 2 decimales
  datosEnvioSerial[posicion++]=(tmpLong & 0xff);
  datosEnvioSerial[posicion++]=((tmpLong & 0xff00)>>8);
  datosEnvioSerial[posicion++]=((tmpLong & 0xff0000)>>16);
  datosEnvioSerial[posicion++]=((tmpLong & 0xff000000)>>24);
  
  tmpLong= round(angulos[0]*1000 ); //16bit (64bit en C# PC). X100 para enviar 2 decimales
  datosEnvioSerial[posicion++]=(tmpLong & 0xff);
  datosEnvioSerial[posicion++]=((tmpLong & 0xff00)>>8);
  datosEnvioSerial[posicion++]=((tmpLong & 0xff0000)>>16);
  datosEnvioSerial[posicion++]=((tmpLong & 0xff000000)>>24);
  
  tmpLong= round(angulos[1] *1000 );  //16bit (64bit en C# PC). X100 para enviar 2 decimales
  datosEnvioSerial[posicion++]=(tmpLong & 0xff);
  datosEnvioSerial[posicion++]=((tmpLong & 0xff00)>>8);
  datosEnvioSerial[posicion++]=((tmpLong & 0xff0000)>>16);
  datosEnvioSerial[posicion++]=((tmpLong & 0xff000000)>>24);
  
  tmpLong= round(angulos[2] *1000 );  //16bit (64bit en C# PC). X100 para enviar 2 decimales
  datosEnvioSerial[posicion++]=(tmpLong & 0xff);
  datosEnvioSerial[posicion++]=((tmpLong & 0xff00)>>8);
  datosEnvioSerial[posicion++]=((tmpLong & 0xff0000)>>16);
  datosEnvioSerial[posicion++]=((tmpLong & 0xff000000)>>24);
  
  tmpLong= round(angulos[3] *1000 );  //16bit (64bit en C# PC). X100 para enviar 2 decimales
  datosEnvioSerial[posicion++]=(tmpLong & 0xff);
  datosEnvioSerial[posicion++]=((tmpLong & 0xff00)>>8);
  datosEnvioSerial[posicion++]=((tmpLong & 0xff0000)>>16);
  datosEnvioSerial[posicion++]=((tmpLong & 0xff000000)>>24);
  
  datosEnvioSerial[posicion++]=tipoControl;
  
  serialPort->write(datosEnvioSerial,28);
}

//Envia los valores de calibracion por bluetooth
void Telemetria::enviarConfiguracionActual(){

  uint8_t datosEnvioSerial[24] = { '$', 0x02,4, 0,0,0,0 ,0,0,0,0 ,0,0,0,0 ,0,0,0,0, 0, 0, 0, '\r', '\n' };
  int32_t tmpLong= round(rc_Configuracion->configActual.C1 *1000.0); 
  int posicion=3;
  datosEnvioSerial[posicion++]=(tmpLong & 0xff);
  datosEnvioSerial[posicion++]=(tmpLong & 0xff00)>>8;
  datosEnvioSerial[posicion++]=(tmpLong & 0xff0000)>>16;
  datosEnvioSerial[posicion++]=(tmpLong & 0xff000000)>>24;
  
  tmpLong= round(rc_Configuracion->configActual.C2 *1000.0);
  datosEnvioSerial[posicion++]=(tmpLong & 0xff);
  datosEnvioSerial[posicion++]=(tmpLong & 0xff00)>>8;
  datosEnvioSerial[posicion++]=(tmpLong & 0xff0000)>>16;
  datosEnvioSerial[posicion++]=(tmpLong & 0xff000000)>>24;
  

  tmpLong= round(rc_Configuracion->configActual.C3 *1000.0);
  datosEnvioSerial[posicion++]=(tmpLong & 0xff);
  datosEnvioSerial[posicion++]=(tmpLong & 0xff00)>>8;
  datosEnvioSerial[posicion++]=(tmpLong & 0xff0000)>>16;
  datosEnvioSerial[posicion++]=(tmpLong & 0xff000000)>>24;

  tmpLong= round(rc_Configuracion->configActual.C4 *1000.0);
  datosEnvioSerial[posicion++]=(tmpLong & 0xff);
  datosEnvioSerial[posicion++]=(tmpLong & 0xff00)>>8;
  datosEnvioSerial[posicion++]=(tmpLong & 0xff0000)>>16;
  datosEnvioSerial[posicion++]=(tmpLong & 0xff000000)>>24;
  
  datosEnvioSerial[posicion++]=tipoControl;
  
  datosEnvioSerial[posicion++]=rc_Configuracion->VERSION_H;
  datosEnvioSerial[posicion++]=rc_Configuracion->VERSION_L;
  
  serialPort->write(datosEnvioSerial,24);

}

//Evento/Interrupcion de recepcion por bluetooth
void Telemetria::serialEvent2(){
  
  if (!serialPort->available()) return;
  
  int buclerino=0; // maximo de caracteres a procesar 100 antes de continuar con el resto de operaciones

  if (!nuevoComando) { //ignorar si no se proceso el anterior
    while (serialPort->available() && !nuevoComando && buclerino <100) {
      buclerino++;
      
      uint8_t inByte = (uint8_t) serialPort->read();
      //Serial.println(inByte);
      if (!conexionCorrecta && inByte != '$') 
      {
        //Compatibilidad con app por defecto android
        if (inByte == 'U') modo_motor_actual =0;
        if (inByte == 'D') modo_motor_actual =1;
        if (inByte == 'a') tipoControl=0;
        if (inByte == 'e') tipoControl=1;
        return;
      }
      conexionCorrecta=true;
      
      if ((bytesRecibidos == 1 && inByte != 2)
            || combrobarFinCadena())  {
            bytesRecibidos = 0;
            conexionCorrecta = false;
            enviarMensaje(2);// error fin de cadena no esperado
            return;
        }
        
       if (bytesRecibidos > 0 || inByte == '$') {
       
          datosRecibidosSerial[bytesRecibidos]=inByte;
          bytesRecibidos++;
          
          if (bytesRecibidos == reconocerComando()) {
            //bytesRecibidos = 0; //movido a momento lectura comando
            nuevoComando=true;
             procesaComando();
          }
       }
    }
  }
}

//Comprueba si el mensaje a terminado
bool Telemetria::combrobarFinCadena(){
  int longitudMensaje=reconocerComando();

  if (bytesRecibidos > longitudMensaje) return true; //error longitud excesiba
  
  if ((bytesRecibidos == (longitudMensaje-1)) && (datosRecibidosSerial[longitudMensaje-2] !='\r')) return true; //error terminador 1
  if ((bytesRecibidos == longitudMensaje) && (datosRecibidosSerial[longitudMensaje-1] !='\n')) return true; //error terminador 2
  
  return false;
}

//Retorna la longitud del comando que se esta recibiendo actualmente basado en el codigo de operacion
int Telemetria::reconocerComando() {
 
  if (bytesRecibidos <=3) return (LONGITUD_BUFFER_BLUETOOTH -1); // aun no se ha recibido el ID de comando(NOTA al recibirlo en el array se suma por tanto el indice ha de ser 4 o mas para q este leido), suponer longitud maxima
  
  uint8_t comando=datosRecibidosSerial[2];
  //todos los comandos basicos tienen un solo byte de datos
  if (comando != COMANDOS_BLUETOOTH_VALORES_CALIBRA)
  { 
    return 6; 
  }
  //calibracion
  if (comando ==COMANDOS_BLUETOOTH_VALORES_CALIBRA)
  {
    return 21;
  }
 
  return 0;
}

//envia un mensage que se mostrara en el GUI
void Telemetria::enviarMensaje(uint8_t idMensaje) {
   uint8_t datosEnvioSerial[6] = { '$', 0x02,2 ,idMensaje,'\r', '\n' };
   serialPort->write(datosEnvioSerial,6);
}

//Procesa la informacion almacenada en el buffer
void Telemetria::procesaComando() {
 
  uint8_t accion= datosRecibidosSerial[2];
  bool procesado =false;


  //start
  if (accion ==COMANDOS_BLUETOOTH_INICIAR)
  {
      modo_motor_actual=1;
      procesado=true;
  }

  //Stop
  if (accion ==  COMANDOS_BLUETOOTH_PARAR)
  {
    modo_motor_actual=0;
    procesado=true;
  }

  //Request calibration
  if (accion ==  COMANDOS_BLUETOOTH_PETICION_CALIBRA)
  {
    enviarConfiguracionActual();
    procesado=true;
  }
  
  //Receive calibration
  if (accion ==COMANDOS_BLUETOOTH_VALORES_CALIBRA)
  {
    //TODO REFACTOR

    int32_t valorLQRK1Recibida=  datosRecibidosSerial[3];
    int32_t tmpLong=datosRecibidosSerial[4]  & 0xFF;
    valorLQRK1Recibida= valorLQRK1Recibida | (tmpLong << 8);
    tmpLong=datosRecibidosSerial[5]  & 0xFF;
    valorLQRK1Recibida= valorLQRK1Recibida | (tmpLong << 16);
    tmpLong=datosRecibidosSerial[6]  & 0xFF;
    rc_Configuracion->configActual.C1= valorLQRK1Recibida | (tmpLong << 24);
    
   
    int32_t valorLQRK2Recibida=  datosRecibidosSerial[7];
    tmpLong=datosRecibidosSerial[8]  & 0xFF;
    valorLQRK2Recibida= valorLQRK2Recibida | (tmpLong << 8);
    tmpLong=datosRecibidosSerial[9]  & 0xFF;
    valorLQRK2Recibida= valorLQRK2Recibida | (tmpLong << 16);
    tmpLong=datosRecibidosSerial[10]  & 0xFF;
    rc_Configuracion->configActual.C2= valorLQRK2Recibida | (tmpLong << 24);

    int32_t valorLQRK3Recibida=  datosRecibidosSerial[11];
    tmpLong=datosRecibidosSerial[12]  & 0xFF;
    valorLQRK3Recibida= valorLQRK3Recibida | (tmpLong << 8);
    tmpLong=datosRecibidosSerial[13]  & 0xFF;
    valorLQRK3Recibida= valorLQRK3Recibida | (tmpLong << 16);
    tmpLong=datosRecibidosSerial[14]  & 0xFF;
    rc_Configuracion->configActual.C3= valorLQRK3Recibida | (tmpLong << 24);

    int32_t valorLQRK4Recibida=  datosRecibidosSerial[15];
    tmpLong=datosRecibidosSerial[16]  & 0xFF;
    valorLQRK4Recibida= valorLQRK4Recibida | (tmpLong << 8);
    tmpLong=datosRecibidosSerial[17]  & 0xFF;
    valorLQRK4Recibida= valorLQRK4Recibida | (tmpLong << 16);
    tmpLong=datosRecibidosSerial[18]  & 0xFF;
    rc_Configuracion->configActual.C4= valorLQRK4Recibida | (tmpLong << 24);

    
    rc_Configuracion->saveToEEPROM();

    enviarMensaje(7);// Nueva configuracion de calibracion recibida
    procesado=true;
  }
  
  

  //Manual move
  if (accion ==COMANDOS_BLUETOOTH_MOVER_POSICION)
  {
    
    procesado=true;
  }

 //Cambiar tipo de control
  if (accion ==COMANDOS_BLUETOOTH_CAMBIAR_TIPO_REGULA)
  {
     if (datosRecibidosSerial[3]== 1) {
        tipoControl=1;
     } else {
        tipoControl=0;
     }
    procesado=true;
  }
  
  
  //Peticion estado
  if (accion ==COMANDOS_BLUETOOTH_PEDIR_ESTADO)
  {
    enviarDatosBluetooth(); // TODO: crear una respuesta mas detallada?
    procesado=true;
  }
  
  
  if (!procesado){
      //TODO: mensage de error, comando no reconocido
      enviarMensaje(1);
  }

  nuevoComando=false;
  bytesRecibidos = 0;
}
