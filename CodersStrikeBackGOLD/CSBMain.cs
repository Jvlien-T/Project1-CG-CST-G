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
        public Coordinates(int inputX, int inputY)
        {
            X = inputX;
            Y = inputY;
        }
        public Coordinates()
        {
            X = 0;
            Y = 0;
        }
    }

    static public class CSBCompute
    {
        // fonction X modulo M :
        static public int mod(int x, int m)
        {
            int r = x % m;
            return r < 0 ? r + m : r;
        }

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

        //fonction qui nous donne la distance a laquelle on devrait passer au plus proche du prochain CP
        static public double ClosestFromNxtCP(Coordinates PreviousPos, Coordinates CurrentPos, Coordinates CPPos, double NextCPDistance)
        {
            double DriftAngle = AngleACB(CurrentPos, CPPos, PreviousPos);
            double AbsGap = NextCPDistance * Math.Tan(DriftAngle);
            // if (NextCPDistance < 900 && AbsGap > 600) { AbsGap = -1; }
            return AbsGap;
        }

        // fonction qui nous donne l'angle signé entre trois points EastOrigin, CentreAngle et Target
        static public double AngleOrientePos(Coordinates EastOrigin, Coordinates Target, Coordinates CentreAngle)
        {
            double Angle = 0;
            double tempAngle = AngleACB(EastOrigin, Target, CentreAngle);
            if (Target.Y > CentreAngle.Y) { Angle = tempAngle; }
            else if (Target.Y < CentreAngle.Y) { Angle = 2 * Math.PI - tempAngle; }
            else if (Target.X > CentreAngle.X) { Angle = 0; }
            else { Angle = Math.PI; }
            return Angle;
        }
        static public double AngleOrienteVecteur(Coordinates EastOrigin, Coordinates Vecteur, Coordinates CentreAngle)
        {
            Coordinates Target = new Coordinates(CentreAngle.X + Vecteur.X, CentreAngle.Y + Vecteur.Y);
            return AngleOrientePos(EastOrigin, Target, CentreAngle);
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

        public CSBTrack(int in1, int in2)
        {
            LapsNumber = in1;
            CPNumber = in2;
            CPTable = new CSBCheckPoint[CPNumber];
        }

        public void AddCheckPoint(int indice, string rawinputs)
        {
            string[] inputs = rawinputs.Split(' ');
            CPTable.SetValue(new CSBCheckPoint(int.Parse(inputs[0]), int.Parse(inputs[1])), indice);
        }
    }

    class CSBPod
    {
        private Coordinates[] Next3CPRoute = new Coordinates[3];
        private Coordinates p_myprevpos = new Coordinates();
        private Coordinates p_mypos = new Coordinates();
        private Coordinates p_myspeed = new Coordinates();
        private Coordinates p_mynextmovepos = new Coordinates();
        private Coordinates p_EastAngleOrigin = new Coordinates(33000, 0);
        private Coordinates p_MyEstimatedNextPos = new Coordinates();
        private int p_MyAngleDeg = 0;
        private int p_mynext1CPID = 1;
        private int p_mynext2CPID = 2;
        private int p_mynext3CPID = 3;
        private int p_MyNextMoveThrust = 0;
        private int DriftSide = 0;
        private bool p_BoostUsed = false;
        private bool p_UseBoostCommand = false;
        private bool p_UseShieldCommand = false;
        private double p_nextCheckPointPreviousDist = 0;
        private double p_nextCheckpointDist = 0;
        private double p_MissingDistNxtCP = 0;
        private double p_NextCPRawAngle = 0;
        private double p_NextCPTrackRelativeAngleDeg = 0;
        private double p_NextCPRelativeAngleDeg = 0;
        private double p_MyTrack = 0;
        private double p_MyAbsoluteSpeed = 0;

        public CSBPod()
        {
            p_mypos.X = -1;
            p_mypos.Y = -1;
        }


        public void Update(string rawinputs, CSBTrack Track)
        {
            string[] inputs = rawinputs.Split(' ');
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
            p_MyAngleDeg = int.Parse(inputs[4]);
            p_EastAngleOrigin.Y = p_mypos.Y;

            // Next 3 CP from Track
            p_mynext1CPID = int.Parse(inputs[5]);
            Next3CPRoute[0] = Track.CPTable[p_mynext1CPID].Position;
            p_mynext2CPID = (p_mynext1CPID + 1) % Track.CPNumber;
            Next3CPRoute[1] = Track.CPTable[p_mynext2CPID].Position;
            p_mynext3CPID = (p_mynext1CPID + 2) % Track.CPNumber;
            Next3CPRoute[2] = Track.CPTable[p_mynext3CPID].Position;

            // Speed
            p_MyEstimatedNextPos.X = p_mypos.X + p_myspeed.X;
            p_MyEstimatedNextPos.Y = p_mypos.Y + p_myspeed.Y;
            p_MyAbsoluteSpeed = CSBCompute.DistAB(p_mypos, p_MyEstimatedNextPos);
            p_MyTrack = CSBCompute.AngleOrienteVecteur(p_EastAngleOrigin, p_myspeed, p_mypos);

            // Angles
            p_NextCPRawAngle = CSBCompute.AngleOrientePos(p_EastAngleOrigin, Next3CPRoute[0], p_mypos);
            // p_NextCPTrackRelativeAngleDeg = CSBCompute.AngleACB(p_MyEstimatedNextPos, Next3CPRoute[0], p_mypos);
            p_NextCPTrackRelativeAngleDeg = CSBCompute.mod((int)((p_NextCPRawAngle - p_MyTrack) / Math.PI * 180), 360);
            if (p_NextCPTrackRelativeAngleDeg > 180) { p_NextCPTrackRelativeAngleDeg = (360 - p_NextCPTrackRelativeAngleDeg); }
            p_NextCPRelativeAngleDeg = ((p_NextCPRawAngle / Math.PI * 180) - p_MyAngleDeg);

            // Distances
            p_nextCheckPointPreviousDist = p_nextCheckpointDist;
            p_nextCheckpointDist = CSBCompute.DistAB(p_mypos, Next3CPRoute[0]);
            p_MissingDistNxtCP = CSBCompute.ClosestFromNxtCP(p_myprevpos, p_mypos, Next3CPRoute[0], p_nextCheckpointDist);

            // Drift

        }


        // Compute, va mettre à jour les 3 prochains points de route en fonction des perturbations extérieures
        public void Compute(CSBPod MyFriend, CSBPod MyFoeG, CSBPod MyFoeH)
        {
            // TBD
        }


        // Move, va se baser sur la table des 3 prochains points de route pour déplacer le POD sans se soucier des perturbations.
        public String Move()
        {
            if (p_nextCheckpointDist < (2 * p_MyAbsoluteSpeed) && p_MissingDistNxtCP < 400)
            { p_mynextmovepos = Next3CPRoute[1]; }
            else
            { p_mynextmovepos = Next3CPRoute[0]; }

            // selon le relevement du prochain WP, on régule les gaz :
            if (Math.Abs(p_NextCPRelativeAngleDeg) < 80) { p_MyNextMoveThrust = 100; }
            else { p_MyNextMoveThrust = 0; }


            // selon la vitesse et la dérive, on corrige le cap :
            if (p_MissingDistNxtCP > 555 && p_nextCheckpointDist < 5000)
            {
                if (p_nextCheckpointDist < (2 * p_MyAbsoluteSpeed))
                {
                    // je voudrais appliquer une grosse rotation du prochain CP pour déterminer mon prochain point de passage
                }
                else
                {
                    // je voudrais appliquer une petite rotation du prochain CP pour déterminer mon prochain point de passage
                }
            }

            // selon la proximité avec le prochain WP, on réduit les gaz :
            if (p_MyAbsoluteSpeed > p_nextCheckpointDist) { p_MyNextMoveThrust = p_MyNextMoveThrust / 5; }

            // selon les conditions, on décide si on va utiliser le boost ou pas :
            if (p_BoostUsed == false && Math.Abs(p_MissingDistNxtCP) < 555 && p_nextCheckpointDist > 5555 && p_MyAbsoluteSpeed > 111) { p_UseBoostCommand = true; }

            // LET'S MOVE
            if (p_UseShieldCommand)
            {
                return p_mynextmovepos.X + " " + p_mynextmovepos.Y + " SHIELD";
            }
            else if (p_UseBoostCommand)
            {
                return p_mynextmovepos.X + " " + p_mynextmovepos.Y + " BOOST";
                p_BoostUsed = true;
                p_UseBoostCommand = false;
            }
            else
            {
                return p_mynextmovepos.X + " " + p_mynextmovepos.Y + " " + p_MyNextMoveThrust;
            }
        }

        public void Debug()
        {
            Console.Error.WriteLine("##############################");
            Console.Error.WriteLine("Position = " + p_mypos.X + " ; " + p_mypos.Y + " . Vitesse = " + p_MyAbsoluteSpeed + " , missing next point : " + p_MissingDistNxtCP);
            Console.Error.WriteLine("heading = " + p_MyAngleDeg + " , bearing = " + p_NextCPRelativeAngleDeg + " , CPAngle = " + (p_NextCPRawAngle / Math.PI * 180));
            Console.Error.WriteLine("formule : " + (int)(p_NextCPRawAngle / Math.PI * 180) + " - " + (int)(p_MyTrack / Math.PI * 180));
            Console.Error.WriteLine("p_NextCPTrackRelativeAngleDeg = " + p_NextCPTrackRelativeAngleDeg);
            Console.Error.WriteLine("##############################");
        }
    }

    static class CSBMain
    {
        static void Main(string[] args)
        {
            string rawinputs;
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
                PodHisG.Update(rawinputs, Track);
                rawinputs = Console.ReadLine();
                PodHisH.Update(rawinputs, Track);

                // Write an action using Console.WriteLine()
                // To debug: Console.Error.WriteLine("Debug messages...");

                PodMyG.Debug();

                PodMyG.Compute(PodMyH, PodHisG, PodHisH);
                PodMyH.Compute(PodMyG, PodHisG, PodHisH);

                Console.WriteLine(PodMyG.Move());
                Console.WriteLine(PodMyH.Move());
            }
        }
    }
}
