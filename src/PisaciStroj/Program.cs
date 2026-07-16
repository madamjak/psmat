using PisaciStroj.Formatovanie;
using PisaciStroj.Navigacia;
using PisaciStroj.Pamat;
using PisaciStroj.Parametre;
using PisaciStroj.Vyhladavanie;
using System;
using System.Collections.Generic;
using System.Text;

namespace PisaciStroj
{
    public interface IPisaciStroj
    {
        List<GapBuffer> Riadky();
        
        
        void NapisZnak(char znak, ParametreVypisu parametreVypisu);
        void NapisText(string vstup, ParametreVypisu parametreVypisu);
        void NapisTextZoSuboru(string text);
        void ZmazText(ParametreVypisu parametreVypisu);
        void ZmazText(int zaciatocnyStlpecVyberu, int zaciatocnyRiadokVyberu, int konecnyStlpecVyberu, int konecnyRiadokVyberu, ParametreVypisu parametreVypisu);

        void VratPoslednuOperaciu(ParametreVypisu parametreVypisu);
        void ZopakujPoslednuOperaciu(ParametreVypisu parametreVypisu);
        
        string PrecitajText(int zaciatocnyRiadok, int zaciatocnyStlpec, int konecnyRiadok, int konecnyStlpec);
        string PrecitajText();

        VyhladaneSlovo? Vyhladaj(string vyhladavanyText, ParametreVypisu parametreVypisu);
        bool VyhladajANahrad(string vyhladavanyText, string novyText, ParametreVypisu parametreVypisu);
        bool VyhladajANahradVsetky(string vyhladavanyText, string novyText, ParametreVypisu parametreVypisu);
        Dictionary<int, VyhladaneSlovo> VyhladajVsetky(GapBuffer riadok, string vyhladavanyText);
        void NastavVyhladavanie(string vyhladavanyText);
        
        bool MaZmenu();

        void PridajOkraj(ParametreVypisu parametreVypisu);
        void PridajMultiLineOkraj(ParametreVypisu parametreVypisu, ParametreVyberu parametreVyberu);
        void ZmazOkraj(ParametreVypisu parametreVypisu, ParametreVyberu parametreVyberu);
        void ZmazMultiLineOkraj(ParametreVypisu parametreVypisu, ParametreVyberu parametreVyberu);
    }

    public class Program : IPisaciStroj
    {
        private const int DlzkaOkraja = 4;

        private PamatOperacii _pamatOperacii;
        private List<GapBuffer> _riadky;
        private IVyhladavac _vyhladavac;

        private int _okraj;

        public Program(List<GapBuffer> riadky)
        {
            _riadky = riadky;
        }

        public Program(IVyhladavac vyhladavac)
        {
            _pamatOperacii = new PamatOperacii();
            _riadky = new List<GapBuffer>() { new GapBuffer() };

            _vyhladavac = vyhladavac;
        }

        public void NapisTextZoSuboru(string vstup)
        {
            var p = new ParametreVypisu();
            foreach (var c in vstup)
            {
                if (c == ((char)ConsoleKey.Tab))
                {
                    foreach (var b in "    ")
                    {
                        NapisZnakInternal(b, p);
                    }
                }
                else
                {
                    NapisZnakInternal(c, p);
                }
            }
        }

        public void NapisText(string vstup, ParametreVypisu parametreVypisu)
        {
            _pamatOperacii.VycistiOperacieNaZopakovanie();

            var t = vstup;
            if(t == Environment.NewLine && _okraj > 0)
            {
                t = t + Indentation.NastavOkraj(_okraj);
            }

            var operacia = NapisTextInternal(t, parametreVypisu);

            _pamatOperacii.PridajOperaciuNaVratenie(operacia);
        }

        public void NapisZnak(char vstup, ParametreVypisu parametreVypisu)
        {
            NapisText(new string(new char[] { vstup }), parametreVypisu);
        }

        public void ZmazText(ParametreVypisu parametreVypisu)
        {
            var konecnyRiadok = parametreVypisu.IndexRiadok;
            var konecnyStlpec = parametreVypisu.IndexStlpec;
            Kurzor.PosunKurzorDolava(parametreVypisu, _riadky);

            var zaciatocnyRiadok = parametreVypisu.IndexRiadok;
            var zaciatocnyStlpec = parametreVypisu.IndexStlpec;
            Kurzor.PosunKurzorDoprava(parametreVypisu, _riadky);

            if (SpravneIndexy(zaciatocnyStlpec, zaciatocnyRiadok, konecnyStlpec, konecnyRiadok))
            {
                ZmazText(zaciatocnyStlpec, zaciatocnyRiadok, konecnyStlpec, konecnyRiadok, parametreVypisu);
            }
            else
            {
                Kurzor.PosunKurzorDolava(parametreVypisu, _riadky);
            }
        }

