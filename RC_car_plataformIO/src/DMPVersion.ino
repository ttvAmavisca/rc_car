/*

*/
/*
   V1.0 control RC
   6 entradas ppm(control remoto)
   2 salidas PPM direccion
   1 salida PPM cambio
   1 conexion serie VESC para control velocidad y lectura de parametros del motor
   1 conexion serie con PC o bluethoot para telemetria

*/

#define SerialDebug false           // Set to true to get Serial output for debugging
#define SerialDebugVESC false       // Set to true to get Serial output for debugging
#define SerialDebugPWMIN false      // Set to true to get Serial output for debugging
#define SerialDebugTelemetria false // Set to true to get Serial output for debugging
#define OUTPUT_TEAPOT false         // paquete teapot para ejemplo de fabricante IMU (intelsense)
#define CON_BLUETOOTH false         // habilitar envio por bluetooth, nota las librerias ocupan mas de la mitad de memoria de programa normal

//Libreria MPU9250, utilizando el DMP(mas lento pero valores filtrados)
#include "src/MPU9250_DMP/MPU9250-DMP.h"

//Libreria VESC
//#include "src/VescUart/VescUart.h"

//Libreria BLHeli
#include "src/LeerTelemetriaBlHeli/LeerTelemetriaBlHeli.h"

//Lectura de Pwm usando RMT
#include "src/LeerPWM_rmt/CanalesPwM.h"

//Salidas PWM a 50hz usando ledc(hardware)
#include "src/Servolib/ServoESP32.h"

#if CON_BLUETOOTH
//Libreria Bluetooth ESP32
#include "BluetoothSerial.h"
#if !defined(CONFIG_BT_ENABLED) || !defined(CONFIG_BLUEDROID_ENABLED)
#error Bluetooth is not enabled! Please run `make menuconfig` to and enable it
#endif

BluetoothSerial SerialBT;
#endif

//Comunicacion para configuracion y telemetria
#include "src/Telemetria/Telemetria.h"
Telemetria rc_Telemetria;

//Datos de configuracion, guardado y cargado de memoria
#include "src/Configuracion/Configuracion.h"

Configuracion rc_Configuracion;

//RC car
#include "src/rc_car_obj/Rc_car.h"
Rc_car rc_car;

uint64_t debug100ms = 0;
uint64_t debugtiming = 0;
uint64_t debugtiming_count = 0;

// Pin definitions

bool rc_mpu_init = false;
#define SerialPort Serial

MPU9250_DMP imu;

/** Initiate VescUart class */

//VescUart UART;

LeerTelemetriaBlHeli BLHeli;


//Servos
Servo servos[4];


//Lectura de canales de entrada. Pines 30+ algunos definidos como solo entrada
CanalesPwM leerCanales(35, 34, 33, 32, 36, 39);


#if CON_BLUETOOTH
void setupBluetooth()
{
  SerialBT.begin("RC_Car_telemetry"); //Bluetooth device name
  Serial.println("bluetooth iniciado");
}
#endif

String command;
int throttle = 127;
HardwareSerial pSerial2(2);

void setupBLHeli(){
  pSerial2.begin(115200, SERIAL_8N1, 16, 17);

  while (!pSerial2)
  {
    ;
  }
  BLHeli.setSerialPort(& pSerial2);
  BLHeli.processData();
}

void BLHeliControl(){
  BLHeli.processData();
  rc_car.ESC_avgInputCurrent = BLHeli.Current;
  rc_car.ESC_Dutycycle = BLHeli.mAh;
  rc_car.ESC_rpmActual = BLHeli.eRpM;
  rc_car.ESC_VoltajeEntrada = BLHeli.Voltage;
  rc_car.ESC_avgMotorCurrent = BLHeli.temperatura;
}

