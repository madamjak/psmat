using Newtonsoft.Json;
using PisaciAutomat.Obrazovka;
using PisaciAutomat.Prikazy.Vysledky;
using PisaciStroj.Chyby;
using PisaciStroj.Lexer;
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
        public string VyhladavanyText { get; set; }

        public bool UlozSuborAko { get; set; }
        public string ExistujucaCesta { get; set; }

        //find all vysledky
        public bool ZobrazVysledky { get; set; }
        public Dictionary<int, Dictionary<int, VyhladaneSlovo>> Vysledky { get; set; }
        public VyhladaneSlovo? GoTo { get; set; }
    }

    public class PrikazovyAutomat
    {
        private PrikazovyAutomatResult _prikazNaPotvrdenie;

        private GapBuffer _riadok;
        private List<GapBuffer> _riadky;
        
        private ILexer _lexer;
        private LexResult _tokeny;

        private ParametreVypisu _parametreVypisu;
        private NavigovaciPrikaz _navigovaciPrikaz;
        private ParametreVyberu _vyber;

        private bool _chyba;

        private HistoriaPrikazov _historiaPrikazov;

        //search results
        private VysledkovyAutomat _vysledky;
        private bool _resultsMode;

        public PrikazovyAutomat()
        {
            _lexer = new LexAutomat(NacitajLexGramatiku());
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

                if (prikazZEditora.Value.GoTo.HasValue)
                {
                    return new PrikazovyAutomatResult()
                    {
                        Prikaz = new Prikaz()
                        {
                            Typ = TypPrikazu.GoTo,
                            GoTo = prikazZEditora.Value.GoTo.Value
                        }
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
                Navigator.Naviguj(_navigovaciPrikaz, _parametreVypisu, _riadky, _vyber);

                if (!_navigovaciPrikaz.Vyber)
                {
                    _vyber = new ParametreVyberu();
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

                        ZmazVsetko();
                        NapisText(o);
                    }
                    else
                    {
                        var operaciaZHistorie = _historiaPrikazov.PoslednaOperaciaHistorie();
                        if(operaciaZHistorie != null)
                        {
                            _historiaPrikazov.PridajOperaciuNaZopakovanie(_riadok.Read());

                            ZmazVsetko();
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

                        ZmazVsetko();
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
                if (_parametreVypisu.IndexStlpec > 0)
                {
                    PosunDolava();

                    _riadok.Delete(_parametreVypisu.IndexStlpec);
                }
            }
            else if(vstup.Key == ConsoleKey.Delete)
            {
                if (Zvyraznovac.MaVybranyText(_vyber))
                {
                    ZmazVyber();
                }
                else
                {
                    _riadok.Delete(_parametreVypisu.IndexStlpec);
                }
            }
            else if (vstup.Key == ConsoleKey.Escape)
            {
                ZmazVsetko();
                _historiaPrikazov.VycistiOperacieNaZopakovanie();

                r.ZavriRiadok = true;
            }
            else if((vstup.Modifiers & ConsoleModifiers.Control) == ConsoleModifiers.Control)
            {
                if(vstup.Key == ConsoleKey.Q)
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
                }
                NapisZnak(vstup.KeyChar);
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
                return new PrikazovyAutomatResult()
                {
                    ZavriRiadok = true,
                    Ukonci = true
                };
            }
            else if (r.ZavriVysledky)
            {
                _resultsMode = false;
            }

            return new PrikazovyAutomatResult();
        }

        private PrikazovyAutomatResult SpracujPrikaz()
        {
            var r = new PrikazovyAutomatResult();

            var prikaz = CitacPrikazov.NacitajPrikaz(_riadok, _tokeny);

            
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

        private void ZmazVsetko()
        {
            Zvyraznovac.VyberVsetko(_vyber, _parametreVypisu, _riadky);
            ZmazVyber();
        }

        private void ZmazVyber()
        {
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

            if(p.Resize || !_resultsMode)
            {
                PrekresliPrikazovyRiadok(p, sb);
            }

            if (_resultsMode)
            {
                sb.Append(VykreslovaciAutomat.NastavKurzor(1, p.OkrajVlavo));
                sb.Append(VykreslovaciAutomat.ZmazOdZaciatkuRiadkuPoKurzor());
                _vysledky.Prekresli(p, sb, riadkyEditora);

                return;
            }
        }

        public void PrekresliPrikazovyRiadok(ParametrePrekreslenia p, StringBuilder sb)
        {
            var pozadie = StylovaciAutomat.FarbaPozadia.Siva;
            if (_chyba)
            {
                pozadie = StylovaciAutomat.FarbaPozadia.CervenaLight;
                _chyba = false;
            }

            sb.Append(VykreslovaciAutomat.NastavKurzor(2, 1));
            sb.Append(VykreslovaciAutomat.ZmazOdKurzoraPoKoniecRiadku());

            sb.Append(VykreslovaciAutomat.NastavKurzor(1, 1));
            sb.Append(VykreslovaciAutomat.ZmazOdKurzoraPoKoniecRiadku());

            sb.Append(VykreslovaciAutomat.NastavPozadie(p.OkrajVlavo - 2));
            sb.Append(StylovaciAutomat.AnsiStyl(StylovaciAutomat.StylTextu.Biela));
            sb.Append("> ");
            sb.Append(StylovaciAutomat.AnsiReset());

            if (_riadok.Length() > 0)
            {
                _tokeny = _lexer.LexZoZatvorkami(_riadky);

                VyhladaneSlovo? zvyraznenyText = null;
                if (Zvyraznovac.MaVybranyText(_vyber))
                {
                    zvyraznenyText = Zvyraznovac.ZvyraznenyText(_vyber, 0, _riadok.Length());
                }

                sb.Append(StylovaciAutomat.SyntaxHighligt(_tokeny.Tokeny[0], _riadok, _parametreVypisu.OffsetStlpec, _parametreVypisu.Sirka, zvyraznenyText, pozadie, StylovaciAutomat.FarbaPozadia.Modra));
            }

            if (_riadok.Length() < _parametreVypisu.Sirka)
            {
                sb.Append(StylovaciAutomat.AnsiStyl(pozadie));
                sb.Append(VykreslovaciAutomat.NastavPozadie(_parametreVypisu.Sirka - _riadok.Length()));
                sb.Append(StylovaciAutomat.AnsiReset());
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

        private LexGramatika NacitajLexGramatiku()
        {
            try
            {
                var cesta = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Config/Lex/Commands.json");

                LexGramatika gramatika;

                using (var file = File.Open(cesta, FileMode.Open))
                {
                    using (var reader = new StreamReader(file))
                    {
                        var s = reader.ReadToEnd();

                        gramatika = (LexGramatika)JsonConvert.DeserializeObject(s, typeof(LexGramatika));
                    }
                }

                return gramatika;
            }
            catch (Exception ex)
            {
                ErrorLogger.GetInstance().Log(new Chyba()
                {
                    Ex = ex
                });

                return new LexGramatika();
            }
        }
    }
}
