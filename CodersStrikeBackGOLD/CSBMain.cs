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
        private Coordinates p_PrevPosMe = new Coordinates();
        private Coordinates p_PosMe = new Coordinates();
        private Coordinates p_PosMySpeed = new Coordinates();
        private Coordinates p_PosForMyNextMove = new Coordinates();
        private Coordinates p_PosEastAngleOrigin = new Coordinates(33000, 0);
        private Coordinates p_PosMyEstimatedNext = new Coordinates();
        private int p_DegAngleOrgHead = 0;
        private int p_mynext1CPID = 1;
        private int p_mynext2CPID = 2;
        private int p_mynext3CPID = 3;
        private int p_ThrustForMyNextMove = 0;
        private int DriftSide = 0;
        private bool p_BoostUsed = false;
        private bool p_UseBoostCommand = false;
        private bool p_UseShieldCommand = false;
        private double p_PrevDistNextCheckPoint = 0;
        private double p_DistNextCheckpoint = 0;
        private double p_DistMissingNxtCP = 0;
        private double p_RadAngleOrgNextCP = 0;
        private double p_DegAngleTrackNextCP = 0;
        private double p_DegAngleHeadNextCP = 0;
        private double p_RadAngleOrgTrack = 0;
        private double p_DistMySpeed = 0;

        public CSBPod()
        {
            p_PosMe.X = -1;
            p_PosMe.Y = -1;
        }


        public void Update(string rawinputs, CSBTrack Track)
        {
            string[] inputs = rawinputs.Split(' ');
            if (p_PosMe.X == -1 && p_PosMe.Y == -1)
            {
                p_PrevPosMe.X = int.Parse(inputs[0]);
                p_PrevPosMe.Y = int.Parse(inputs[1]);
            }
            else
            {
                p_PrevPosMe.X = p_PosMe.X;
                p_PrevPosMe.Y = p_PosMe.Y;
            }
            p_PosMe.X = int.Parse(inputs[0]);
            p_PosMe.Y = int.Parse(inputs[1]);
            p_PosMySpeed.X = int.Parse(inputs[2]);
            p_PosMySpeed.Y = int.Parse(inputs[3]);
            p_DegAngleOrgHead = int.Parse(inputs[4]);
            p_PosEastAngleOrigin.Y = p_PosMe.Y;

            // Next 3 CP from Track
            p_mynext1CPID = int.Parse(inputs[5]);
            Next3CPRoute[0] = Track.CPTable[p_mynext1CPID].Position;
            p_mynext2CPID = (p_mynext1CPID + 1) % Track.CPNumber;
            Next3CPRoute[1] = Track.CPTable[p_mynext2CPID].Position;
            p_mynext3CPID = (p_mynext1CPID + 2) % Track.CPNumber;
            Next3CPRoute[2] = Track.CPTable[p_mynext3CPID].Position;

            // Speed
            p_PosMyEstimatedNext.X = p_PosMe.X + p_PosMySpeed.X;
            p_PosMyEstimatedNext.Y = p_PosMe.Y + p_PosMySpeed.Y;
            p_DistMySpeed = CSBCompute.DistAB(p_PosMe, p_PosMyEstimatedNext);

            // Angles
            p_RadAngleOrgTrack = CSBCompute.AngleOrienteVecteur(p_PosEastAngleOrigin, p_PosMySpeed, p_PosMe);
            p_RadAngleOrgNextCP = CSBCompute.AngleOrientePos(p_PosEastAngleOrigin, Next3CPRoute[0], p_PosMe);
            // p_DegAngleTrackNextCP = CSBCompute.AngleACB(p_PosMyEstimatedNext, Next3CPRoute[0], p_PosMe);
            // p_DegAngleTrackNextCP = CSBCompute.mod((int)((p_RadAngleOrgNextCP - p_RadAngleOrgTrack) / Math.PI * 180), 360);
            p_DegAngleTrackNextCP = (p_RadAngleOrgNextCP - p_RadAngleOrgTrack + 360) % 360;
            if (p_DegAngleTrackNextCP > 180) { p_DegAngleTrackNextCP -= 360; }
            p_DegAngleHeadNextCP = ((p_RadAngleOrgNextCP / Math.PI * 180) - p_DegAngleOrgHead);

            // Distances
            p_PrevDistNextCheckPoint = p_DistNextCheckpoint;
            p_DistNextCheckpoint = CSBCompute.DistAB(p_PosMe, Next3CPRoute[0]);
            p_DistMissingNxtCP = CSBCompute.ClosestFromNxtCP(p_PrevPosMe, p_PosMe, Next3CPRoute[0], p_DistNextCheckpoint);

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
            if (p_DistNextCheckpoint < (2 * p_DistMySpeed) && p_DistMissingNxtCP < 400)
            { p_PosForMyNextMove = Next3CPRoute[1]; }
            else
            { p_PosForMyNextMove = Next3CPRoute[0]; }

            // selon le relevement du prochain WP, on régule les gaz :
            if (Math.Abs(p_DegAngleHeadNextCP) < 80) { p_ThrustForMyNextMove = 100; }
            else { p_ThrustForMyNextMove = 0; }


            // selon la vitesse et la dérive, on corrige le cap :
            if (p_DistMissingNxtCP > 555 && p_DistNextCheckpoint < 5000)
            {
                if (p_DistNextCheckpoint < (2 * p_DistMySpeed))
                {
                    // je voudrais appliquer une grosse rotation du prochain CP pour déterminer mon prochain point de passage
                }
                else
                {
                    // je voudrais appliquer une petite rotation du prochain CP pour déterminer mon prochain point de passage
                }
            }

            // selon la proximité avec le prochain WP, on réduit les gaz :
            if (p_DistMySpeed > p_DistNextCheckpoint) { p_ThrustForMyNextMove = p_ThrustForMyNextMove / 5; }

            // selon les conditions, on décide si on va utiliser le boost ou pas :
            if (p_BoostUsed == false && Math.Abs(p_DistMissingNxtCP) < 555 && p_DistNextCheckpoint > 5555 && p_DistMySpeed > 111) { p_UseBoostCommand = true; }

            // LET'S MOVE
            if (p_UseShieldCommand)
            {
                return p_PosForMyNextMove.X + " " + p_PosForMyNextMove.Y + " SHIELD";
            }
            else if (p_UseBoostCommand)
            {
                return p_PosForMyNextMove.X + " " + p_PosForMyNextMove.Y + " BOOST";
                p_BoostUsed = true;
                p_UseBoostCommand = false;
            }
            else
            {
                return p_PosForMyNextMove.X + " " + p_PosForMyNextMove.Y + " " + p_ThrustForMyNextMove;
            }
        }

        public void Debug()
        {
            Console.Error.WriteLine("##############################");
            Console.Error.WriteLine("Vitesse = " + p_DistMySpeed + " , missing next point : " + p_DistMissingNxtCP);
            Console.Error.WriteLine("heading = " + p_DegAngleOrgHead + " , bearing = " + p_DegAngleHeadNextCP + " , CPAngle = " + (p_RadAngleOrgNextCP / Math.PI * 180));
            Console.Error.WriteLine("Drift = " + p_DegAngleTrackNextCP);
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