/*
void setupVesc()
{

  pSerial2.begin(115200, SERIAL_8N1, 16, 17);

  while (!pSerial2)
  {
    ;
  }

  // Define which ports to use as UART
  UART.setSerialPort(&pSerial2);

  if (SerialDebugVESC)
  {
    UART.setDebugPort(&Serial);
  }
}


void vescControl()
{
  UART.nunchuck.valueY = throttle;
  // Call the function setNunchuckValues to send the current nunchuck values to the VESC
  UART.setNunchuckValues();
  if (UART.getVescValues() > 0)
  {
    if (SerialDebug)
    {
      Serial.print("input V = ");
      Serial.println(UART.data.inpVoltage);
    }
  }
}
*/

void setupMPU9250()
{
  // Call imu.begin() to verify communication and initialize
  if (imu.begin() != INV_SUCCESS)
  {
    rc_mpu_init = false;
    Serial.println("Unable to communicate with MPU-9250");
    Serial.println("Check connections, and try again.");
    Serial.println();
    delay(5000);
  }

  imu.dmpBegin(DMP_FEATURE_6X_LP_QUAT |  // Enable 6-axis quat
                   DMP_FEATURE_GYRO_CAL, // Use gyro calibration
               1000);                    // Set DMP FIFO rate to 1000 Hz
  // DMP_FEATURE_LP_QUAT can also be used. It uses the
  // accelerometer in low-power mode to estimate quat's.
  // DMP_FEATURE_LP_QUAT and 6X_LP_QUAT are mutually exclusive

  rc_mpu_init = true;
}

void imuGetValues()
{
  if (imu.fifoAvailable())
  {
    // Use dmpUpdateFifo to update the ax, gx, mx, etc. values
    if (imu.dmpUpdateFifo() == INV_SUCCESS)
    {
      debugtiming_count = esp_timer_get_time() - debugtiming; // tiempo entre updates de IMU para calculos performance
      debugtiming = esp_timer_get_time();

      // computeEulerAngles can be used -- after updating the
      // quaternion values -- to estimate roll, pitch, and yaw
      imu.computeEulerAngles();
      rc_Telemetria.NuevosValoresImu(); //Actualizar valores de telemetria
      Serial.print((int)esp_timer_get_time());Serial.print(" trolo ");Serial.print((int)imu.time);Serial.println("su");
    }
    else{
      Serial.print((int)esp_timer_get_time());Serial.println("nu");
    }
  } else {Serial.print((int)esp_timer_get_time());Serial.println("nunu");}
}

void setup_pwmIN()
{
  //Inicia los 6 canales de lectura PWM
  leerCanales.initCanales();
}

void setup_pwmOut()
{
  //asignar pines de salida y inicializar canales
  servos[Rc_car::enum_rueda_derecha].attach(25);
  servos[Rc_car::enum_rueda_izquierda].attach(26,5000,-1,0,180,0,5000);
  servos[Rc_car::enum_marcha].attach(27);
  servos[Rc_car::enum_motor].attach(14);
}

void calcular_pwmOut()
{
  //servos[enum_rueda_derecha].write((100 + (35 * 1)) % 180);
  servos[Rc_car::enum_rueda_derecha].writeMicroseconds(rc_car.consigna[Rc_car::enum_rueda_derecha]);
  //servos[Rc_car::enum_rueda_izquierda].writeMicroseconds(rc_car.consigna[Rc_car::enum_rueda_izquierda]);
  servos[Rc_car::enum_marcha].writeMicroseconds(rc_car.consigna[Rc_car::enum_marcha]);
  servos[Rc_car::enum_motor].writeMicroseconds(rc_car.consigna[Rc_car::enum_motor]);
}

