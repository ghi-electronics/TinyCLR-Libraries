using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Devices.Pwm;
using GHIElectronics.TinyCLR.Pins;
using System;

namespace GHIElectronics.TinyCLR.BrainPad {
    public class ServoMotors {
        private PwmPin[] servos;
        //private bool started;
        // min and max pulse width in milliseconds
        private double _MinPulseCalibration = 1.0;
        private double _MaxPulseCalibration = 2.0;
        // invert servos
        private bool[] invertServo;
        private PwmController controller;

        public ServoMotors() {
            this.controller = PwmController.FromId(G30.PwmPin.Controller2.Id);
            this.invertServo = new bool[2]
            {
                false,
                false
            };

            var PC1 = GpioController.GetDefault().OpenPin(G30.GpioPin.PC1);
            PC1.SetDriveMode(GpioPinDriveMode.InputPullDown);
            if (PC1.Read() == GpioPinValue.High) {
                // new brainpad
                this.servos = new PwmPin[2]
                {
                    this.controller.OpenPin(G30.PwmPin.Controller2.PA3),
                    this.controller.OpenPin(G30.PwmPin.Controller2.PA0)
                };
            }
            else {
                // old brainpad
                this.servos = new PwmPin[2]
               {
                    this.controller.OpenPin(G30.PwmPin.Controller1.PA8),
                    this.controller.OpenPin(G30.PwmPin.Controller2.PA0)
               };
            }


            this.controller.SetDesiredFrequency(1 / 0.020);
            //output = new PWM(Peripherals.ServoMotor, 20000, 1250, PWM.ScaleFactor.Microseconds, false);
            //started = false;
        }
        // Use to calibrate the servo in use. Set it to the minimum pulse width the servo needs, in milliseconds.
        public double MinPulseCalibration {
            set {
                if (value > 1.5 || value < 0.1)
                    throw new ArgumentOutOfRangeException("Must be between 0.1 and 1.5ms");
                this._MinPulseCalibration = value;
            }
        }
        // Use to calibrate the servo in use. Set it to the maximum pulse width the servo needs, in milliseconds.
        public double MaxPulseCalibration {
            set {
                if (value > 3 || value < 1.6)
                    throw new ArgumentOutOfRangeException("Must be between 1.6 and 3ms");
                this._MaxPulseCalibration = value;
            }
        }
        /// <summary>
        /// Inverts a servo's behavior.
        /// </summary>
        /// <param name="servo">The servo to be inverted.</param>
        public void InvertServoOne(bool invert) => this.invertServo[0] = invert;
        public void InvertServoTwo(bool invert) => this.invertServo[1] = invert;

        /// <summary>
        /// Sets the position of a fixed-type Servo Motor.
        /// </summary>
        /// <param name="position">The position of the servo between 0 and 180 degrees.</param>
        public void FixedSetPositionServoOne(double position) => this.FixedSetPosition(0, position);
        public void FixedSetPositionServoTwo(double position) => this.FixedSetPosition(1, position);

        private void FixedSetPosition(int servo, double position) {
            if (position < 0 || position > 180) throw new ArgumentOutOfRangeException("degrees", "degrees must be between 0 and 180.");


            this.controller.SetDesiredFrequency(1 / 0.020);// in case we used the other stuff. remove when we fix PWM controllers

            if (this.invertServo[(int)servo] == true)
                position = 180 - position;

            // Typically, with 50 hz, 0 degree is 0.05 and 180 degrees is 0.10
            //double duty = ((position / 180.0) * (0.10 - 0.05)) + 0.05;
            var duty = ((position / 180.0) * (this._MaxPulseCalibration / 20 - this._MinPulseCalibration / 20)) + this._MinPulseCalibration / 20;


            this.servos[(int)servo].SetActiveDutyCyclePercentage(duty);
            this.servos[(int)servo].Start();
        }

        /// <summary>
        /// Sets the position of a continous-type Servo Motor.
        /// </summary>
        /// <param name="speed">The speed of the servo between -100 and 100 percent.</param>
        public void ContiniousSetSpeedServoOne(int speed) => this.ContiniousSetSpeed(0, speed);
        public void ContiniousSetSpeedServoTwo(int speed) => this.ContiniousSetSpeed(1, speed);

        private void ContiniousSetSpeed(int servo, int speed) {
            if (speed < -100 || speed > 100) throw new ArgumentOutOfRangeException("speed", "degrees must be between -100 and 100.");

            speed += 100;
            var d = speed / 200.0 * 180;
            FixedSetPosition(servo, (int)d);
        }


        /// <summary>
        /// Stops the servo motor.
        /// </summary>
        public void StopServoOne() => this.servos[0].Stop();
        public void StopServoTwo() => this.servos[1].Stop();
    }
}
