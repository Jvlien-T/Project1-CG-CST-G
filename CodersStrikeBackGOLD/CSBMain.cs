using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace CodersStrikeBackGOLD
{
    struct Coordinates
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    static class CSBCompute
    {

    }

    class CSBPod
    {
        private Coordinates p_myprevpos;
        private Coordinates p_mypos;
        private Coordinates p_myspeed;
        private int p_myangle;
        private int p_mynextCPID;

        public void Update(int myposX, int myposY, int myspeedX, int myspeedY, int myangle, int mynextCPID)
        {
            p_myprevpos.X = p_mypos.X == 0 ? myposX : p_mypos.X;
            p_myprevpos.Y = p_mypos.Y == 0 ? myposY : p_mypos.Y;
            p_mypos.X = myposX;
            p_mypos.Y = myposY;
            p_myspeed.X = myspeedX;
            p_myspeed.Y = myspeedY;
            p_myangle = myangle;
            p_mynextCPID = mynextCPID;
        }
    }

    class CSBCheckPoint
    {
        private int cp_id;
        private Coordinates Position;

    }

    class CSBTrack
    {
        public int LapsNumber { get; }
        public int CPNumber { get; }
        // + déclaration d'un array contenant les CP.

        public CSBTrack (int in1, int in2)
        {
            LapsNumber = in1;
            CPNumber = in2;
        }
    }

    static class CSBProgram
    {
        static void Main(string[] args)
        {
            string[] inputs;
            CSBPod PodMyG = new CSBPod();
            CSBPod PodMyH = new CSBPod();
            CSBPod PodHisG = new CSBPod();
            CSBPod PodHisH = new CSBPod();

            int laps = int.Parse(Console.ReadLine());
            int checkpointCount = int.Parse(Console.ReadLine());
            CSBTrack FullTrack = new CSBTrack(laps, checkpointCount);

            for (int i = 0; i < FullTrack.LapsNumber; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int checkpointX = int.Parse(inputs[0]);
                int checkpointY = int.Parse(inputs[1]);

            }

            // game loop
            while (true)
            {
                for (int i = 0; i < 4; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    int x = int.Parse(inputs[0]); // x Coordinates of pod
                    int y = int.Parse(inputs[1]); // y Coordinates of pod
                    int vx = int.Parse(inputs[2]); // x speed of pod
                    int vy = int.Parse(inputs[3]); // y speed of pod
                    int angle = int.Parse(inputs[4]); // angle of pod
                    int nextCheckPointId = int.Parse(inputs[5]); // next check point id of pod

                }

                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");
                // You have to output the target Coordinates
                // followed by the power (0 <= thrust <= 100)
                // i.e.: "x y thrust"
                Console.WriteLine("8000 4500 100");
                Console.WriteLine("8000 4500 100");
            }
        }
    }
}
