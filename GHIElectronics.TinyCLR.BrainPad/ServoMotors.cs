using GHIElectronics.TinyCLR.Devices.Pwm;
using GHIElectronics.TinyCLR.Pins;
using System;
using System.ComponentModel;

namespace GHIElectronics.TinyCLR.BrainPad {
    public class ServoMotors {
        private PwmPin[] servos;
        //private bool started;
        // invert servos
        private bool[] invertServo;
        private double[] minPulseLength;
        private double[] maxPulseLength;
        private PwmController controller;

        public ServoMotors() {
            this.controller = PwmController.FromId(G30.PwmPin.Controller2.Id);
            this.invertServo = new bool[2]
            {
                false,
                false
            };

            this.minPulseLength = new[] { 1.0, 1.0 };
            this.maxPulseLength = new[] { 2.0, 2.0 };


            switch (Board.BoardType) {
                case BoardType.BP1:
                    this.servos = new[] {
                        this.controller.OpenPin(G30.PwmPin.Controller2.PA3),
                        this.controller.OpenPin(G30.PwmPin.Controller2.PA0)
                    };

                    break;

                case BoardType.Original:
                    this.servos = new[] {
                        this.controller.OpenPin(G30.PwmPin.Controller1.PA8),
                        this.controller.OpenPin(G30.PwmPin.Controller2.PA0)
                    };

                    break;

                default: throw new InvalidOperationException();
            }

            this.controller.SetDesiredFrequency(1 / 0.020);
            //output = new PWM(Peripherals.ServoMotor, 20000, 1250, PWM.ScaleFactor.Microseconds, false);
            //started = false;
        }
        // Use to calibrate the servo in use. Set it to the minimum pulse width the servo needs, in milliseconds.
        public double ServoOneMinimumPulseDuration { get => this.minPulseLength[0]; set => this.minPulseLength[0] = (value > 1.5 || value < 0.1) ? throw new ArgumentOutOfRangeException("Must be between 0.1 and 1.5ms") : value; }
        public double ServoTwoMinimumPulseDuration { get => this.minPulseLength[1]; set => this.minPulseLength[1] = (value > 1.5 || value < 0.1) ? throw new ArgumentOutOfRangeException("Must be between 0.1 and 1.5ms") : value; }
        public double ServoOneMaximumPulseDuration { get => this.maxPulseLength[0]; set => this.maxPulseLength[0] = (value > 3 || value < 1.6) ? throw new ArgumentOutOfRangeException("Must be between 0.1 and 1.5ms") : value; }
        public double ServoTwoMaximumPulseDuration { get => this.maxPulseLength[1]; set => this.maxPulseLength[1] = (value > 3 || value < 1.6) ? throw new ArgumentOutOfRangeException("Must be between 0.1 and 1.5ms") : value; }

        /// <summary>
        /// Inverts a servo's behavior.
        /// </summary>
        /// <param name="servo">The servo to be inverted.</param>
        public bool IsServoOneInverted { get => this.invertServo[0]; set => this.invertServo[0] = value; }
        public bool IsServoTwoInverted { get => this.invertServo[1]; set => this.invertServo[1] = value; }

        /// <summary>
        /// Sets the position of a fixed-type Servo Motor.
        /// </summary>
        /// <param name="position">The position of the servo between 0 and 180 degrees.</param>
        public void SetServoOnePosition(double position) => this.FixedSetPosition(0, position);
        public void SetServoTwoPosition(double position) => this.FixedSetPosition(1, position);

        private void FixedSetPosition(int servo, double position) {
            if (position < 0 || position > 180) throw new ArgumentOutOfRangeException("degrees", "degrees must be between 0 and 180.");


            this.controller.SetDesiredFrequency(1 / 0.020);// in case we used the other stuff. remove when we fix PWM controllers

            if (this.invertServo[(int)servo] == true)
                position = 180 - position;

            // Typically, with 50 hz, 0 degree is 0.05 and 180 degrees is 0.10
            //double duty = ((position / 180.0) * (0.10 - 0.05)) + 0.05;
            var duty = ((position / 180.0) * (this.maxPulseLength[servo] / 20 - this.minPulseLength[servo] / 20)) + this.minPulseLength[servo] / 20;


            this.servos[(int)servo].SetActiveDutyCyclePercentage(duty);
            this.servos[(int)servo].Start();
        }

        /// <summary>
        /// Sets the position of a continous-type Servo Motor.
        /// </summary>
        /// <param name="speed">The speed of the servo between -100 and 100 percent.</param>
        public void SetServoOneSpeed(int speed) => this.ContiniousSetSpeed(0, speed);
        public void SetServoTwoSpeed(int speed) => this.ContiniousSetSpeed(1, speed);

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

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => base.Equals(obj);
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => base.GetHashCode();
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => base.ToString();
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType() => base.GetType();
    }
}
