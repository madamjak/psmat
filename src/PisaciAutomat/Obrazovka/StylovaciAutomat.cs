using PisaciAutomat.Config;
using PisaciStroj.Lexer;
using PisaciStroj.Navigacia;
using PisaciStroj.Pamat;
using PisaciStroj.Vyhladavanie;
using System;
using System.Collections.Generic;
using System.Text;
using static PisaciAutomat.Config.Farby;

namespace PisaciAutomat.Obrazovka
{
    public static class StylovaciAutomat
    {
        public static string SyntaxHighligt(Dictionary<int, Token> tokens, 
            GapBuffer riadok, 
            int offset, 
            int maxDlzka, 
            VyhladaneSlovo? zvyraznenyText, 
            FarbaPozadia pozadie, 
            FarbaPozadia farbaZvyraznenia)
        {
            var sb = new StringBuilder();
            var index = offset;
            var dlzka = 0;
            var dlzkaZvyraznenehoTextu = 0;

            while (true)
            {
                if(index >= riadok.Length())
                {
                    break;
                }

                if(dlzka == maxDlzka)
                {
                    break;
                }

                if (zvyraznenyText.HasValue && zvyraznenyText.Value.Pozicia == index)
                {
                    dlzkaZvyraznenehoTextu = zvyraznenyText.Value.Dlzka;
                }

                Token t;
                if (dlzkaZvyraznenehoTextu > 0)
                {
                    sb.Append(AnsiStyl(farbaZvyraznenia));
                    sb.Append(riadok.Read(index, 1));
                    sb.Append(AnsiReset());
                    dlzkaZvyraznenehoTextu--;
                    index++;
                    dlzka++;
                }
                else if (tokens.TryGetValue(index, out t))
                {
                    sb.Append(AnsiStyl(pozadie));
                    var styl = VyberStyl(t.Typ);

                    if (styl != StylTextu.Standard)
                    {
                        sb.Append(AnsiStyl(styl));
                    }

                    var dlzkaT = t.Dlzka;
                    if (dlzka + t.Dlzka > maxDlzka)
                    {
                        dlzkaT = maxDlzka - dlzka;
                    }

                    sb.Append(riadok.Read(index, t.Dlzka));

                    //if (styl != StylTextu.Standard)
                    //{
                        sb.Append(AnsiReset());
                    //}

                    index += dlzkaT;
                    dlzka += dlzkaT;
                }
                else
                {
                    sb.Append(AnsiStyl(pozadie));
                    sb.Append(riadok.Read(index, 1));
                    sb.Append(AnsiReset());
                    index++;
                    dlzka += 1;
                }
            }

            return sb.ToString();
        }

