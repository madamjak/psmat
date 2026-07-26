using Newtonsoft.Json;
using PisaciAutomat.Config;
using PisaciAutomat.Obrazovka;
using PisaciAutomat.Prikazy.Vykreslovanie;
using PisaciAutomat.Prikazy.Vysledky;
using PisaciStroj.Chyby;
using PisaciStroj.Lexer;
using PisaciStroj.Lexer.Algoritmy;
using PisaciStroj.Navigacia;
using PisaciStroj.Pamat;
using PisaciStroj.Parametre;
using PisaciStroj.Vyhladavanie;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static PisaciStroj.Vyhladavanie.VyhladavaciAutomat;

namespace PisaciAutomat.Prikazy
{
    public struct PrikazPrePrikazovyRiadok
    {
        //search
        public string VyhladavanyText { get; set; }

        //save
        public bool UlozSuborAko { get; set; }
        public string ExistujucaCesta { get; set; }

        //find all vysledky
        public bool ZobrazVysledky { get; set; }
        public Dictionary<int, Dictionary<int, VyhladaneSlovo>> Vysledky { get; set; }
        public VyhladaneSlovo? GoToSlovo { get; set; }

        //goto
        public bool GoToPozicia { get; set; }
    }

    public class PrikazovyAutomat
    {
        private PrikazovyAutomatResult _prikazNaPotvrdenie;

        private GapBuffer _riadok;
        private List<GapBuffer> _riadky;

        private ParametreVypisu _parametreVypisu;
        private NavigovaciPrikaz _navigovaciPrikaz;
        private ParametreVyberu _vyber;

        private bool _chyba;
        private bool _chybaReset;

        private HistoriaPrikazov _historiaPrikazov;

        //search results
        private VysledkovyAutomat _vysledky;
        private bool _resultsMode;
        private bool _zmazHlasku;

        //vykreslovanie
        private VykreslovacCmd _vykreslovacCmd;
        private ParametrePrekreslenia _parametreVykreslovania;

        public PrikazovyAutomat()
        {
            var l = new LexAutomat(new LexGramatika() 
                                    {
                                        Pravidla = GramatikaPrikazov.Gramatika()
                                    });

            _vykreslovacCmd = new VykreslovacCmd(l);

            _riadok = new GapBuffer();
            _riadky = new List<GapBuffer> { _riadok };
            _parametreVypisu = new ParametreVypisu();
            _navigovaciPrikaz = new NavigovaciPrikaz();
            _vyber = new ParametreVyberu();
            _historiaPrikazov = new HistoriaPrikazov();
            _vysledky = new VysledkovyAutomat();
        }

        public PrikazovyAutomatResult NacitajPrikaz(PrikazPrePrikazovyRiadok? prikazZEditora = null, ConsoleKeyInfo? vstup = null)
        {
            _parametreVykreslovania = new ParametrePrekreslenia();
            if (prikazZEditora.HasValue)
            {
                if (prikazZEditora.Value.VyhladavanyText != null)
                {
                    var p = VyhladavaciPrikaz(prikazZEditora);

                    return p;
                }

                if (prikazZEditora.Value.UlozSuborAko)
                {
                    _riadok.Delete(0, _riadok.Length());
                    _parametreVypisu.Stlpec = 0;
                    _parametreVypisu.OffsetStlpec = 0;

                    var prikaz = string.Format("saas \"{0}\"", prikazZEditora.Value.ExistujucaCesta);

                    foreach (char ch in prikaz)
                    {
                        NapisZnak(ch);
                    }

                    return new PrikazovyAutomatResult();
                }

                if (prikazZEditora.Value.ZobrazVysledky)
                {
                    _resultsMode = true;
                    _vysledky.NastavVysledkovyAutomat(prikazZEditora.Value.Vysledky);

                    return new PrikazovyAutomatResult();
                }

                if (prikazZEditora.Value.GoToSlovo.HasValue)
                {
                    return new PrikazovyAutomatResult()
                    {
                        Prikaz = new Prikaz()
                        {
                            Typ = TypPrikazu.GoToSlovo,
                            GoTo = prikazZEditora.Value.GoToSlovo.Value
                        }
                    };
                }

                if (prikazZEditora.Value.GoToPozicia)
                {
                    _riadok.Delete(0, _riadok.Length());
                    _parametreVypisu.Stlpec = 0;
                    _parametreVypisu.OffsetStlpec = 0;

                    var prikaz = "goto ";

                    foreach (char ch in prikaz)
                    {
                        NapisZnak(ch);
                    }

                    return new PrikazovyAutomatResult() 
                    {
                        Hlaska = "Zadaj cislo riadku a stlpca."
                    };
                }

            } else if (vstup.HasValue)
            {
                var r = SpracujVstup(vstup.Value);

                return r;
            }

            return new PrikazovyAutomatResult();
        }

