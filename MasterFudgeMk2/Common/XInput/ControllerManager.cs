﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterFudgeMk2.Common.XInput
{
    public static class ControllerManager
    {
        const int maxControllers = 4;

        static Controller[] controllers;

        static ControllerManager()
        {
            controllers = new Controller[maxControllers];
            for (int i = 0; i < controllers.Length; i++)
                controllers[i] = new Controller(i);
        }

        public static Controller GetController(int index)
        {
            if (index < 0 || index >= maxControllers) throw new Exception("Controller index out of range");
            return controllers[index];
        }

        public static void Update()
        {
            for (int i = 0; i < controllers.Length; i++)
                controllers[i].Update();
        }
    }
}
