using PisaciAutomat.Config;
using PisaciAutomat.Obrazovka;
using PisaciStroj.Lexer;
using PisaciStroj.Navigacia;
using PisaciStroj.Pamat;
using PisaciStroj.Parametre;
using PisaciStroj.Vyhladavanie;
using System;
using System.Collections.Generic;
using System.Text;

namespace PisaciAutomat.Prikazy.Vykreslovanie
{
    public class VykreslovacCmd
    {
        private ILexer _lexer;
        
        private string _aktualnyRiadok;

        private LexResult _tokeny;

        private bool _chyba;

        public VykreslovacCmd(ILexer lexer)
        {
            _lexer = lexer;
        }

        public string PrecitajPrikazovyRiadok(ParametrePrekreslenia p,
            ParametrePrekreslenia p2, //cmd parametre
            GapBuffer riadok,
            ParametreVypisu parametreVypisu,
            ParametreVyberu vyber,
            List<GapBuffer> riadky,
            bool chyba,
            bool chybaReset)
        {
            if(p2 != null && p2.Necitaj)
            {
                return string.Empty;
            }

            if (p2 != null && p2.OptimalizaciaPrekreslenia && !chybaReset)
            {
                _tokeny = _lexer.LexPrePrikazovyRiadok(riadky);
                PrekresliUpravenyRiadok(p2, parametreVypisu, vyber, riadok, riadky);

                return string.Empty;
            }

            var sb = new StringBuilder();
            var pozadie = Farby.FarbaPrikazRiadku();
            if (chyba)
            {
                pozadie = Farby.FarbaPozadia.CervenaLight;
            }

            sb.Append(VykreslovaciAutomat.NastavKurzor(1, 1));
            sb.Append(VykreslovaciAutomat.ZmazOdKurzoraPoKoniecRiadku());

            sb.Append(VykreslovaciAutomat.NastavPozadie(p.OkrajVlavo - 2));
            sb.Append(Farby.AnsiStyl(Farby.FarbaIndikatoraPrikazRiadku()));
            sb.Append("> ");


            if (riadok.Length() > 0)
            {
                _tokeny = _lexer.LexPrePrikazovyRiadok(riadky);
                PrecitajRiadok(riadok, _tokeny, parametreVypisu, vyber, sb, pozadie, parametreVypisu.OffsetStlpec, parametreVypisu.Sirka - 1);
            }

            if (riadok.Length() < parametreVypisu.Sirka)
            {
                sb.Append(Farby.AnsiStyl(pozadie));
                sb.Append(VykreslovaciAutomat.NastavPozadie(parametreVypisu.Sirka - riadok.Length()));
                sb.Append(Farby.AnsiReset());
            }

            _aktualnyRiadok = sb.ToString();

            return _aktualnyRiadok;
        }

