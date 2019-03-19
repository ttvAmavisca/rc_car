using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.XInput;
using System.Threading;

namespace rc_car_config
{
    //clase para controlar joystic, implemetado Xinput, planeado Directinput si necesario
    class ControlConJoystick
    {
        public Controller[] controllers;
        public bool Habilitado { get; set; }
        public bool loop { get; set; }

        public ControlConJoystick()
        {
             controllers = new[] { new Controller(UserIndex.One), new Controller(UserIndex.Two), new Controller(UserIndex.Three), new Controller(UserIndex.Four) };
            loop = true;


        }
        
        public Controller GetFirstjoystick()
        {
            // Get 1st controller available
            Controller controller = null;
            foreach (var selectControler in controllers)
            {
                if (selectControler.IsConnected)
                {
                    controller = selectControler;
                    break;
                }
            }
            return controller;

        }

        public bool StartCaptureFirstJostick()
        {
           
            Thread thread2 = new Thread(new ThreadStart(LoopControlJoystick));
            thread2.Start();
            //LoopControlJoystick(controller);
            return true;
        }

        public void LoopControlJoystick()
        {
            while (loop)
            {
                Controller controller = GetFirstjoystick();
                // if (controller == null) return;
                if (controller != null)
                {
                    var previousState = controller.GetState();
                    while (controller.IsConnected & loop)
                    {
                        var state = controller.GetState();
                        if (previousState.PacketNumber != state.PacketNumber)
                            CambioEstadoJoystick(state);
                        Thread.Sleep(10);
                        previousState = state;
                    }
                }
                Thread.Sleep(1000);
            }
        }


        /******************************************************************/
        private void CambioEstadoJoystick(State es)
        {
            CambioEstadoJoystickArgs parametros = new CambioEstadoJoystickArgs();

            if (es.Gamepad.LeftTrigger > 0) //prioridad de frenada
            {
                parametros.Motor = Math.Max(-100f, Math.Min(100f, -100.0f * es.Gamepad.LeftTrigger / 255.0f));
            } else if (es.Gamepad.RightTrigger > 1)
            {
                parametros.Motor = Math.Max(-100f, Math.Min(100f, 100.0f * es.Gamepad.RightTrigger / 255.0f));
            }

           
            parametros.DireccionLX = Math.Max(-100f, Math.Min(100f, 100.0f * es.Gamepad.LeftThumbX / 32767.0f));
            parametros.DireccionLY = Math.Max(-100f, Math.Min(100f, 100.0f * es.Gamepad.LeftThumbY / 32767.0f));
            parametros.DireccionRX = Math.Max(-100f, Math.Min(100f, 100.0f * es.Gamepad.RightThumbX / 32767.0f));
            parametros.DireccionRY = Math.Max(-100f, Math.Min(100f, 100.0f * es.Gamepad.RightThumbY / 32767.0f));

            if (es.Gamepad.Buttons.HasFlag(GamepadButtonFlags.A))
            {
                parametros.Marcha = 100.0f;
            }
            if (es.Gamepad.Buttons.HasFlag(GamepadButtonFlags.B))
            {
                parametros.Marcha = -100.0f;
            }

            CambioEstadoJoystickevent?.Invoke(this, parametros);
        }

        public event EventHandler<CambioEstadoJoystickArgs> CambioEstadoJoystickevent;

        public class CambioEstadoJoystickArgs : EventArgs
        {
            public float Motor { get; set; }
            public float DireccionLY { get; set; }
            public float DireccionLX { get; set; }
            public float DireccionRY { get; set; }
            public float DireccionRX { get; set; }
            public float Marcha { get; set; }
        }


        /******************************************************************/



    }


}
