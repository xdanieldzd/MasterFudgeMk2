﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

using SharpDX.DirectInput;

using MasterFudgeMk2.Common.EventArguments;

namespace MasterFudgeMk2.InputBackends
{
    // TODO: config storage (*probably* ok as-is?), PoV switch directions (ugggghhhhhhhh), blah blah blah

    [Description("DirectInput (SharpDX)")]
    public class DirectInputBackend : IInputBackend
    {
        DirectInput directInput;

        Keyboard keyboard;
        Joystick joystick;

        public DirectInputBackend()
        {
            directInput = new DirectInput();

            keyboard = new Keyboard(directInput);
            if (keyboard != null)
            {
                keyboard.Properties.BufferSize = 128;
                keyboard.Acquire();
            }

            var joystickDevices = directInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly);
            if (joystickDevices.Count > 0)
            {
                var joystickGuid = joystickDevices.FirstOrDefault().InstanceGuid;
                joystick = new Joystick(directInput, joystickGuid);
                if (joystick != null)
                {
                    joystick.Properties.BufferSize = 128;
                    joystick.Properties.DeadZone = 2000;
                    joystick.Acquire();
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (joystick != null)
                {
                    joystick.Unacquire();
                    joystick.Dispose();
                }

                if (keyboard != null)
                {
                    keyboard.Unacquire();
                    keyboard.Dispose();
                }

                if (directInput != null)
                {
                    directInput.Dispose();
                }
            }
        }

        public IEnumerable<Enum> GetInputValues()
        {
            List<Enum> values = new List<Enum>();
            values.AddRange(Enum.GetValues(typeof(JoystickOffset)).Cast<Enum>());
            values.AddRange(Enum.GetValues(typeof(Key)).Cast<Enum>());
            return values;
        }

        public string GetInputDescription(Enum input, int value)
        {
            string description = string.Empty;

            try
            {
                DeviceObjectInstance objectInstance;

                if (input.GetType() == typeof(Key))
                    objectInstance = keyboard?.GetObjectInfoByName(Enum.GetName(typeof(Key), input));
                else if (input.GetType() == typeof(JoystickOffset))
                    objectInstance = joystick?.GetObjectInfoByName(Enum.GetName(typeof(JoystickOffset), input));
                else
                    throw new Exception("Trying to get description from unhandled enum");

                if (objectInstance != null)
                    description = objectInstance.Name;
            }
            catch (SharpDX.SharpDXException e)
            {
                // 0x80070002 (ERROR_FILE_NOT_FOUND) means we're trying to get infos for an object the input device doesn't support, otherwise rethrow the exception
                if (e.HResult != -2147024894)
                    throw e;
            }

            return description;
        }

        public void OnPollInput(object sender, PollInputEventArgs e)
        {
            var inputs = new List<Enum>();

            if (keyboard != null)
            {
                keyboard.Poll();
                var keyboardUpdates = keyboard.GetBufferedData();
                inputs.AddRange(keyboardUpdates.Where(x => x.IsPressed).Select(x => (Enum)x.Key));
            }

            if (joystick != null)
            {
                joystick.Poll();
                var joystickUpdates = joystick.GetBufferedData();
                inputs.AddRange(joystickUpdates.Select(x => (Enum)x.Offset));
            }

            e.Pressed = inputs;




            var tmpx = inputs.FirstOrDefault(x => x is JoystickOffset);
            if (tmpx != null)
            {
                bool tmpxx = false;
            }
        }
    }
}