        private PrikazovyAutomatResult VyhladavaciPrikaz(PrikazPrePrikazovyRiadok? prikazZEditora)
        {
            _riadok.Delete(0, _riadok.Length());
            _parametreVypisu.Stlpec = 0;
            _parametreVypisu.OffsetStlpec = 0;

            var prikaz = string.Format("fnext \"{0}\"", prikazZEditora.Value.VyhladavanyText);

            foreach (char ch in prikaz)
            {
                NapisZnak(ch);
            }

            PridajPrikazDoHistorie();

            return new PrikazovyAutomatResult()
            {
                Prikaz = new Prikaz()
                {
                    Typ = TypPrikazu.VyhladajDalsi,
                    VyhladavanyText = prikazZEditora.Value.VyhladavanyText
                }
            };
        }

        public PrikazovyAutomatResult SpracujVstup(ConsoleKeyInfo vstup)
        {
            int indexStlpec, offsetStlpec, dlzkaRiadku;
            ResetMonitoruPrekreslenia(out indexStlpec, out offsetStlpec, out dlzkaRiadku);

            var r = new PrikazovyAutomatResult();

            if (_resultsMode)
            {
                return ResultsMode(vstup);
            }
            else if (_prikazNaPotvrdenie != null)
            {
                if (vstup.KeyChar == 'a')
                {
                    return PotvrdPrikaz(_prikazNaPotvrdenie);
                }
                else
                {
                    PridajPrikazDoHistorie();
                }

                _prikazNaPotvrdenie = null;
            }
            else if (Navigator.NavigujVPrikazovomRiadku(vstup, _navigovaciPrikaz))
            {
                var malVybranyText = Zvyraznovac.MaVybranyText(_vyber);
                if (!_navigovaciPrikaz.Vyber)
                {
                    _vyber = new ParametreVyberu();
                }

                var bracketHighlighted = false;
                if (_parametreVypisu.IndexStlpec < _riadok.Length())
                {
                    bracketHighlighted = StackBracketMatching.Zatvorky.Contains(_riadok.CharAt(_parametreVypisu.IndexStlpec));
                }

                Navigator.Naviguj(_navigovaciPrikaz, _parametreVypisu, _riadky, _vyber);

                var bracketHighlightedPo = false;
                if (_parametreVypisu.IndexStlpec < _riadok.Length())
                {
                    bracketHighlightedPo = StackBracketMatching.Zatvorky.Contains(_riadok.CharAt(_parametreVypisu.IndexStlpec));
                }

                var zmenaVyberuTextu = (!malVybranyText && Zvyraznovac.MaVybranyText(_vyber))
                    || (Zvyraznovac.MaVybranyText(_vyber)
                        && indexStlpec != _parametreVypisu.IndexStlpec);
                var resetVyberuTextu = malVybranyText && !Zvyraznovac.MaVybranyText(_vyber);
                var zmenaStranky = offsetStlpec != _parametreVypisu.OffsetStlpec;
                var zmenaBracketHighlight = !(!bracketHighlighted && !bracketHighlightedPo);

                _parametreVykreslovania.Necitaj = true;
                if (zmenaStranky || zmenaVyberuTextu || resetVyberuTextu || zmenaBracketHighlight)
                {
                    _parametreVykreslovania.Necitaj = false;
                    _parametreVykreslovania.LenPrekresli = true;
                    _parametreVykreslovania.ZaciatocnyStlpec = indexStlpec;
                    _parametreVykreslovania.KonecnySlpec = _parametreVypisu.IndexStlpec;
                    if (zmenaVyberuTextu)
                    {
                        if (zmenaVyberuTextu && !zmenaStranky)
                        {
                            _parametreVykreslovania.OptimalizaciaPrekreslenia = true;
                        }

                    }
                }
            }
            else if (vstup.Key == ConsoleKey.DownArrow || vstup.Key == ConsoleKey.UpArrow)
            {
                if (vstup.Key == ConsoleKey.UpArrow)
                {
                    if (_historiaPrikazov.PocetOperaciiNaVratenie > 0)
                    {
                        _historiaPrikazov.PridajOperaciuNaZopakovanie(_riadok.Read());

                        var o = _historiaPrikazov.OperaciaNaVratenie();

                        ZmazVsetko(true);
                        NapisText(o);
                    }
                    else
                    {
                        var operaciaZHistorie = _historiaPrikazov.PoslednaOperaciaHistorie();
                        if (operaciaZHistorie != null)
                        {
                            _historiaPrikazov.PridajOperaciuNaZopakovanie(_riadok.Read());

                            ZmazVsetko(true);
                            NapisText(operaciaZHistorie);
                        }
                    }
                }
                else
                {
                    if (_historiaPrikazov.PocetOperaciiNaZopakovanie > 0)
                    {
                        _historiaPrikazov.PridajOperaciuNaVratenie(_riadok.Read());

                        var o = _historiaPrikazov.OperaciaNaZopakovanie();

                        ZmazVsetko(true);
                        NapisText(o);
                    }
                }
            }
            else if (vstup.Key == ConsoleKey.Backspace)
            {
                if (Zvyraznovac.MaVybranyText(_vyber))
                {
                    ZmazVyber();
                }
                else
                {
                    if (_parametreVypisu.IndexStlpec > 0)
                    {
                        PosunDolava();

                        _riadok.Delete(_parametreVypisu.IndexStlpec);

                        MonitorOptimalzaciePrekreslenia(indexStlpec, offsetStlpec, dlzkaRiadku);
                    }
                }
            }
            else if (vstup.Key == ConsoleKey.Delete)
            {
                if (Zvyraznovac.MaVybranyText(_vyber))
                {
                    ZmazVyber();
                }
                else
                {
                    if (_riadok.Length() > 0 && _parametreVypisu.IndexStlpec < _riadok.Length())
                    {
                        PosunDoprava();
                        ResetMonitoruPrekreslenia(out indexStlpec, out offsetStlpec, out dlzkaRiadku);
                        PosunDolava();
                        _riadok.Delete(_parametreVypisu.IndexStlpec);
                        MonitorOptimalzaciePrekreslenia(indexStlpec, offsetStlpec, dlzkaRiadku);
                    }
                }
            }
            else if (vstup.Key == ConsoleKey.Escape)
            {
                ZmazVsetko();
                _historiaPrikazov.VycistiOperacieNaZopakovanie();

                r.ZavriRiadok = true;
            }
            else if ((vstup.Modifiers & ConsoleModifiers.Control) == ConsoleModifiers.Control)
            {
                if (vstup.Key == ConsoleKey.Q)
                {
                    return UkonciApplikaciu();
                }

                if (vstup.Key == ConsoleKey.A)
                {
                    _vyber = new ParametreVyberu();
                    Zvyraznovac.VyberVsetko(_vyber, _parametreVypisu, _riadky);
                }

                else if (vstup.Key == ConsoleKey.C && Zvyraznovac.MaVybranyText(_vyber))
                {
                    Clipboard.Clipboard.SkopirujDoClipboardu(PrecitajVyber());
                }
                else if (vstup.Key == ConsoleKey.X && Zvyraznovac.MaVybranyText(_vyber))
                {
                    Clipboard.Clipboard.SkopirujDoClipboardu(PrecitajVyber());

                    ZmazVyber();
                    _vyber = new ParametreVyberu();
                }
                else if (vstup.Key == ConsoleKey.V)
                {
                    var t = Clipboard.Clipboard.PreciajZClipboardu();

                    if (!string.IsNullOrEmpty(t))
                    {
                        NapisText(t);
                        _vyber = new ParametreVyberu();
                    }
                }
            }
            else if (Program.IsPrintable(vstup.KeyChar))
            {
                if (Zvyraznovac.MaVybranyText(_vyber))
                {
                    ZmazVyber();
                    PrekresliZCmd();
                    ResetMonitoruPrekreslenia(out indexStlpec, out offsetStlpec, out dlzkaRiadku);
                }

                NapisZnak(vstup.KeyChar);
                MonitorOptimalzaciePrekreslenia(indexStlpec, offsetStlpec, dlzkaRiadku);
            }
            else if (vstup.Key == ConsoleKey.Enter)
            {
                if (Zvyraznovac.MaVybranyText(_vyber))
                {
                    ZmazVyber();
                }

                return SpracujPrikaz();
            }

            return r;
        }