        private void PrekresliUpravenyRiadok(ParametrePrekreslenia parametrePrekreslenia, 
            ParametreVypisu parametre, 
            ParametreVyberu parametreVyberu,
            GapBuffer riadok,
            List<GapBuffer> riadky)
        {
            var pocetZnakov = parametrePrekreslenia.KonecnySlpec - parametrePrekreslenia.ZaciatocnyStlpec;
            var mazanie = false;
            if (pocetZnakov < 0)
            {
                mazanie = true;
            }

            var indexRiadok = parametre.IndexRiadok;
            var indexStlpec = parametre.IndexStlpec;

            var sb = new StringBuilder();
            sb.Append(VykreslovaciAutomat.NastavKurzorUnVisible());
            if (mazanie && !parametrePrekreslenia.LenPrekresli)
            {
                var pocetPotrebnych = Math.Abs(pocetZnakov);
                sb.Append(VykreslovaciAutomat.NastavKurzor(parametre.RiadokKurzora + 1, parametre.StlpecKurzora + 1));
                VykreslovaciAutomat.DeleteCharacterShiftTextLeft(pocetPotrebnych, sb);

                //vyber aktualne slovo
                var _navigovaciPrikaz = new NavigovaciPrikaz();
                var _parametreVyberu = new ParametreVyberu();
                if (parametre.IndexStlpec < riadok.Length() - 1)
                {
                    _navigovaciPrikaz.Vyber = false;
                    _navigovaciPrikaz.Typ = TypNavigacie.SlovoDoprava;

                    Navigator.Naviguj(_navigovaciPrikaz, parametre, riadky, _parametreVyberu);
                }

                _navigovaciPrikaz.Vyber = true;
                _navigovaciPrikaz.Typ = TypNavigacie.SlovoDolava;
                _parametreVyberu = new ParametreVyberu();

                if (parametre.IndexStlpec > 0)
                {
                    Navigator.Naviguj(_navigovaciPrikaz, parametre, riadky, _parametreVyberu);
                }
                if (parametre.IndexStlpec > 0)
                {
                    Navigator.Naviguj(_navigovaciPrikaz, parametre, riadky, _parametreVyberu);
                }
                if (parametre.IndexStlpec > 0)
                {
                    Navigator.Naviguj(_navigovaciPrikaz, parametre, riadky, _parametreVyberu);
                }

                if (_parametreVyberu.Zaciatok.HasValue)
                {
                    Kurzor.GoTo(indexRiadok, _parametreVyberu.Zaciatok.Value.Stlpec, parametre, riadky);
                }

                sb.Append(VykreslovaciAutomat.NastavKurzor(parametre.RiadokKurzora + 1, parametre.StlpecKurzora + 1));


                var stlpecCitania = _parametreVyberu.Zaciatok.HasValue ? _parametreVyberu.Zaciatok.Value.Stlpec : parametre.IndexStlpec - 1;
                var pocet = _parametreVyberu.Zaciatok.HasValue ? _parametreVyberu.PocetZnakov : 1;
                var stlpecPisania = _parametreVyberu.Zaciatok.HasValue ? parametre.StlpecKurzora + 1 : parametre.StlpecKurzora;

                var uprava = PrecitajRiadok(parametre,
                                parametreVyberu,
                                riadky,
                                stlpecCitania,
                                pocet);


                sb.Append(VykreslovaciAutomat.NastavKurzor(parametre.RiadokKurzora + 1, stlpecPisania));
                VykreslovaciAutomat.ShiftTextRightAndInsert(uprava, pocetZnakov, sb);

                var stlpec = VykreslovaciAutomat.KonecnyStlpecRiadkuObrazovky(parametre, riadok.Length());

                if (stlpec.HasValue)
                {
                    //ak treba zaplnit prazdne miesto na konci riadku
                    uprava = PrecitajRiadok(parametre,
                                parametreVyberu,
                                riadky,
                                stlpecCitania,
                                pocet);

                    sb.Append(VykreslovaciAutomat.NastavKurzor(parametre.RiadokKurzora + 1, stlpec.Value.StlpecKurzora + 1));
                    VykreslovaciAutomat.ShiftTextRightAndInsert(uprava, pocetPotrebnych, sb);
                }
                else
                {
                    sb.Append(VykreslovaciAutomat.NastavKurzor(parametre.RiadokKurzora + 1, parametre.SirkaKonzoly));
                    VykreslovaciAutomat.ShiftTextRightAndInsert(VykreslovaciAutomat.NastavPozadie(1, Farby.FarbaPrikazRiadku()), 2, sb);
                }

                Kurzor.GoTo(indexRiadok, indexStlpec, parametre, riadky);
                sb.Append(VykreslovaciAutomat.NastavKurzor(parametre.RiadokKurzora + 1, parametre.StlpecKurzora + 1));
            }
            else if (parametrePrekreslenia.LenPrekresli)
            {
                var stlpecCitania = parametrePrekreslenia.ZaciatocnyStlpec;
                if (parametrePrekreslenia.KonecnySlpec < parametrePrekreslenia.ZaciatocnyStlpec)
                {
                    stlpecCitania = parametrePrekreslenia.KonecnySlpec;
                }
                var pocet = Math.Abs(parametrePrekreslenia.KonecnySlpec - parametrePrekreslenia.ZaciatocnyStlpec);

                var uprava = PrecitajRiadok(parametre,
                                parametreVyberu,
                                riadky,
                                stlpecCitania,
                                pocet);

                Kurzor.GoTo(parametre.IndexRiadok, stlpecCitania, parametre, riadky);
                sb.Append(VykreslovaciAutomat.NastavKurzor(parametre.RiadokKurzora + 1, parametre.StlpecKurzora + 1));
                VykreslovaciAutomat.DeleteCharacterShiftTextLeft(pocet, sb);
                sb.Append(VykreslovaciAutomat.NastavKurzor(parametre.RiadokKurzora + 1, parametre.StlpecKurzora + 1));
                VykreslovaciAutomat.ShiftTextRightAndInsert(uprava, pocet, sb);

                Kurzor.GoTo(indexRiadok, indexStlpec, parametre, riadky);
                sb.Append(VykreslovaciAutomat.NastavKurzor(parametre.RiadokKurzora + 1, parametre.StlpecKurzora + 1));
            }
            else
            {
                //vyber aktualne slovo
                var _navigovaciPrikaz = new NavigovaciPrikaz();
                var _parametreVyberu = new ParametreVyberu();
                if (parametre.IndexStlpec < riadok.Length() - 1)
                {
                    _navigovaciPrikaz.Vyber = false;
                    _navigovaciPrikaz.Typ = TypNavigacie.SlovoDoprava;

                    Navigator.Naviguj(_navigovaciPrikaz, parametre, riadky, _parametreVyberu);
                }

                _navigovaciPrikaz.Vyber = true;
                _navigovaciPrikaz.Typ = TypNavigacie.SlovoDolava;
                _parametreVyberu = new ParametreVyberu();

                if (parametre.IndexStlpec > 0)
                {
                    Navigator.Naviguj(_navigovaciPrikaz, parametre, riadky, _parametreVyberu);
                }
                if (parametre.IndexStlpec > 0)
                {
                    Navigator.Naviguj(_navigovaciPrikaz, parametre, riadky, _parametreVyberu);
                }
                if (parametre.IndexStlpec > 0)
                {
                    Navigator.Naviguj(_navigovaciPrikaz, parametre, riadky, _parametreVyberu);
                }

                var stlpecCitania = _parametreVyberu.Zaciatok.HasValue ? _parametreVyberu.Zaciatok.Value.Stlpec : parametre.IndexStlpec - 1;
                var pocet = Math.Min(_parametreVyberu.Zaciatok.HasValue ? _parametreVyberu.PocetZnakov : 1, parametre.Sirka - 1);
                var stlpecPisania = _parametreVyberu.Zaciatok.HasValue ? parametre.StlpecKurzora + 1 : parametre.StlpecKurzora;

                if (_parametreVyberu.Zaciatok.HasValue)
                {
                    //vyhni sa nespravnemu zvyrazneniu zatvorky...
                    Kurzor.PosunKurzorDoprava(parametre, riadky);
                }

                var uprava = PrecitajRiadok(parametre,
                                parametreVyberu,
                                riadky,
                                stlpecCitania,
                                pocet);
                sb.Append(VykreslovaciAutomat.NastavKurzor(parametre.RiadokKurzora + 1, stlpecPisania));
                VykreslovaciAutomat.ShiftTextRightAndInsert(uprava, pocetZnakov, sb);

                var stlpec = VykreslovaciAutomat.KonecnyStlpecRiadkuObrazovky(parametre, riadok.Length());

                if (stlpec.HasValue)
                {
                    //ak treba zaplnit prazdne miesto na konci riadku
                    sb.Append(VykreslovaciAutomat.NastavKurzor(parametre.RiadokKurzora + 1, parametre.SirkaKonzoly));
                    VykreslovaciAutomat.ShiftTextRightAndInsert(" ", 2, sb);
                }


                Kurzor.GoTo(indexRiadok, indexStlpec, parametre, riadky);
                sb.Append(VykreslovaciAutomat.NastavKurzor(parametre.RiadokKurzora + 1, parametre.StlpecKurzora + 1));
            }
            Console.Write(sb.ToString());
        }

