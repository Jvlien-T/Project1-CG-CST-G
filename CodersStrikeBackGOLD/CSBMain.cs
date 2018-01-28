﻿using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace CodersStrikeBackGOLD
{
    static class CSBCompute
    {

    }

    class CSBPod
    {
        private int p_myprevposX;
        private int p_myprevposY;
        private int p_myposX;
        private int p_myposY;
        private int p_myspeedX;
        private int p_myspeedY;
        private int p_myangle;
        private int p_mynextCPID;

        public CSBPod (int myposX, int myposY, int myspeedX, int myspeedY, int myangle, int mynextCPID)
        {
            p_myprevposX = myposX;
            p_myprevposY = myposY;
            p_myposX = myposX;
            p_myposY = myposY;
            p_myspeedX = myspeedX;
            p_myspeedY = myspeedY;
            p_myangle = myangle;
            p_mynextCPID = mynextCPID;
        }

        public void Update(int myposX, int myposY, int myspeedX, int myspeedY, int myangle, int mynextCPID)
        {
            p_myprevposX = p_myposX;
            p_myprevposY = p_myposY;
            p_myposX = myposX;
            p_myposY = myposY;
            p_myspeedX = myspeedX;
            p_myspeedY = myspeedY;
            p_myangle = myangle;
            p_mynextCPID = mynextCPID;
        }
    }

    class CSBTrack
    {

    }

    static class CSBProgram
    {
        static void Main(string[] args)
        {
            string[] inputs;
            int laps = int.Parse(Console.ReadLine());
            int checkpointCount = int.Parse(Console.ReadLine());
            for (int i = 0; i < checkpointCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int checkpointX = int.Parse(inputs[0]);
                int checkpointY = int.Parse(inputs[1]);
            }

            // game loop
            while (true)
            {
                for (int i = 0; i < 2; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    int x = int.Parse(inputs[0]); // x position of your pod
                    int y = int.Parse(inputs[1]); // y position of your pod
                    int vx = int.Parse(inputs[2]); // x speed of your pod
                    int vy = int.Parse(inputs[3]); // y speed of your pod
                    int angle = int.Parse(inputs[4]); // angle of your pod
                    int nextCheckPointId = int.Parse(inputs[5]); // next check point id of your pod
                }
                for (int i = 0; i < 2; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    int x2 = int.Parse(inputs[0]); // x position of the opponent's pod
                    int y2 = int.Parse(inputs[1]); // y position of the opponent's pod
                    int vx2 = int.Parse(inputs[2]); // x speed of the opponent's pod
                    int vy2 = int.Parse(inputs[3]); // y speed of the opponent's pod
                    int angle2 = int.Parse(inputs[4]); // angle of the opponent's pod
                    int nextCheckPointId2 = int.Parse(inputs[5]); // next check point id of the opponent's pod
                }

                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");


                // You have to output the target position
                // followed by the power (0 <= thrust <= 100)
                // i.e.: "x y thrust"
                Console.WriteLine("8000 4500 100");
                Console.WriteLine("8000 4500 100");
            }
        }
    }
}
