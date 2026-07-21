using Lexer.Algoritmy;
using PisaciStroj.Lexer;
using PisaciStroj.Pamat;
using PisaciStroj.Parametre;
using System.Collections.Generic;
using System.Text;
using static PisaciStroj.Vyhladavanie.VyhladavaciAutomat;

namespace PisaciStroj.Vyhladavanie
{
    public struct VyhladaneSlovo
    {
        public int Riadok { get; set; }

        public int Pozicia { get; set; }

        public int Dlzka { get; set; }
    }

    public interface IVyhladavac
    {
        void NastavVyhladavanie(string vyhladavanyText, Lexer.LexResult vyhladavanyRegex);
        VyhladaneSlovo? Vyhladaj(string vyhladavanyText, ParametreVypisu parametreVypisu, List<GapBuffer> riadky, bool obratene = false);
        VyhladaneSlovo? VyhladajNasledujuci(GapBuffer text, int pozicia, string vyhladavanyText, bool obratene = false);
        Dictionary<int, VyhladaneSlovo> VyhladajVsetky(GapBuffer riadok, string vyhladavanyText, bool obratene = false);
        VyhladavacResult VyhladajVsetky(List<GapBuffer> text, string vyhladavanyText);
    }

    public class VyhladavaciAutomat : IVyhladavac
    {
        private AhoSethiUllman _sethiUllman;
        private IDfaSimulator _automat;

        public VyhladavaciAutomat()
        {
            _sethiUllman = new AhoSethiUllman();
        }

        private void NastavVyhladavaciAutomat(string vyhladavanyText, LexResult lexResults)
        {
            var regex = SkonstruujRegex(vyhladavanyText, lexResults);

            var a = _sethiUllman.BuildDfaForSearch(regex);
            
            _automat = new DfaSimulator(a);
        }

        public static string ReverseString(string s)
        {
            var r = new char[s.Length];
            var a = 0;
            for (int i = s.Length - 1; i >= 0; i--)
            {
                r[a] = s[i];
                a++;
            }

            return new string(r);
        }

        public Dictionary<int, VyhladaneSlovo> VyhladajVsetky(GapBuffer riadok, string vyhladavanyText, bool obratene = false)
        {
            return VyhladajVsetkyInternal(riadok, vyhladavanyText, obratene, 0);
        }

        public void NastavVyhladavanie(string vyhladavanyText, LexResult lexResults)
        {
            NastavVyhladavaciAutomat(vyhladavanyText, lexResults);
        }

        public VyhladaneSlovo? Vyhladaj(string vyhladavanyText, ParametreVypisu parametreVypisu, List<GapBuffer> riadky)
        {
            VyhladaneSlovo? vyhladaneSlovo = null;
            for (int i = parametreVypisu.IndexRiadok; i<riadky.Count; i++)
            {
                var index = i == parametreVypisu.IndexRiadok ? parametreVypisu.IndexStlpec : 0;
                vyhladaneSlovo = VyhladajNasledujuci(riadky[i], index, vyhladavanyText);

                if (vyhladaneSlovo.HasValue)
                {
                    return new VyhladaneSlovo()
                    {
                        Riadok = i,
                        Pozicia = vyhladaneSlovo.Value.Pozicia,
                        Dlzka = vyhladaneSlovo.Value.Dlzka
                    };
                }
            }
            return vyhladaneSlovo;
        }

        public VyhladaneSlovo? Vyhladaj(string vyhladavanyText, ParametreVypisu parametreVypisu, List<GapBuffer> riadky, bool obratene = false)
        {
            VyhladaneSlovo? vyhladaneSlovo = null;

            if (obratene)
            {
                for (int i = parametreVypisu.IndexRiadok; i >= 0; i--)
                {
                    var index = i == parametreVypisu.IndexRiadok ? parametreVypisu.IndexStlpec : riadky[i].Length() - 1;
                    if (index < 0)
                    {
                        continue;
                    }
                    vyhladaneSlovo = VyhladajNasledujuci(riadky[i], index, vyhladavanyText, obratene);

                    if (vyhladaneSlovo.HasValue)
                    {
                        return new VyhladaneSlovo()
                        {
                            Riadok = i,
                            Pozicia = vyhladaneSlovo.Value.Pozicia,
                            Dlzka = vyhladaneSlovo.Value.Dlzka
                        };
                    }
                }
            }
            else
            {
                for (int i = parametreVypisu.IndexRiadok; i < riadky.Count; i++)
                {
                    var index = i == parametreVypisu.IndexRiadok ? parametreVypisu.IndexStlpec : 0;
                    vyhladaneSlovo = VyhladajNasledujuci(riadky[i], index, vyhladavanyText);

                    if (vyhladaneSlovo.HasValue)
                    {
                        return new VyhladaneSlovo()
                        {
                            Riadok = i,
                            Pozicia = vyhladaneSlovo.Value.Pozicia,
                            Dlzka = vyhladaneSlovo.Value.Dlzka
                        };
                    }
                }
            }

            return vyhladaneSlovo;
        }

        public VyhladaneSlovo? VyhladajNasledujuci(GapBuffer text, int pozicia, string vyhladavanyText, bool obratene = false)
        {
            VhlAutomatatReset();

            return VyhladajNasledujuciInternal(text, pozicia, _automat, obratene, 0);
        }

