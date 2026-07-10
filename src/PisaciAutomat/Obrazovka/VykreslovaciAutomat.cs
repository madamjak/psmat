using PisaciStroj.Lexer;
using PisaciStroj.Navigacia;
using PisaciStroj.Pamat;
using PisaciStroj.Parametre;
using PisaciStroj.Vyhladavanie;
using PisaciStroj.Vypis;
using System;
using System.Collections.Generic;
using System.Text;

namespace PisaciAutomat.Obrazovka
{
    public class VykreslovaciAutomat
    {
        private EditorScreen _aktualnaObrazovka;

        private ILexer _lexer;
        private PisaciStroj.Program _editor;
        private IVyhladavac _vyhladavac;

        public VykreslovaciAutomat(LexGramatika gramatika, PisaciStroj.Program editor, IVyhladavac vyhladavac)
        {
            _lexer = new LexAutomat(gramatika);
            _editor = editor;
            _vyhladavac = vyhladavac;
        }

        public EditorScreen Precitaj(ParametreVypisu parametre, ParametreVyhladavania search, ParametreVyberu parametreVyberu, ParametreZapisu parametreZapisu)
        {
            var lexResult = _lexer.Lex(_editor.Riadky());

            return Precitaj2(parametre, search, lexResult, _editor.Riadky(), parametreVyberu, parametreZapisu);
        }

        public static EditorScreen Precitaj2(ParametreVypisu parametre, ParametreVyhladavania search, LexResult lexResult, List<GapBuffer> riadky, ParametreVyberu parametreVyberu, ParametreZapisu parametreZapisu)
        {
            var result = new EditorScreen(parametre.Sirka, parametre.Vyska)
            {
                Riadok = parametre.RiadokKurzora + 1,
                Stlpec = parametre.StlpecKurzora + 1
            };

            var pocetRiadkov = 0;
            var riadokObrazovky = 0;
            for (int i = parametre.OffsetRiadok; i < riadky.Count; i++)
            {
                if (pocetRiadkov == parametre.Vyska)
                {
                    break;
                }

                Dictionary<int, VyhladaneSlovo> vyhladaneSlova = null;
                VyhladaneSlovo? vSlovo = null;
                Dictionary<int, Token> tokeny = null;
                Dictionary<int, Zatvorka> zatvorky = null;
                VyhladaneSlovo? zvyraznenyText = null;

                if (search.VyhladaneSlova == null || !search.VyhladaneSlova.TryGetValue(i, out vyhladaneSlova)) 
                {
                    vyhladaneSlova = new Dictionary<int, VyhladaneSlovo>();
                }
                if (search.VyhladaneSlovo.HasValue && search.VyhladaneSlovo.Value.Riadok == i)
                {
                    vSlovo = search.VyhladaneSlovo;
                }

                if(lexResult.Tokeny == null || !lexResult.Tokeny.TryGetValue(i, out tokeny))
                {
                    tokeny = new Dictionary<int, Token>();
                }
                
                if(lexResult.Zatvorky == null || !lexResult.Zatvorky.TryGetValue(i, out zatvorky))
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

                result.Riadky[riadokObrazovky] = string.Format("{0}  {1}", CislaRiadkov((i).ToString("D3")),
                    StylovaciAutomat.SyntaxAndSearchHighligt2(riadky[i], 
                    parametre.OffsetStlpec, parametre.Sirka, 
                    vyhladaneSlova, vSlovo, tokeny, zatvorky, poziciaKurzora,
                    zvyraznenyText));

                if(parametre.IndexRiadok == i)
                {
                    Indentation.NastavOkraj(parametreZapisu, riadky[i]);
                }

                pocetRiadkov++;
                riadokObrazovky++;
            }

            return result;
        }