        private string PrecitajRiadok(ParametreVypisu parametre, 
            ParametreVyberu parametreVyberu, 
            List<GapBuffer> riadky, 
            int stlpecCitania, 
            int pocet)
        {
            var riadok = riadky[0];

            var sb = new StringBuilder();
            PrecitajRiadok(riadok,
                _tokeny,
                parametre,
                parametreVyberu,
                sb,
                Farby.FarbaPrikazRiadku(),
                stlpecCitania,
                pocet);

            return sb.ToString();

        }

        private void PrecitajRiadok(GapBuffer riadok,
            LexResult lexResult,
            ParametreVypisu parametreVypisu, 
            ParametreVyberu vyber, 
            StringBuilder sb, 
            Farby.FarbaPozadia pozadie,
            int zaciatokCitania,
            int pocet)
        {
            VyhladaneSlovo? zvyraznenyText = null;
            if (Zvyraznovac.MaVybranyText(vyber))
            {
                zvyraznenyText = Zvyraznovac.ZvyraznenyText(vyber, 0, riadok.Length());
            }

            var slova = new Dictionary<int, VyhladaneSlovo>();
            VyhladaneSlovo? vyhladaneSlovo = null;

            Dictionary<int, Zatvorka> zatvorky = null;
            Dictionary<int, Token> regexTokens = new Dictionary<int, Token>();

            var pozicia = new Pozicia()
            {
                Stlpec = parametreVypisu.IndexStlpec
            };

            lexResult.RegexTokeny.TryGetValue(0, out regexTokens);
            lexResult.Zatvorky.TryGetValue(0, out zatvorky);

            sb.Append(StylovaciAutomat.SyntaxAndSearchHighligt2(
                    riadok,
                    zaciatokCitania,
                    pocet,
                    slova, vyhladaneSlovo,
                    lexResult.Tokeny[0],
                    zatvorky,
                    pozicia,
                    zvyraznenyText,
                    regexTokens,
                    pozadie));
        }

        internal LexResult GetTokens()
        {
            return _tokeny;
        }
    }
}