        private void ResetMonitoruPrekreslenia(out int indexStlpec, out int offsetStlpec, out int dlzkaRiadku)
        {
            indexStlpec = _parametreVypisu.IndexStlpec;
            offsetStlpec = _parametreVypisu.OffsetStlpec;
            dlzkaRiadku = _riadok.Length();
            _parametreVykreslovania = new ParametrePrekreslenia();
        }

        private PrikazovyAutomatResult UkonciApplikaciu()
        {
            ZmazVsetko();
            _historiaPrikazov.VycistiOperacieNaZopakovanie();

            return new PrikazovyAutomatResult()
            {
                ZavriRiadok = true,
                Ukonci = true
            };
        }

        private PrikazovyAutomatResult ResultsMode(ConsoleKeyInfo vstup)
        {
            _resultsMode = true;

            var r = _vysledky.SpracujVstup(vstup);

            if (r.Prikaz != null)
            {
                return NacitajPrikaz(r.Prikaz);
            }

            if (r.Ukonci)
            {
                _resultsMode = false;
                _zmazHlasku = true;
                return new PrikazovyAutomatResult()
                {
                    ZavriRiadok = true,
                    Ukonci = true
                };
            }
            else if (r.ZavriVysledky)
            {
                _resultsMode = false;
                _zmazHlasku = true;
            }

            return new PrikazovyAutomatResult();
        }