void calcular_consignas()
{

  //motor
  switch (rc_car.modo_motor_actual)
  {
  case Rc_car::enum_manual:
    rc_car.consigna[Rc_car::enum_rueda_derecha] = 1000+  rc_car.consigna_manual[Rc_car::enum_rueda_derecha]/10;
    rc_car.consigna[Rc_car::enum_rueda_izquierda] = 1000  +  rc_car.consigna_manual[Rc_car::enum_rueda_izquierda]/10;
    rc_car.consigna[Rc_car::enum_marcha] =1000 +  rc_car.consigna_manual[Rc_car::enum_marcha]/10;
    rc_car.consigna[Rc_car::enum_motor] = 1000 + rc_car.consigna_manual[Rc_car::enum_motor]/10 ;
    break;
  case Rc_car::enum_sistema_salida:
    rc_car.consigna[Rc_car::enum_rueda_derecha] = leerCanales.valor(1) / 8;
    rc_car.consigna[Rc_car::enum_rueda_izquierda] = leerCanales.valor(1) / 8;
    rc_car.consigna[Rc_car::enum_marcha] = leerCanales.valor(3) / 8;
    rc_car.consigna[Rc_car::enum_motor] = 1700;
    break;
  case Rc_car::enum_semi_auto:
    rc_car.consigna[Rc_car::enum_rueda_derecha] = leerCanales.valor(1) / 8;
    rc_car.consigna[Rc_car::enum_rueda_izquierda] = leerCanales.valor(1) / 8;
    rc_car.consigna[Rc_car::enum_marcha] = leerCanales.valor(3) / 8;
    rc_car.consigna[Rc_car::enum_motor] = leerCanales.valor(4) / 8;
    break;
  case Rc_car::enum_full_auto:
    rc_car.consigna[Rc_car::enum_rueda_derecha] = leerCanales.valor(1) / 8;
    rc_car.consigna[Rc_car::enum_rueda_izquierda] = leerCanales.valor(1) / 8;
    rc_car.consigna[Rc_car::enum_marcha] = leerCanales.valor(3) / 8;
    //consigna[enum_motor]=leerCanales.valor(4) / 8;
    break;
  }
}

void setup()
{
  

  Wire.begin();
  // TWBR = 12;  // 400 kbit/sec I2C speed
  Serial.begin(115200);

  while (!Serial)
  {
  };

  setupMPU9250();

  //si se compila con bluetoht inicializarlo
#if CON_BLUETOOTH
  setupBluetooth();

  rc_Telemetria.setSerialPort(&SerialBT);
#else
  rc_Telemetria.setSerialPort(&Serial);
#endif

  //Inicializar objetos de telemetria
  rc_Telemetria.setImu(&imu);                 //pasar objeto de imu a telemetria
  rc_Telemetria.setCar(&rc_car);              //pasar objeto de coche
  rc_Telemetria.setConfig(&rc_Configuracion); //pasar objeto de config
  rc_Telemetria.setDebug(SerialDebugTelemetria);

  //setupVesc(); //configurar conumicacion con VESC
  setupBLHeli();

  setup_pwmIN(); //Configurar lectura de pwm de entrada usando rmt

  setup_pwmOut(); //configurar salidas PWM de control

  //Leer configuracion de EEPROM
  rc_Configuracion.getFromEEPROM();

  
  rc_car.consigna[Rc_car::enum_motor] = 1000;        // consigna inicial del motor para pruebas
}

void loop()
{

  //vescControl();
  BLHeliControl();

  

  if (SerialDebugPWMIN)
    leerCanales.debugOutSerial(&Serial);

  if (rc_mpu_init)
    imuGetValues();
 
  calcular_consignas();
  calcular_pwmOut();

  rc_Telemetria.serialEvent2(); //comprueba si hay nuevo mensaje bluetooth y responde


  if (millis() > debug100ms)
  {
    debug100ms = millis() + 1000;
    rc_Telemetria.autoTelemetria(); // Si habilitado envia cada 100ms datos de telemetria
    servos[Rc_car::enum_rueda_izquierda].writeMicroseconds(30);//rc_car.consigna[Rc_car::enum_rueda_izquierda]);
    
  } else {
    
    servos[Rc_car::enum_rueda_izquierda].writeMicroseconds(rc_car.consigna[Rc_car::enum_rueda_izquierda]);
  }
}
