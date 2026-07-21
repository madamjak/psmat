using PisaciStroj.Vyhladavanie;
using System.Collections.Generic;

namespace PSMat.Testy.Vyhladavanie
{
    public static class VyhladavacTestHelper
    {
        public static bool RovnakeVysledky(Dictionary<int, VyhladaneSlovo> t1, Dictionary<int, VyhladaneSlovo> t2)
        {
            try
            {
                if (t1.Count != t2.Count)
                {
                    return false;
                }

                foreach (var token1 in t1)
                {
                    var token2 = t2[token1.Key];

                    if (token1.Value.Pozicia != token2.Pozicia
                        || token1.Value.Dlzka != token2.Dlzka)
                    {
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