        private bool SpravneIndexy(int zaciatocnyStlpec, int zaciatocnyRiadok, int konecnyStlpec, int konecnyRiadok)
        {
            return (konecnyRiadok == zaciatocnyRiadok && konecnyStlpec > zaciatocnyStlpec) || (konecnyRiadok > zaciatocnyRiadok);
        }

        public void ZmazText(int zaciatocnyStlpecVyberu, int zaciatocnyRiadokVyberu, int konecnyStlpecVyberu, int konecnyRiadokVyberu, ParametreVypisu parametreVypisu)
        {
            _pamatOperacii.VycistiOperacieNaZopakovanie();

            Kurzor.GoTo(zaciatocnyRiadokVyberu, zaciatocnyStlpecVyberu, parametreVypisu, _riadky);

            var operacia = ZmazTextInternal(konecnyStlpecVyberu, konecnyRiadokVyberu, zaciatocnyStlpecVyberu, zaciatocnyRiadokVyberu);

            _pamatOperacii.PridajOperaciuNaVratenie(operacia);
        }

        private Operacia NapisTextInternal(string vstup, ParametreVypisu parametreVypisu)
        {
            var operacia = new Operacia()
            {
                Typ = TypOperacie.Zapis,
                ZaciatocnyRiadok = parametreVypisu.IndexRiadok,
                ZaciatocnyStlpec = parametreVypisu.IndexStlpec
            };

            foreach (var c in vstup)
            {
                if (c == ((char)ConsoleKey.Tab))
                {
                    foreach (var b in "    ")
                    {
                        NapisZnakInternal(b, parametreVypisu);
                    }
                }
                else
                {
                    NapisZnakInternal(c, parametreVypisu);
                }
            }

            operacia.KonecnyRiadok = parametreVypisu.IndexRiadok;
            operacia.KonecnyStlpec = parametreVypisu.IndexStlpec;

            return operacia;
        }

        private void NapisZnakInternal(char c, ParametreVypisu parametreVypisu)
        {
            var koniecRiadka = LineFeed(c);
            var zaciatokNovehoRiadka = CarriageReturn(c);

            if (zaciatokNovehoRiadka)
            {
                return;
            }

            var riadok = _riadky[parametreVypisu.IndexRiadok];

            if (koniecRiadka)
            {
                var novyRiadok = new GapBuffer();

                if (parametreVypisu.IndexStlpec < riadok.Length())
                {
                    var text = riadok.Read(parametreVypisu.IndexStlpec);

                    riadok.Delete(parametreVypisu.IndexStlpec, text.Length);

                    novyRiadok.Append(text);
                }

                if (parametreVypisu.IndexRiadok == _riadky.Count - 1)
                {
                    _riadky.Add(novyRiadok);
                }
                else
                {
                    _riadky.Insert(parametreVypisu.IndexRiadok + 1, novyRiadok);
                }
            }
            else
            {
                riadok.Insert(c, parametreVypisu.IndexStlpec);
            }

            Kurzor.PosunKurzorDoprava(parametreVypisu, _riadky);
        }

