#include "rc_car.h"


Rc_car::Rc_car(void){
	angulos[0]=1;
  angulos[1]=2;
  angulos[2]=3;
  angulos[3]=4;
  pitch=5;
  roll=6;
  yaw=7;
  rpmActual=8;
  
  
  giro_val[0]=13;
  giro_val[1]=14;
  giro_val[2]=15;
  aceleracion[0]=16;
  aceleracion[1]=17;
  aceleracion[2]=18;
  modo_motor_actual=0;
  tipo_control_actual=0;
  tipo_regulacion_direccion=0;
  tipo_regulacion_potencia=0;

  
  ESC_Dutycycle=21;
  ESC_avgInputCurrent=22;
  ESC_avgMotorCurrent=23;
  ESC_rpmActual=24;
  ESC_VoltajeEntrada=25;

  consigna[e_servo_rueda_derecha]=9;
  consigna[e_servo_rueda_izquierda]=10;
  consigna[e_servo_marcha]=11;
  consigna[e_servo_motor]=12;

  consigna_manual[e_servo_rueda_derecha]=0.0f;
  consigna_manual[e_servo_rueda_izquierda]=0.0f;
  consigna_manual[e_servo_marcha]=0.0f;
  consigna_manual[e_servo_motor]=0.0f;

  consigna_rc[e_servo_rueda_derecha]=26;
  consigna_rc[e_servo_rueda_izquierda]=27;
  consigna_rc[e_servo_marcha]=28;
  consigna_rc[e_servo_motor]=29;

  estado_arranque=0;
  estado_auto=0;
	estado_semi_auto=0;

  arranque_aceleracion_fase_1 =1;
  arranque_tmax_fase_1 =1000000;
  arranque_Vmax_fase_1 =59;
	arranque_aceleracion_fase_2 =1;
  arranque_tmax_fase_2 =1000000;
  arranque_Vmax_fase_2 =59;
	arranque_aceleracion_fase_3 =1;
  arranque_tmax_fase_3 =1000000;
  arranque_Vmax_fase_3 =1;

}

void Rc_car::calcular_consignas()
{

//si se pierde la conexion poner a 0
  if(esp_timer_get_time() >last_manual_received) consigna_manual[Rc_car::e_servo_motor] =0;
  if(esp_timer_get_time() >last_rc_received) consigna_rc[Rc_car::e_servo_motor] =0;

switch (tipo_control_actual)
  {
    case e_control_RC:
      //consigna_control[e_servo_rueda_derecha] =consigna_rc[Rc_car::e_ch_rc_canal_1];
      //consigna_control[e_servo_rueda_izquierda] =consigna_rc[Rc_car::e_ch_rc_canal_1];
      consigna_control[e_servo_direccion] =consigna_rc[Rc_car::e_ch_rc_canal_1];
      consigna_control[e_servo_marcha] =consigna_rc[Rc_car::e_ch_rc_canal_3];
      consigna_control[e_servo_motor] = consigna_rc[Rc_car::e_ch_rc_canal_2] ;
      break;
    case e_control_BT:
      //[Rc_car::e_servo_rueda_derecha] =consigna_manual[Rc_car::e_servo_rueda_derecha];
      //consigna_control[Rc_car::e_servo_rueda_izquierda] =consigna_manual[Rc_car::e_servo_rueda_izquierda];
      consigna_control[Rc_car::e_servo_direccion] =consigna_manual[Rc_car::e_servo_rueda_derecha];
      consigna_control[Rc_car::e_servo_marcha] =consigna_manual[Rc_car::e_servo_marcha];
      consigna_control[Rc_car::e_servo_motor] = consigna_manual[Rc_car::e_servo_motor] ;
      break;
  }

  //motor
  switch (modo_motor_actual)
  {
  case Rc_car::e_modo_manual: //modo de control manual directo sobre todo los motores
    consigna[Rc_car::e_servo_rueda_derecha] =consigna_manual[Rc_car::e_servo_rueda_derecha];
    consigna[Rc_car::e_servo_rueda_izquierda] =consigna_manual[Rc_car::e_servo_rueda_izquierda];
    consigna[Rc_car::e_servo_marcha] =consigna_manual[Rc_car::e_servo_marcha];
    consigna[Rc_car::e_servo_motor] = consigna_manual[Rc_car::e_servo_motor];
    break;
  case Rc_car::e_modo_sistema_salida: //sistema de salida
    Modo_salida();
    break;
  case Rc_car::e_modo_semi_auto: //modo de cambio manual
    modo_semi_auto();
    break;
  case Rc_car::e_modo_full_auto: //modo de cambio automatico
    modo_auto();
    break;
  }

  if ( modo_motor_actual !=Rc_car::e_modo_manual){
    switch(tipo_regulacion_direccion){
      case Rc_car::e_reg_dir_directa: //Directa, se pasa a las 2 ruedas el mismo valor(rueda derecha)
          consigna[Rc_car::e_servo_rueda_derecha] = consigna_control[Rc_car::e_servo_rueda_derecha];
          consigna[Rc_car::e_servo_rueda_izquierda] = consigna_control[Rc_car::e_servo_rueda_derecha];//consigna_control[Rc_car::e_servo_rueda_izquierda];  
      break;
      case Rc_car::e_reg_dir_Ackermann:
          calculateAckerman();
      break;
    }
  }
}

