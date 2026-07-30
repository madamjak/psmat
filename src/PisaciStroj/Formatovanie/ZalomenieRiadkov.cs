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

            var maxDlzka = parametreVypisu.Sirka - 2;
            while (riadok < riadky.Count)
            {
                var dlzka = riadky[riadok].Length();
                stlpec = Math.Min(dlzka, maxDlzka);

                if(stlpec == dlzka)
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
                    Typ = TypNavigacie.SlovoDolava
                };
                Navigator.Naviguj(p, parametreVypisu, riadky, new ParametreVyberu());

                while (true)
                {
                    if (parametreVypisu.IndexStlpec == 0 || riadky[riadok].CharAt(parametreVypisu.IndexStlpec) == ' ')
                    {
                        break;
                    }

                    Navigator.Naviguj(p, parametreVypisu, riadky, new ParametreVyberu());
                }
                
                if(parametreVypisu.IndexStlpec == 0)
                {
                    //nie jednoducho rozdelitelny riadok
                    
                    p.Typ = TypNavigacie.Doprava;
                    if(riadky[riadok].CharAt(parametreVypisu.IndexStlpec) == ' ')
                    {
                        Navigator.Naviguj(p, parametreVypisu, riadky, new ParametreVyberu());
                    }

                    while (true)
                    {
                        if (parametreVypisu.IndexStlpec == riadky[riadok].Length() || riadky[riadok].CharAt(parametreVypisu.IndexStlpec) == ' ')
                        {
                            break;
                        }

                        
                        Navigator.Naviguj(p, parametreVypisu, riadky, new ParametreVyberu());
                    }

                    if(parametreVypisu.IndexStlpec == riadky[riadok].Length())
                    {
                        
                        riadok++;
                        continue;
                    }
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
