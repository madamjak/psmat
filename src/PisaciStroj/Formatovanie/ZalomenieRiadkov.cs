using PisaciStroj.Navigacia;
using PisaciStroj.Parametre;
using System;

namespace PisaciStroj.Formatovanie
{
    public static class ZalomenieRiadkov
    {
        public static int Zalom(IPisaciStroj editor, ParametreVypisu parametreVypisu)
        {
            var pocetNovychRiadkov = 0;
            var riadok = 0;
            var stlpec = 0;

            var riadky = editor.Riadky();
            Kurzor.GoTo(riadok, stlpec, parametreVypisu, riadky);

            while(riadok < riadky.Count)
            {
                stlpec = Math.Min(riadky[riadok].Length(), parametreVypisu.Sirka);

                if(stlpec < parametreVypisu.Sirka)
                {
                    riadok++;
                    continue;
                }

                Kurzor.GoTo(riadok, stlpec, parametreVypisu, riadky);
                if (riadky[riadok].CharAt(stlpec) == ' ')
                {
                    editor.NapisText(Environment.NewLine, parametreVypisu);
                    pocetNovychRiadkov++;
                    riadok++;
                    continue;
                }

                
                var p = new NavigovaciPrikaz()
                {
                    Typ = TypNavigacie.SlovoDoprava
                };
                Navigator.Naviguj(p, parametreVypisu, riadky, new ParametreVyberu());

                var koniecSlova = parametreVypisu.IndexStlpec;

                while (true)
                {
                    p.Typ = TypNavigacie.SlovoDolava;
                    Navigator.Naviguj(p, parametreVypisu, riadky, new ParametreVyberu());

                    if (riadky[riadok].CharAt(parametreVypisu.IndexStlpec) == ' ')
                    {
                        break;
                    }
                }

                var zaciatokSlova = parametreVypisu.IndexStlpec;

                //nie jednoducho rozdelitelny riadok
                if(koniecSlova - zaciatokSlova >= parametreVypisu.Sirka)
                {
                    Kurzor.GoTo(riadok, koniecSlova, parametreVypisu, riadky);
                }

                editor.NapisText(Environment.NewLine, parametreVypisu);
                pocetNovychRiadkov++;
                riadok++;
                continue;
            }

            return pocetNovychRiadkov;
        }
    }
}