void Rc_car::modo_semi_auto()
{
 
  consigna[Rc_car::e_servo_marcha] = consigna_control[Rc_car::e_servo_marcha];
  consigna[Rc_car::e_servo_motor] = consigna_control[Rc_car::e_servo_motor];
}

void Rc_car::modo_auto()
{
  consigna[Rc_car::e_servo_rueda_derecha] = consigna_control[Rc_car::e_servo_rueda_derecha];
  consigna[Rc_car::e_servo_rueda_izquierda] = consigna_control[Rc_car::e_servo_rueda_izquierda];
  consigna[Rc_car::e_servo_marcha] = consigna_control[Rc_car::e_servo_marcha];
  consigna[Rc_car::e_servo_motor] = consigna_control[Rc_car::e_servo_motor];
}

void Rc_car::calculateAckerman()
{
  consigna[Rc_car::e_servo_rueda_derecha] = consigna_control[Rc_car::e_servo_rueda_derecha];
  consigna[Rc_car::e_servo_rueda_izquierda] = consigna_control[Rc_car::e_servo_rueda_derecha];//consigna_control[Rc_car::e_servo_rueda_izquierda]; 
}


bool Rc_car::Cambiar_modo(int nuevo_modo )
{

  switch (modo_motor_actual)
  {
  case Rc_car::e_modo_manual:
    
    break;
  case Rc_car::e_modo_sistema_salida:
    
    break;
  case Rc_car::e_modo_semi_auto:
    
    break;
  case Rc_car::e_modo_full_auto:
    
    break;
  }
  
  modo_motor_actual =nuevo_modo;
  return true;
}

void Rc_car::Modo_salida()
{
  int64_t tiempo_Fase;

//si se suelta el acelerador parar
    if (consigna_control[Rc_car::e_servo_motor] < 0.1){
        estado_arranque =0; //cambiar a etapa 1 al detectar acelerador
    }
    
  //motor
  switch (estado_arranque)
  {
  //estado inicial
  case 0:
    //Esperando acelerador
    consigna[Rc_car::e_servo_marcha] = -100.0;
    consigna[Rc_car::e_servo_motor] = 0.0;
    if (consigna_control[Rc_car::e_servo_motor] > 0.5) {
        estado_arranque =1; //cambiar a etapa 1 al detectar acelerador
        t_arranque=esp_timer_get_time();
    }
    break;
  case 1:
    consigna[Rc_car::e_servo_marcha] = 0.0; //enum_marcha
   
     tiempo_Fase = esp_timer_get_time() -t_arranque;
    consigna[Rc_car::e_servo_motor] += (tiempo_Fase /1000000.f) * arranque_aceleracion_fase_1 ;

    
    if ( ESC_rpmActual > arranque_Vmax_fase_1 || tiempo_Fase > arranque_tmax_fase_1) {
        estado_arranque =2; //cambiar a etapa 
        t_arranque=esp_timer_get_time();
    }

    break;
  case 2:
    consigna[Rc_car::e_servo_marcha] = 100.0; 
   
     tiempo_Fase = esp_timer_get_time() -t_arranque;
    consigna[Rc_car::e_servo_motor] += (tiempo_Fase /1000000.f) * arranque_aceleracion_fase_2 ;

    
    if ( ESC_rpmActual > arranque_Vmax_fase_2 || tiempo_Fase > arranque_tmax_fase_2) {
        estado_arranque =3; //cambiar a etapa 
        t_arranque=esp_timer_get_time();
    }
    break;
  case 3:
    consigna[Rc_car::e_servo_marcha] = 100.0; 
   
     tiempo_Fase = esp_timer_get_time() -t_arranque;
    consigna[Rc_car::e_servo_motor] += (tiempo_Fase /1000000.f) * arranque_aceleracion_fase_3 ;

    
    if ( ESC_rpmActual > arranque_Vmax_fase_3 || tiempo_Fase > arranque_tmax_fase_3) {
        Cambiar_modo(Rc_car::e_modo_semi_auto);
    }
    break;
    
  }
}