        public void VratPoslednuOperaciu(ParametreVypisu parametreVypisu)
        {
            if (_pamatOperacii.PocetOperaciiNaVratenie == 0)
            {
                return;
            }

            var operacia = _pamatOperacii.OperaciaNaVratenie();

            if (operacia.Typ == TypOperacie.Zapis)
            {
                Kurzor.GoTo(operacia.ZaciatocnyRiadok, operacia.ZaciatocnyStlpec, parametreVypisu, _riadky);

                var operaciaNaZopakovanie = ZmazTextInternal(operacia.KonecnyStlpec, operacia.KonecnyRiadok, operacia.ZaciatocnyStlpec, operacia.ZaciatocnyRiadok);

                _pamatOperacii.PridajOperaciuNaZopakovanie(operaciaNaZopakovanie);
            }
            else if (operacia.Typ == TypOperacie.Mazanie)
            {
                Kurzor.GoTo(operacia.ZaciatocnyRiadok, operacia.ZaciatocnyStlpec, parametreVypisu, _riadky);

                var operaciaNaZopakovanie = NapisTextInternal(operacia.ZmazanaCastTextu, parametreVypisu);

                _pamatOperacii.PridajOperaciuNaZopakovanie(operaciaNaZopakovanie);
            }
            else if (operacia.Typ == TypOperacie.VyhladajNahrad 
                || operacia.Typ == TypOperacie.VyhladajNahradVsetky
                || operacia.Typ == TypOperacie.OdsekZvyraznenehoTextu)
            {
                var pocetOperacii = operacia.PocetOperacii;
                while (true)
                {
                    if(pocetOperacii == 0)
                    {
                        break;
                    }

                    VratPoslednuOperaciu(parametreVypisu);
                    pocetOperacii--;

                    if(operacia.Typ == TypOperacie.OdsekZvyraznenehoTextu)
                    {
                        _okraj--; 
                    }
                }

                _pamatOperacii.PridajOperaciuNaZopakovanie(operacia);
            }
        }

        public void ZopakujPoslednuOperaciu(ParametreVypisu parametreVypisu)
        {
            if (_pamatOperacii.PocetOperaciiNaZopakovanie == 0)
            {
                return;
            }

            var operacia = _pamatOperacii.OperaciaNaZopakovanie();

            if (operacia.Typ == TypOperacie.Zapis)
            {
                var operaciaNaVratenie = ZmazTextInternal(operacia.KonecnyStlpec, operacia.KonecnyRiadok, operacia.ZaciatocnyStlpec, operacia.ZaciatocnyRiadok);

                _pamatOperacii.PridajOperaciuNaVratenie(operaciaNaVratenie);

                Kurzor.GoTo(operacia.ZaciatocnyRiadok, operacia.ZaciatocnyStlpec, parametreVypisu, _riadky);
            }
            else if(operacia.Typ == TypOperacie.Mazanie)
            {
                Kurzor.GoTo(operacia.ZaciatocnyRiadok, operacia.ZaciatocnyStlpec, parametreVypisu, _riadky);

                var operaciaNaVratenie = NapisTextInternal(operacia.ZmazanaCastTextu, parametreVypisu);

                _pamatOperacii.PridajOperaciuNaVratenie(operaciaNaVratenie);
            }
            else if (operacia.Typ == TypOperacie.VyhladajNahrad 
                || operacia.Typ == TypOperacie.VyhladajNahradVsetky 
                || operacia.Typ == TypOperacie.OdsekZvyraznenehoTextu)
            {
                var pocetOperacii = operacia.PocetOperacii;
                while (true)
                {
                    if (pocetOperacii == 0)
                    {
                        break;
                    }

                    ZopakujPoslednuOperaciu(parametreVypisu);
                    pocetOperacii--;

                    if (operacia.Typ == TypOperacie.OdsekZvyraznenehoTextu)
                    {
                        _okraj++;
                    }
                }

                _pamatOperacii.PridajOperaciuNaVratenie(operacia);
            }
        }

        private Operacia ZmazTextInternal(int konecnyStlpec, int konecnyRiadok, int zaciatocnyStlpec, int zaciatocnyRiadok)
        {
            var operacia = new Operacia
            {
                KonecnyStlpec = konecnyStlpec,
                KonecnyRiadok = konecnyRiadok,
                ZaciatocnyStlpec = zaciatocnyStlpec,
                ZaciatocnyRiadok = zaciatocnyRiadok,
                Typ = TypOperacie.Mazanie,
                ZmazanaCastTextu = PrecitajTextInternal(zaciatocnyRiadok, zaciatocnyStlpec, konecnyRiadok, konecnyStlpec, true)
            };

            return operacia;
        }

        public string PrecitajText(int zaciatocnyRiadok, int zaciatocnyStlpec, int konecnyRiadok, int konecnyStlpec)
        {
            return PrecitajTextInternal(zaciatocnyRiadok, zaciatocnyStlpec, konecnyRiadok, konecnyStlpec, false);
        }

