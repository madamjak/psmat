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
    public struct ParametrePrekreslenia
    {
        public bool Resize { get; set; }

        public bool Necitaj { get; set; }
    }

    public class VykreslovaciAutomat
    {
        private EditorScreen _aktualnaObrazovka;
        
        private ILexer _lexer;
        private IPisaciStroj _editor;

        private StavovyRiadok _stavovyRiadok;

        public VykreslovaciAutomat(LexGramatika gramatika, IPisaciStroj editor)
        {
            _lexer = new LexAutomat(gramatika);
            _editor = editor;
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

            return Precitaj2(parametre, search, lexResult, _editor, parametreVyberu, _lexer);
        }

        public static EditorScreen Precitaj2(ParametreVypisu parametre,
            ParametreVyhladavania search,
            LexResult lexResult,
            IPisaciStroj editor,
            ParametreVyberu parametreVyberu,
            ILexer lexer)
        {
            var result = new EditorScreen(parametre.Sirka, parametre.Vyska)
            {
                Riadok = parametre.RiadokKurzora + 1,
                Stlpec = parametre.StlpecKurzora + 1,
            };

            var pocetRiadkov = 0;
            var riadokObrazovky = 0;
            var riadky = editor.Riadky();
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

                if (search.VyhladavanyText != null)
                {
                    if (search.ZaciatokVyhladavania.HasValue)
                    {
                        //....toto by malo byt sucastou vyhladavaca
                        if(i >= search.ZaciatokVyhladavania.Value.Riadok)
                        {
                            vyhladaneSlova = editor.VyhladajVsetky(riadky[i], search.VyhladavanyText);

                            if(i == search.ZaciatokVyhladavania.Value.Riadok)
                            {
                                var poz = vyhladaneSlova.Keys.Where(x => x < search.ZaciatokVyhladavania.Value.Stlpec);
                                if (poz.Any())
                                {
                                    foreach(var p in poz)
                                    {
                                        vyhladaneSlova.Remove(p);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        vyhladaneSlova = editor.VyhladajVsetky(riadky[i], search.VyhladavanyText);
                    }
                }
                if (search.VyhladaneSlovo.HasValue && search.VyhladaneSlovo.Value.Riadok == i)
                {
                    vSlovo = search.VyhladaneSlovo;
                }

                if (lexResult.Tokeny == null || !lexResult.Tokeny.TryGetValue(i, out tokeny))
                {
                    tokeny = lexer.Lex(riadky[i]);
                }
                else
                {
                    var r = new Dictionary<int, Token>();
                    var t = lexer.Lex(riadky[i]);
                    foreach(var to in t)
                    {
                        var zvyrazniToken = true;
                        foreach(var koment in tokeny)
                        {
                            r.TryAdd(koment.Key, koment.Value);
                            if(koment.Key <= to.Key && to.Key <= koment.Key + koment.Value.Dlzka)
                            {
                                zvyrazniToken = false;
                            }
                        }

                        if (zvyrazniToken)
                        {
                            r.Add(to.Key, to.Value);
                        }
                    }

                    tokeny = r;
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

                result.Riadky[riadokObrazovky] = string.Format("{0}  {1}", CislaRiadkov((i).ToString("D3")),
                    StylovaciAutomat.SyntaxAndSearchHighligt2(riadky[i],
                    parametre.OffsetStlpec, parametre.Sirka,
                    vyhladaneSlova, vSlovo, tokeny, zatvorky, poziciaKurzora,
                    zvyraznenyText));

                pocetRiadkov++;
                riadokObrazovky++;
            }

            return result;
        }

        public void VykresliNaKonzolu(EditorScreen novaObrazovka, StavovyRiadokInfo stavovyRiadok, ParametreVypisu parametre, string hlaska, bool _cmdMode, ParametrePrekreslenia p)
        {
            var sb = new StringBuilder();

            sb.Append(NastavKurzorUnVisible());
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

            sb.Append(_stavovyRiadok.Vykresli(p.Resize, stavovyRiadok, parametre));

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

            sb.Append(NastavKurzorVisible());

            Console.Write(sb.ToString());
        }

        private static void VykresliHlasku(ParametreVypisu parametre, string hlaska, StringBuilder sb)
        {
            sb.Append(VykresliHlasku(hlaska, parametre.OkrajVlavo));
            sb.Append(NastavKurzor(2, 1));
            sb.Append(ZmazOdKurzoraPoKoniecRiadku());
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
            
            sb.Append(NastavKurzor(novaObrazovka.Riadok, novaObrazovka.Stlpec));
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

            sb.Append(NastavKurzor(parametre.RiadokKurzora + 1, parametre.StlpecKurzora + 1));
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

        public static string EraseScree()
        {
            return "\u001b[2J";
        }
    }
}