        public void VykresliNaKonzolu(EditorScreen novaObrazovka, string stavovyRiadok, ParametreVypisu parametre, string hlaska, bool _cmdMode)
        {
            var sb = new StringBuilder();

            if (!_cmdMode)
            {
                sb.Append(NastavKurzor(1, 1));
                sb.Append(ZmazOdKurzoraPoKoniecRiadku());
                sb.Append(NastavKurzor(2, 1));
                sb.Append(ZmazOdKurzoraPoKoniecRiadku());
            }

            if (hlaska != null)
            {
                VykresliHlasku(parametre, hlaska, sb);
            }

            if (_aktualnaObrazovka == null)
            {
                Vykresli(novaObrazovka, sb, stavovyRiadok, parametre);
                _aktualnaObrazovka = novaObrazovka;
            }
            else
            {
                Prekresli(novaObrazovka, sb, stavovyRiadok, parametre);
                _aktualnaObrazovka = novaObrazovka;
            }

            Console.Write(sb.ToString());
        }

        private static void VykresliHlasku(ParametreVypisu parametre, string hlaska, StringBuilder sb)
        {
            sb.Append(VykresliHlasku(hlaska, parametre.OkrajVlavo));
            sb.Append(NastavKurzor(2, 1));
            sb.Append(ZmazOdKurzoraPoKoniecRiadku());
        }

        private void Prekresli(EditorScreen novaObrazovka, StringBuilder sb, string stavovyRiadok, ParametreVypisu parametre)
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

            sb.Append(NastavKurzor(parametre.VyskaKonzoly, parametre.OkrajVlavo + 1));
            sb.Append(ZmazOdKurzoraPoKoniecRiadku());
            sb.Append(StavovyRiadok(stavovyRiadok));

            sb.Append(NastavKurzor(parametre.RiadokKurzora + 1, parametre.StlpecKurzora + 1));
        }

        private static void PrekresliRiadok(EditorScreen novaObrazovka, StringBuilder sb, ParametreVypisu parametre, int i)
        {
            sb.Append(NastavKurzor(i + parametre.OkrajHore + 1, 1));
            sb.Append(ZmazOdKurzoraPoKoniecRiadku());
            sb.Append(novaObrazovka.Riadky[i]);
        }

        public static void Vykresli(EditorScreen novaObrazovka, StringBuilder sb, string stavovyRiadok, ParametreVypisu parametre)
        {
            sb.Append(NastavKurzor(parametre.OkrajHore + 1, 1));
            foreach (var riadok in novaObrazovka.Riadky)
            {
                sb.AppendLine(riadok);
            }

            sb.Append(NastavKurzor(parametre.VyskaKonzoly, parametre.OkrajVlavo + 1));
            sb.Append(StavovyRiadok(stavovyRiadok));

            sb.Append(NastavKurzor(parametre.RiadokKurzora + 1, parametre.StlpecKurzora + 1));
        }

        public static string NastavKurzor(int riadok, int stlpec)
        {
            return string.Format("\u001b[{0};{1}H", riadok, stlpec);
        }

        public static string ZmazOdKurzoraPoKoniecRiadku()
        {
            return string.Format("\u001b[0K");
        }

        public static string StavovyRiadok(string s)
        {
            return string.Format("\u001b[44;1m{0}\u001b[0m", s);
        }
        public static string Chyba()
        {
            return string.Format("\u001b[41;1m{0}\u001b[0m", "???");
        }

        public static string Hlaska(string v)
        {
            return string.Format("\u001b[42;1m{0}\u001b[0m", v);
        }

        public static string CislaRiadkov(string v)
        {
            return string.Format("\u001b[2m{0}\u001b[0m", v);
        }

        public static string VykresliHlasku(string hlaska, int okraj)
        {
            var sb = new StringBuilder();
            sb.Append(NastavKurzor(1, 1));
            sb.Append(ZmazOdKurzoraPoKoniecRiadku());
            sb.Append(NastavKurzor(1, okraj + 1));
            sb.Append(Hlaska(hlaska));

            return sb.ToString();
        }

        public static string VykresliChybu(int okraj)
        {
            var sb = new StringBuilder();
            sb.Append(VykreslovaciAutomat.NastavKurzor(2, 1));
            sb.Append(VykreslovaciAutomat.ZmazOdKurzoraPoKoniecRiadku());
            sb.Append(VykreslovaciAutomat.NastavKurzor(2, okraj + 1));
            sb.Append(VykreslovaciAutomat.Chyba());

            return sb.ToString();
        }

        internal static string EraseScree()
        {
            return "\u001b[2J";
        }
    }
}