        public static string SyntaxAndSearchHighligt2(GapBuffer riadok, 
            int offset, 
            int maxDlzka, 
            Dictionary<int, VyhladaneSlovo> slova, 
            VyhladaneSlovo? vyhladaneSlovo, 
            Dictionary<int, Token> tokeny, 
            Dictionary<int, Zatvorka> zatvorky, 
            Pozicia poziciaKurzora,
            VyhladaneSlovo? zvyraznenyText,
            Dictionary<int, Token> regexTokens,
            FarbaPozadia? pozadieRiadku = null)
        {
            var sb = new StringBuilder();
            var index = 0;
            var dlzka = 0;

            var dlzkaSlova = 0;
            var dlzkaTokenu = 0;
            Token? lastToken = null;
            bool extraZvyrazni = false;
            bool zvyrazniZatvorku = false;

            Token? lastRegex = null;
            var dlzkaRegexTokenu = 0;
            
            var dlzkaZvyraznenehoTextu = 0;

            sb.Append(AnsiReset(pozadieRiadku));
            while (true)
            {
                var precitalSlovo = false;
                var precitalToken = false;
                var precitalZatvorku = false;

                if (index == riadok.Length())
                {
                    if(index == 0 && zvyraznenyText.HasValue)
                    {
                        sb.Append(AnsiStyl(Farby.FarbaVysledkov()));
                        sb.Append(" ");
                        sb.Append(AnsiReset(pozadieRiadku));
                    }
                    break;
                }

                if (dlzka == maxDlzka)
                {
                    break;
                }

                VyhladaneSlovo s;
                if (dlzkaSlova == 0 && slova.TryGetValue(index, out s))
                {
                    dlzkaSlova = s.Dlzka;
                    extraZvyrazni = vyhladaneSlovo.HasValue && vyhladaneSlovo.Value.Pozicia == s.Pozicia;
                }

                Token t;
                if (dlzkaTokenu == 0 && tokeny.TryGetValue(index, out t))
                {
                    dlzkaTokenu = t.Dlzka;
                    lastToken = t;
                }

                Zatvorka z;
                if(zatvorky != null && zatvorky.TryGetValue(index, out z))
                {
                    precitalZatvorku = true;
                    zvyrazniZatvorku = (poziciaKurzora.Riadok == z.Start.Riadok && poziciaKurzora.Stlpec == z.Start.Stlpec)
                        || (poziciaKurzora.Riadok == z.End.Riadok && poziciaKurzora.Stlpec == z.End.Stlpec);
                }

                if (zvyraznenyText.HasValue && zvyraznenyText.Value.Pozicia == index)
                {
                    dlzkaZvyraznenehoTextu = zvyraznenyText.Value.Dlzka;
                }

                Token r;
                if(dlzkaRegexTokenu == 0 && regexTokens.TryGetValue(index, out r))
                {
                    dlzkaRegexTokenu = r.Dlzka;
                    lastRegex = r;
                }

                if (dlzkaSlova > 0)
                {
                    if(index >= offset)
                    {
                        sb.Append(extraZvyrazni ? StylSearchResultExtra() : StylSearchResult());
                        sb.Append(riadok.Read(index, 1));
                        sb.Append(AnsiReset(pozadieRiadku));
                    }

                    dlzkaSlova--;
                    precitalSlovo = true;
                }

                if (dlzkaTokenu > 0)
                {
                    if(index >= offset && !precitalSlovo)
                    {
                        if (precitalZatvorku)
                        {
                            if (zvyrazniZatvorku)
                            {
                                sb.Append(Farby.StylSearchResultExtra());
                            }
                            else
                            {
                                if (Farby.BracketHighlighted)
                                {
                                    sb.Append(Farby.StylZatvorky());
                                }
                            }
                            sb.Append(riadok.Read(index, 1));
                            sb.Append(AnsiReset(pozadieRiadku));
                        } else if (dlzkaRegexTokenu > 0)
                        {
                            var rstyl = VyberStylRegex(lastRegex.Value.Typ);
                            if (rstyl != StylTextu.Standard)
                            {
                                sb.Append(AnsiStyl(rstyl));
                            }
                            sb.Append(riadok.Read(index, 1));
                            if (rstyl != StylTextu.Standard)
                            {
                                sb.Append(AnsiReset(pozadieRiadku));
                            }
                        }
                        else
                        {
                            var styl = VyberStyl(lastToken.Value.Typ);
                            if (styl != StylTextu.Standard)
                            {
                                sb.Append(AnsiStyl(styl));
                            }
                            sb.Append(riadok.Read(index, 1));
                            if (styl != StylTextu.Standard)
                            {
                                sb.Append(AnsiReset(pozadieRiadku));
                            }
                        }
                    }

                    dlzkaTokenu--;
                    precitalToken = true;
                    if(dlzkaRegexTokenu > 0)
                    {
                        dlzkaRegexTokenu--;
                    }
                }

                if(!precitalToken && !precitalSlovo && precitalZatvorku)
                {
                    if(index >= offset)
                    {
                        if (zvyrazniZatvorku)
                        {
                            sb.Append(Farby.StylSearchResultExtra());
                        }
                        else
                        {
                            if (Farby.BracketHighlighted)
                            {
                                sb.Append(Farby.StylZatvorky());
                            }
                        }
                        sb.Append(riadok.Read(index, 1));
                        sb.Append(AnsiReset(pozadieRiadku));
                    }
                }
                
                if (!precitalSlovo && !precitalToken && !precitalZatvorku && index >= offset)
                {
                    sb.Append(riadok.Read(index, 1));
                }

                if (dlzkaZvyraznenehoTextu > 0)
                {
                    if(index >= offset && !precitalSlovo)
                    {
                        sb.Append(Farby.AnsiStyl(Farby.FarbaVysledkov()));
                        sb.Append("\b");
                        sb.Append(riadok.Read(index, 1));
                        sb.Append(AnsiReset(pozadieRiadku));
                    }

                    dlzkaZvyraznenehoTextu--;
                }

                if(index >= offset)
                {
                    dlzka += 1;
                }
                index += 1;
            }

            return sb.ToString();
        }
    }
}