        private string PrecitajTextInternal(int zaciatocnyRiadok, int zaciatocnyStlpec, int konecnyRiadok, int konecnyStlpec, bool zmazPrecitany)
        {
            var sb = new StringBuilder();

            var riadkyNaZmazanie = new List<int>();

            //mazanie znaku alebo na jednom riadku
            if(zaciatocnyRiadok == konecnyRiadok)
            {
                var s = _riadky[zaciatocnyRiadok].Read(zaciatocnyStlpec, konecnyStlpec - zaciatocnyStlpec);

                if (zmazPrecitany)
                {
                    _riadky[zaciatocnyRiadok].Delete(zaciatocnyStlpec, konecnyStlpec - zaciatocnyStlpec);
                }

                return s;
            }
            else
            {
                //mazanie riadkov a mozny join
                sb.AppendLine(_riadky[zaciatocnyRiadok].Read(zaciatocnyStlpec));

                if (zmazPrecitany)
                {
                    _riadky[zaciatocnyRiadok].Delete(zaciatocnyStlpec, _riadky[zaciatocnyRiadok].Length() - zaciatocnyStlpec);
                }
                
                for(int i = zaciatocnyRiadok + 1; i < konecnyRiadok; i++)
                {
                    sb.AppendLine(_riadky[i].Read());

                    if (zmazPrecitany)
                    {
                        riadkyNaZmazanie.Add(i);
                    }
                }

                sb.Append(_riadky[konecnyRiadok].Read(0, konecnyStlpec));
                
                if (zmazPrecitany)
                {
                    _riadky[konecnyRiadok].Delete(0, konecnyStlpec);

                    if(_riadky[konecnyRiadok].Length() > 0)
                    {
                        _riadky[zaciatocnyRiadok].Append(_riadky[konecnyRiadok].Read());
                    }

                     riadkyNaZmazanie.Add(konecnyRiadok);
                }
            }

            if (riadkyNaZmazanie.Count > 0)
            {
                foreach(var index in riadkyNaZmazanie)
                {
                    _riadky.RemoveAt(riadkyNaZmazanie[0]);
                }
            }

            return sb.ToString();
        }

        public VyhladaneSlovo? Vyhladaj(string vyhladavanyText, ParametreVypisu parametreVypisu)
        {
            VyhladaneSlovo? vyhladaneSlovo = null;

            var vyhladanyRiadok = 0;
            for (int i = parametreVypisu.IndexRiadok; i < _riadky.Count; i++)
            {
                var index = i == parametreVypisu.IndexRiadok ? parametreVypisu.IndexStlpec : 0;
                vyhladaneSlovo = _vyhladavac.VyhladajNasledujuci(_riadky[i], index, vyhladavanyText);

                if (vyhladaneSlovo.HasValue)
                {
                    return new VyhladaneSlovo()
                    {
                        Riadok = i,
                        Pozicia = vyhladaneSlovo.Value.Pozicia,
                        Dlzka = vyhladaneSlovo.Value.Dlzka
                    };
                }
            }

            return vyhladaneSlovo;
        }

        public bool VyhladajANahrad(string vyhladavanyText, string novyText, ParametreVypisu parametreVypisu)
        {
            return VyhladajANahrad(parametreVypisu.IndexRiadok, parametreVypisu.IndexStlpec, vyhladavanyText, novyText, parametreVypisu);
        }


        private bool VyhladajANahrad(int zaciatocnyRiadok, int zaciatocnyStlpec, string vyhladavanyText, string novyText,  ParametreVypisu parametreVypisu)
        {
            var riadok = zaciatocnyRiadok;

            VyhladaneSlovo? vyhladaneSlovo = null;
            int vyhladanyRiadok = 0;
            for(int i = riadok; i < _riadky.Count; i++)
            {
                var index = i == riadok ? zaciatocnyStlpec : 0;
                vyhladaneSlovo = _vyhladavac.VyhladajNasledujuci(_riadky[i], index, vyhladavanyText);

                if (vyhladaneSlovo.HasValue)
                {
                    vyhladanyRiadok = i;
                    break;
                }
            }

            if (vyhladaneSlovo.HasValue)
            {
                var operacia = new Operacia()
                {
                    Typ = TypOperacie.VyhladajNahrad
                };

                ZmazText(vyhladaneSlovo.Value.Pozicia, vyhladanyRiadok, vyhladaneSlovo.Value.Pozicia + vyhladaneSlovo.Value.Dlzka, vyhladanyRiadok, parametreVypisu);
                operacia.PocetOperacii++;

                Kurzor.GoTo(vyhladanyRiadok, vyhladaneSlovo.Value.Pozicia, parametreVypisu, _riadky);

                NapisText(novyText, parametreVypisu);
                operacia.PocetOperacii++;

                _pamatOperacii.VycistiOperacieNaZopakovanie();
                _pamatOperacii.PridajOperaciuNaVratenie(operacia);

                return true;
            }

            return false;
        }

