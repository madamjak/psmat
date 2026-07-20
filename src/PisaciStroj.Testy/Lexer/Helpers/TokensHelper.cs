using PisaciStroj.Lexer;
using System;
using System.Collections.Generic;
using System.Text;

namespace PSMat.Testy.Lexer.Helpers
{
    public static class TokensHelper
    {
        public static bool SuRovnakeTokeny(Dictionary<int, Token> t1, Dictionary<int, Token> t2)
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

                    if (token1.Value.Typ != token2.Typ
                        || token1.Value.Pozicia != token2.Pozicia
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
