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
#define Usar_DMP true           // Set to true to get Serial output for debugging
#define Usar_INA219 false           // usar INA en vez de valores de ESC


#define CON_VESC false         // Utilizar VESC como ESC
#define CON_BLHELI true         //Utilizar ESC BLHELI


#if Usar_INA219
//TODO: Libreria INA incluye un delay, intentar eliminarlo?
#include "src/INA219/INA219.h"
  TwoWire WireIna219(1);
  const int pin_sda_ina =2;
  const int pin_scl_ina =15;
#endif

#if Usar_DMP
//Libreria MPU9250, utilizando el DMP(mas lento pero valores filtrados)
#include "src/MPU9250_DMP/MPU9250-DMP.h"
#else
//Libreria MPU9250, utilizando el DMP(mas lento pero valores filtrados)
#include "src/MPU9250/MPU9250.h"
#endif
#include "src/BMP280/BMP280.h"


#if CON_VESC
//Libreria VESC
#include "src/VescUart/VescUart.h"
#endif

#if CON_BLHELI
//Libreria BLHeli
#include "src/LeerTelemetriaBlHeli/LeerTelemetriaBlHeli.h"
#endif
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


#if Usar_INA219
INA219 ina219;
#endif

#if Usar_DMP
MPU9250_DMP imu;
#else
IMU_MPU9250 imu;
#endif
BMP280 barometro;

/** Initiate VescUart class */
#if CON_VESC
VescUart UART;
#endif
#if CON_BLHELI
LeerTelemetriaBlHeli BLHeli;
#endif

//Servos
Servo servos[5];


//Lectura de canales de entrada. Pines 30+ algunos definidos como solo entrada
CanalesPwM leerCanales(35, 34, 33, 32, 36, 39);


#if CON_BLUETOOTH
void setupBluetooth()
{
  SerialBT.begin("RC_Car_telemetry"); //Bluetooth device name
  Serial.println("bluetooth iniciado");
}
#endif



HardwareSerial pSerial2(2);

#if Usar_INA219
void ina219getValues(){
  
  rc_car.ina_shuntvoltage = ina219.getShuntVoltage_mV();
  rc_car.ina_busvoltage = ina219.getBusVoltage_V();
  rc_car.ina_current_mA = ina219.getCurrent_mA();
  rc_car.ina_power_mW = ina219.getPower_mW();
  rc_car.ina_loadvoltage = rc_car.ina_busvoltage + (rc_car.ina_shuntvoltage / 1000);
/*
  Serial.print("Bus Voltage:   "); Serial.print(rc_car.ina_busvoltage); Serial.println(" V");
  Serial.print("Shunt Voltage: "); Serial.print(rc_car.ina_shuntvoltage ); Serial.println(" mV");
  Serial.print("Load Voltage:  "); Serial.print(rc_car.ina_loadvoltage); Serial.println(" V");
  Serial.print("Current:       "); Serial.print(rc_car.ina_current_mA); Serial.println(" mA");
  Serial.print("Power:         "); Serial.print(rc_car.ina_power_mW); Serial.println(" mW");
  Serial.println("");
*/
}
#endif

#if CON_BLHELI
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
  rc_car.ESC_mah = BLHeli.mAh;
  rc_car.ESC_rpmActual = BLHeli.eRpM;
  rc_car.ESC_VoltajeEntrada = BLHeli.Voltage;
  rc_car.ESC_temp = BLHeli.temperatura;
}
#endif
#if CON_VESC
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
  UART.nunchuck.valueY = 1;
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
#endif

void setupMPU9250()
{
  // Call imu.begin() to verify communication and initialize

  #if Usar_DMP
  if (imu.begin() != INV_SUCCESS)
  {
    rc_mpu_init = false;
    Serial.println("Unable to communicate with MPU-9250");
    Serial.println("Check connections, and try again.");
    Serial.println();
    delay(3000);
  }
  


  imu.dmpBegin(DMP_FEATURE_6X_LP_QUAT |  // Enable 6-axis quat
                   DMP_FEATURE_GYRO_CAL | // Use gyro calibration
                  DMP_FEATURE_SEND_CAL_GYRO | // Enviar info de gyro calibrada
                  DMP_FEATURE_SEND_RAW_ACCEL, // Enviar aceleracion raw
               1000);                    // Set DMP FIFO rate to 1000 Hz
 #else
    imu.Setup();
    imu.imu_SerialDebug=SerialDebug;
 #endif
 if (!barometro.begin(BMP280_ADDRESS_ALT)) {
    Serial.println(F("imposible conectar con barometro!"));
     delay(3000);
  }

  /* Default settings from datasheet. */
  barometro.setSampling(BMP280::MODE_NORMAL,     /* Operating Mode. */
                  BMP280::SAMPLING_X2,     /* Temp. oversampling */
                  BMP280::SAMPLING_X16,    /* Pressure oversampling */
                  BMP280::FILTER_X16,      /* Filtering. */
                  BMP280::STANDBY_MS_250); /* Standby time. */
  

  rc_mpu_init = true;
}

