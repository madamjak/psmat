using PisaciStroj.Navigacia;
using PisaciStroj.Pamat;
using PisaciStroj.Parametre;
using PisaciStroj.Vyhladavanie;
using System;
using System.Collections.Generic;
using System.Text;

namespace PisaciStroj
{
    public class Program
    {
        private PamatOperacii _pamatOperacii;
        private List<GapBuffer> _riadky;
        private IVyhladavac _vyhladavac;

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

            var operacia = NapisTextInternal(vstup, parametreVypisu);

            _pamatOperacii.PridajOperaciuNaVratenie(operacia);
        }

        public void NapisZnak(char vstup, ParametreVypisu parametreVypisu)
        {
            NapisText(new string(new char[] { vstup }), parametreVypisu);
        }

        public void ZmazText(ParametreVypisu parametreVypisu)
        {
            //TODO mohlo by to byt zrozumitelnejsie...
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
            else if (operacia.Typ == TypOperacie.VyhladajNahrad || operacia.Typ == TypOperacie.VyhladajNahradVsetky)
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
            else if (operacia.Typ == TypOperacie.VyhladajNahrad || operacia.Typ == TypOperacie.VyhladajNahradVsetky)
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

                sb.AppendLine(_riadky[konecnyRiadok].Read(0, konecnyStlpec));
                if (zmazPrecitany)
                {
                    _riadky[konecnyRiadok].Delete(0, konecnyStlpec);

                    _riadky[zaciatocnyRiadok].Append(_riadky[konecnyRiadok].Read(konecnyStlpec));

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


        public bool VyhladajANahrad(int zaciatocnyRiadok, int zaciatocnyStlpec, string vyhladavanyText, string novyText,  ParametreVypisu parametreVypisu)
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

        private bool LineFeed(char c)
        {
            return c == '\n';
        }

        private bool CarriageReturn(char c)
        {
            return c == '\r';
        }
    }
}
