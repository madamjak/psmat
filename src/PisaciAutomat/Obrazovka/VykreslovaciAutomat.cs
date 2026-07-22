using PisaciAutomat.Config;
using PisaciStroj;
using PisaciStroj.Lexer;
using PisaciStroj.Navigacia;
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
        public bool Resize { get; set; }

        public bool Necitaj { get; set; }
        
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
        private EditorScreen _aktualnaObrazovka;
        
        private ILexer _lexer;
        private IPisaciStroj _editor;
        private IVyhladavac _vyhladavac;

        private StavovyRiadok _stavovyRiadok;

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
                //TODO v pripade navigace na zatvorku sa zatvorka nezvyrazni
                _aktualnaObrazovka.Riadok = parametre.RiadokKurzora + 1;
                _aktualnaObrazovka.Stlpec = parametre.StlpecKurzora + 1;

                return _aktualnaObrazovka;
            }

            var lexResult = _lexer.ZatvorkyAKomentare(_editor.Riadky());

            return Precitaj2(parametre, search, lexResult, _editor, parametreVyberu, _lexer, _vyhladavac);
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

                Dictionary<int, VyhladaneSlovo> vyhladaneSlova = new Dictionary<int, VyhladaneSlovo>();
                VyhladaneSlovo? vSlovo = null;
                Dictionary<int, Token> tokeny = null;
                Dictionary<int, Zatvorka> zatvorky = null;
                VyhladaneSlovo? zvyraznenyText = null;
                Dictionary<int, Token> regexTokens = new Dictionary<int, Token>();

                if(search.VyhladaneSlova != null)
                {
                    if(!search.VyhladaneSlova.TryGetValue(i, out vyhladaneSlova))
                    {
                        vyhladaneSlova = new Dictionary<int, VyhladaneSlovo>();
                    }
                }
                else if (search.VyhladavanyText != null)
                {
                    vyhladaneSlova = vyhladavac.VyhladajVsetky(riadky[i], search.VyhladavanyText, search.Obratene);
                }

                if (search.VyhladaneSlovo.HasValue && search.VyhladaneSlovo.Value.Riadok == i)
                {
                    vSlovo = search.VyhladaneSlovo;
                }

                if (lexResult.Tokeny == null || !lexResult.Tokeny.TryGetValue(i, out tokeny))
                {
                    tokeny = lexer.LexPreEditor(riadky[i]);
                }
                else
                {
                    var t = lexer.LexPreEditor(riadky[i]);
                    if (t.Count > 0)
                    {
                        foreach (var to in t)
                        {
                            var zvyrazniToken = true;
                            foreach (var koment in tokeny)
                            {
                                if (koment.Key <= to.Key && to.Key <= koment.Key + koment.Value.Dlzka)
                                {
                                    zvyrazniToken = false;
                                }
                            }

                            if (zvyrazniToken)
                            {
                                tokeny.Add(to.Key, to.Value);
                            }
                        }
                    }
                }

                if (lexResult.Zatvorky == null || !lexResult.Zatvorky.TryGetValue(i, out zatvorky))
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
                    zvyraznenyText = Zvyraznovac.ZvyraznenyText(parametreVyberu, i, riadky[i].Length());
                }

                result.Riadky[riadokObrazovky] = string.Format("{0}{1}", Farby.StylCislaRiadkov((i).ToString(formatCislaRiadkov)),
                    StylovaciAutomat.SyntaxAndSearchHighligt2(riadky[i],
                    parametre.OffsetStlpec, parametre.Sirka - 1,
                    vyhladaneSlova, vSlovo, tokeny, zatvorky, poziciaKurzora,
                    zvyraznenyText, regexTokens));

                pocetRiadkov++;
                riadokObrazovky++;
            }

            return result;
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
            }

            if (hlaska.HasValue)
            {
                VykresliInfoHlasku(parametre, hlaska.Value, sb);
            }else if (dialog != null)
            {
                VykresliDialog(parametre, dialog, sb);
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
            sb.Append(NastavKurzor(2, 1));
            sb.Append(ZmazOdKurzoraPoKoniecRiadku());
        }

        public static void VykresliInfoHlasku(ParametreVypisu parametre, Hlaska hlaska, StringBuilder sb)
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
    }
}
