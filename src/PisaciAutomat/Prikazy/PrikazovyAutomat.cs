using Newtonsoft.Json;
using PisaciAutomat.Obrazovka;
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

namespace PisaciAutomat.Prikazy
{
    public struct PrikazPrePrikazovyRiadok
    {
        public string VyhladavanyText { get; set; }

        public bool UlozSuborAko { get; set; }
        public string ExistujucaCesta { get; set; }
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

        public PrikazovyAutomat()
        {
            _lexer = new LexAutomat(NacitajLexGramatiku());
            _riadok = new GapBuffer();
            _riadky = new List<GapBuffer> { _riadok };
            _parametreVypisu = new ParametreVypisu();
            _navigovaciPrikaz = new NavigovaciPrikaz();
            _vyber = new ParametreVyberu();
            _historiaPrikazov = new HistoriaPrikazov();
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

            if (_prikazNaPotvrdenie != null)
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
                    ZmazVsetko();
                    _historiaPrikazov.VycistiOperacieNaZopakovanie();

                    r.ZavriRiadok = true;
                    r.Ukonci = true;
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

        public void Prekresli(ParametrePrekreslenia p, StringBuilder sb)
        {
            _parametreVypisu.OkrajVlavo = p.OkrajVlavo;

            var pozadie = StylovaciAutomat.FarbaPozadia.Siva;
            if (_chyba)
            {
                pozadie = StylovaciAutomat.FarbaPozadia.CervenaLight;
                _chyba = false;
            }

            sb.Append(VykreslovaciAutomat.NastavKurzor(1, 1));
            sb.Append(VykreslovaciAutomat.ZmazOdKurzoraPoKoniecRiadku());
            sb.Append(VykreslovaciAutomat.NastavPozadie(p.OkrajVlavo - 2));
            sb.Append(StylovaciAutomat.AnsiStyl(StylovaciAutomat.StylTextu.Biela));
            sb.Append("> ");
            sb.Append(StylovaciAutomat.AnsiReset());
            

            if (_riadok.Length() > 0)
            {
                _tokeny = _lexer.Lex(_riadky);

                VyhladaneSlovo? zvyraznenyText = null;
                if (Zvyraznovac.MaVybranyText(_vyber))
                {
                    zvyraznenyText = Zvyraznovac.ZvyraznenyText(_vyber, 0, _riadok.Length());
                }

                sb.Append(StylovaciAutomat.SyntaxHighligt(_tokeny.Tokeny[0], _riadok, _parametreVypisu.OffsetStlpec, _parametreVypisu.Sirka, zvyraznenyText, pozadie));
            }

            if (_riadok.Length() < _parametreVypisu.Sirka)
            {
                sb.Append(StylovaciAutomat.AnsiStyl(pozadie));
                sb.Append(VykreslovaciAutomat.NastavPozadie(_parametreVypisu.Sirka - _riadok.Length()));
                sb.Append(StylovaciAutomat.AnsiReset());
            }

            sb.Append(VykreslovaciAutomat.NastavKurzor(1, _parametreVypisu.StlpecKurzora + 1));
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
