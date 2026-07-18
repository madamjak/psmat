using PisaciStroj.Pamat;
using PisaciStroj.Parametre;
using System;
using System.Collections.Generic;

namespace PisaciStroj.Navigacia
{
    public enum TypNavigacie
    {
        Doprava,
        Dolava,
        Hore,
        Dole,
        SlovoDoprava,
        SlovoDolava,
        ZaciatokRiadku,
        ZaciatokTextu,
        KonecRiadku,
        KoniecTextu,
        DalsiaStranka,
        PredoslaStranka
    }

    public class NavigovaciPrikaz
    {
        public TypNavigacie Typ { get; set; }

        public bool Vyber { get; set; }
    }

    public static class Navigator
    {
        private static char _medzera = ' ';

        private static HashSet<char> _wordSeparators = new HashSet<char>
        {
            '.', ',', ';', '-', '_', ')', '(', '[', ']', '{', '}', '+', '/', '\\', '\'', '*', '"', '<', '>', '=', '&', '|'
        };

        public static void Naviguj(NavigovaciPrikaz prikaz, ParametreVypisu parametreVypisu, List<GapBuffer> riadky, ParametreVyberu parametreVyberu)
        {
            var posPred = new Pozicia();
            var posPo = new Pozicia();
            if (prikaz.Vyber)
            {
                posPred.Riadok = parametreVypisu.IndexRiadok;
                posPred.Stlpec = parametreVypisu.IndexStlpec;
            }

            switch (prikaz.Typ)
            {
                case TypNavigacie.Dolava:

                    Kurzor.PosunKurzorDolava(parametreVypisu, riadky);
                    break;

                case TypNavigacie.Doprava:

                    Kurzor.PosunKurzorDoprava(parametreVypisu, riadky);
                    break;

                case TypNavigacie.Hore:

                    Kurzor.PosunKurzorHore(parametreVypisu, riadky);
                    break;

                case TypNavigacie.Dole:

                    Kurzor.PosunKurzorDole(parametreVypisu, riadky);
                    break;

                case TypNavigacie.SlovoDolava:

                    if (parametreVypisu.IndexStlpec == 0)
                    {
                        Kurzor.PosunKurzorDolava(parametreVypisu, riadky);
                        break;
                    }

                    var indexStlpca = Math.Min(parametreVypisu.IndexStlpec - 1, riadky[parametreVypisu.IndexRiadok].Length() - 1);

                    var navigujNaSeparator = !_wordSeparators.Contains(riadky[parametreVypisu.IndexRiadok].CharAt(indexStlpca));
                    var jeBieleMiesto = riadky[parametreVypisu.IndexRiadok].CharAt(indexStlpca) == _medzera;

                    while (true)
                    {
                        if(!Kurzor.PosunKurzorDolava(parametreVypisu, riadky))
                        {
                            break;
                        }

                        if (parametreVypisu.IndexStlpec == 0)
                        {
                            break;
                        }

                        indexStlpca = Math.Min(parametreVypisu.IndexStlpec - 1, riadky[parametreVypisu.IndexRiadok].Length() - 1);
                        if (jeBieleMiesto)
                        {
                            if(_medzera != riadky[parametreVypisu.IndexRiadok].CharAt(indexStlpca))
                            {
                                break;
                            }
                        } else if (navigujNaSeparator)
                        {
                            if (_wordSeparators.Contains(riadky[parametreVypisu.IndexRiadok].CharAt(indexStlpca))
                                || (_medzera == riadky[parametreVypisu.IndexRiadok].CharAt(indexStlpca)))
                            {
                                break;
                            }

                        }
                        else
                        {
                            if (!_wordSeparators.Contains(riadky[parametreVypisu.IndexRiadok].CharAt(indexStlpca))
                            || (_medzera == riadky[parametreVypisu.IndexRiadok].CharAt(indexStlpca)))
                            {
                                break;
                            }
                        }
                    }
                    
                    break;

                case TypNavigacie.SlovoDoprava:

                    if (parametreVypisu.IndexStlpec == riadky[parametreVypisu.IndexRiadok].Length())
                    {
                        Kurzor.PosunKurzorDoprava(parametreVypisu, riadky);
                        break;
                    }

                    var nsp = !_wordSeparators.Contains(riadky[parametreVypisu.IndexRiadok].CharAt(parametreVypisu.IndexStlpec));
                    var jbm = riadky[parametreVypisu.IndexRiadok].CharAt(parametreVypisu.IndexStlpec) == _medzera;
                    while (true)
                    {
                        if (!Kurzor.PosunKurzorDoprava(parametreVypisu, riadky))
                        {
                            break;
                        }

                        if (jbm)
                        {
                            if (_medzera != riadky[parametreVypisu.IndexRiadok].CharAt(parametreVypisu.IndexStlpec))
                            {
                                break;
                            }
                        }
                        else if (nsp)
                        {
                            if (parametreVypisu.IndexStlpec == riadky[parametreVypisu.IndexRiadok].Length()
                                || _wordSeparators.Contains(riadky[parametreVypisu.IndexRiadok].CharAt(parametreVypisu.IndexStlpec))
                                || (_medzera == riadky[parametreVypisu.IndexRiadok].CharAt(parametreVypisu.IndexStlpec)))
                            {
                                break;
                            }
                        }
                        else
                        {
                            if (parametreVypisu.IndexStlpec == riadky[parametreVypisu.IndexRiadok].Length()
                                || !_wordSeparators.Contains(riadky[parametreVypisu.IndexRiadok].CharAt(parametreVypisu.IndexStlpec))
                                || (_medzera == riadky[parametreVypisu.IndexRiadok].CharAt(parametreVypisu.IndexStlpec)))
                            {
                                break;
                            }
                        }
                    }

                    break;

                case TypNavigacie.ZaciatokRiadku:

                    Kurzor.GoTo(parametreVypisu.IndexRiadok, 0, parametreVypisu, riadky);
                    break;

                case TypNavigacie.KonecRiadku:

                    Kurzor.GoTo(parametreVypisu.IndexRiadok, riadky[parametreVypisu.IndexRiadok].Length(), parametreVypisu, riadky);
                    break;

                case TypNavigacie.ZaciatokTextu:

                    Kurzor.GoTo(0, 0, parametreVypisu, riadky);
                    break;

                case TypNavigacie.KoniecTextu:

                    Kurzor.GoTo(riadky.Count - 1, riadky[riadky.Count - 1].Length(), parametreVypisu, riadky);
                    break;
                case TypNavigacie.DalsiaStranka:

                    //teoreticky by to slo lepsie nejak cez offset
                    var koniecDalsejStranky = Math.Min(parametreVypisu.IndexRiadok + (parametreVypisu.Vyska - 1) * 2, riadky.Count - 1);
                    Kurzor.GoTo(koniecDalsejStranky, 0, parametreVypisu, riadky);

                    var zaciatokDalsejStranky = Math.Max(parametreVypisu.IndexRiadok - parametreVypisu.Vyska + 1, 0);

                    Kurzor.GoTo(zaciatokDalsejStranky, 0, parametreVypisu, riadky);
                    break;

                case TypNavigacie.PredoslaStranka:

                    var predIndexRiadku = Math.Max(parametreVypisu.IndexRiadok - parametreVypisu.Vyska, 0);
                    Kurzor.GoTo(predIndexRiadku, 0, parametreVypisu, riadky);
                    break;
            }

            if (prikaz.Vyber)
            {
                posPo.Riadok = parametreVypisu.IndexRiadok;
                posPo.Stlpec = parametreVypisu.IndexStlpec;

                Zvyraznovac.UpravVyber(posPred, posPo, parametreVyberu);
                Zvyraznovac.SpocitajVyber(parametreVyberu, riadky);
            }
        }

