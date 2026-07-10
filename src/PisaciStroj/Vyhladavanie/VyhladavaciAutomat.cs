using Lexer.Algoritmy;
using PisaciStroj.Pamat;
using PisaciStroj.Parametre;
using System;
using System.Collections.Generic;
using System.Text;

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
        VyhladaneSlovo? VyhladajNasledujuci(GapBuffer text, int pozicia, string vyhladavanyText);

        VyhladaneSlovo? VyhladajNasledujuciRegex(GapBuffer text, int pozicia, string regex);

        Dictionary<int, VyhladaneSlovo> VyhladajVsetky(GapBuffer text, string vyhladavanyText);

        Dictionary<int, Dictionary<int, VyhladaneSlovo>> VyhladajVsetky(ParametreVypisu parametre, List<GapBuffer> text, string vyhladavanyText);

        Dictionary<int, VyhladaneSlovo> VyhladajVsetkyRegex(GapBuffer text, string regex);

        void NastavVyhladavaciAutomat(string vyhladavanyText);
    }

    public class VyhladavaciAutomat : IVyhladavac
    {
        private SethiUllman _sethiUllman;
        private IDfaSimulator _automat;

        public VyhladavaciAutomat()
        {
            _sethiUllman = new SethiUllman();
        }

        public void NastavVyhladavaciAutomat(string vyhladavanyText)
        {
            var regex = SkonstruujRegex(vyhladavanyText);

            var a = _sethiUllman.BuildDfaForSearch(regex);
            
            _automat = new DfaSimulator(a);
        }

        public VyhladaneSlovo? VyhladajNasledujuci(GapBuffer text, int pozicia, string vyhladavanyText)
        {
            var regex = SkonstruujRegex(vyhladavanyText);

            var automat = SkonstruujAutomat(regex);

            return VyhladajNasledujuci(text, pozicia, automat);
        }

        private VyhladaneSlovo? VyhladajNasledujuci(GapBuffer text, int pozicia, IDfaSimulator automat)
        {
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

        public VyhladaneSlovo? VyhladajNasledujuciRegex(GapBuffer text, int pozicia, string regex)
        {
            var automat = SkonstruujAutomat(regex);

            return VyhladajNasledujuci(text, pozicia, automat);
        }

        public Dictionary<int, VyhladaneSlovo> VyhladajVsetky(GapBuffer text, string vyhladavanyText)
        {
            var regex = SkonstruujRegex(vyhladavanyText);

            var automat = SkonstruujAutomat(regex);

            return VyhladajVsetky(text, automat);
        }

        private Dictionary<int, VyhladaneSlovo> VyhladajVsetky(GapBuffer text, IDfaSimulator automat)
        {
            var result = new Dictionary<int, VyhladaneSlovo>();
            var pozicia = 0;
            while (true)
            {
                var nasledujuci = VyhladajNasledujuci(text, pozicia, automat);

                if (nasledujuci.HasValue)
                {
                    result.Add(nasledujuci.Value.Pozicia, nasledujuci.Value);

                    pozicia = nasledujuci.Value.Pozicia + nasledujuci.Value.Dlzka;
                }
                else
                {
                    pozicia = text.Length();
                }

                if (pozicia == text.Length())
                {
                    break;
                }
            }

            return result;
        }

        public Dictionary<int, VyhladaneSlovo> VyhladajVsetkyRegex(GapBuffer text, string regex)
        {
            var automat = SkonstruujAutomat(regex);

            return VyhladajVsetky(text, automat);
        }

        protected virtual IDfaSimulator SkonstruujAutomat(string regex)
        {
            if(_automat != null)
            {
                _automat.Reset();
                return _automat;
            }

            var automat = _sethiUllman.BuildDfaForSearch(regex);

            return new DfaSimulator(automat);
        }

        protected string SkonstruujRegex(string vyhladavanyText)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < vyhladavanyText.Length; i++)
            {
                sb.Append(vyhladavanyText[i]);
                sb.Append('.');
            }

            sb.Append('\0');

            return sb.ToString();
        }

        public Dictionary<int, Dictionary<int, VyhladaneSlovo>> VyhladajVsetky(ParametreVypisu parametre, List<GapBuffer> text, string vyhladavanyText)
        {
            var pocetRiadkov = 0;

            var result = new Dictionary<int, Dictionary<int, VyhladaneSlovo>>();
            for (int i = parametre.OffsetRiadok; i < text.Count; i++)
            {
                if (pocetRiadkov == parametre.Vyska)
                {
                    break;
                }

                var vyhladaneSlova = VyhladajVsetky(text[i], vyhladavanyText);

                result.Add(i, vyhladaneSlova);
                pocetRiadkov++;
            }

            return result;
        }
    }
}