        private PrikazovyAutomatResult SpracujPrikaz()
        {
            var r = new PrikazovyAutomatResult();

            var prikaz = CitacPrikazov.NacitajPrikaz(_riadok, _vykreslovacCmd.GetTokens());

            
            if (prikaz != null && prikaz.Potvrd)
            {
                r.Dialog = prikaz.Dialog;
                _prikazNaPotvrdenie = prikaz;
                return r;
            }

            if (prikaz == null || prikaz.Prikaz == null)
            {
                _chyba = true;
                if(prikaz != null)
                {
                    r.Hlaska = prikaz.Hlaska;
                }
            }
            else
            {
                return PotvrdPrikaz(prikaz);
            }

            return r;
        }
        private PrikazovyAutomatResult PotvrdPrikaz(PrikazovyAutomatResult prikaz)
        {
            PridajPrikazDoHistorie();
            if (prikaz.Prikaz.ZavriRiadok)
            {
                ZmazVsetko();
                _historiaPrikazov.VycistiOperacieNaZopakovanie();
            }
            else
            {
                _historiaPrikazov.VycistiOperacieNaZopakovanie();
            }

            prikaz.ZavriRiadok = prikaz.Prikaz.ZavriRiadok;

            return prikaz;
        }

        private void PridajPrikazDoHistorie()
        {
            _historiaPrikazov.VycistiOperacieNaZopakovanie();
            _historiaPrikazov.PridajOperaciuDoHistorie(_riadok.Read());
        }

        private string PrecitajVyber()
        {
            return _riadok.Read(_vyber.Zaciatok.Value.Stlpec, _vyber.PocetZnakov);
        }

        private void ZmazVsetko(bool ingorujOptimalizaciuPrekreslovania = false)
        {
            Zvyraznovac.VyberVsetko(_vyber, _parametreVypisu, _riadky);
            ZmazVyber(ingorujOptimalizaciuPrekreslovania);
        }

        private void ZmazVyber(bool ingorujOptimalizaciuPrekreslovania = false)
        {
            if (!ingorujOptimalizaciuPrekreslovania)
            {
                _parametreVykreslovania.ZaciatocnyStlpec = _vyber.Koniec.Value.Stlpec;
                _parametreVykreslovania.KonecnySlpec = _vyber.Zaciatok.Value.Stlpec;

                var dlzkaRiadku = _riadok.Length();
                var jednoznakoveMazanie = _parametreVykreslovania.KonecnySlpec - _parametreVykreslovania.ZaciatocnyStlpec == -1;
                if (jednoznakoveMazanie || dlzkaRiadku < _parametreVypisu.SirkaKonzoly)
                {
                    _parametreVykreslovania.OptimalizaciaPrekreslenia = true;
                }
            }

            var s = _vyber.Zaciatok.Value.Stlpec;
            _riadok.Delete(s, _vyber.PocetZnakov);
            _parametreVypisu.Stlpec = 0;
            _parametreVypisu.OffsetStlpec = 0;
            while(_parametreVypisu.IndexStlpec != s)
            {
                PosunDoprava();
            }
            _vyber = new ParametreVyberu();
        }

