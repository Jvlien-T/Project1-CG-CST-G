using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace CodersStrikeBackGOLD
{
    enum SectionLengthType { Close, Average, Far };
    enum CurveStrengthType { Open, Medium, Hairpin };
    struct Coordinates
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    static class CSBCompute
    {
        // fonction qui nous donne la distance entre deux points A et B :
        static double DistAB(Coordinates A, Coordinates B) { return Math.Sqrt(Math.Pow(Math.Abs(A.X - B.X), 2) + Math.Pow(Math.Abs(A.Y - B.Y), 2)); }

        // fonction qui nous donne une idée de la distance qui sépare deux points A et B :
        static SectionLengthType DistBetTwoPoints(Coordinates A, Coordinates B)
        {
            double Dist = DistAB(A, B);
            if (Dist > 8000) { return SectionLengthType.Far; }
            else if (Dist > 3000) { return SectionLengthType.Average; }
            else { return SectionLengthType.Close; }
        }

        // fonction qui nous donne l'angle ACB
        static double AngleACB(Coordinates A, Coordinates B, Coordinates C)
        {
            return Math.Acos((Math.Pow(DistAB(B, C), 2) + Math.Pow(DistAB(A, C), 2) - Math.Pow(DistAB(A, B), 2)) / (2 * DistAB(B, C) * DistAB(A, C)));
        }

        // fonction qui nous donne une idée de l'angle entre trois points ACB
        static CurveStrengthType AngleBetThreePoints(Coordinates A, Coordinates B, Coordinates C)
        {
            double Angle = Math.Abs(AngleACB(A, B, C));
            if (Angle > 120) { return CurveStrengthType.Open; }
            else if (Angle > 60) { return CurveStrengthType.Medium; }
            else { return CurveStrengthType.Hairpin; }
        }

        //fonction qui nous donne la distance a laquelle on devrait passer au plus proche du prochain WP
        static double ClosestFromNxtWP(Coordinates PreviousPos, Coordinates CurrentPos, Coordinates CPPos, double NextCPDistance)
        {
            double DriftAngle = AngleACB(CurrentPos, CPPos, PreviousPos);
            double AbsGap = NextCPDistance * Math.Tan(DriftAngle);
            if (NextCPDistance < 900 && AbsGap > 600) { AbsGap = -1; }
            return AbsGap;
        }
    }

    class CSBPod
    {
        private Coordinates p_myprevpos;
        private Coordinates p_mypos;
        private Coordinates p_myspeed;
        private int p_myangle;
        private int p_mynextCPID;

        public CSBPod()
        {
            p_myprevpos.X = 0;
            p_myprevpos.Y = 0;
            p_mypos.X = 0;
            p_mypos.Y = 0;
            p_myspeed.X = 0;
            p_myspeed.Y = 0;
            p_myangle = 0;
            p_mynextCPID = 0;
        }
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
        public void AddCheckPoint (int X, int Y)
        {
            ;
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
                FullTrack.AddCheckPoint(checkpointX, checkpointY);
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