        private VyhladaneSlovo? VyhladajNasledujuciInternal(GapBuffer text, int pozicia, IDfaSimulator automat, bool obratene, int indexRiadku)
        {
            if (obratene)
            {
                return VyhladajObratene(text, pozicia, automat);
            }

            var result = default(VyhladaneSlovo?);
            var poziciaHlavy = pozicia;
            var najdenaPozicia = -1;

            while (true)
            {
                if (poziciaHlavy == text.Length())
                {
                    break;
                }

                var canRead = automat.ReadSymbol(text.CharAt(poziciaHlavy));
                if (canRead && najdenaPozicia == -1)
                {
                    najdenaPozicia = poziciaHlavy;
                }
                
                poziciaHlavy++;

                if (automat.IsAccepting().HasValue)
                {
                    result = new VyhladaneSlovo
                    {
                        Riadok = indexRiadku,
                        Pozicia = najdenaPozicia,
                        Dlzka = poziciaHlavy - najdenaPozicia
                    };
                    
                    break;
                }

                if (!canRead)
                {
                    automat.Reset();
                    najdenaPozicia = -1;
                }
            }

            return result;
        }

        private VyhladaneSlovo? VyhladajObratene(GapBuffer text, int pozicia, IDfaSimulator automat)
        {
            var result = default(VyhladaneSlovo?);
            var poziciaHlavy = pozicia;
            var najdenaPozicia = -1;

            while (true)
            {
                if (poziciaHlavy < 0)
                {
                    break;
                }

                var canRead = automat.ReadSymbol(text.CharAt(poziciaHlavy));
                if (canRead && najdenaPozicia == -1)
                {
                    najdenaPozicia = poziciaHlavy;
                }

                poziciaHlavy--;
                
                if (automat.IsAccepting().HasValue)
                {
                    result = new VyhladaneSlovo
                    {
                        Pozicia = poziciaHlavy + 1,
                        Dlzka = najdenaPozicia - poziciaHlavy
                    };

                    break;
                }

                if (!canRead)
                {
                    automat.Reset();
                    najdenaPozicia = -1;
                }
            }

            return result;
        }

        private Dictionary<int, VyhladaneSlovo> VyhladajVsetkyInternal(GapBuffer text, string vyhladavanyText, bool obratene = false, int? indexRiadku = null)
        {
            VhlAutomatatReset();

            var i = 0;
            if (indexRiadku.HasValue)
            {
                i = indexRiadku.Value;
            }
            return VyhladajVsetkyInternal(text, _automat, obratene, i);
        }

        private void VhlAutomatatReset()
        {
            if(_automat != null)
            {
                _automat.Reset();
            }
        }

        private Dictionary<int, VyhladaneSlovo> VyhladajVsetkyInternal(GapBuffer text, IDfaSimulator automat, bool obratene, int indexRiadku)
        {
            if (obratene)
            {
                return VyhladajVsetkyObratene(text, automat);
            }
            
            var result = new Dictionary<int, VyhladaneSlovo>();
            var pozicia = 0;
            while (true)
            {
                var nasledujuci = VyhladajNasledujuciInternal(text, pozicia, automat, obratene, indexRiadku);

                if (nasledujuci.HasValue)
                {
                    result.Add(nasledujuci.Value.Pozicia, nasledujuci.Value);
                    pozicia = nasledujuci.Value.Pozicia + nasledujuci.Value.Dlzka;
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        private Dictionary<int, VyhladaneSlovo> VyhladajVsetkyObratene(GapBuffer text, IDfaSimulator automat)
        {
            var result = new Dictionary<int, VyhladaneSlovo>();
            var pozicia = text.Length() - 1;
            while (true)
            {
                var nasledujuci = VyhladajObratene(text, pozicia, automat);

                if (nasledujuci.HasValue)
                {
                    result.Add(nasledujuci.Value.Pozicia, nasledujuci.Value);
                    pozicia = nasledujuci.Value.Pozicia;
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        protected string SkonstruujRegex(string vyhladavanyText, LexResult lexResults)
        {
            var sb = new StringBuilder();

            if(lexResults != null && lexResults.RegexTokeny.Count > 0)
            {
                return ParseRegex(lexResults, vyhladavanyText, sb);
            }
            else
            {
                for (int i = 0; i < vyhladavanyText.Length; i++)
                {
                    var x = vyhladavanyText[i];
                    if (x == '(' || x == ')' || x == '.' || x == '*' || x == '|')
                    {
                        sb.Append(string.Format("\\{0}", vyhladavanyText[i]));
                    }
                    else
                    {
                        sb.Append(vyhladavanyText[i]);
                    }
                    sb.Append('.');
                }
            }

            sb.Append('\0');

            return sb.ToString();
        }

        private string ParseRegex(LexResult lexResults, string vyhladavanyText, StringBuilder sb)
        {
            sb.Append("(");
            for (int i = 1; i < vyhladavanyText.Length - 1; i++)
            {
                sb.Append(vyhladavanyText[i]);
            }
            sb.Append(")");

            sb.Append('.');
            sb.Append('\0');

            return sb.ToString();
        }

        public class VyhladavacResult
        {
            public Dictionary<int, Dictionary<int, VyhladaneSlovo>> Slova { get; set; }

            public int PocetNajdenychSlov { get; set; }
        }

        public VyhladavacResult VyhladajVsetky(List<GapBuffer> text, string vyhladavanyText)
        {
            var pocet = 0;
            var result = new Dictionary<int, Dictionary<int, VyhladaneSlovo>>();
            for (int i = 0; i < text.Count; i++)
            {
                var vyhladaneSlova = VyhladajVsetkyInternal(text[i], vyhladavanyText, false, i);
                
                if (vyhladaneSlova.Count > 0)
                {
                    result.Add(i, vyhladaneSlova);
                    pocet += vyhladaneSlova.Count;
                }
            }

            return new VyhladavacResult()
            {
                PocetNajdenychSlov = pocet,
                Slova = result
            };
        }
    }
}
