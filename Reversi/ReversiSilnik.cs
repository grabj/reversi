using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Navigation;

namespace Reversi
{
    internal class ReversiSilnik
    {
        public int SzerokośćPlanszy { get; private set; }
        public int WysokośćPlanszy { get; private set; }
        public int NumerGraczaWykonującegoNastępnyRuch { get; private set; } = 1;
        private int[,] plansza;
        private bool czyWspółrzędnePolaPrawidłowe(int poziomo, int pionowo)
        {
            return poziomo >= 0 && poziomo < SzerokośćPlanszy &&
                pionowo >= 0 && pionowo < WysokośćPlanszy;
        }
        public int PobierzStanPola(int poziomo, int pionowo)
        {
            if (!czyWspółrzędnePolaPrawidłowe(poziomo, pionowo))
                throw new Exception("Nieprawidłowe współrzędne pola");
            return plansza[poziomo, pionowo];
        }
        private static int numerPrzeciwnika(int numerGracza)
        {
            if (numerGracza == 1) 
                return 2;
            else 
                return 1;
        }
        private void czyśćPlanszę()
        {
            for (int i = 0; i < SzerokośćPlanszy; i++)
                for (int j = 0; j < WysokośćPlanszy; j++)
                    plansza[i,j] = 0;

            int srodekSzer = SzerokośćPlanszy / 2;
            int srodekWys = WysokośćPlanszy / 2;
            plansza[srodekSzer - 1, srodekWys - 1] = plansza[srodekSzer, srodekWys] = 1;
            plansza[srodekSzer - 1, srodekWys] = plansza[srodekSzer, srodekWys - 1] = 2;
        }
        public ReversiSilnik(int numerGraczaRozpoczynającego, int szerokośćPlanszy = 8, int wysokośćPlanszy = 8)
        {
            if (numerGraczaRozpoczynającego < 1 || numerGraczaRozpoczynającego > 2)
                throw new Exception("Nieprawidłowy nuimer gracza rozpoczynającego grę");
            SzerokośćPlanszy = szerokośćPlanszy;
            WysokośćPlanszy = wysokośćPlanszy;
            plansza = new int[SzerokośćPlanszy, WysokośćPlanszy];
            czyśćPlanszę();
            NumerGraczaWykonującegoNastępnyRuch = numerGraczaRozpoczynającego;
            obliczLiczbyPól();
        }
        private void zmieńBieżącegoGracza()
        {
            NumerGraczaWykonującegoNastępnyRuch = numerPrzeciwnika(NumerGraczaWykonującegoNastępnyRuch);
        }
        protected int PołóżKamień(int poziomo, int pionowo, bool tylkoTest)
        {
            // Czy współrzędne są prawidłowe
            if (!czyWspółrzędnePolaPrawidłowe(poziomo, poziomo))
                throw new Exception("Nieprawidłowe współrzędne pola");
            // Czy pole nie jest już zajęte
            if (plansza[poziomo, pionowo] != 0) 
                return -1;

            int ilePólPrzejętych = 0;

            // Pętla po 8 kierunkach
            for (int kierunekPoziomo = -1; kierunekPoziomo <= 1; kierunekPoziomo++)
                for (int kierunekPionowo = -1; kierunekPionowo <= 1; kierunekPionowo++)
                {
                    // Wymuszenie pominięcia przypadku, gdy obie zmienne są równe 0
                    if (kierunekPoziomo == 0 && kierunekPionowo == 0) 
                        continue;

                    // Szukanie kamieni gracza w jednym z 8 kierunków
                    int i = poziomo;
                    int j = pionowo;
                    bool znalezionyKamieńPrzeciwnika = false;
                    bool znalezionyKamieńGraczaWykonującegoRuch = false;
                    bool znalezionePustePole = false;
                    bool osiągniętaKrawędźPlanszy = false;
                    do
                    {
                        i += kierunekPoziomo;
                        j += kierunekPionowo;
                        if (!czyWspółrzędnePolaPrawidłowe(i, j))
                            osiągniętaKrawędźPlanszy = true;
                        if (!osiągniętaKrawędźPlanszy)
                        {
                            if (plansza[i, j] == NumerGraczaWykonującegoNastępnyRuch)
                                znalezionyKamieńGraczaWykonującegoRuch = true;
                            if (plansza[i, j] == 0) 
                                znalezionePustePole = true;
                            if (plansza[i, j] == numerPrzeciwnika(NumerGraczaWykonującegoNastępnyRuch))
                                znalezionyKamieńPrzeciwnika = true;
                        }
                    }
                    while (!(osiągniętaKrawędźPlanszy || znalezionyKamieńGraczaWykonującegoRuch || znalezionePustePole));
                    // Sprawdzenie warunku poprawności ruchu
                    bool położenieKamieniaJestMożliwe = znalezionyKamieńPrzeciwnika && znalezionyKamieńGraczaWykonującegoRuch && !znalezionePustePole;

                    // "Odwrócenie" kamieni w przypadku spełnionego warunku
                    if (położenieKamieniaJestMożliwe)
                    {
                        int maks_indeks = Math.Max(Math.Abs(i - poziomo), Math.Abs(j - pionowo));

                        if (!tylkoTest)
                        {
                            for (int indeks = 0; indeks < maks_indeks; indeks++)
                                plansza[poziomo + indeks * kierunekPoziomo,
                                        pionowo + indeks * kierunekPionowo] = NumerGraczaWykonującegoNastępnyRuch;
                        }
                        ilePólPrzejętych += maks_indeks - 1;
                    }
                } // Koniec pętli po kieurnkach
                  // Zmiana gracza, jeżeli ruch został wykonany
            if (ilePólPrzejętych > 0 && !tylkoTest)
            {
                zmieńBieżącegoGracza();
                obliczLiczbyPól();
            }
            // Zmienna ilePólPrzejętych nie uwzględnia dostawionego kamienia
            return ilePólPrzejętych;
        }
        public bool PołóżKamień(int poziomo, int pionowo)
        {
            return PołóżKamień(poziomo, pionowo, false) > 0;
        }

