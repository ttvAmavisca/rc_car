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
//Libreria MPU9250
#include "src/MPU9250_DMP/MPU9250-DMP.h"

//Libreria VESC
#include "src/VescUart/VescUart.h"

//Libreria Bluetooth ESP32
#include "BluetoothSerial.h"

//periferico rmt para procesado ppm
#include <driver/rmt.h>
 


#if !defined(CONFIG_BT_ENABLED) || !defined(CONFIG_BLUEDROID_ENABLED)
#error Bluetooth is not enabled! Please run `make menuconfig` to and enable it
#endif

BluetoothSerial SerialBT;


#define SerialDebug false  // Set to true to get Serial output for debugging
#define SerialDebugVESC false  // Set to true to get Serial output for debugging
#define OUTPUT_TEAPOT false  // paquete teapot para ejemplo de fabricante

// Pin definitions


bool rc_mpu_init = false;
#define SerialPort Serial

MPU9250_DMP imu;

/** Initiate VescUart class */

VescUart UART;


void setupBluetooth()
{
  SerialBT.begin("RC_Car_telemetr,y"); //Bluetooth device name
  Serial.println("bluetooth iniciado");
}


String command;
int throttle = 127;
HardwareSerial pSerial2(2);

void setupVesc()
{

  pSerial2.begin(115200, SERIAL_8N1, 16, 17);


  while (!pSerial2) {
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
  if (UART.getVescValues() > 0 ) {
    if (SerialDebug)
    {
      Serial.print("input V = ");  Serial.println(UART.data.inpVoltage);
    }
  }
}

void setupMPU9250()
{
  // Call imu.begin() to verify communication and initialize
  if (imu.begin() != INV_SUCCESS)
  {
    rc_mpu_init = false;
    SerialPort.println("Unable to communicate with MPU-9250");
    SerialPort.println("Check connections, and try again.");
    SerialPort.println();
    delay(5000);

  }

  imu.dmpBegin(DMP_FEATURE_6X_LP_QUAT | // Enable 6-axis quat
               DMP_FEATURE_GYRO_CAL, // Use gyro calibration
               100); // Set DMP FIFO rate to 10 Hz
  // DMP_FEATURE_LP_QUAT can also be used. It uses the
  // accelerometer in low-power mode to estimate quat's.
  // DMP_FEATURE_LP_QUAT and 6X_LP_QUAT are mutually exclusive

  rc_mpu_init = true;
}

void imuGetValues()
{
  if ( imu.fifoAvailable() )
  {
    // Use dmpUpdateFifo to update the ax, gx, mx, etc. values
    if ( imu.dmpUpdateFifo() == INV_SUCCESS)
    {
      // computeEulerAngles can be used -- after updating the
      // quaternion values -- to estimate roll, pitch, and yaw
      imu.computeEulerAngles();

      if (SerialDebug)
      {
        float q0 = imu.calcQuat(imu.qw);
        float q1 = imu.calcQuat(imu.qx);
        float q2 = imu.calcQuat(imu.qy);
        float q3 = imu.calcQuat(imu.qz);

        SerialPort.println("Q: " + String(q0, 4) + ", " +
                           String(q1, 4) + ", " + String(q2, 4) +
                           ", " + String(q3, 4));
        SerialPort.println("R/P/Y: " + String(imu.roll) + ", "
                           + String(imu.pitch) + ", " + String(imu.yaw));
        SerialPort.println("Time: " + String(imu.time) + " ms");
        SerialPort.println();
      }


#ifdef OUTPUT_TEAPOT
      float q0 = imu.calcQuat(imu.qw)* 16384.0f;
      float q1 = imu.calcQuat(imu.qx)* 16384.0f;
      float q2 = imu.calcQuat(imu.qy)* 16384.0f;
      float q3 = imu.calcQuat(imu.qz)* 16384.0f;

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
      if(OUTPUT_TEAPOT) Serial.write(teapotPacket, 14);
      teapotPacket[11]++; // packetCount, loops at 0xFF on purpose
#endif
    }
  }
}



// use 13 bit precission for LEDC timer
#define LEDC_TIMER_13_BIT  13

// use 5000 Hz as a LEDC base frequency
#define LEDC_BASE_FREQ     1000


int brightness = 0;    // how bright the LED is
int fadeAmount = 5;    // how many points to fade the LED by
int omega=0;
int omegalul=0;


static uint32_t *s_channels;
static uint32_t channels[16];
rmt_obj_t* rmt_recv = NULL;
unsigned long debug_print_lul =0;

extern "C" void receive_data(uint32_t *data, size_t len)
{
    parseRmt((rmt_data_t*) data, len, channels);
}


void parseRmt(rmt_data_t* items, size_t len, uint32_t* channels){
    size_t chan = 0;
    bool valid = true;
    rmt_data_t* it = NULL;

    if (!channels)  {
        return;
    }
    s_channels = channels;

    it = &items[0];
    omegalul=len;
    for(size_t i = 0; i<len; i++){

        if(!valid){
            break;
        }
        it = &items[i];
        
        omega=it->duration1;
        /*
        if(XJT_VALID(it)){
            if(it->duration1 >= 5 && it->duration1 <= 8){
                valid = xjtReceiveBit(i, false);
            } else if(it->duration1 >= 13 && it->duration1 <= 16){
                valid = xjtReceiveBit(i, true);
            } else {
                valid = false;
            }
        } else if(!it->duration1 && !it->level1 && it->duration0 >= 5 && it->duration0 <= 8) {
                    valid = xjtReceiveBit(i, false);

        }
        */
    }
}

void setup_rmt() {
  if ((rmt_recv = rmtInit(32, false, RMT_MEM_64)) == NULL)
    {
         if (SerialDebug) Serial.println("init receiver failed\n");
    }

    // Setup 1us tick
    float realTick = rmtSetTick(rmt_recv, 1000);
     if (SerialDebug) Serial.printf("real tick set to: %fns\n", realTick);

    // Ask to start reading
    rmtRead(rmt_recv, receive_data);


 //salida pwm prueba
  ledcSetup(0, LEDC_BASE_FREQ, LEDC_TIMER_13_BIT);
  ledcAttachPin(35, 0);
}

void rmt_update_values(){
  //
   ledcWrite(0, brightness);
  brightness = brightness + fadeAmount;

  // reverse the direction of the fading at the ends of the fade:
  if (brightness <= 0 || brightness >= 255) {
    fadeAmount = -fadeAmount;
  }

  if(millis() > debug_print_lul) {
          debug_print_lul = millis() +200;
          Serial.print("lul "); Serial.print(omega);Serial.print(" || "); Serial.println(omegalul);
        }
//  Serial.print("lul wat "); Serial.print(it->duration1); Serial.println("trisca");
}
 


void setup()
{
  Wire.begin();
  // TWBR = 12;  // 400 kbit/sec I2C speed
  Serial.begin(115200);

  while (!Serial) {};

  setupMPU9250();

  //setupBluetooth();

  setupVesc();

  setup_rmt();
  
  
}





void loop()
{
 

  vescControl();

  if (rc_mpu_init) imuGetValues();

  rmt_update_values();

}
