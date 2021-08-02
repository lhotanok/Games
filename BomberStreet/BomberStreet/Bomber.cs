using System;
using System.Collections.Generic;
using System.Drawing;

namespace BomberStreet
{
    public enum Stav { NovaHra, RozehranaHra, Vyhra, Prohra };

    public enum Sipka { Nestisknuta, Vpravo, Vlevo, Dolu, Nahoru };

    abstract class Prvek
    {
        public int X; //souradnice prvku na ose x na herni plose
        public int Y; //souradnice prvku na ose y na herni plose
        public char Id; //identifikator typu prvku (hrac / nepritel / bomba apod)
        public HerniPlocha HerniPlocha;
    }
    abstract class StatickyPrvek : Prvek
    {
        //predmety, ktere samy neprovadeji zadnou akci (nepohybuji se po herni plose, nemeni ji)
        public abstract void OdstranSe(); //odstraneni prvku z herni plochy a celkove ze hry
    }

    abstract class DynamickyPrvek : Prvek
    {
        //pohyblive prvky menici stav svuj a sveho okoli (hrac, nepritel, bomby)
        public int Zivoty;
        public abstract void OdehrajTah();
    }

    class Bomber : DynamickyPrvek
    {
        public int NasledujiciX;
        public int NasledujiciY;
        public int MaxBomb;
        public int Dosah;
        public bool Stit;
        public bool PosunovatBombu;
        public List<Vylepseni> SebranaVylepseni;
        public List<Vylepseni> VylepseniOdstraneni;
        public List<Bomba> UmisteneBomby;
        protected int[] Vektory = new int[] { -1, 1 };
        protected int NoveX;
        protected int NoveY;
        protected int ODveDalX;
        protected int ODveDalY;
        public Bomber(int pozice_x, int pozice_y, HerniPlocha herniPlocha)
        {
            this.X = pozice_x;
            this.Y = pozice_y;
            this.Id = 'H';
            this.HerniPlocha = herniPlocha;
            this.Zivoty = 3;
            this.NasledujiciX = 0;
            this.NasledujiciY = 0;
            this.MaxBomb = 1;
            this.Dosah = 1;
            this.Stit = false;
            this.PosunovatBombu = false;
            this.SebranaVylepseni = new List<Vylepseni>();
            this.VylepseniOdstraneni = new List<Vylepseni>();
            this.UmisteneBomby = new List<Bomba>();
        }
        public void NastavSouradnice()
        {
            NoveX = X;
            NoveY = Y;
            ODveDalX = X;
            ODveDalY = Y;
        }
        public override void OdehrajTah()
        {
            NastavSouradnice();
            switch (HerniPlocha.Sipka)
            {
                case Sipka.Nestisknuta:
                    break;

                case Sipka.Vpravo:
                    NoveX = PohybHorizontalne(NoveX, Vektory[1], this);
                    ODveDalX += 2;
                    break;

                case Sipka.Vlevo:
                    NoveX = PohybHorizontalne(NoveX, Vektory[0], this);
                    ODveDalX -= 2;
                    break;

                case Sipka.Dolu:
                    NoveY = PohybVertikalne(NoveY, Vektory[1], this);
                    ODveDalY += 2;
                    break;

                case Sipka.Nahoru:
                    NoveY = PohybVertikalne(NoveY, Vektory[0], this);
                    ODveDalY -= 2;
                    break;
            }
            if (HerniPlocha.Mezernik) //hrac se pokousi umistit bombu stiskem mezerniku
            {
                ZkusUmistitBombu();
                return;
            }
            ZkontrolujObsahPozice(X, Y, NoveX, NoveY, ODveDalX, ODveDalY, this);
            ZkontrolujListVylepseni();
        }
        public void ZkusUmistitBombu()
        {
            if (NasledujiciX > 0 && NasledujiciY > 0 && NasledujiciX < HerniPlocha.Sirka && NasledujiciY < HerniPlocha.Vyska) //kontrola indexu
            {
                if (HerniPlocha.VolnaPozice(NasledujiciX, NasledujiciY) && MaxBomb > 0)
                {
                    UmistiBombu();
                }
            }
        }
        public int PohybHorizontalne(int nove_x, int smer, Bomber bomber)
        {
            NasledujiciX = nove_x + (2 * smer);
            NasledujiciY = Y;
            if (!HerniPlocha.VolnaPozice(nove_x + smer, Y)) //kontrola, jestli pozdeji nebude zkouset umistovat bombu pres prekazku
            {
                NasledujiciX = nove_x + smer;
            }
            if (smer == 1) //smer vpravo
            {
                ZmenZnakSmeruPohybu(bomber.Id, 0);
            }
            else //smer vlevo
            {
                ZmenZnakSmeruPohybu(bomber.Id, 1);
            }
            return nove_x + smer;
        }
        public int PohybVertikalne(int nove_y, int smer, Bomber bomber)
        {
            NasledujiciY = nove_y + (2 * smer);
            NasledujiciX = X;
            if (!HerniPlocha.VolnaPozice(X, nove_y + smer)) //kontrola, jestli nezkousi umistovat bombu pres prekazku
            {
                NasledujiciY = nove_y + smer;
            }
            if (smer == 1) //smer dolu
            {
                ZmenZnakSmeruPohybu(bomber.Id, 2);
            }
            else //smer nahoru
            {
                ZmenZnakSmeruPohybu(bomber.Id, 3);
            }
            return nove_y + smer;
        }
        public void ZmenZnakSmeruPohybu(char bomber, int indexZnaku)
        {
            Dictionary<char, string> smeryBombera = new Dictionary<char, string>() { { 'H', "dasw" }, { 'N', "><v^" } }; //znaky pro smery pohybu hrace / nepritele
            HerniPlocha.Plocha[X, Y] = smeryBombera[bomber][indexZnaku];
        }
        public void ZkontrolujObsahPozice(int x, int y, int nove_x, int nove_y, int o_dve_dal_x, int o_dve_dal_y, Bomber bomber)
        {
            if (HerniPlocha.VolnaPozice(nove_x, nove_y) || HerniPlocha.Lava(nove_x, nove_y))
            {
                VstupNaPole(bomber);
            }
            else if (HerniPlocha.Vylepseni(nove_x, nove_y))
            {
                VezmiVylepseni(nove_x, nove_y, this);
                HerniPlocha.PresunPrvek(x, y, nove_x, nove_y);
            }
            else if (PosunovatBombu && HerniPlocha.Bomba(nove_x, nove_y)) //je sebrano vylepseni pro presunovani bomby a bomba je v novem smeru pohybu
            {
                ZkusPosunoutBombu();
            }
            else if ((bomber.Id == 'H' && HerniPlocha.Nepritel(nove_x, nove_y)) || (bomber.Id == 'N' && HerniPlocha.Hrac(nove_x, nove_y))) //stret hrace s nepritelem
            { 
                //muze nastat pri volani od hrace, nebo od nepritele (nepritel se pohne ve smeru hrace)
                HerniPlocha.OdeberZivot(HerniPlocha.Bomber); //odebira zivot pouze hraci, nepriteli nikoli
            }
        }
        public void VstupNaPole(Bomber bomber)
        {
            if (bomber.Id == 'N' && HerniPlocha.Lava(NoveX, NoveY)) //inteligence pro nepritele, aby zbytecne nevstoupil na pole s lavou
            {
                return;
            }
            if (HerniPlocha.Lava(NoveX, NoveY))
            {
                HerniPlocha.OdeberZivot(bomber);
            }
            HerniPlocha.PresunPrvek(X, Y, NoveX, NoveY);
        }
        public void ZkusPosunoutBombu()
        {
            if (NoveX != ODveDalX && HerniPlocha.VolnaPozice(ODveDalX, NoveY)) //lze posunout bombu vodorovne
            {
                HerniPlocha.PresunPrvek(NoveX, NoveY, ODveDalX, NoveY);
            }
            else if (NoveY != ODveDalY && HerniPlocha.VolnaPozice(NoveX, ODveDalY)) //lze posunout bombu svisle
            {
                HerniPlocha.PresunPrvek(NoveX, NoveY, NoveX, ODveDalY);
            }
        }
        public void UmistiBombu()
        {
            Bomba bomba = new Bomba(NasledujiciX, NasledujiciY, this, HerniPlocha, new List<Lava>());
            HerniPlocha.PrvkyPridani.Add(bomba);
            UmisteneBomby.Add(bomba);
            HerniPlocha.Plocha[NasledujiciX, NasledujiciY] = 'b';
            MaxBomb -= 1;
        }
        public void VezmiVylepseni(int x, int y, Bomber bomber)
        {
            if (HerniPlocha.Zivot(x, y))
            {
                bomber.Zivoty += 1; //vylepseni pridavajici zivot tomu, kdo jej sebral
                if (bomber.Id == 'N')
                {
                    bomber.HerniPlocha.CelkoveZivotyNepratel += 1;
                }
                return; //zivot nema casovac, nema smysl jej inicializovat v dalsim kroku
            }
            const int casovacVylepseni = 100;
            if (HerniPlocha.Stit(x, y))
            {
                bomber.Stit = true; //prida ochranny stit, ktery predejde nejblizsi ztrate 1 zivota
                AktualizujListVylepseni(casovacVylepseni, 'S');
            }
            else if (HerniPlocha.Batoh(x, y)) //umozni umistit vice bomb najednou
            {
                const int bombyBatoh = 3;
                bomber.MaxBomb += bombyBatoh; //kazdy sebrany batoh zvetsi aktualni max pocet bomb o konstantu 3
                AktualizujListVylepseni(casovacVylepseni, 'r');
            }
            else if (HerniPlocha.LektvarSily(x, y)) //umozni tlacit bombu pred sebou
            {
                bomber.PosunovatBombu = true;
                AktualizujListVylepseni(casovacVylepseni, 'L');
            }
            else if (HerniPlocha.Krystal(x, y)) //zvetsi dosah bomby
            {
                bomber.Dosah = 2; 
                AktualizujListVylepseni(casovacVylepseni, 'k');
            }
        }
        public void AktualizujListVylepseni(int casovac, char id)
        {
            foreach (Vylepseni v in SebranaVylepseni)
            {
                if (v.Id == id) //dane vylepseni je aktualne sebrane - pouze obnovime casovac na max
                {
                    v.Casovac = casovac;
                    return;
                }
            }
            Vylepseni vylepseni = new Vylepseni(casovac, id, this);
            SebranaVylepseni.Add(vylepseni);
        }
        public void ZkontrolujListVylepseni()
        {
            foreach (Vylepseni vylepseni in SebranaVylepseni)
            {
                vylepseni.MerCas();
            }
            foreach (Vylepseni odstraneni in VylepseniOdstraneni)
            {
                SebranaVylepseni.Remove(odstraneni);
            }
        }
    }
    class Nepritel : Bomber
    {
        public char[] Smery = new char[] { 'h', 'v' }; //horizontalni nebo vertikalni
        public Nepritel(int pozice_x, int pozice_y, HerniPlocha herniPlocha)
            : base(pozice_x, pozice_y, herniPlocha) //dle Bomber, atributy nastaveny stejne az na id a zivoty
        {
            this.Id = 'N';
            this.Zivoty = 7;
        }
        public override void OdehrajTah()
        {
            NastavSouradnice();
            char smerPohybu = Smery[HerniPlocha.Rnd.Next(0, Smery.Length)]; //horizontalne nebo vertikalne
            int vektor = Vektory[HerniPlocha.Rnd.Next(0, Vektory.Length)]; //hodnota -1 nebo +1
            bool provestPohyb = HerniPlocha.Rozhodnuti[HerniPlocha.Rnd.Next(0, HerniPlocha.Rozhodnuti.Length)]; //ano nebo ne

            if (provestPohyb)
            {
                if (smerPohybu == 'h') //nepritel zvolil horizontalni pohyb
                {
                    NoveX = PohybHorizontalne(NoveX, vektor, this);
                    ODveDalX += (2 * vektor);
                }
                else //vertikalni pohyb
                {
                    NoveY = PohybVertikalne(NoveY, vektor, this);
                    ODveDalY += (2 * vektor);
                }
                ZkontrolujObsahPozice(X, Y, NoveX, NoveY, ODveDalX, ODveDalY, this);
            }
            else //nechce provest pohyb - chce umistit bombu?
            {
                bool umistitBombu = HerniPlocha.Rozhodnuti[HerniPlocha.Rnd.Next(0, HerniPlocha.Rozhodnuti.Length)]; //ano nebo ne
                if (umistitBombu)
                {
                    ZkusUmistitBombu();
                }
            }
            ZkontrolujListVylepseni();
        }
    }
    class Bomba : DynamickyPrvek
    {
        private Bomber _bomber;
        public List<Lava> LavovaPole;
        public bool Nadbytecna; //bomba byla umistena behem aktivovaneho vylepseni batohu (vice bomb najednou)
        public Bomba(int pozice_x, int pozice_y, Bomber bomber, HerniPlocha herniPlocha, List<Lava> lavovaPole)
        {
            this.X = pozice_x;
            this.Y = pozice_y;
            this.HerniPlocha = herniPlocha;
            this._bomber = bomber;
            this.Zivoty = 20; //pocet kroku pred detonaci
            this.Nadbytecna = false;
            this.LavovaPole = lavovaPole;
        }
        public override void OdehrajTah()
        {
            Zivoty -= 1;
            if (Zivoty == 0) //vyprsel cas, detonace
            {
                ProvedDetonaci(_bomber);
                Zivoty -= 1;
            }
            else if (Zivoty == -10) //odstranit bombu z herni plochy po uplynuti casovace
            {
                VycistiMistoVybuchu();
                AktualizujMaxBomb();
            }
        }
        public void ProvedDetonaci(Bomber bomber)
        {
            HerniPlocha.Plocha[X, Y] = 'B'; //zmena ikony bomby na detonujici
            for (int i = X - bomber.Dosah; i <= X + bomber.Dosah; i++) //okoli bomby
            {
                for (int j = Y - bomber.Dosah; j <= Y + bomber.Dosah; j++)
                {
                    if (i > 0 && j > 0 && i < HerniPlocha.Sirka && j < HerniPlocha.Vyska && (i == X ^ j == Y)) //indexy nejsou mimo matici, nejedna se o index bomby samotne
                    {
                        //i == x ^ j == y ... XOR, vybuch pouze vodorovne / svisle od bomby, nikoli diagonalne, navic ne v miste samotne bomby, kde i == x && j == y
                        ZkontrolujMistoVybuchu(i, j, X, Y);
                    }
                }
            }
        }
        public void ZkontrolujMistoVybuchu(int x, int y, int bomba_x, int bomba_y)
        {
            if (HerniPlocha.Hrac(x, y))
            {
                HerniPlocha.OdeberZivot(HerniPlocha.Bomber); //vybuch zasahl hrace
            }
            else if (HerniPlocha.Nepritel(x, y))
            {
                Bomber zasazeny = NajdiZasazenehoNepritele(x, y);
                HerniPlocha.OdeberZivot(zasazeny); //vybuch zasahl protihrace
            }
            else if (HerniPlocha.VolnaPozice(x, y) || !HerniPlocha.NeprorazitelnaZed(x, y))
            {
                if (!ZastavSeZaZdi(x, y, bomba_x, bomba_y))
                {
                    VytvorLavu(x, y); //vytvori lavu pouze pokud misto neni za neprorazitelnou zdi
                }
            }
        }
        public bool ZastavSeZaZdi(int x, int y, int bomba_x, int bomba_y)
        {
            if (y == bomba_y) //v ramci 1 radku
            {
                x += UrciSmerVybuchu(x, bomba_x);
            }
            else if (x == bomba_x) //v ramci 1 sloupce
            {
                y += UrciSmerVybuchu(y, bomba_y);
            }
            if (HerniPlocha.NeprorazitelnaZed(x, y))
            {
                return true; // lava by se vytvorila za zdi, ktera by nebyla prorazena
            }
            return false;
        }
        public int UrciSmerVybuchu(int souradnice_lavy, int souradnice_bomby)
        {
            if (souradnice_lavy < souradnice_bomby)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }
        public Nepritel NajdiZasazenehoNepritele(int x, int y)
        {
            foreach (Nepritel nepritel in HerniPlocha.Nepratele)
            {
                if (nepritel.X == x && nepritel.Y == y)
                {
                    return nepritel;
                }
            }
            return new Nepritel (0, 0, HerniPlocha); //vsechny casti musi mit navratovou hodnotu (i kdyz aspon 1 nepritel v Nepratele musi existovat)
        }
        public void VytvorLavu(int x, int y)
        {
            HerniPlocha.Plocha[x, y] = 'l';
            Lava lava = new Lava(x, y, HerniPlocha);
            LavovaPole.Add(lava);
        }
        public void VycistiMistoVybuchu()
        {
            HerniPlocha.PrvkyOdstraneni.Add(this);
            foreach (Lava lava in LavovaPole)
            {
                lava.OdstranSe();
            }
        }
        public void AktualizujMaxBomb()
        {
            if (!Nadbytecna) //nebyla vytvorena diky aktivnimu vylepseni pro zvyseni max poctu bomb (nadbytecnost se nastavi az po deaktivaci vylepseni)
            {
                _bomber.MaxBomb += 1;
            }
        }
    }
    class Vylepseni : StatickyPrvek
    {
        public int Casovac;
        public Bomber Bomber;
        public Vylepseni(int casovac, char id, Bomber bomber)
        {
            this.Casovac = casovac;
            this.Bomber = bomber;
            this.Id = id;
        }
        public void MerCas ()
        {
            Casovac -= 1;
            if (Casovac == 0)
            {
                OdstranSe();
            }
        }
        public override void OdstranSe() //z bomberova listu sebranych vylepseni, aktualizuje stav
        {
            if (Id == 'S') //stit
            {
                Bomber.Stit = false;
            }
            else if (Id == 'r') //batoh pro vice bomb najednou
            {
                Bomber.MaxBomb = 1;
                foreach (Bomba bomba in Bomber.UmisteneBomby)
                {
                    bomba.Nadbytecna = true;
                }
            }
            else if (Id == 'L') //lektvar sily
            {
                Bomber.PosunovatBombu = false;
            }
            else if (Id == 'k') //krystal pro vetsi dosah bomb
            {
                Bomber.Dosah = 1;
            }
            Bomber.VylepseniOdstraneni.Add(this);
        }
    }
    class Lava : StatickyPrvek
    {
        public Lava(int pozice_x, int pozice_y, HerniPlocha herniPlocha)
        {
            this.X = pozice_x;
            this.Y = pozice_y;
            this.HerniPlocha = herniPlocha;
        }
        public override void OdstranSe()
        {
            if (HerniPlocha.Plocha[X, Y] == 'l') //pokud je na miste stale lava a neprosel mezitim hrac / nepritel
            {
                HerniPlocha.Plocha[X, Y] = ' ';
            }
        }
    }
    class HerniPlocha
    {
        public char[,] Plocha;
        public int Vyska;
        public int Sirka;