        private int[] liczbyPól = new int[3]; // Puste, Gracz 1, Gracz 2
        public int LiczbaPustychPól { get { return liczbyPól[0]; } }
        public int LiczbaPólGracz1 { get { return liczbyPól[1]; } }
        public int LiczbaPólGracz2 { get { return liczbyPól[2]; } }
        private void obliczLiczbyPól()
        {
            for (int i = 0; i < liczbyPól.Length; ++i) 
                liczbyPól[i] = 0;
            for (int i = 0; i < SzerokośćPlanszy; ++i)
                for (int j = 0; j < WysokośćPlanszy; ++j)
                    liczbyPól[plansza[i, j]]++;
        }
        private bool czyBieżącyGraczMożeWykonaćRuch()
        {
            int liczbaPoprawnychPól = 0;
            for (int i = 0; i < SzerokośćPlanszy; ++i)
                for (int j = 0; j < WysokośćPlanszy; ++j)
                    if (plansza[i, j] == 0 && PołóżKamień(i, j, true) > 0)
                        liczbaPoprawnychPól++;
            return liczbaPoprawnychPól > 0;
        }
        public void Pasuj()
        {
            if (czyBieżącyGraczMożeWykonaćRuch())
                throw new Exception("Gracz nie może oddać ruchu, jeżeli wykonanie ruchu jest możliwe");
            zmieńBieżącegoGracza();
        }
        public enum SytuacjaNaPlanszy
        {
            RuchJestMożliwy,
            BieżącyGraczNieMożeWykonaćRuchu,
            ObajGraczeNieMogąWykonaćRuchu,
            WszystkiePolaPlanszySąZajęte
        }
        public SytuacjaNaPlanszy ZbadajSytuacjęNaPlanszy()
        {
            if (LiczbaPustychPól == 0)
                return SytuacjaNaPlanszy.WszystkiePolaPlanszySąZajęte;
            // Badanie możliwości ruchu bieżącego gracza
            bool czyMożliwyRuch = czyBieżącyGraczMożeWykonaćRuch();
            if (czyMożliwyRuch)
                return SytuacjaNaPlanszy.RuchJestMożliwy;
            else
            {
                zmieńBieżącegoGracza();
                bool czyMożliwyRuchOponenta = czyBieżącyGraczMożeWykonaćRuch();
                zmieńBieżącegoGracza();
                if (czyMożliwyRuchOponenta)
                    return SytuacjaNaPlanszy.BieżącyGraczNieMożeWykonaćRuchu;
                else return SytuacjaNaPlanszy.ObajGraczeNieMogąWykonaćRuchu;
            }
        }
        public int NumerGraczaMającegoPrzewagę
        {
            get
            {
                if (LiczbaPólGracz1 == LiczbaPólGracz2)
                    return 0;
                else if (LiczbaPólGracz1 > LiczbaPólGracz2)
                    return 1;
                else
                    return 2;
            }
        }
    }
}
