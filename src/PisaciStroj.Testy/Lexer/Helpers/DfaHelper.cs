using Lexer.Algoritmy;

namespace PSMat.Testy.Lexer.Helpers
{
    public static class DfaHelper
    {
        internal static bool PorovnajAutomaty(DfaAutomaton ocakavanyAutomat, DfaAutomaton vygenerovanyAutomat)
        {
            var spravne = vygenerovanyAutomat.States.Length == ocakavanyAutomat.States.Length;
            if (spravne)
            {
                for (int i = 0; i < vygenerovanyAutomat.States.Length; i++)
                {
                    if (vygenerovanyAutomat.States[i].IsFinal != ocakavanyAutomat.States[i].IsFinal)
                    {
                        spravne = false;
                        break;
                    }

                    if (vygenerovanyAutomat.States[i].Transitions.Count != ocakavanyAutomat.States[i].Transitions.Count)
                    {
                        spravne = false;
                        break;
                    }

                    foreach (var transition in vygenerovanyAutomat.States[i].Transitions)
                    {
                        if (transition.Value != ocakavanyAutomat.States[i].Transitions[transition.Key])
                        {
                            spravne = false;
                            break;
                        }
                    }
                }
            }

            return spravne;
        }
    }
}
