using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace CodersStrikeBackGOLD
{
    public enum SectionLengthType { Close, Average, Far };
    public enum CurveStrengthType { Open, Medium, Hairpin };
    public class Coordinates
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    static public class CSBCompute
    {
        // fonction qui nous donne la distance entre deux points A et B :
        static public double DistAB(Coordinates A, Coordinates B) { return Math.Sqrt(Math.Pow(Math.Abs(A.X - B.X), 2) + Math.Pow(Math.Abs(A.Y - B.Y), 2)); }

        // fonction qui nous donne une idée de la distance qui sépare deux points A et B :
        static public SectionLengthType DistBetTwoPoints(Coordinates A, Coordinates B)
        {
            double Dist = DistAB(A, B);
            if (Dist > 8000) { return SectionLengthType.Far; }
            else if (Dist > 3000) { return SectionLengthType.Average; }
            else { return SectionLengthType.Close; }
        }

        // fonction qui nous donne l'angle ACB
        static public double AngleACB(Coordinates A, Coordinates B, Coordinates C)
        {
            return Math.Acos((Math.Pow(DistAB(B, C), 2) + Math.Pow(DistAB(A, C), 2) - Math.Pow(DistAB(A, B), 2)) / (2 * DistAB(B, C) * DistAB(A, C)));
        }

        // fonction qui nous donne une idée de l'angle entre trois points ACB
        static public CurveStrengthType AngleBetThreePoints(Coordinates A, Coordinates B, Coordinates C)
        {
            double Angle = Math.Abs(AngleACB(A, B, C));
            if (Angle > 120) { return CurveStrengthType.Open; }
            else if (Angle > 60) { return CurveStrengthType.Medium; }
            else { return CurveStrengthType.Hairpin; }
        }

        //fonction qui nous donne la distance a laquelle on devrait passer au plus proche du prochain WP
        static public double ClosestFromNxtWP(Coordinates PreviousPos, Coordinates CurrentPos, Coordinates CPPos, double NextCPDistance)
        {
            double DriftAngle = AngleACB(CurrentPos, CPPos, PreviousPos);
            double AbsGap = NextCPDistance * Math.Tan(DriftAngle);
            if (NextCPDistance < 900 && AbsGap > 600) { AbsGap = -1; }
            return AbsGap;
        }
    }

    class CSBPod
    {
        private Coordinates p_myprevpos = new Coordinates();
        private Coordinates p_mypos = new Coordinates();
        private Coordinates p_myspeed = new Coordinates();
        private int p_myangle;
        private int p_mynextCPID;
        private Coordinates p_mynextmovepos = new Coordinates();
        private int p_mynextmovespeed = 0;
        private Coordinates p_nextCPpos = new Coordinates();
        double oldCheckPointDist = 0;
        double nextCheckpointDist = 0;

        public CSBPod()
        {
            p_mypos.X = -1;
            p_mypos.Y = -1;
        }
        public void Update(int myposX, int myposY, int myspeedX, int myspeedY, int myangle, int mynextCPID)
        {
            if (p_mypos.X == -1 && p_mypos.Y == -1)
            {
                p_myprevpos.X = myposX;
                p_myprevpos.Y = myposY;
            }
            else
            {
                p_myprevpos.X = p_mypos.X;
                p_myprevpos.Y = p_mypos.Y;
            }
            p_mypos.X = myposX;
            p_mypos.Y = myposY;
            p_myspeed.X = myspeedX;
            p_myspeed.Y = myspeedY;
            p_myangle = myangle;
            p_mynextCPID = mynextCPID;
        }
        public void Update(CSBTrack Track)
        {
            p_nextCPpos = Track.CPTable[p_mynextCPID].Position;
            oldCheckPointDist = nextCheckpointDist;
            nextCheckpointDist = CSBCompute.DistAB(p_mypos, p_nextCPpos);
            double GapWithNxtWP = CSBCompute.ClosestFromNxtWP(p_myprevpos, p_mypos, p_nextCPpos, nextCheckpointDist);


            // To Be Completed

            p_mynextmovepos = p_nextCPpos;
            p_mynextmovespeed = 66;
        }

        public String Move(CSBTrack Track, CSBPod MyFriend, CSBPod MyFoeG, CSBPod MyFoeH)
        {
            return p_mynextmovepos.X + " " + p_mynextmovepos.Y + " " + p_mynextmovespeed;
        }
    }

    class CSBCheckPoint
    {
        public int cp_id { get; }
        public Coordinates Position { get; } = new Coordinates();
        public CSBCheckPoint(int x, int y)
        {
            Position.X = x;
            Position.Y = y;
        }
    }

    class CSBTrack
    {
        public int LapsNumber { get; }
        public int CPNumber { get; }
        public CSBCheckPoint[] CPTable { get; }

        public CSBTrack (int in1, int in2)
        {
            LapsNumber = in1;
            CPNumber = in2;
            CPTable = new CSBCheckPoint[CPNumber];
        }

        public void AddCheckPoint (int indice, int X, int Y)
        {
            CPTable.SetValue(new CSBCheckPoint(X, Y), indice);
        }
    }

    static class Player
    {
        static void Main(string[] args)
        {
            string rawinputs;
            string[] inputs;
            CSBPod PodMyG = new CSBPod();
            CSBPod PodMyH = new CSBPod();
            CSBPod PodHisG = new CSBPod();
            CSBPod PodHisH = new CSBPod();

            // read Track
            int laps = int.Parse(Console.ReadLine());
            int checkpointCount = int.Parse(Console.ReadLine());
            CSBTrack Track = new CSBTrack(laps, checkpointCount);
            for (int i = 0; i < Track.CPNumber; i++)
            {
                rawinputs = Console.ReadLine();
                inputs = rawinputs.Split(' ');
                int checkpointX = int.Parse(inputs[0]);
                int checkpointY = int.Parse(inputs[1]);
                Track.AddCheckPoint(i, checkpointX, checkpointY);
            }
            // game loop
            while (true)
            {
                // read Pods
                for (int i = 0; i < 4; i++)
                {
                    rawinputs = Console.ReadLine();
                    inputs = rawinputs.Split(' ');
                    int x = int.Parse(inputs[0]); // x Coordinates of pod
                    int y = int.Parse(inputs[1]); // y Coordinates of pod
                    int vx = int.Parse(inputs[2]); // x speed of pod
                    int vy = int.Parse(inputs[3]); // y speed of pod
                    int angle = int.Parse(inputs[4]); // angle of pod
                    int nextCheckPointId = int.Parse(inputs[5]); // next check point id of pod
                    switch(i)
                    {
                        case 0:
                            PodMyG.Update(x, y, vx, vy, angle, nextCheckPointId);
                            break;
                        case 1:
                            PodMyH.Update(x, y, vx, vy, angle, nextCheckPointId);
                            break;
                        case 2:
                            PodHisG.Update(x, y, vx, vy, angle, nextCheckPointId);
                            break;
                        case 3:
                            PodHisH.Update(x, y, vx, vy, angle, nextCheckPointId);
                            break;
                    }
                }

                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");
                // You have to output the target Coordinates
                // followed by the power (0 <= thrust <= 100)
                // i.e.: "x y thrust"

                PodMyG.Update(Track);
                PodMyH.Update(Track);

                Console.WriteLine(PodMyG.Move(Track, PodMyH, PodHisG, PodHisH));
                Console.WriteLine(PodMyH.Move(Track, PodMyG, PodHisG, PodHisH));
            }
        }
    }
}
