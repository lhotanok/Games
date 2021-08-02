using System;
using System.Drawing;
using System.Windows.Forms;

namespace BomberStreet
{
    public partial class Form1 : Form
    {
        readonly Random _rnd = new Random();
        Graphics _grafika;
        HerniPlocha _herniPlocha;
        int _level;
        int _pocetLevelu = 2;
        int _pocatecniRadek;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _grafika = CreateGraphics();
            _herniPlocha = new HerniPlocha(_pocatecniRadek, "plocha.txt", "textury.png", _rnd); //dalsi level n
            timer1.Enabled = true;
            panel1.Visible = false;
            this.Text = $@"Počet životů: {_herniPlocha.Bomber.Zivoty}      Počet životů nepřítele: {_herniPlocha.CelkoveZivotyNepratel}";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        Sipka _sipka = Sipka.Nestisknuta; //pohyb hrace
        bool _mezernik; //umisteni bomby hracem
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) //kontrola stisku sipek / mezerniku
        {
            if (keyData == Keys.Right)
            {
                _sipka = Sipka.Vpravo;
                return true;
            }
            if (keyData == Keys.Left)
            {
                _sipka = Sipka.Vlevo;
                return true;
            }
            if (keyData == Keys.Up)
            {
                _sipka = Sipka.Nahoru;
                return true;
            }
            if (keyData == Keys.Down)
            {
                _sipka = Sipka.Dolu;
                return true;
            }
            if (keyData == Keys.Space)
            {
                _mezernik = true;
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            switch (_herniPlocha.Stav)
            {
                case Stav.RozehranaHra:
                    _herniPlocha.AktualizujPlochu(_sipka, _mezernik);
                    _herniPlocha.VykresliSvujObsah(ClientSize.Width, ClientSize.Height, _grafika);
                    this.Text = $@"Počet životů: {_herniPlocha.Bomber.Zivoty}      Počet životů nepřátel: {_herniPlocha.CelkoveZivotyNepratel}";
                    break;
                case Stav.Vyhra:
                    timer1.Enabled = false;
                    panel1.Visible = true;
                    button1.Text = @"Nová hra";
                    _level = (_level + 1) % _pocetLevelu; //po projdeni dostupnych levelu se vrati na level 0
                    if (_level != 0)
                    {
                        button1.Text = @"Nový level"; //pri vyhre postup do dalsiho levelu, pokud je dostupny
                        _pocatecniRadek += _herniPlocha.Vyska + 4; //posuneme se v textovem souboru o vysku aktualni plochy a 4 radky (údaje o vysce, sirce, 2 prazdne radky)
                    }
                    else
                    {
                        _pocatecniRadek = 0;
                    }
                    MessageBox.Show(text: @"Vyhráli jste.");
                    this.Text = @"Bomber Street";
                    break;
                case Stav.Prohra:
                    timer1.Enabled = false;
                    panel1.Visible = true;
                    button1.Text = @"Nová hra"; //opakovani aktualniho levelu
                    const string text1 = "Prohráli jste.";
                    _ = MessageBox.Show(text1);
                    this.Text = @"Bomber Street";
                    break;
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            _sipka = Sipka.Nestisknuta;
            _mezernik = false;
        }
    }
}