        public bool VyhladajANahradVsetky(string vyhladavanyText, string novyText, ParametreVypisu parametreVypisu)
        {
            var operacia = new Operacia()
            {
                Typ = TypOperacie.VyhladajNahradVsetky
            };

            var vyhladavanieZlyhalo = false;
            vyhladavanieZlyhalo = VyhladajANahrad(0, 0, vyhladavanyText, novyText, parametreVypisu);

            if (!vyhladavanieZlyhalo)
            {
                operacia.PocetOperacii++;
            }

            while (true)
            {
                vyhladavanieZlyhalo = VyhladajANahrad(parametreVypisu.IndexRiadok, parametreVypisu.IndexStlpec, vyhladavanyText, novyText, parametreVypisu);

                if (!vyhladavanieZlyhalo)
                {
                    break;
                }
                else
                {
                    operacia.PocetOperacii++;
                }
            }

            if(operacia.PocetOperacii > 0)
            {
                _pamatOperacii.PridajOperaciuNaVratenie(operacia);
                return true;
            }

            return false;
        }

        public List<GapBuffer> Riadky()
        {
            return _riadky;
        }

        public string PrecitajText()
        {
            var sb = new StringBuilder();

            foreach (var riadok in _riadky)
            {
                sb.AppendLine(riadok.Read());
            }

            return sb.ToString();
        }

        public static bool LineFeed(char c)
        {
            return c == '\n';
        }

        public static bool CarriageReturn(char c)
        {
            return c == '\r';
        }

        public Dictionary<int, VyhladaneSlovo> VyhladajVsetky(GapBuffer riadok, string vyhladavanyText)
        {
            return _vyhladavac.VyhladajVsetky(riadok, vyhladavanyText);
        }

        public void NastavVyhladavanie(string vyhladavanyText)
        {
            _vyhladavac.NastavVyhladavaciAutomat(vyhladavanyText);
        }

        public bool MaZmenu()
        {
            return _pamatOperacii.PocetOperaciiNaVratenie > 0;
        }

        public void PridajOkraj(ParametreVypisu parametreVypisu)
        {
            NapisText("    ", parametreVypisu);
            _okraj += DlzkaOkraja;
        }

        public void PridajMultiLineOkraj(ParametreVypisu parametreVypisu, ParametreVyberu parametreVyberu)
        {
            var operacia = new Operacia()
            {
                Typ = TypOperacie.OdsekZvyraznenehoTextu
            };

            NapisText("    ", parametreVypisu);
            operacia.PocetOperacii++;
            var okraj = DlzkaOkraja;

            var zaciatocnyRiadok = parametreVyberu.Zaciatok.Value.Riadok;
            var konecnyRiadok = parametreVyberu.Koniec.Value.Riadok;
            var vratSaNariadok = 0; var vratSaNaStlpec = 0;
            if(parametreVypisu.IndexRiadok == zaciatocnyRiadok)
            {
                vratSaNariadok = zaciatocnyRiadok;
                zaciatocnyRiadok++;
            }
            else
            {
                vratSaNariadok = konecnyRiadok;
                konecnyRiadok--;
            }

            vratSaNaStlpec = parametreVypisu.IndexStlpec;

            var pocetRiadkovNaPosun = zaciatocnyRiadok == konecnyRiadok ? 1 : konecnyRiadok - zaciatocnyRiadok + 1;
            var i = 0;
            var aktualnyRiadok = zaciatocnyRiadok;
            while (true)
            {
                if(i == pocetRiadkovNaPosun)
                {
                    break;
                }

                Kurzor.GoTo(aktualnyRiadok, 0, parametreVypisu, _riadky);

                if (_riadky[aktualnyRiadok].CharAt(parametreVypisu.IndexStlpec) == ' ')
                {
                    Navigator.Naviguj(new NavigovaciPrikaz() 
                    {
                        Typ = TypNavigacie.SlovoDoprava
                    }, parametreVypisu, _riadky, parametreVyberu);
                }

                NapisText("    ", parametreVypisu);
                operacia.PocetOperacii++;

                aktualnyRiadok++;
                i++;
            }

            _pamatOperacii.PridajOperaciuNaVratenie(operacia);

            Zvyraznovac.PosunVyberDoprava(parametreVyberu, okraj);
            Kurzor.GoTo(vratSaNariadok, vratSaNaStlpec, parametreVypisu, _riadky);

            _okraj += okraj;
        }

