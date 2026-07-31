using PisaciAutomat.Config;
using PisaciAutomat.Obrazovka;
using PisaciStroj.Lexer;
using PisaciStroj.Navigacia;
using PisaciStroj.Pamat;
using PisaciStroj.Parametre;
using PisaciStroj.Vyhladavanie;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PisaciAutomat.Prikazy.Vykreslovanie
{
    public class VykreslovacCmd
    {
        private ILexer _lexer;
        
        private string _aktualnyRiadok;

        private LexResult _tokeny;

        public VykreslovacCmd(ILexer lexer)
        {
            _lexer = lexer;
        }

        public string PrecitajPrikazovyRiadok(ParametrePrekreslenia p2, //cmd parametre
            GapBuffer riadok,
            ParametreVypisu parametreVypisu,
            ParametreVyberu vyber,
            List<GapBuffer> riadky,
            bool chyba,
            bool chybaReset)
        {
            if(p2 != null && p2.Necitaj && !p2.Resize)
            {
                return string.Empty;
            }

            if (p2 != null && p2.OptimalizaciaPrekreslenia && !chybaReset && !p2.Resize)
            {

                var bolUprostredRetazcaAleboKomentara = false;
                if (_tokeny != null && _tokeny.Tokeny.Count > 0)
                {
                    bolUprostredRetazcaAleboKomentara = _tokeny.Tokeny[parametreVypisu.IndexRiadok].Values
                                                            .Any(x => (x.Typ == TypTokenu.Retazec)
                                                            && x.Pozicia <= parametreVypisu.IndexStlpec
                                                            && (parametreVypisu.IndexStlpec <= x.Pozicia + x.Dlzka
                                                                || (parametreVypisu.IndexStlpec > 0 && parametreVypisu.IndexStlpec - 1 <= x.Pozicia + x.Dlzka)));
                }

                _tokeny = _lexer.LexPrePrikazovyRiadok(riadky);

                var jeUprostredRetazcaAleboKomentara = false;
                if(_tokeny.Tokeny.Count > 0)
                {
                    jeUprostredRetazcaAleboKomentara = _tokeny.Tokeny[parametreVypisu.IndexRiadok].Values
                                                            .Any(x => (x.Typ == TypTokenu.Retazec)
                                                            && x.Pozicia <= parametreVypisu.IndexStlpec && parametreVypisu.IndexStlpec <= x.Pozicia + x.Dlzka);
                }

                var prekresliCely = (bolUprostredRetazcaAleboKomentara && !jeUprostredRetazcaAleboKomentara)
                                || (!bolUprostredRetazcaAleboKomentara && jeUprostredRetazcaAleboKomentara);

                if (!prekresliCely)
                {
                    PrekresliUpravenyRiadok(p2, parametreVypisu, vyber, riadok, riadky, jeUprostredRetazcaAleboKomentara);

                    return string.Empty;
                }
            }

            var sb = new StringBuilder();
            var pozadie = Farby.FarbaPrikazRiadku();
            if (chyba)
            {
                pozadie = Farby.FarbaPozadia.CervenaLight;
            }

            sb.Append(VykreslovaciAutomat.NastavKurzor(1, 1));
            sb.Append(VykreslovaciAutomat.ZmazOdKurzoraPoKoniecRiadku());

            sb.Append(VykreslovaciAutomat.NastavPozadie(p2.OkrajVlavo - 2));
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

        //copypaste z VykreslovaciAutomat
        private void PrekresliUpravenyRiadok(ParametrePrekreslenia parametrePrekreslenia, 
            ParametreVypisu parametre, 
            ParametreVyberu parametreVyberu,
            GapBuffer riadok,
            List<GapBuffer> riadky,
            bool jeUprostredRetazca)
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

            //mazanie
            if (mazanie && !parametrePrekreslenia.LenPrekresli)
            {
                var pocetPotrebnych = Math.Abs(pocetZnakov);
                sb.Append(VykreslovaciAutomat.NastavKurzor(parametre.RiadokKurzora + 1, parametre.StlpecKurzora + 1));
                VykreslovaciAutomat.DeleteCharacterShiftTextLeft(pocetPotrebnych, sb);

                //vyber aktualne slovo v pripade potreby prekreslit slova inou farbou po uprave napr pridanie medzery do retazca publicclass
                var _navigovaciPrikaz = new NavigovaciPrikaz();
                var _parametreVyberu = new ParametreVyberu();
                if (!jeUprostredRetazca)
                {
                    VykreslovaciAutomat.VyberSlovaNaPrekreslenie(parametre, riadky, _parametreVyberu, _navigovaciPrikaz);
                }

                if (_parametreVyberu.Zaciatok.HasValue)
                {
                    //vrat sa naspat (prevencia napr. proti nespravnemu bracket highlight)
                    Kurzor.GoTo(indexRiadok, indexStlpec, parametre, riadky);

                    var uprava = PrecitajRiadok(parametre,
                                parametreVyberu,
                                riadky,
                                _parametreVyberu.Zaciatok.Value.Stlpec,
                                _parametreVyberu.PocetZnakov);

                    Kurzor.GoTo(indexRiadok, _parametreVyberu.Zaciatok.Value.Stlpec, parametre, riadky);
                    sb.Append(VykreslovaciAutomat.NastavKurzor(parametre.RiadokKurzora + 1, parametre.StlpecKurzora + 1));
                    VykreslovaciAutomat.ShiftTextRightAndInsert(uprava, -1, sb); //zda sa ze pouzitie -1 funguje dobre pre nahradu textu
                }

                var stlpec = VykreslovaciAutomat.KonecnyStlpecRiadkuObrazovky(parametre, riadky[parametre.IndexRiadok].Length());

                if (stlpec.HasValue)
                {
                    //ak treba zaplnit prazdne miesto na konci riadku\
                    var uprava = PrecitajRiadok(parametre,
                                parametreVyberu,
                                riadky,
                                stlpec.Value.IndexStlpec,
                                pocetPotrebnych);

                    sb.Append(VykreslovaciAutomat.NastavKurzor(parametre.RiadokKurzora + 1, stlpec.Value.StlpecKurzora + 1));
                    VykreslovaciAutomat.ShiftTextRightAndInsert(uprava, pocetPotrebnych, sb);
                }
                else
                {
                    sb.Append(VykreslovaciAutomat.NastavKurzor(parametre.RiadokKurzora + 1, parametre.SirkaKonzoly - pocetPotrebnych));
                    VykreslovaciAutomat.ShiftTextRightAndInsert(VykreslovaciAutomat.NastavPozadie(pocetPotrebnych, Farby.FarbaPrikazRiadku()), pocetPotrebnych, sb);
                }

                //vrat sa naspat
                Kurzor.GoTo(indexRiadok, indexStlpec, parametre, riadky);
                sb.Append(VykreslovaciAutomat.NastavKurzor(parametre.RiadokKurzora + 1, parametre.StlpecKurzora + 1));
            }


            //vyber textu
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

            //zapis, podobne mazaniu
            else
            {
                //pridaj dostatocny pocet miesta
                Kurzor.GoTo(indexRiadok, indexStlpec - pocetZnakov, parametre, riadky);
                sb.Append(VykreslovaciAutomat.NastavKurzor(parametre.RiadokKurzora + 1, parametre.StlpecKurzora + 1));
                VykreslovaciAutomat.ShiftTextRightAndInsertSpace(pocetZnakov, sb);

                //vyber aktualne slovo v pripade potreby prekreslit slova inou farbou po uprave napr pridanie medzery do retazca publicclass
                var _navigovaciPrikaz = new NavigovaciPrikaz();
                var _parametreVyberu = new ParametreVyberu();
                if (!jeUprostredRetazca)
                {
                    //vrat sa naspat
                    Kurzor.GoTo(indexRiadok, indexStlpec, parametre, riadky);

                    VykreslovaciAutomat.VyberSlovaNaPrekreslenie(parametre, riadky, _parametreVyberu, _navigovaciPrikaz);
                }

                //prekresli upraveny znak / slova
                var stlpecCitania = _parametreVyberu.Zaciatok.HasValue ? _parametreVyberu.Zaciatok.Value.Stlpec : parametre.IndexStlpec;
                var pocet = Math.Min(_parametreVyberu.Zaciatok.HasValue ? _parametreVyberu.PocetZnakov : pocetZnakov, parametre.Sirka - 1);
                var stlpecPisania = parametre.StlpecKurzora + 1;

                //vrat sa naspat (prevencia napr. proti nespravnemu bracket highlight)
                Kurzor.GoTo(indexRiadok, indexStlpec, parametre, riadky);

                var uprava = PrecitajRiadok(parametre,
                                parametreVyberu,
                                riadky,
                                stlpecCitania,
                                pocet);

                //zapis
                sb.Append(VykreslovaciAutomat.NastavKurzor(parametre.RiadokKurzora + 1, stlpecPisania));
                VykreslovaciAutomat.ShiftTextRightAndInsert(uprava, -1, sb); //-1 nahradi text

                //dana funkcia vrati stlpec v pripade ze je potrebne skryt znaky na konci riadku
                var stlpec = VykreslovaciAutomat.KonecnyStlpecRiadkuObrazovky(parametre, riadky[parametre.IndexRiadok].Length());

                if (stlpec.HasValue)
                {
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