        private void MonitorOptimalzaciePrekreslenia(int indexStlpec, int offsetStlpec, int dlzkaRiadku)
        {
            if (indexStlpec == _parametreVypisu.IndexStlpec)
            {
                return;
            }

            _parametreVykreslovania.KonecnySlpec = _parametreVypisu.IndexStlpec;
            _parametreVykreslovania.ZaciatocnyStlpec = indexStlpec;

            var zapis = _parametreVypisu.IndexStlpec - indexStlpec > 0;
            var jednoznakoveMazanie = _parametreVypisu.IndexStlpec - indexStlpec == -1;
            if (offsetStlpec == _parametreVypisu.OffsetStlpec
                && (zapis || jednoznakoveMazanie || dlzkaRiadku < _parametreVypisu.SirkaKonzoly))
            {
                _parametreVykreslovania.OptimalizaciaPrekreslenia = true;
            }
        }

        private void NapisText(string t)
        {
            foreach(var c in t)
            {
                if(PisaciStroj.Program.CarriageReturn(c) || PisaciStroj.Program.LineFeed(c))
                {
                    continue;
                }

                NapisZnak(c);
            }
        }

        private void NapisZnak(char ch)
        {
            _riadok.Insert(ch, _parametreVypisu.IndexStlpec);

            PosunDoprava();
        }

        public void Resize(int novaSirka)
        {
            var indexStlpec = _parametreVypisu.IndexStlpec;

            _parametreVypisu.SirkaKonzoly = novaSirka;
            _parametreVypisu.Stlpec = 0;
            _parametreVypisu.OffsetStlpec = 0;

            while(_parametreVypisu.IndexStlpec != indexStlpec)
            {
                PosunDoprava();
            }

            _vysledky.Resize(novaSirka);
        }

        private void PosunDoprava()
        {
            _parametreVypisu.Stlpec++;
            if (_parametreVypisu.Stlpec == _parametreVypisu.Sirka)
            {
                _parametreVypisu.OffsetStlpec++;
                _parametreVypisu.Stlpec--;

            }
        }

        private void PosunDolava()
        {
            _parametreVypisu.Stlpec--;
            if (_parametreVypisu.Stlpec < 0)
            {
                _parametreVypisu.OffsetStlpec -= _parametreVypisu.Sirka;
                if (_parametreVypisu.OffsetStlpec < 0)
                {
                    _parametreVypisu.OffsetStlpec = 0;
                }

                _parametreVypisu.Stlpec = _parametreVypisu.Sirka - 1;

                if (_parametreVypisu.Stlpec > _riadok.Length() - 1)
                {
                    _parametreVypisu.Stlpec = _riadok.Length() - 1;
                }
            }
        }

        public void Prekresli(ParametrePrekreslenia p, StringBuilder sb, List<GapBuffer> riadkyEditora)
        {
            _parametreVypisu.OkrajVlavo = p.OkrajVlavo;
            _parametreVykreslovania.OkrajVlavo = p.OkrajVlavo;

            if (p.Resize || !_resultsMode)
            {
                PrekresliPrikazovyRiadok(sb);
            }

            if (_resultsMode)
            {
                sb.Append(VykreslovaciAutomat.NastavKurzor(1, p.OkrajVlavo));
                sb.Append(VykreslovaciAutomat.ZmazOdZaciatkuRiadkuPoKurzor());
                _vysledky.Prekresli(p, sb, riadkyEditora);

                return;
            }

            if (_zmazHlasku)
            {
                _vysledky.ZmazInfoHlasku(sb);
                _zmazHlasku = false;
            }
        }

        public void PrekresliZCmd()
        {
            var sb = new StringBuilder();

            _parametreVykreslovania.OkrajVlavo = _parametreVypisu.OkrajVlavo;

            PrekresliPrikazovyRiadok(sb);

            var upraveny = sb.ToString();
            if (!string.IsNullOrEmpty(upraveny))
            {
                Console.Write(upraveny);
            }
        }

        public void PrekresliPrikazovyRiadok(StringBuilder sb)
        {
            sb.Append(_vykreslovacCmd.PrecitajPrikazovyRiadok(_parametreVykreslovania,
                _riadok,
                _parametreVypisu,
                _vyber,
                _riadky,
                _chyba,
                _chybaReset));

            if (_chybaReset)
            {
                _chybaReset = false;
            }
            if (_chyba)
            {
                _chyba = false;
                _chybaReset = true;
            }
        }

        public void NastavKurzor(StringBuilder sb)
        {
            if (!_resultsMode)
            {
                sb.Append(VykreslovaciAutomat.NastavKurzor(1, _parametreVypisu.StlpecKurzora + 1));
                sb.Append(VykreslovaciAutomat.NastavKurzorVisible());
            }
        }
    }
}
