using PisaciStroj.Pamat;

namespace PisaciAutomat.Prikazy
{
    public static class CitacPrikazov
    {
        public static Prikaz NacitajPrikaz(GapBuffer prikazovyRiadok)
        {
            var p = new Prikaz();
            try
            {
                var parts = prikazovyRiadok.Read().Split(' ');

                if (parts.Length == 2 && (parts[0] == "fall"))
                {
                    p.Typ = TypPrikazu.Vyhladaj;
                    p.VyhladavanyText = parts[1];
                    p.ZavriRiadok = true;

                    return p;
                }
                if (parts.Length == 2 && (parts[0] == "fnext"))
                {
                    p.Typ = TypPrikazu.VyhladajDalsi;
                    p.VyhladavanyText = parts[1];

                    return p;
                }
                //if (parts.Length == 2 && (parts[0] == "fprev"))
                //{
                //    p.Typ = TypPrikazu.VyhladajPredosly;
                //    p.VyhladavanyText = parts[1];

                //    return p;
                //}
                if (parts.Length == 1 && parts[0] == "rest")
                {
                    p.Typ = TypPrikazu.VyhladajReset;

                    return p;
                }
                else if (parts.Length == 3 && (parts[0] == "rfirst"))
                {
                    p.Typ = TypPrikazu.VyhladajNahrad;
                    p.VyhladavanyText = parts[1];
                    p.NovyText = parts[2];

                    return p;
                }
                else if (parts.Length == 3 && parts[0] == "rall")
                {
                    p.Typ = TypPrikazu.VyhladajNahradVsetky;
                    p.VyhladavanyText = parts[1];
                    p.NovyText = parts[2];

                    return p;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
