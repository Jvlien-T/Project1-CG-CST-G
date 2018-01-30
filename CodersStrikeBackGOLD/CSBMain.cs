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
        private int p_mynext1CPID;
        private int p_mynext2CPID;
        private int p_mynext3CPID;
        private Coordinates p_mynextmovepos = new Coordinates();
        private int p_mynextmovespeed = 0;
        private Coordinates p_next1CPpos = new Coordinates();
        private Coordinates p_next2CPpos = new Coordinates();
        private Coordinates p_next3CPpos = new Coordinates();
        double oldCheckPointDist = 0;
        double nextCheckpointDist = 0;

        public CSBPod()
        {
            p_mypos.X = -1;
            p_mypos.Y = -1;
        }
        public void Update(string rawinputs)
        {
            string inputs = rawinputs.Split(' ');
            if (p_mypos.X == -1 && p_mypos.Y == -1)
            {
                p_myprevpos.X = int.Parse(inputs[0]);
                p_myprevpos.Y = int.Parse(inputs[1]);
            }
            else
            {
                p_myprevpos.X = p_mypos.X;
                p_myprevpos.Y = p_mypos.Y;
            }
            p_mypos.X = int.Parse(inputs[0]);
            p_mypos.Y = int.Parse(inputs[1]);
            p_myspeed.X = int.Parse(inputs[2]);
            p_myspeed.Y = int.Parse(inputs[3]);
            p_myangle = int.Parse(inputs[4]);
            p_mynext1CPID = int.Parse(inputs[5]);
        }
        public void Update(string rawinputs, CSBTrack Track)
        {
            this.Update(rawinputs);
            p_next1CPpos = Track.CPTable[p_mynext1CPID].Position;
            p_mynext2CPID = (p_mynext1CPID + 1) % Track.CPNumber;
            p_next2CPpos = Track.CPTable[p_mynext2CPID].Position;
            p_mynext3CPID = (p_mynext1CPID + 2) % Track.CPNumber;
            p_next3CPpos = Track.CPTable[p_mynext3CPID].Position;
            oldCheckPointDist = nextCheckpointDist;
            nextCheckpointDist = CSBCompute.DistAB(p_mypos, p_next1CPpos);
            double GapWithNxtWP = CSBCompute.ClosestFromNxtWP(p_myprevpos, p_mypos, p_next1CPpos, nextCheckpointDist);


            // To Be Completed

            p_mynextmovepos = p_next1CPpos;
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

        public void AddCheckPoint (int indice, string rawinputs)
        {
            string[] inputs = rawinputs.Split(' ');
            CPTable.SetValue(new CSBCheckPoint(int.Parse(inputs[0]), int.Parse(inputs[1])), indice);
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
                Track.AddCheckPoint(i, rawinputs);
            }
            // game loop
            while (true)
            {
                // read Pods
                rawinputs = Console.ReadLine();
                PodMyG.Update(rawinputs, Track);
                rawinputs = Console.ReadLine();
                PodMyH.Update(rawinputs, Track);
                rawinputs = Console.ReadLine();
                PodHisG.Update(rawinputs);
                rawinputs = Console.ReadLine();
                PodHisH.Update(rawinputs);

                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");

                Console.WriteLine(PodMyG.Move(Track, PodMyH, PodHisG, PodHisH));
                Console.WriteLine(PodMyH.Move(Track, PodMyG, PodHisG, PodHisH));
            }
        }
    }
}