void imuGetValues()
{
  if(imu.getValues()){
     rc_Telemetria.NuevosValoresImu(); //Actualizar valores de telemetria
  }


  if (barometro.actualizar()){
      rc_Telemetria.NuevosValoresBar();
      
  }
}

void setup_pwmIN()
{
  //Inicia los 6 canales de lectura PWM
  leerCanales.initCanales_Interupt();
}

void leer_pwmIN()
{
  //Lee el ultimo valor leido desde memoria
  rc_car.consigna_rc[Rc_car::e_ch_rc_canal_1]=(leerCanales.valor(1));
  rc_car.consigna_rc[Rc_car::e_ch_rc_canal_2]=(leerCanales.valor(2));
  rc_car.consigna_rc[Rc_car::e_ch_rc_canal_3]=(leerCanales.valor(3));
  rc_car.consigna_rc[Rc_car::e_ch_rc_canal_4]=(leerCanales.valor(4));
  rc_car.consigna_rc[Rc_car::e_ch_rc_canal_5]=(leerCanales.valor(5));
  rc_car.consigna_rc[Rc_car::e_ch_rc_canal_6]=(leerCanales.valor(6));

}

void setup_pwmOut()
{
  //asignar pines de salida y inicializar canales
  servos[Rc_car::e_servo_rueda_derecha].attach(25,-1,-1,0,180,1000,2000);
  servos[Rc_car::e_servo_rueda_izquierda].attach(26,5000,-1,0,180,1000,2000);
  servos[Rc_car::e_servo_marcha].attach(27,-1,-1,0,180,1000,2000);
  servos[Rc_car::e_servo_motor].attach(14,-1,-1,0,180,1000,2000);

//TODO: borrar
  servos[4].attach(12,-1,-1,0,180,1000,2000);
}

void calcular_pwmOut()
{
  //servos[enum_rueda_derecha].write((100 + (35 * 1)) % 180);
  servos[Rc_car::e_servo_rueda_derecha].writePerCent(rc_car.consigna[Rc_car::e_servo_rueda_derecha]);
  servos[Rc_car::e_servo_rueda_izquierda].writePerCent(rc_car.consigna[Rc_car::e_servo_rueda_izquierda]);
  servos[Rc_car::e_servo_marcha].writePerCent(rc_car.consigna[Rc_car::e_servo_marcha]);
  servos[Rc_car::e_servo_motor].writePerCent(rc_car.consigna[Rc_car::e_servo_motor]);
  
    //TODO: borrar
  servos[4].writePerCent(rc_car.consigna_manual[Rc_car::e_servo_marcha]);
}

void setup()
{
  

  //Wire.begin(21,22,400000);// pines 21 y 22 400 kbit/sec I2C speed
   
  Serial.begin(115200); //usb

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
  rc_Telemetria.setBar(&barometro);
  rc_Telemetria.setDebug(SerialDebugTelemetria);


#if CON_VESC
  setupVesc();
#endif

#if CON_BLHELI
  setupBLHeli();
#endif

  setup_pwmIN(); //Configurar lectura de pwm de entrada usando rmt

  setup_pwmOut(); //configurar salidas PWM de control

#if Usar_INA219
  WireIna219.begin(pin_sda_ina, pin_scl_ina);
  ina219.begin(&WireIna219);
  //ina219.begin();
#endif

  //Leer configuracion de EEPROM
  rc_Configuracion.getFromEEPROM();

  Serial.println(F("Fin config"));
  Serial.printf("Internal Total heap %d, internal Free Heap %d\n",ESP.getHeapSize(),ESP.getFreeHeap());
  Serial.printf("SPIRam Total heap %d, SPIRam Free Heap %d\n",ESP.getPsramSize(),ESP.getFreePsram());
  Serial.printf("ChipRevision %d, Cpu Freq %d, SDK Version %s\n",ESP.getChipRevision(), ESP.getCpuFreqMHz(), ESP.getSdkVersion());
  Serial.printf(" Flash Size %d, Flash Speed %d\n",ESP.getFlashChipSize(), ESP.getFlashChipSpeed());
  //rc_car.consigna[Rc_car::e_servo_motor] = 100;        // consigna inicial del motor para pruebas
}

void loop()
{
  
#if CON_VESC
  vescControl();
#endif
#if CON_BLHELI
  BLHeliControl();
#endif

#if Usar_INA219
  ina219getValues();
#endif
  
  leer_pwmIN();

  if (SerialDebugPWMIN)
    leerCanales.debugOutSerial(&Serial);

  if (rc_mpu_init)
    imuGetValues();
 
  rc_car.calcular_consignas();

  calcular_pwmOut();

  rc_Telemetria.serialEvent2(); //comprueba si hay nuevo mensaje bluetooth y responde

   rc_Telemetria.autoTelemetria();

}
