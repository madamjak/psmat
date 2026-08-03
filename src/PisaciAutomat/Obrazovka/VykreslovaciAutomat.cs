using PisaciAutomat.Config;
using PisaciStroj;
using PisaciStroj.Lexer;
using PisaciStroj.Navigacia;
using PisaciStroj.Pamat;
using PisaciStroj.Parametre;
using PisaciStroj.Vyhladavanie;
using PisaciStroj.Vypis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PisaciAutomat.Obrazovka
{
    public class ParametrePrekreslenia
    {
        //optimalizacia prekreslovania
        public bool Necitaj { get; set; }
        public bool LenPrekresli { get; set; }
        public bool Resize { get; set; }
        
        //optimalizacia upravy jedneho riadku
        public int ZaciatocnyStlpec { get; set; }
        public int KonecnySlpec { get; set; }
        public bool UpravaAleboPrekreslenieRiadku { get; set; }
        public bool OptimalizaciaPrekreslenia { get; set; }

        //pre prikazovy riadok nastavene podla poctu riadkov
        public int OkrajVlavo { get; set; }
        public int OkrajHore { get; internal set; }
    }

    public enum TypHlasky
    {
        Info,
        Chyba,
        Dialog
    }

    public struct Hlaska
    {
        public string Sprava { get; set; }

        public TypHlasky Typ { get; set; }
    }

    public class VykreslovaciAutomat
    {
        private static EditorScreen _aktualnaObrazovka;
        
        private ILexer _lexer;
        private IPisaciStroj _editor;
        private IVyhladavac _vyhladavac;

        private StavovyRiadok _stavovyRiadok;

        private LexResult _precitanyText;

        public VykreslovaciAutomat(ILexer lexer, IPisaciStroj editor, IVyhladavac vyhladavac)
        {
            _lexer = lexer;
            _editor = editor;
            _vyhladavac = vyhladavac;
            _stavovyRiadok = new StavovyRiadok();
        }

        public EditorScreen Precitaj(ParametreVypisu parametre, ParametreVyhladavania search, ParametreVyberu parametreVyberu, ParametrePrekreslenia parametrePrekreslenia)
        {
            if (_aktualnaObrazovka != null && parametrePrekreslenia.Necitaj)
            {
                _aktualnaObrazovka.Riadok = parametre.RiadokKurzora + 1;
                _aktualnaObrazovka.Stlpec = parametre.StlpecKurzora + 1;

                return _aktualnaObrazovka;
            }

            if(_precitanyText == null)
            {
                PrecitajText();
            }
            else
            {
                //uprava na riadku ak sa da necitaj
                if (parametrePrekreslenia.UpravaAleboPrekreslenieRiadku)
                {
                    return JednoriadkovaUprava(parametrePrekreslenia, parametre, search, parametreVyberu);
                }
                //ak nebolo nic zmenene necitaj znova
                else if (parametrePrekreslenia.LenPrekresli)
                {
                    //uloz neupravene riadky na znovu-pouzitie pri highlightingu co sa da, ukladaj vsak vzdy len stranku
                    Optimalizacie.UpravTokenyRiadkov(_precitanyText.Tokeny,
                        parametre,
                        _precitanyText,
                        _editor.Riadky(),
                        _lexer);
                }
                //viacriadkove upravy
                else
                {
                    PrecitajText();
                }
            }

            return Precitaj2(parametre, search, _precitanyText, _editor, parametreVyberu, _lexer, _vyhladavac);
        }

        private EditorScreen JednoriadkovaUprava(ParametrePrekreslenia parametrePrekreslenia, 
            ParametreVypisu parametre, 
            ParametreVyhladavania search,
            ParametreVyberu parametreVyberu)
        {
            if (parametrePrekreslenia.LenPrekresli)
            {
                //uloz neupravene riadky na znovu-pouzitie pri highlightingu co sa da, ukladaj vsak vzdy len stranku
                Optimalizacie.UpravTokenyRiadkov(_precitanyText.Tokeny,
                    parametre,
                    _precitanyText,
                    _editor.Riadky(),
                    _lexer);

                if (parametrePrekreslenia.OptimalizaciaPrekreslenia)
                {
                    UpravEditorScreen(parametrePrekreslenia, parametre, search, parametreVyberu);
                    if(PrekresliUpravenyRiadok(parametrePrekreslenia, parametre, search, parametreVyberu, true))
                    {
                        return _aktualnaObrazovka;
                    }
                }

                return Precitaj2(parametre, search, _precitanyText, _editor, parametreVyberu, _lexer, _vyhladavac);
            }
            else if (Optimalizacie.UpravPrecitanyText(parametrePrekreslenia, parametre, _precitanyText, _editor.Riadky(), _lexer))
            {
                //tieto su specialne a pri uprave sa netreba obavat prekreslovania susednych slov
                //predpoklad ze riadok typicky neobsahuje mnoho tokenov
                var bolUprostredRetazcaAleboKomentara = _precitanyText.Tokeny[parametre.IndexRiadok].Values
                                                            .Any(x => (x.Typ == TypTokenu.Retazec || x.Typ == TypTokenu.Komentar)
                                                            && x.Pozicia <= parametre.IndexStlpec 
                                                            && (parametre.IndexStlpec <= x.Pozicia + x.Dlzka
                                                                || (parametre.IndexStlpec > 0 && parametre.IndexStlpec - 1 <= x.Pozicia + x.Dlzka)));

                _precitanyText.Tokeny[parametre.IndexRiadok] = Optimalizacie.PrecitajTokenyRiadku(_precitanyText, _lexer, _editor.Riadky(), parametre.IndexRiadok);

                var jeUprostredRetazcaAleboKomentara = _precitanyText.Tokeny[parametre.IndexRiadok].Values
                                                            .Any(x => (x.Typ == TypTokenu.Retazec || x.Typ == TypTokenu.Komentar)
                                                            && x.Pozicia <= parametre.IndexStlpec && parametre.IndexStlpec <= x.Pozicia + x.Dlzka);

                var prekresliCely = (bolUprostredRetazcaAleboKomentara && !jeUprostredRetazcaAleboKomentara)
                                || (!bolUprostredRetazcaAleboKomentara && jeUprostredRetazcaAleboKomentara);

                if (parametrePrekreslenia.OptimalizaciaPrekreslenia && !prekresliCely)
                {
                    UpravEditorScreen(parametrePrekreslenia, parametre, search, parametreVyberu);
                    if (PrekresliUpravenyRiadok(parametrePrekreslenia, parametre, search, parametreVyberu, jeUprostredRetazcaAleboKomentara))
                    {
                        return _aktualnaObrazovka;
                    }
                }

                return Precitaj2(parametre, search, _precitanyText, _editor, parametreVyberu, _lexer, _vyhladavac);
            }
            else
            {
                PrecitajText();
                return Precitaj2(parametre, search, _precitanyText, _editor, parametreVyberu, _lexer, _vyhladavac);
            }
        }

        private bool PrekresliUpravenyRiadok(ParametrePrekreslenia parametrePrekreslenia, 
            ParametreVypisu parametre, 
            ParametreVyhladavania search, 
            ParametreVyberu parametreVyberu, 
            bool jeUprostredRetazcaAleboKomentara)
        {
            var pocetZnakov = parametrePrekreslenia.KonecnySlpec - parametrePrekreslenia.ZaciatocnyStlpec;
            var mazanie = false;
            if(pocetZnakov < 0)
            {
                mazanie = true;
            }

            var indexRiadok = parametre.IndexRiadok;
            var indexStlpec = parametre.IndexStlpec;
            var offsetStlpec = parametre.OffsetStlpec;

            var sb = new StringBuilder();
            sb.Append(NastavKurzorUnVisible());
            //mazanie
            if (mazanie && !parametrePrekreslenia.LenPrekresli)
            {
                var pocetPotrebnych = Math.Abs(pocetZnakov);
                sb.Append(NastavKurzor(parametre.RiadokKurzora + 1, parametre.StlpecKurzora + 1));
                DeleteCharacterShiftTextLeft(pocetPotrebnych, sb);

                //vyber aktualne slovo v pripade potreby prekreslit slova inou farbou po uprave napr pridanie medzery do retazca publicclass
                var _navigovaciPrikaz = new NavigovaciPrikaz();
                var _parametreVyberu = new ParametreVyberu();
                if (!jeUprostredRetazcaAleboKomentara)
                {
                    VyberSlovaNaPrekreslenie(parametre, _editor.Riadky(), _parametreVyberu, _navigovaciPrikaz);
                }

                //prekresli cely
                if (_parametreVyberu.Zaciatok.HasValue)
                {
                    if (offsetStlpec != parametre.OffsetStlpec)
                    {
                        //vrat sa naspat
                        Kurzor.GoTo(indexRiadok, indexStlpec, parametre, _editor.Riadky());

                        return false;
                    }
                }

                if (_parametreVyberu.Zaciatok.HasValue) 
                {
                    //vrat sa naspat (prevencia napr. proti nespravnemu bracket highlight)
                    Kurzor.GoTo(indexRiadok, indexStlpec, parametre, _editor.Riadky());

                    var uprava = PrecitajRiadok(parametre,
                                    search,
                                    _precitanyText,
                                    parametreVyberu,
                                    _lexer,
                                    _vyhladavac,
                                    _editor.Riadky(),
                                    parametre.IndexRiadok,
                                    _parametreVyberu.Zaciatok.Value.Stlpec,
                                    _parametreVyberu.PocetZnakov);

                    Kurzor.GoTo(indexRiadok, _parametreVyberu.Zaciatok.Value.Stlpec, parametre, _editor.Riadky());
                    sb.Append(NastavKurzor(parametre.RiadokKurzora + 1, parametre.StlpecKurzora + 1));
                    ShiftTextRightAndInsert(uprava, -1, sb); //zda sa ze pouzitie -1 funguje dobre pre nahradu textu
                }

                var stlpec = KonecnyStlpecRiadkuObrazovky(parametre, _editor.Riadky()[parametre.IndexRiadok].Length());

                if (stlpec.HasValue)
                {
                    //ak treba zaplnit prazdne miesto na konci riadku
                    var uprava =
                    PrecitajRiadok(parametre,
                                search,
                                _precitanyText,
                                parametreVyberu,
                                _lexer,
                                _vyhladavac,
                                _editor.Riadky(),
                                parametre.IndexRiadok,
                                stlpec.Value.IndexStlpec,
                                pocetPotrebnych);

                    sb.Append(NastavKurzor(parametre.RiadokKurzora + 1, stlpec.Value.StlpecKurzora + 1));
                    ShiftTextRightAndInsert(uprava, pocetPotrebnych, sb);

                    sb.Append(NastavKurzor(parametre.RiadokKurzora + 1, parametre.SirkaKonzoly));
                    ShiftTextRightAndInsert(" ", 2, sb);
                }

                //vrat sa naspat
                Kurzor.GoTo(indexRiadok, indexStlpec, parametre, _editor.Riadky());
                sb.Append(NastavKurzor(parametre.RiadokKurzora + 1, parametre.StlpecKurzora + 1));
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
                                search,
                                _precitanyText,
                                parametreVyberu,
                                _lexer,
                                _vyhladavac,
                                _editor.Riadky(),
                                parametre.IndexRiadok,
                                stlpecCitania,
                                pocet);

                Kurzor.GoTo(parametre.IndexRiadok, stlpecCitania, parametre, _editor.Riadky());
                sb.Append(NastavKurzor(parametre.RiadokKurzora + 1, parametre.StlpecKurzora + 1));
                DeleteCharacterShiftTextLeft(pocet, sb);
                sb.Append(NastavKurzor(parametre.RiadokKurzora + 1, parametre.StlpecKurzora + 1));
                ShiftTextRightAndInsert(uprava, pocet, sb);

                Kurzor.GoTo(indexRiadok, indexStlpec, parametre, _editor.Riadky());
                sb.Append(NastavKurzor(parametre.RiadokKurzora + 1, parametre.StlpecKurzora + 1));
            }
            
            //zapis, podobne mazaniu
            else
            {
                //pridaj dostatocny pocet miesta
                Kurzor.GoTo(indexRiadok, indexStlpec - pocetZnakov, parametre, _editor.Riadky());
                sb.Append(NastavKurzor(parametre.RiadokKurzora + 1, parametre.StlpecKurzora + 1));
                ShiftTextRightAndInsertSpace(pocetZnakov, sb);

                //vyber aktualne slovo v pripade potreby prekreslit slova inou farbou po uprave napr pridanie medzery do retazca publicclass
                var _navigovaciPrikaz = new NavigovaciPrikaz();
                var _parametreVyberu = new ParametreVyberu();
                if (!jeUprostredRetazcaAleboKomentara)
                {
                    //vrat sa naspat
                    Kurzor.GoTo(indexRiadok, indexStlpec, parametre, _editor.Riadky());

                    VyberSlovaNaPrekreslenie(parametre, _editor.Riadky(), _parametreVyberu, _navigovaciPrikaz);
                }

                //prekresli cely
                if (_parametreVyberu.Zaciatok.HasValue)
                {
                    if(offsetStlpec != parametre.OffsetStlpec)
                    {
                        //vrat sa naspat
                        Kurzor.GoTo(indexRiadok, indexStlpec, parametre, _editor.Riadky());

                        return false;
                    }
                }
                                                
                //prekresli upraveny znak / slova
                var stlpecCitania = _parametreVyberu.Zaciatok.HasValue ? _parametreVyberu.Zaciatok.Value.Stlpec : parametre.IndexStlpec;
                var pocet = Math.Min(_parametreVyberu.Zaciatok.HasValue ? _parametreVyberu.PocetZnakov : pocetZnakov, parametre.Sirka - 1);
                var stlpecPisania = parametre.StlpecKurzora + 1;

                //vrat sa naspat (prevencia napr. proti nespravnemu bracket highlight)
                Kurzor.GoTo(indexRiadok, indexStlpec, parametre, _editor.Riadky());

                var uprava = PrecitajRiadok(parametre,
                                search,
                                _precitanyText,
                                parametreVyberu,
                                _lexer,
                                _vyhladavac,
                                _editor.Riadky(),
                                parametre.IndexRiadok,
                                stlpecCitania,
                                pocet);

                //zapis
                sb.Append(NastavKurzor(parametre.RiadokKurzora + 1, stlpecPisania));
                ShiftTextRightAndInsert(uprava, -1, sb); //-1 nahradi text

                //dana funkcia vrati stlpec v pripade ze je potrebne skryt znaky na konci riadku
                var stlpec = KonecnyStlpecRiadkuObrazovky(parametre, _editor.Riadky()[parametre.IndexRiadok].Length());

                if (stlpec.HasValue)
                {
                    sb.Append(NastavKurzor(parametre.RiadokKurzora + 1, parametre.SirkaKonzoly));
                    ShiftTextRightAndInsert(" ", 2, sb);
                }

                Kurzor.GoTo(indexRiadok, indexStlpec, parametre, _editor.Riadky());
                sb.Append(NastavKurzor(parametre.RiadokKurzora + 1, parametre.StlpecKurzora + 1));
            }
            Console.Write(sb.ToString());
            return true;
        }

        public static void VyberSlovaNaPrekreslenie(ParametreVypisu parametre,
            List<GapBuffer> riadky,
            ParametreVyberu _parametreVyberu,
            NavigovaciPrikaz _navigovaciPrikaz)
        {
            _navigovaciPrikaz.Vyber = false;
            _navigovaciPrikaz.Typ = TypNavigacie.SlovoDoprava;

            if(parametre.IndexStlpec < riadky[parametre.IndexRiadok].Length())
            {
                Navigator.Naviguj(_navigovaciPrikaz, parametre, riadky, _parametreVyberu);
            }

            while (true)
            {
                if (parametre.IndexStlpec == riadky[parametre.IndexRiadok].Length()
                    || riadky[parametre.IndexRiadok].CharAt(parametre.IndexStlpec) == ' ')
                {
                    break;
                }

                Navigator.Naviguj(_navigovaciPrikaz, parametre, riadky, _parametreVyberu);
            }

            //naspat
            _navigovaciPrikaz.Vyber = true;
            _navigovaciPrikaz.Typ = TypNavigacie.SlovoDolava;

            if (parametre.IndexStlpec > 0)
            {
                Navigator.Naviguj(_navigovaciPrikaz, parametre, riadky, _parametreVyberu);
            }
            while (true)
            {
                if (parametre.IndexStlpec == 0 || riadky[parametre.IndexRiadok].CharAt(parametre.IndexStlpec) == ' ')
                {
                    break;
                }

                Navigator.Naviguj(_navigovaciPrikaz, parametre, riadky, _parametreVyberu);
            }

            //slovo nalavo od medzery
            if (parametre.IndexStlpec > 0)
            {
                Navigator.Naviguj(_navigovaciPrikaz, parametre, riadky, _parametreVyberu);
            }
            while (true)
            {
                if (parametre.IndexStlpec == 0 || riadky[parametre.IndexRiadok].CharAt(parametre.IndexStlpec) == ' ')
                {
                    break;
                }

                Navigator.Naviguj(_navigovaciPrikaz, parametre, riadky, _parametreVyberu);
            }
        }

        public struct PoziciaNaRiadku
        {
            public int IndexStlpec { get; set; }
            public int StlpecKurzora { get; set; }
        }

        public static PoziciaNaRiadku? KonecnyStlpecRiadkuObrazovky(ParametreVypisu parametre, int dlzkaRiadku)
        {
            var stlpec = parametre.Stlpec;
            var indexStlpec = parametre.Stlpec + parametre.OffsetStlpec;

            while (true)
            {
                if(indexStlpec == dlzkaRiadku)
                {
                    return null;
                }

                if (stlpec == parametre.Sirka - 2)
                {
                    return new PoziciaNaRiadku()
                    {
                        IndexStlpec = indexStlpec,
                        StlpecKurzora = stlpec + parametre.OkrajVlavo
                    };
                }

                stlpec++;
                indexStlpec++;
            }
        }

        private void UpravEditorScreen(ParametrePrekreslenia parametrePrekreslenia, ParametreVypisu parametre, ParametreVyhladavania search, ParametreVyberu parametreVyberu)
        {
            var okraj = _editor.Riadky().Count.ToString().Length + 2;
            if (okraj == 3)
            {
                okraj = 5;
            }
            var formatCislaRiadkov = "D" + (okraj - 2);
            var novyRiadok = UpravRiadokObrazovky(parametre,
                search,
                _precitanyText,
                parametreVyberu,
                _lexer,
                _vyhladavac,
                _editor.Riadky(),
                formatCislaRiadkov,
                parametre.IndexRiadok);

            _aktualnaObrazovka.Riadky[parametre.Riadok] = novyRiadok;
            _aktualnaObrazovka.Riadok = parametre.RiadokKurzora + 1;
            _aktualnaObrazovka.Stlpec = parametre.StlpecKurzora + 1;
        }

        private void PrecitajText()
        {
            _precitanyText = _lexer.ZatvorkyAKomentare(_editor.Riadky());
        }

        public static EditorScreen Precitaj2(ParametreVypisu parametre,
            ParametreVyhladavania search,
            LexResult lexResult,
            IPisaciStroj editor,
            ParametreVyberu parametreVyberu,
            ILexer lexer,
            IVyhladavac vyhladavac)
        {
            var result = new EditorScreen(parametre.Sirka, parametre.Vyska)
            {
                Riadok = parametre.RiadokKurzora + 1,
                Stlpec = parametre.StlpecKurzora + 1,
            };

            var pocetRiadkov = 0;
            var riadokObrazovky = 0;
            var riadky = editor.Riadky();
            var formatCislaRiadkov = "D" + (parametre.OkrajVlavo - 2);
            for (int i = parametre.OffsetRiadok; i < riadky.Count; i++)
            {
                if (pocetRiadkov == parametre.Vyska)
                {
                    break;
                }

                result.Riadky[riadokObrazovky] = UpravRiadokObrazovky(parametre, 
                    search, lexResult, parametreVyberu, lexer, vyhladavac, riadky, formatCislaRiadkov, i);

                pocetRiadkov++;
                riadokObrazovky++;
            }

            return result;
        }

        private static string UpravRiadokObrazovky(ParametreVypisu parametre, 
            ParametreVyhladavania search, 
            LexResult lexResult, 
            ParametreVyberu parametreVyberu, 
            ILexer lexer, 
            IVyhladavac vyhladavac, 
            List<GapBuffer> riadky, 
            string formatCislaRiadkov, 
            int i)
        {
            return string.Format("{0}{1}", Farby.StylCislaRiadkov((i + 1).ToString(formatCislaRiadkov)), PrecitajRiadok(
                                parametre,
                                search,
                                lexResult,
                                parametreVyberu,
                                lexer,
                                vyhladavac,
                                riadky,
                                i,
                                parametre.OffsetStlpec,
                                parametre.Sirka - 1));
        }

        private static string PrecitajRiadok(
            ParametreVypisu parametre, 
            ParametreVyhladavania search, 
            LexResult lexResult, 
            ParametreVyberu parametreVyberu, 
            ILexer lexer, 
            IVyhladavac vyhladavac, 
            List<GapBuffer> riadky, 
            int indexRiadku,
            int offsetStlpec,
            int sirka)
        {
            Dictionary<int, VyhladaneSlovo> vyhladaneSlova = new Dictionary<int, VyhladaneSlovo>();
            VyhladaneSlovo? vSlovo = null;
            Dictionary<int, Token> tokeny = null;
            Dictionary<int, Zatvorka> zatvorky = null;
            VyhladaneSlovo? zvyraznenyText = null;
            Dictionary<int, Token> regexTokens = new Dictionary<int, Token>();

            if (search.VyhladaneSlova != null)
            {
                if (!search.VyhladaneSlova.TryGetValue(indexRiadku, out vyhladaneSlova))
                {
                    vyhladaneSlova = new Dictionary<int, VyhladaneSlovo>();
                }
            }
            else if (search.VyhladavanyText != null)
            {
                vyhladaneSlova = vyhladavac.VyhladajVsetky(riadky[indexRiadku], search.VyhladavanyText, search.Obratene);
            }

            if (search.VyhladaneSlovo.HasValue && search.VyhladaneSlovo.Value.Riadok == indexRiadku)
            {
                vSlovo = search.VyhladaneSlovo;
            }


            if (lexResult.Tokeny == null || !lexResult.Tokeny.TryGetValue(indexRiadku, out tokeny))
            {
                tokeny = Optimalizacie.PrecitajTokenyRiadku(lexResult, lexer, riadky, indexRiadku);
            }

            if (lexResult.Zatvorky == null || !lexResult.Zatvorky.TryGetValue(indexRiadku, out zatvorky))
            {
                zatvorky = new Dictionary<int, Zatvorka>();
            }

            var poziciaKurzora = new Pozicia()
            {
                Riadok = parametre.IndexRiadok,
                Stlpec = parametre.IndexStlpec
            };


            if (Zvyraznovac.MaVybranyText(parametreVyberu))
            {
                zvyraznenyText = Zvyraznovac.ZvyraznenyText(parametreVyberu, indexRiadku, riadky[indexRiadku].Length());
            }

            return StylovaciAutomat.SyntaxAndSearchHighligt2(riadky[indexRiadku],
            offsetStlpec, sirka,
            vyhladaneSlova, vSlovo, tokeny, zatvorky, poziciaKurzora,
            zvyraznenyText, regexTokens);
        }

        public void VykresliNaKonzolu(EditorScreen novaObrazovka,
            StavovyRiadokInfo stavovyRiadok,
            ParametreVypisu parametre,
            Hlaska? hlaska,
            string dialog,
            bool _cmdMode, 
            ParametrePrekreslenia p, 
            StringBuilder sb)
        {
            sb.Append(Farby.AnsiReset());

            if (!_cmdMode)
            {
                sb.Append(NastavKurzor(1, 1));
                sb.Append(ZmazOdKurzoraPoKoniecRiadku());
                sb.Append(NastavKurzor(2, 1));
                sb.Append(ZmazOdKurzoraPoKoniecRiadku());
                if (hlaska.HasValue)
                {
                    VykresliInfoHlasku(hlaska.Value, sb);
                }
                else if (dialog != null)
                {
                    VykresliDialog(parametre, dialog, sb);
                }
            }

            if (_aktualnaObrazovka == null || p.Resize)
            {
                Vykresli(novaObrazovka, sb, stavovyRiadok, parametre);
                _aktualnaObrazovka = novaObrazovka;
            }
            else
            {
                Prekresli(novaObrazovka, sb, stavovyRiadok, parametre, p);
                _aktualnaObrazovka = novaObrazovka;
            }

            _stavovyRiadok.Vykresli(p.Resize, stavovyRiadok, parametre, sb);
        }

        private static void VykresliDialog(ParametreVypisu parametre, string hlaska, StringBuilder sb)
        {
            sb.Append(VykresliDialog(hlaska, 1));
            sb.Append(Farby.AnsiReset());
            sb.Append(NastavKurzor(2, 1));
            sb.Append(ZmazOdKurzoraPoKoniecRiadku());
        }

        public static void ZmazHlasku(StringBuilder sb)
        {
            sb.Append(Farby.AnsiReset());
            sb.Append(NastavKurzor(2, 1));
            sb.Append(ZmazOdKurzoraPoKoniecRiadku());
        }

        public static void VykresliInfoHlasku(Hlaska hlaska, StringBuilder sb)
        {
            sb.Append(NastavKurzor(2, 1));
            sb.Append(ZmazOdKurzoraPoKoniecRiadku());

            if(hlaska.Typ == TypHlasky.Info)
            {
                sb.Append(Farby.Info(hlaska.Sprava));
            }

            if(hlaska.Typ == TypHlasky.Chyba)
            {
                sb.Append(Farby.Chyba(hlaska.Sprava));
            }

            if (hlaska.Typ == TypHlasky.Dialog)
            {
                sb.Append(Farby.Dialog(hlaska.Sprava));
            }

            //sb.Append(HlaskaDialogu(hlaska));
            //sb.Append(Info(hlaska));
        }

        private void Prekresli(EditorScreen novaObrazovka, StringBuilder sb, StavovyRiadokInfo stavovyRiadok, ParametreVypisu parametre, ParametrePrekreslenia p)
        {
            if (!p.Necitaj)
            {
                for (int i = 0; i < novaObrazovka.Riadky.Count; i++)
                {
                    if (novaObrazovka.Riadky.Count != _aktualnaObrazovka.Riadky.Count)
                    {
                        PrekresliRiadok(novaObrazovka, sb, parametre, i);
                        continue;
                    }
                    else
                    {
                        if (novaObrazovka.Riadky[i] != _aktualnaObrazovka.Riadky[i])
                        {
                            PrekresliRiadok(novaObrazovka, sb, parametre, i);
                        }
                    }
                }
            }
        }

        private static void PrekresliRiadok(EditorScreen novaObrazovka, StringBuilder sb, ParametreVypisu parametre, int i)
        {
            sb.Append(NastavKurzor(i + parametre.OkrajHore + 1, 1));
            sb.Append(ZmazOdKurzoraPoKoniecRiadku());
            sb.Append(novaObrazovka.Riadky[i]);
        }

        /// <summary>
        /// https://learn.microsoft.com/en-us/windows/console/console-virtual-terminal-sequences
        /// </summary>
        public static void ShiftTextRightAndInsert(string novyAnsiKod, int pocetZnakov, StringBuilder sb)
        {
            ShiftTextRightAndInsertSpace(pocetZnakov, sb);
            sb.Append(novyAnsiKod);
        }

        public static void ShiftTextRightAndInsertSpace(int pocetZnakov, StringBuilder sb)
        {
            sb.Append(string.Format("\u001b[{0}@", pocetZnakov));
        }

        public static void DeleteCharacterShiftTextLeft(int pocetZnakov, StringBuilder sb)
        {
            sb.Append(string.Format("\u001b[{0}P", pocetZnakov));
        }

        public static void Vykresli(EditorScreen novaObrazovka, StringBuilder sb, StavovyRiadokInfo stavovyRiadok, ParametreVypisu parametre)
        {
            sb.Append(NastavKurzor(parametre.OkrajHore + 1, 1));
            foreach (var riadok in novaObrazovka.Riadky)
            {
                sb.AppendLine(riadok);
            }
        }

        public static string NastavKurzor(int riadok, int stlpec)
        {
            return string.Format("\u001b[{0};{1}H", riadok, stlpec);
        }

        public static string NastavKurzorVisible()
        {
            return string.Format("\u001b[?25h");
        }

        public static string NastavKurzorUnVisible()
        {
            return string.Format("\u001b[?25l");
        }

        public static string ZmazOdKurzoraPoKoniecRiadku()
        {
            return string.Format("\u001b[0K");
        }

        public static string ZmazOdZaciatkuRiadkuPoKurzor()
        {

            return string.Format("\u001b[1K");
        }

        public static string Chyba2()
        {
            return string.Format("\u001b[41;1m{0}\u001b[0m", " ! ");
        }

        public static string HlaskaDialogu(string v)
        {
            //return string.Format("\u001b[42;1m {0} \u001b[0m", v);
            return Farby.Dialog(v);
        }


        public static string VykresliDialog(string hlaska, int okraj)
        {
            var sb = new StringBuilder();
            sb.Append(NastavKurzor(1, 1));
            sb.Append(ZmazOdKurzoraPoKoniecRiadku());
            sb.Append(NastavKurzor(1, 1));
            sb.Append(HlaskaDialogu(hlaska));

            return sb.ToString();
        }

        public static string VykresliChybu2(string sprava)
        {
            var sb = new StringBuilder();
            sb.Append(VykreslovaciAutomat.NastavKurzor(1, 1));
            sb.Append(VykreslovaciAutomat.Chyba2());
            sb.Append(" ");
            sb.Append(sprava);           

            return sb.ToString();
        }

        public static string EraseScreen()
        {
            //return "\u001b[2J";
            //force Windows Terminal to not preserve screen in scrollback buffer
            //https://github.com/microsoft/terminal/issues/18835
            return "\u001b[H" + "\u001b[0J";
        }

        public static string NastavPozadie(int sirka)
        {
            var i = 0;
            var sb = new StringBuilder();
            while (true)
            {
                if (i == sirka)
                {
                    break;
                }
                sb.Append(" ");
                i++;
            }
            return sb.ToString();
        }

        public static string NastavPozadie(int sirka, Farby.FarbaPozadia farba)
        {
            var i = 0;
            var sb = new StringBuilder();
            while (true)
            {
                if (i == sirka)
                {
                    break;
                }
                sb.Append(string.Format("{0} {1}", Farby.AnsiStyl(farba), Farby.AnsiReset()));
                i++;
            }
            return sb.ToString();
        }
    }
}