        public Sipka Sipka; //zachyceni pohybu hrace
        public Stav Stav;
        public bool Mezernik; //pro umistovani bomb

        public Bomber Bomber;
        public List<DynamickyPrvek> DynamickeBezHrace;
        public List<Nepritel> Nepratele;
        public List<DynamickyPrvek> PrvkyOdstraneni; //prvky, ktere maji byt v ramci aktualniho kola odstraneny
        public List<DynamickyPrvek> PrvkyPridani; //prvky, ktere maji byt v ramci aktualniho kola pridany
        public char[] DostupnaVylepseni; //dle dostupnych textur a odpovidajicich znaku
        public Random Rnd; //pro nahodne generovani pohybu nepratel a umistovani vylepseni na herni plochu
        public bool[] Rozhodnuti = new bool[] { false, true, false, false }; //prevazuje false, aby byla vetsi pravdepodobnost, ze se rozhodne pro NE
        public int CelkoveZivotyNepratel;

        Bitmap[] _textury;
        int _rozmer; // rozmer ikony

        private int _casovac; //k mereni poctu kol (nektere akce nebudou probihat pri kazde iteraci, ale napr. pri kazde dvacate)

        public HerniPlocha(int radek, string umisteniHerniPlocha, string umisteniTextury, Random rnd)
        {
            this.Stav = Stav.RozehranaHra;
            this.Vyska = 0;
            this.Sirka = 0;
            this.Rnd = rnd;
            this._casovac = 0;
            this.CelkoveZivotyNepratel = 0;
            VytvorHerniPlochu(umisteniHerniPlocha, radek);
            NactiTextury(umisteniTextury);
        }
        public void NactiTextury(string umisteni)
        {
            Bitmap bitmap = new Bitmap(umisteni);
            this._rozmer = bitmap.Height; //rozmer 1 ikony
            int pocet = bitmap.Width / _rozmer; //pocet ikon v 1 radku
            _textury = new Bitmap[pocet];
            for (int i = 0; i < pocet; i++)
            {
                Rectangle rectangle = new Rectangle(_rozmer * i, 0, _rozmer, _rozmer);
                _textury[i] = bitmap.Clone(rectangle, System.Drawing.Imaging.PixelFormat.DontCare);
            }
        }
        public void VytvorHerniPlochu(string umisteni, int pocatecniRadek)
        {
            System.IO.StreamReader reader = new System.IO.StreamReader(umisteni);
            for (int i = 0; i < pocatecniRadek; i++) //zacne cist az od radku pro prislusny level
            {
                reader.ReadLine();
            }

            this.Vyska = int.Parse(reader.ReadLine() ?? string.Empty);
            this.Sirka = int.Parse(reader.ReadLine() ?? string.Empty);

            Plocha = new char[Sirka, Vyska];

            DynamickeBezHrace = new List<DynamickyPrvek>();
            Nepratele = new List<Nepritel>();
            PrvkyOdstraneni = new List<DynamickyPrvek>();
            PrvkyPridani = new List<DynamickyPrvek>();
            DostupnaVylepseni = new[] { 'z', 'S', 'r', 'k', 'L' };

            for (int y = 0; y < Vyska; y++)
            {
                string radek = reader.ReadLine();
                for (int x = 0; x < Sirka; x++)
                {
                    if (radek != null)
                    {
                        char znak = radek[x];
                        Plocha[x, y] = znak;

                        switch (znak)
                        {
                            case 'd':
                            case 'a':
                            case 's':
                            case 'w':
                                this.Bomber = new Bomber(x, y, this);
                                break;

                            case '>':
                            case '<':
                            case 'v':
                            case '^':
                                Nepritel nepritel = new Nepritel(x, y, this);
                                DynamickeBezHrace.Add(nepritel);
                                Nepratele.Add(nepritel);
                                CelkoveZivotyNepratel += nepritel.Zivoty;
                                break;
                        }
                    }
                }
            }
            reader.Close();
        }
        public void OdstranDynamickyPrvek(int x, int y)
        {
            for (int i = 0; i < DynamickeBezHrace.Count; i++)
            {
                if ((DynamickeBezHrace[i].X == x) && (DynamickeBezHrace[i].Y == y))
                {
                    Plocha[x, y] = ' ';
                    DynamickeBezHrace.RemoveAt(i);
                    return;
                }
            }
        }
        public void OdeberZivot(Bomber bomber)
        {
            if (bomber.Stit)
            {
                bomber.Stit = false; //stit se spotreboval
                return; //zadny zivot se neodebere, nemeni se stav poctu zivotu
            }
            if (bomber.Zivoty == 0) //osetri napriklad pripad, kdy je prvek v 1 kole zasazen vice bombami najednou (kazdy zasah se mu pokusi odebrat zivot)
            {
                return; //nelze odebrat vice zivotu 
            }
            bomber.Zivoty -= 1;
            if (bomber.Id == 'H' && bomber.Zivoty == 0)
            {
                this.Stav = Stav.Prohra; //hrac ztratil vsechny zivoty
            }
            else if (bomber.Id == 'N')
            {
                CelkoveZivotyNepratel -= 1;
                if (CelkoveZivotyNepratel == 0)
                {
                    this.Stav = Stav.Vyhra; //nepritel ztratil vsechny zivoty
                    return;
                }
                if (bomber.Zivoty == 0)
                {
                    PrvkyOdstraneni.Add(bomber); //odstraneni konkretniho zemreleho nepritele
                }
            }
        }
        public void PresunPrvek(int start_x, int start_y, int cil_x, int cil_y) //odkud na ose x, odkud na ose y, kam na ose x, kam na ose y
        {
            char aktualniPrvek = Plocha[start_x, start_y];
            Plocha[start_x, start_y] = ' ';
            Plocha[cil_x, cil_y] = aktualniPrvek;

            if (aktualniPrvek == 'a' || aktualniPrvek == 'w' || aktualniPrvek == 's' || aktualniPrvek == 'd')
            {
                Bomber.X = cil_x;
                Bomber.Y = cil_y;
                return;
            }

            foreach (DynamickyPrvek prvek in DynamickeBezHrace)
            {
                if ((prvek.X == start_x) && (prvek.Y == start_y))
                {
                    prvek.X = cil_x;
                    prvek.Y = cil_y;
                    return;
                }
            }
        }
        public void UmistiVylepseni()
        {
            _casovac += 1;
            if (_casovac > 20) //zabranuje prilis castemu pridavani vylepseni
            {
                bool umistit = Rozhodnuti[Rnd.Next(0, Rozhodnuti.Length)]; //ano nebo ne
                if (umistit)
                {
                    int naOseX = Rnd.Next(1, Sirka - 1); //okraje herni plochy jsou obsazeny zdmi - nezapocitavame
                    int naOseY = Rnd.Next(1, Vyska - 1);
                    if (VolnaPozice(naOseX, naOseY))
                    {
                        int indexZvoleneho = Rnd.Next(0, DostupnaVylepseni.Length);
                        char znakVylepseni = DostupnaVylepseni[indexZvoleneho];
                        Plocha[naOseX, naOseY] = znakVylepseni;
                    }
                }
                _casovac = 0;
            }
        }
        public bool VolnaPozice(int x, int y)
        {
            char pozice = Plocha[x, y];
            return pozice == ' ';
        }
        public bool NeprorazitelnaZed(int x, int y)
        {
            char pozice = Plocha[x, y];
            return pozice == 'X';
        }
        public bool Lava(int x, int y)
        {
            char pozice = Plocha[x, y];
            return pozice == 'l';
        }
        public bool Nepritel(int x, int y)
        {
            char pozice = Plocha[x, y];
            return (pozice == '<') || (pozice == '>') || (pozice == 'v') || (pozice == '^');
        }
        public bool Hrac(int x, int y)
        {
            char pozice = Plocha[x, y];
            return (pozice == 'w') || (pozice == 'a') || (pozice == 's') || (pozice == 'd');
        }
        public bool Bomba(int x, int y)
        {
            char pozice = Plocha[x, y];
            return (pozice == 'B') || (pozice == 'b');
        }
        public bool Vylepseni(int x, int y)
        {
            char pozice = Plocha[x, y];
            return (pozice == 'z') || (pozice == 'S') || (pozice == 'r') || (pozice == 'k') || (pozice == 'L');
        }
        public bool Zivot(int x, int y)
        {
            char pozice = Plocha[x, y];
            return (pozice == 'z');
        }
        public bool Stit(int x, int y)
        {
            char pozice = Plocha[x, y];
            return (pozice == 'S');
        }
        public bool Batoh(int x, int y)
        {
            char pozice = Plocha[x, y];
            return (pozice == 'r');
        }
        public bool LektvarSily(int x, int y)
        {
            char pozice = Plocha[x, y];
            return (pozice == 'L');
        }
        public bool Krystal(int x, int y)
        {
            char pozice = Plocha[x, y];
            return (pozice == 'k');
        }
        public void VykresliSvujObsah(int sirka_px, int vyska_px, Graphics grafika)
        {
            int sirkaVyrezu = sirka_px / this._rozmer;
            int vyskaVyrezu = vyska_px / this._rozmer;

            sirkaVyrezu = Math.Min(sirkaVyrezu, Sirka);
            vyskaVyrezu = Math.Min(vyskaVyrezu, Vyska);

            int vyrezOsaX = Bomber.X - sirkaVyrezu / 2;

            if (vyrezOsaX < 0)
                vyrezOsaX = 0; //zobrazi od zacatku osy x
            if (vyrezOsaX + sirkaVyrezu - 1 >= this.Sirka)
                vyrezOsaX = this.Sirka - sirkaVyrezu;

            int vyrezOsaY = Bomber.Y - vyskaVyrezu / 2;

            if (vyrezOsaY < 0)
                vyrezOsaY = 0;
            if (vyrezOsaY + vyskaVyrezu - 1 >= this.Vyska)
                vyrezOsaY = this.Vyska - vyskaVyrezu;

            for (int x = 0; x < sirkaVyrezu; x++)
            {
                for (int y = 0; y < vyskaVyrezu; y++)
                {
                    int plochaX = vyrezOsaX + x;
                    int plochaY = vyrezOsaY + y;

                    char aktualniSymbol = Plocha[plochaX, plochaY];
                    int indexTextury = "sdawv><^ hDXbBpzSlLrqQtk".IndexOf(aktualniSymbol);

                    grafika.DrawImage(_textury[indexTextury], x * _rozmer, y * _rozmer);
                }
            }
        }

        public void AktualizujPlochu(Sipka sipka, bool mezernik)
        {
            this.Sipka = sipka;
            this.Mezernik = mezernik;

            foreach (DynamickyPrvek prvek in DynamickeBezHrace)
            {
                prvek.OdehrajTah();
            }

            foreach (DynamickyPrvek odstraneni in PrvkyOdstraneni)
            {
                odstraneni.HerniPlocha.OdstranDynamickyPrvek(odstraneni.X, odstraneni.Y);
            }
            PrvkyOdstraneni.Clear();

            foreach (DynamickyPrvek pridani in PrvkyPridani)
            {
                pridani.HerniPlocha.DynamickeBezHrace.Add(pridani);
            }
            PrvkyPridani.Clear();

            Bomber.OdehrajTah();
            Bomber.HerniPlocha.UmistiVylepseni();
        }
    }
}