        public static bool NavigovaciPrikaz(ConsoleKeyInfo vstup, NavigovaciPrikaz prikaz)
        {
            prikaz.Vyber = (vstup.Modifiers & ConsoleModifiers.Shift) != 0;

            if (vstup.Key == ConsoleKey.LeftArrow)
            {
                prikaz.Typ = TypNavigacie.Dolava;

                if ((vstup.Modifiers & ConsoleModifiers.Control) != 0)
                {
                    prikaz.Typ = TypNavigacie.SlovoDolava;
                }

                return true;
            }
            else if (vstup.Key == ConsoleKey.RightArrow)
            {
                prikaz.Typ = TypNavigacie.Doprava;

                if ((vstup.Modifiers & ConsoleModifiers.Control) != 0)
                {
                    prikaz.Typ = TypNavigacie.SlovoDoprava;
                }

                return true;
            }
            else if (vstup.Key == ConsoleKey.UpArrow)
            {
                prikaz.Typ = TypNavigacie.Hore;
                return true;
            }
            else if (vstup.Key == ConsoleKey.DownArrow)
            {
                prikaz.Typ = TypNavigacie.Dole;
                return true;
            }
            else if(vstup.Key == ConsoleKey.Home)
            {
                prikaz.Typ = TypNavigacie.ZaciatokRiadku;

                if ((vstup.Modifiers & ConsoleModifiers.Control) != 0)
                {
                    prikaz.Typ = TypNavigacie.ZaciatokTextu;
                }
                return true;
            }
            else if (vstup.Key == ConsoleKey.End)
            {
                prikaz.Typ = TypNavigacie.KonecRiadku;

                if ((vstup.Modifiers & ConsoleModifiers.Control) != 0)
                {
                    prikaz.Typ = TypNavigacie.KoniecTextu;
                }
                return true;
            }
            else if (vstup.Key == ConsoleKey.PageDown)
            {
                prikaz.Typ = TypNavigacie.DalsiaStranka;

                return true;
            }
            else if (vstup.Key == ConsoleKey.PageUp)
            {
                prikaz.Typ = TypNavigacie.PredoslaStranka;

                return true;
            }


            return false;
        }

        public static bool NavigujVPrikazovomRiadku(ConsoleKeyInfo vstup, NavigovaciPrikaz prikaz)
        {
            if(!(vstup.Key == ConsoleKey.LeftArrow || vstup.Key == ConsoleKey.RightArrow || vstup.Key == ConsoleKey.Home || vstup.Key == ConsoleKey.End))
            {
                return false;
            }

            return NavigovaciPrikaz(vstup, prikaz);
        }
    }
}
