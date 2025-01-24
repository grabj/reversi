using System;
using System.Collections.Generic;

namespace Reversi
{
    internal class ReversiSilnikAI : ReversiSilnik
    {
        public ReversiSilnikAI(int numerGraczaRozpoczynającego, int szerokośćPlanszy = 8, int wysokośćPlanszy = 8) 
            : base(numerGraczaRozpoczynającego, szerokośćPlanszy, wysokośćPlanszy)
        {
        }
        public struct MożliwyRuch : IComparable<MożliwyRuch>
        {
            public int poziomo;
            public int pionowo;
            public int priorytet;
            public MożliwyRuch(int poziomo, int pionowo, int priorytet)
            {
                this.poziomo = poziomo;
                this.pionowo = pionowo;
                this.priorytet = priorytet;
            }
            public int CompareTo(MożliwyRuch innyRuch)
            {
                return innyRuch.priorytet - this.priorytet;
            }
        }
        public void ProponujNajlepszyRuch(out int najlepszyRuchPoziomo, out int najlepszyRuchPionowo)
        {
            // Deklaracja tablicy możliwych ruchów
            List<MożliwyRuch> możliweRuchy = new List<MożliwyRuch>();
            int skokPriorytetu = SzerokośćPlanszy * WysokośćPlanszy;
            // Poszukiwanie możliwych ruchów
            for (int poziomo = 0; poziomo < SzerokośćPlanszy; poziomo++)
                for (int pionowo = 0; pionowo < WysokośćPlanszy; pionowo++)
                    if (PobierzStanPola(poziomo, pionowo) == 0)
                    {
                        // Liczba zajętych pól
                        int priorytet = PołóżKamień(poziomo, pionowo, true);
                        if (priorytet > 0)
                        {
                            MożliwyRuch mr = new MożliwyRuch(poziomo, pionowo, priorytet);
                            // Pole w rogu +
                            if ((mr.poziomo == 0 || mr.poziomo == SzerokośćPlanszy - 1) && (mr.pionowo == 0 || mr.pionowo == WysokośćPlanszy - 1))
                                mr.priorytet += skokPriorytetu * skokPriorytetu;
                            // Pole sąsiadujące z rogiem na przekątnych -
                            if ((mr.poziomo == 1 || mr.poziomo == SzerokośćPlanszy - 2) && (mr.pionowo == 1 || mr.pionowo == WysokośćPlanszy - 2))
                                mr.priorytet -= skokPriorytetu * skokPriorytetu;
                            // Pole sąsiadujące z rogiem w pionie -
                            if ((mr.poziomo == 0 || mr.poziomo == SzerokośćPlanszy - 1) && (mr.pionowo == 1 || mr.pionowo == WysokośćPlanszy - 2))
                                mr.priorytet -= skokPriorytetu * skokPriorytetu;
                            // Pole sąsiadujące z rogiem w poziomie -
                            if ((mr.poziomo == 1 || mr.poziomo == SzerokośćPlanszy - 2) && (mr.pionowo == 0 || mr.pionowo == WysokośćPlanszy - 1))
                                mr.priorytet -= skokPriorytetu * skokPriorytetu;
                            // Pole na brzegu +
                            if (mr.poziomo == 0 || mr.poziomo == SzerokośćPlanszy - 1 || mr.pionowo == 0 || mr.pionowo == WysokośćPlanszy - 1)
                                mr.priorytet += skokPriorytetu;
                            // Pole sąsiadujące z brzegiem -
                            if (mr.poziomo == 1 || mr.poziomo == SzerokośćPlanszy - 2 || mr.pionowo == 1 || mr.pionowo == WysokośćPlanszy - 2)
                                mr.priorytet -= skokPriorytetu * skokPriorytetu;
                            // oddanie do listy możliwych ruchów
                            możliweRuchy.Add(mr);
                        }
                    }
            // Wybór pola o największym priorytecie
            if (możliweRuchy.Count > 0)
            {
                możliweRuchy.Sort();
                najlepszyRuchPoziomo = możliweRuchy[0].poziomo;
                najlepszyRuchPionowo = możliweRuchy[0].pionowo;
            }
            else
                throw new Exception("Brak możliwych ruchów");
        }
    }
}
