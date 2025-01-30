using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Reversi
{
    public partial class MainWindow : Window
    {
        // Generowanie planszy
        private ReversiSilnikAI silnik = new ReversiSilnikAI(1, 8, 8);
        bool graPrzeciwkoKomputerowi = true;
        // Timer do opóźnienia ruchu komputera
        private DispatcherTimer timer;
        // Przypisanie kolorów do graczy
        private SolidColorBrush[] kolory = { Brushes.Ivory, Brushes.Green, Brushes.Sienna };
        string[] nazwyGraczy = { "", "zielony", "brązowy" };
        // Pola na planszy
        private Button[,] plansza;
        private bool planszaZainicjowana
        {
            get { return plansza[silnik.SzerokośćPlanszy - 1, silnik.WysokośćPlanszy - 1] != null; }
        }
        private void uzgodnijZawartośćPlanszy()
        {
            if (!planszaZainicjowana)
                return;
            for (int i = 0; i < silnik.SzerokośćPlanszy; i++)
                for (int j = 0; j < silnik.WysokośćPlanszy; j++)
                {
                    plansza[i, j].Background = kolory[silnik.PobierzStanPola(i, j)];
                    plansza[i, j].Content = silnik.PobierzStanPola(i, j).ToString();
                }
            przyciskKolorGracza.Background = kolory[silnik.NumerGraczaWykonującegoNastępnyRuch];
            liczbaPólZielony.Text = silnik.LiczbaPólGracz1.ToString();
            liczbaPólBrązowy.Text = silnik.LiczbaPólGracz2.ToString();
        }
        struct WspółrzędnePola
        {
            public int Poziomo, Pionowo;
        }
        WspółrzędnePola OstatniRuch;
        private static string symbolPola(int poziomo, int pionowo)
        {
            if (poziomo > 25 || pionowo > 8)
                return "(" + poziomo.ToString() + "," + pionowo.ToString() + ")";
            return "" + "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[poziomo] + "123456789"[pionowo];
        }
        void kliknięciePolaPlanszy(object sender, RoutedEventArgs e)
        {
            Button klikniętyPrzycisk = sender as Button;
            WspółrzędnePola współrzędne = (WspółrzędnePola)klikniętyPrzycisk.Tag;
            int klikniętePoziomo = współrzędne.Poziomo;
            int klikniętePionowo = współrzędne.Pionowo;
            // Wykonanie ruchu
            int zapamiętanyNumerGracza = silnik.NumerGraczaWykonującegoNastępnyRuch;
            if (silnik.PołóżKamień(klikniętePoziomo, klikniętePionowo))
            {
                uzgodnijZawartośćPlanszy();
                // Lista ruchów
                switch (zapamiętanyNumerGracza)
                {
                    case 1:
                        listaRuchówZielony.Items.Add(symbolPola(klikniętePoziomo, klikniętePionowo));
                        break;
                    case 2:
                        listaRuchówBrązowy.Items.Add(symbolPola(klikniętePoziomo, klikniętePionowo));
                        break;
                }
                listaRuchówZielony.SelectedIndex = listaRuchówZielony.Items.Count - 1;
                listaRuchówBrązowy.SelectedIndex = listaRuchówBrązowy.Items.Count - 1;
            }
            OstatniRuch.Poziomo = klikniętePoziomo;
            OstatniRuch.Pionowo = klikniętePionowo;
            // Sytuacje specjalne
            ReversiSilnik.SytuacjaNaPlanszy sytuacjaNaPlanszy = silnik.ZbadajSytuacjęNaPlanszy();
            bool koniecGry = false;
            switch (sytuacjaNaPlanszy)
            {
                case ReversiSilnik.SytuacjaNaPlanszy.BieżącyGraczNieMożeWykonaćRuchu:
                    MessageBox.Show("Aktywny gracz nie może oddać ruchu w tej turze.");
                    silnik.Pasuj();
                    uzgodnijZawartośćPlanszy();
                    break;
                case ReversiSilnik.SytuacjaNaPlanszy.ObajGraczeNieMogąWykonaćRuchu:
                    MessageBox.Show("Obaj gracze nie mogą wykonać ruchu");
                    koniecGry = true;
                    break;
                case ReversiSilnik.SytuacjaNaPlanszy.WszystkiePolaPlanszySąZajęte:
                    koniecGry = true;
                    break;
            }
            // Koniec gry - informacja o wyniku
            if (koniecGry)
            {
                int numerZwycięzcy = silnik.NumerGraczaMającegoPrzewagę;
                if (numerZwycięzcy != 0)
                    MessageBox.Show("Wygrał gracz " + nazwyGraczy[numerZwycięzcy], Title, MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show("Remis", Title, MessageBoxButton.OK, MessageBoxImage.Information);
                if (MessageBox.Show("Czy rozpocząć grę od nowa?", "Reversi", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes)
                    przygotowaniePlanszyDoNowejGry(1, silnik.SzerokośćPlanszy, silnik.WysokośćPlanszy);
                else
                {
                    planszaSiatka.IsEnabled = false;
                    przyciskKolorGracza.IsEnabled = false;
                }
            }
            else
            {
                if (graPrzeciwkoKomputerowi && silnik.NumerGraczaWykonującegoNastępnyRuch == 2)
                {
                    if (timer == null)
                    {
                        timer = new DispatcherTimer();
                        timer.Interval = new TimeSpan(0, 0, 0, 0, 1500);
                        timer.Tick += (_sender, _e) => { timer.IsEnabled = false; wykonajNajlepszyRuch(); };
                    }
                    timer.Start();
                }
            }
        }
        private void przygotowaniePlanszyDoNowejGry(int numerGraczaRozpoczynającego, int szerokośćPlanszy = 8, int wysokośćPlanszy = 8)
        {
            silnik = new ReversiSilnikAI(numerGraczaRozpoczynającego, szerokośćPlanszy, wysokośćPlanszy);
            listaRuchówZielony.Items.Clear();
            listaRuchówBrązowy.Items.Clear();
            uzgodnijZawartośćPlanszy();
            planszaSiatka.IsEnabled = true;
            przyciskKolorGracza.IsEnabled = true;
        }
        public MainWindow()
        {
            InitializeComponent();
            // Podział siatki na wiersze i kolumny
            for (int i = 0; i < 8; i++)
                planszaSiatka.ColumnDefinitions.Add(new ColumnDefinition());
            for (int j = 0; j < 8; j++)
                planszaSiatka.RowDefinitions.Add(new RowDefinition());

            // Tworzenie przycisków
            plansza = new Button[silnik.SzerokośćPlanszy, silnik.WysokośćPlanszy];
            for (int i = 0; i < silnik.SzerokośćPlanszy; i++)
                for (int j = 0; j < silnik.WysokośćPlanszy; j++)
                {
                    Button przycisk = new Button();
                    przycisk.Margin = new Thickness(0);
                    planszaSiatka.Children.Add(przycisk);
                    Grid.SetColumn(przycisk, i);
                    Grid.SetRow(przycisk, j);
                    przycisk.Tag = new WspółrzędnePola { Poziomo = i, Pionowo = j };
                    przycisk.Click += new RoutedEventHandler(kliknięciePolaPlanszy);
                    plansza[i, j] = przycisk;
                }
            uzgodnijZawartośćPlanszy();
        }
        private WspółrzędnePola? ustalNajlepszyRuch()
        {
            if (!planszaSiatka.IsEnabled) return null;
            if (silnik.LiczbaPustychPól == 0)
            {
                MessageBox.Show("Nie ma już wolnych pól na planszy", Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }
            try
            {
                int poziomo, pionowo;
                silnik.ProponujNajlepszyRuch(out poziomo, out pionowo);
                return new WspółrzędnePola() { Poziomo = poziomo, Pionowo = pionowo };
            }
            catch
            {
                MessageBox.Show("Bieżący gracz nie może wykonać ruchu", Title, MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }
        }
        private void zaznaczNajlepszyRuch()
        {
            WspółrzędnePola? wspPola = ustalNajlepszyRuch();
            if(wspPola.HasValue)
            {
                SolidColorBrush kolorPodpowiedzi = 
                    kolory[silnik.NumerGraczaWykonującegoNastępnyRuch].Lerp(kolory[0]);
                plansza[wspPola.Value.Poziomo, wspPola.Value.Pionowo].Background = kolorPodpowiedzi;
            }
        }
        private void zaznaczOstatniRuch()
        {
            WspółrzędnePola? wspPola = OstatniRuch;
            if (wspPola.HasValue && silnik.NumerGraczaWykonującegoNastępnyRuch == 1)
            {
                SolidColorBrush kolorPodpowiedzi =
                kolory[silnik.NumerGraczaWykonującegoNastępnyRuch + 1].Lerp(kolory[0]);
                plansza[wspPola.Value.Poziomo, wspPola.Value.Pionowo].Background = kolorPodpowiedzi;
            }
            else if (wspPola.HasValue && silnik.NumerGraczaWykonującegoNastępnyRuch == 2)
            {
                SolidColorBrush kolorPodpowiedzi =
                kolory[silnik.NumerGraczaWykonującegoNastępnyRuch - 1].Lerp(kolory[0]);
                plansza[wspPola.Value.Poziomo, wspPola.Value.Pionowo].Background = kolorPodpowiedzi;
            }
        }
        private void wykonajNajlepszyRuch()
        {
            WspółrzędnePola? wspPola = ustalNajlepszyRuch();
            if (wspPola.HasValue)
            {
                Button przycisk = 
                    plansza[wspPola.Value.Poziomo, wspPola.Value.Pionowo];
                kliknięciePolaPlanszy(przycisk, null);
            }
        }

        #region Metody zdarzeniowe menu głównego
        // Gra, Nowa gra dla jednego gracza, Rozpoczyna komputer (brązowy)
        private void MenuItem_NowaGraDla1Gracza_RozpoczynaKomputer_Click (object sender, RoutedEventArgs e)
        {
            graPrzeciwkoKomputerowi = true;
            Title = "Reversi - Gra z komputerem - Grasz zielonym";
            przygotowaniePlanszyDoNowejGry(2);
            wykonajNajlepszyRuch();
        }
        // Gra, Nowa gra dla jednego gracza, Rozpoczynasz Ty (zielony)
        private void MenuItem_NowaGraDla1Gracza_Click(object sender, RoutedEventArgs e)
        {
            graPrzeciwkoKomputerowi = true;
            Title = "Reversi - Gra z komputerem - Grasz brązowym";
            przygotowaniePlanszyDoNowejGry(1);
        }
        // Gra, Nowa gra dla dwóch graczy
        private void MenuItem_NowaGraDla2Graczy_Click(object sender, RoutedEventArgs e)
        {
            graPrzeciwkoKomputerowi = false;
            Title = "Reversi - 2 graczy";
            przygotowaniePlanszyDoNowejGry(1);
        }
        // Gra, zamknij
        private void MenuItem_Zamknij_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        // Pomoc, Podpowiedź ruchu
        private void MenuItem_PodpowiedźRuchu_Click(object sender, RoutedEventArgs e)
        {
            zaznaczNajlepszyRuch();
        }
        //Pomoc, Ruch wykonany przez komputer
        private void MenuItem_RuchWykonanyPrzezPrzeciwnika_Click(object sender, RoutedEventArgs e)
        {
            if(listaRuchówZielony.Items.Count + listaRuchówZielony.Items.Count > 0)
            {
                zaznaczOstatniRuch();
            }
        }
        // Pomoc, Zasady gry
        private void MenuItem_ZasadyGry_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "W grze Reversi gracze zajmują na przemian pola planszy, przejmując przy tym wszystkie pola przeciwnika znajdujące się" +
                "między nowo zajętym polem a innymi polami gracza wykonującego ruch. Celem gry jest zdobycie większej liczby pól niż przeciwnik.\n" +
                "Gracze może zając jedynie takie pole, które pozwoli mu przejąć przynajmniej jedno pole przeciwnika. Jeżeli tekiego pola nie ma ,musi oddać ruch.\n" +
                "Gra kończy się w momencie zajęcia wszystkich pól lub gdy żaden z graczy nie może wykonać ruchu.\n" +
                "Naciśnij przycisk koloru gracza by uzyskać podpowiedź.\n" +
                "Naciśnij przycisk koloru gracza trzymając przycisk CTRL, by wykonać najlepszy ruch.",
                "Reversi - Zasady gry");
        }
        // Pomoc, Strategia komputera
        private void MenuItem_StrategiaKomputera_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Komputer kieruje się następującymi priorytetami (od najwyższego):\n" +
                "1. Ustawić pionek w rogu. \n" +
                "2. Unikać ustawiania pionka tuż przy rogu. \n" +
                "3. Ustawić pionek przy krawędzi planszy.\n" +
                "4. Unikać ustawienia pionka w wierszu lub kolumnie oddalonej o jedno pole od krawędzi planszy.\n" +
                "5. Wybierać pole, w wyniku którego zdobyta zostanie największa liczba pól przeciwnika.\n",
                "Reversi - Strategia komputera");
        }
        // Pomoc, O programie
        private void MenuItem_Informacje_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Reversi należy do gier strategicznych, które charakteryzują się szybkimi zmianami sytuacji i wyników. Gra wymaga myślenia perspektywicznego. Jest znana od końca XVIII wieku, zyskała popularność w połowie XIX wieku w Europie. Największy sukces Polaka na międzynarodowej arenie to zdobycie tytułu Mistrza Europy przez Miłosza Cupiała w 2009 roku.", "O aplikacji");
        }
        #endregion

        private void przyciskKolorGracza_Click(object sender, RoutedEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
                wykonajNajlepszyRuch();
            else
              zaznaczNajlepszyRuch();
        }
    }
}