        private int ZmazOkrajInternal(ParametreVypisu parametreVypisu, ParametreVyberu parametreVyberu)
        {
            //v pripade ze je kurzor uprostred slova nerob nic
            var i = parametreVypisu.IndexStlpec;
            var riadok = _riadky[parametreVypisu.IndexRiadok];
            if (i == riadok.Length())
            {
                i--;
            }

            if (riadok.CharAt(i) != ' ')
            {
                i--;
                if (i < 0)
                {
                    return 0;
                }

                if (riadok.CharAt(i) != ' ')
                {
                    return 0;
                }
            }

            var zaciatocnyStlpec = Indentation.VypocitajZaciatokOkrajaNaZmazanie(riadok, i, DlzkaOkraja);
            if (zaciatocnyStlpec > 0)
            {
                zaciatocnyStlpec++;
            }
            var konecnyStlpec = i == parametreVypisu.IndexStlpec ? i : i + 1;
            var pocetZnakov = konecnyStlpec - zaciatocnyStlpec;

            ZmazText(zaciatocnyStlpec, parametreVypisu.IndexRiadok, konecnyStlpec, parametreVypisu.IndexRiadok, parametreVypisu);

            return pocetZnakov;
        }

        public void ZmazOkraj(ParametreVypisu parametreVypisu, ParametreVyberu parametreVyberu)
        {
            var pocetZnakov = ZmazOkrajInternal(parametreVypisu, parametreVyberu);

            if (Zvyraznovac.MaVybranyText(parametreVyberu))
            {
                Zvyraznovac.PosunVyberDolava(parametreVyberu, pocetZnakov);
            }

            _okraj -= pocetZnakov;
        }

        public void ZmazMultiLineOkraj(ParametreVypisu parametreVypisu, ParametreVyberu parametreVyberu)
        {
            var operacia = new Operacia()
            {
                Typ = TypOperacie.ZmazOdsekZvyraznenehoTextu
            };

            var pocetZnakov = ZmazOkrajInternal(parametreVypisu, parametreVyberu);
            operacia.PocetOperacii++;

            var zaciatocnyRiadok = parametreVyberu.Zaciatok.Value.Riadok;
            var konecnyRiadok = parametreVyberu.Koniec.Value.Riadok;
            var vratSaNariadok = 0; var vratSaNaStlpec = 0;
            if (parametreVypisu.IndexRiadok == zaciatocnyRiadok)
            {
                vratSaNariadok = zaciatocnyRiadok;
                zaciatocnyRiadok++;
            }
            else
            {
                vratSaNariadok = konecnyRiadok;
                konecnyRiadok--;
            }

            vratSaNaStlpec = parametreVypisu.IndexStlpec;

            var pocetRiadkovNaPosun = zaciatocnyRiadok == konecnyRiadok ? 1 : konecnyRiadok - zaciatocnyRiadok + 1;
            var i = 0;
            var aktualnyRiadok = zaciatocnyRiadok;
            while (true)
            {
                if (i == pocetRiadkovNaPosun)
                {
                    break;
                }

                Kurzor.GoTo(aktualnyRiadok, 0, parametreVypisu, _riadky);

                if (_riadky[aktualnyRiadok].CharAt(parametreVypisu.IndexStlpec) == ' ')
                {
                    Navigator.Naviguj(new NavigovaciPrikaz()
                    {
                        Typ = TypNavigacie.SlovoDoprava
                    }, parametreVypisu, _riadky, parametreVyberu);
                }

                ZmazOkrajInternal(parametreVypisu, parametreVyberu);
                operacia.PocetOperacii++;

                aktualnyRiadok++;
                i++;
            }

            _pamatOperacii.PridajOperaciuNaVratenie(operacia);

            Zvyraznovac.PosunVyberDolava(parametreVyberu, pocetZnakov);
            Kurzor.GoTo(vratSaNariadok, vratSaNaStlpec, parametreVypisu, _riadky);

            _okraj -= pocetZnakov;
        }
    }
}
